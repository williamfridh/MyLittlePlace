using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;
using System.Collections.Generic;

public class WorldGeneratorScript : MonoBehaviour
{

    public static WorldGeneratorScript Instance { get; set; }

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

    // Nature to spawn
    [Header("Nature Settings")]
    public GameObject treeBirchPrefab;
    public GameObject treeOakPrefab;
    public GameObject treePinePrefab;
    public GameObject cactusPrefab;
    public GameObject rockPrefab;
    public GameObject bushPrefab;
    public GameObject grassPrefab;

    // Objects to spawn
    [Header("Objects Settings")]
    public GameObject campfirePrefab;

    // Map generation parameters
    [Header("Generation Parameters")]
    public float noiseScale = 0.08f;
    private float temperatureOffsetX;
    private float temperatureOffsetY;
    private float moistureOffsetX;
    private float moistureOffsetY;
    private float elevationOffsetX;
    private float elevationOffsetY;
    private float nutrientOffsetX;
    private float nutrientOffsetY;
    public int width = 100;
    public int height = 100;
    public GameObject spawnedObjectsParent;

    [Header("Starting Camp")]
    [SerializeField] private int startingCampRadius = 3;

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
    float[,] temperatureMap;
    float[,] moistureMap;
    float[,] elevationMap;
    float[,] nutrientMap;

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

    private void AddRockMapBorder()
    {
        // Top and bottom borders
        for (int x = 0; x < width; x++)
        {
            if (biomeMap[x, 0] != BiomeType.Water)
            {
                Instantiate(rockPrefab, new Vector3(x + 0.5f, 0, 0), Quaternion.identity, spawnedObjectsParent.transform);
                MarkOccupied(x, 0, 1, 1);
            }

            if (biomeMap[x, height-1] != BiomeType.Water)
            {
                Instantiate(rockPrefab, new Vector3(x + 0.5f, height-1, 0), Quaternion.identity, spawnedObjectsParent.transform);
                MarkOccupied(x, height-1, 1, 1);
            }
        }
        // Left and right borders
        for (int y = 1; y < height - 1; y++)
        {
            if (biomeMap[0, y] != BiomeType.Water)
            {
                Instantiate(rockPrefab, new Vector3(0.5f, y, 0), Quaternion.identity, spawnedObjectsParent.transform);
                MarkOccupied(0, y, 1, 1);
            }

            if (biomeMap[width-1, y] != BiomeType.Water)
            {
                Instantiate(rockPrefab, new Vector3(width - 1.0f + 0.5f, y, 0), Quaternion.identity, spawnedObjectsParent.transform);
                MarkOccupied(width-1, y, 1, 1);
            }
        }
    }

    private bool CanFitSize(int x, int y, int sizeX, int sizeY)
    {
        for (int i = 0; i < sizeX; i++) {
            for (int j = 0; j < sizeY; j++) {
                // Catch index out of bounds
                if (x + i >= width || y + j >= height) return false;
                // Check if if specific offset tile is water
                if (biomeMap[x + i, y + j] == BiomeType.Water) return false;
                // Check if tile is occupied
                if (occupiedMap[x + i, y + j]) return false;
            }
        }
        return true; // All tiles are within bounds and not occupied
    }

