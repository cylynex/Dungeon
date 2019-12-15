using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName="Item",menuName="Items / Add Item")]
public class Item : ScriptableObject {
	
    [Header("Item Specifics")]
    public int itemID;
    public string itemName;
    public Sprite itemIcon;
    public int slotID;

    [Header("Types")]
    public bool isEquippable;
    public bool isConsumable;
    public bool isArmor;
    public bool isWeapon;
    public bool isQuestItemNotCrap;

    [Header("Weapons")]
    public int damage;
    public int delay;

    [Header("Stat Bonuses")]
    public int itemBonusStrength;
    public int itemBonusStamina;
    public int itemBonusIntelligence;
    public int itemBonusDexterity;

    [Header("Resists")]
    public float resistPhysical;
    public float resistCold;
    public float resistFire;
    public float resistElectric;
    public float resistPoison;

    [Header("Internal Only")]
    public int quantity;
}
