using UnityEngine;
using System.Text.Json;

public class TodoScript : MonoBehaviour
{

    public class Todo
    {
        public string Title { get; set; }
        public bool IsCompleted { get; set; }
        public Todo parent { get; set; }
    }

    public Todo[] todos;
    public int totalTodos;
    public int totalCompletedTodos;

    void addTodo(string title, Todo parent = null)
    {
        Todo newTodo = new Todo
        {
            Title = title,
            IsCompleted = false,
            parent = parent
        };

        // Add the new todo to the array (this is a simple example, you might want to use a List<Todo> for dynamic resizing)
        int newSize = todos.Length + 1;
        Todo[] newTodosArray = new Todo[newSize];
        for (int i = 0; i < todos.Length; i++)
        {
            newTodosArray[i] = todos[i];
        }
        newTodosArray[newSize - 1] = newTodo;
        todos = newTodosArray;
        totalTodos++;
    }

    void markTodoAsCompleted(Todo todo)
    {
        if (todo != null)
        {
            todo.IsCompleted = true;
            totalCompletedTodos++;
        }
        saveToJson();
    }

    void markTodoAsIncomplete(Todo todo)
    {
        if (todo != null && todo.IsCompleted)
        {
            todo.IsCompleted = false;
            totalCompletedTodos--;
        }
        saveToJson();
    }

    void saveToJson()
    {
        // Save Todos
        string json = JsonSerializer.Serialize(todos);
        System.IO.File.WriteAllText("todos.json", json);
    }

    bool loadFromJson()
    {
        if (!System.IO.File.Exists("todos.json"))
        {
            todos = new Todo[0];
            return false;
        }
        string json = System.IO.File.ReadAllText("todos.json");
        todos = JsonSerializer.Deserialize<Todo[]>(json);
        // Print via Debug.Log to verify loading
        Debug.Log("Loaded Todos:");
        foreach (Todo todo in todos)
        {
            Debug.Log($"Title: {todo.Title}, Completed: {todo.IsCompleted}");
        }
        return true;
    }

    void calcTodos()
    {
        totalTodos = todos.Length;
        totalCompletedTodos = 0;
        foreach (Todo todo in todos)
        {
            if (todo.IsCompleted)
            {
                totalCompletedTodos++;
            }
        }
    }

    void Start()
    {
        if (!loadFromJson())
        {
            // If no todos are loaded, initialize with some default todos
            addTodo("Buy groceries");
            addTodo("Walk the dog");
            addTodo("Finish homework");
        }
        calcTodos();
    }

}
