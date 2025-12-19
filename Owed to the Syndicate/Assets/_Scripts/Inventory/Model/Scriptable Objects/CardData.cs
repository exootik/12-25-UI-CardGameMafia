using UnityEngine;

[CreateAssetMenu(fileName = "ItemData", menuName = "Scriptable Objects/ItemData")]
public class Card : ScriptableObject
{
    [SerializeField] private string _name;
    [SerializeField] private string _description;
    [SerializeField] private Sprite _icon;
    [SerializeField] private int _price;
    [SerializeField] private int _pricePurchase;
    [SerializeField] private Sprite _quality;
    [SerializeField] private CardEffect _effect;


    public string Name => _name;
    public string Description => _description;
    public Sprite Icon => _icon;

    public int Price
    {
        get => _price;
        set => _price = value;
    }

    public int PricePurchase
    {
        get => _pricePurchase;
        set => _pricePurchase = value;
    }
    public Sprite Quality
    {
        get => _quality;
    }

    public CardEffect Effect => _effect;
}

