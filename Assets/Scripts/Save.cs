using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System;

[System.Serializable]
public class SavedProfile {

    public Dictionary<string, float> playerStatistics = new Dictionary<string, float>();
    public List<int> dungeonCampsOpen = new List<int>();
    public List<DateTime> dungeonLockTimers = new List<DateTime>();
    public List<Character> characters = new List<Character>();
    public List<int> inventory = new List<int>();
    public List<int> spellBook = new List<int>();
    public int activeCharacterID;
    public int gold;
    public int characterSlots;

    // For Inventory Storage 
    public List<string> characterNames = new List<string>();
    public List<string> characterInventory = new List<string>();
    public List<string> characterEquippedGear = new List<string>();
    public List<string> characterSpells = new List<string>();
    string itemList;
    string spellList;

    // For dungeon lock timers 
    public List<string> openDungeons = new List<string>();
    public List<int> openCamps = new List<int>();
    public List<DateTime> lockTimers = new List<DateTime>();

}

public class Save : MonoBehaviour {

    public SavedProfile profile;

    public void SaveGame() {
        
        if (profile == null) {
            Debug.Log("creating new save profile");
            profile = new SavedProfile();
        }

        Debug.Log("saving game");
       
        // Player Statistics
        profile.playerStatistics = Statistics.playerStatistics;

        // Dungeon Camp Locks
        foreach (KeyValuePair<string,int> campsOpen in DungeonLocks.unlockedCamps) {
            profile.openDungeons.Add(campsOpen.Key);
            profile.openCamps.Add(DungeonLocks.unlockedCamps[campsOpen.Key]);
            //profile.lockTimers.Add(DungeonLocks.dungeonRespawnTimers[campsOpen.Key]);
            Debug.Log("Added: " + campsOpen.Key + " - " + DungeonLocks.unlockedCamps[campsOpen.Key]+" - "+DungeonLocks.dungeonRespawnTimers[campsOpen.Key]);
        }

        // Characters 
        GameManager.characters[GameManager.activeCharacterID] = Player.character;
        profile.characters = GameManager.characters;
        profile.activeCharacterID = GameManager.activeCharacterID;
        profile.characterSlots = GameManager.characterSlots;

        // Gold
        profile.gold = GameManager.gold;

        // Inventory 2
        // Loop through each character

        Debug.Log("starting character loop: total chars - " + GameManager.characters.Count);
        profile.characterNames.Clear();
        for (int i = 0; i < GameManager.characters.Count; i++) {
            string charName = GameManager.characters[i].characterName;
            profile.characterNames.Add(charName);

            // Init the item list for this item
            string itemList = "";
            string gearList = "";
            string equippedItemList = "";
            string spellList = "";

            // Loop through their bag inventory and construct the string to store their gear in.
            for (int g = 0; g < GameManager.inventories[charName].itemsInBag.Count; g++) {
                itemList += GameManager.inventories[charName].itemsInBag[g].itemID;
                if (g < GameManager.inventories[charName].itemsInBag.Count - 1) { itemList += ","; }
            }

            profile.characterInventory.Clear();
            profile.characterInventory.Add(itemList);

            // Loop through their equipped inventory and construct the string to store their gear in.

            for (int n = 0; n < GameManager.inventories[Player.character.characterName].gearSlots.Length; n++) {
                if (GameManager.inventories[charName].gearSlots[n] != null) {
                    equippedItemList += GameManager.inventories[charName].gearSlots[n].itemID;
                } else {
                    equippedItemList += "empty";
                }
                if (n < GameManager.inventories[charName].gearSlots.Length - 1) { equippedItemList += ","; }
            }

            Debug.Log("Gear List: " + equippedItemList);
            profile.characterEquippedGear.Clear();
            profile.characterEquippedGear.Add(equippedItemList);


            // Spellbook
            if (GameManager.characters[i].hasSpells) {
                Debug.Log(charName+" has this many spells: " + GameManager.spellbooks[charName].spell.Count);
                for (int s = 0; s < GameManager.spellbooks[charName].spell.Count; s++) {
                    spellList += GameManager.spellbooks[charName].spell[s].spellID;
                    Debug.Log("saving spell: " + GameManager.spellbooks[charName].spell[s].spellName);
                    if (s < GameManager.spellbooks[charName].spell.Count - 1) { spellList += ","; }
                }
            } else {
                profile.characterSpells.Clear();
                profile.characterSpells.Add("empty");
            }

            profile.characterSpells.Clear();
            profile.characterSpells.Add(spellList);

        }

       
        BinaryFormatter bf = new BinaryFormatter();

        string path = Application.persistentDataPath + "/save.dat";
        Debug.Log("path is: " + Application.persistentDataPath);

        if (File.Exists(path))
            File.Delete(path);

        FileStream fs = File.Open(path, FileMode.OpenOrCreate);
        bf.Serialize(fs, profile);

        fs.Close();
    }




    private void Update() {
        if (Input.GetKeyDown(KeyCode.S)) {
            //SaveGame();
            //Debug.Log("Game saved!");
        }
    }
}
