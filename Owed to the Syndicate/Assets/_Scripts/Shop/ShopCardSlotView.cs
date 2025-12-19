using System;
using TMPro;
using UnityEngine;

public class ShopCardSlotView : MonoBehaviour
{
    [Header("Card Display")]
    [SerializeField] private CardSlotView cardSlotView;
    [SerializeField] private TextMeshProUGUI priceOverrideText;

    [Header("Purchase")]
    [SerializeField] private ButtonCustom buyButton;
    [SerializeField] private GameObject affordableVisual;
    [SerializeField] private GameObject tooExpensiveVisual;

    public event Action<ShopCardSlotView> OnBuyClicked;

    public Card CurrentCard { get; private set; }
    public int CurrentPrice { get; private set; }

    private void Awake()
    {
        if (buyButton != null)
        {
            buyButton.OnButtonClicked += OnButtonClicked;
        }
    }

    private void OnDestroy()
    {
        if (buyButton != null)
        {
            buyButton.OnButtonClicked -= OnButtonClicked;
        }
    }

    /// <summary>
    /// Affiche une carte UNE SEULE FOIS au démarrage
    /// </summary>
    public void SetCard(Card card, int pricePurchase)
    {
        CurrentCard = card;
        CurrentPrice = pricePurchase;

        if (cardSlotView != null && card != null)
        {
            cardSlotView.gameObject.SetActive(true);
            cardSlotView.SetCard(card, true);

            if (priceOverrideText != null)
            {
                priceOverrideText.text = pricePurchase.ToString();
            }
        }

        Debug.Log($"[ShopCardSlotView] Card set: {card?.Name} - Price: {pricePurchase}");
    }

    /// <summary>
    /// Change juste l'état du bouton selon le gold
    /// </summary>
    public void UpdateAffordability(bool canAfford)
    {
        if (buyButton != null)
        {
            buyButton.interactable = canAfford;
        }

        SetAffordableState(canAfford);

        Debug.Log($"[ShopCardSlotView] {CurrentCard?.Name} - Can afford: {canAfford}");
    }

    /// <summary>
    /// Cache la carte après achat (définitif)
    /// </summary>
    public void HideCard()
    {
        CurrentCard = null;

        if (cardSlotView != null)
        {
            cardSlotView.gameObject.SetActive(false);
        }

        if (buyButton != null)
        {
            buyButton.interactable = false;
        }

        if (affordableVisual != null)
        {
            affordableVisual.SetActive(false);
        }

        if (tooExpensiveVisual != null)
        {
            tooExpensiveVisual.SetActive(false);
        }

        Debug.Log("[ShopCardSlotView] Card hidden (purchased)");
    }

    private void SetAffordableState(bool canAfford)
    {
        if (affordableVisual != null)
        {
            affordableVisual.SetActive(canAfford);
        }

        if (tooExpensiveVisual != null)
        {
            tooExpensiveVisual.SetActive(!canAfford);
        }
    }

    private void OnButtonClicked()
    {
        if (CurrentCard != null)
        {
            Debug.Log($"[ShopCardSlotView] Buy button clicked for {CurrentCard.Name}");
            OnBuyClicked?.Invoke(this);
        }
    }
}