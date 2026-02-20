using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BoardSwitcherUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button previousButton;
    [SerializeField] private Button nextButton;
    [SerializeField] private TextMeshProUGUI boardInfoText;

    [Header("Settings")]
    [SerializeField] private bool startHidden = false;

    private void Start()
    {
        if (previousButton != null)
        {
            previousButton.onClick.AddListener(OnPreviousButtonClicked);
        }

        if (nextButton != null)
        {
            nextButton.onClick.AddListener(OnNextButtonClicked);
        }

        if (BoardManager.Instance != null)
        {
            BoardManager.Instance.OnBoardChanged += OnBoardChanged;
        }

        if (startHidden)
        {
            gameObject.SetActive(false);
        }
        else
        {
            UpdateUI();
        }
    }

    private void OnDestroy()
    {
        if (previousButton != null)
        {
            previousButton.onClick.RemoveListener(OnPreviousButtonClicked);
        }

        if (nextButton != null)
        {
            nextButton.onClick.RemoveListener(OnNextButtonClicked);
        }

        if (BoardManager.Instance != null)
        {
            BoardManager.Instance.OnBoardChanged -= OnBoardChanged;
        }
    }

    private void OnPreviousButtonClicked()
    {
        if (BoardManager.Instance != null)
        {
            BoardManager.Instance.SwitchToPreviousBoard();
        }
    }

    private void OnNextButtonClicked()
    {
        if (BoardManager.Instance != null)
        {
            BoardManager.Instance.SwitchToNextBoard();
        }
    }

    private void OnBoardChanged(int previousIndex, int newIndex)
    {
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (BoardManager.Instance == null)
        {
            if (boardInfoText != null)
            {
                boardInfoText.text = "No BoardManager";
            }
            return;
        }

        UpdateButtonStates();
        UpdateBoardInfo();
    }

    private void UpdateButtonStates()
    {
        int boardCount = BoardManager.Instance.BoardCount;

        if (previousButton != null)
        {
            previousButton.interactable = boardCount > 1;
        }

        if (nextButton != null)
        {
            nextButton.interactable = boardCount > 1;
        }
    }

    private void UpdateBoardInfo()
    {
        if (boardInfoText == null) return;

        int currentIndex = BoardManager.Instance.ActiveBoardIndex;
        int totalBoards = BoardManager.Instance.BoardCount;

        if (totalBoards == 0)
        {
            boardInfoText.text = "No Boards Loaded";
            return;
        }

        Board activeBoard = BoardManager.Instance.ActiveBoard;
        if (activeBoard != null && activeBoard.BoardData != null)
        {
            boardInfoText.text = $"Board {currentIndex + 1}/{totalBoards}\n{activeBoard.BoardData.name}";
        }
        else
        {
            boardInfoText.text = $"Board {currentIndex + 1}/{totalBoards}";
        }
    }

    public void Show()
    {
        gameObject.SetActive(true);
        UpdateUI();
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
