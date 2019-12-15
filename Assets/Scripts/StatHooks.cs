using UnityEngine;
using System.Collections;

public class StatHooks : MonoBehaviour {

    public static StatHooks instance;
    bool statsSetup = false;

    float bonus;
    int bonusHP;
    int intBonus;

    void Awake() {
        if (instance != null) { Debug.Log("multiple statHooks found"); return; } else { instance = this; }
    }


    // Setup the characters base stats and bonuses
    public void SetupBaseStats() {
        CalculateMeleeDamageBonuses();
        CalculateStaminaBonuses();
    }


    // Recalculates all bonuses based on the stats on the player character object.  This should recalculate ANYTHING that could effect the character.
    public void CalculateAllBonuses() {
        CalculateStaminaBonuses();
        CalculateIntelligenceBonus();
        CalculateMeleeDamageBonuses();
    }


    // Damage Bonuses Hook area TODO we will need weapon info / spell info as well once thats in
    /* Determinants:
     * 1: 1-1 Ratio of strength to + damage
     * 2: 1-2 Ratio of strength to + damage (weaker)
    */
    public void CalculateMeleeDamageBonuses() {

        // Reset to base damage first.  If there is a weapon equipped, it already did this when it was equipped so leave it alone.
        if (Player.character.weaponPrimaryEquipped == false) { Player.character.meleeDamage = Player.character.damage; } 
        else { Player.character.meleeDamage = Player.character.weaponDamage; }

        // Strength bonus
        switch (Player.character.baseClass) {
            case "Warrior":
                bonus = (int)Player.character.strength / 1; break;
            case "Cleric":
                bonus = (int)Player.character.strength / 2; break;
            case "Wizard":
                bonus = (int)Player.character.strength / 3; break;
        }

        Debug.Log("calculated bonus to be " + bonus);
        Debug.Log("base melee: " + Player.character.meleeDamage);

        if (bonus <= 0) { bonus = 0; }
        Player.character.meleeDamage += bonus;
    }


    // Calculate Stamina Bonus
    void CalculateStaminaBonuses() {

        // Reset HP to base first
        Player.character.modifiedHitPoints = Player.character.baseHitPoints;

        // Add appropriately
        switch (Player.character.baseClass) {
            case "Warrior":
                bonusHP = Player.character.stamina * 3; break;
            case "Cleric":
                bonusHP = Player.character.stamina * 2; break;
            case "Wizard":
                bonusHP = Player.character.stamina; break;
        }

        Player.character.modifiedHitPoints += bonusHP;
    }


    // Calculate Intelligence Bonus
    void CalculateIntelligenceBonus() {

        switch(Player.character.baseClass) {
            case "Cleric":
                intBonus = (int)Player.character.intelligence; break;
            case "Wizard":
                intBonus = (int)Player.character.intelligence * 2; break;
            case "Warrior":
                intBonus = 0; break;
        }

        if (intBonus <= 0) { intBonus = 0; }
        Player.character.spellDamageBonus = intBonus;
    }


    // Handle Strength Stat +/-
    public void AddBonusStrength(int amount) {
        Player.character.strength += amount;
    }
    public void SubtractBonusStrength(int amount) {
        Player.character.strength -= amount;
    }

    // Handle Intelligence Stat +/-
    public void AddBonusIntelligence(int amount) {
        Player.character.intelligence += amount;
        Debug.Log("char int is now: " + Player.character.intelligence);
    }
    public void SubtractBonusIntelligence(int amount) {
        Player.character.intelligence -= amount;
    }

    // Handle Stamina Stat +/-
    public void AddBonusStamina(int amount) {
        Player.character.stamina += amount;
    }
    public void SubtractBonusStamina(int amount) {
        Player.character.strength -= amount;
    }

    // Handle Dexterity Stat +/-
    public void AddBonusDexterity(int amount) {
        Player.character.dexterity += amount;
    }
    public void SubtractBonusDexterity(int amount) {
        Player.character.dexterity -= amount;
    }


    // Remove Stat Bonuses(When Unequipping an item)
    public void RemoveBonuses(Item removedItem) {

        // Resists
        Player.character.resistPhysical -= removedItem.resistPhysical;
        Player.character.resistCold -= removedItem.resistCold;
        Player.character.resistFire -= removedItem.resistFire;
        Player.character.resistElectric -= removedItem.resistElectric;
        Player.character.resistPoison -= removedItem.resistPoison;

        // Stats
        SubtractBonusStrength(removedItem.itemBonusStrength);
        SubtractBonusStamina(removedItem.itemBonusStamina);
        SubtractBonusIntelligence(removedItem.itemBonusIntelligence);
        SubtractBonusDexterity(removedItem.itemBonusDexterity);
    }


    // Add Bonuses (When equipping an item)
    public void AddBonuses(Item equippedItem) {

        Debug.Log("equipped item: " + equippedItem.itemName);

        // Resists
        Player.character.resistPhysical += equippedItem.resistPhysical;
        Player.character.resistCold += equippedItem.resistCold;
        Player.character.resistFire += equippedItem.resistFire;
        Player.character.resistElectric += equippedItem.resistElectric;
        Player.character.resistPoison += equippedItem.resistPoison;

        // Stats
        AddBonusStrength(equippedItem.itemBonusStrength);
        AddBonusStamina(equippedItem.itemBonusStamina);
        AddBonusIntelligence(equippedItem.itemBonusIntelligence);
        AddBonusDexterity(equippedItem.itemBonusDexterity);
    }
}