    private void MarkOccupied(int x, int y, int sizeX, int sizeY)
    {
        for (int i = 0; i < sizeX; i++) {
            for (int j = 0; j < sizeY; j++) {
                if (x + i < width && y + j < height) {
                    occupiedMap[x + i, y + j] = true; // Mark tile as occupied
                }
            }
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

    void Awake()
    {
        // Singleton pattern to ensure only one instance of WorldGeneratorScript exists
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // Preapre offsets
        temperatureOffsetX = Random.Range(0f, 10000f);
        temperatureOffsetY = Random.Range(0f, 10000f);
        moistureOffsetX = Random.Range(0f, 10000f);
        moistureOffsetY = Random.Range(0f, 10000f);
        elevationOffsetX = Random.Range(0f, 10000f);
        elevationOffsetY = Random.Range(0f, 10000f);
        nutrientOffsetX = Random.Range(0f, 10000f);
        nutrientOffsetY = Random.Range(0f, 10000f);

        biomeMap = new BiomeType[width, height];
        // Initilize occupied map to track which tiles are populated
        // and set the default value to false (not occupied)
        occupiedMap = new bool[width, height];
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                occupiedMap[x, y] = false;
            }
        }
        // Then initilize the maps for temperature, moisture, elevation and nutrition
        temperatureMap = new float[width, height];
        moistureMap = new float[width, height];
        elevationMap = new float[width, height];
        nutrientMap = new float[width, height];

        // Finally we populate the maps
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // Get temperature and moisture values from Perlin noise
                float temperature = Mathf.PerlinNoise((x + temperatureOffsetX) * noiseScale, (y + temperatureOffsetY) * noiseScale);
                float moisture = Mathf.PerlinNoise((x + moistureOffsetX) * noiseScale, (y + moistureOffsetY) * noiseScale);
                // Save values in maps for later use (e.g., for spawning objects based on biome conditions)
                temperatureMap[x, y] = temperature;
                moistureMap[x, y] = moisture;
                elevationMap[x, y] = Mathf.PerlinNoise((x + elevationOffsetX) * noiseScale, (y + elevationOffsetY) * noiseScale);
                nutrientMap[x, y] = Mathf.PerlinNoise((x + nutrientOffsetX) * noiseScale, (y + nutrientOffsetY) * noiseScale);

                // Target biome type based on temperature and moisture values
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
            }
        }
    }

    /// <summary>
    /// Used for fetching a list of coordinates shaping a circle. Intended to be
    /// reused, created for CreateCamp function.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="radius"></param>
    List<Vector2> GetCircleCoordinates(float x, float y, int radius)
    {
        List<Vector2> coordinateList = new List<Vector2>();
        
        int startX = Mathf.FloorToInt(x - radius);
        int endX = Mathf.FloorToInt(x + radius);
        int startY = Mathf.FloorToInt(y - radius);
        int endY = Mathf.FloorToInt(y + radius);
        
        for (float xx = startX; xx <= endX; xx++)
        {
            for (float yy = startY; yy <= endY; yy++)
            {
                float dx = (xx + 0.5f) - (x + 0.5f);
                float dy = (yy + 0.5f) - (y + 0.5f);
                if ((dx * dx) + (dy * dy) <= radius * radius)
                {
                    coordinateList.Add(new Vector2(xx, yy));
                }
            }
        }
        return coordinateList;
    }

    void CreateCamp(int radius = 3)
    {
        int x_center = width / 2 - 1;
        int y_center = height / 2 - 1;
        List<Vector2> campCoordinates = GetCircleCoordinates(x_center, y_center, radius);
        // Create biome
        foreach (Vector2 coordinates in campCoordinates)
        {
            int cx = (int)coordinates.x;
            int cy = (int)coordinates.y;
            groundTilemap.SetTile(new Vector3Int(cx, cy, 0), gravelTile);
            SetBiomeType(cx, cy, BiomeType.Meadow);
            MarkOccupied(cx, cy, 1, 1);
        }
        // Move player
        PlayerScript.Instance.transform.position = new Vector3(x_center-2, y_center, 0);
        // Add campfire
        Instantiate(campfirePrefab, new Vector3(x_center, y_center, 0), Quaternion.identity, spawnedObjectsParent.transform);
    }

    void SetBiomeType(int x, int y, BiomeType biomeType)
    {
        if (x == null || y == null || biomeType == null) return;
        biomeMap[x, y] = biomeType;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        AddRockMapBorder();
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                BiomeType biome = biomeMap[x, y];
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
                        waterTilemap.SetTile(new Vector3Int(x, y, 0), waterTile);
                        break;
                }
                // Spawn objects based on biome
                SpawnRock(x, y, biome);
                SpawnPlant(x, y, biome);
            }
        }
        CreateCamp(startingCampRadius);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
