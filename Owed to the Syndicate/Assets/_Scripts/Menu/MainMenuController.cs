using UnityEngine;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private MainMenuView mainMenuView;

    [Header("Game Controller")]
    [SerializeField] private GameController gameController;
    [SerializeField] private ShopController shopController;

    private void Awake()
    {
        if (mainMenuView != null)
        {
            mainMenuView.OnPlayButtonClicked += OnPlayButtonClicked;
            mainMenuView.OnShopButtonClicked += OnShopButtonClicked;
        }
    }

    private void OnDestroy()
    {
        if (mainMenuView != null)
        {
            mainMenuView.OnPlayButtonClicked -= OnPlayButtonClicked;
            mainMenuView.OnShopButtonClicked -= OnShopButtonClicked;
        }
    }

    private void Start()
    {
        ShowMenu();
    }

    private void OnPlayButtonClicked()
    {
        if (gameController != null)
        {
            gameController.StartCurrentLevel();
        }
    }

    private void OnShopButtonClicked()
    {
        if (shopController != null)
        {
            shopController.OpenShop();
        }
    }

    public void ShowMenu()
    {
        if (mainMenuView != null)
        {
            mainMenuView.Show();
        }
    }

    public void HideMenu()
    {
        if (mainMenuView != null)
        {
            mainMenuView.Hide();
        }
    }
}