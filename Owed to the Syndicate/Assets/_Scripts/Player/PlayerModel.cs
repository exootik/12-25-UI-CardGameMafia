using UnityEngine;

[CreateAssetMenu(fileName = "PlayerModel", menuName = "Scriptable Objects/PlayerModel")]
public class PlayerModel : ScriptableObject
{
    [Header("Health")]
    [SerializeField] private int maxHealth = 30;

    [Header("Mana")]
    [SerializeField] private int maxMana = 6;
    [SerializeField] private int manaPerTurn = 4;

    [Header("Gold")]
    [SerializeField] private int startingGold = 0;

    public int MaxHealth => maxHealth;
    public int MaxMana => maxMana;
    public int ManaPerTurn => manaPerTurn;
    public int StartingGold => startingGold;
}