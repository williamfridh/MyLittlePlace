using UnityEngine;
using System.Collections;

public class HealingAreaScript : MonoBehaviour
{
    private bool inside = false;
    [SerializeField] private float healingInterval = 10f;
    [SerializeField] private int healingAmount = 1;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Only trigger for player and if player instance exists
        if (!other.CompareTag("Player") || PlayerScript.Instance == null) return;
        Debug.Log("HealingAreaScript: Enter");
        inside = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // Only trigger for player and if player instance exists
        if (!other.CompareTag("Player") || PlayerScript.Instance == null) return;
        Debug.Log("HealingAreaScript: Exit");
        inside = false;
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        // Only trigger for player and if player instance exists
        if (!other.CompareTag("Player") || PlayerScript.Instance == null) return;
        Debug.Log("HealingAreaScript: Stay");
        inside = true;
    }

    void Start()
    {
        StartCoroutine(Heal());
    }

    public IEnumerator Heal()
    {
        while (true)
        {
            yield return new WaitForSeconds(healingInterval);
            if (inside) PlayerScript.Instance.Heal(healingAmount);
        }
    }
}
