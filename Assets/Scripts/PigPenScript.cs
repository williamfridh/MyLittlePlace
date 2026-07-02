using UnityEngine;

public class PigPenScript : MonoBehaviour
{

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("PigPenScript: Trigger");
        WorldObjectIdentity objectIdentity = other.GetComponent<WorldObjectIdentity>();
        if (objectIdentity.UniqueObjectID != "lost_boar") return;
        Debug.Log("PigPenScript: Lost Boar entered the pen");
        // Remove the Lost Boar from the scene
        GameObject lostBoar = other.gameObject;
        GameObject.Destroy(lostBoar);
        // Add the Lost Boar to the player's inventory
        SaveManagerScript.Instance.playerSave.AddToInventory("boar", 1);
    }
}
