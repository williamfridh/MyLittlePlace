using UnityEngine;

public class WorldStateScript : MonoBehaviour
{

    public static WorldStateScript Instance { get; set; }

    [Header("Current State")]
    [SerializeField] public bool paused = false;

    public void SetPaused(bool targetState) => paused = targetState;

    void Awake()
    {
        // Enforce Singleton pattern
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
}
