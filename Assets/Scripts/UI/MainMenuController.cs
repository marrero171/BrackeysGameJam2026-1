using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button creditsButton;
    [SerializeField] private Button quitButton;

    [Header("Credits Panel")]
    [SerializeField] private GameObject creditsPanel;
    [SerializeField] private Button creditsCloseButton;

    [Header("Scene Settings")]
    [SerializeField] private string gameSceneName = "PrototypeScene";

    private void Start()
    {
        if (startButton != null)
        {
            startButton.onClick.AddListener(OnStartClicked);
        }

        if (creditsButton != null)
        {
            creditsButton.onClick.AddListener(OnCreditsClicked);
        }

        if (quitButton != null)
        {
            quitButton.onClick.AddListener(OnQuitClicked);
        }

        if (creditsCloseButton != null)
        {
            creditsCloseButton.onClick.AddListener(OnCreditsCloseClicked);
        }

        if (creditsPanel != null)
        {
            creditsPanel.SetActive(false);
        }

        Debug.Log("[MainMenuController] Initialized");
    }

    private void OnDestroy()
    {
        if (startButton != null)
        {
            startButton.onClick.RemoveListener(OnStartClicked);
        }

        if (creditsButton != null)
        {
            creditsButton.onClick.RemoveListener(OnCreditsClicked);
        }

        if (quitButton != null)
        {
            quitButton.onClick.RemoveListener(OnQuitClicked);
        }

        if (creditsCloseButton != null)
        {
            creditsCloseButton.onClick.RemoveListener(OnCreditsCloseClicked);
        }
    }

    public void OnStartClicked()
    {
        Debug.Log("[MainMenuController] Start button clicked - Loading game scene");
        SceneManager.LoadScene(gameSceneName);
    }

    public void OnCreditsClicked()
    {
        Debug.Log("[MainMenuController] Credits button clicked");
        if (creditsPanel != null)
        {
            creditsPanel.SetActive(true);
        }
    }

    public void OnCreditsCloseClicked()
    {
        Debug.Log("[MainMenuController] Credits close button clicked");
        if (creditsPanel != null)
        {
            creditsPanel.SetActive(false);
        }
    }

    public void OnQuitClicked()
    {
        Debug.Log("[MainMenuController] Quit button clicked - Exiting application");

        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}
