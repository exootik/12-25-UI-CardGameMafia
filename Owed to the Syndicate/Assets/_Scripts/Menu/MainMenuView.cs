using System;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuView : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private ButtonCustom playButton; 
    [SerializeField] private ButtonCustom shopButton; 
    [SerializeField] private GameObject menuPanel;

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

    public void Show()
    {
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
