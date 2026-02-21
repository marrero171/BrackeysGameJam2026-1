using UnityEngine;
using UnityEngine.UI;

public class GameUIManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject playButton;
    [SerializeField] private GameObject winText;
    [SerializeField] private GameObject failText;

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

    public void ShowWinText()
    {
        if (winText != null)
        {
            winText.gameObject.SetActive(true);
            Debug.Log("[GameUIManager] Win text shown");
        }
    }

    public void HideWinText()
    {
        if (winText != null)
        {
            winText.gameObject.SetActive(false);
        }
    }

    public void ShowFailText()
    {
        if (failText != null)
        {
            failText.gameObject.SetActive(true);
            Debug.Log("[GameUIManager] Fail text shown");
        }
    }

    public void HideFailText()
    {
        if (failText != null)
        {
            failText.gameObject.SetActive(false);
        }
    }

    private void HideAllUI()
    {
        HidePlayButton();
        HideWinText();
        HideFailText();
    }
}
