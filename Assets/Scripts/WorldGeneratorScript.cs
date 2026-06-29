using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;
using System.Collections.Generic;
using System;

public class WorldGeneratorScript : MonoBehaviour
{
    // Singleton
    public static WorldGeneratorScript Instance { get; set; }

    [Header("Status text objects")]
    [SerializeField] GameObject biomesCheck;
    [SerializeField] GameObject bordersCheck;
    [SerializeField] GameObject campCheck;
    [SerializeField] GameObject natureCheck;
    [SerializeField] GameObject terrainCheck;
    [SerializeField] GameObject finalCheck;

    [Header("Generation settings")]
    [SerializeField] int width = 300;
    [SerializeField] int height = 300;
    [SerializeField] float noise = 0.1f;
    [SerializeField] int campRadius = 3;

    [Header("Spawn amounts")]
    [SerializeField] int woodPileSpawnAmount = 50;
    [SerializeField, Range(0f, 1f)] float plantDensity = 0.25f;

    /// <summary>
    /// Used for converting numbers to descriptive buckets for easier
    /// biome selection and interpretation.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="scale"></param>
    private void CategorizeFloat(float value, out BiomeSelectionScale scale)
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

    public IEnumerator GenerateWorldRoutine(Action<WorldSaveState> onComplete)
    {
        // Initilize a new world.
        WorldSaveState newWorld = new WorldSaveState(width, height, noise, campRadius);

        // Add biomes
        yield return AddBiomes(newWorld);
        // Add map borders
        yield return AddMapBorders(newWorld);
        // Add camp
        yield return AddCamp(newWorld);
        // Add nature
        yield return AddNature(newWorld);
        // Add terrain
        yield return AddTerrain(newWorld);

        // Set spawn location
        newWorld.spawnX =(width / 2) - 0.5f;
        newWorld.spawnY = (height / 2) + 1.0f;
        SaveManagerScript.Instance.playerSave.x_pos = newWorld.spawnX;
        SaveManagerScript.Instance.playerSave.y_pos = newWorld.spawnY;
        SaveManagerScript.Instance.SavePlayer();

        // Pass data to caller
        onComplete?.Invoke(newWorld);
    }

