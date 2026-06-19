using UnityEngine;

public class MenuScript : MonoBehaviour
{

    public static MenuScript Instance { get; private set; }

    [Header("Menu Panels")]
    [SerializeField] private CanvasGroup settingsMenuPanel;
    [SerializeField] private CanvasGroup todoMenuPanel;
    [SerializeField] private CanvasGroup hudPanel;
    [SerializeField] private CanvasGroup addTodoMenuPanel;

    public void ShowTodoMenu()
    {
        CloseAll();
        MenuButtonAudio.Instance.PlayClickSound();
        ShowPanel(todoMenuPanel);
        WorldStateScript.Instance.SetPaused(true);
    }

    public void ShowAddTodoMenu()
    {
        CloseAll();
        MenuButtonAudio.Instance.PlayClickSound();
        ShowPanel(addTodoMenuPanel);
        WorldStateScript.Instance.SetPaused(true);
    }

    public void ShowSettingsMenu()
    {
        CloseAll();
        MenuButtonAudio.Instance.PlayClickSound();
        ShowPanel(settingsMenuPanel);
        WorldStateScript.Instance.SetPaused(true);
    }
    public void Close()
    {
        MenuButtonAudio.Instance.PlayClickSound();
        ShowPanel(hudPanel);
        HidePanel(todoMenuPanel);
        HidePanel(settingsMenuPanel);
        HidePanel(addTodoMenuPanel);
        WorldStateScript.Instance.SetPaused(false);
    }
    public void CloseAll()
    {
        HidePanel(hudPanel);
        HidePanel(todoMenuPanel);
        HidePanel(settingsMenuPanel);
        HidePanel(addTodoMenuPanel);
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

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        ShowPanel(hudPanel);
        HidePanel(todoMenuPanel);
        HidePanel(settingsMenuPanel);
        HidePanel(addTodoMenuPanel);
    }
}
