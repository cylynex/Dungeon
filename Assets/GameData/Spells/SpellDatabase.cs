using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SpellDB", menuName = "Spells / Add Spell DB")]
public class SpellDatabase : ScriptableObject {

    public Spell[] spells;

    public Spell GetSpell(int spellID) {
        foreach (var spell in spells) {
            if (spell != null && spell.spellID == spellID) return spell;
        }
        return null;
    }
}
