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
            Debug.LogError("Nie mo�na zapisa� danych gry: PlayerInventory nie jest przypisany do GameManager.");
        }
        // Tutaj w przysz�o�ci mo�esz doda� zapis innych danych gry (np. stan �wiata)
    }

    public void LoadGameData()
    {
        if (inventory != null)
        {
            bool loadedSuccessfully = inventory.LoadInventoryFromFile(inventorySaveFileName);
            if (!loadedSuccessfully)
            {
                Debug.Log("Nie znaleziono pliku zapisu ekwipunku lub wyst�pi� b��d. Inicjalizowanie nowego ekwipunku.");
                // U�yj warto�ci domy�lnych lub skonfigurowanych w Inspektorze dla PlayerInventory,
                // je�li ma w�asne pola dla maxSlots/stackLimits.
                // Lub przeka� je tutaj.
                inventory.InitializePlayerInventory(
                    maxSlotsTiles: 12, // Przyk�adowe warto�ci, mo�esz je zdefiniowa� w GameManagerze
                    maxSlotsItems: 24,
                    stackLimitTiles: 16,
                    stackLimitItems: 64
                );
            }
        }
        else
        {
            Debug.LogError("Nie mo�na wczyta� danych gry: PlayerInventory nie jest przypisany do GameManager.");
            // Awaryjnie, mo�na by spr�bowa� znale�� komponent, ale lepiej przypisa� w Inspektorze
            // inventory = FindObjectOfType<PlayerInventory>();
            // if (inventory != null) LoadGameData(); // Spr�buj ponownie
        }
        // Tutaj w przysz�o�ci mo�esz doda� wczytywanie innych danych gry

        void Update()
        {
            // Przyk�adowe skr�ty do testowania (mo�esz je usun�� lub przenie��)
            if (Input.GetKeyDown(KeyCode.F5)) // Klawisz do zapisu
            {
                SaveGameData();
            }
            if (Input.GetKeyDown(KeyCode.F9)) // Klawisz do wczytania (lepiej po restarcie gry)
            {
                // Ponowne wczytanie w tej samej sesji mo�e by� myl�ce bez pe�nego resetu stanu.
                // Zazwyczaj LoadGameData jest wo�ane w Start().
                // Ale dla test�w mo�na:
                // LoadGameData();
            }
        }

        // Opcjonalnie: Obs�uga zapisu przy wyj�ciu z gry
        void OnApplicationQuit()
        {
             SaveGameData(); // Rozwa�, czy chcesz automatyczny zapis przy wyj�ciu
        }
    }


}
