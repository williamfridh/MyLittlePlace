using UnityEngine;

public enum BiomeType
{
    Water = 0,     // Lowest dominance: everything blends over water
    Camp = 1,
    Desert = 2,    // (Sand)
    Meadow = 3,
    Forest = 4,
    Mountain = 5,  // Highest dominance: mountains cut cleanly over everything
    Menu = 100
}

public enum BiomeSelectionScale
{
    Low = 1,
    Medium = 2,
    High = 3
}

[System.Serializable]
public class WorldCell
{
    public BiomeType biomeType;
    public string objectID; // String such as "rock" or "birch"
    public string propID; // String such as "grass" or "busch"
    public BiomeSelectionScale temperature;
    public BiomeSelectionScale moisture;
    public float elevation;
    public float nutrition;
    public bool occupied = false;

    public bool AssignObject(string id, bool forceAdd = false)
    {
        if (occupied && !forceAdd) return false;
        objectID = id;
        occupied = true;
        return true;
    }
}