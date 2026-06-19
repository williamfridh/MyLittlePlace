using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class YSortSprite : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    [SerializeField] private int precision = 100;
    [SerializeField] private int sortingOffset = 0;

    [Header("Hierarchy Settings")]
    [Tooltip("If checked, it will sort by the parent's ground Y instead of the sprite's jump Y.")]
    [SerializeField] private bool useParentTransform;

    private Transform targetTransform;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (useParentTransform && transform.parent != null)
        {
            targetTransform = transform.parent;
        }
        else
        {
            targetTransform = transform;
        }
    }

    private void LateUpdate()
    {
        if (targetTransform == null) return;
        spriteRenderer.sortingOrder =
            Mathf.RoundToInt(-targetTransform.position.y * precision) + sortingOffset;
    }
}