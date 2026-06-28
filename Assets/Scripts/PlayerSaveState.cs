using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[System.Serializable]
public class InventoryItem
{
    public string id;
    public int amount;
}

[System.Serializable]
public class PlayerSaveState
{

    public int life = 10;
    public float x_pos;
    public float y_pos;
    public bool position_initilized = false;
    public List<InventoryItem> inventory = new List<InventoryItem>();

    // JsonUtility loading fallback
    public PlayerSaveState() { }

    /// <summary>
    /// Adds to player inventory. Returns true/false depending on if the
    /// item already existed in the inventory and was simply increased.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="amount"></param>
    /// <returns></returns>
    public bool AddToInventory(string id, int amount)
    {
        InventoryItem item = inventory.Find(i => i.id == id);
        if (item == null)
        {
            inventory.Add(new InventoryItem
            {
                id = id,
                amount = amount
            });
            TriggerSave();
            return false;
        }
        else
        {
            item.amount += amount;
            TriggerSave();
            return true;
        }
    }

    /// <summary>
    /// Removes from player inventory. Rerturns true on success,
    /// and false upon failure.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="amount"></param>
    public bool RemoveFromInventory(string id, int amount)
    {
        InventoryItem item = inventory.Find(i => i.id == id);
        if (item == null || item.amount < amount) return false;
        item.amount -= amount;
        if (item.amount <= 0)
        {
            inventory.Remove(item);
        }
        TriggerSave();
        return true;
    }

    void TriggerSave()
    {
        if (SaveManagerScript.Instance)
        {
            Debug.Log("PlayerSaveState: Triggered player save.");
            SaveManagerScript.Instance.SavePlayer();
        }
        else
        {
            Debug.LogWarning("PlayerSaveState: Could not find SaveManagerScript instance, thus cannot tirgger save!");
        }
    }
}