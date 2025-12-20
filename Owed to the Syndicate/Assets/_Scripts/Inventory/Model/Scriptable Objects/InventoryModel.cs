using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "InventoryModel", menuName = "Scriptable Objects/InventoryModel")]
public class InventoryModel : ScriptableObject
{
    [SerializeField] private int _handSizeMax = 10;
    [SerializeField] private List<Card> _items;
    public int HandSizeMax => _handSizeMax;
    public List<Card> Items => _items;
}