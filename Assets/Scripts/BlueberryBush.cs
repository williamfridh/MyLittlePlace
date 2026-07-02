using UnityEngine;
using System.Collections;

public class BlueberryBush : MonoBehaviour, IInteractable
{
    [SerializeField] private GameObject berriesObject;
    [SerializeField] private int amount = 1;
    public bool Interact()
    {
        Debug.Log("BlueberryBush: Interaction detected");
        if (amount > 0)
        {
            if (SaveManagerScript.Instance == null) return false;
            SaveManagerScript.Instance.playerSave.AddToInventory("blueberry", amount);
            amount = 0;
            StartCoroutine(GrowNewBerry());
            if (AudioManagerScript.Instance) AudioManagerScript.Instance.PlayGatherSound();
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool CanInteract()
    {
        return amount > 0;
    }

    void Start()
    {
        // If no berries on restart, restart timer
        if (amount == 0) StartCoroutine(GrowNewBerry());
    }

    void Update()
    {
        berriesObject.SetActive(amount >= 1);
    }

    public IEnumerator GrowNewBerry()
    {
        yield return new WaitForSeconds(10f);
        amount = 1; 
    }
}