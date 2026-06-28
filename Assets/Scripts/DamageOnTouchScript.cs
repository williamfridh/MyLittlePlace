using UnityEngine;

public class DamageOnTouchScript : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private int damage = 1;
    [SerializeField] private float pushback = 2;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Only trigger for player and if player instance exists
        if (!other.CompareTag("Player") || PlayerScript.Instance == null) return;
        Debug.Log("DamageOnTouchScript: Trigger");
        PlayerScript.Instance.TakeDamage(damage, pushback, transform.position);
    }
}
