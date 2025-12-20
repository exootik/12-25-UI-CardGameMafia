using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Controller: Main coordinator using MVC pattern with events and services.
/// Delegates responsibilities to specialized services.
/// </summary>
public class InventoryController : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private InventoryModel inventoryConfig;
    [SerializeField] private CardSlotController cardSlotPrefab;

    [Header("Views")]
    [SerializeField] private InventoryView handView;
    [SerializeField] private DrawView drawPileView;
    [SerializeField] private DrawView discardPileView;
    [SerializeField] private CardAnimationManager animationManager;

    [Header("Drag Configuration")]
    [SerializeField] private RectTransform dragZone;
    [SerializeField] private RectTransform dragTargetZone;
    [SerializeField] private GameObject dragCircle;

    [Header("Effect Configuration")]
    [SerializeField] private CardEffectController effectController;
    [SerializeField] private RectTransform enemyContainer;

    [SerializeField] private AnimationRunner animationRunner;

    private RuntimeInventory inventory;
    private InventoryEvents events;
    private DragService dragService;
    private CardValidationService validationService;
    private CardPoolService cardPool;
    private HandLayoutService layoutService;

    private readonly List<CardSlotController> activeCards = new List<CardSlotController>();
    private Transform poolParent;
    private RectTransform handContainerRect;
    private RectTransform drawPileRect;
    private RectTransform discardPileRect;

    private bool isAnimating;

    private void Start()
    {
        InitializeServices();
        InitializeModel();
        InitializeViews();
        SubscribeToEvents();
        LoadInitialInventory();
    }

    private void InitializeServices()
    {
        events = new InventoryEvents();
        validationService = new CardValidationService();

        if (handView != null)
        {
            handContainerRect = handView.GetHandContainer() as RectTransform;
            layoutService = new HandLayoutService(120f, 5f, -50f);
        }

        poolParent = handContainerRect?.parent ?? new GameObject("CardPool").transform;
        if (poolParent.parent == null)
        {
            poolParent.SetParent(transform, worldPositionStays: false);
        }

        cardPool = new CardPoolService(cardSlotPrefab, poolParent);
        dragService = new DragService(dragZone, dragTargetZone, events);
    }

    private void InitializeModel()
    {
        if (inventoryConfig == null)
        {
            Debug.LogError("InventoryController: inventoryConfig is null");
            return;
        }

        inventory = new RuntimeInventory(inventoryConfig.HandSizeMax);
        inventory.OnInventoryChanged += () => events.InventoryChanged();
    }

    private void InitializeViews()
    {
        if (drawPileView != null)
            drawPileRect = drawPileView.GetComponent<RectTransform>();
        if (discardPileView != null)
            discardPileRect = discardPileView.GetComponent<RectTransform>();

        if (animationManager == null)
        {
            animationManager = GetComponent<CardAnimationManager>();
        }

        if (effectController == null)
        {
            effectController = GetComponent<CardEffectController>();
        }
    }

    private void SubscribeToEvents()
    {
        events.OnInventoryChanged += RefreshCards;
        events.OnInventoryChanged += UpdateHandLayout;
        events.OnInventoryChanged += UpdatePileCounts;

        events.OnCardHoverChanged += HandleCardHover;
        events.OnCardDragMoved += HandleCardDragMove;
        events.OnCardDiscarded += HandleCardDiscard;
        events.OnCardEffectRequested += HandleEffectRequested;

        // MODIFIÉ : événement avec 2 positions
        events.OnEffectPreviewShow += HandleEffectPreviewShow;
        events.OnEffectPreviewHide += HandleEffectPreviewHide;
    }
    public void AddItem(Card card)
    {
        if (inventory == null || card == null) return;
        inventory.AddItem(card, CardLocation.Draw);
        events.InventoryChanged();
    }

    private void LoadInitialInventory()
    {
        foreach (var card in inventoryConfig.Items)
        {
            inventory.AddItem(card, CardLocation.Draw);
        }

        events.InventoryChanged();
    }

    /// <summary>
    /// Draws cards from draw pile to hand.
    /// </summary>
    public void Draw(int count)
    {
        if (count <= 0 || inventory == null) return;

        StartCoroutine(DrawCoroutine(count));
    }

    private IEnumerator DrawCoroutine(int count)
    {
        for (int i = 0; i < count; i++)
        {
            var drawSlot = inventory.GetOccupiedSlots(CardLocation.Draw).FirstOrDefault();

            if (drawSlot == null)
            {
                inventory.ShuffleDiscardToDraw();
                bool shuffleDone = false;
                animationRunner.PlayShuffleAnimation(discardPileRect.position, drawPileRect.position, this.transform, () => shuffleDone = true);
                yield return new WaitUntil(() => shuffleDone);

                events.PileCountsChanged();
                drawSlot = inventory.GetOccupiedSlots(CardLocation.Draw).FirstOrDefault();
                if (drawSlot == null) yield break;
            }

            isAnimating = true;

            inventory.Draw(1);
            var allHandSlots = inventory.GetOccupiedSlots(CardLocation.Hand).ToList();
            int finalHandCount = allHandSlots.Count;

            RefreshCards();

            var controller = activeCards.FirstOrDefault(c => c?.AssignedSlot == drawSlot);
            if (controller == null)
            {
                isAnimating = false;
                continue;
            }

            var cardRect = controller.GetComponent<RectTransform>();
            if (cardRect == null || drawPileRect == null || handContainerRect == null)
            {
                isAnimating = false;
                continue;
            }

            cardRect.position = drawPileRect.position;

            int targetIndex = allHandSlots.IndexOf(drawSlot);
            if (targetIndex < 0) targetIndex = finalHandCount - 1;

            bool animComplete = false;
            animationManager.AnimateCardFromDeckToHand(
                cardRect,
                drawPileRect,
                targetIndex,
                finalHandCount,
                onComplete: () => animComplete = true
            );

            yield return new WaitUntil(() => animComplete);

            isAnimating = false;
            events.InventoryChanged();

            yield return new WaitForSeconds(0.1f);
        }
    }

    private void RefreshCards()
    {
        if (cardSlotPrefab == null || poolParent == null || inventory == null) return;

        var currentSlots = new HashSet<Slot>(inventory.Slots);
        var slotToController = new Dictionary<Slot, CardSlotController>();

        foreach (var controller in activeCards.ToList())
        {
            if (controller == null) continue;

            Slot slot = controller.AssignedSlot;
            if (slot != null && currentSlots.Contains(slot))
            {
                if (!slotToController.ContainsKey(slot))
                {
                    slotToController[slot] = controller;
                }
                else
                {
                    cardPool.Release(controller);
                    activeCards.Remove(controller);
                }
            }
            else
            {
                cardPool.Release(controller);
                activeCards.Remove(controller);
            }
        }

        foreach (var slot in inventory.Slots)
        {
            if (slot == null) continue;

            if (slotToController.ContainsKey(slot))
            {
                var existing = slotToController[slot];
                existing.UpdateView();
                continue;
            }

            var newController = cardPool.Acquire(poolParent);
            if (newController != null)
            {
                newController.Setup(this, slot, dragService, validationService, events);
                activeCards.Add(newController);
            }
        }
    }

    private void UpdateHandLayout()
    {
        if (handView == null || inventory == null || isAnimating || dragService.IsDragging)
            return;

        var handContainer = handContainerRect;
        var activeCardRects = new List<RectTransform>();

        foreach (var slot in inventory.Slots)
        {
            if (slot?.Location != CardLocation.Hand) continue;

            var controller = activeCards.FirstOrDefault(c => c?.AssignedSlot == slot);
            if (controller == null) continue;

            if (handContainer != null && controller.transform.parent != handContainer)
            {
                controller.transform.SetParent(handContainer, worldPositionStays: false);
            }

            var rect = controller.GetComponent<RectTransform>();
            if (rect != null)
            {
                activeCardRects.Add(rect);
            }

            controller.gameObject.SetActive(true);
        }

        foreach (var controller in activeCards)
        {
            if (controller == null) continue;

            bool isInHand = controller.AssignedSlot?.Location == CardLocation.Hand;
            if (!isInHand)
            {
                controller.gameObject.SetActive(false);
            }
        }

        handView.LayoutHand(activeCardRects);
    }

    private void HandleCardHover(CardSlotController card, bool hovered)
    {
        if (handView == null || dragService.IsDragging) return;

        var activeCardRects = GetHandCardRects();
        int hoveredIndex = hovered ? activeCardRects.IndexOf(card.GetComponent<RectTransform>()) : -1;

        handView.UpdateHandVisuals(activeCardRects, hoveredIndex);
    }


    private void HandleCardDiscard(Slot slot)
    {
        if (slot == null || inventory == null) return;

        var controller = activeCards.FirstOrDefault(c => c?.AssignedSlot == slot);
        var cardRect = controller?.GetComponent<RectTransform>();

        if (cardRect != null && discardPileRect != null)
        {
            isAnimating = true;

            animationManager.AnimateCardFromHandToDiscard(
                cardRect,
                discardPileRect,
                onComplete: () =>
                {
                    inventory.Discard(slot);
                    isAnimating = false;
                    events.InventoryChanged();
                }
            );
        }
        else
        {
            inventory.Discard(slot);
            events.InventoryChanged();
        }
    }
    public void ClearHand()
    {
        var handSlots = inventory.GetOccupiedSlots(CardLocation.Hand).ToList();
        foreach (var slot in handSlots)
        {
            inventory.Discard(slot);
        }
        events.InventoryChanged();
    }

    private void HandleEffectRequested(Vector3 position, CardEffect effect)
    {
        if (effectController != null)
        {
            effectController.ApplyEffect(position, effect);
        }
    }

    private void HandleEffectPreviewShow(Vector3 cardPosition, Vector3 circlePosition, CardEffect effect)
    {
        if (effectController != null)
        {
            effectController.ShowEffect(cardPosition, circlePosition, effect);
        }
    }
    private void HandleCardDragMove(CardSlotController card, Vector3 pointerPosition)
    {
        if (dragCircle != null)
        {
            dragCircle.SetActive(dragService.IsOutsideDragZone(pointerPosition));
            dragCircle.transform.position = pointerPosition;
        }

        if (dragService.IsOutsideDragZone(pointerPosition) && card != null)
        {
            Card cardData = card.AssignedSlot?.CardSlot;
            if (cardData != null && cardData.Effect != null)
            {
                RectTransform cardRect = card.GetComponent<RectTransform>();
                if (cardRect != null && effectController != null)
                {
                    effectController.UpdateEffectPosition(cardRect.position, pointerPosition);
                }
            }
        }
    }
    private void HandleEffectPreviewHide()
    {
        if (effectController != null)
        {
            effectController.HideEffect();
        }
    }

    private void UpdatePileCounts()
    {
        drawPileView?.UpdateText(inventory.GetOccupiedSlots(CardLocation.Draw).Count());
        discardPileView?.UpdateText(inventory.GetOccupiedSlots(CardLocation.Discard).Count());
    }

    private List<RectTransform> GetHandCardRects()
    {
        return inventory.GetOccupiedSlots(CardLocation.Hand)
            .Select(s => activeCards.FirstOrDefault(c => c?.AssignedSlot == s))
            .Where(c => c != null)
            .Select(c => c.GetComponent<RectTransform>())
            .Where(r => r != null)
            .ToList();
    }

    public void ShakeInPlace(CardSlotController ctrl)
    {
        if (ctrl != null)
        {
            StartCoroutine(ShakeCoroutine(ctrl));
        }
    }

    public void ShakeAndReturn(CardSlotController ctrl)
    {
        if (ctrl != null)
        {
            StartCoroutine(ShakeAndReturnCoroutine(ctrl));
        }
    }

    private IEnumerator ShakeCoroutine(CardSlotController ctrl)
    {
        var rect = ctrl?.GetComponent<RectTransform>();
        if (rect == null) yield break;

        Vector3 originalLocalPos = rect.localPosition;
        float duration = 0.2f;
        float elapsed = 0f;
        float magnitude = 10f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float damper = 1f - t;
            float offset = Mathf.Sin(elapsed * 40f) * magnitude * damper;
            rect.localPosition = originalLocalPos + new Vector3(offset, 0f, 0f);
            yield return null;
        }

        rect.localPosition = originalLocalPos;
    }

    private IEnumerator ShakeAndReturnCoroutine(CardSlotController ctrl)
    {
        yield return StartCoroutine(ShakeCoroutine(ctrl));

        var rect = ctrl?.GetComponent<RectTransform>();
        if (rect == null || handContainerRect == null) yield break;

        var handSlots = inventory?.GetOccupiedSlots(CardLocation.Hand).ToList();
        int finalCount = handSlots?.Count ?? 1;
        int index = handSlots?.FindIndex(s => s == ctrl.AssignedSlot) ?? -1;
        if (index < 0) index = finalCount - 1;

        var targetData = GetFanTransformForIndex(index, finalCount);

        if (animationManager != null)
        {
            animationManager.AnimateCardToContainer(
                rect,
                handContainerRect,
                targetData.position,
                targetData.rotation,
                onComplete: () => events.InventoryChanged()
            );
        }
    }

    public RuntimeInventory GetRuntimeInventory() => inventory;
    public (Vector3 position, Quaternion rotation) GetFanTransformForIndex(int index, int totalCards)
    {
        return layoutService?.GetFanTransform(index, totalCards) ?? (Vector3.zero, Quaternion.identity);
    }

    private void OnDestroy()
    {
        if (events != null)
        {
            events.OnInventoryChanged -= RefreshCards;
            events.OnInventoryChanged -= UpdateHandLayout;
            events.OnInventoryChanged -= UpdatePileCounts;
            events.OnCardHoverChanged -= HandleCardHover;
            events.OnCardDragMoved -= HandleCardDragMove;
            events.OnCardDiscarded -= HandleCardDiscard;
            events.OnCardEffectRequested -= HandleEffectRequested;
            events.OnEffectPreviewShow -= HandleEffectPreviewShow;  // MODIFIÉ
            events.OnEffectPreviewHide -= HandleEffectPreviewHide;
        }

        cardPool?.Clear();
    }

}