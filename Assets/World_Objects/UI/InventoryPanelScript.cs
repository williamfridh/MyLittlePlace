using UnityEngine;
using System.Collections.Generic;

public class InventoryPanelScript : MonoBehaviour
{

    public static InventoryPanelScript Instance { get; private set; }

    [Header("References")]
    [SerializeField] private ItemDatabase itemDatabase;
    [SerializeField] private Transform uiContainer;
    [SerializeField] private GameObject itemPrefab;

    private List<GameObject> _activeItemElements = new List<GameObject>();


    public void RefreshInventoryUI()
    {
        if (SaveManagerScript.Instance == null)
        {
            Debug.LogError("InventoryPanelScript: Could not refreash inventory ui due to inaccessible SaveManagerScript instance!");
            return;
        }
        else
        {
            Debug.Log("InventoryPanelScript: Refreshing inventory UI");
        }

        List<InventoryItem> playerInventory = SaveManagerScript.Instance.playerSave.inventory;

        // Clear UI
        foreach (var element in _activeItemElements) Destroy(element);
        _activeItemElements.Clear();

        // Loop trough the inventory
        foreach (InventoryItem item in playerInventory)
        {
            Debug.Log($"item id: {item.id} amount: {item.amount}");
            if (item.amount <= 0) continue;
            ItemVisualDefinition visualAsset = itemDatabase.GetItemData(item.id);
            if (visualAsset == null)
            {
                Debug.LogWarning("InventoryPanelScript: Could not find database entry '{item.id}'");
                continue;   
            }
            
            GameObject entryGo = Instantiate(itemPrefab, uiContainer);
            _activeItemElements.Add(entryGo);

            InventoryItemScript entryScript = entryGo.GetComponent<InventoryItemScript>();
            entryScript.Draw(visualAsset.icon, visualAsset.name, item.amount, visualAsset.description);
        }
        
    }
    
    void Start()
    {
        RefreshInventoryUI();
    }

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
}
