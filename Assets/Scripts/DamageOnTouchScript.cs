using UnityEngine;

public class DamageOnTouchScript : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private int damage = 1;
    [SerializeField] private float pushback = 2;

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("DamageOnTouchScript: Trigger");
        Unit playerUnit = other.GetComponent<Unit>();
        if (playerUnit != null)
        {
            playerUnit.Damage(damage, pushback, transform.position);
        }
        else
        {
            Debug.LogWarning("DamageOnTouchScript: No Unit component found on the colliding object.");
        }
    }
}
