using UnityEngine;
using UnityEngine.UI;

public class GameUIManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject playButton;
    [SerializeField] private GameObject winText;
    [SerializeField] private GameObject failText;

    private void Start()
    {
        if (playButton != null)
        {
            Button button = playButton.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(OnPlayButtonClicked);
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
        Debug.Log("[GameUIManager] Play button clicked");
        GameStateMachine.Instance?.TransitionTo<State_Playing>();
    }

    public void ShowPlayButton()
    {
        if (playButton != null)
        {
            playButton.gameObject.SetActive(true);
            Debug.Log("[GameUIManager] Play button shown");
        }
    }

    public void HidePlayButton()
    {
        if (playButton != null)
        {
            playButton.gameObject.SetActive(false);
            Debug.Log("[GameUIManager] Play button hidden");
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
