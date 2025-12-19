using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ShopView : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject shopPanel;

    [Header("Card Slots")]
    [SerializeField] private ShopCardSlotView[] cardSlots;

    [Header("Player Info")]
    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private TextMeshProUGUI deckCountText;

    [Header("Buttons")]
    [SerializeField] private ButtonCustom closeButton;

    public event Action OnCloseClicked;
    public event Action<ShopCardSlotView> OnCardSlotClicked;

    private bool isInitialized = false;

    private void Awake()
    {
        if (closeButton != null)
        {
            closeButton.OnButtonClicked += OnCloseButtonClicked;
        }

        // S'abonner aux clics sur les slots
        if (cardSlots != null)
        {
            foreach (var slot in cardSlots)
            {
                if (slot != null)
                {
                    slot.OnBuyClicked += OnSlotBuyClicked;
                }
            }
        }
    }

    private void OnDestroy()
    {
        if (closeButton != null)
        {
            closeButton.OnButtonClicked -= OnCloseButtonClicked;
        }

        if (cardSlots != null)
        {
            foreach (var slot in cardSlots)
            {
                if (slot != null)
                {
                    slot.OnBuyClicked -= OnSlotBuyClicked;
                }
            }
        }
    }

    /// <summary>
    /// Affiche le panel du shop
    /// </summary>
    public void Show()
    {
        if (shopPanel != null)
        {
            shopPanel.SetActive(true);
        }

        Debug.Log("[ShopView] Shop opened");
    }

    /// <summary>
    /// Cache le panel du shop
    /// </summary>
    public void Hide()
    {
        if (shopPanel != null)
        {
            shopPanel.SetActive(false);
        }

        Debug.Log("[ShopView] Shop closed");
    }

    /// <summary>
    /// Initialise les cartes UNE SEULE FOIS
    /// </summary>
    public void InitializeCards(List<Card> offers, ShopModel model)
    {
        if (isInitialized)
        {
            Debug.LogWarning("[ShopView] Cards already initialized!");
            return;
        }

        if (cardSlots == null || cardSlots.Length == 0)
        {
            Debug.LogWarning("[ShopView] No card slots assigned!");
            return;
        }

        Debug.Log($"[ShopView] Initializing {offers.Count} cards");

        for (int i = 0; i < cardSlots.Length; i++)
        {
            if (cardSlots[i] == null) continue;

            if (i < offers.Count && offers[i] != null)
            {
                Card card = offers[i];
                int pricePurchase = model.GetCardPricePurchase(card);

                cardSlots[i].SetCard(card, pricePurchase);
            }
        }

        isInitialized = true;
        Debug.Log("[ShopView] Cards initialized");
    }

    /// <summary>
    /// Met à jour SEULEMENT l'affordability selon le gold
    /// </summary>
    public void UpdateAffordability(List<Card> offers, ShopModel model, int playerGold)
    {
        if (cardSlots == null) return;

        // Si pas encore initialisé, initialiser d'abord
        if (!isInitialized)
        {
            InitializeCards(offers, model);
        }

        Debug.Log($"[ShopView] Updating affordability with {playerGold} gold");

        for (int i = 0; i < cardSlots.Length; i++)
        {
            if (cardSlots[i] == null) continue;

            Card card = cardSlots[i].CurrentCard;
            if (card != null)
            {
                int pricePurchase = model.GetCardPricePurchase(card);
                bool canAfford = playerGold >= pricePurchase;

                cardSlots[i].UpdateAffordability(canAfford);
            }
        }
    }

    /// <summary>
    /// Cache un slot après achat
    /// </summary>
    public void HideSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= cardSlots.Length) return;

        if (cardSlots[slotIndex] != null)
        {
            cardSlots[slotIndex].HideCard();
        }
    }

    /// <summary>
    /// Met à jour l'affichage de l'or
    /// </summary>
    public void UpdateGoldDisplay(int gold)
    {
        if (goldText != null)
        {
            goldText.text = gold.ToString();
        }
    }

    /// <summary>
    /// Met à jour l'affichage du nombre de cartes dans le deck
    /// </summary>
    public void UpdateDeckCountDisplay(int cardCount)
    {
        if (deckCountText != null)
        {
            deckCountText.text = cardCount.ToString();
        }
    }

    /// <summary>
    /// Vérifie si le shop est visible
    /// </summary>
    public bool IsVisible()
    {
        return shopPanel != null && shopPanel.activeSelf;
    }

    private void OnCloseButtonClicked()
    {
        Debug.Log("[ShopView] Close button clicked");
        OnCloseClicked?.Invoke();
    }

    private void OnSlotBuyClicked(ShopCardSlotView slot)
    {
        Debug.Log($"[ShopView] Slot clicked: {slot.CurrentCard?.Name}");
        OnCardSlotClicked?.Invoke(slot);
    }
}