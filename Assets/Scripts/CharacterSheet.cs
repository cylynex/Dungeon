using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CharacterSheet : MonoBehaviour {

    public static CharacterSheet instance;

    public Transform statsWindow;
    public Transform statsWindow2;
    public Transform combatStats;
    public GameObject statDisplay;
    public Text charNameClassLevel;

    GameObject thisStat;
    Text[] tdata;

    // Inventory stuff TODO move to its own class
    [Header("Inventory Stuff")]
    public int inventoryNumberSlots;
    public Transform inventoryHolder;
    public GameObject inventoryPrefab;
    public Text goldDisplay;
    InventorySlot[] invSlot; // Inventory Slot items

    [Header("Inventory Detail Panel")]
    public Transform invDetailScreen;
    public Transform invButtonHolder;
    public GameObject invEquipButtonPrefab;
    public GameObject unEquipButtonPrefab;
    public GameObject invDestroyButtonPrefab;
    public Image invDetailIcon;
    public Text invDetailText;

    [Header("Resists")]
    public Text physicalResist;
    public Text coldResist;
    public Text fireResist;
    public Text electricResist;
    public Text poisonResist;

    [Header("Equipped Slots")]
    public GameObject[] equippedGearSlot;

    void Awake() {
        if (instance != null) { Debug.Log("multiple charsheet found"); return; } else { instance = this; }
    }

    void Start() {

        Scene currentScene = SceneManager.GetActiveScene();
        if (currentScene.name == "Character") {
            ShowCharacterSheet();
        }

    }


    void ShowCharacterSheet() {

        invDetailScreen.gameObject.SetActive(false);
        LoadStats();

        // Number of inventory slots.  TODO make this dynamic and part of the player somewhere.  also serialize it.
        inventoryNumberSlots = 36;

        // Display the inventory slots based on number of slots
        SetupInventorySlots();

        // Populate the inventory slots with players inventory
        FillInventory();

        // Show equipped gear
        EquippedDisplay();

        // Display Gold
        DisplayGold();

        // Debug shit
        Debug.Log("Prepared spell: " + GameManager.preparedSpells[0].spellName);
    }


    public void LoadStats() {

        // Clear all children in case its a reload
        foreach (Transform child in statsWindow) { GameObject.Destroy(child.gameObject); }
        foreach (Transform child in statsWindow2) { GameObject.Destroy(child.gameObject); }
        foreach (Transform child in combatStats) { GameObject.Destroy(child.gameObject); }

        // Populate character name level class
        charNameClassLevel.text = Player.character.characterName + " - Level " + Player.character.currentLevel + " " + Player.character.baseClass;

        // TODO make this into a loop somehow.  Ok make it better still room to fix.

        CreateStatBox(statsWindow);
        tdata[0].text = "Class";
        tdata[1].text = Player.character.baseClass.ToString(); 
        tdata[0].color = Color.red;
        tdata[1].color = Color.red;

        CreateStatBox(statsWindow);
        tdata[0].text = "Hit Points";
        tdata[1].text = Player.character.modifiedHitPoints.ToString() + "(" + Player.character.baseHitPoints.ToString() + ")";

        CreateStatBox(statsWindow);
        tdata[0].text = "Movement";
        tdata[1].text = Player.character.speed.ToString(); 

        CreateStatBox(statsWindow2);
        tdata[0].text = "Strength";
        tdata[1].text = Player.character.strength.ToString(); 

        CreateStatBox(statsWindow2);
        tdata[0].text = "Stamina";
        tdata[1].text = Player.character.stamina.ToString(); 

        CreateStatBox(statsWindow2);
        tdata[0].text = "Intelligence";
        tdata[1].text = Player.character.intelligence.ToString(); 

        CreateStatBox(statsWindow2);
        tdata[0].text = "Dexterity";
        tdata[1].text = Player.character.dexterity.ToString();

        CreateStatBox(combatStats); // TODO make this from weapon not character
        tdata[0].text = "Unarmed Damage";
        tdata[1].text = Player.character.damage.ToString(); 

        CreateStatBox(combatStats); // TODO make this from weapon not character
        tdata[0].text = "Melee Damage";
        tdata[1].text = Player.character.meleeDamage.ToString();

        // Resists
        physicalResist.text = Player.character.resistPhysical.ToString();
        coldResist.text = Player.character.resistCold.ToString();
        fireResist.text = Player.character.resistFire.ToString();
        electricResist.text = Player.character.resistElectric.ToString();
        poisonResist.text = Player.character.resistPoison.ToString();

    }


    void CreateStatBox(Transform parent) {
        thisStat = (GameObject)Instantiate(statDisplay, statsWindow.position, Quaternion.identity);
        thisStat.transform.SetParent(parent, false);
        tdata = thisStat.GetComponentsInChildren<Text>();
    }


    // Display Inventory
    void SetupInventorySlots() {
        for (int i = 0; i < inventoryNumberSlots; i++) {
            GameObject thisItem = (GameObject)Instantiate(inventoryPrefab, inventoryHolder.position, Quaternion.identity);
            thisItem.transform.SetParent(inventoryHolder, false);
            thisItem.GetComponent<InventorySlot>().slotID = i;
        }
    }


    // Put players items into inventory
    void FillInventory() {
        invSlot = inventoryHolder.GetComponentsInChildren<InventorySlot>();
        for (int i = 0; i < GameManager.inventories[Player.character.characterName].itemsInBag.Count; i++) {
            int slotID = i;
            invSlot[i].AddItemToInventorySlot(GameManager.inventories[Player.character.characterName].itemsInBag[i], slotID);
        }
    }


    // Display gold
    void DisplayGold() {
        goldDisplay.text = GameManager.gold.ToString();
    }


    // Show item details when clicked from inventory - SlotTouse matches both the inventory slot ID and the inventory slot id
    public void ShowItemDetails(Item item, int slotToUse, bool alreadyEquipped) {
        
        // Remove the old Item buttons
        foreach (Transform child in invButtonHolder) {
            Destroy(child.gameObject);
        }

        // Populate the data
        invDetailScreen.gameObject.active = true;

        // Item Text and stats etc
        string itemDetails;
        itemDetails = "<b>" + item.itemName + "</b> (Slot "+slotToUse+")\r\n";

        if (item.isQuestItemNotCrap) { itemDetails += "Quest Item (Not Crap!)\r\n"; }
        if (item.isWeapon) { itemDetails += "Weapon - Damage: " + item.damage + "  Delay: " + item.delay+"\r\n"; }
        if (item.isArmor) { itemDetails += "Armor\r\n"; }
        if (item.isConsumable) { itemDetails += "Consumable (no bonus yet)\r\n"; }

        itemDetails += "Slot: " + CharacterSheet.instance.equippedGearSlot[item.slotID].name + "\r\n";

        itemDetails += "<color=#0000ff>HP +0 Mana +0</color>";

        invDetailIcon.sprite = item.itemIcon;

        string statsText = "";
        if (item.itemBonusStrength > 0) { statsText += "STR: " + item.itemBonusStrength; }
        if (item.itemBonusStamina > 0) { statsText += " STA: " + item.itemBonusStamina; }
        if (item.itemBonusIntelligence > 0) { statsText += " INT: " + item.itemBonusIntelligence; }
        if (item.itemBonusDexterity > 0) { statsText += " DEX: " + item.itemBonusDexterity; }
        if (statsText != "") { itemDetails += "\r\n" + statsText; }

        string resistText = "";
        if (item.resistPhysical > 0) { resistText += "RPh: " + item.resistPhysical; }
        if (item.resistCold > 0) { resistText += " RC: " + item.resistCold; }
        if (item.resistFire > 0) { resistText += " RF: " + item.resistFire; }
        if (item.resistPoison > 0) { resistText += " RPo: " + item.resistPoison; }
        if (item.resistElectric > 0) { resistText += " RE: " + item.resistElectric; }

        if (resistText != "") { itemDetails += "\r\n" + resistText; }

        invDetailText.text = itemDetails;

        Debug.Log("is this the right item? " + item.itemName);
        Debug.Log("Equippable status of this item: " + item.isEquippable);

        // Buttons
        Debug.Log("making the show button.  Is this item already equipped? " + alreadyEquipped);
        if (item.isEquippable) {
            if (alreadyEquipped) {
                GameObject unEquipButton = Instantiate(unEquipButtonPrefab, invButtonHolder.position, Quaternion.identity);
                unEquipButton.transform.SetParent(invButtonHolder, false);
                unEquipButton.GetComponent<Button>().onClick.AddListener(() => { UnequipItem(slotToUse); });
            } else {
                GameObject equipButton = Instantiate(invEquipButtonPrefab, invButtonHolder.position, Quaternion.identity);
                equipButton.transform.SetParent(invButtonHolder, false);
                equipButton.GetComponent<Button>().onClick.AddListener(() => { EquipItem(item, slotToUse); });
            }
        }

        GameObject destroyButton = Instantiate(invDestroyButtonPrefab, invButtonHolder.position, Quaternion.identity);
        destroyButton.transform.SetParent(invButtonHolder, false);
        int islotID = slotToUse;
        destroyButton.GetComponent<Button>().onClick.AddListener(() => { DestroyItem(islotID); });

    }


    // Equip an Item
    void EquipItem(Item item, int inventorySlotID) {
        Debug.Log("attempting to put gear (" + item.itemName + ") from inventory slot " + inventorySlotID + " to equip slot: " + item.slotID);

        // Check if slot is empty
        if (GameManager.inventories[Player.character.characterName].gearSlots[item.slotID] == null) {
            // It's empty - just add the item to the slot
            AddItemToSlot(item);

            // Remove that item from the inventory
            RemoveItemFromInventory(inventorySlotID);
        } else {
            // There's something in the slot
            // Move that item to inventory
            MoveEquippedItemToInventory(item.slotID);

            // Replace that item with the new item
            AddItemToSlot(item);
        }

        // Reload display
        StatHooks.instance.CalculateAllBonuses();
        ResetSlots();
        FillInventory();
        EquippedDisplay();
        ReloadScene();
    }


    void UnequipItem(int slotID) {
        MoveEquippedItemToInventory(slotID);

        // Reload display
        StatHooks.instance.CalculateAllBonuses();
        ResetSlots();
        FillInventory();
        EquippedDisplay();
        ReloadScene();
    }


    void ReloadScene() {
        GameManager.instance.GetComponent<Save>().SaveGame();
        SceneManager.LoadScene("Character");
    }


    // Add Item to a equipped slot
    void AddItemToSlot(Item itemToEquip) {
        Debug.Log("adding item to a slot now");
        GameManager.inventories[Player.character.characterName].gearSlots[itemToEquip.slotID] = itemToEquip;

        if (itemToEquip.slotID == 0) { EquipPrimaryWeapon(itemToEquip); }

        // Add Bonuses
        StatHooks.instance.AddBonuses(itemToEquip);
    }


    // Remove an Item from inventory
    void RemoveItemFromInventory(int inventorySlotID) {
        Debug.Log("removing an item from inventory now");
        GameManager.inventories[Player.character.characterName].itemsInBag.RemoveAt(inventorySlotID);
        ReloadScene();
    }


    // Move an equipped item to inventory (since its being unequipped)
    void MoveEquippedItemToInventory(int equipSlotID) {
        Debug.Log("moving item from equipped to inventory now");
        Item tempItem = GameManager.inventories[Player.character.characterName].gearSlots[equipSlotID];

        GameManager.inventories[Player.character.characterName].gearSlots[equipSlotID] = null;
        Image[] slotImage = equippedGearSlot[equipSlotID].GetComponentsInChildren<Image>();
        slotImage[2].sprite = null;
        var tempColor = slotImage[2].color;
        tempColor.a = 0f;
        slotImage[2].color = tempColor;

        AddItemToInventory(tempItem);

        if (equipSlotID == 0) { UnequipPrimaryWeapon(); }

        // Remove Bonuses
        StatHooks.instance.RemoveBonuses(tempItem);

    }


    // Add Item to inventory
    public void AddItemToInventory(Item item) {
        Debug.Log("adding item to inventory CHARSHEET SIDE "+item.itemName+" - equip? "+item.isEquippable);
        //GameManager.inventories[GameManager.activeCharacterID].itemsInBag.Add(item);
        GameManager.inventories[Player.character.characterName].itemsInBag.Add(item);
    }


    // Reset Inventory slot display
    void ResetSlots() {
        Debug.Log("resetting all inventory slots");         for (int i = 0; i < invSlot.Length; i++) {             invSlot[i].ClearItem();         }     }


    // The equipped gear display
    void EquippedDisplay() {
        for (int i = 0; i < equippedGearSlot.Length; i++) {
            if (GameManager.inventories[Player.character.characterName].gearSlots[i] != null) {
                Image[] gearImage = equippedGearSlot[i].GetComponentsInChildren<Image>();
                gearImage[2].sprite = GameManager.inventories[Player.character.characterName].gearSlots[i].itemIcon;
                var tempColor = gearImage[2].color;
                tempColor.a = 1f;
                gearImage[2].color = tempColor;
            }
        }
    }


    // Destroy Item TODO this only works for inventory destroys not equipped
    void DestroyItem(int emptySlot) {
        //GameManager.inventories[GameManager.activeCharacterID].itemsInBag.RemoveAt(emptySlot);
        GameManager.inventories[Player.character.characterName].itemsInBag.RemoveAt(emptySlot);         invSlot[emptySlot].ClearItem();         invDetailScreen.gameObject.active = false;         ResetSlots();         FillInventory();         GameManager.instance.GetComponent<Save>().SaveGame();     } 

    // Equip Weapon Stuff
    void EquipPrimaryWeapon(Item item) {         Player.character.weaponPrimaryEquipped = true;         Player.character.weaponDamage = item.damage;         Player.character.weaponDelay = item.delay;
        Player.character.attackTime = item.delay;         Player.character.meleeDamage = item.damage;     }

    // UnEquip Weapon Stuff
    void UnequipPrimaryWeapon() {
        Player.character.weaponPrimaryEquipped = false;
        Player.character.weaponDamage = 0;
        Player.character.weaponDelay = 0;
        Player.character.attackTime = Player.character.attackTime;
        Player.character.meleeDamage = Player.character.damage;
    }
}