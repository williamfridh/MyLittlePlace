using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class YSortSprite : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    [SerializeField] private int precision = 100;
    [SerializeField] private int sortingOffset = 0;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void LateUpdate()
    {
        spriteRenderer.sortingOrder =
            Mathf.RoundToInt(-transform.position.y * precision) + sortingOffset;
    }
}