using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

[Serializable]
public class InventorySlot<T>
{
    public T Type { get; private set; }
    public int Count { get; private set; }
    public int MaxStackSize { get; set; }

    public InventorySlot(T type, int count, int maxStackSize)
    {
        Type = type;
        Count = count;
        MaxStackSize = maxStackSize;
    }

    public bool CanStack(T type, int amount)
    {
        return Type.Equals(type) && (Count + amount) <= MaxStackSize;
    }

    public int AddItems(int amount)
    {
        int spaceLeft = MaxStackSize - Count;
        int added = Math.Min(spaceLeft, amount);
        Count += added;
        return amount - added;
    }

    public int RemoveItems(int amount)
    {
        int removed = Math.Min(Count, amount);
        Count -= removed;
        return removed;
    }

    public bool IsEmpty() => Count == 0;
}

[Serializable]
public class PlayerInventory
{
    public List<InventorySlot<TileType>> tileSlots { get; private set; }
    public List<InventorySlot<ItemType>> itemSlots { get; private set; }
    public int tileStackLimit { get; private set; }
    public int itemStackLimit { get; private set; }
    public int maxTileSlots { get; private set; }
    public int maxItemSlots { get; private set; }

    public PlayerInventory(int maxTileSlots, int maxItemSlots, int tileStackLimit, int itemStackLimit)
    {
        this.maxTileSlots = maxTileSlots;
        this.maxItemSlots = maxItemSlots;
        this.tileStackLimit = tileStackLimit;
        this.itemStackLimit = itemStackLimit;

        tileSlots = new List<InventorySlot<TileType>>();
        itemSlots = new List<InventorySlot<ItemType>>();
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
        if (tileSlots.Count < maxTileSlots)
        {
            tileSlots.Add(new InventorySlot<TileType>(tile, amount, tileStackLimit));
            return true;
        }
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
        if (itemSlots.Count < maxItemSlots)
        {
            itemSlots.Add(new InventorySlot<ItemType>(item, amount, itemStackLimit));
            return true;
        }
        return false;
    }

    public bool Remove(TileType tile, int amount)
    {
        for (int i = 0; i < tileSlots.Count; i++)
        {
            if (tileSlots[i].Type == tile)
            {
                amount -= tileSlots[i].RemoveItems(amount);
                if (tileSlots[i].IsEmpty()) tileSlots.RemoveAt(i);
                if (amount <= 0) return true;
            }
        }
        return false;
    }

    public bool Remove(ItemType item, int amount)
    {
        for (int i = 0; i < itemSlots.Count; i++)
        {
            if (itemSlots[i].Type == item)
            {
                amount -= itemSlots[i].RemoveItems(amount);
                if (itemSlots[i].IsEmpty()) itemSlots.RemoveAt(i);
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
            Debug.Log($"{slot.Type}: {slot.Count}/{slot.MaxStackSize}");
        }

        Debug.Log("Item Inventory:");
        foreach (var slot in itemSlots)
        {
            Debug.Log($"{slot.Type}: {slot.Count}/{slot.MaxStackSize}");
        }
    }
}


