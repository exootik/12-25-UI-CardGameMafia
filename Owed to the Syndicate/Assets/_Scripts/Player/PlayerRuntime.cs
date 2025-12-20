using System;
using UnityEngine;

/// <summary>
/// Runtime representation of the player. Holds current stats and emits events when they change.
/// Similar to RuntimeInventory, this is the MODEL that holds the actual game data.
/// </summary>
public class PlayerRuntime
{
    public event Action OnHealthChanged;
    public event Action OnManaChanged;
    public event Action OnGoldChanged;
    public event Action OnPlayerDeath;
    public event Action OnPlayerTakeDamage;

    public int CurrentHealth { get; private set; }
    public int MaxHealth { get; private set; }

    public int CurrentMana { get; private set; }
    public int MaxMana { get; private set; }
    public int ManaPerTurn { get; private set; }

    public int CurrentGold { get; private set; }

    public bool IsDead => CurrentHealth <= 0;

    /// <summary>
    /// Initialise le joueur avec un PlayerModel (ScriptableObject)
    /// </summary>
    public void Initialize(PlayerModel model)
    {
        if (model == null)
        {
            Debug.LogError("[PlayerRuntime] PlayerModel is null!");
            return;
        }

        MaxHealth = model.MaxHealth;
        CurrentHealth = MaxHealth;

        MaxMana = model.MaxMana;
        CurrentMana = 4; 
        ManaPerTurn = model.ManaPerTurn;

        CurrentGold = model.StartingGold;

        Debug.Log($"[PlayerRuntime] Initialized: HP={CurrentHealth}/{MaxHealth}, Mana={CurrentMana}/{MaxMana}, Gold={CurrentGold}");

        OnHealthChanged?.Invoke();
        OnManaChanged?.Invoke();
        OnGoldChanged?.Invoke();
    }

    #region Health Management

    /// <summary>
    /// Inflige des dégâts au joueur
    /// </summary>
    public void TakeDamage(int amount)
    {
        if (amount <= 0 || IsDead) return;

        CurrentHealth -= amount;
        CurrentHealth = Mathf.Max(0, CurrentHealth);

        Debug.Log($"[PlayerRuntime] Took {amount} damage. HP: {CurrentHealth}/{MaxHealth}");

        OnHealthChanged?.Invoke();
        OnPlayerTakeDamage?.Invoke();

        if (IsDead)
        {
            Debug.Log("[PlayerRuntime] Player died!");
            OnPlayerDeath?.Invoke();
        }
    }

    /// <summary>
    /// Soigne le joueur
    /// </summary>
    public void Heal(int amount)
    {
        if (amount <= 0 || IsDead) return;

        CurrentHealth += amount;
        CurrentHealth = Mathf.Min(CurrentHealth, MaxHealth);

        Debug.Log($"[PlayerRuntime] Healed {amount}. HP: {CurrentHealth}/{MaxHealth}");

        OnHealthChanged?.Invoke();
    }

    /// <summary>
    /// Définit la vie du joueur (pour les cas spéciaux)
    /// </summary>
    public void SetHealth(int value)
    {
        if (IsDead) return;

        CurrentHealth = Mathf.Clamp(value, 0, MaxHealth);
        OnHealthChanged?.Invoke();

        if (IsDead)
        {
            OnPlayerDeath?.Invoke();
        }
    }

    #endregion

    #region Mana Management

    /// <summary>
    /// Restaure le mana au début du tour
    /// </summary>
    public void RestoreMana()
    {
        CurrentMana += ManaPerTurn;
        CurrentMana = Math.Min(CurrentMana, MaxMana);

        Debug.Log($"[PlayerRuntime] Mana restored. Mana: {CurrentMana}/{MaxMana}");

        OnManaChanged?.Invoke();
    }

    /// <summary>
    /// Dépense du mana (pour jouer une carte)
    /// </summary>
    public bool SpendMana(int amount)
    {
        if (amount <= 0)
        {
            Debug.LogWarning("[PlayerRuntime] Cannot spend 0 or negative mana");
            return false;
        }

        if (CurrentMana < amount)
        {
            Debug.LogWarning($"[PlayerRuntime] Not enough mana! Need {amount}, have {CurrentMana}");
            return false;
        }

        CurrentMana -= amount;

        Debug.Log($"[PlayerRuntime] Spent {amount} mana. Remaining: {CurrentMana}/{MaxMana}");

        OnManaChanged?.Invoke();
        return true;
    }

    /// <summary>
    /// Vérifie si le joueur a assez de mana
    /// </summary>
    public bool HasEnoughMana(int amount)
    {
        return CurrentMana >= amount;
    }

    /// <summary>
    /// Définit le mana actuel 
    /// </summary>
    public void SetMana(int value)
    {
        CurrentMana = Mathf.Clamp(value, 0, MaxMana);
        OnManaChanged?.Invoke();
    }

    /// <summary>
    /// Augmente le mana max (pour des upgrades)
    /// </summary>
    public void IncreaseMaxMana(int amount)
    {
        MaxMana += amount;
        Debug.Log($"[PlayerRuntime] Max mana increased to {MaxMana}");
        OnManaChanged?.Invoke();
    }

    #endregion

    #region Gold Management

    /// <summary>
    /// Ajoute de l'or
    /// </summary>
    public void AddGold(int amount)
    {
        if (amount <= 0) return;

        CurrentGold += amount;

        Debug.Log($"[PlayerRuntime] Gained {amount} gold. Total: {CurrentGold}");

        OnGoldChanged?.Invoke();
    }

    /// <summary>
    /// Dépense de l'or
    /// </summary>
    public bool SpendGold(int amount)
    {
        if (amount <= 0)
        {
            Debug.LogWarning("[PlayerRuntime] Cannot spend 0 or negative gold");
            return false;
        }

        if (CurrentGold < amount)
        {
            Debug.LogWarning($"[PlayerRuntime] Not enough gold! Need {amount}, have {CurrentGold}");
            return false;
        }

        CurrentGold -= amount;

        Debug.Log($"[PlayerRuntime] Spent {amount} gold. Remaining: {CurrentGold}");

        OnGoldChanged?.Invoke();
        return true;
    }

    /// <summary>
    /// Vérifie si le joueur a assez d'or
    /// </summary>
    public bool HasEnoughGold(int amount)
    {
        return CurrentGold >= amount;
    }

    /// <summary>
    /// Définit l'or actuel
    /// </summary>
    public void SetGold(int value)
    {
        CurrentGold = Mathf.Max(0, value);
        OnGoldChanged?.Invoke();
    }

    #endregion

    #region Reset

    public void ResetForNewLevel(PlayerModel model)
    {
        if (model == null)
        {
            Debug.LogError("[PlayerRuntime] PlayerModel is null!");
            return;
        }

        // Restaurer vie et mana
        MaxHealth = model.MaxHealth;
        CurrentHealth = MaxHealth;

        MaxMana = model.MaxMana;
        CurrentMana = model.MaxMana;
        ManaPerTurn = model.ManaPerTurn;

        // On garde l'or actuel

        Debug.Log($"[PlayerRuntime] Reset for new level: HP={CurrentHealth}/{MaxHealth}, Mana={CurrentMana}/{MaxMana}, Gold={CurrentGold} (kept)");

        OnHealthChanged?.Invoke();
        OnManaChanged?.Invoke();
    }

    /// <summary>
    /// Reset le joueur (pour recommencer une partie)
    /// </summary>
    public void ResetComplete(PlayerModel model)
    {
        Initialize(model);
    }

    #endregion
}