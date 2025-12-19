using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Gère la progression de niveaux - ULTRA SIMPLE (sans sauvegarde)
/// </summary>
public class LevelProgression : MonoBehaviour
{
    [Header("Levels")]
    [SerializeField] private List<LevelDefinition> levels = new List<LevelDefinition>();

    private int currentLevelIndex = 0;

    /// <summary>
    /// Obtient le niveau actuel
    /// </summary>
    public LevelDefinition GetCurrentLevel()
    {
        if (levels == null || levels.Count == 0)
        {
            Debug.LogError("[LevelProgression] No levels in list!");
            return null;
        }

        if (currentLevelIndex >= levels.Count)
        {
            Debug.LogError($"[LevelProgression] Level index {currentLevelIndex} out of range!");
            return null;
        }

        return levels[currentLevelIndex];
    }

    /// <summary>
    /// Passe au niveau suivant
    /// </summary>
    public void NextLevel()
    {
        currentLevelIndex++;

        if (currentLevelIndex >= levels.Count)
        {
            Debug.Log("[LevelProgression] All levels completed!");
            currentLevelIndex = levels.Count; // Bloque à la fin
        }

        Debug.Log($"[LevelProgression] Advanced to level {currentLevelIndex}");
    }

    /// <summary>
    /// Recommence depuis le début
    /// </summary>
    public void Restart()
    {
        currentLevelIndex = 0;
        Debug.Log("[LevelProgression] Restarted from level 0");
    }

    /// <summary>
    /// Vérifie si tous les niveaux sont terminés
    /// </summary>
    public bool IsComplete()
    {
        return currentLevelIndex >= levels.Count;
    }

    /// <summary>
    /// Info pour affichage
    /// </summary>
    public string GetLevelInfo()
    {
        if (currentLevelIndex >= levels.Count)
        {
            return $"{levels.Count}/{levels.Count} (Terminé)";
        }
        return $"{currentLevelIndex + 1}/{levels.Count}";
    }

    /// <summary>
    /// Obtient l'index actuel
    /// </summary>
    public int GetCurrentIndex()
    {
        return currentLevelIndex;
    }
}