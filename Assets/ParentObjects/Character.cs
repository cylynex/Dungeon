using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Character {

    [Header("Basics")]
    public int characterID;
    public string characterName;
    public string baseClass;
    public string classType;
    public bool hasSpells;
    public bool hasAbilities;

    [Header("Experience")]
    public int currentLevel;
    public float currentExperience;
    public float expForNextLevel;

    [Header("Defense")]
    public float baseHitPoints;
    public float modifiedHitPoints;

    [Header("Resists")]
    public float resistPhysical;
    public float resistCold;
    public float resistFire;
    public float resistElectric;
    public float resistPoison;

    [Header("Offense")] // TODO this should be determined by weapon or spell.  Once we have weapons and spells.
    public float attackRange;
    public float damage;
    public float attackTime;
    public float attackTimer;
    public float meleeDamage;

    [Header("Movement")]
    public float speed = 1f;

    [Header("Primary Statistics")]
    public int strength;
    public int stamina;
    public int intelligence;
    public int dexterity;

    [Header("Weapon Mods")]
    public bool weaponPrimaryEquipped;
    public int weaponDamage;
    public int weaponDelay;

    [Header("Spells")]
    public Spell[] spells;
    public int spellbookID;
    public int spellDamageBonus;
    public List<Spell> activeSpells = new List<Spell>();
}
