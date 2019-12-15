using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System;


public class Load : MonoBehaviour {

    GameObject gameManager;

    public List<string> charNames = new List<string>();
    public List<string> charGear = new List<string>();
    Spell thisSpell;

    void Awake() {

        gameManager = GameObject.FindGameObjectWithTag("GameController");
        if (Statistics.isGameLoaded == false) {
            Debug.Log("Game is not loaded - load it now");
            LoadGame();
            Statistics.isGameLoaded = true;
        } else {
            Debug.Log("game is already loaded");
        }

    }

    private void LoadGame() {
        string pathToLoad = Application.persistentDataPath + "/save.dat";
        if (!File.Exists(pathToLoad)) {
            Debug.Log("No saved profile found! Create new game.  Do stuff that hasnt been done yet here.");
            gameManager.GetComponent<Statistics>().CreateNewGame();

            // Init character slots
            GameManager.characterSlots = 2;

            // Start the new character process
            SceneManager.LoadScene("Character Creation");

        } else {
            // Found save file - load
            Debug.Log("loading game" + Application.persistentDataPath);
            BinaryFormatter bf = new BinaryFormatter();
            FileStream fs = File.Open(pathToLoad, FileMode.Open);
            SavedProfile loadedProfile = bf.Deserialize(fs) as SavedProfile;
            fs.Close();

            // Characters
            GameManager.characters = loadedProfile.characters;
            GameManager.activeCharacterID = loadedProfile.activeCharacterID;
            Player.character = GameManager.characters[GameManager.activeCharacterID]; // Set active char in player locally
            GameManager.characterSlots = loadedProfile.characterSlots;

            // Gold
            GameManager.gold = loadedProfile.gold;

            // Inventory & Equipped Gear
            for (int x = 0; x < loadedProfile.characterNames.Count; x++) {


                // Setup Load Variables
                string currentCharacterName = loadedProfile.characterNames[x];
                Inventory loadCharInventory = new Inventory();

                string tempInventory = loadedProfile.characterInventory[x];
                string[] arrayOfItems = loadedProfile.characterInventory[x].Split(","[0]);
                int outputItem;

                // Inventory Section
                if (arrayOfItems.Length > 0) {
                    
                    for (int i = 0; i < arrayOfItems.Length; i++) {
                        int.TryParse(arrayOfItems[i], out outputItem);
                        //Item thisItem = GameManager.instance.itemDB.GetItem(outputItem);
                        Item thisItem = GetComponent<GameManager>().itemDB.GetItem(outputItem);
                        loadCharInventory.itemsInBag.Add(thisItem);
                    }
                }

                // Equipped Gear Section
                string tempEquipped = loadedProfile.characterEquippedGear[x];
                string[] arrayOfGear = loadedProfile.characterEquippedGear[x].Split(","[0]);
                int outputGear;
                string currentClass = "";
                bool hasSpells = false;

                for (int i = 0; i < arrayOfGear.Length; i++) {
                    if (arrayOfGear[i] != "empty") {
                        int.TryParse(arrayOfGear[i], out outputItem);
                        //Item thisItem = GameManager.instance.itemDB.GetItem(outputItem);
                        Item thisItem = GetComponent<GameManager>().itemDB.GetItem(outputItem);
                        loadCharInventory.gearSlots[i] = thisItem;
                    } else {
                        // Do nothing, that slot is empty,  dont actually have to make it null
                    }
                }

                // Dump the inventory to the characters active inventory
                GameManager.inventories.Add(currentCharacterName, loadCharInventory);

                // Spellbooks
                Spellbook loadCharSpellbook = new Spellbook();
                string tempSpellbook = loadedProfile.characterSpells[x];
                string[] arrayOfSpells = loadedProfile.characterSpells[x].Split(","[0]);
                int outputSpell;

                // Need the class for this to work TODO make this more efficient
                for (int c = 0; c < GameManager.characters.Count; c++) {
                    if (GameManager.characters[c].characterName == currentCharacterName) {
                        currentClass = GameManager.characters[c].baseClass;
                        hasSpells = GameManager.characters[c].hasSpells;
                    }
                }

                for (int i = 0; i < arrayOfSpells.Length; i++) {
                    if (arrayOfSpells[i] != "empty") {
                        int.TryParse(arrayOfSpells[i], out outputSpell);
                        switch(currentClass) {
                            case "Wizard":
                                //thisSpell = GameManager.instance.wizardSpells.GetSpell(outputSpell);
                                thisSpell = GetComponent<GameManager>().wizardSpells.GetSpell(outputSpell);
                                loadCharSpellbook.spell.Add(thisSpell);
                                Debug.Log("Loading spell: " + thisSpell.spellName);
                                break;
                            case "Cleric":
                                //thisSpell = GameManager.instance.clericSpells.GetSpell(outputSpell);
                                thisSpell = GetComponent<GameManager>().clericSpells.GetSpell(outputSpell);
                                loadCharSpellbook.spell.Add(thisSpell);
                                Debug.Log("Loading spell: " + thisSpell.spellName);
                                break;
                        }
                    } else {
                        // Do nothing, that slot is empty,  dont actually have to make it null
                        Debug.Log("this character has no spells to load.");
                    }
                }

                // Dump the spellbook to the GM if its a casting class only
                if (hasSpells) { 
                    Debug.Log("spell class found - load spellbook");
                    GameManager.spellbooks.Add(currentCharacterName, loadCharSpellbook); 
                }
            }

            Debug.Log("finished loading spells.  Spellbooks: " + GameManager.spellbooks.Count);

            // Player Statistics
            Statistics.playerStatistics = loadedProfile.playerStatistics;
            Statistics.isGameLoaded = true;

            // Dungeon Locks
            for (int i = 0; i < loadedProfile.openDungeons.Count; i++) {
                DungeonLocks.unlockedCamps.Add(loadedProfile.openDungeons[i],loadedProfile.openCamps[i]);
                DungeonLocks.dungeonRespawnTimers.Add(loadedProfile.openDungeons[i], loadedProfile.lockTimers[i]);
                Debug.Log("loaded: " + loadedProfile.openDungeons[i] + " - " + loadedProfile.openCamps[i] + " - " + loadedProfile.lockTimers[i]);
            }
                
        }
    }


    // Setup the load listener
    public void LoadedGameStart() {
        /*
        if (Player.isDocked) {
            SceneManager.LoadScene("Starport");
        } else {
            SceneManager.LoadScene("Game");
        }
        */
    }

}
