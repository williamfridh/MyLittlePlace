using UnityEngine;

public class HUDPanelScript : MonoBehaviour
{

    [Header("Game Objects")]
    [SerializeField] private GameObject leftControl;
    [SerializeField] private GameObject rightControl;

    void AdjustForSmartphone()
    {
        // Do something...
    }
    void AdjustForComputer()
    {
        leftControl.SetActive(false);
    }

    void Start()
    {
        if (GameStateScript.Instance == null)
        {
            Debug.LogWarning("HUDPanelScript: GameStateScript instance not found. Will adjust for smartphone!");
            AdjustForSmartphone();
        }
        else
        {
            if(!GameStateScript.Instance.usingComputer)
            {
                AdjustForSmartphone();
            }
            else
            {
                AdjustForComputer();
            }
        }
    }
}
