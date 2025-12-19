using System;
using UnityEngine;

public enum GameState
{
    MainMenu,
    PlayerTurn,
    EnemyTurn,
    Victory,
    Defeat
}
public class GameModel : MonoBehaviour
{

    public event Action<GameState> OnStateChanged;
    public GameState CurrentState { get; private set; }
    public LevelDefinition CurrentLevel { get; private set; }
    public int CurrentTurn { get; private set; }

    public GameModel(LevelDefinition level)
    {
        CurrentLevel = level;
        CurrentTurn = 0;
        CurrentState = GameState.MainMenu;
    }

    public void SetState(GameState newState)
    {
        if (CurrentState == newState) return;

        CurrentState = newState;
        OnStateChanged?.Invoke(newState);
    }

    public void IncrementTurn()
    {
        CurrentTurn++;
    }

    public void ResetTurn()
    {
        CurrentTurn = 0;
    }
}
