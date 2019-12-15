using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour {

    public int slotID;
    public Item itemInSlot = null;

    Image[] imageSlots;
    Button invButton;

    void Start() {
        imageSlots = GetComponentsInChildren<Image>();
    }

    public void ClearItem() {
        itemInSlot = null;
        imageSlots[2].sprite = null;
        slotID = 9999;
    }


    public void ShowItemDetails() {
        CharacterSheet.instance.ShowItemDetails(itemInSlot,slotID,false);
    }


    // Put item in slot
    public void AddItemToInventorySlot(Item item, int slot) {
        imageSlots = GetComponentsInChildren<Image>();
        slotID = slot;
        itemInSlot = item;
        imageSlots[2].sprite = itemInSlot.itemIcon;
    }
}
