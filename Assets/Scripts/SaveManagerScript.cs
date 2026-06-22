using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;

public class SaveManagerScript : MonoBehaviour
{
    public static SaveManagerScript Instance { get; private set; }

    [SerializeField] private bool forceNewSave = false;

    [System.Serializable]
    public class SaveWrapper
    {
        public List<TodoManagerScript.Todo> todoList;
        public bool todoListInitilized = false;
        public WorldSaveState world;
        public bool worldInitilized = false;
    }

    private string fileName = "save.json";
    public SaveWrapper save = new SaveWrapper();

    public void Load()
    {
        string path = Application.persistentDataPath + "/" + fileName;
        if (!System.IO.File.Exists(path) || forceNewSave)
        {
            // Generate new world
            WorldSaveState newWorld = WorldGeneratorScript.Instance.GenerateWorld(300, 300, 0.08f, 3);
            // Create new empty save.
            save = new SaveWrapper
            {
                todoList = new List<TodoManagerScript.Todo>(),
                world = newWorld,
                worldInitilized = true
            };
            Save();
            Debug.Log("SaveManagerScript: Missing save file, creating new...");
        }
        ;
        string json = System.IO.File.ReadAllText(path);
        save = JsonUtility.FromJson<SaveWrapper>(json);
        Debug.Log("SaveManagerScript: Loaded file called '" + fileName + "'");
        // Load world scene
        SceneManager.LoadScene("World Scene");
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

    public void UpdateWorld(WorldSaveState world)
    {
        save.world = world;
        Save();
    }

    void Awake()
    {
        if (Instance == null) 
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else 
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Note that we call load from start and not Awake to avoid race condition
        Load();
    }
}
