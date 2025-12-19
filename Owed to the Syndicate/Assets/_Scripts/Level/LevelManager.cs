using System;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private EnemySpawner spawner;
    private List<EnemyController> spawned = new List<EnemyController>();

    public event Action OnAllEnemiesDead;   

    public void StartLevel(LevelDefinition level)
    {
        if (spawner == null) 
        { 
            Debug.LogError("[LevelManager] spawner non assigné.");
            return; 
        }

        spawner.ClearOccupied();
        ClearSpawned();

        foreach (var entry in level.enemies)
        {
            for (int i = 0; i < Mathf.Max(1, entry.count); i++)
            {
                var ctrl = spawner.Spawn(entry.enemyModel);
                if (ctrl != null)
                {
                    spawned.Add(ctrl);
                    var runtime = ctrl.GetComponent<EnemyRuntime>();
                    if (runtime != null)
                    {
                        runtime.OnDeath += () => OnEnemyDeath(ctrl);
                    }

                }
            }
        }

        Debug.Log($"[LevelManager] Level started with {spawned.Count} enemies");
    }

    private void OnEnemyDeath(EnemyController ctrl)
    {
        spawned.Remove(ctrl);
        Debug.Log($"[LevelManager] Enemy died. Remaining: {spawned.Count}");
        if (spawned.Count == 0)
        {
            Debug.Log("[LevelManager] All enemy dead !!");
            OnAllEnemiesDead?.Invoke();
        }
    }

    private void ClearSpawned()
    {
        foreach (var e in spawned)
        {
            if (e != null)
            {
                DestroyImmediate(e.gameObject);
            }
        }
        spawned.Clear();
    }

    public void ClearLevel()
    {
        ClearSpawned();
        spawner?.ClearOccupied();
    }

    public int GetRemainingEnemiesCount()
    {
        return spawned.Count;
    }
}
