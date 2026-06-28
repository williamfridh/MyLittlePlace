using UnityEngine;

public enum BiomeType
{
    Menu = 100,
    Camp = 1,
    Meadow = 2,
    Forest = 3,
    Mountain = 4,
    Desert = 5,
    Water = 6
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