using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Inventory {

    public string characterName;

    // Gear in Slots
    /* Slot Key
     * 0: Primary Hand
     * 1: Secondary Hand
     * 2: Gloves
     * 3: Helm
     * 4: Coif
     * 5: Breastplate
     * 6: Belt
     * 7: Leggings
     * 8: Boots
     * 9: Left Bracer
     * 10: Right Bracer
     * 11: Arms
     * 12: Gloves
     * 13: Trinket
     * 14: TBA
     * 15: Main Ring
     * 16: Necklace
     * 17: Earrings
     * 18: Pinky Ring
    */
    public Item[] gearSlots = new Item[19];

    // Gear in bag
    public List<Item> itemsInBag = new List<Item>();

}
