using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    [Header("Timing")]
    public float delayBetweenEachEnemy = 0.2f;

    [Header("Player Reference")]
    [SerializeField] private PlayerController playerController;

    private Coroutine enemyPhaseCoroutine;
    private bool isEnemyPhaseRunning = false;

    /// <summary>
    /// Prédit la prochaine action de tous les ennemis
    /// </summary>
    public void PredictAllEnemiesNextAction()
    {
        var enemies = FindObjectsByType<EnemyController>(FindObjectsSortMode.None);

        if (enemies.Length == 0)
        {
            Debug.LogWarning("[TurnManager] No enemies found for prediction");
            return;
        }

        Vector2 playerPos = GetPlayerPosition();

        foreach (var enemy in enemies)
        {
            if (enemy == null) continue;

            var runtime = enemy.GetEnemyRuntime();
            if (runtime == null || runtime.CurrentState == EnemyState.Dead) continue;

            Vector2 enemyPos = enemy.GetBodyLocalPosition();
            runtime.PredictNextAction(enemyPos, playerPos);
        }

        Debug.Log($"[TurnManager] Predicted actions for {enemies.Length} enemies");
    }

    /// <summary>
    /// Oriente tous les ennemis vers le joueur
    /// </summary>
    public void OrientAllEnemiesTowardsPlayer()
    {
        var enemies = FindObjectsByType<EnemyController>(FindObjectsSortMode.None);

        if (enemies.Length == 0)
        {
            Debug.LogWarning("[TurnManager] No enemies found for orientation");
            return;
        }

        Vector2 playerPos = GetPlayerPosition();

        foreach (var enemy in enemies)
        {
            if (enemy != null)
            {
                enemy.FacePlayer(playerPos);
            }
        }

        Debug.Log($"[TurnManager] Oriented {enemies.Length} enemies towards player");
    }

    public Coroutine StartEnemyPhase()
    {
        if (enemyPhaseCoroutine != null)
        {
            StopCoroutine(enemyPhaseCoroutine);
            enemyPhaseCoroutine = null;
        }

        enemyPhaseCoroutine = StartCoroutine(ExecuteEnemyPhase());
        return enemyPhaseCoroutine;
    }

    private IEnumerator ExecuteEnemyPhase()
    {
        if (isEnemyPhaseRunning)
        {
            Debug.LogWarning("[TurnManager] Enemy phase already running!");
            yield break;
        }

        isEnemyPhaseRunning = true;

        Debug.Log("[TurnManager] Starting enemy phase");

        var enemies = FindObjectsByType<EnemyController>(FindObjectsSortMode.None).ToList();

        if (enemies[0] == null)
        {
            Debug.LogWarning("[TurnManager] No enemies found for enemy phase");
            isEnemyPhaseRunning = false;
            yield break;
        }

        Vector2 playerPos = GetPlayerPosition();

        List<Coroutine> runningCoroutines = new List<Coroutine>();
        int processedCount = 0;
        foreach (var enemy in enemies)
        {
            if (enemy == null) continue;

            var runtime = enemy.GetEnemyRuntime();
            if (runtime == null || runtime.CurrentState == EnemyState.Dead) continue;

            Debug.Log($"[TurnManager] Processing enemy {enemy.name}");

            Coroutine enemyCoroutine = StartCoroutine(enemy.HandleTurn(playerPos));
            runningCoroutines.Add(enemyCoroutine);

            processedCount++;

            if (delayBetweenEachEnemy > 0f)
            {
                yield return new WaitForSeconds(delayBetweenEachEnemy);
            }
        }

        foreach (var coroutine in runningCoroutines)
        {
            yield return coroutine;
        }

        Debug.Log($"[TurnManager] Enemy phase completed - Processed {processedCount} enemies");

        isEnemyPhaseRunning = false;
        enemyPhaseCoroutine = null;
    }

    /// <summary>
    /// Réinitialise le TurnManager (appelé au retour au menu)
    /// </summary>
    public void ResetTurnManager()
    {
        Debug.Log("[TurnManager] Resetting TurnManager");

        if (enemyPhaseCoroutine != null)
        {
            StopCoroutine(enemyPhaseCoroutine);
            enemyPhaseCoroutine = null;
        }

        StopAllCoroutines();

        isEnemyPhaseRunning = false;

        Debug.Log("[TurnManager] TurnManager reset complete");
    }

    /// <summary>
    /// Obtient la position du joueur
    /// </summary>
    private Vector2 GetPlayerPosition()
    {
        if (playerController == null)
        {
            Debug.LogError("[TurnManager] PlayerController is null!");
            return Vector2.zero;
        }

        var rectTransform = playerController.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            return rectTransform.anchoredPosition;
        }

        Debug.LogWarning("[TurnManager] Cannot get player position!");
        return Vector2.zero;
    }
}