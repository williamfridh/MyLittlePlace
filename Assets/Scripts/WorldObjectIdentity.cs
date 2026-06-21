using UnityEngine;

public class WorldObjectIdentity : MonoBehaviour
{
    [Tooltip("The unique ID used to look up this prefab in your save/load database.")]
    [SerializeField] private string uniqueObjectID;

    public string UniqueObjectID => uniqueObjectID;
}