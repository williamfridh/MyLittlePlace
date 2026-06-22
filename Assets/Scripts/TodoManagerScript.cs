using UnityEngine;
using TMPro;
using System.Linq;
using System.Collections.Generic;

public class TodoManagerScript : MonoBehaviour
{
    public static TodoManagerScript Instance { get; private set; }

    // Todo and todo list (array)
    [System.Serializable]
    public class Todo
    {
        public int id;
        public string title;
        public bool isComplete;
        public int parentId;
    }
    [System.Serializable]
    private class TodoSerializationWrapper
    {
        public List<Todo> todoList;
    }
    public List<Todo> todoList = new List<Todo>();

    [Header("Components")]
    [SerializeField] private TextMeshProUGUI amountTextComponent;
    [SerializeField] private GameObject todoContainer;
    [SerializeField] private GameObject rightUiColumnEditing;
    [SerializeField] private GameObject rightUiColumnNotEditing;
    [Header("Prefabs")]
    [SerializeField] private GameObject todoEntryPrefab;
    [Header("Settings")]
    [SerializeField] public bool isEditing = false;

    private int totalTodos;
    private int totalCompletedTodos;

    public void EnableEditing()
    {
        MenuButtonAudio.Instance.PlayClickSound();
        isEditing = true;
        SelectRightUIColumn();

    }
    public void DisableEditing()
    {
        MenuButtonAudio.Instance.PlayClickSound();
        isEditing = false;
        SelectRightUIColumn();
    }
    void SelectRightUIColumn()
    {
        rightUiColumnEditing.SetActive(isEditing);
        rightUiColumnNotEditing.SetActive(!isEditing);
    }

    public void AddTodo(string title, int parentId = 0)
    {
        int highestId = 0;
        foreach(Todo todo in todoList)
        {
            if (todo.id > highestId)
            {
                highestId = todo.id;
            }
        }
        Todo newTodo = new Todo
        {
            id = highestId + 1,
            title = title,
            isComplete = false,
            parentId = parentId
        };

        todoList.Add(newTodo);
        totalTodos++;
        // Save and refresh
        SaveManagerScript.Instance.UpdateTodoList(todoList);
        Refresh();
    }

    public void MarkTodoAsComplete(Todo todo)
    {
        if (todo != null)
        {
            todo.isComplete = true;
            totalCompletedTodos++;
        }
        SaveManagerScript.Instance.UpdateTodoList(todoList);
        updateAmountText();
        DrawTodo();
    }

    public void MarkTodoAsIncomplete(Todo todo)
    {
        if (todo != null && todo.isComplete)
        {
            todo.isComplete = false;
            totalCompletedTodos--;
        }
        SaveManagerScript.Instance.UpdateTodoList(todoList);
        updateAmountText();
        DrawTodo();
    }

    public void DeleteTodo(Todo todo)
    {
        todoList.Remove(todo);
        SaveManagerScript.Instance.UpdateTodoList(todoList);
        Refresh();
    }

    public void DeleteAllComplete()
    {
        MenuButtonAudio.Instance.PlayClickSound();
        List<Todo> todosToDelete = new List<Todo>();
        foreach (Todo todo in todoList)
        {
            if (todo.isComplete)
            {
                todosToDelete.Add(todo);
            }
        }
        foreach (Todo todo in todosToDelete)
        {
            DeleteTodo(todo);
        }
    }

    bool DeleteJsonFile()
    {
        string path = Application.persistentDataPath + "/todoList.json";
        if (!System.IO.File.Exists(path)) return false;
        System.IO.File.Delete(path);
        return true;
    }

    void CalcTodos()
    {
        totalTodos = todoList.Count;
        totalCompletedTodos = 0;
        foreach (Todo todo in todoList)
        {
            if (todo.isComplete)
            {
                totalCompletedTodos++;
            }
        }
    }

    void updateAmountText()
    {
        if (amountTextComponent is null)
        {
            Debug.LogError("TodoScript: No amount text component selected.");
            return;
        }
        amountTextComponent.text = "Total amount: " + totalCompletedTodos + "/" + totalTodos;
    }

    void DrawTodo()
    {
        if (todoContainer == null)
        {
            Debug.LogError("TodoScript: No todo container selected. Cannot draw list.");
            return;
        }
        if (todoEntryPrefab == null)
        {
            Debug.LogError("TodoScript: No todo entry prefab selected. Cannot draw list.");
            return;
        }
        // Remove old list
        foreach (Transform child in todoContainer.transform)
        {
            if (child.name == "Todo Amount") continue;
            Destroy(child.gameObject);
        }
        // Draw new list
        foreach (Todo todo in todoList)
        {
            GameObject newTodoObject = Instantiate(todoEntryPrefab, transform.position, Quaternion.identity);
            newTodoObject.transform.SetParent(todoContainer.transform, false);
            TodoEntryScript scriptComponent = newTodoObject.GetComponent<TodoEntryScript>();
            scriptComponent.Setup(todo);
        }
    }

    public Todo getTodo(int id)
    {
        Todo resultingTodo = todoList.FirstOrDefault(t => t.id == id);
        if (resultingTodo == null)
        {
            Debug.LogWarning("TodoManagerScript: Could not find todo by id " + id);
            return null;
        }
        else
        {
            return resultingTodo;
        }
    }

    public void Save()
    {
        SaveManagerScript.Instance.UpdateTodoList(todoList);
    }
    void Refresh()
    {
        CalcTodos();
        updateAmountText();
        DrawTodo();
        SelectRightUIColumn();
    }

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (!SaveManagerScript.Instance.save.todoListInitilized)
        {
            // If no todoList are loaded, initialize with some default todoList
            AddTodo("Buy groceries");
            AddTodo("Walk the dog");
            AddTodo("Finish homework");
            SaveManagerScript.Instance.UpdateTodoList(todoList);
            SaveManagerScript.Instance.save.todoListInitilized = true;
        }
        else
        {
            todoList = SaveManagerScript.Instance.save.todoList;
        }
        Refresh();
    }

}
