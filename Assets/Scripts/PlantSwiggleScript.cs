using UnityEngine;

public class PlantSwiggleScript : MonoBehaviour
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
        float targetZ = 0f;

        if (isPlayerInside && playerTransform != null)
        {
            float distanceX = playerTransform.position.x - plantTransform.position.x;
            float intersectionFactor = Mathf.Clamp(distanceX / interactionRadius, -1f, 1f);
            targetZ = intersectionFactor * maxBendAngle;
            plantTransform.localRotation = Quaternion.Euler(0f, 0f, targetZ);
        }
        else
        {
            // get current rotation and normalize
            float currentZ = plantTransform.localEulerAngles.z;
            if (currentZ > 180) currentZ -= 360f;

            // Smoothly rotate
            float newZ = Mathf.MoveTowards(currentZ, targetZ, restoreSpeed * Time.deltaTime);
            plantTransform.localRotation = Quaternion.Euler(0f, 0f, newZ);

            if (!isPlayerInside && Mathf.Abs(newZ) <= 0.05f)
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
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = false;
        }
    }
}
