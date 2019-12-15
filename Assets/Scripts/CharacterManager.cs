using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CharacterManager : MonoBehaviour {

    public Transform characterHolder;
    public GameObject characterPrefab;
    public Text slotsUsedText;
    public Text addCharacterButton;

    void Start() {
        Debug.Log("characters found: " + GameManager.characters.Count);

        ShowCharacters();

        ShowUsedCharacterSlots();

        ShowAddButton();
    }


    void ShowCharacters() {
        for (int i = 0; i < GameManager.characters.Count; i++) {
            GameObject thisCharacter = (GameObject)Instantiate(characterPrefab, characterHolder.position, Quaternion.identity);
            thisCharacter.transform.SetParent(characterHolder, false);
            Debug.Log("char name: " + GameManager.characters[i].characterName);
            Text[] thisCharText = thisCharacter.GetComponentsInChildren<Text>();
            thisCharText[0].text = GameManager.characters[i].characterName;
            thisCharText[1].text = "Level " + GameManager.characters[i].currentLevel + " " + GameManager.characters[i].baseClass;

            Button[] characterActionButton = thisCharacter.GetComponentsInChildren<Button>();
            int thisCharID = i;

            if (GameManager.activeCharacterID == thisCharID) {
                thisCharText[3].text = "Current";
            } else {
                characterActionButton[0].onClick.AddListener(() => { MakeCharacterActive(thisCharID); });
            }

            characterActionButton[1].onClick.AddListener(() => { DeleteCharacter(thisCharID); });

        }
    }


    // Activate  a character
    void MakeCharacterActive(int charID) {
        GameManager.activeCharacterID = charID;
        Player.character = GameManager.characters[charID];
        SceneManager.LoadScene("CharacterManagement");
        Player.character.expForNextLevel = GameManager.instance.experiencePerLevel[Player.character.currentLevel];
    }


    // Delete a Character
    void DeleteCharacter(int charID) {

        int errorToDelete = 0;

        // Make sure it's not the only character
        if (GameManager.characters.Count == 1) {
            errorToDelete++;
            Debug.Log("can't delete it's their only character");
        }

        // Make sure its not the active character
        if (GameManager.activeCharacterID == charID) {
            errorToDelete++;
            Debug.Log("can't delete it's the active character");
        }

        // Delete the character
        if (errorToDelete == 0) {
            Debug.Log("ok to delete, doing so");
            GameManager.characters.RemoveAt(charID);
            SceneManager.LoadScene("CharacterManagement");
        }

    }


    void ShowUsedCharacterSlots() {
        string slotLine = "Slots Used: " + GameManager.characters.Count + " / " + GameManager.characterSlots;
        slotsUsedText.text = slotLine;

    }


    void ShowAddButton() {
        if (GameManager.characters.Count >= GameManager.characterSlots) {
            addCharacterButton.text = "All Slots Used";
        } else {
            addCharacterButton.text = "Add Character";
            addCharacterButton.gameObject.AddComponent<Button>();
            Button addButton = addCharacterButton.GetComponent<Button>();
            addButton.onClick.AddListener(() => { SceneManager.LoadScene("Character Creation"); });
        }
    }

}