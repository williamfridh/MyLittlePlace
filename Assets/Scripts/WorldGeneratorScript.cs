using UnityEngine;
using UnityEngine.Tilemaps;

public class WorldGeneratorScript : MonoBehaviour
{
    // Tiles
    public Tilemap groundTilemap;
    public TileBase forestTile;
    public TileBase meadowTile;
    public TileBase mountainTile;
    public TileBase desertTile;
    public TileBase waterTile;

    // Objects to spawn
    public GameObject treeBirchPrefab;
    public GameObject treeOakPrefab;
    public GameObject treePinePrefab;
    public GameObject cactusPrefab;
    public GameObject rockPrefab;
    public GameObject grassPrefab;

    // Map generation parameters
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

    private void SpawnObject(int x, int y, BiomeType biome)
    {
        // As the SpawnObject is typically called column then row wise,
        // we can check the occupation of right and lower tile to determine
        // the possible spawn size. Checks up too 3 on box axes, starting
        // with column (x) and then row (y).
        int max_x = 0;
        int max_y = 0;
        //if ()

        float spawnChance = Random.Range(0f, 1f);
        switch (biome)
        {
            case BiomeType.Meadow:
                if (spawnChance > 0 && spawnChance < 0.01f)
                {
                    Instantiate(treeOakPrefab, new Vector3(x + 0.5f, y + 0.5f, 0), Quaternion.identity, spawnedObjectsParent.transform);
                }
                else if (spawnChance > 0.01f && spawnChance < 0.03f)
                {
                    Instantiate(rockPrefab, new Vector3(x + 0.5f, y + 0.5f, 0), Quaternion.identity, spawnedObjectsParent.transform);
                }
                else if (spawnChance > 0.03f && spawnChance < 0.05f)
                {
                    Instantiate(treeBirchPrefab, new Vector3(x + 0.5f, y + 0.5f, 0), Quaternion.identity, spawnedObjectsParent.transform);
                }
                else if (spawnChance > 0.05f && spawnChance < 0.5f)
                {
                    Instantiate(grassPrefab, new Vector3(x + 0.5f, y + 0.5f, 0), Quaternion.identity, spawnedObjectsParent.transform);
                }
                break;
            case BiomeType.Forest:
                if (spawnChance > 0 && spawnChance < 0.03f)
                {
                    Instantiate(rockPrefab, new Vector3(x + 0.5f, y + 0.5f, 0), Quaternion.identity, spawnedObjectsParent.transform);
                }
                else if (spawnChance > 0.03f && spawnChance < 0.15f)
                {
                    Instantiate(treePinePrefab, new Vector3(x + 0.5f, y + 0.5f, 0), Quaternion.identity, spawnedObjectsParent.transform);
                }
                else if (spawnChance > 0.15f && spawnChance < 0.3f)
                {
                    Instantiate(grassPrefab, new Vector3(x + 0.5f, y + 0.5f, 0), Quaternion.identity, spawnedObjectsParent.transform);
                }
                break;
            case BiomeType.Mountain:
                if (spawnChance > 0 && spawnChance < 0.10f)
                {
                    Instantiate(rockPrefab, new Vector3(x + 0.5f, y + 0.5f, 0), Quaternion.identity, spawnedObjectsParent.transform);
                }
                else if (spawnChance > 0.10f && spawnChance < 0.02f)
                {
                    Instantiate(grassPrefab, new Vector3(x + 0.5f, y + 0.5f, 0), Quaternion.identity, spawnedObjectsParent.transform);
                }
                break;
        }
    }

    void Awake()
    {
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
