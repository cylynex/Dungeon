using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CreateCharacter : MonoBehaviour {

    public GameObject characterClassPrefab;
    public Transform classHolder;

    public Text classSelection;
    public Text classDescription;
    public Image classImage;
    public Text classStatData;
    public Button classCreateButton;

    public List<CharacterClass> characterClasses = new List<CharacterClass>();

    Spellbook newSpellBook;

    string charName;

    void Start() {
        classImage.enabled = false;
        classCreateButton.enabled = false;

        Debug.Log("classes: " + characterClasses.Count);

        for (int i = 0; i < characterClasses.Count; i++) {
            GameObject thisClass = (GameObject)Instantiate(characterClassPrefab, classHolder.position, Quaternion.identity);
            thisClass.transform.SetParent(classHolder,false);

            thisClass.GetComponentInChildren<Image>().sprite = characterClasses[i].classImage;
            thisClass.GetComponentInChildren<Text>().text = characterClasses[i].className;

            Button activeButton = thisClass.GetComponent<Button>();
            CharacterClass selectedClass = characterClasses[i];
            activeButton.onClick.AddListener(() => { SelectClass(selectedClass); });


        }
    }


    // Button Actions
    void SelectClass(CharacterClass selectedClass) {
        classSelection.text = selectedClass.className;
        classDescription.text = selectedClass.classDescription;
        classImage.enabled = true;
        classImage.sprite = selectedClass.classImage;
        classStatData.text = "HP: " + selectedClass.baseHitPoints + "\r\n";
        classStatData.text += "Range: " + selectedClass.attackRange + "\r\n";
        classStatData.text += "Damage: " + selectedClass.damage + "\r\n";
        classStatData.text += "Attack Time: " + selectedClass.attackTime + "\r\n";
        classStatData.text += "Movement: " + selectedClass.speed + "\r\n";

        // Setup button
        classCreateButton.enabled = true;
        classCreateButton.onClick.AddListener(() => { CreateClass(selectedClass); });
    }


    void CreateClass(CharacterClass selectedClass) {

        Debug.Log("START CREATE HERE");

        //string characterName = GameObject.Find("InputField").GetComponent<InputField>().text;
        string characterName = GameObject.FindGameObjectWithTag("CreationNameField").GetComponent<Text>().text;

        // Create the object and give it values from the class template
        Character newChar = new Character();

        // Name and Class
        //newChar.characterName = charName;
        newChar.characterName = characterName;
        newChar.baseClass = selectedClass.className;
        newChar.classType = selectedClass.classType;
        newChar.hasSpells = selectedClass.hasSpells;
        newChar.hasAbilities = selectedClass.hasAbilities;

        // Stats
        newChar.baseHitPoints = selectedClass.baseHitPoints;
        newChar.modifiedHitPoints = selectedClass.baseHitPoints;
        newChar.speed = selectedClass.speed;
        newChar.strength = selectedClass.strength;
        newChar.intelligence = selectedClass.intelligence;
        newChar.dexterity = selectedClass.dexterity;
        newChar.stamina = selectedClass.stamina;

        // Off / Def TODO this is all gonna move to armor and weapons later.  Just here for now since we dont have either yet.
        newChar.attackRange = selectedClass.attackRange;
        newChar.damage = selectedClass.damage;
        newChar.attackTime = selectedClass.attackTime;

        // Setup Exp Values.  Don't need to but why not be thorough.
        newChar.currentExperience = 0;
        newChar.currentLevel = 1;
        newChar.expForNextLevel = GameManager.instance.experiencePerLevel[1];

        // Make the new character active while we're at it.
        Player.character = newChar;

        // Calculate starting stat bonuses
        StatHooks.instance.SetupBaseStats();

        // Give player starting spells
        if (selectedClass.hasSpells) {
            newSpellBook = new Spellbook();

            // Add starting spells
            for (int i = 0; i < selectedClass.spells.Length; i++) {
                Debug.Log("adding spell: " + selectedClass.spells[i]);
                newSpellBook.spell.Add(selectedClass.spells[i]);

                // Add the first spell to the prepared spells list
                if (i == 0) {
                    GameManager.preparedSpells.Add(selectedClass.spells[i]);
                }
            }

            // Add the spellbook we just made for this character to the main inventory
            GameManager.instance.AddSpellBook(Player.character.characterName, newSpellBook);


        }

        // Create an inventory for the character and match the ID to the character.  Then give them any newbie items for their class
        Inventory newCharInventory = new Inventory();
        for (int i = 0; i < selectedClass.startingItems.Length; i++) {
            newCharInventory.itemsInBag.Add(selectedClass.startingItems[i]);
        }
        GameManager.inventories.Add(Player.character.characterName, newCharInventory);

        // Insert it into the player's character List 
        GameManager.instance.AddNewCharacter(newChar);

        Debug.Log("END CREATE HERE");

        // Go to the map // TODO probly should take you to character screen here.
        SceneManager.LoadScene("WorldMap");

    }

}
