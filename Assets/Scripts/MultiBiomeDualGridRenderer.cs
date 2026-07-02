using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;
using System.Collections.Generic;
using NavMeshPlus.Components;

public class MultiBiomeDualGridRenderer : MonoBehaviour
{

    public static MultiBiomeDualGridRenderer Instance { get; set; }

    [System.Serializable]
    public struct BiomeTileset
    {
        public BiomeType biomeType;
        public TileBase[] sixteenTileSet;
    }
    
    [System.Serializable]
    private class BiomeGroundTextureCollection
    {
        [SerializeField] public BiomeType biome;
        [SerializeField] public List<TileBase> textureList;
    }

    [SerializeField] private List<BiomeTileset> tilesetDatabase;
    private Dictionary<BiomeType, TileBase[]> biomeLookups;

    [SerializeField] private NavMeshSurface navMeshSurface2DSmall;
    [SerializeField] private NavMeshSurface navMeshSurface2DMedium;
    [SerializeField] private NavMeshSurface navMeshSurface2DLarge;

    [Header("Tilemaps")]
    public Tilemap groundTilemap;
    public Tilemap groundTextureTilemap;
    public Tilemap[] transitionTilemaps;
    public Tilemap waterTilemap;

    [Header("Uniform tiles")]
    public TileBase forestTile;
    public TileBase meadowTile;
    public TileBase mountainTile;
    public TileBase desertTile;
    public TileBase waterTile;
    public TileBase CampTile;

    [Header("Ground texture")]
    [SerializeField] private List<BiomeGroundTextureCollection> biomeGroundTextures;

    [Header("Fine tuning")]
    [SerializeField] private float yOffset = 0.25f;

    [SerializeField] GameObject spawnedObjectsParent;

    [SerializeField] private List<GameObject> spawnablePrefabs;
    private Dictionary<string, GameObject> prefabLookupTable;

    void DrawWorld(WorldSaveState world, int startX, int stopX, int startY, int stopY)
    {
    // Pass 1: Flat tilemap
    for (int x = startX; x < stopX; x++)
    {
        for (int y = startY; y < stopY; y++)
        {
            WorldCell cell = world.GetCell(x, y);
            if (cell == null) continue;
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
            // Add texture
            if (cell.textureID > -1)
            {
                BiomeGroundTextureCollection collection = biomeGroundTextures.Find(i => i.biome == cell.biomeType);
                groundTextureTilemap.SetTile(new Vector3Int(x, y, 0), collection.textureList[cell.textureID]);
            }
        }
    }

        // Pass 2: Using dual-grid overlap for tranistions
        for (int x = startX; x < stopX - 1; x ++)
        {
            for (int y = startY; y < stopY - 1; y ++)
            {

                WorldCell targetCell = world.GetCell(x, y);
                if (targetCell == null)
                {
                    Debug.LogError($"MultiBiomeDualGridRenderer: Could not find cell ({x}, {y}). Skipping cell...");
                    continue;
                }

                // Sample all 4 corners
                int tl = (int)world.GetCell(x, y + 1).biomeType;
                int tr = (int)world.GetCell(x + 1, y + 1).biomeType;
                int bl = (int)world.GetCell(x, y).biomeType;
                int br = (int)world.GetCell(x + 1, y).biomeType;

                // Find unique biomes
                List<int> presentBiomes = new List<int> {tl, tr, bl, br};
                presentBiomes.Sort();

                List<int> uniqueBiomes = new List<int>();
                foreach (int b in presentBiomes)
                {
                    if (!uniqueBiomes.Contains(b)) uniqueBiomes.Add(b);
                }

                // Iterate trough the unique biomes (but skipping 0 as pass 1 acts as base)
                int layerIndex = 0;
                for (int i = 1; i < uniqueBiomes.Count; i++)
                {
                    int currentDominance = uniqueBiomes[i];
                    BiomeType currentBiome = (BiomeType) currentDominance;

                    // Based on dominance, we calculate binary index mask
                    // This works because the tiles have a certain naming sceme
                    int bit0 = (tl >= currentDominance) ? 1 : 0; // Value: 8
                    int bit1 = (tr >= currentDominance) ? 1 : 0; // Value: 4
                    int bit2 = (bl >= currentDominance) ? 1 : 0; // Value: 2
                    int bit3 = (br >= currentDominance) ? 1 : 0; // Value: 1

                    int spriteIndex = (bit0 << 3) | (bit1 << 2) | (bit2 << 1) | bit3;

                    // If sprite index is zero, we fall back to the ground tilemap
                    if (spriteIndex == 0) continue;

                    // Get correct tile and place on tilemap
                    if (biomeLookups.TryGetValue(currentBiome, out TileBase[] currentTileset))
                    {
                        TileBase chosenTile = currentTileset[spriteIndex];
                        if (layerIndex < transitionTilemaps.Length)
                        {
                            transitionTilemaps[layerIndex].SetTile(new Vector3Int(x, y, 0), chosenTile);
                            layerIndex++;
                        }
                        else
                        {
                            Debug.LogWarning($"Not enough transition tilemaps to handle intersection at {x},{y}");
                        }
                    }
                }

                // Add object
                if (targetCell.objectID != null && targetCell.objectID != "")
                {
                    GameObject objectPrefab = GetPrefabByID(targetCell.objectID);
                    if (objectPrefab == null)
                    {
                        Debug.LogWarning($"MultiBiomeDualGridRenderer: Cannot find prefab for {targetCell.objectID} for cell ({x}, {y})!");
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

        // Populate the biome lookups table
        biomeLookups = new Dictionary<BiomeType, TileBase[]>();
        foreach (var set in tilesetDatabase)
        {
            biomeLookups[set.biomeType] = set.sixteenTileSet;
        }
        
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
        StartCoroutine(Initilize());
    }

    private IEnumerator Initilize()
    {
        // Draw world
        WorldSaveState world = SaveManagerScript.Instance.worldSave;
        DrawWorld(world, 0, world.width, 0, world.height);

        // Wait for the end of th frame
        yield return new WaitForEndOfFrame();

        // Build navigation mesh
        if (navMeshSurface2DSmall)
            yield return navMeshSurface2DSmall.BuildNavMeshAsync();

        if (navMeshSurface2DMedium)
            yield return navMeshSurface2DMedium.BuildNavMeshAsync();

        if (navMeshSurface2DLarge)
            yield return navMeshSurface2DLarge.BuildNavMeshAsync();

        // Spawn NPCs
        SpawnerScript.Instance.Initilize();
    }
}

