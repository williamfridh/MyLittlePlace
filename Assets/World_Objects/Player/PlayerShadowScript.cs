using UnityEngine;

public class PlayerShadowScript : MonoBehaviour
{
    [Header("Objects")]
    [SerializeField] private Transform character;
    [SerializeField] private Transform shadow;

    [Header("Animation settings")]
    [SerializeField] private float maxJumpHeight;
    [Range(0f, 1f)]
    [SerializeField] private float minSize = 0.4f;

    private Vector3 shadowDefaultScale = Vector3.one;
    private Vector3 minSizeVector3 = Vector3.zero;
    private float groundLocalY;

    void Start()
    {
        if (character == null || shadow == null) return;
        // Captura initial transofrm profiles
        shadowDefaultScale = shadow.localScale;
        minSizeVector3 = shadowDefaultScale * minSize;
        // Get baseline anchor
        groundLocalY = character.localPosition.y;
    }

    void Update()
    {
        if (character == null || shadow == null) return;
        // Calc. relative height
        float currentHeight = character.localPosition.y - groundLocalY;
        if (currentHeight < 0) currentHeight = 0;
        // Normalize
        float heightPercentage = 0f;
        if (maxJumpHeight > 0.001f)
        {
            heightPercentage = Mathf.Clamp01(currentHeight / maxJumpHeight);
        }
        // SMooth scale execution
        shadow.localScale = Vector3.Lerp(shadowDefaultScale, minSizeVector3, heightPercentage);
    }
}
