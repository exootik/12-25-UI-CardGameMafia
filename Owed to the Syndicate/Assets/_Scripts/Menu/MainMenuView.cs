using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuView : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private ButtonCustom playButton; 
    [SerializeField] private ButtonCustom shopButton; 
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private TMP_Text playText;

    public event Action OnPlayButtonClicked;
    public event Action OnShopButtonClicked;

    private void Awake()
    {
        if (playButton != null)
        {
            playButton.OnButtonClicked += OnPlayClicked;
        }

        if (shopButton != null)
        {
            shopButton.OnButtonClicked += OnShopClicked;
        }
    }

    private void OnDestroy()
    {
        if (playButton != null)
        {
            playButton.OnButtonClicked -= OnPlayClicked;
        }

        if (shopButton != null)
        {
            shopButton.OnButtonClicked -= OnShopClicked;
        }
    }

    private void OnPlayClicked()
    {
        OnPlayButtonClicked?.Invoke();
    }

    private void OnShopClicked()
    {
        OnShopButtonClicked?.Invoke();
    }

    public void Show(string currentLevel)
    {
        playText.text = "Start level " + currentLevel;
        if (menuPanel != null)
        {
            menuPanel.SetActive(true);
        }
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        if (menuPanel != null)
        {
            menuPanel.SetActive(false);
        }
        gameObject.SetActive(false);
    }
}
