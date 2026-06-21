using UnityEngine;

// The world save state is handled as a separate class,
// this is important for convertion to JSON.
// Keep this simple, as logic should remain mostly in
// the outer script.
[System.Serializable]
public class WorldSaveState
{
    public float noiseScale;
    [SerializeField] private float temperatureOffsetX;
    [SerializeField] private float temperatureOffsetY;
    [SerializeField] private float moistureOffsetX;
    [SerializeField] private float moistureOffsetY;
    [SerializeField] private float elevationOffsetX;
    [SerializeField] private float elevationOffsetY;
    [SerializeField] private float nutrientOffsetX;
    [SerializeField] private float nutrientOffsetY;

    [SerializeField] private int startingCampRadius = 3;

    public int width;
    public int height;
    public WorldCell[] cellGrid; // Flattened 2D array

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
        cellGrid = new WorldCell[w*h];
        noiseScale = noise;
        startingCampRadius = campRadius;
    
        // Prepare offsets
        temperatureOffsetX = Random.Range(0f, 10000f);
        temperatureOffsetY = Random.Range(0f, 10000f);
        moistureOffsetX = Random.Range(0f, 10000f);
        moistureOffsetY = Random.Range(0f, 10000f);
        elevationOffsetX = Random.Range(0f, 10000f);
        elevationOffsetY = Random.Range(0f, 10000f);
        nutrientOffsetX = Random.Range(0f, 10000f);
        nutrientOffsetY = Random.Range(0f, 10000f);
    }

    public WorldCell GetCell(int x, int y)
    {
        return cellGrid[x + y*world.width];
    }
}