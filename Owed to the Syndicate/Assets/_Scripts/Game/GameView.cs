using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class GameView : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject gamePanel;
    [SerializeField] private ButtonCustom endTurnButton;
    [SerializeField] private TMP_Text turnText;
    [SerializeField] private TMP_Text helperActionPossibleText;

    [Header("Victory Panel")]
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private ButtonCustom victoryReturnButton;

    [Header("Defeat Panel")]
    [SerializeField] private GameObject defeatPanel;
    [SerializeField] private ButtonCustom defeatReturnButton;


    public event Action OnEndTurnClickedEvent;
    public event Action OnVictoryReturnClickedEvent;
    public event Action OnDefeatReturnClickedEvent;

    private void Awake()
    {
        if (endTurnButton != null)
        {
            endTurnButton.OnButtonClicked += OnEndTurnButtonClicked;
        }

        if (victoryReturnButton != null)
        {
            victoryReturnButton.OnButtonClicked += OnVictoryReturnClicked;
        }

        if (defeatReturnButton != null)
        {
            defeatReturnButton.OnButtonClicked += OnDefeatReturnClicked;
        }
    }

    private void OnDestroy()
    {
        if (endTurnButton != null)
        {
            endTurnButton.OnButtonClicked -= OnEndTurnButtonClicked;
        }

        if (victoryReturnButton != null)
        {
            victoryReturnButton.OnButtonClicked -= OnVictoryReturnClicked;
        }

        if (defeatReturnButton != null)
        {
            defeatReturnButton.OnButtonClicked -= OnDefeatReturnClicked;
        }
    }

    private void OnEndTurnButtonClicked()
    {
        OnEndTurnClickedEvent?.Invoke();
    }

    private void OnVictoryReturnClicked()
    {
        OnVictoryReturnClickedEvent?.Invoke();
    }

    private void OnDefeatReturnClicked()
    {
        OnDefeatReturnClickedEvent?.Invoke();
    }

    public void Show()
    {
        if (gamePanel != null)
        {
            gamePanel.SetActive(true);
        }
        gameObject.SetActive(true);
        HideVictoryPanel();
        HideDefeatPanel();
    }

    public void Hide()
    {
        if (gamePanel != null)
        {
            gamePanel.SetActive(false);
        }
        gameObject.SetActive(false);
    }

    public void EnableEndTurnButton(bool enable)
    {
        if (endTurnButton != null)
        {
            endTurnButton.SetInteractable(enable);
        }
    }

    public void UpdateTurnText(int turnNumber, string phase = "")
    {
        if (turnText != null)
        {
            turnText.text = $"{turnNumber}";
        }
    }

    public void ShowVictoryPanel()
    {
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);
        }
        EnableEndTurnButton(false);
    }

    public void HideVictoryPanel()
    {
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(false);
        }
    }

    public void ShowDefeatPanel()
    {
        if (defeatPanel != null)
        {
            defeatPanel.SetActive(true);
        }
        EnableEndTurnButton(false);
    }

    public void HideDefeatPanel()
    {
        if (defeatPanel != null)
        {
            defeatPanel.SetActive(false);
        }
    }

    public void ShowHelperActionPossibleText()
    {
        if (helperActionPossibleText != null)
        {
            helperActionPossibleText.gameObject.SetActive(true);
            StartCoroutine(HideHelperActionPossibleTextAfterDelay());
        }
    }

    private IEnumerator HideHelperActionPossibleTextAfterDelay()
    {
        yield return new WaitForSeconds(10f);
        helperActionPossibleText.gameObject.SetActive(false);
    }
}