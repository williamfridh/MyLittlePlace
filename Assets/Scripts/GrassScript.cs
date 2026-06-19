using UnityEngine;

public class GrassScript : MonoBehaviour
{
    [Header("Sway Settings")]
    [SerializeField] private float maxBendAngle = 15f;
    [SerializeField] private float interactionRadius = 0.8f;
    [SerializeField] private float restoreSpeed = 5f;

    [SerializeField] private Transform plantTransform;
    private Transform playerTransform;
    private bool isPlayerInside = false;

    void Update()
    {
        // If player is not touching grass
        if (!isPlayerInside)
        {
            if (playerTransform == null) return;
            float currentZ = plantTransform.localEulerAngles.z;
            if (currentZ > 180) currentZ -= 360f; // Handle Unity's 0-360 deg. warp around
            if (Mathf.Abs(currentZ) > 0.05f)
            {
                float newZ = Mathf.MoveTowards(currentZ, 0f, restoreSpeed * Time.deltaTime);
                plantTransform.localRotation = Quaternion.Euler(0f, 0f, newZ);
            }
            else
            {
                playerTransform = null;
            }
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        
        isPlayerInside = true;
        playerTransform = other.transform;

        // Calc. vectore from grass origin to player, focusing on X axis
        float distanceX = playerTransform.position.x - plantTransform.position.x;
        // Normalize distance using max interaction radius
        float interactionFactor = Mathf.Clamp(distanceX / interactionRadius, -1f, 1f);
        // Calc. target angle
        float targetZRotation = interactionFactor * maxBendAngle;

        // Smoothly interpolate to calculated angle
        plantTransform.localRotation = Quaternion.Euler(0f, 0f, targetZRotation);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = false;
        }
    }
}
