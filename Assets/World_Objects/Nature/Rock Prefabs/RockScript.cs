using UnityEngine;

public class RockScript : MonoBehaviour
{
    [SerializeField] private Sprite[] rockVariants;

    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // Randomize the visual appearance immediately when the grass object is spawned
        if (rockVariants != null && rockVariants.Length > 0)
        {
            int randomIndex = Random.Range(0, rockVariants.Length);
            spriteRenderer.sprite = rockVariants[randomIndex];
        }
    }
}
