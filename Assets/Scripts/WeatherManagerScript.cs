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

    [Header("Blend Settings")]
    [Tooltip("How fast the environment lighting transitions between biomes.")]
    public float blendSpeed = 2.0f;

    private WorldGeneratorScript.BiomeType lastPlayerBiome;

     void Start()
    {
        Vector3 playerPos = PlayerScript.Instance.transform.position;
        WorldGeneratorScript.BiomeType playerBiome = WorldGeneratorScript.Instance.GetBiomeAtPosition(playerPos);
        switch(playerBiome)
        {
            case WorldGeneratorScript.BiomeType.Meadow:
                targetColor = meadowDayColor;
                targetIntensity = meadowDayIntensity;
                break;
            case WorldGeneratorScript.BiomeType.Forest:
                targetColor = forestDayColor;
                targetIntensity = forestDayIntensity;
                break;
            case WorldGeneratorScript.BiomeType.Desert:
                targetColor = desertDayColor;
                targetIntensity = desertDayIntensity;
                break;
            case WorldGeneratorScript.BiomeType.Mountain:
                targetColor = mountainDayColor;
                targetIntensity = mountainDayIntensity;
                break;
            case WorldGeneratorScript.BiomeType.Water:
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
        // Get player location
        Vector3 playerPos = PlayerScript.Instance.transform.position;
        // Get biome at player location
        WorldGeneratorScript.BiomeType playerBiome = WorldGeneratorScript.Instance.GetBiomeAtPosition(playerPos);
        // Get time of day ratio
        float timeRatio = TimeManagerScript.Instance.GetTimeNightRatio();

        if (playerBiome != lastPlayerBiome)
        {
            lastPlayerBiome = playerBiome;
            Debug.Log("Player entered biome: " + playerBiome.ToString());
        }

        switch(playerBiome)
        {
            case WorldGeneratorScript.BiomeType.Meadow:
                targetColor = Color.Lerp(meadowDayColor, meadowNightColor, timeRatio);
                targetIntensity = Mathf.Lerp(meadowDayIntensity, meadowNightIntensity, timeRatio);
                break;
            case WorldGeneratorScript.BiomeType.Forest:
                targetColor = Color.Lerp(forestDayColor, forestNightColor, timeRatio);
                targetIntensity = Mathf.Lerp(forestDayIntensity, forestNightIntensity, timeRatio);
                break;
            case WorldGeneratorScript.BiomeType.Desert:
                targetColor = Color.Lerp(desertDayColor, desertNightColor, timeRatio);
                targetIntensity = Mathf.Lerp(desertDayIntensity, desertNightIntensity, timeRatio);
                break;
            case WorldGeneratorScript.BiomeType.Mountain:
                targetColor = Color.Lerp(mountainDayColor, mountainNightColor, timeRatio);
                targetIntensity = Mathf.Lerp(mountainDayIntensity, mountainNightIntensity, timeRatio);
                break;
            case WorldGeneratorScript.BiomeType.Water:
                targetColor = Color.Lerp(waterDayColor, waterNightColor, timeRatio);
                targetIntensity = Mathf.Lerp(waterDayIntensity, waterNightIntensity, timeRatio);
                break;
        }

        globalLight.color = Color.Lerp(globalLight.color, targetColor, Time.deltaTime * blendSpeed);
        globalLight.intensity = Mathf.Lerp(globalLight.intensity, targetIntensity, Time.deltaTime * blendSpeed);
    }
}
