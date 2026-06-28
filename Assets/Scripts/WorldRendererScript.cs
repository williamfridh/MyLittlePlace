using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;
using System.Collections.Generic;

public class WorldRendererScript : MonoBehaviour
{

    public static WorldRendererScript Instance { get; set; }

    // Tiles
    [Header("Tile Settings")]
    public Tilemap groundTilemap;
    public Tilemap waterTilemap;
    public TileBase forestTile;
    public TileBase meadowTile;
    public TileBase mountainTile;
    public TileBase desertTile;
    public TileBase waterTile;
    public TileBase gravelTile;

    [Header("Fine tuning")]
    [SerializeField] private float yOffset = 0.25f;

    [SerializeField] GameObject spawnedObjectsParent;

    [SerializeField] private List<GameObject> spawnablePrefabs;
    private Dictionary<string, GameObject> prefabLookupTable;

    void DrawWorld(WorldSaveState world, int startX, int stopX, int startY, int stopY)
    {
        for (int x = startX; x < stopX; x ++)
        {
            for (int y = startY; y < stopY; y ++)
            {
                WorldCell cell = world.GetCell(x, y);
                if (cell == null)
                {
                    Debug.LogError($"WorldRendererScript: Could not find cell ({x}, {y}). Skipping cell...");
                    continue;
                }
                // Draw tile (biome)
                switch (cell.biomeType)
                {
                    case BiomeType.Camp:
                        groundTilemap.SetTile(new Vector3Int(x, y, 0), mountainTile);
                        break;
                    case BiomeType.Meadow:
                        groundTilemap.SetTile(new Vector3Int(x, y, 0), meadowTile);
                        break;
                    case BiomeType.Forest:
                        groundTilemap.SetTile(new Vector3Int(x, y, 0), forestTile);
                        break;
                    case BiomeType.Mountain:
                        groundTilemap.SetTile(new Vector3Int(x, y, 0), mountainTile);
                        break;
                    case BiomeType.Desert:
                        groundTilemap.SetTile(new Vector3Int(x, y, 0), desertTile);
                        break;
                    case BiomeType.Water:
                        waterTilemap.SetTile(new Vector3Int(x, y, 0), waterTile);
                        break;
                    default:
                        Debug.LogError($"WorldRendererScript: Invalid biome type for cell ({x}, {y}). Tried using {cell.biomeType}");
                        break;
                }
                // Add object
                if (cell.objectID != null && cell.objectID != "")
                {
                    GameObject objectPrefab = GetPrefabByID(cell.objectID);
                    if (objectPrefab == null)
                    {
                        Debug.LogWarning($"WorldRendererScript: Cannot find prefab for {cell.objectID} {cell.objectID} for cell ({x}, {y})!");
                        continue;
                    }
                    Instantiate(objectPrefab, new Vector3(x, y + yOffset, 0), Quaternion.identity, spawnedObjectsParent.transform); // Note that we draw each object +0.5 on y axis.
                }
            }
        }
    }

    /// <summary>
    /// Helper function used for getting correct prefab based on provided
    /// prefab id. Note that each prefab must have a WorldObjectIdentity
    /// component attached to it.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public GameObject GetPrefabByID(string id)
    {
        if (prefabLookupTable.TryGetValue(id, out GameObject foundPrefab))
        {
            return foundPrefab;
        }   
        return null;
    }

    void Awake()
    {
        // Singleton pattern to ensure only one instance of WorldGeneratorScript exists
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        
        // Populate prefab lookup table
        prefabLookupTable = new Dictionary<string, GameObject>();
        foreach (GameObject prefab in spawnablePrefabs)
        {
            if (prefab.TryGetComponent(out WorldObjectIdentity id))
            {
                if (!prefabLookupTable.ContainsKey(id.UniqueObjectID))
                {
                    prefabLookupTable.Add(id.UniqueObjectID, prefab);
                }
            }
            else
            {
                Debug.LogWarning($"Prefab {prefab.name} is missing a WorldObjectIdentity component!");
            }
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        WorldSaveState world = SaveManagerScript.Instance.worldSave;
        DrawWorld(world, 0, world.width, 0, world.height);
    }
}

