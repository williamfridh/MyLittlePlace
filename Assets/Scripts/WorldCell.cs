using UnityEngine;

public enum BiomeType
{
    Meadow = 1,
    Forest = 2,
    Mountain = 3,
    Desert = 4,
    Water = 5
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
    public string objectId; // String such as "rock" or "birch"
    public string propId; // String such as "grass" or "busch"
    public BiomeSelectionScale temperature;
    public BiomeSelectionScale moisture;
    public float elevation;
    public float nutrition;
}