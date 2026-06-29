using UnityEngine;
using System.Collections;

public class WoodPile : MonoBehaviour, IInteractable
{
    [SerializeField] private int amount = 1;
    public bool Interact()
    {
        Debug.Log("WoodPile: Interaction detected");
        if (amount > 0)
        {
            if (SaveManagerScript.Instance == null) return false;
            SaveManagerScript.Instance.playerSave.AddToInventory("wood", amount);
            amount = 0;
            if (AudioManagerScript.Instance) AudioManagerScript.Instance.PlayGatherWoodSound();
            Destroy(gameObject);
            return true;
        }
        else
        {
            return false;
        }
    }
}