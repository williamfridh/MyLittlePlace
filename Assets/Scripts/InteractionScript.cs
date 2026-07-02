using UnityEngine;

public class InteractionScript : MonoBehaviour
{
    public static InteractionScript Instance { get; set; }

    private IInteractable currentInteractable;
    
    /// <summary>
    /// Trigger the interaction by calling the Interact() function of the object.
    /// Note that this must be complemeted with logic that will stop the interaction
    /// from even being attempted (could be present in the Interact() function).
    /// </summary>
    public bool TriggerInteraction()
    {
        if (currentInteractable == null) return false;
        bool interactWorked = currentInteractable.Interact();
        if (interactWorked) currentInteractable = null;
        return interactWorked;
    }

    /// <summary>
    /// Detect interactable object by selecting the correct compoenent
    /// and storing it in this object.
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerEnter2D(Collider2D other)
    {
        IInteractable interactable = other.GetComponent<IInteractable>();
        if (interactable == null) return;
        if (!interactable.CanInteract()) return;
        currentInteractable = interactable;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        IInteractable interactable = other.GetComponent<IInteractable>();
        if (currentInteractable == interactable)
        {
            currentInteractable = null;
        }
    } 

    public bool CanInteract()
    {
        return currentInteractable != null;
    }
    void Awake()
    {
        // Singleton pattern to ensure only one instance of PlayerScript exists
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
}