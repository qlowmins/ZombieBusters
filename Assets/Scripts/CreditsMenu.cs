using UnityEngine;
using UnityEngine.UI;

public sealed class CreditsMenu : MonoBehaviour
{
    [SerializeField] private GameObject menuButtons;
    [SerializeField] private GameObject panel;
    [SerializeField] private Button openButton;
    [SerializeField] private Button backButton;
    [SerializeField] private ScrollRect scrollView;
    [SerializeField] private PauseManager pauseManager;
    [SerializeField] private GameObject weaponStorePanel;

    private bool pauseWasEnabled;

    private void Awake()
    {
        openButton.onClick.AddListener(Open);
        backButton.onClick.AddListener(Close);
    }

    private void Open()
    {
        if (pauseManager.pauseMenuUI.activeSelf || weaponStorePanel.activeSelf)
        {
            return;
        }

        pauseWasEnabled = pauseManager.enabled;
        pauseManager.enabled = false;
        menuButtons.SetActive(false);
        panel.SetActive(true);
        Canvas.ForceUpdateCanvases();
        scrollView.StopMovement();
        scrollView.verticalNormalizedPosition = 1f;
        backButton.Select();
    }

    private void Close()
    {
        panel.SetActive(false);
        menuButtons.SetActive(true);
        pauseManager.enabled = pauseWasEnabled;
        openButton.Select();
    }
}
