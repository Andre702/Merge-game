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

    public void InitializePlayerInventory(int maxSlotsTiles, int maxSlotsItems, int stackLimitTiles, int stackLimitItems)
    {
        this.maxTileSlots = maxSlotsTiles;
        this.maxItemSlots = maxSlotsItems;
        this.tileStackLimit = stackLimitTiles;
        this.itemStackLimit = stackLimitItems;

        // Inicjalizacja list, jeœli nie zosta³y jeszcze utworzone
        if (tileSlots == null) tileSlots = new List<InvSlot<TileType>>(maxTileSlots);
        else tileSlots.Clear(); // Wyczyœæ jeœli ju¿ istniej¹

        if (itemSlots == null) itemSlots = new List<InvSlot<ItemType>>(maxItemSlots);
        else itemSlots.Clear();

        // Wyczyœæ stare sloty UI, jeœli istniej¹
        foreach (Transform child in tileInventory) Destroy(child.gameObject);
        foreach (Transform child in itemInventory) Destroy(child.gameObject);

        // Tworzenie UI i logicznych slotów dla KAFELKÓW
        for (int i = 0; i < maxTileSlots; i++)
        {
            GameObject slotGO = Instantiate(slotPrefab, tileInventory);
            slotGO.name = $"TileSlot_{i}";
            InvSlot<TileType> newSlot = new InvSlot<TileType>(TileType.UNLABELED, 0, tileStackLimit, slotGO);
            tileSlots.Add(newSlot);
            UpdateSlotDisplay(newSlot); // Zaktualizuj UI do stanu pustego (powinno ju¿ byæ z ClearSlot, ale dla pewnoœci)
        }

        // Tworzenie UI i logicznych slotów dla PRZEDMIOTÓW
        for (int i = 0; i < maxItemSlots; i++)
        {
            GameObject slotGO = Instantiate(slotPrefab, itemInventory);
            slotGO.name = $"ItemSlot_{i}";
            InvSlot<ItemType> newSlot = new InvSlot<ItemType>(ItemType.None, 0, itemStackLimit, slotGO);
            itemSlots.Add(newSlot);
            UpdateSlotDisplay(newSlot);
        }
    }

    // Metoda do zastosowania wczytanych danych (za³ó¿my, ¿e mamy PlayerInventoryData)
    public void ApplyLoadedData(PlayerInventoryData loadedData)
    {
        // Krok 1: Zainicjuj UI i logiczne struktury slotów.
        // U¿yj zapisanych wartoœci, jeœli istniej¹, w przeciwnym razie domyœlnych.
        InitializePlayerInventory(
            loadedData.savedMaxTileSlots > 0 ? loadedData.savedMaxTileSlots : this.maxTileSlots, // U¿yj zapisanych lub bie¿¹cych
            loadedData.savedMaxItemSlots > 0 ? loadedData.savedMaxItemSlots : this.maxItemSlots,
            loadedData.savedTileStackLimit > 0 ? loadedData.savedTileStackLimit : this.tileStackLimit,
            loadedData.savedItemStackLimit > 0 ? loadedData.savedItemStackLimit : this.itemStackLimit
        );

        // Krok 2: Zastosuj dane do slotów kafelków
        for (int i = 0; i < tileSlots.Count; i++) // Iteruj po aktualnie istniej¹cych slotach UI
        {
            if (i < loadedData.tileSlotsData.Count) // Jeœli s¹ dane dla tego slotu
            {
                InvSlotData<TileType> data = loadedData.tileSlotsData[i];
                if (data.count > 0 && data.type != TileType.UNLABELED)
                {
                    tileSlots[i].AssignType(data.type);
                    tileSlots[i].SetCount(data.count); // U¿yj metody, która bezpoœrednio ustawia iloœæ
                }
                else
                {
                    tileSlots[i].ClearSlot(); // Tylko logika, UI póŸniej
                }
            }
            else // Brak danych dla tego slotu UI (np. ekwipunek zosta³ zmniejszony)
            {
                tileSlots[i].ClearSlot();
            }
            UpdateSlotDisplay(tileSlots[i]); // Zawsze aktualizuj UI dla ka¿dego slotu
        }

        // Analogicznie dla itemSlots
        for (int i = 0; i < itemSlots.Count; i++)
        {
            if (i < loadedData.itemSlotsData.Count)
            {
                InvSlotData<ItemType> data = loadedData.itemSlotsData[i];
                if (data.count > 0 && data.type != ItemType.None)
                {
                    itemSlots[i].AssignType(data.type);
                    itemSlots[i].SetCount(data.count); // U¿yj metody, która bezpoœrednio ustawia iloœæ
                }
                else
                {
                    itemSlots[i].ClearSlot();
                }
            }
            else
            {
                itemSlots[i].ClearSlot();
            }
            UpdateSlotDisplay(itemSlots[i]);
        }
    }

    // Metody zapisu i odczytu teraz operuj¹ na PlayerInventoryData
    public void SaveInventoryToFile(string filename) // Zmieniona nazwa dla jasnoœci
    {
        PlayerInventoryData dataToSave = GetInventoryDataForSave();
        string json = JsonUtility.ToJson(dataToSave, true);
        File.WriteAllText(Application.persistentDataPath + "/" + filename, json);
        Debug.Log($"Inventory saved to {filename}");
    }

    public bool LoadInventoryFromFile(string filename) // Zwraca bool, czy siê uda³o
    {
        string path = Application.persistentDataPath + "/" + filename;
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            PlayerInventoryData loadedData = JsonUtility.FromJson<PlayerInventoryData>(json);
            if (loadedData != null)
            {
                ApplyLoadedData(loadedData);
                Debug.Log($"Inventory loaded from {filename}");
                return true;
            }
            else
            {
                Debug.LogError($"Failed to parse inventory data from {filename}");
                return false;
            }
        }
        Debug.LogWarning($"Inventory file not found: {filename}");
        return false;
    }

    public PlayerInventoryData GetInventoryDataForSave()
    {
        PlayerInventoryData data = new PlayerInventoryData
        {
            // Zapisz aktualne konfiguracje
            savedMaxTileSlots = this.maxTileSlots,
            savedMaxItemSlots = this.maxItemSlots,
            savedTileStackLimit = this.tileStackLimit,
            savedItemStackLimit = this.itemStackLimit,

            tileSlotsData = new List<InvSlotData<TileType>>(),
            itemSlotsData = new List<InvSlotData<ItemType>>()
        };

        foreach (var slot in tileSlots)
        {
            data.tileSlotsData.Add(new InvSlotData<TileType> { type = slot.type, count = slot.count });
        }
        foreach (var slot in itemSlots)
        {
            data.itemSlotsData.Add(new InvSlotData<ItemType> { type = slot.type, count = slot.count });
        }
        return data;
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
        if (tile == TileType.UNLABELED && amount > 0) return true; // Nie dodawaj pustych

        foreach (var slot in tileSlots)
        {
            if (slot.type == tile && slot.CanStack(tile, amount))
            {
                amount = slot.AddItems(amount);
                UpdateSlotDisplay(slot); // Zaktualizuj UI
                if (amount == 0) return true;
            }
        }

        if (amount > 0)
        {
            foreach (var slot in tileSlots)
            {
                if (slot.IsEmpty())
                {
                    // Musimy 'ustawiæ' typ dla pustego slotu przed dodaniem
                    // To wymaga ma³ej modyfikacji InvSlot lub nowej metody
                    slot.AssignType(tile); // Za³ó¿my, ¿e dodasz tê metodê do InvSlot
                    amount = slot.AddItems(amount);
                    UpdateSlotDisplay(slot); // Zaktualizuj UI
                    if (amount == 0) return true;
                }
            }
        }

        if (amount > 0)
        {
            Debug.LogWarning($"Brak miejsca w ekwipunku na {tile} (iloœæ: {amount})");
            return false;
        }
        return true;
    }

    public bool Add(ItemType item, int amount)
    {
        if (item == ItemType.None && amount > 0) return true;

        foreach (var slot in itemSlots)
        {
            if (slot.type == item && slot.CanStack(item, amount))
            {
                amount = slot.AddItems(amount);
                UpdateSlotDisplay(slot); // Zaktualizuj UI
                if (amount == 0) return true;
            }
        }

        if (amount > 0)
        {
            foreach (var slot in itemSlots)
            {
                if (slot.IsEmpty())
                {
                    slot.AssignType(item); // Za³ó¿my, ¿e dodasz tê metodê do InvSlot
                    amount = slot.AddItems(amount);
                    UpdateSlotDisplay(slot); // Zaktualizuj UI
                    if (amount == 0) return true;
                }
            }
        }

        if (amount > 0)
        {
            Debug.LogWarning($"Brak miejsca w ekwipunku na {item} (iloœæ: {amount})");
            return false;
        }
        return true;

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
                int removedAmount = tileSlots[i].RemoveItems(amount);
                amount -= removedAmount;

                if (tileSlots[i].IsEmpty()) // Jeœli po usuniêciu slot sta³ siê pusty
                {
                    // Logika w RemoveItems ju¿ powinna wywo³aæ ClearSlotLogicOnly()
                    // Tutaj tylko aktualizujemy UI
                }
                UpdateSlotDisplay(tileSlots[i]);

                if (amount <= 0) return true;
            }
        }
        return false; // Nie uda³o siê usun¹æ ca³ej ¿¹danej iloœci
    }

    public bool Remove(ItemType item, int amount)
    {
        for (int i = 0; i < itemSlots.Count; i++)
        {
            if (itemSlots[i].type == item)
            {
                int removedAmount = itemSlots[i].RemoveItems(amount);
                amount -= removedAmount;

                if (itemSlots[i].IsEmpty()) // Jeœli po usuniêciu slot sta³ siê pusty
                {
                    // Logika w RemoveItems ju¿ powinna wywo³aæ ClearSlotLogicOnly()
                    // Tutaj tylko aktualizujemy UI
                }
                UpdateSlotDisplay(itemSlots[i]);

                if (amount <= 0) return true;
            }
        }
        return false; // Nie uda³o siê usun¹æ ca³ej ¿¹danej iloœci
    }

    private void UpdateSlotDisplay<T>(InvSlot<T> slot)
    {
        if (slot == null || slot.slotObject == null) return;

        Sprite spriteToDisplay = null;
        if (!slot.IsEmpty())
        {
            if (slot is InvSlot<TileType> tileSlot)
            {
                spriteToDisplay = ResourceManager.Instance.GetSprite(tileSlot.type);
            }
            else if (slot is InvSlot<ItemType> itemSlot)
            {
                spriteToDisplay = ResourceManager.Instance.GetSprite(itemSlot.type);
            }
        }

        // pusty slot -> spriteToDisplay == null
        slot.UpdateDisplay(spriteToDisplay, slot.count);
    }

    // Wywo³uj UpdateSlotDisplay po ka¿dej modyfikacji slotu w Add/Remove
    // oraz po ClearSlot.

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

[Serializable]
public class InvSlotData<T> // Musi byæ generyczna, tak jak InvSlot
{
    public T type;
    public int count;
    // Nie ma potrzeby przechowywaæ maxStackSize, jeœli jest sta³y
    // i ustawiany podczas inicjalizacji ekwipunku.
    // Jeœli maxStackSize mo¿e siê ró¿niæ per slot i jest dynamiczny, to dodaj tutaj.
}

[Serializable]
public class PlayerInventoryData
{
    public List<InvSlotData<TileType>> tileSlotsData;
    public List<InvSlotData<ItemType>> itemSlotsData;

    // Opcjonalnie, jeœli te wartoœci maj¹ byæ zapisywane i odczytywane:
    public int savedMaxTileSlots;
    public int savedMaxItemSlots;
    public int savedTileStackLimit;
    public int savedItemStackLimit;

    public PlayerInventoryData()
    {
        tileSlotsData = new List<InvSlotData<TileType>>();
        itemSlotsData = new List<InvSlotData<ItemType>>();
    }
}
