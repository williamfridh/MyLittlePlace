using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.SceneManagement;

public class SpawnerScript : MonoBehaviour
{

    public static SpawnerScript Instance { get; private set; }

    [System.Serializable]
    private class ObjectRate
    {
        public BiomeType biome;
        [Range(0f, 1f)] public float rate;
    }

    [Header("Wood Pile")]
    [SerializeField] List<ObjectRate> spawnWoodPileRate;
    [SerializeField] GameObject woodPilePrefab;
    [SerializeField] float woodPileSpawnTimer = 60f;

    [Header("General")]
    [SerializeField] GameObject spawnedObjectsParent;

    private void Awake()
    {
        // Enforce Singleton Pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Subscribe to scene loading events
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        StartCoroutine(SpawnWoodPileRoutine());
    }

    private void OnDestroy()
    {
        // Unsubscribe form event to prveent memoery leaks
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Main Menu") Destroy(gameObject);
    }

    public bool SpawnWoodPile(bool instantiate, WorldSaveState? world)
    {
        if (spawnWoodPileRate.Count == 0)
        {
            Debug.LogWarning("SpawnerScript: No spawnWoodPileRate list. Cannot execute SpawnWoodPile()!");
            return false;
        }
        
        WorldSaveState targetWorld = world != null ? world : SaveManagerScript.Instance.worldSave;

        foreach (ObjectRate obj in spawnWoodPileRate)
        {
            if (obj.rate >= UnityEngine.Random.Range(0f, 1f))
            {
                Vector2Int? spawnPos = targetWorld.GetRandomPosition(obj.biome);
                if (!spawnPos.HasValue) continue;
                Vector2Int finalPos = spawnPos.Value;
                if (instantiate)
                {
                    Instantiate(
                        woodPilePrefab,
                        new Vector3(
                            finalPos.x,
                            finalPos.y + WorldRendererScript.Instance.yOffset,
                            0
                            ),
                        Quaternion.identity,
                        spawnedObjectsParent.transform
                        );
                }
                // Adjust save
                targetWorld.AssignObjectToPosition(
                    finalPos.x,
                    finalPos.y,
                    "wood_pile_0"
                    );
                return true;
            }
        }
        return false;
    }

    public IEnumerator SpawnWoodPileRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(woodPileSpawnTimer);
            if (GameStateScript.Instance && !GameStateScript.Instance.paused) SpawnWoodPile(true, null);
            SpawnWoodPileRoutine();
        }
    }
}
