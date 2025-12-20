using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn zone")]
    public SpawnArea spawnArea;

    [Header("Placement")]
    public float minSeparation = 50f;
    public int maxSpawnAttempts = 20;

    private readonly List<Vector2> occupied = new List<Vector2>();

    public void ClearOccupied() => occupied.Clear();

    public EnemyController Spawn(EnemyModel model)
    {
        if (spawnArea == null) { Debug.LogError("[EnemySpawner] spawnArea non assigné."); return null; }

        Vector2 chosen;
        bool ok = TryFindValidLocalPosition(out chosen);

        if (!ok)
        {
            Debug.LogWarning("[EnemySpawner] position valide non trouvée, fallback centre du spawnArea.");
            chosen = spawnArea.rootRect != null ? spawnArea.rootRect.rect.center : Vector2.zero;
        }

        var go = Instantiate(model.prefab);
        Transform parent = spawnArea.rootRect != null ? spawnArea.rootRect.transform : this.transform;
        go.transform.SetParent(parent, false);

        var rt = go.GetComponent<RectTransform>();
        if (rt != null) rt.anchoredPosition = chosen;

        var controller = go.GetComponent<EnemyController>();
        if (controller != null) controller.Init(model);
        else Debug.LogWarning("[EnemySpawner] enemyPrefab n'a pas de EnemyController.");

        occupied.Add(chosen);
        return controller;
    }

    private bool TryFindValidLocalPosition(out Vector2 result)
    {
        result = Vector2.zero;
        for (int i = 0; i < maxSpawnAttempts; i++)
        {
            Vector2 cand = spawnArea.GetRandomLocalPoint();

            bool tooClose = false;
            foreach (var p in occupied)
            {
                if (Vector2.Distance(p, cand) < minSeparation) { tooClose = true; break; }
            }

            if (!tooClose)
            {
                result = cand;
                return true;
            }
        }
        return false;
    }
}
