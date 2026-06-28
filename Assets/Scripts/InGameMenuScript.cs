using UnityEngine;
using UnityEngine.SceneManagement;

public class InGameMenuScript : MonoBehaviour
{

    public static InGameMenuScript Instance { get; private set; }

    [Header("Menu Panels")]
    [SerializeField] private CanvasGroup settingsMenuPanel;
    [SerializeField] private CanvasGroup todoMenuPanel;
    [SerializeField] private CanvasGroup hudPanel;
    [SerializeField] private CanvasGroup addTodoMenuPanel;
    [SerializeField] private CanvasGroup inventoryPanel;

    public void ShowTodoMenu()
    {
        HidePanel(hudPanel);
        CloseMenues();
        MenuButtonAudio.Instance.PlayClickSound();
        ShowPanel(todoMenuPanel);
        GameStateScript.Instance.SetPaused(true);
    }

    public void ShowAddTodoMenu()
    {
        if (addTodoMenuPanel == null)
        {
            Debug.LogError("InGameMenuScript: addTodoMenuPanel is not set!");
            return;
        }
        HidePanel(hudPanel);
        CloseMenues();
        MenuButtonAudio.Instance.PlayClickSound();
        ShowPanel(addTodoMenuPanel);
        GameStateScript.Instance.SetPaused(true);
    }

    public void ShowSettingsMenu()
    {
        if (settingsMenuPanel == null)
        {
            Debug.LogError("InGameMenuScript: addTodoMenuPanel is not set!");
            return;
        }
        HidePanel(hudPanel);
        CloseMenues();
        MenuButtonAudio.Instance.PlayClickSound();
        ShowPanel(settingsMenuPanel);
        GameStateScript.Instance.SetPaused(true);
    }

    public void ShowInventory()
    {
        if (settingsMenuPanel == null)
        {
            Debug.LogError("InGameMenuScript: inventoryPanel is not set!");
            return;
        }
        if (InventoryPanelScript.Instance == null)
        {
            Debug.LogError("TodoManagerScript: Could not find InventoryPanelScript instance. Won't display inventory!");
            return;
        }
        InventoryPanelScript.Instance.RefreshInventoryUI();
        HidePanel(hudPanel);
        CloseMenues();
        MenuButtonAudio.Instance.PlayClickSound();
        ShowPanel(inventoryPanel);
        GameStateScript.Instance.SetPaused(true);
    }
    public void Close()
    {
        MenuButtonAudio.Instance.PlayClickSound();
        ShowPanel(hudPanel);
        CloseMenues();
        GameStateScript.Instance.SetPaused(false);
    }
    public void CloseMenues()
    {
        HidePanel(todoMenuPanel);
        HidePanel(settingsMenuPanel);
        HidePanel(addTodoMenuPanel);
        HidePanel(inventoryPanel);
    }

    void ShowPanel(CanvasGroup panel)
    {
        if (panel == null) return;

        panel.alpha = 1f;               // Make instance visible
        panel.interactable = true;      // Allow interaction with instance
        panel.blocksRaycasts = true;    // Allow instance to receive raycasts
    }

    void HidePanel(CanvasGroup panel)
    {
        if (panel == null) return;

        panel.alpha = 0f;               // Make instance invisible
        panel.interactable = false;     // Disable interaction with instance
        panel.blocksRaycasts = false;   // Prevent instance from receiving raycasts
    }

    public void HandleGoToMenuClick()
    {
        if (SaveManagerScript.Instance == null)
        {
            Debug.LogWarning("InGameMenuScript: Cannot find SaveManagerScript Instance, thus cannot trigger save.");
        }
        else
        {
            SaveManagerScript.Instance.SaveAll();
        }
        AmbienceAudioManager.Instance.TransitionToNone();
        AmbienceMusicManager.Instance.TransitionToBiome(BiomeType.Menu);
        MenuButtonAudio.Instance.PlayClickSound();
        SceneManager.LoadScene("Main Menu");
    }

    public void HandleExitGameClick()
    {
        Debug.Log("MainMenuScript: Exit game requested!");
        AmbienceAudioManager.Instance.TransitionToNone();
        AmbienceMusicManager.Instance.TransitionToBiome(BiomeType.Menu);
        if (SaveManagerScript.Instance == null)
        {
            Debug.LogWarning("InGameMenuScript: Cannot find SaveManagerScript Instance, thus cannot trigger save.");
        }
        else
        {
            SaveManagerScript.Instance.SaveAll();
        }
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        CloseMenues();
        ShowPanel(hudPanel);
    }
}