    public IEnumerator AddBiomes(WorldSaveState world)
    {
        // Loop through the tiles (x and y) coordinates
        // and assign temperature, moisture, elevation,
        // and nutrition, then biome type.
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {

                WorldCell cell = new WorldCell();
                world.cellGrid.Add(cell);

                // Get temperature, moisture, elevation, and nutrition
                float temperature = Mathf.PerlinNoise(
                    (x + world.temperatureOffsetX) * world.noiseScale,
                    (y + world.temperatureOffsetY) * world.noiseScale
                    );
                float moisture = Mathf.PerlinNoise(
                    (x + world.moistureOffsetX) * world.noiseScale,
                    (y + world.moistureOffsetY) * world.noiseScale
                    );
                float elevation = Mathf.PerlinNoise(
                    (x + world.elevationOffsetX) * world.noiseScale,
                    (y + world.elevationOffsetY) * world.noiseScale
                    );
                float nutrition = Mathf.PerlinNoise(
                    (x + world.nutrientOffsetX) * world.noiseScale,
                    (y + world.nutrientOffsetY) * world.noiseScale
                    );

                // Convert temperature and moisture to biome selection scale.
                CategorizeFloat(temperature, out BiomeSelectionScale tempScale);
                CategorizeFloat(moisture, out BiomeSelectionScale moistScale);

                cell.temperature     = tempScale;
                cell.moisture        = moistScale;
                cell.elevation       = elevation;
                cell.nutrition       = nutrition;

                // Select biome type depending on temperature and moisture.
                if (tempScale == BiomeSelectionScale.Low && moistScale == BiomeSelectionScale.Low)
                {
                    cell.biomeType = BiomeType.Mountain;
                }
                else if (tempScale == BiomeSelectionScale.Low && moistScale == BiomeSelectionScale.Medium)
                {
                    cell.biomeType = BiomeType.Forest;
                }
                else if (tempScale == BiomeSelectionScale.Low && moistScale == BiomeSelectionScale.High)
                {
                    cell.biomeType = BiomeType.Forest;
                }
                else if (tempScale == BiomeSelectionScale.Medium && moistScale == BiomeSelectionScale.Low)
                {
                    cell.biomeType = BiomeType.Desert;
                }
                else if (tempScale == BiomeSelectionScale.Medium && moistScale == BiomeSelectionScale.Medium)
                {
                    cell.biomeType = BiomeType.Meadow;
                }
                else if (tempScale == BiomeSelectionScale.Medium && moistScale == BiomeSelectionScale.High)
                {
                    cell.biomeType = BiomeType.Forest;
                }
                else if (tempScale == BiomeSelectionScale.High && moistScale == BiomeSelectionScale.Low)
                {
                    cell.biomeType = BiomeType.Desert;
                }
                else if (tempScale == BiomeSelectionScale.High && moistScale == BiomeSelectionScale.Medium)
                {
                    cell.biomeType = BiomeType.Water;
                }
                else
                {
                    cell.biomeType = BiomeType.Water; // Default biome
                }
            }
            // We yield every 10th column to prevent lowered frame rate
            if (x % 10 == 0) 
            {
                yield return null; 
            }
        }
        Debug.Log("WorldGeneratorScript: Generated biomes");
        biomesCheck.SetActive(true);
        yield return null;
    }

    public IEnumerator AddMapBorders(WorldSaveState world)
    {
        // Add map rock border.
        // Top and bottom borders
        for (int x = 0; x < width; x++)
        {
            WorldCell topCell = world.GetCell(x, 0);
            WorldCell bottomCell = world.GetCell(x, height-1);
            if (topCell.biomeType != BiomeType.Water) topCell.AssignObject("rock_1x1_rand");
            if (bottomCell.biomeType != BiomeType.Water) bottomCell.AssignObject("rock_1x1_rand");
        }
        // Left and right borders
        for (int y = 1; y < height - 1; y++)
        {
            WorldCell leftCell = world.GetCell(0, y);
            WorldCell rightCell = world.GetCell(width-1, y);
            if (leftCell.biomeType != BiomeType.Water) leftCell.AssignObject("rock_1x1_rand");
            if (rightCell.biomeType != BiomeType.Water) rightCell.AssignObject("rock_1x1_rand");
        }
        Debug.Log("WorldGeneratorScript: Added map borders");
        bordersCheck.SetActive(true);
        yield return null;
    }

    public IEnumerator AddCamp(WorldSaveState world)
    {
        // Create camp
        int x_center = width / 2;
        int y_center = height / 2;
        List<Vector2> campCoordinates = world.GetCircleCoordinates(x_center, y_center, campRadius);
        // Create biome
        foreach (Vector2 coordinates in campCoordinates)
        {
            int cx = (int)coordinates.x;
            int cy = (int)coordinates.y;
            WorldCell cell = world.GetCell(cx, cy);
            cell.biomeType = BiomeType.Camp;
            cell.occupied = true;
        }
        // Add campfire
        WorldCell campFireCell = world.GetCell(x_center, y_center);
        campFireCell.AssignObject("campfire", true);
        Debug.Log("WorldGeneratorScript: Added camp");
        campCheck.SetActive(true);
        yield return null;
    }

    public IEnumerator AddTerrain(WorldSaveState world)
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                WorldCell cell = world.GetCell(x, y);
                if (cell.biomeType == BiomeType.Water) continue;
                float nutrition = cell.nutrition;
                float elevation = cell.elevation;

                // Lower nutrition means more rocks, higher nutrition means more plants
                float rockSpawnChance = ((1.0f - nutrition) * 0.02f + elevation * 0.02f) / 2.0f; // Normalize to 0-1 range
                if (cell.biomeType == BiomeType.Mountain)
                {
                    rockSpawnChance += 0.1f; // Mountains have more rocks
                }
                rockSpawnChance = Mathf.Clamp01(rockSpawnChance); // Ensure chance is between 0 and 1
                if (UnityEngine.Random.Range(0f, 1f) < rockSpawnChance && world.CanFitSize(x, y, 1, 1))
                {
                    
                    cell.AssignObject("rock_1x1_rand");
                    world.MarkAsOccupied(x, y, 1, 1);
                }
            }
            // We yield every 10th column to prevent lowered frame rate
            if (x % 10 == 0) 
            {
                yield return null; 
            }
        }
        Debug.Log("WorldGeneratorScript: Added terrain");
        terrainCheck.SetActive(true);
        yield return null;
    }

    public IEnumerator AddNature(WorldSaveState world)
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                WorldCell cell = world.GetCell(x, y);

                // Avoid occupied and creating too dense nature
                if (cell.occupied) continue;
                if (UnityEngine.Random.Range(0f, 1f) > plantDensity) continue;

                float nutrition = cell.nutrition;
                float elevation = cell.elevation;

                float location_quality = (nutrition * 0.8f) + ((1.0f - elevation) * 0.2f); // Higher nutrition and lower elevation means better location for plants
                location_quality /= 0.8f;
                float spawnChange = UnityEngine.Random.Range(0.0f, 0.5f + location_quality * 0.5f);

                switch (cell.biomeType)
                {
                    case BiomeType.Meadow:
                        if (spawnChange < 0.2f) {
                            break;
                        } else if (spawnChange >= 0.2f && spawnChange < 0.45f && world.CanFitSize(x, y, 1, 1))
                        {
                            cell.AssignObject("bush_0");
                            world.MarkAsOccupied(x, y, 1, 1);
                        } else if (spawnChange >= 0.45f && spawnChange < 0.5f && world.CanFitSize(x, y, 1, 1))
                        {
                            cell.AssignObject("raspberry_bush_0");
                            world.MarkAsOccupied(x, y, 1, 1);
                        } else if (spawnChange > 0.5f && spawnChange < 0.8f && world.CanFitSize(x, y, 1, 2)) {
                            cell.AssignObject("tree_birch");
                            world.MarkAsOccupied(x, y, 1, 2);
                        } else if (spawnChange >= 0.8f && world.CanFitSize(x, y, 2, 2)) {
                            cell.AssignObject("tree_oak");
                            world.MarkAsOccupied(x, y, 2, 2);
                        } else {
                            // ...
                        }
                        break;
                    case BiomeType.Forest:
                        if (spawnChange < 0.2f) {
                            break;
                        } else if (spawnChange >= 0.35f && spawnChange < 0.40f && world.CanFitSize(x, y, 1, 1))
                        {
                            cell.AssignObject("thorn_bush_0");
                            world.MarkAsOccupied(x, y, 1, 1);
                        } else if (spawnChange >= 0.40f && spawnChange < 0.45f && world.CanFitSize(x, y, 1, 1))
                        {
                            cell.AssignObject("blueberry_bush_0");
                            world.MarkAsOccupied(x, y, 1, 1);
                        } else if (spawnChange >= 0.45f && spawnChange < 0.5f && world.CanFitSize(x, y, 1, 1))
                        {
                            cell.AssignObject("blueberry_bush_0");
                            world.MarkAsOccupied(x, y, 1, 1);
                        } else if (spawnChange >= 0.5f && spawnChange < 0.8f && world.CanFitSize(x, y, 2, 1)) {
                            cell.AssignObject("tree_pine");
                            world.MarkAsOccupied(x, y, 2, 1);
                        }  else if (spawnChange >= 0.8f && world.CanFitSize(x, y, 2, 2)) {
                            cell.AssignObject("tree_oak");
                            world.MarkAsOccupied(x, y, 2, 2);
                        } else {
                            // ...
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
            // We yield every 10th column to prevent lowered frame rate
            if (x % 10 == 0) 
            {
                yield return null; 
            }
        }

        // Add wood piles
        SpawnerScript spawner = SpawnerScript.Instance;
        int amount = woodPileSpawnAmount;
        int safetyLimit = 100000000;
        while (amount > 0 && safetyLimit > 0)
        {
            safetyLimit--;
            if (spawner.SpawnWoodPile(false, world)) amount--;
        }
        if (safetyLimit <= 0) Debug.Log("WorldGeneratorScript: Limit reached for spawning wood piles.");


        Debug.Log("WorldGeneratorScript: Added nature");
        natureCheck.SetActive(true);
        yield return null;
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // Optional: DontDestroyOnLoad(gameObject); if you want it to persist across scenes
        }
        else
        {
            Destroy(gameObject);
        }

        // Hide texts
        if (biomesCheck) biomesCheck.SetActive(false);
        if (bordersCheck) bordersCheck.SetActive(false);
        if (campCheck) campCheck.SetActive(false);
        if (natureCheck) natureCheck.SetActive(false);
        if (finalCheck) finalCheck.SetActive(false);
        if (terrainCheck) terrainCheck.SetActive(false);
    }

    void Start()
    {
        if (SaveManagerScript.Instance)
        {
            SaveManagerScript.Instance.Load(true, true, true);
        }
        else
        {
            Debug.LogError("WorldGeneratorScript: Could not access SaveManagerScript instance");
        }
    }
}
