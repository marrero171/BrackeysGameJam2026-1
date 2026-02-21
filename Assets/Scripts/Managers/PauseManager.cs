using UnityEngine;

public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance { get; private set; }
    
    [Header("References")]
    [SerializeField] private GameUIManager uiManager;
    [SerializeField] private GameStateMachine stateMachine;
    
    private bool _isPaused = false;
    public bool IsPaused => _isPaused;
    
    public delegate void PauseStateChangedHandler(bool isPaused);
    public event PauseStateChangedHandler OnPauseStateChanged;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
    }
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (CanPause())
            {
                TogglePause();
            }
        }
    }
    
    private bool CanPause()
    {
        if (stateMachine == null)
            return false;
            
        var currentState = stateMachine.CurrentState;
        
        return currentState is State_Playing || 
               currentState is State_Setup;
    }
    
    public void TogglePause()
    {
        if (_isPaused)
            Resume();
        else
            Pause();
    }
    
    public void Pause()
    {
        if (_isPaused) return;
        
        _isPaused = true;
        Time.timeScale = 0f;
        
        if (uiManager != null)
        {
            uiManager.ShowGameMenu();
        }
        
        OnPauseStateChanged?.Invoke(true);
        Debug.Log("[PauseManager] Game paused");
    }
    
    public void Resume()
    {
        if (!_isPaused) return;
        
        _isPaused = false;
        Time.timeScale = 1f;
        
        if (uiManager != null)
        {
            uiManager.HideGameMenu();
        }
        
        OnPauseStateChanged?.Invoke(false);
        Debug.Log("[PauseManager] Game resumed");
    }
    
    public void ForceResume()
    {
        if (_isPaused)
        {
            _isPaused = false;
            Time.timeScale = 1f;
            
            if (uiManager != null)
            {
                uiManager.HideGameMenu();
            }
        }
    }
    
    private void OnDestroy()
    {
        Time.timeScale = 1f;
        
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
