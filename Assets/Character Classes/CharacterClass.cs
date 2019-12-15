using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "CharacterClass", menuName="Add Character Class")]
public class CharacterClass : ScriptableObject {
	
    public string className;
    public Sprite classImage;
    public string classType;
    [TextArea(5,5)]
    public string classDescription;
    public bool hasSpells;
    public bool hasAbilities;

    [Header("Defense")]
    public float baseHitPoints;

    [Header("Offense")] // TODO this should be determined by weapon or spell.  Once we have weapons and spells.
    public float attackRange;
    public float damage;
    public float attackTime;

    [Header("Movement")]
    public float speed = 1f;

    [Header("Primary Statistics")]
    public int strength;
    public int stamina;
    public int intelligence;
    public int dexterity;

    [Header("Spells")]
    public Spell[] spells;

    [Header("Starting Items")]
    public Item[] startingItems;

}