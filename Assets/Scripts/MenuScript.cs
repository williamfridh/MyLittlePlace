using UnityEngine;

public class MenuScript : MonoBehaviour
{

    [Header("Menu Panels")]
    [SerializeField] private CanvasGroup settingsMenuPanel;
    [SerializeField] private CanvasGroup todoMenuPanel;
    [SerializeField] private CanvasGroup hudPanel;
    [SerializeField] private CanvasGroup closePanel;

    void Awake()
    {
        HidePanel(settingsMenuPanel);
        HidePanel(todoMenuPanel);
        //HidePanel(hudPanel);
        HidePanel(closePanel);
    }

    public void ShowTodoMenu()
    {
        ShowPanel(todoMenuPanel);
        HidePanel(settingsMenuPanel);
        HidePanel(hudPanel);
        ShowPanel(closePanel);
    }

    public void HideTodoMenu()
    {
        HidePanel(todoMenuPanel);
        HidePanel(closePanel);
        HidePanel(settingsMenuPanel);
        ShowPanel(hudPanel);
    }

    public void ShowSettingsMenu()
    {
        ShowPanel(settingsMenuPanel);
        HidePanel(todoMenuPanel);
        HidePanel(hudPanel);
        ShowPanel(closePanel);
    }
    public void HideSettingsMenu()
    {
        HidePanel(settingsMenuPanel);
        HidePanel(closePanel);
        HidePanel(todoMenuPanel);
        ShowPanel(hudPanel);
    }
    public void Close()
    {
        ShowPanel(hudPanel);
        HidePanel(todoMenuPanel);
        HidePanel(settingsMenuPanel);
        HidePanel(closePanel);
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
