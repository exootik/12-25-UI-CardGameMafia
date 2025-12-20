using UnityEngine;

/// <summary>
/// Controller: Manages a single card's interaction and delegates to services.
/// No business logic, only coordination.
/// </summary>
public class CardSlotController : MonoBehaviour
{
    [SerializeField] private CardSlotView view;

    private InventoryController inventoryController;
    private DragService dragService;
    private CardValidationService validationService;
    private InventoryEvents events;
    private Slot assignedSlot;
    private bool isSetup;

    public Slot AssignedSlot => assignedSlot;
    public bool Drag { get; private set; }

    private Vector3 lastPointerPos;

    /// <summary>
    /// Initializes the card controller with dependencies.
    /// </summary>
    public void Setup(
        InventoryController controller,
        Slot slot,
        DragService dragService,
        CardValidationService validationService,
        InventoryEvents events)
    {
        if (controller == null || slot == null) return;

        Cleanup();

        this.inventoryController = controller;
        this.dragService = dragService;
        this.validationService = validationService;
        this.events = events;
        this.assignedSlot = slot;

        if (view != null)
        {
            view.OnDragMove -= HandleDragMove;
            view.OnDragMove += HandleDragMove;
            view.UnDrag -= HandleDragEnd;
            view.UnDrag += HandleDragEnd;
        }

        isSetup = true;
        UpdateView();

        events.OnInventoryChanged -= UpdateView;
        events.OnInventoryChanged += UpdateView;
    }

    /// <summary>
    /// Updates the card's visual representation.
    /// </summary>
    public void UpdateView()
    {
        bool isActive = assignedSlot != null &&
                       assignedSlot.CardSlot != null &&
                       assignedSlot.Location == CardLocation.Hand;

        if (view != null)
        {
            view.UpdateUISlot(assignedSlot, isActive);
        }
    }

    /// <summary>
    /// Resets the controller for reuse in a pool.
    /// </summary>
    public void ResetForReuse()
    {
        Cleanup();

        if (view != null)
        {
            view.SetCard(null, false);
        }

        assignedSlot = null;
        inventoryController = null;
        dragService = null;
        validationService = null;
        events = null;
        isSetup = false;
        Drag = false;
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Handles card hover.
    /// </summary>
    public void OnHover(bool hovered)
    {
        if (!isSetup || Drag) return;
        events?.CardHoverChanged(this, hovered);
    }

    private void HandleDragMove(Vector3 pointerPos)
    {
        lastPointerPos = pointerPos;

        if (!Drag)
        {
            Card card = assignedSlot?.CardSlot;
            if (card == null) return;

            if (validationService != null && !validationService.CanDragCard(card))
            {
                inventoryController?.ShakeInPlace(this);
                return;
            }

            Drag = true;
            dragService?.StartDrag(this);
        }

        dragService?.UpdateDrag(this, pointerPos);
    }

    private void HandleDragEnd()
    {
        if (!Drag || dragService == null) return;

        bool shouldPlayCard = dragService.EndDrag(this, lastPointerPos);

        if (shouldPlayCard)
        {
            Card card = assignedSlot?.CardSlot;
            if (card == null)
            {
                Drag = false;
                return;
            }

            if (validationService != null && !validationService.TryConsumeResourcesForCard(card))
            {
                inventoryController?.ShakeAndReturn(this);
                Drag = false;
                return;
            }

            if (card.Effect != null)
            {
                events?.CardEffectRequested(lastPointerPos, card.Effect);
            }

            events?.CardDiscarded(assignedSlot);
        }

        Drag = false;
    }

    private void Cleanup()
    {
        if (isSetup)
        {
            if (events != null)
            {
                events.OnInventoryChanged -= UpdateView;
            }

            if (view != null)
            {
                view.OnDragMove -= HandleDragMove;
                view.UnDrag -= HandleDragEnd;
            }
        }
    }

    private void OnDestroy()
    {
        Cleanup();
        isSetup = false;
    }
}