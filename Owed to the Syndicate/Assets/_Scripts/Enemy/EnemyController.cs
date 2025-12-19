using System.Collections;
using UnityEngine;
using static UnityEngine.CullingGroup;

public class EnemyController : MonoBehaviour
{
    [SerializeField] private EnemyRuntime enemyRuntime;
    [SerializeField] private EnemyView enemyView;
    [SerializeField] private RectTransform rectTransform;

    [SerializeField] private float moveDuration = 1f;
    public bool IsBusy { get; private set; } = false;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void Init(EnemyModel enemyModel)
    {
        if (enemyRuntime != null)
        {
            enemyRuntime.Initialize(enemyModel);
        }

        if (enemyView != null)
        {
            UpdateView();
        }
    }

    private void Start()
    {
        if (enemyRuntime != null)
        {
            enemyRuntime.OnStatChange += OnStatChange;
            enemyRuntime.OnStateChanged += OnStateChanged;
            enemyRuntime.OnNextDecisionChanged += OnNextDecisionChanged;
            enemyRuntime.OnHurt += OnHurt;
            enemyRuntime.OnDeath += OnDeath;
        }

        if (enemyView != null)
        {
            enemyView.OnEnemyClicked += OnEnemyClicked;
        }
    }

    private void OnDestroy()
    {
        if (enemyRuntime != null)
        {
            enemyRuntime.OnStatChange -= OnStatChange;
            enemyRuntime.OnStateChanged -= OnStateChanged;
            enemyRuntime.OnNextDecisionChanged -= OnNextDecisionChanged;
            enemyRuntime.OnHurt -= OnHurt;
            enemyRuntime.OnDeath -= OnDeath;
        }

        if (enemyView != null)
        {
            enemyView.OnEnemyClicked -= OnEnemyClicked;
        }
    }

    public void TakeDamage(int value)
    {
        enemyRuntime.TakeDamage(value);
    }

    private void OnStatChange()
    {
        Debug.Log("[EnemyController] OnStatChange");

        UpdateView();
    }

    private void OnStateChanged()
    {
        Debug.Log("[EnemyController] OnStateChanged");
        if (enemyView != null && enemyRuntime != null)
        {
            enemyView.OnStateChanged(enemyRuntime.CurrentState);
        }

        UpdateView();
    }

    private void OnNextDecisionChanged(EnemyRuntime.TurnDecision newTurnDecision)
    {
        if (enemyView != null)
        {
            enemyView.SetStateIcon(newTurnDecision);
        }
    }

    private void OnHurt(int dmg)
    {
        if (enemyView != null)
        {
            enemyView.OnHurt();
        }
    }

    private void OnDeath()
    {
        Debug.Log("[EnemyController] OnDeath");

        enemyView.OnDeath();

        StartCoroutine(DelayedDestroy(2.0f));
    } 

    private IEnumerator DelayedDestroy(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }

    private void OnEnemyClicked()
    {
        TakeDamage(5);
        Debug.Log("[EnemyController] OnEnemyClicked");
    }

    private void UpdateView()
    {
        if (enemyRuntime == null || enemyView == null) return;
        enemyView.UpdateView(enemyRuntime);
    }

    public IEnumerator HandleTurn(Vector2 playerLocalPos)
    {
        Debug.Log("HandleTurn Appelé !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");

        if (enemyRuntime == null || enemyRuntime.CurrentState == EnemyState.Dead)
        {
        yield break;
        }

        IsBusy = true;

        enemyRuntime.StartExecution();

        var decision = enemyRuntime.NextDecision;

        switch (decision.action)
        {
            case EnemyState.Stunned:
                yield return new WaitForSeconds(0.5f);
                enemyRuntime.SetState(EnemyState.Idle);
                break;


            case EnemyState.Attacking:
                enemyRuntime.SetState(EnemyState.Attacking);
                // Timing a ajuster en fonction de la durée d'anim : 
                yield return new WaitForSeconds(0.5f);
                if (GameController.Instance != null)
                {
                    var playerCtrl = GameController.Instance.GetPlayerController();
                    if (playerCtrl != null)
                    {
                        int damage = enemyRuntime.Damage;
                        playerCtrl.TakeDamage(damage);
                        Debug.Log($"Enemy dealt {damage} damage to player!");
                    }
                }
                yield return new WaitForSeconds(0.5f);

                enemyRuntime.SetState(EnemyState.Idle);
                break;


            case EnemyState.Moving:
                enemyRuntime.SetState(EnemyState.Moving);
                // Ajuster la durée de l'animation ici également 
                yield return StartCoroutine(MoveToLocalPosition(decision.moveTargetLocal, moveDuration));
                enemyRuntime.SetState(EnemyState.Idle);
                break;


            case EnemyState.Idle:
                enemyRuntime.SetState(EnemyState.Idle);
                break;


            case EnemyState.Dead:
                break;
        }

        enemyRuntime.FinishExecutionAndPredictNext(GetBodyLocalPosition(), playerLocalPos);

        UpdateView();

        IsBusy = false;
    }

    private IEnumerator MoveToLocalPosition(Vector2 target, float duration)
    {
        if (rectTransform == null) yield break;
        Vector2 start = GetBodyLocalPosition();
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            rectTransform.anchoredPosition = Vector2.Lerp(start, target, Mathf.Clamp01(t / duration));
            yield return null;
        }
        rectTransform.anchoredPosition = target;

    }

    public void FacePlayer(Vector2 playerLocalPos)
    {
        var bodyLocal = GetBodyLocalPosition();
        bool faceRight = bodyLocal.x < playerLocalPos.x;
        enemyView.SetFacing(faceRight);
    }

    public Vector2 GetBodyLocalPosition()
    {
        if (rectTransform == null) return Vector2.zero;

        return rectTransform.anchoredPosition;
    }

    public EnemyRuntime GetEnemyRuntime()
    {
        return enemyRuntime;
    }
}
