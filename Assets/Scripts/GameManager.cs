using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {

    public static GameManager instance;
    public static List<Character> characters = new List<Character>();
    public static int activeCharacterID;
    public static int gold;
    public static int characterSlots;
    public ItemDatabase itemDB;
    public SpellDatabase wizardSpells;
    public SpellDatabase clericSpells;
    public float[] experiencePerLevel;

    // Spellbooks
    public static Dictionary<string, Spellbook> spellbooks = new Dictionary<string, Spellbook>();
    public static List<Spell> preparedSpells = new List<Spell>();

    // Inventory
    public static Dictionary<string, Inventory> inventories = new Dictionary<string, Inventory>();


    void Awake() {
        if (instance != null) { Debug.Log("multiple inventory found"); return; } else { instance = this; }
        Item thisItem = instance.itemDB.GetItem(2);
        Debug.Log("GM CHECK: " + thisItem);
    }

   
    // Add a new character
    public void AddNewCharacter(Character newChar) {
        Debug.Log("adding: " + newChar.characterName);

        // Insert into list
        characters.Add(newChar);

        // Make the character we just made active
        activeCharacterID = characters.Count - 1;

        // Save Game
        GetComponent<Save>().SaveGame();

    }


    // Add spellbook
    public void AddSpellBook(string characterName, Spellbook spellbookToAdd) {
        Debug.Log("GM Adding spellbook now");
        spellbooks.Add(characterName, spellbookToAdd);
    }


    // Convert an item into an inventory item
    public Item ItemToInventoryItem(Item item) {
        Item inventoryItem = new Item();
        inventoryItem.isArmor = item.isArmor;
        inventoryItem.isConsumable = item.isConsumable;
        inventoryItem.isQuestItemNotCrap = item.isQuestItemNotCrap;
        inventoryItem.isWeapon = item.isWeapon;
        inventoryItem.itemIcon = item.itemIcon;
        inventoryItem.itemName = item.itemName;
        inventoryItem.itemID = item.itemID;
        return inventoryItem;
    }





}
