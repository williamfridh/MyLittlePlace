using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TodoEntryScript : MonoBehaviour
{

    [Header("Target Components")]
    [SerializeField] private Image checkedImage;
    [SerializeField] private Image unCheckedImage;
    [SerializeField] private Image deleteButton;
    [SerializeField] private TextMeshProUGUI titleHolder;
    [SerializeField] private TMP_InputField titleInputField;
    public TodoManagerScript.Todo todo;

    void DrawCheckbox()
    {
        checkedImage.gameObject.SetActive(todo.isComplete);
        unCheckedImage.gameObject.SetActive(!todo.isComplete);
    }

    void DrawTextOrInput()
    {
        bool isEditing = TodoManagerScript.Instance.isEditing;
        titleHolder.gameObject.SetActive(!isEditing);
        titleInputField.gameObject.SetActive(isEditing);
        deleteButton.gameObject.SetActive(isEditing);
    }

    void Refresh()
    {
        titleHolder.text = todo.title;
        titleInputField.text = todo.title;
        DrawObjects();
    }

    void DrawObjects()
    {
        DrawCheckbox();
        DrawTextOrInput();
    }

    public void UpdateTodo()
    {
        todo.title = titleInputField.text;
        TodoManagerScript.Instance.Save();
        Refresh();
    }

    public void HandleDeleteClick()
    {
        MenuButtonAudio.Instance.PlayClickSound();
        TodoManagerScript.Instance.DeleteTodo(todo);
    }
    public void ToggleComplete()
    {
        MenuButtonAudio.Instance.PlayClickSound();
        if (todo.isComplete)
        {
            TodoManagerScript.Instance.MarkTodoAsIncomplete(todo);
        }
        else
        {
            TodoManagerScript.Instance.MarkTodoAsComplete(todo);
        }
        Refresh();
    }
    public void Setup(TodoManagerScript.Todo newTodo)
    {
        todo = newTodo;
        Refresh();
    }

    void Update()
    {
        DrawObjects();
    }
}
