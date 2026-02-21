using UnityEngine;
using UnityEngine.UI;

public class GameUIManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject playButton;
    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject failPanel;
    [SerializeField] private GameObject gameMenuPanel;

    [Header("Win Panel Buttons")]
    [SerializeField] private Button nextButton;
    [SerializeField] private Button retryButtonWin;
    [SerializeField] private Button mainMenuButtonWin;

    [Header("Fail Panel Buttons")]
    [SerializeField] private Button retryButtonFail;
    [SerializeField] private Button mainMenuButtonFail;

    [Header("Game Menu Buttons")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button retryButtonGameMenu;
    [SerializeField] private Button mainMenuButtonGameMenu;

    private Button _playButtonComponent;
    private bool _isPlayButtonEnabled = true;

    private void Start()
    {
        if (playButton != null)
        {
            _playButtonComponent = playButton.GetComponent<Button>();
            if (_playButtonComponent != null)
            {
                _playButtonComponent.onClick.AddListener(OnPlayButtonClicked);
                Debug.Log("[GameUIManager] Play button connected successfully");
            }
            else
            {
                Debug.LogError("[GameUIManager] PlayButton does not have a Button component!");
            }
        }
        else
        {
            Debug.LogError("[GameUIManager] PlayButton GameObject is not assigned!");
        }

        ConnectPanelButtons();
    }

    private void OnDestroy()
    {
        if (playButton != null)
        {
            Button button = playButton.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.RemoveListener(OnPlayButtonClicked);
            }
        }

        DisconnectPanelButtons();
    }

    private void ConnectPanelButtons()
    {
        if (nextButton != null)
            nextButton.onClick.AddListener(OnNextButtonClicked);
        if (retryButtonWin != null)
            retryButtonWin.onClick.AddListener(OnRetryButtonClicked);
        if (mainMenuButtonWin != null)
            mainMenuButtonWin.onClick.AddListener(OnMainMenuButtonClicked);

        if (retryButtonFail != null)
            retryButtonFail.onClick.AddListener(OnRetryButtonClicked);
        if (mainMenuButtonFail != null)
            mainMenuButtonFail.onClick.AddListener(OnMainMenuButtonClicked);

        if (resumeButton != null)
            resumeButton.onClick.AddListener(OnResumeButtonClicked);
        if (retryButtonGameMenu != null)
            retryButtonGameMenu.onClick.AddListener(OnRetryButtonClicked);
        if (mainMenuButtonGameMenu != null)
            mainMenuButtonGameMenu.onClick.AddListener(OnMainMenuButtonClicked);
    }

    private void DisconnectPanelButtons()
    {
        if (nextButton != null)
            nextButton.onClick.RemoveListener(OnNextButtonClicked);
        if (retryButtonWin != null)
            retryButtonWin.onClick.RemoveListener(OnRetryButtonClicked);
        if (mainMenuButtonWin != null)
            mainMenuButtonWin.onClick.RemoveListener(OnMainMenuButtonClicked);

        if (retryButtonFail != null)
            retryButtonFail.onClick.RemoveListener(OnRetryButtonClicked);
        if (mainMenuButtonFail != null)
            mainMenuButtonFail.onClick.RemoveListener(OnMainMenuButtonClicked);

        if (resumeButton != null)
            resumeButton.onClick.RemoveListener(OnResumeButtonClicked);
        if (retryButtonGameMenu != null)
            retryButtonGameMenu.onClick.RemoveListener(OnRetryButtonClicked);
        if (mainMenuButtonGameMenu != null)
            mainMenuButtonGameMenu.onClick.RemoveListener(OnMainMenuButtonClicked);
    }

    public void OnPlayButtonClicked()
    {
        if (!_isPlayButtonEnabled)
        {
            return;
        }

        _isPlayButtonEnabled = false;
        
        if (_playButtonComponent != null)
        {
            _playButtonComponent.interactable = false;
        }

        CharacterMover characterMover = FindAnyObjectByType<CharacterMover>();
        if (characterMover != null && BoardManager.Instance != null)
        {
            int characterBoard = characterMover.GetCurrentBoardIndex();
            if (BoardManager.Instance.ActiveBoardIndex != characterBoard)
            {
                BoardManager.Instance.SetActiveBoard(characterBoard);
                Debug.Log($"[GameUIManager] Switched to character's board {characterBoard} on Play");
            }
        }

        GameStateMachine.Instance?.TransitionTo<State_Playing>();
    }

    public void ShowPlayButton()
    {
        if (playButton != null)
        {
            playButton.gameObject.SetActive(true);
            _isPlayButtonEnabled = true;
            
            if (_playButtonComponent != null)
            {
                _playButtonComponent.interactable = true;
            }
        }
    }

    public void HidePlayButton()
    {
        if (playButton != null)
        {
            playButton.gameObject.SetActive(false);
        }
    }

    public void ShowWinPanel()
    {
        if (winPanel != null)
        {
            winPanel.SetActive(true);
            Debug.Log("[GameUIManager] Win panel shown");
        }
    }

    public void HideWinPanel()
    {
        if (winPanel != null)
        {
            winPanel.SetActive(false);
        }
    }

    public void ShowFailPanel()
    {
        if (failPanel != null)
        {
            failPanel.SetActive(true);
            Debug.Log("[GameUIManager] Fail panel shown");
        }
    }

    public void HideFailPanel()
    {
        if (failPanel != null)
        {
            failPanel.SetActive(false);
        }
    }

    public void ShowGameMenu()
    {
        if (gameMenuPanel != null)
        {
            gameMenuPanel.SetActive(true);
            Debug.Log("[GameUIManager] Game menu shown");
        }
    }

    public void HideGameMenu()
    {
        if (gameMenuPanel != null)
        {
            gameMenuPanel.SetActive(false);
        }
    }

    private void OnResumeButtonClicked()
    {
        Debug.Log("[GameUIManager] Resume button clicked");
        
        if (PauseManager.Instance != null)
        {
            PauseManager.Instance.Resume();
        }
    }

    private void OnNextButtonClicked()
    {
        Debug.Log("[GameUIManager] Next button clicked");
        
        HideWinPanel();
        
        if (LevelManager.Instance != null)
        {
            if (LevelManager.Instance.HasNextLevel)
            {
                LevelManager.Instance.LoadNextLevel();
            }
            else
            {
                Debug.Log("[GameUIManager] No more levels! Restarting from first level.");
                LevelManager.Instance.LoadLevel(0);
            }
        }
        
        if (GameStateMachine.Instance != null)
        {
            GameStateMachine.Instance.TransitionTo<State_Setup>();
        }
    }

    private void OnRetryButtonClicked()
    {
        Debug.Log("[GameUIManager] Retry button clicked");
        
        if (PauseManager.Instance != null)
        {
            PauseManager.Instance.ForceResume();
        }
        
        HideWinPanel();
        HideFailPanel();
        HideGameMenu();
        
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.ReloadCurrentLevel();
        }
        
        if (GameStateMachine.Instance != null)
        {
            GameStateMachine.Instance.TransitionTo<State_Setup>();
        }
    }

    private void OnMainMenuButtonClicked()
    {
        Debug.Log("[GameUIManager] Main Menu button clicked");
        
        if (PauseManager.Instance != null)
        {
            PauseManager.Instance.ForceResume();
        }
        
        OnRetryButtonClicked();
    }

    private void HideAllUI()
    {
        HidePlayButton();
        HideWinPanel();
        HideFailPanel();
        HideGameMenu();
    }
}
