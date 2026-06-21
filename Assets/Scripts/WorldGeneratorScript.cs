using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;
using System.Collections.Generic;

public class WorldGeneratorScript : MonoBehaviour
{
    // Singleton
    public static WorldGeneratorScript Instance { get; set; }

    private void ValueToScale(float value, out BiomeSelectionScale scale)
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

    WorldSaveState GenerateWorld(int w, int h, float noise, int campRad)
    {
        // Initilize a new world.
        WorldSaveState newWorld = new WorldSaveState(w, h, noise, campRad);

        // Loop through the tiles (x and y) coordinates
        // and assign temperature, moisture, elevation,
        // and nutrition, then biome type.
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {

                WorldCell cell = newWorld.GetCell(x, y);

                // Get temperature, moisture, elevation, and nutrition
                float temperature = Mathf.PerlinNoise(
                    (x + temperatureOffsetX) * noiseScale,
                    (y + temperatureOffsetY) * noiseScale
                    );
                float moisture = Mathf.PerlinNoise(
                    (x + moistureOffsetX) * noiseScale,
                    (y + moistureOffsetY) * noiseScale
                    );
                temperatureMap[x, y] = temperature;
                moistureMap[x, y] = moisture;
                cell.elevation = Mathf.PerlinNoise(
                    (x + elevationOffsetX) * noiseScale,
                    (y + elevationOffsetY) * noiseScale
                    );
                cell.nutrition = Mathf.PerlinNoise(
                    (x + nutrientOffsetX) * noiseScale,
                    (y + nutrientOffsetY) * noiseScale
                    );

                // Convert temperature and moisture to biome selection scale.
                ValueToScale(temperature, out BiomeSelectionScale tempScale);
                ValueToScale(moisture, out BiomeSelectionScale moistScale);
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

            return newWorld;
        }
        
        // Add map rock border.
        // Top and bottom borders
        for (int x = 0; x < width; x++)
        {
            WorldCell topCell = world.GetCell(x, 0);
            WorldCell bottomCell = world.GetCell(x, height-1);
            if (topCell.biomeType != BiomeType.Water) topCell.objectId = "rock_1x1_0";
            if (bottomCell.biomeType != BiomeType.Water) bottomCell.objectId = "rock_1x1_0";
        }
        // Left and right borders
        for (int y = 1; y < height - 1; y++)
        {
            WorldCell leftCell = world.GetCell(0, y);
            WorldCell rightCell = world.GetCell(width-1, y);
            if (leftCell.biomeType != BiomeType.Water) leftCell.objectId = "rock_1x1_0";
            if (rightCell.biomeType != BiomeType.Water) rightCell.objectId = "rock_1x1_0";
        }

        world = newWorld;
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

/*     void CreateCamp(int radius = 3)
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
            mapInitilized[cx, cy] = true;
        }
        // Move player
        PlayerScript.Instance.transform.position = new Vector3(x_center-2, y_center, 0);
        // Add campfire
        Instantiate(campfirePrefab, new Vector3(x_center, y_center, 0), Quaternion.identity, spawnedObjectsParent.transform);
    } */

    void SetBiomeType(int x, int y, BiomeType biomeType)
    {
        if (x == null || y == null || biomeType == null) return;
        biomeMap[x, y] = biomeType;
    }

    void Start()
    {
        GenerateWorld();
    }
}
