using UnityEngine;

public class GameStateScript : MonoBehaviour
{

    public static GameStateScript Instance { get; set; }

    [Header("Current State")]
    [SerializeField] public bool paused = false;

    public bool usingComputer = false;

    public void SetPaused(bool targetState)
    {
        paused = targetState;
        if (targetState && SaveManagerScript.Instance != null) SaveManagerScript.Instance.SaveAll();
    }

    void Awake()
    {
        // Enforce Singleton pattern
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
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
