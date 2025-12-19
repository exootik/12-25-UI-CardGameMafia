using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ShopModel", menuName = "Shop/Shop Model")]
public class ShopModel : ScriptableObject
{
    [Header("Card Pool")]
    [Tooltip("Toutes les cartes disponibles à l'achat dans le shop")]
    [SerializeField] private List<Card> availableCards = new List<Card>();

    [Header("Shop Configuration")]
    [Tooltip("Nombre de cartes affichées simultanément dans le shop")]
    [SerializeField] private int shopSlotCount = 3;

    public List<Card> AvailableCards => availableCards;
    public int ShopSlotCount => shopSlotCount;

    /// <summary>
    /// Obtient le prix d'une carte avec le multiplicateur appliqué
    /// </summary>
    public int GetCardPrice(Card card)
    {
        if (card == null) return 0;
        return card.Price;
    }

    public int GetCardPricePurchase(Card card)
    {
        if (card == null) return 0;
        return card.PricePurchase;
    }

    /// <summary>
    /// Valide la configuration
    /// </summary>
    private void OnValidate()
    {
        if (shopSlotCount < 1)
        {
            shopSlotCount = 1;
        }
    }
}