using UnityEngine;
using UnityEngine.Tilemaps;

public class WorldGeneratorScript : MonoBehaviour
{

    public static WorldGeneratorScript Instance { get; set; }

    // Tiles
    [Header("Tile Settings")]
    public Tilemap groundTilemap;
    public TileBase forestTile;
    public TileBase meadowTile;
    public TileBase mountainTile;
    public TileBase desertTile;
    public TileBase waterTile;

    // Objects to spawn
    [Header("Object Settings")]
    public GameObject treeBirchPrefab;
    public GameObject treeOakPrefab;
    public GameObject treePinePrefab;
    public GameObject cactusPrefab;
    public GameObject rockPrefab;
    public GameObject grassPrefab;

    // Map generation parameters
    [Header("Generation Parameters")]
    public float noiseScale = 0.08f;
    private float temperatureOffsetX;
    private float temperatureOffsetY;
    private float moistureOffsetX;
    private float moistureOffsetY;
    public int width = 100;
    public int height = 100;
    public GameObject spawnedObjectsParent;

    // Preset types
    public enum BiomeType
    {
        Forest,
        Meadow,
        Mountain,
        Desert,
        Water
    }
    private enum BiomeSelectionScale
    {
        Low,
        Medium,
        High
    }

    BiomeType[,] biomeMap;
    bool[,] occupiedMap;

    public BiomeType GetBiomeAtPosition(Vector3 position)
    {
        int x = Mathf.FloorToInt(position.x);
        int y = Mathf.FloorToInt(position.y);

        if (x >= 0 && x < width && y >= 0 && y < height)
        {
            return biomeMap[x, y];
        }
        else
        {
            throw new System.IndexOutOfRangeException("Position out of bounds of the biome map.");
        }
    }

    private void valueToScale(float value, out BiomeSelectionScale scale)
    {
        if (value < 0.33f)
        {
            scale = BiomeSelectionScale.Low;
        }
        else if (value < 0.66f)
        {
            scale = BiomeSelectionScale.Medium;
        }
        else
        {
            scale = BiomeSelectionScale.High;
        }
    }

    private bool canFitSize(int x, int y, int sizeX, int sizeY)
    {
        for (int i = 0; i < sizeX; i++) {
            for (int j = 0; j < sizeY; j++) {
                if (x + i >= width || y + j >= height || occupiedMap[x + i, y + j]) {
                    return false; // Out of bounds or tile is occupied
                }
            }
        }
        return true; // All tiles are within bounds and not occupied
    }

    private void markOccupied(int x, int y, int sizeX, int sizeY)
    {
        for (int i = 0; i < sizeX; i++) {
            for (int j = 0; j < sizeY; j++) {
                if (x + i < width && y + j < height) {
                    occupiedMap[x + i, y + j] = true; // Mark tile as occupied
                }
            }
        }
    }

    private void SpawnObject(int x, int y, BiomeType biome)
    {
        // As the SpawnObject is typically called column then row wise,
        // we can check the occupation of right and lower tile to determine
        // the possible spawn size.

        float spawnChance = Random.Range(0f, 1f);
        switch (biome)
        {
            case BiomeType.Meadow:
                if (spawnChance > 0 && spawnChance < 0.01f && canFitSize(x, y, 2, 2))
                {
                    Instantiate(treeOakPrefab, new Vector3(x + 1.0f, y + 1.0f, 0), Quaternion.identity, spawnedObjectsParent.transform);
                    markOccupied(x, y, 2, 1);
                }
                else if (spawnChance > 0.01f && spawnChance < 0.03f && canFitSize(x, y, 1, 1))
                {
                    Instantiate(rockPrefab, new Vector3(x + 0.5f, y + 0.5f, 0), Quaternion.identity, spawnedObjectsParent.transform);
                    markOccupied(x, y, 1, 1);
                }
                else if (spawnChance > 0.03f && spawnChance < 0.05f && canFitSize(x, y, 1, 2))
                {
                    Instantiate(treeBirchPrefab, new Vector3(x + 0.5f, y + 1.0f, 0), Quaternion.identity, spawnedObjectsParent.transform);
                    markOccupied(x, y, 1, 2);
                }
                else if (spawnChance > 0.05f && spawnChance < 0.5f && canFitSize(x, y, 1, 1))
                {
                    Instantiate(grassPrefab, new Vector3(x + 0.5f, y + 0.5f, 0), Quaternion.identity, spawnedObjectsParent.transform);
                    markOccupied(x, y, 1, 1);
                }
                break;
            case BiomeType.Forest:
                if (spawnChance > 0 && spawnChance < 0.03f && canFitSize(x, y, 1, 1))
                {
                    Instantiate(rockPrefab, new Vector3(x + 0.5f, y + 0.5f, 0), Quaternion.identity, spawnedObjectsParent.transform);
                    markOccupied(x, y, 1, 1);
                }
                else if (spawnChance > 0.03f && spawnChance < 0.15f && canFitSize(x, y, 2, 2))
                {
                    Instantiate(treePinePrefab, new Vector3(x + 1.0f, y + 1.0f, 0), Quaternion.identity, spawnedObjectsParent.transform);
                    markOccupied(x, y, 2, 1);
                }
                else if (spawnChance > 0.15f && spawnChance < 0.3f && canFitSize(x, y, 1, 1))
                {
                    Instantiate(grassPrefab, new Vector3(x + 0.5f, y + 0.5f, 0), Quaternion.identity, spawnedObjectsParent.transform);
                    markOccupied(x, y, 1, 1);
                }
                break;
            case BiomeType.Mountain:
                if (spawnChance > 0 && spawnChance < 0.10f && canFitSize(x, y, 1, 1))
                {
                    Instantiate(rockPrefab, new Vector3(x + 0.5f, y + 0.5f, 0), Quaternion.identity, spawnedObjectsParent.transform);
                    markOccupied(x, y, 1, 1);
                }
                else if (spawnChance > 0.10f && spawnChance < 0.02f && canFitSize(x, y, 1, 1))
                {
                    Instantiate(grassPrefab, new Vector3(x + 0.5f, y + 0.5f, 0), Quaternion.identity, spawnedObjectsParent.transform);
                    markOccupied(x, y, 1, 1);
                }
                break;
        }
    }

