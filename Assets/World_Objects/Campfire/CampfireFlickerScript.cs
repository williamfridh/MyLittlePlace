using UnityEngine;
using UnityEngine.Rendering.Universal;

public class CampfireFlickerScript : MonoBehaviour
{
    [Header("Flicker Settings")]
    public float minRadius = 4.5f;
    public float maxRadius = 5.5f;

    public float flickerSpeed = 3.0f;

    private Light2D fireLight;
    private float randomOffset;

    void Start()
    {
        fireLight = GetComponent<Light2D>();
        randomOffset = Random.Range(0f, 100f);
    }

    void Update()
    {
        if (fireLight == null) return;
        float noise = Mathf.PerlinNoise(randomOffset, Time.time * flickerSpeed);
        fireLight.pointLightOuterRadius = Mathf.Lerp(minRadius, maxRadius, noise);   
    }
}
