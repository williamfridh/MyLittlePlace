using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;
using System.Collections.Generic;

public class WorldLoaderScript : MonoBehaviour
{

    public static WorldLoaderScript Instance { get; set; }

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

    [SerializeField] private List<GameObject> spawnablePrefabs;
    private Dictionary<string, GameObject> prefabLookupTable;

    /// <summary>
    /// Get the biome typa at target position. Used by functions in other
    /// classes/objects.
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    /// <exception cref="System.IndexOutOfRangeException"></exception>
    public BiomeType GetBiomeAtPosition(Vector3 position)
    {
        int x = Mathf.FloorToInt(position.x);
        int y = Mathf.FloorToInt(position.y);

        if (x >= 0 && x < width && y >= 0 && y < height)
        {
            WorldCell cell = world.GetCell(x, y);
            return cell.biomeType;
        }
        else
        {
            throw new System.IndexOutOfRangeException("Position out of bounds of the biome map.");
        }
    }

    private void SpawnRock(int x, int y, BiomeType biome)
    {
        if (biome == BiomeType.Water) return;
        float nutrition = nutrientMap[x, y];
        float elevation = elevationMap[x, y];
        // Lower nutrition means more rocks, higher nutrition means more plants
        float rockSpawnChance = ((1.0f - nutrition) * 0.02f + elevation * 0.02f) / 2.0f; // Normalize to 0-1 range
        if (biome == BiomeType.Mountain)
        {
            rockSpawnChance += 0.1f; // Mountains have more rocks
        }
        rockSpawnChance = Mathf.Clamp01(rockSpawnChance); // Ensure chance is between 0 and 1
        if (Random.Range(0f, 1f) < rockSpawnChance && CanFitSize(x, y, 1, 1))
        {
            Instantiate(rockPrefab, new Vector3(x + 0.5f, y, 0), Quaternion.identity, spawnedObjectsParent.transform);
            MarkOccupied(x, y, 1, 1);
        }
    }

    private void SpawnPlant(int x, int y, BiomeType biome)
    {
        float nutrition = nutrientMap[x, y];
        float elevation = elevationMap[x, y];
        float location_quality = (nutrition * 0.8f) + ((1.0f - elevation) * 0.2f); // Higher nutrition and lower elevation means better location for plants
        float spawnChange = Random.Range(0.0f, 0.5f + location_quality * 0.5f);
        //float spawnChance = Random.Range(0f, 1f) * (nutrition * 0.5f) * (1.0f - elevation) * 0.5f; // Higher nutrition and lower elevation means more plants
        switch (biome)
        {
            case BiomeType.Meadow:
                if (spawnChange < 0.2f) {
                    break;
                } else if (spawnChange >= 0.2f && spawnChange < 0.5f && CanFitSize(x, y, 1, 1))
                {
                    Instantiate(grassPrefab, new Vector3(x + 0.5f, y, 0), Quaternion.identity, spawnedObjectsParent.transform);
                    MarkOccupied(x, y, 1, 1);
                } else if (spawnChange > 0.5f && spawnChange < 0.8f && CanFitSize(x, y, 1, 2)) {
                    Instantiate(treeBirchPrefab, new Vector3(x + 0.5f, y, 0), Quaternion.identity, spawnedObjectsParent.transform);
                    MarkOccupied(x, y, 1, 2);
                } else if (spawnChange >= 0.8f && CanFitSize(x, y, 2, 2)) {
                    Instantiate(treeOakPrefab, new Vector3(x+1.0f, y, 0), Quaternion.identity, spawnedObjectsParent.transform);
                    MarkOccupied(x, y, 2, 2);
                } else {
                    break;
                }
                break;
            case BiomeType.Forest:
                if (spawnChange >= 0.5f && spawnChange < 0.8f && CanFitSize(x, y, 2, 1)) {
                    Instantiate(treePinePrefab, new Vector3(x+1.0f, y, 0), Quaternion.identity, spawnedObjectsParent.transform);
                    MarkOccupied(x, y, 2, 1);
                }  else if (spawnChange >= 0.8f && CanFitSize(x, y, 2, 1)) {
                    Instantiate(treeOakPrefab, new Vector3(x+1.0f, y, 0), Quaternion.identity, spawnedObjectsParent.transform);
                    MarkOccupied(x, y, 2, 1);
                } else {
                    break;
                }
                break;
            case BiomeType.Mountain:
                break;
            case BiomeType.Water:
                break;
            case BiomeType.Desert:
                break;
        }
    }

    void DrawWorld(int startX, int stopX, int startY, int stopY)
    {
        for (int x = startX; x < stopX; x ++)
        {
            for (int y = startY; y < stopY; y ++)
            {
                WorldCell cell = world.GetCell(x, y);
                switch (cell.biomeType)
                {
                    case BiomeType.Forest:
                        groundTilemap.SetTile(new Vector3Int(x, y, 0), forestTile);
                        break;
                    case BiomeType.Meadow:
                        groundTilemap.SetTile(new Vector3Int(x, y, 0), meadowTile);
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
                }
            }
        }
    }

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
        DrawWorld( 0, width, 0, height);
    }
}
