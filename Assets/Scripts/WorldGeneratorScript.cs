using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;
using System.Collections.Generic;

public class WorldGeneratorScript : MonoBehaviour
{
    // Singleton
    public static WorldGeneratorScript Instance { get; set; }

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

    public WorldSaveState GenerateWorld(int width, int height, float noise, int campRadius)
    {
        // Initilize a new world.
        WorldSaveState newWorld = new WorldSaveState(width, height, noise, campRadius);

        // Loop through the tiles (x and y) coordinates
        // and assign temperature, moisture, elevation,
        // and nutrition, then biome type.
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {

                WorldCell cell = new WorldCell();
                newWorld.cellGrid.Add(cell);

                // Get temperature, moisture, elevation, and nutrition
                float temperature = Mathf.PerlinNoise(
                    (x + newWorld.temperatureOffsetX) * newWorld.noiseScale,
                    (y + newWorld.temperatureOffsetY) * newWorld.noiseScale
                    );
                float moisture = Mathf.PerlinNoise(
                    (x + newWorld.moistureOffsetX) * newWorld.noiseScale,
                    (y + newWorld.moistureOffsetY) * newWorld.noiseScale
                    );
                cell.elevation = Mathf.PerlinNoise(
                    (x + newWorld.elevationOffsetX) * newWorld.noiseScale,
                    (y + newWorld.elevationOffsetY) * newWorld.noiseScale
                    );
                cell.nutrition = Mathf.PerlinNoise(
                    (x + newWorld.nutrientOffsetX) * newWorld.noiseScale,
                    (y + newWorld.nutrientOffsetY) * newWorld.noiseScale
                    );

                // Convert temperature and moisture to biome selection scale.
                CategorizeFloat(temperature, out BiomeSelectionScale tempScale);
                CategorizeFloat(moisture, out BiomeSelectionScale moistScale);
                cell.temperature     = tempScale;
                cell.moisture        = moistScale;

                // Select biome type depending on temperature and moisture.
                BiomeType b = cell.biomeType;
                if (tempScale == BiomeSelectionScale.Low && moistScale == BiomeSelectionScale.Low)
                {
                    b = BiomeType.Mountain;
                }
                else if (tempScale == BiomeSelectionScale.Low && moistScale == BiomeSelectionScale.Medium)
                {
                    b = BiomeType.Forest;
                }
                else if (tempScale == BiomeSelectionScale.Low && moistScale == BiomeSelectionScale.High)
                {
                    b = BiomeType.Forest;
                }
                else if (tempScale == BiomeSelectionScale.Medium && moistScale == BiomeSelectionScale.Low)
                {
                    b = BiomeType.Desert;
                }
                else if (tempScale == BiomeSelectionScale.Medium && moistScale == BiomeSelectionScale.Medium)
                {
                    b = BiomeType.Meadow;
                }
                else if (tempScale == BiomeSelectionScale.Medium && moistScale == BiomeSelectionScale.High)
                {
                    b = BiomeType.Forest;
                }
                else if (tempScale == BiomeSelectionScale.High && moistScale == BiomeSelectionScale.Low)
                {
                    b = BiomeType.Desert;
                }
                else if (tempScale == BiomeSelectionScale.High && moistScale == BiomeSelectionScale.Medium)
                {
                    b = BiomeType.Water;
                }
                else
                {
                    b = BiomeType.Water; // Default biome
                }
            }
        }
        Debug.Log("WorldGeneratorScript: Generated biomes");
        
        // Add map rock border.
        // Top and bottom borders
        for (int x = 0; x < width; x++)
        {
            WorldCell topCell = newWorld.GetCell(x, 0);
            WorldCell bottomCell = newWorld.GetCell(x, height-1);
            if (topCell.biomeType != BiomeType.Water) topCell.AssignObject("rock_1x1_0");
            if (bottomCell.biomeType != BiomeType.Water) bottomCell.AssignObject("rock_1x1_0");
        }
        // Left and right borders
        for (int y = 1; y < height - 1; y++)
        {
            WorldCell leftCell = newWorld.GetCell(0, y);
            WorldCell rightCell = newWorld.GetCell(width-1, y);
            if (leftCell.biomeType != BiomeType.Water) leftCell.AssignObject("rock_1x1_0");
            if (rightCell.biomeType != BiomeType.Water) rightCell.AssignObject("rock_1x1_0");
        }
        Debug.Log("WorldGeneratorScript: Added map borders");

        // Create camp
        int x_center = width / 2 - 1;
        int y_center = height / 2 - 1;
        List<Vector2> campCoordinates = newWorld.GetCircleCoordinates(x_center, y_center, campRadius);
        // Create biome
        foreach (Vector2 coordinates in campCoordinates)
        {
            int cx = (int)coordinates.x;
            int cy = (int)coordinates.y;
            WorldCell cell = newWorld.GetCell(cx, cy);
            cell.biomeType = BiomeType.Camp;
            cell.occupied = true;
        }
        // Add campfire
        WorldCell campFireCell = newWorld.GetCell(x_center, y_center);
        campFireCell.AssignObject("campfire");
        Debug.Log("WorldGeneratorScript: Added camp");

        return newWorld;
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
    }
}
