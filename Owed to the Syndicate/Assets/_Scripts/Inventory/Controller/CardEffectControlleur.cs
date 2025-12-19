using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Contrôleur qui gère la logique de détection des ennemis UI et l'application des effets
/// Version Canvas/UI (RectTransform au lieu de Collider2D)
/// </summary>
public class CardEffectController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CardEffectView effectView;
    [SerializeField] private Canvas gameCanvas;

    [Header("Detection Settings")]
    [SerializeField] private string enemyTag = "Enemy";
    [SerializeField] private Transform enemiesContainer;

    private bool isShowingEffect = false;
    private CardEffect currentEffect;
    private Vector3 currentCardPosition;
    private List<RectTransform> cachedEnemies = new List<RectTransform>();

    private void Awake()
    {
        if (effectView == null)
        {
            effectView = GetComponent<CardEffectView>();
            if (effectView == null)
            {
                effectView = gameObject.AddComponent<CardEffectView>();
                Debug.LogWarning("CardEffectView not assigned, added automatically");
            }
        }

        if (gameCanvas == null)
        {
            gameCanvas = GetComponentInParent<Canvas>();
            if (gameCanvas == null)
            {
                gameCanvas = FindObjectOfType<Canvas>();
            }
        }

        CacheEnemies();
    }

    /// <summary>
    /// Met en cache tous les ennemis dans le conteneur
    /// </summary>
    private void CacheEnemies()
    {
        cachedEnemies.Clear();

        if (enemiesContainer == null) return;

        RectTransform[] allRects = enemiesContainer.GetComponentsInChildren<RectTransform>();
        foreach (var rect in allRects)
        {
            if (rect.CompareTag(enemyTag))
            {
                cachedEnemies.Add(rect);
            }
        }

        Debug.Log($"Cached {cachedEnemies.Count} enemies");
    }

    /// <summary>
    /// Rafraîchir le cache des ennemis (appeler quand des ennemis spawn/meurent)
    /// </summary>
    public void RefreshEnemyCache()
    {
        CacheEnemies();
    }

    /// <summary>
    /// Affiche la zone d'effet pendant le drag avec la position de la carte
    /// </summary>
    public void ShowEffect(Vector3 cardPosition, Vector3 circlePosition, CardEffect effect)
    {
        if (effect == null || effectView == null) return;

        currentEffect = effect;
        currentCardPosition = cardPosition;
        isShowingEffect = true;

        effectView.ShowEffectCircle(cardPosition, circlePosition, effect.Range, effect.Effect);
    }

    /// <summary>
    /// Met à jour les positions de la carte et de la zone d'effet
    /// </summary>
    public void UpdateEffectPosition(Vector3 cardPosition, Vector3 circlePosition)
    {
        if (isShowingEffect && effectView != null)
        {
            currentCardPosition = cardPosition;
            effectView.UpdateEffectCirclePosition(cardPosition, circlePosition);
        }
    }

    /// <summary>
    /// Cache la zone d'effet et la flèche
    /// </summary>
    public void HideEffect()
    {
        if (effectView != null)
        {
            effectView.HideEffectCircle();
        }
        isShowingEffect = false;
    }

    /// <summary>
    /// Applique l'effet à tous les ennemis dans la zone au moment du relâchement
    /// </summary>
    public void ApplyEffect(Vector3 screenPosition, CardEffect effect)
    {
        if (effect == null) return;

        PlayAnimation(screenPosition);
        RefreshEnemyCache();

        List<GameObject> affectedEnemies = new List<GameObject>();

        // Parcourir tous les ennemis en cache
        foreach (var enemyRect in cachedEnemies)
        {
            if (enemyRect == null) continue;

            float distance = Vector2.Distance(enemyRect.position, screenPosition);

            if (distance <= effect.Range + 20)
            {
                affectedEnemies.Add(enemyRect.gameObject);
            }
        }

        // Appliquer l'effet à chaque ennemi détecté
        foreach (var enemy in affectedEnemies)
        {
            ApplyEffectToEnemy(enemy, effect);
        }

        Debug.Log($"Applied {effect.Effect} effect to {affectedEnemies.Count} enemies at position {screenPosition}");
    }

    /// <summary>
    /// Applique l'effet à un ennemi spécifique
    /// </summary>
    private void ApplyEffectToEnemy(GameObject enemy, CardEffect cardEffect)
    {
        switch (cardEffect.Effect)
        {
            case effect.Damage:
                Debug.Log("damage " + cardEffect.Damage);
                var enemyController = enemy.GetComponent<EnemyController>();
                if (enemyController != null)
                {
                    Debug.Log("enemy : " + enemyController.name);
                    enemyController.TakeDamage((int)cardEffect.Damage);
                }
                break;

            case effect.Heal:
                Debug.Log($"Healed {cardEffect.Damage} to {enemy.name}");
                break;
        }
    }

    /// <summary>
    /// Obtient tous les ennemis dans une zone donnée (utile pour debug)
    /// </summary>
    public List<GameObject> GetEnemiesInRange(Vector3 screenPosition, float range)
    {
        List<GameObject> enemiesInRange = new List<GameObject>();

        foreach (var enemyRect in cachedEnemies)
        {
            if (enemyRect == null) continue;

            float distance = Vector2.Distance(enemyRect.position, screenPosition);
            if (distance <= range)
            {
                enemiesInRange.Add(enemyRect.gameObject);
            }
        }

        return enemiesInRange;
    }

    /// <summary>
    /// Visualise la zone d'effet dans l'éditeur (pour debug en Scene view)
    /// </summary>
    private void OnDrawGizmos()
    {
        if (isShowingEffect && currentEffect != null && gameCanvas != null)
        {
            Gizmos.color = currentEffect.Effect == effect.Heal
                ? new Color(0f, 1f, 0f, 0.3f)
                : new Color(1f, 0f, 0f, 0.3f);

            Gizmos.DrawWireSphere(transform.position, currentEffect.Range / 100f);
        }
    }

    private void PlayAnimation(Vector2 screenPosition)
    {
        if (currentEffect == null || currentEffect.Animation == null) return;

        GameObject animObj = Instantiate(currentEffect.Animation, gameCanvas.transform);
        animObj.transform.position = screenPosition;

        StartCoroutine(PlayChildrenWithDelay(animObj, 0.15f));

        Destroy(animObj, 0.5f);
    }

    private IEnumerator PlayChildrenWithDelay(GameObject parent, float delay)
    {
        Animator[] childAnimators = parent.GetComponentsInChildren<Animator>();

        foreach (Animator anim in childAnimators)
        {
            if (anim.gameObject == parent) continue;

            anim.SetTrigger("fire");

            yield return new WaitForSeconds(delay);
        }
    }
}