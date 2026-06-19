using UnityEngine;

public class PlantGrassScript : MonoBehaviour
{
    [SerializeField] private Sprite[] grassVariants;

    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // Randomize the visual appearance immediately when the grass object is spawned
        if (grassVariants != null && grassVariants.Length > 0)
        {
            int randomIndex = Random.Range(0, grassVariants.Length);
            spriteRenderer.sprite = grassVariants[randomIndex];
        }
    }
}
