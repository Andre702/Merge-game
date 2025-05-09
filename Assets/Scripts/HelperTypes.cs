using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
{
    [SerializeField]
    private List<TKey> keys = new List<TKey>();

    [SerializeField]
    private List<TValue> values = new List<TValue>();

    // Zapisuje s³ownik do list przed serializacj¹ przez Unity
    public void OnBeforeSerialize()
    {
        keys.Clear();
        values.Clear();
        foreach (KeyValuePair<TKey, TValue> pair in this)
        {
            keys.Add(pair.Key);
            values.Add(pair.Value);
        }
    }

    // Wczytuje s³ownik z list po deserializacji przez Unity
    public void OnAfterDeserialize()
    {
        this.Clear();

        if (keys.Count != values.Count)
        {
            throw new System.Exception(string.Format("W SerializableDictionary<{0},{1}> jest {2} kluczy i {3} wartoœci. Powinny byæ równe.", typeof(TKey), typeof(TValue), keys.Count, values.Count));
        }

        for (int i = 0; i < keys.Count; i++)
        {
            // Sprawdzenie duplikatów kluczy przy deserializacji, jeœli listy zosta³y rêcznie zmodyfikowane w Inspektorze
            if (!this.ContainsKey(keys[i]))
            {
                this.Add(keys[i], values[i]);
            }
            // Mo¿na dodaæ logikê obs³ugi duplikatów, jeœli to konieczne, np. Debug.LogWarning.
            // Obecnie cicho ignoruje zduplikowane klucze podczas deserializacji, bior¹c pierwszy napotkany.
        }
    }
}