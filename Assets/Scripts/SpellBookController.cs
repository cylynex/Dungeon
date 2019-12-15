using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpellBookController : MonoBehaviour {

    public Transform spellList;
    public Transform activeSpellList;
    public GameObject spellPrefab;

    void Start() {
        ShowAllSpells();
    }


    void ShowAllSpells() {
        Debug.Log("spells in the book: " + GameManager.spellbooks[Player.character.characterName].spell.Count);

        for (int i = 0; i < GameManager.spellbooks[Player.character.characterName].spell.Count; i++) {
            Spell thisSpell = GameManager.spellbooks[Player.character.characterName].spell[i];
            GameObject spellSpot = Instantiate(spellPrefab, spellList.position, Quaternion.identity);
            spellSpot.transform.SetParent(spellList, false);
            Text[] spellText = spellSpot.GetComponentsInChildren<Text>();
            spellText[0].text = thisSpell.spellName;
            spellText[1].text = "DMG: "+thisSpell.spellDamage.ToString();
            spellText[2].text = "Range: "+thisSpell.spellExplosionRadius.ToString();
        }
    }

}