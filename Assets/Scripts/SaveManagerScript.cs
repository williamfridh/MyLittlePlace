using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Threading.Tasks;

public class SaveManagerScript : MonoBehaviour
{
    public static SaveManagerScript Instance { get; private set; }

    [System.Serializable]
    public class SaveWrapperTodo
    {
        public List<TodoManagerScript.Todo> todoList;
        public bool todoListInitilized = false;
    }

    private string fileNameTodoSave = "save_todo.json";
    private string fileNameWorldSave = "save_world.json";
    private string fileNamePlayerSave = "save_player.json";

    private string todoSavePath;
    private string worldSavePath;
    private string playerSavePath;
    
    public SaveWrapperTodo todoSave = new SaveWrapperTodo();
    public WorldSaveState worldSave = new WorldSaveState();
    public PlayerSaveState playerSave = new PlayerSaveState();

    public bool SaveExists()
    {
        return System.IO.File.Exists(todoSavePath) &&
               System.IO.File.Exists(worldSavePath) &&
               System.IO.File.Exists(playerSavePath);
    }

    public async void Load(bool forceNewTodo, bool forceNewWorld, bool forceNewPlayer)
    {
        // Load todo
        if (!System.IO.File.Exists(todoSavePath) || forceNewTodo)
        {
            todoSave = new SaveWrapperTodo
            {
                todoList = new List<TodoManagerScript.Todo>()
            };
            await SaveTodoAsync();
            Debug.Log("SaveManagerScript: Missing todo save file, creating new...");
        }
        else
        {
            string json = await System.IO.File.ReadAllTextAsync(todoSavePath);
            todoSave = JsonUtility.FromJson<SaveWrapperTodo>(json);
            Debug.Log("SaveManagerScript: Loaded file called '" + fileNameTodoSave + "'");
        }

        // Load player
        if (!System.IO.File.Exists(playerSavePath) || forceNewPlayer)
        {
            playerSave = new PlayerSaveState();
            await SavePlayerAsync();
            Debug.Log("SaveManagerScript: Missing player save file, creating new...");
        }
        else
        {
            string json = await System.IO.File.ReadAllTextAsync(playerSavePath);
            playerSave = JsonUtility.FromJson<PlayerSaveState>(json);
            Debug.Log("SaveManagerScript: Loaded file called '" + fileNamePlayerSave + "'");
        }

        // Load world
        if (!System.IO.File.Exists(worldSavePath) || forceNewWorld)
        {
            // Generate new world
            StartCoroutine(WorldGeneratorScript.Instance.GenerateWorldRoutine((newWorld) =>
            {
                // Create new save.
                worldSave = newWorld;
                _ = SaveWorldAsync();
                Debug.Log("SaveManagerScript: Missing world save file, creating new...");
                // Load world scene
                SceneManager.LoadScene("World");
            }
            ));
        }
        else
        {
            string json = await System.IO.File.ReadAllTextAsync(worldSavePath);
            worldSave = JsonUtility.FromJson<WorldSaveState>(json);
            Debug.Log("SaveManagerScript: Loaded file called '" + fileNameWorldSave + "'");
            // Load world scene
            SceneManager.LoadScene("World");
        }
    }

    public async Task SaveWorldAsync()
    {
        string saveJSON = JsonUtility.ToJson(worldSave, true);
        await Task.Run(() => System.IO.File.WriteAllText(worldSavePath, saveJSON));
        Debug.Log("SaveManagerScript: Saved world");
    }
    public async Task SaveTodoAsync()
    {
        string saveJSON = JsonUtility.ToJson(todoSave, true);
        await Task.Run(() => System.IO.File.WriteAllText(todoSavePath, saveJSON));
        Debug.Log("SaveManagerScript: Saved todo");
    }
    public async Task SavePlayerAsync()
    {
        string saveJSON = JsonUtility.ToJson(playerSave, true);
        await Task.Run(() => System.IO.File.WriteAllText(playerSavePath, saveJSON));
        Debug.Log("SaveManagerScript: Saved player");
    }

    public void SaveWorld()
    {
        string saveJSON = JsonUtility.ToJson(worldSave, true);
        System.IO.File.WriteAllText(Application.persistentDataPath + "/" + fileNameWorldSave, saveJSON);
        Debug.Log("SaveManagerScript: Saved world");
    }
    public void SaveTodo()
    {
        string saveJSON = JsonUtility.ToJson(todoSave, true);
        System.IO.File.WriteAllText(Application.persistentDataPath + "/" + fileNameTodoSave, saveJSON); 
        Debug.Log("SaveManagerScript: Saved todo");
    }
    public void SavePlayer()
    {
        string saveJSON = JsonUtility.ToJson(playerSave, true);
        System.IO.File.WriteAllText(Application.persistentDataPath + "/" + fileNamePlayerSave, saveJSON); 
        Debug.Log("SaveManagerScript: Saved player");
    }

    public void SaveAll()
    {
        SaveWorld();
        SaveTodo();
        SavePlayer();
    }

    public async void SaveAllAsync()
    {
        // First we capture JSON on main thread
        string worldJson = JsonUtility.ToJson(worldSave, true);
        string todoJson = JsonUtility.ToJson(todoSave, true);
        string playerJson = JsonUtility.ToJson(playerSave, true);
        // Then we run all save function in parallel
        await Task.WhenAll(
            Task.Run(() => System.IO.File.WriteAllText(worldSavePath, worldJson)),
            Task.Run(() => System.IO.File.WriteAllText(todoSavePath, todoJson)),
            Task.Run(() => System.IO.File.WriteAllText(playerSavePath, playerJson))
        );
        Debug.Log("SaveManagerScript: All files saved in parallel");
    }

    public void UpdateTodoList(List<TodoManagerScript.Todo> newTodoList)
    {
        todoSave.todoList = newTodoList;
        _ = SaveTodoAsync(); // Fire and forget safely
    }

    public void UpdateWorld(WorldSaveState world)
    {
        worldSave = world;
        _ = SaveWorldAsync();
    }

    public void UpdatePlayer(PlayerSaveState player)
    {
        playerSave = player;
        _ = SavePlayerAsync();
    }

    /// <summary>
    /// Automatically trigger save on application minimization.
    /// </summary>
    /// <param name="pauseStatus"></param>
    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus) SaveAllSynchronous();
    }

    /// <summary>
    /// Automatically save upon application quit.
    /// </summary>
    private void OnApplicationQuit()
    {
        SaveAllSynchronous();
    }

    private void SaveAllSynchronous()
    {
        System.IO.File.WriteAllText(worldSavePath, JsonUtility.ToJson(worldSave, true));
        System.IO.File.WriteAllText(todoSavePath, JsonUtility.ToJson(todoSave, true));
        System.IO.File.WriteAllText(playerSavePath, JsonUtility.ToJson(playerSave, true));
        Debug.Log("SaveManagerScript: Saved all synchronously (App pause/quit)");
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

        // Set paths
        todoSavePath = Application.persistentDataPath + "/" + fileNameTodoSave;
        worldSavePath = Application.persistentDataPath + "/" + fileNameWorldSave;
        playerSavePath = Application.persistentDataPath + "/" + fileNamePlayerSave;
    }

    void Start()
    {
        StartCoroutine(AutosavePlayerRoutine());
    }

    private IEnumerator AutosavePlayerRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(60);
            if (GameStateScript.Instance == null || PlayerScript.Instance == null) yield return null;
            if (GameStateScript.Instance.paused) yield return null;
            _ = SavePlayerAsync();
        }
    }
}
