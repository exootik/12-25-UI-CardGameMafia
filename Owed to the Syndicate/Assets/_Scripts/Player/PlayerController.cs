using System;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private PlayerModel playerModel;

    [Header("View")]
    [SerializeField] private PlayerView playerView;

    private PlayerRuntime playerRuntime;

    public event Action OnPlayerDeath;

    public PlayerRuntime GetPlayerRuntime() => playerRuntime;

    private void Awake()
    {
        playerRuntime = new PlayerRuntime();
    }

    private void Start()
    {
        if (playerModel == null)
        {
            Debug.LogError("[PlayerController] PlayerModel not assigned!");
            return;
        }

        if (playerView == null)
        {
            Debug.LogError("[PlayerController] PlayerView not assigned!");
            return;
        }

        InitializePlayer();
    }

    /// <summary>
    /// Initialise le joueur avec le PlayerModel
    /// </summary>
    public void InitializePlayer()
    {
        if (playerModel == null || playerRuntime == null || playerView == null)
            return;

        playerRuntime.Initialize(playerModel);

        playerRuntime.OnHealthChanged += OnHealthChanged;
        playerRuntime.OnManaChanged += OnManaChanged;
        playerRuntime.OnGoldChanged += OnGoldChanged;
        playerRuntime.OnPlayerDeath += OnPlayerDeathInternal;

        UpdateAllUI();
        playerView.ForceUpdateBars();

        Debug.Log("[PlayerController] Player initialized");
    }

    private void OnDestroy()
    {
        // Se désabonner des events
        if (playerRuntime != null)
        {
            playerRuntime.OnHealthChanged -= OnHealthChanged;
            playerRuntime.OnManaChanged -= OnManaChanged;
            playerRuntime.OnGoldChanged -= OnGoldChanged;
            playerRuntime.OnPlayerDeath -= OnPlayerDeathInternal;
        }
    }

    #region Event Handlers

    private void OnHealthChanged()
    {
        if (playerView != null && playerRuntime != null)
        {
            playerView.UpdateHealth(playerRuntime.CurrentHealth, playerRuntime.MaxHealth);
        }
    }

    private void OnManaChanged()
    {
        if (playerView != null && playerRuntime != null)
        {
            playerView.UpdateMana(playerRuntime.CurrentMana, playerRuntime.MaxMana);
        }
    }

    private void OnGoldChanged()
    {
        if (playerView != null && playerRuntime != null)
        {
            playerView.UpdateGold(playerRuntime.CurrentGold);
        }
    }

    private void OnPlayerDeathInternal()
    {
        Debug.Log("[PlayerController] Player has died!");
        OnPlayerDeath?.Invoke();
    }

    #endregion

    #region Public API - GameController calls these

    /// <summary>
    /// Appelé au début du tour du joueur
    /// </summary>
    public void OnTurnStart()
    {
        Debug.Log("[PlayerController] Turn started");

        // Restaurer le mana
        playerRuntime?.RestoreMana();
    }

    /// <summary>
    /// Appelé à la fin du tour du joueur
    /// </summary>
    public void OnTurnEnd()
    {
        Debug.Log("[PlayerController] Turn ended");

        // Rien à faire pour l'instant, mais vous pourriez ajouter des effets
    }

    /// <summary>
    /// Le joueur prend des dégâts
    /// </summary>
    public void TakeDamage(int amount)
    {
        playerRuntime?.TakeDamage(amount);
    }

    /// <summary>
    /// Le joueur est soigné
    /// </summary>
    public void Heal(int amount)
    {
        playerRuntime?.Heal(amount);
    }

    /// <summary>
    /// Dépenser du mana (pour jouer une carte)
    /// </summary>
    public bool TrySpendMana(int amount)
    {
        if (playerRuntime == null) return false;
        return playerRuntime.SpendMana(amount);
    }

    /// <summary>
    /// Vérifier si le joueur a assez de mana
    /// </summary>
    public bool HasEnoughMana(int amount)
    {
        if (playerRuntime == null) return false;
        return playerRuntime.HasEnoughMana(amount);
    }

    /// <summary>
    /// Ajouter de l'or
    /// </summary>
    public void AddGold(int amount)
    {
        playerRuntime?.AddGold(amount);
    }

    /// <summary>
    /// Dépenser de l'or
    /// </summary>
    public bool TrySpendGold(int amount)
    {
        if (playerRuntime == null) return false;
        return playerRuntime.SpendGold(amount);
    }

    /// <summary>
    /// Vérifier si le joueur a assez d'or
    /// </summary>
    public bool HasEnoughGold(int amount)
    {
        if (playerRuntime == null) return false;
        return playerRuntime.HasEnoughGold(amount);
    }

    /// <summary>
    /// Reset le joueur pour un nouveau niveau (garde l'or)
    /// </summary>
    public void ResetForNewLevel()
    {
        if (playerModel != null && playerRuntime != null)
        {
            playerRuntime.ResetForNewLevel(playerModel);
            UpdateAllUI();
            playerView?.ForceUpdateBars();
        }
    }

    /// <summary>
    /// Reset le joueur (pour recommencer une partie)
    /// </summary>
    public void ResetPlayerComplete()
    {
        if (playerModel != null && playerRuntime != null)
        {
            playerRuntime.ResetComplete(playerModel);
            UpdateAllUI();
            playerView?.ForceUpdateBars();
        }
    }

    #endregion

    #region Helpers

    private void UpdateAllUI()
    {
        OnHealthChanged();
        OnManaChanged();
        OnGoldChanged();
    }

    #endregion

}