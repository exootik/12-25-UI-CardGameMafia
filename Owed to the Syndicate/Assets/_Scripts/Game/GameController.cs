using System;
using System.Collections;
using System.Linq;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public static GameController Instance;

    public event Action<GameState> OnStateChanged;

    [Header("Level Progression")]
    [SerializeField] private LevelProgression levelProgression;

    [Header("Managers")]
    [SerializeField] private LevelManager levelManager;
    [SerializeField] private TurnManager turnManager;

    [Header("Controllers")]
    [SerializeField] private MainMenuController mainMenuController;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private InventoryController inventoryController;
    [SerializeField] private PauseController pauseController;

    [Header("Views")]
    [SerializeField] private GameView gameView;

    [Header("Victory Settings")]
    [SerializeField] private float victoryDelaySeconds = 2f;

    private GameModel gameModel;
    private bool isEnemyPhaseComplete = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        if (gameView != null)
        {
            gameView.OnEndTurnClickedEvent += EndPlayerTurn;
            gameView.OnVictoryReturnClickedEvent += ReturnToMenu;
            gameView.OnDefeatReturnClickedEvent += ReturnToMenu;
        }

        if (levelManager != null)
        {
            levelManager.OnAllEnemiesDead += OnAllEnemiesDead;
        }

        if (playerController != null)
        {
            playerController.OnPlayerDeath += OnPlayerDeath;
        }

        if (gameView != null)
        {
            gameView.Hide();
        }
    }

    private void OnDestroy()
    {
        if (gameView != null)
        {
            gameView.OnEndTurnClickedEvent -= EndPlayerTurn;
            gameView.OnVictoryReturnClickedEvent -= ReturnToMenu;
            gameView.OnDefeatReturnClickedEvent -= ReturnToMenu;
        }

        if (levelManager != null)
        {
            levelManager.OnAllEnemiesDead -= OnAllEnemiesDead;
        }

        if (playerController != null)
        {
            playerController.OnPlayerDeath -= OnPlayerDeath;
        }
    }

    public void StartCurrentLevel()
    {
        if (levelProgression == null)
        {
            Debug.LogError("[GameController] LevelProgression is null!");
            return;
        }

        LevelDefinition level = levelProgression.GetCurrentLevel();

        if (level == null)
        {
            Debug.LogError("[GameController] No level to start!");
            return;
        }

        Debug.Log($"[GameController] Starting level: {level.name}");

        StartGame(level);
    }

    public void StartGame(LevelDefinition level)
    {
        if (level == null)
        {
            Debug.LogError("[GameController] Level is null!");
            return;
        }

        Debug.Log("[GameController] Starting game");

        gameModel = new GameModel(level);

        if (playerController != null)
        {
            playerController.ResetForNewLevel();
        }

        levelManager.StartLevel(level);

        mainMenuController.HideMenu();
        gameView.Show();
        StartCoroutine(InitializeGameAfterSpawn());
        
    }

    private IEnumerator InitializeGameAfterSpawn()
    {
        yield return new WaitForEndOfFrame();

        gameView?.ShowHelperActionPossibleText();
        turnManager?.PredictAllEnemiesNextAction();
        turnManager?.OrientAllEnemiesTowardsPlayer();

        // On pioche 5 carte au début du jeu 
        if (inventoryController != null)
        {
            Debug.Log("[GameController] Drawing initial 5 cards");
        }

        StartPlayerTurn();
    }

    public void StartPlayerTurn()
    {
        Debug.Log("[GameController] Starting Player Turn");

        inventoryController.Draw(5);

        SetState(GameState.PlayerTurn);

        gameModel.IncrementTurn();

        if (playerController != null)
        {
            playerController.OnTurnStart(); // Restaure le mana
        }

        turnManager.PredictAllEnemiesNextAction();

        StartCoroutine(EnableEndTurnButtonWithDelay());
        gameView.UpdateTurnText(gameModel.CurrentTurn);
    }

    private IEnumerator EnableEndTurnButtonWithDelay()
    {
        yield return new WaitForSeconds(5f);
        gameView.EnableEndTurnButton(true);
    }

    public void EndPlayerTurn()
    {
        Debug.Log("[GameController] Ending Player Turn");

        SetState(GameState.EnemyTurn);

        if (playerController != null)
        {
            playerController.OnTurnEnd();
            inventoryController.ClearHand();
        }

        gameView.EnableEndTurnButton(false);
        gameView.UpdateTurnText(gameModel.CurrentTurn);

        StartCoroutine(ExecuteEnemyPhase());
    }

    private IEnumerator ExecuteEnemyPhase()
    {
        Debug.Log("[GameController] Executing Enemy Phase");

        isEnemyPhaseComplete = false;

        turnManager.StartEnemyPhase();

        //yield return new WaitForSeconds(0.5f);

        // Temps d'attente en fonction du nombre d'ennemis :
        var enemies = FindObjectsByType<EnemyController>(FindObjectsSortMode.None);
        float estimatedTime = enemies.Length * (turnManager.delayBetweenEachEnemy);

        yield return new WaitForSeconds(estimatedTime);

        isEnemyPhaseComplete = true;

        Debug.Log("[GameController] Enemy Phase Complete");

        OnEnemyPhaseComplete();
    }

    private void OnEnemyPhaseComplete()
    {
        Debug.Log("[GameController] Checking game state after enemy phase");

        if (CheckVictory())
        {
            OnVictory();
        }
        else if (CheckDefeat())
        {
            OnDefeat();
        }
        else
        {
            StartPlayerTurn();
        }
    }

    private bool CheckVictory()
    {
        int remaining = levelManager.GetRemainingEnemiesCount();

        Debug.Log($"[GameController] CheckVictory: {remaining} enemies remaining");

        return remaining == 0;
    }

    private bool CheckDefeat()
    {
        if (playerController != null)
        {
            var playerRuntime = playerController.GetPlayerRuntime();
            if (playerRuntime != null && playerRuntime.IsDead)
            {
                Debug.Log("[GameController] Player is dead!");
                return true;
            }
        }

        return false;
    }

    private void OnVictory()
    {
        Debug.Log("[GameController] Victory !");

        SetState(GameState.Victory);

        // Récompense de victoire : 
        if (playerController != null)
        {
            playerController.AddGold(10);
        }

        if (levelProgression != null)
        {
            levelProgression.NextLevel();
        }

        StartCoroutine(ShowVictoryPanelDelayed());
    }

    private IEnumerator ShowVictoryPanelDelayed()
    {
        yield return new WaitForSeconds(victoryDelaySeconds);
        gameView?.ShowVictoryPanel();
    }

    private void OnDefeat()
    {
        Debug.Log("[GameController] Defeat !");

        SetState(GameState.Defeat);
        StartCoroutine(ShowDefeatPanelDelayed());
    }

    private IEnumerator ShowDefeatPanelDelayed()
    {
        yield return new WaitForSeconds(0.5f);
        gameView?.ShowDefeatPanel();
    }

    private void OnAllEnemiesDead()
    {
        Debug.Log("[GameController] All enemies dead - checking victory");

        if (CheckVictory())
        {
            Debug.Log("[GameController] All enemies dead - dans le CheckVictory ");
            OnVictory();
        }
    }

    private void OnPlayerDeath()
    {
        Debug.Log("[GameController] OnPlayerDeath triggered, Player died");
        OnDefeat();
    }

    public void ReturnToMenu()
    {
        Debug.Log("[GameController] Returning to menu");

        if (turnManager != null)
        {
            turnManager.ResetTurnManager();
        }

        if (levelManager != null)
        {
            levelManager.ClearLevel();
        }

        string levelNumber = GetCurrentLevelNumber();

        gameModel = null;

        SetState(GameState.MainMenu);
        gameView.Hide();
        mainMenuController?.ShowMenu(levelNumber);
    }

    private void SetState(GameState newState)
    {
        if (gameModel != null)
        {
            gameModel.SetState(newState);
        }
        OnStateChanged?.Invoke(newState);

        Debug.Log($"[GameController] State changed to: {newState}");
    }

    public PlayerController GetPlayerController()
    {
        return playerController;
    }

    public GameModel GetGameModel()
    {
        return gameModel;
    }

    public PauseController GetPauseController()
    {
        return pauseController;
    }

    public void RestartProgression()
    {
        if (levelProgression != null)
        {
            levelProgression.Restart();
        }
    }

    public bool IsProgressionComplete()
    {
        return levelProgression != null && levelProgression.IsComplete();
    }

    private string GetCurrentLevelNumber()
    {
        if (levelProgression != null)
        {
            var currentLevel = levelProgression.GetCurrentLevel();
            if (currentLevel != null)
            {
                return currentLevel.levelNumber.ToString();
            }
        }
        return "1";
    }
}