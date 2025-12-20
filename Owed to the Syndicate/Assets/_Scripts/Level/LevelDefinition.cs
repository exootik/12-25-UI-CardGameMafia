using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelDef", menuName = "Scriptable Objects/LevelDefinition")]
public class LevelDefinition : ScriptableObject
{
    public int levelNumber;
    [Serializable]
    public class EnemyEntry
    {
        public EnemyModel enemyModel;
        public int count = 1;
    }

    public List<EnemyEntry> enemies = new List<EnemyEntry>();
}
