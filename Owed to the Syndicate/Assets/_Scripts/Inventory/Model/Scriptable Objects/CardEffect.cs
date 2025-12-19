using UnityEngine;

public enum effect
{
    Heal,
    Damage
}

[CreateAssetMenu(fileName = "CardEffect", menuName = "Scriptable Objects/CardEffect")]
public class CardEffect : ScriptableObject
{
    [SerializeField] private float range;
    [SerializeField] private float damage;
    [SerializeField] private effect effect;
    [SerializeField] private GameObject animation;

    public float Range => range;
    public float Damage => damage;
    public effect Effect => effect;
    public GameObject Animation => animation;
}
