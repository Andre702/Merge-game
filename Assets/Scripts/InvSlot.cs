using UnityEngine;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

[Serializable]
public class InvSlot<T>
{
    public T type { get; private set; }
    public int count { get; private set; }
    public int maxStackSize { get; set; }
    public GameObject slotObject { get; set; }

    public TextMeshProUGUI countText;
    public Image icon;

    private static T emptyValue => default;

    // Konstruktor do tworzenia z UI
    public InvSlot(T type, int count, int maxStackSize, GameObject slotObject)
    {
        this.maxStackSize = maxStackSize; // Ustaw najpierw maxStackSize
        this.slotObject = slotObject;

        if (slotObject != null) // Zabezpieczenie
        {
            countText = slotObject.GetComponentInChildren<TextMeshProUGUI>();
            Transform iconTransform = slotObject.transform.Find("Icon");
            if (iconTransform != null) icon = iconTransform.GetComponent<Image>();
            else Debug.LogWarning($"Nie znaleziono 'Icon' w slocie {slotObject.name}");
        }

        AssignType(EqualityComparer<T>.Default.Equals(type, default) ? emptyValue : type);
        SetCount(count); // U¿yj SetCount do poprawnego ustawienia pocz¹tkowej wartoœci
    }

    public InvSlot(T type, int count, int maxStackSize)
    {
        this.type = type;
        this.count = count;
        this.maxStackSize = maxStackSize;
    }

    public void AssignType(T newType)
    {
        if (IsEmpty())
        {
            this.type = newType;
        }
        else if (!EqualityComparer<T>.Default.Equals(this.type, newType))
        {
            Debug.LogError("Próba zmiany typu niepustego slotu przez AssignType. U¿yj ClearSlot() najpierw.");
        }
    }

    public void SetCount(int newCount)
    {
        count = Mathf.Clamp(newCount, 0, maxStackSize);
        if (count == 0 && !EqualityComparer<T>.Default.Equals(type, emptyValue))
        {
            // Jeœli count spada do zera, a typ nie jest pusty,
            // to slot powinien staæ siê pusty (logicznie).
            // UI zostanie zaktualizowane przez PlayerInventory.UpdateSlotDisplay.
            // Mo¿na by te¿ bezpoœrednio wo³aæ ClearSlotLogicOnly(), ale
            // PlayerInventory kontroluje UpdateSlotDisplay.
            // AssignType(emptyValue); // To by wyzerowa³o te¿ typ
        }
    }

    public bool CanStack(T type, int amount)
    {
        if (EqualityComparer<T>.Default.Equals(this.type, emptyValue)) // if slot is empty
        {
            return true;
        }
        return this.type.Equals(type) && (count + amount) <= maxStackSize;
    }

    public int AddItems(int amount) //slot zna swój typ
    {
        if (EqualityComparer<T>.Default.Equals(this.type, emptyValue) && amount > 0)
        {
            // To siê nie powinno zdarzyæ, jeœli AssignType zosta³o wywo³ane
            Debug.LogError("Próba dodania przedmiotów do slotu bez przypisanego typu! U¿yj AssignType najpierw.");
            return amount;
        }
        int spaceLeft = maxStackSize - count;
        int added = Math.Min(spaceLeft, amount);
        count += added;
        // Aktualizacja UI powinna byæ robiona z poziomu PlayerInventory po tej operacji
        return amount - added;
    }

    public int RemoveItems(int amount)
    {
        int removed = Math.Min(count, amount);
        count -= removed;
        if (count <= 0)
        {
            ClearSlot();
        }
        return removed;
    }

    public bool IsEmpty() => count <= 0;

    public void ClearSlot()
    {
        type = emptyValue;
        count = 0;
    }

    public void UpdateDisplay(Sprite spriteToShow, int currentCount)
    {
        if (slotObject == null) return; // Slot nie ma UI (np. tylko dane)

        bool hasContent = currentCount > 0 && !EqualityComparer<T>.Default.Equals(this.type, emptyValue);

        if (icon != null)
        {
            if (hasContent && spriteToShow != null)
            {
                icon.sprite = spriteToShow;
                icon.enabled = true;
            }
            else
            {
                icon.sprite = null; 
                icon.enabled = false;
            }
        }

        if (countText != null)
        {
            if (hasContent && currentCount > 0)
            {
                countText.text = currentCount.ToString();
                countText.enabled = true;
            }
            else
            {
                countText.text = "";
                countText.enabled = false;
            }
        }


    }
}
