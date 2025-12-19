using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Service: Manages a pool of card controllers for efficient reuse.
/// </summary>
public class CardPoolService
{
    private readonly List<CardSlotController> pool = new List<CardSlotController>();
    private readonly CardSlotController prefab;
    private readonly Transform poolParent;

    public CardPoolService(CardSlotController prefab, Transform poolParent)
    {
        this.prefab = prefab;
        this.poolParent = poolParent;
    }

    /// <summary>
    /// Acquires a card controller from the pool or creates a new one.
    /// </summary>
    public CardSlotController Acquire(Transform parent)
    {
        if (pool.Count > 0)
        {
            int lastIndex = pool.Count - 1;
            CardSlotController controller = pool[lastIndex];
            pool.RemoveAt(lastIndex);

            if (controller != null && parent != null)
            {
                controller.transform.SetParent(parent, worldPositionStays: false);
                controller.gameObject.SetActive(true);
            }

            return controller;
        }

        if (prefab != null && parent != null)
        {
            return Object.Instantiate(prefab, parent);
        }

        return null;
    }

    /// <summary>
    /// Releases a card controller back to the pool.
    /// </summary>
    public void Release(CardSlotController controller)
    {
        if (controller == null) return;

        controller.ResetForReuse();
        controller.gameObject.SetActive(false);

        if (!pool.Contains(controller))
        {
            pool.Add(controller);
        }
    }

    /// <summary>
    /// Clears the entire pool.
    /// </summary>
    public void Clear()
    {
        foreach (var controller in pool)
        {
            if (controller != null)
            {
                Object.Destroy(controller.gameObject);
            }
        }
        pool.Clear();
    }
}