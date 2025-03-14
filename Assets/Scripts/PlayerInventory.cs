using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

[Serializable]
public class PlayerInventory : MonoBehaviour
{
    public GameObject slotPrefab;
    public Transform itemInventory;
    public Transform tileInventory;

    public List<InvSlot<TileType>> tileSlots { get; private set; }
    public List<InvSlot<ItemType>> itemSlots { get; private set; }
    public int tileStackLimit { get; private set; }
    public int itemStackLimit { get; private set; }
    public int maxTileSlots { get; private set; }
    public int maxItemSlots { get; private set; }

    public void InitializePlayerInventory(int maxTileSlots, int maxItemSlots, int tileStackLimit, int itemStackLimit)
    {
        this.maxTileSlots = maxTileSlots;
        this.maxItemSlots = maxItemSlots;
        this.tileStackLimit = tileStackLimit;
        this.itemStackLimit = itemStackLimit;

        tileSlots = new List<InvSlot<TileType>>();
        itemSlots = new List<InvSlot<ItemType>>();
    }

    public void SaveInventory(string filename)
    {
        string json = JsonUtility.ToJson(this, true);
        File.WriteAllText(Application.persistentDataPath + "/" + filename, json);
    }

    public static PlayerInventory LoadInventory(string filename)
    {
        string path = Application.persistentDataPath + "/" + filename;
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            return JsonUtility.FromJson<PlayerInventory>(json);
        }
        return null;
    }

    public bool Add(TileType tile, int amount)
    {
        foreach (var slot in tileSlots)
        {
            if (slot.CanStack(tile, amount))
            {
                amount = slot.AddItems(amount);
                if (amount == 0) return true;
            }
        }
        //if (tileSlots.Count < maxTileSlots)
        //{
        //    tileSlots.Add(new InvSlot<TileType>(tile, amount, tileStackLimit, Instantiate(slotPrefab, tileInventory)));
        //    return true;
        //}
        return false;
    }

    public bool Add(ItemType item, int amount)
    {
        if (item == ItemType.None) { return true; }
        foreach (var slot in itemSlots)
        {
            if (slot.CanStack(item, amount))
            {
                amount = slot.AddItems(amount);
                if (amount == 0) return true;
            }
        }
        //if (itemSlots.Count < maxItemSlots)
        //{
        //    itemSlots.Add(new InvSlot<ItemType>(item, amount, itemStackLimit, Instantiate(slotPrefab, itemInventory)));
        //    return true;
        //}
        return false;
    }

    public bool Remove(TileType tile, int amount)
    {
        for (int i = 0; i < tileSlots.Count; i++)
        {
            if (tileSlots[i].type == tile)
            {
                amount -= tileSlots[i].RemoveItems(amount);
                if (tileSlots[i].IsEmpty()) tileSlots[i].ClearSlot();
                if (amount <= 0) return true;
            }
        }
        return false;
    }

    public bool Remove(ItemType item, int amount)
    {
        for (int i = 0; i < itemSlots.Count; i++)
        {
            if (itemSlots[i].type == item)
            {
                amount -= itemSlots[i].RemoveItems(amount);
                if (itemSlots[i].IsEmpty()) itemSlots[i].ClearSlot();
                if (amount <= 0) return true;
            }
        }
        return false;
    }

    public void PrintInventory()
    {
        Debug.Log("Tile Inventory:");
        foreach (var slot in tileSlots)
        {
            Debug.Log($"{slot.type}: {slot.count}/{slot.maxStackSize}");
        }

        Debug.Log("Item Inventory:");
        foreach (var slot in itemSlots)
        {
            Debug.Log($"{slot.type}: {slot.count}/{slot.maxStackSize}");
        }
    }
}


