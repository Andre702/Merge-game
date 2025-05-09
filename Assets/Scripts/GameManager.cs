using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;


public class GameManager : MonoBehaviour
{
    public World currentWorld;
    public static GameManager Instance { get; private set; }

    public List<MergeProgression> progressionPaths;

    public Dictionary<TileBase, TileBase> mergeProgression = new Dictionary<TileBase, TileBase>();

    public PlayerInventory inventory;
    public string inventorySaveFileName = "playerInventory.json";

    private void Awake()
    {
        // If there is an instance, and it's not me, delete myself.

        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    private void Start()
    {
        InitializeMergreProgression();
        LoadGameData();
    }

    void InitializeMergreProgression()
    {
        mergeProgression.Clear();

        foreach (var path in progressionPaths)
        {
            foreach (var pair in path.mergePairs)
            {
                mergeProgression[pair.currentItem] = pair.nextItem;
            }

        }
        
    }

    public void SaveGameData()
    {
        if (inventory != null)
        {
            inventory.SaveInventoryToFile(inventorySaveFileName);
            Debug.Log("Dane gry (ekwipunek) zapisane.");
        }
        else
        {
            Debug.LogError("Nie mo¿na zapisaæ danych gry: PlayerInventory nie jest przypisany do GameManager.");
        }
        // Tutaj w przysz³oœci mo¿esz dodaæ zapis innych danych gry (np. stan œwiata)
    }

    public void LoadGameData()
    {
        if (inventory != null)
        {
            bool loadedSuccessfully = inventory.LoadInventoryFromFile(inventorySaveFileName);
            if (!loadedSuccessfully)
            {
                Debug.Log("Nie znaleziono pliku zapisu ekwipunku lub wyst¹pi³ b³¹d. Inicjalizowanie nowego ekwipunku.");
                // U¿yj wartoœci domyœlnych lub skonfigurowanych w Inspektorze dla PlayerInventory,
                // jeœli ma w³asne pola dla maxSlots/stackLimits.
                // Lub przeka¿ je tutaj.
                inventory.InitializePlayerInventory(
                    maxSlotsTiles: 12, // Przyk³adowe wartoœci, mo¿esz je zdefiniowaæ w GameManagerze
                    maxSlotsItems: 24,
                    stackLimitTiles: 16,
                    stackLimitItems: 64
                );
            }
        }
        else
        {
            Debug.LogError("Nie mo¿na wczytaæ danych gry: PlayerInventory nie jest przypisany do GameManager.");
            // Awaryjnie, mo¿na by spróbowaæ znaleŸæ komponent, ale lepiej przypisaæ w Inspektorze
            // inventory = FindObjectOfType<PlayerInventory>();
            // if (inventory != null) LoadGameData(); // Spróbuj ponownie
        }
        // Tutaj w przysz³oœci mo¿esz dodaæ wczytywanie innych danych gry

        void Update()
        {
            // Przyk³adowe skróty do testowania (mo¿esz je usun¹æ lub przenieœæ)
            if (Input.GetKeyDown(KeyCode.F5)) // Klawisz do zapisu
            {
                SaveGameData();
            }
            if (Input.GetKeyDown(KeyCode.F9)) // Klawisz do wczytania (lepiej po restarcie gry)
            {
                // Ponowne wczytanie w tej samej sesji mo¿e byæ myl¹ce bez pe³nego resetu stanu.
                // Zazwyczaj LoadGameData jest wo³ane w Start().
                // Ale dla testów mo¿na:
                // LoadGameData();
            }
        }

        // Opcjonalnie: Obs³uga zapisu przy wyjœciu z gry
        void OnApplicationQuit()
        {
             SaveGameData(); // Rozwa¿, czy chcesz automatyczny zapis przy wyjœciu
        }
    }


}
