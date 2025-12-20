using System.Linq;
using UnityEngine;

public class ShopController : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private ShopModel shopModel;

    [Header("References")]
    [SerializeField] private ShopView shopView;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private InventoryController inventoryController;

    private ShopRuntime shopRuntime;

    private void Awake()
    {
        if (shopModel == null)
        {
            Debug.LogError("[ShopController] ShopModel not assigned!");
            return;
        }

        shopRuntime = new ShopRuntime(shopModel);
    }

    private void Start()
    {
        if (shopView != null)
        {
            shopView.OnCloseClicked += CloseShop;
            shopView.OnCardSlotClicked += OnCardSlotClicked;
        }

        if (shopRuntime != null)
        {
            shopRuntime.OnOffersChanged += OnOffersChanged;
            shopRuntime.OnCardPurchased += OnCardPurchased;
        }
    }

    private void OnDestroy()
    {
        if (shopView != null)
        {
            shopView.OnCloseClicked -= CloseShop;
            shopView.OnCardSlotClicked -= OnCardSlotClicked;
        }

        if (shopRuntime != null)
        {
            shopRuntime.OnOffersChanged -= OnOffersChanged;
            shopRuntime.OnCardPurchased -= OnCardPurchased;
        }
    }

    /// <summary>
    /// Ouvre le shop
    /// </summary>
    public void OpenShop()
    {
        if (shopView == null)
        {
            Debug.LogError("[ShopController] ShopView is null!");
            return;
        }

        Debug.Log("[ShopController] Opening shop");

        shopView.Show();

        RefreshDisplay();
    }

    /// <summary>
    /// Ferme le shop
    /// </summary>
    public void CloseShop()
    {
        if (shopView == null) return;

        Debug.Log("[ShopController] Closing shop");

        shopView.Hide();
    }
    /// <summary>
    /// Gère le clic sur un slot de carte
    /// </summary>
    private void OnCardSlotClicked(ShopCardSlotView slot)
    {
        if (slot == null || slot.CurrentCard == null)
        {
            Debug.LogWarning("[ShopController] Invalid slot clicked");
            return;
        }

        Card card = slot.CurrentCard;
        int price = slot.CurrentPrice;

        Debug.Log($"[ShopController] Attempting to purchase {card.Name} for {price} gold");

        // Vérifier si le joueur a assez d'or
        if (playerController == null)
        {
            Debug.LogError("[ShopController] PlayerController is null!");
            return;
        }

        if (!playerController.HasEnoughGold(price))
        {
            Debug.Log($"[ShopController] Not enough gold! Need {price}, have {playerController.GetPlayerRuntime().CurrentGold}");
            return;
        }

        // Dépenser l'or
        if (!playerController.TrySpendGold(price))
        {
            Debug.LogWarning("[ShopController] Failed to spend gold");
            return;
        }

        // Ajouter la carte à l'inventaire
        if (inventoryController == null)
        {
            Debug.LogError("[ShopController] InventoryController is null!");
            playerController.AddGold(price);
            return;
        }

        AddCardToInventory(card);

        // Retirer la carte des offres (elle disparaît définitivement)
        shopRuntime.PurchaseCard(card);

        Debug.Log($"[ShopController] Successfully purchased {card.Name}");

        // Refresh l'affichage
        RefreshDisplay();
    }

    /// <summary>
    /// Ajoute une carte à l'inventaire du joueur
    /// </summary>
    private void AddCardToInventory(Card card)
    {
        if (inventoryController != null)
        {
            inventoryController.AddItem(card);
            Debug.Log($"[ShopController] Added {card.Name} to player's inventory");
        }
        else
        {
            Debug.LogError("[ShopController] InventoryController is null!");
        }
    }

    /// <summary>
    /// Refresh l'affichage complet du shop
    /// </summary>
    private void RefreshDisplay()
    {
        if (shopView == null || shopRuntime == null) return;

        int playerGold = GetPlayerGold();
        int deckCount = GetPlayerDeckCount();

        // Mettre à jour les infos du joueur
        shopView.UpdateGoldDisplay(playerGold);
        shopView.UpdateDeckCountDisplay(deckCount);

        shopView.UpdateAffordability(shopRuntime.CurrentOffers, shopModel, playerGold);
    }

    /// <summary>
    /// Appelé quand les offres changent (quand une carte est achetée)
    /// </summary>
    private void OnOffersChanged()
    {
        Debug.Log("[ShopController] Offers changed");

        if (shopView != null && shopView.IsVisible())
        {
            RefreshDisplay();
        }
    }

    /// <summary>
    /// Appelé quand une carte est achetée
    /// </summary>
    private void OnCardPurchased(Card card)
    {
        Debug.Log($"[ShopController] Card purchased: {card.Name}");
    }

    #region Helpers

    private int GetPlayerGold()
    {
        if (playerController != null && playerController.GetPlayerRuntime() != null)
        {
            return playerController.GetPlayerRuntime().CurrentGold;
        }
        return 0;
    }

    private int GetPlayerDeckCount()
    {
        if (inventoryController != null)
        {
            var runtimeInventory = inventoryController.GetRuntimeInventory();
            if (runtimeInventory != null)
            {
                return runtimeInventory.GetOccupiedSlots(null).Count();
            }
        }
        return 0;
    }

    #endregion
}