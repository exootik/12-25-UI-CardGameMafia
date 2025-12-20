using UnityEngine;

[CreateAssetMenu(fileName = "EnemyModel", menuName = "Scriptable Objects/EnemyModel")]
public class EnemyModel : ScriptableObject
{
    public int life;
    public int damage;
    public float rangeDamage;
    public float rangeMove;

    public GameObject prefab;
}
