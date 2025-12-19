using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerView : MonoBehaviour
{
    [Header("Gold")]
    [SerializeField] private TMP_Text goldText;

    [Header("Health")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private TMP_Text healthText;

    [Header("Mana)")]
    [SerializeField] private TMP_Text manaText;

    [Header("Animation Settings")]
    [SerializeField] private float barAnimationSpeed = 5f;

    [Header("Take Damage Feedback")]
    [SerializeField] private Image damagePanel;
    public float flashAlpha = 0.5f;
    public float fadeDuration = 1f;

    private float targetHealthValue = 1f;

    /// <summary>
    /// Met à jour l'affichage de la vie
    /// </summary>
    public void UpdateHealth(int current, int max)
    {
        StartCoroutine(FlashRoutine());
        // Texte au milieu du coeur
        if (healthText != null)
        {
            healthText.text = current.ToString();
        }

        // Slider (animation smooth)
        if (healthSlider != null)
        {
            targetHealthValue = max > 0 ? (float)current / max : 0f;
        }
    }


    /// <summary>
    /// Met à jour l'affichage du mana
    /// Juste un nombre dans une image bleue
    /// </summary>
    public void UpdateMana(int current, int max)
    {
        if (manaText != null)
        {
            manaText.text = $"{current}";
        }
    }

    /// <summary>
    /// Met à jour l'affichage de l'or
    /// Icône + texte en overlay sur le portrait du joueur
    /// </summary>
    public void UpdateGold(int current)
    {
        if (goldText != null)
        {
            goldText.text = current.ToString();
        }
    }
    /// <summary>
    /// Animation smooth du slider de vie
    /// </summary>
    private void Update()
    {
        if (healthSlider != null)
        {
            float currentValue = healthSlider.value;
            if (Mathf.Abs(currentValue - targetHealthValue) > 0.001f)
            {
                healthSlider.value = Mathf.Lerp(currentValue, targetHealthValue, Time.deltaTime * barAnimationSpeed);
            }
        }
    }

    private IEnumerator FlashRoutine()
    {
        SetPanelAlpha(flashAlpha);

        var elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            var a = Mathf.Lerp(flashAlpha, 0f, elapsed / fadeDuration);
            SetPanelAlpha(a);
            yield return null;
        }

        SetPanelAlpha(0f);
    }

    private void SetPanelAlpha(float a)
    {
        if (damagePanel == null) return;
        var color = damagePanel.color;
        color.a = a;
        damagePanel.color = color;
    }

    /// <summary>
    /// Force la mise à jour immédiate du slider (sans animation)
    /// </summary>
    public void ForceUpdateBars()
    {
        if (healthSlider != null)
        {
            healthSlider.value = targetHealthValue;
        }
    }
}