    void Awake()
    {
        // Singleton pattern to ensure only one instance of WorldGeneratorScript exists
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        biomeMap = new BiomeType[width, height];
        // Initilize occupied map to track which tiles are populated
        // and set the default value to false (not occupied)
        occupiedMap = new bool[width, height];
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                occupiedMap[x, y] = false;
            }
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        temperatureOffsetX = Random.Range(0f, 10000f);
        temperatureOffsetY = Random.Range(0f, 10000f);
        moistureOffsetX = Random.Range(0f, 10000f);
        moistureOffsetY = Random.Range(0f, 10000f);
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // Get random value using Perline Noise
                float temperature = Mathf.PerlinNoise((x + temperatureOffsetX) * noiseScale, (y + temperatureOffsetY) * noiseScale);
                float moisture = Mathf.PerlinNoise((x + moistureOffsetX) * noiseScale, (y + moistureOffsetY) * noiseScale);
                BiomeType t = biomeMap[x, y];
                // Convert temperature and moisture to biome selection scale
                valueToScale(temperature, out BiomeSelectionScale tempScale);
                valueToScale(moisture, out BiomeSelectionScale moistScale);
                // Select biome type
                // Note that lower then 0.33 is low, 0.33 to 0.66 is medium and higher then 0.66 is high
                BiomeType biome;
                if (tempScale == BiomeSelectionScale.Low && moistScale == BiomeSelectionScale.Low)
                {
                    biome = BiomeType.Mountain;
                }
                else if (tempScale == BiomeSelectionScale.Low && moistScale == BiomeSelectionScale.Medium)
                {
                    biome = BiomeType.Forest;
                }
                else if (tempScale == BiomeSelectionScale.Low && moistScale == BiomeSelectionScale.High)
                {
                    biome = BiomeType.Forest;
                }
                else if (tempScale == BiomeSelectionScale.Medium && moistScale == BiomeSelectionScale.Low)
                {
                    biome = BiomeType.Desert;
                }
                else if (tempScale == BiomeSelectionScale.Medium && moistScale == BiomeSelectionScale.Medium)
                {
                    biome = BiomeType.Meadow;
                }
                else if (tempScale == BiomeSelectionScale.Medium && moistScale == BiomeSelectionScale.High)
                {
                    biome = BiomeType.Forest;
                }
                else if (tempScale == BiomeSelectionScale.High && moistScale == BiomeSelectionScale.Low)
                {
                    biome = BiomeType.Desert;
                }
                else if (tempScale == BiomeSelectionScale.High && moistScale == BiomeSelectionScale.Medium)
                {
                    biome = BiomeType.Water;
                }
                else
                {
                    biome = BiomeType.Water; // Default biome
                }
                // Save biome in map
                biomeMap[x, y] = biome;
                // Set tile
                switch (biome)
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
                        groundTilemap.SetTile(new Vector3Int(x, y, 0), waterTile);
                        break;
                }
                // Spawn objects based on biome
                SpawnObject(x, y, biome);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
