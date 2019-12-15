using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Spells", menuName = "Spells / Add Spell")]
public class Spell : ScriptableObject {

    public int spellID;
    public string spellName;
    public GameObject spellPrefab;

    public bool healSpell;
    public bool ddSpell;

    [Header("DD Spells That Travel")]
    public float spellMoveSpeed;
    public float spellExplosionRadius; // AE on Hit
    public int spellDamage;
    public GameObject spellImpactEffect;

    [Header("Heal Spells")]
    public int amountToHeal;
    public bool selfonly;

    [Header("Mods from the character")]
    public int spellDamageBonus;
}
