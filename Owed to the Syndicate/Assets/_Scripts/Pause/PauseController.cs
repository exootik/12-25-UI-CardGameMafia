using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;

public class PauseController : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private PauseModel pauseModel;

    [Header("View")]
    [SerializeField] private PauseView pauseView;

    [Header("Input")]
    [SerializeField] private InputActionReference escapeKeyAction;

    private PauseRuntime pauseRuntime;

    private void Awake()
    {
        // Initialiser le runtime
        pauseRuntime = new PauseRuntime();

        // Vérifications
        if (pauseView == null)
        {
            Debug.LogError("[PauseController] PauseView is not assigned!");
        }

        if (pauseModel == null)
        {
            Debug.LogWarning("[PauseController] PauseModel is not assigned, using defaults");
        }
    }

    private void Start()
    {
        // S'abonner aux events du runtime
        if (pauseRuntime != null)
        {
            pauseRuntime.OnPaused += OnGamePaused;
            pauseRuntime.OnResumed += OnGameResumed;
        }

        // S'abonner aux events de la vue
        if (pauseView != null)
        {
            pauseView.OnResumeClicked += ResumeGame;
            pauseView.OnReturnToMenuClicked += ReturnToMenu;
            pauseView.OnPauseButtonClicked += PauseGame;
        }

        if (escapeKeyAction != null) escapeKeyAction.action.performed += OnTestActionPerformed;
    }

    private void OnDestroy()
    {
        // Se désabonner des events
        if (pauseRuntime != null)
        {
            pauseRuntime.OnPaused -= OnGamePaused;
            pauseRuntime.OnResumed -= OnGameResumed;
        }

        if (pauseView != null)
        {
            pauseView.OnResumeClicked -= ResumeGame;
            pauseView.OnReturnToMenuClicked -= ReturnToMenu;
            pauseView.OnPauseButtonClicked -= PauseGame;
        }

        if (escapeKeyAction != null) escapeKeyAction.action.performed -= OnTestActionPerformed;

    }

    private void OnTestActionPerformed(InputAction.CallbackContext ctx)
    {
        TogglePause();
    }

    /// <summary>
    /// Toggle pause/resume
    /// </summary>
    public void TogglePause()
    {
        if (pauseRuntime == null) return;

        if (pauseRuntime.IsPaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }

    /// <summary>
    /// Met le jeu en pause
    /// </summary>
    public void PauseGame()
    {
        if (pauseRuntime == null) return;

        pauseRuntime.Pause();
    }

    /// <summary>
    /// Reprend le jeu
    /// </summary>
    public void ResumeGame()
    {
        if (pauseRuntime == null) return;

        pauseRuntime.Resume();
    }

    /// <summary>
    /// Retourne au menu principal
    /// </summary>
    public void ReturnToMenu()
    {
        Debug.Log("[PauseController] Returning to menu");

        // Rétablir le temps avant de quitter
        if (pauseModel != null && pauseModel.PauseTimeOnOpen)
        {
            Time.timeScale = 1f;
        }

        // Reset le runtime
        if (pauseRuntime != null)
        {
            pauseRuntime.Reset();
        }

        // Demander au GameController de retourner au menu
        if (GameController.Instance != null)
        {
            GameController.Instance.ReturnToMenu();
        }
    }

    /// <summary>
    /// Appelé quand le jeu est mis en pause
    /// </summary>
    private void OnGamePaused()
    {
        Debug.Log("[PauseController] OnGamePaused");

        // Afficher le menu pause
        if (pauseView != null)
        {
            pauseView.Show(animate: true);
        }

        // Mettre le temps en pause si configuré
        if (pauseModel != null && pauseModel.PauseTimeOnOpen)
        {
            Time.timeScale = 0f;
            Debug.Log("[PauseController] Time.timeScale set to 0");
        }

        // Désactiver les inputs si configuré
        if (pauseModel != null && pauseModel.DisableInputOnPause)
        {
            // TODO: Désactiver les inputs du joueur
            // Exemple: PlayerInputManager.Instance?.Disable();
        }
    }

    /// <summary>
    /// Appelé quand le jeu reprend
    /// </summary>
    private void OnGameResumed()
    {
        Debug.Log("[PauseController] OnGameResumed");

        // Cacher le menu pause
        if (pauseView != null)
        {
            pauseView.Hide(animate: true);
        }

        // Rétablir le temps si configuré
        if (pauseModel != null && pauseModel.PauseTimeOnOpen)
        {
            Time.timeScale = 1f;
            Debug.Log("[PauseController] Time.timeScale set to 1");
        }

        // Réactiver les inputs si configuré
        if (pauseModel != null && pauseModel.DisableInputOnPause)
        {
            // TODO: Réactiver les inputs du joueur
            // Exemple: PlayerInputManager.Instance?.Enable();
        }
    }

    /// <summary>
    /// Getter pour savoir si le jeu est en pause
    /// </summary>
    public bool IsPaused()
    {
        return pauseRuntime?.IsPaused ?? false;
    }

    /// <summary>
    /// Getter pour le runtime
    /// </summary>
    public PauseRuntime GetPauseRuntime()
    {
        return pauseRuntime;
    }
}