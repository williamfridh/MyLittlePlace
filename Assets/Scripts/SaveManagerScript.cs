using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class SaveManagerScript : MonoBehaviour
{
    public static SaveManagerScript Instance { get; private set; }

    [System.Serializable]
    public class SaveWrapper
    {
        public List<TodoManagerScript.Todo> todoList;
    }

    private string fileName = "user_data_todo.json";
    public SaveWrapper save = new SaveWrapper();

    public bool Load()
    {
        string path = Application.persistentDataPath + "/" + fileName;
        if (!System.IO.File.Exists(path)) return false;
        string json = System.IO.File.ReadAllText(path);
        save = JsonUtility.FromJson<SaveWrapper>(json);
        Debug.Log("SaveManagerScript: Loaded file called '" + fileName + "'");
        return true;
    }

    public void Save()
    {
        string saveJSON = JsonUtility.ToJson(save, true);
        System.IO.File.WriteAllText(Application.persistentDataPath + "/" + fileName, saveJSON); 
    }

    public void UpdateTodoList(List<TodoManagerScript.Todo> newTodoList)
    {
        save.todoList = newTodoList;
        Save();
    }

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        Load();
    }
}
