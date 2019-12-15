using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadPopup : MonoBehaviour {

    public GameObject spellbook;

    public void LoadSpellbook() {
        spellbook.SetActive(true);
    }
}
