using UnityEngine;

public class MenuScript : MonoBehaviour
{

    public static MenuScript Instance { get; private set; }

    [Header("Menu Panels")]
    [SerializeField] private CanvasGroup settingsMenuPanel;
    [SerializeField] private CanvasGroup todoMenuPanel;
    [SerializeField] private CanvasGroup hudPanel;
    [SerializeField] private CanvasGroup addTodoMenuPanel;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        Close();
    }

    public void ShowTodoMenu()
    {
        CloseAll();
        MenuButtonAudio.Instance.PlayClickSound();
        ShowPanel(todoMenuPanel);
    }

    public void ShowAddTodoMenu()
    {
        CloseAll();
        MenuButtonAudio.Instance.PlayClickSound();
        ShowPanel(addTodoMenuPanel);
    }

    public void ShowSettingsMenu()
    {
        CloseAll();
        MenuButtonAudio.Instance.PlayClickSound();
        ShowPanel(settingsMenuPanel);
    }
    public void Close()
    {
        MenuButtonAudio.Instance.PlayClickSound();
        ShowPanel(hudPanel);
        HidePanel(todoMenuPanel);
        HidePanel(settingsMenuPanel);
        HidePanel(addTodoMenuPanel);
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
}
