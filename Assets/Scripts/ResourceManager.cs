using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance { get; private set; }

    // Wype�nij te s�owniki w Inspektorze lub w Awake/Start
    public SerializableDictionary<TileType, Sprite> tileSprites;
    public SerializableDictionary<ItemType, Sprite> itemSprites;
    public Sprite defaultEmptySlotSprite; // Opcjonalnie

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public Sprite GetSprite(TileType type)
    {
        return tileSprites.TryGetValue(type, out Sprite sprite) ? sprite : defaultEmptySlotSprite;
    }

    public Sprite GetSprite(ItemType type)
    {
        return itemSprites.TryGetValue(type, out Sprite sprite) ? sprite : defaultEmptySlotSprite;
    }
}
// Do SerializableDictionary potrzebujesz niestandardowej klasy, np. z GitHub lub Asset Store,
// albo u�yj dw�ch list (jedna dla kluczy, druga dla warto�ci) i buduj s�ownik w Awake.
// Alternatywnie, dla prostoty, mo�na u�y� if/else lub switch w GetSprite.