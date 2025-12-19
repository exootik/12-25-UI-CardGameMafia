using System;
using UnityEngine;

public enum EnemyState
{
    Idle,
    Moving,
    Attacking,
    Stunned,
    Dead
}

public class EnemyRuntime : MonoBehaviour
{
    public event Action OnStatChange;
    public event Action OnStateChanged;
    public event Action<TurnDecision> OnNextDecisionChanged;
    public event Action<int> OnHurt;
    public event Action OnDeath;

    [Header("Stats")]
    public int Life { get; private set; }
    public int MaxLife { get; private set; }
    public int Damage { get; private set; }
    public float RangeDamage { get; private set; }
    public float RangeMove { get; private set; }
    public EnemyState CurrentState { get; private set; }
    public TurnDecision NextDecision { get; private set; }
    public EnemyState DisplayedState => (CurrentState == EnemyState.Idle) ? NextDecision.action : CurrentState;

    public struct TurnDecision
    {
        public EnemyState action; // Moving / Attacking / Idle / Stunned / Dead
        public Vector2 moveTargetLocal;
    }

    public void Initialize(EnemyModel model)
    {
        Life = model.life;
        MaxLife = model.life;
        Damage = model.damage;
        RangeDamage = model.rangeDamage;
        RangeMove = model.rangeMove;
        CurrentState = EnemyState.Idle;
        NextDecision = new TurnDecision { action = EnemyState.Idle, moveTargetLocal = Vector2.zero };
        OnStatChange?.Invoke();
    }

    public TurnDecision PredictNextAction(Vector2 enemyPos, Vector2 playerPos)
    {
        var newDecision = DecideAction(enemyPos, playerPos);

        bool changed = newDecision.action != NextDecision.action
                   || newDecision.moveTargetLocal != NextDecision.moveTargetLocal;

        NextDecision = newDecision;

        if (changed)
        {
            OnNextDecisionChanged?.Invoke(NextDecision);
        }

        return NextDecision;
    }

    public void StartExecution()
    {
        CurrentState = NextDecision.action;
        OnStateChanged?.Invoke();
    }

    public TurnDecision FinishExecutionAndPredictNext(Vector2 enemyPos, Vector2 playerPos, float enemyRadius = 0f)
    {
        CurrentState = EnemyState.Idle;
        OnStateChanged?.Invoke();
        return PredictNextAction(enemyPos, playerPos);
    }

    public void TakeDamage(int value)
    {
        if (CurrentState == EnemyState.Dead) return;

        Life -= value;
        OnStatChange?.Invoke();
        OnHurt?.Invoke(value);

        if (Life <= 0)
        {
            CurrentState = EnemyState.Dead;
            OnDeath?.Invoke();
        }
    }

    public void SetState(EnemyState newState)
    {
        CurrentState = newState;
        OnStateChanged?.Invoke();
    }

    // Décide l'action que l'ennemie va faire pendant son tour (en fonction de la pos du joueur
    public TurnDecision DecideAction(Vector2 enemyPos, Vector2 playerPos)
    {
        TurnDecision turnDecision = new TurnDecision();


        if (CurrentState == EnemyState.Dead)
        {
            turnDecision.action = EnemyState.Dead;
            return turnDecision;
        }


        if (CurrentState == EnemyState.Stunned)
        {
            turnDecision.action = EnemyState.Stunned;
            return turnDecision;
        }


        float dist = Vector2.Distance(enemyPos, playerPos);

        if (dist <= RangeDamage)
        {
            turnDecision.action = EnemyState.Attacking;
            return turnDecision;
        }

        // Si on doit se déplacer, on se déplace d'une distance égal à "RangeMove" mais on s'arrete si on est a porté d'attaque
        Vector2 dir = (playerPos - enemyPos).normalized;
        float desiredMove = Mathf.Min(RangeMove, Mathf.Max(0f, dist - RangeDamage));
        turnDecision.action = EnemyState.Moving;
        turnDecision.moveTargetLocal = enemyPos + dir * desiredMove;
        return turnDecision;
    }
}
