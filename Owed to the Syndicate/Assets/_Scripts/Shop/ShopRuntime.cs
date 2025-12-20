using System;
using System.Collections.Generic;
using UnityEngine;

public class ShopRuntime
{
    public List<Card> CurrentOffers { get; private set; }

    public event Action OnOffersChanged;
    public event Action<Card> OnCardPurchased;

    private ShopModel shopModel;

    public ShopRuntime(ShopModel model)
    {
        shopModel = model;
        CurrentOffers = new List<Card>();

        InitializeFixedOffers();
    }

    /// <summary>
    /// Initialise les offres FIXES (appelé une seule fois)
    /// </summary>
    private void InitializeFixedOffers()
    {
        if (shopModel == null || shopModel.AvailableCards == null || shopModel.AvailableCards.Count == 0)
        {
            Debug.LogWarning("[ShopRuntime] No available cards!");
            return;
        }

        CurrentOffers.Clear();

        int offersToAdd = Mathf.Min(shopModel.ShopSlotCount, shopModel.AvailableCards.Count);

        for (int i = 0; i < offersToAdd; i++)
        {
            CurrentOffers.Add(shopModel.AvailableCards[i]);
        }

        Debug.Log($"[ShopRuntime] Initialized {CurrentOffers.Count} fixed offers: {string.Join(", ", CurrentOffers.ConvertAll(c => c.Name))}");
    }
    /// <summary>
    /// Achète une carte (la retire des offres)
    /// </summary>
    public bool PurchaseCard(Card card)
    {
        if (card == null)
        {
            Debug.LogWarning("[ShopRuntime] Cannot purchase null card");
            return false;
        }

        if (!CurrentOffers.Contains(card))
        {
            Debug.LogWarning($"[ShopRuntime] Card {card.Name} is not in current offers");
            return false;
        }

        CurrentOffers.Remove(card);

        Debug.Log($"[ShopRuntime] Purchased {card.Name}. Remaining offers: {CurrentOffers.Count}");

        OnCardPurchased?.Invoke(card);
        OnOffersChanged?.Invoke();

        return true;
    }

    /// <summary>
    /// Obtient une carte à un slot donné (null si déjà acheté)
    /// </summary>
    public Card GetCardAtSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= CurrentOffers.Count)
        {
            return null;
        }

        return CurrentOffers[slotIndex];
    }

    /// <summary>
    /// Obtient le nombre d'offres actuelles
    /// </summary>
    public int GetOfferCount()
    {
        return CurrentOffers.Count;
    }

    /// <summary>
    /// Vérifie si le shop a des offres
    /// </summary>
    public bool HasOffers()
    {
        return CurrentOffers.Count > 0;
    }
}