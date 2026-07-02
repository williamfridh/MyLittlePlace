using UnityEngine;
using UnityEngine.SceneManagement;

public class GameStateScript : MonoBehaviour
{

    public static GameStateScript Instance { get; set; }

    [Header("Current State")]
    [SerializeField] public bool paused = false;
    [SerializeField] public bool worldRendered = false;

    public bool usingComputer = false;

    public void SetPaused(bool targetState)
    {
        paused = targetState;
        if (targetState && SaveManagerScript.Instance != null) SaveManagerScript.Instance.SaveAll();
    }

    private void Awake()
    {
        // Enforce Singleton Pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Subscribe to scene loading events
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe form event to prveent memoery leaks
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Main Menu") Destroy(gameObject);
    }

    void Start()
    {
        // Prevent screen timeout
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        // Detect system usage
        if (Application.platform == RuntimePlatform.WindowsPlayer || 
            Application.platform == RuntimePlatform.OSXPlayer || 
            Application.platform == RuntimePlatform.LinuxPlayer)
        {
            Debug.Log("GameStateScript: Computer detected");
            usingComputer = true;
        }
        else if (Application.platform == RuntimePlatform.Android)
        {
            Debug.Log("GameStateScript: Android detected");
            usingComputer = false;
        }
        else
        {
            Debug.Log("GameStateScript: Unknown detected. Fallback to smartphone");
            usingComputer = false;
        }
    }
}
