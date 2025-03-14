using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

    public InvSlot(T type, int count, int maxStackSize, GameObject slotObject)
    {
        this.type = type;
        this.count = count;
        this.maxStackSize = maxStackSize;
        this.slotObject = slotObject;

        countText = slotObject.GetComponentInChildren<TextMeshProUGUI>();
        icon = slotObject.transform.Find("Icon").GetComponent<Image>();

        this.type = EqualityComparer<T>.Default.Equals(type, default) ? emptyValue : type;
    }

    public bool CanStack(T type, int amount)
    {
        if (EqualityComparer<T>.Default.Equals(this.type, emptyValue)) // if slot is empty
        {
            return true;
        }
        return this.type.Equals(type) && (count + amount) <= maxStackSize;
    }

    public int AddItems(int amount)
    {
        int spaceLeft = maxStackSize - count;
        int added = Math.Min(spaceLeft, amount);
        count += added;
        return amount - added;
    }

    public int RemoveItems(int amount)
    {
        int removed = Math.Min(count, amount);
        count -= removed;
        return removed;
    }

    public bool IsEmpty() => count <= 0;

    public void ClearSlot()
    {
        type = emptyValue;
        count = 0;
    }
}
