using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ItemVisualDefinition
{
    public string id;
    public string name;
    public Sprite icon;
}

[CreateAssetMenu(fileName = "ItemDatabase", menuName = "Game/Item Database")]
public class ItemDatabase : ScriptableObject
{
    [SerializeField] private List<ItemVisualDefinition> itemDefinitions;

    // Fast lookup dictionary initilized at runtime avoids O(N) list searchers
    private Dictionary<string, ItemVisualDefinition> _dbLookup;

    public void Initilize()
    {
        _dbLookup = new Dictionary<string, ItemVisualDefinition>();
        foreach (var def in itemDefinitions)
        {
            if (!_dbLookup.ContainsKey(def.id))
            {
                _dbLookup.Add(def.id, def);
            }
        }
    }

    public ItemVisualDefinition GetItemData(string id)
    {
        if (_dbLookup == null) Initilize();

        if (_dbLookup.TryGetValue(id, out ItemVisualDefinition data)) return data;

        Debug.LogWarning($"ItemDatabase: Item id '{id}' not found in database!");
        return null;
    }
}