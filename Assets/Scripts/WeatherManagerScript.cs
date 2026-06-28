using UnityEngine;
using UnityEngine.Rendering.Universal; // Required for URP 2D Lights

public class WeatherManagerScript : MonoBehaviour
{

    public Light2D globalLight;

    [Header("Meadow Settings")]
    public Color meadowDayColor;
    public float meadowDayIntensity;
    public Color meadowNightColor;
    public float meadowNightIntensity;

    [Header("Forest Settings")]
    public Color forestDayColor;
    public float forestDayIntensity;
    public Color forestNightColor;
    public float forestNightIntensity;

    [Header("Desert Settings")]
    public Color desertDayColor;
    public float desertDayIntensity;
    public Color desertNightColor;
    public float desertNightIntensity;

    [Header("Mountain Settings")]
    public Color mountainDayColor;
    public float mountainDayIntensity;
    public Color mountainNightColor;
    public float mountainNightIntensity;

    [Header("Water Settings")]
    public Color waterDayColor;
    public float waterDayIntensity;
    public Color waterNightColor;
    public float waterNightIntensity;

    private Color targetColor;
    private float targetIntensity;

    private Color currentDayColor;
    private Color currentNightColor;
    private float currentDayIntensity;
    private float currentNightIntensity;


    [Header("Blend Settings")]
    [Tooltip("How fast the environment lighting transitions between biomes.")]
    public float blendSpeed = 2.0f;

    private BiomeType lastPlayerBiome;
    private bool enabled = true;

     void Start()
    {
        // Stop if no WorldGeneratorScript is present
        if (WorldRendererScript.Instance == null)
        {
            Debug.LogError("WeatherManagerScript: No WorldRendererScript instance found in the scene.");
            enabled = false;
            return;
        }
        // Stop if no PlayerScript is present
        if (PlayerScript.Instance == null)
        {
            Debug.LogError("WeatherManagerScript: No PlayerScript instance found in the scene.");
            enabled = false;
            return;
        }
        // Continue as normal
        Vector3 playerPos = PlayerScript.Instance.transform.position;
        BiomeType playerBiome = SaveManagerScript.Instance.worldSave.GetBiomeAtPosition(playerPos);
        switch(playerBiome)
        {
            case BiomeType.Meadow:
                targetColor = meadowDayColor;
                targetIntensity = meadowDayIntensity;
                break;
            case BiomeType.Forest:
                targetColor = forestDayColor;
                targetIntensity = forestDayIntensity;
                break;
            case BiomeType.Desert:
                targetColor = desertDayColor;
                targetIntensity = desertDayIntensity;
                break;
            case BiomeType.Mountain:
                targetColor = mountainDayColor;
                targetIntensity = mountainDayIntensity;
                break;
            case BiomeType.Water:
                targetColor = waterDayColor;
                targetIntensity = waterDayIntensity;
                break;
        }
        globalLight.color = targetColor;
        globalLight.intensity = targetIntensity;
    }

    // Update is called once per frame
    void Update()
    {
        if (!enabled) return;
        // Get player location
        Vector3 playerPos = PlayerScript.Instance.transform.position;
        // Get biome at player location
        BiomeType playerBiome = SaveManagerScript.Instance.worldSave.GetBiomeAtPosition(playerPos);
        // Get time of day ratio
        float timeRatio = TimeManagerScript.Instance.GetTimeNightRatio();
        //Debug.Log(timeRatio);

        if (playerBiome != lastPlayerBiome)
        {
            lastPlayerBiome = playerBiome;
            Debug.Log("Player entered biome: " + playerBiome.ToString());
        }

        Color targetDayColor = meadowDayColor;
        Color targetNightColor = meadowDayColor;
        float targetDayIntensity = meadowDayIntensity;
        float targetNightIntensity = meadowNightIntensity;

        switch(playerBiome)
        {
            case BiomeType.Meadow:
                targetDayColor = meadowDayColor;
                targetNightColor = meadowNightColor;
                targetDayIntensity = meadowDayIntensity;
                targetNightIntensity= meadowNightIntensity;
                break;
            case BiomeType.Forest:
                targetDayColor = forestDayColor;
                targetNightColor = forestNightColor;
                targetDayIntensity = forestDayIntensity;
                targetNightIntensity= forestNightIntensity;
                break;
            case BiomeType.Desert:
                targetDayColor = desertDayColor;
                targetNightColor = desertNightColor;
                targetDayIntensity = desertDayIntensity;
                targetNightIntensity= desertNightIntensity;
                break;
            case BiomeType.Mountain:
                targetDayColor = mountainDayColor;
                targetNightColor = mountainNightColor;
                targetDayIntensity = mountainDayIntensity;
                targetNightIntensity= mountainNightIntensity;
                break;
            case BiomeType.Water:
                targetDayColor = waterDayColor;
                targetNightColor = waterNightColor;
                targetDayIntensity = waterDayIntensity;
                targetNightIntensity= waterNightIntensity;
                break;
        }

        currentDayColor = Color.Lerp(currentDayColor, targetDayColor, Time.deltaTime * blendSpeed);
        currentNightColor = Color.Lerp(currentNightColor, targetNightColor, Time.deltaTime * blendSpeed);
        currentDayIntensity = Mathf.Lerp(currentDayIntensity, targetDayIntensity, Time.deltaTime * blendSpeed);
        currentNightIntensity = Mathf.Lerp(currentNightIntensity, targetNightIntensity, Time.deltaTime * blendSpeed);

        globalLight.color =
            Color.Lerp(currentDayColor, currentNightColor, timeRatio);

        globalLight.intensity =
            Mathf.Lerp(currentDayIntensity, currentNightIntensity, timeRatio);
    }
}
