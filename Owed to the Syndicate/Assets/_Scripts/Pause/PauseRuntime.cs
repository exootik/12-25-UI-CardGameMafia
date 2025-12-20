using System;
using UnityEngine;

public class PauseRuntime
{
    public bool IsPaused { get; private set; } = false;
    public float TimeWhenPaused { get; private set; } = 0f;

    public event Action OnPauseStateChanged;
    public event Action OnPaused;
    public event Action OnResumed;

    /// <summary>
    /// Met le jeu en pause
    /// </summary>
    public void Pause()
    {
        if (IsPaused) return;

        IsPaused = true;
        TimeWhenPaused = Time.time;

        Debug.Log("[PauseRuntime] Game paused");

        OnPaused?.Invoke();
        OnPauseStateChanged?.Invoke();
    }

    /// <summary>
    /// Reprend le jeu
    /// </summary>
    public void Resume()
    {
        if (!IsPaused) return;

        IsPaused = false;

        Debug.Log("[PauseRuntime] Game resumed");

        OnResumed?.Invoke();
        OnPauseStateChanged?.Invoke();
    }

    /// <summary>
    /// Toggle pause/resume
    /// </summary>
    public void TogglePause()
    {
        if (IsPaused)
        {
            Resume();
        }
        else
        {
            Pause();
        }
    }

    /// <summary>
    /// Reset l'état
    /// </summary>
    public void Reset()
    {
        IsPaused = false;
        TimeWhenPaused = 0f;
    }
}