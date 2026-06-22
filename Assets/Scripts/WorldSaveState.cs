using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

// The world save state is handled as a separate class,
// this is important for convertion to JSON.
// Keep this simple, as logic should remain mostly in
// the outer script.
[System.Serializable]
public class WorldSaveState
{
    public float noiseScale;
    [SerializeField] public float temperatureOffsetX;
    [SerializeField] public float temperatureOffsetY;
    [SerializeField] public float moistureOffsetX;
    [SerializeField] public float moistureOffsetY;
    [SerializeField] public float elevationOffsetX;
    [SerializeField] public float elevationOffsetY;
    [SerializeField] public float nutrientOffsetX;
    [SerializeField] public float nutrientOffsetY;

    [SerializeField] public int startingCampRadius = 3;


    [SerializeField] public string worldCreatedAt;

    public int width;
    public int height;
    public List<WorldCell> cellGrid = new List<WorldCell>(); // Flattened 2D array

    // Public getters
    public float TempOffsetX => temperatureOffsetX;
    public float TempOffsetY => temperatureOffsetY;
    public float MoistOffsetX => moistureOffsetX;
    public float MoistOffsetY => moistureOffsetY;
    public float ElevOffsetX => elevationOffsetX;
    public float ElevOffsetY => elevationOffsetY;
    public float NutOffsetX => nutrientOffsetX;
    public float NutOffsetY => nutrientOffsetY;
    public int StartingCampRadius => startingCampRadius;

    // JsonUtility loading fallback
    public WorldSaveState() { }

    public WorldSaveState(int w, int h, float noise, int campRadius)
    {
        width = w;
        height = h;
        noiseScale = noise;
        startingCampRadius = campRadius;
        worldCreatedAt = DateTime.Now.ToString("o");
    
        // Prepare offsets
        temperatureOffsetX = UnityEngine.Random.Range(0f, 10000f);
        temperatureOffsetY = UnityEngine.Random.Range(0f, 10000f);
        moistureOffsetX = UnityEngine.Random.Range(0f, 10000f);
        moistureOffsetY = UnityEngine.Random.Range(0f, 10000f);
        elevationOffsetX = UnityEngine.Random.Range(0f, 10000f);
        elevationOffsetY = UnityEngine.Random.Range(0f, 10000f);
        nutrientOffsetX = UnityEngine.Random.Range(0f, 10000f);
        nutrientOffsetY = UnityEngine.Random.Range(0f, 10000f);
    }

    public WorldCell GetCell(int x, int y)
    {
        return cellGrid[x + y*width];
    }

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
            WorldCell cell = GetCell(x, y);
            return cell.biomeType;
        }
        else
        {
            throw new System.IndexOutOfRangeException("Position out of bounds of the biome map.");
        }
    }

    /// <summary>
    /// Used for fetching a list of coordinates shaping a circle. Intended to be
    /// reused, created for CreateCamp function.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="radius"></param>
    public List<Vector2> GetCircleCoordinates(float x, float y, int radius)
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

    /// <summary>
    /// Used for checking if certain objects can fit by checking target and
    /// surrounding cells (whether they are occupied or not).
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="sizeX"></param>
    /// <param name="sizeY"></param>
    /// <returns></returns>
    private bool CanFitSize(int x, int y, int sizeX, int sizeY)
    {
        for (int i = 0; i < sizeX; i++) {
            for (int j = 0; j < sizeY; j++) {
                // Catch index out of bounds
                if (x + i >= width || y + j >= height) return false;
                // Select cell
                WorldCell cell = GetCell(x + i, y + j);
                // Check if if specific offset tile is water
                if (cell.biomeType == BiomeType.Water) return false;
                // Check if tile is occupied
                if (cell.occupied) return false;
            }
        }
        return true; // All tiles are within bounds and not occupied
    }
}