using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PauseView : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private ButtonCustom resumeButton;
    [SerializeField] private ButtonCustom returnToMenuButton;

    [Header("Pause Button (in game UI)")]
    [SerializeField] private ButtonCustom pauseButton;

    [Header("Animations")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float fadeSpeed = 5f;

    public event Action OnResumeClicked;
    public event Action OnReturnToMenuClicked;
    public event Action OnPauseButtonClicked;

    private bool isAnimating = false;

    private void Awake()
    {
        // S'assurer que le panel est caché au départ
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        // Connecter les boutons
        if (resumeButton != null)
        {
            resumeButton.OnButtonClicked += HandleResumeClick;

        }

        if (returnToMenuButton != null)
        {
            returnToMenuButton.OnButtonClicked += HandleReturnToMenuClick;
        }

        if (pauseButton != null)
        {
            pauseButton.OnButtonClicked += HandlePauseButtonClick;
            Debug.Log("[PauseView] Pause button connected");
        }
        else
        {
            Debug.LogWarning("[PauseView] Pause button is not assigned!");
        }

        // Initialiser le CanvasGroup si pas assigné
        if (canvasGroup == null && pausePanel != null)
        {
            canvasGroup = pausePanel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = pausePanel.AddComponent<CanvasGroup>();
            }
        }
    }

    private void OnDestroy()
    {
        // Déconnecter les boutons
        if (resumeButton != null)
        {
            resumeButton.OnButtonClicked -= HandleResumeClick;
        }

        if (returnToMenuButton != null)
        {
            returnToMenuButton.OnButtonClicked -= HandleReturnToMenuClick;
        }

        if (pauseButton != null)
        {
            pauseButton.OnButtonClicked -= HandlePauseButtonClick;
        }
    }

    /// <summary>
    /// Affiche le menu pause
    /// </summary>
    public void Show(bool animate = true)
    {
        if (pausePanel == null)
        {
            Debug.LogError("[PauseView] pausePanel is null!");
            return;
        }

        pausePanel.SetActive(true);

        if (animate && canvasGroup != null)
        {
            StartCoroutine(FadeIn());
        }
        else if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
        }

        Debug.Log("[PauseView] Pause menu shown");
    }

    /// <summary>
    /// Cache le menu pause
    /// </summary>
    public void Hide(bool animate = true)
    {
        if (pausePanel == null) return;

        if (animate && canvasGroup != null)
        {
            StartCoroutine(FadeOut());
        }
        else
        {
            pausePanel.SetActive(false);
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
            }
        }

        Debug.Log("[PauseView] Pause menu hidden");
    }

    /// <summary>
    /// Active/désactive les boutons
    /// </summary>
    public void SetButtonsInteractable(bool interactable)
    {
        if (resumeButton != null)
        {
            resumeButton.interactable = interactable;
        }

        if (returnToMenuButton != null)
        {
            returnToMenuButton.interactable = interactable;
        }
    }

    private void HandleResumeClick()
    {
        Debug.Log("[PauseView] Resume button clicked");
        OnResumeClicked?.Invoke();
    }

    private void HandleReturnToMenuClick()
    {
        Debug.Log("[PauseView] Return to menu button clicked");
        OnReturnToMenuClicked?.Invoke();
    }

    private void HandlePauseButtonClick()
    {
        Debug.Log("[PauseView] Pause button clicked");
        OnPauseButtonClicked?.Invoke();
    }

    private System.Collections.IEnumerator FadeIn()
    {
        if (canvasGroup == null) yield break;

        isAnimating = true;
        canvasGroup.alpha = 0f;

        while (canvasGroup.alpha < 1f)
        {
            canvasGroup.alpha += Time.unscaledDeltaTime * fadeSpeed;
            yield return null;
        }

        canvasGroup.alpha = 1f;
        isAnimating = false;
    }

    private System.Collections.IEnumerator FadeOut()
    {
        if (canvasGroup == null) yield break;

        isAnimating = true;

        while (canvasGroup.alpha > 0f)
        {
            canvasGroup.alpha -= Time.unscaledDeltaTime * fadeSpeed;
            yield return null;
        }

        canvasGroup.alpha = 0f;
        pausePanel.SetActive(false);
        isAnimating = false;
    }
}