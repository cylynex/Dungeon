using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GearSlotController : MonoBehaviour {

    public void ShowItemDetails(int slotID) {
        Debug.Log("show the item details for this slot: "+slotID);
        if (CharacterSheet.instance.equippedGearSlot[slotID] != null) {
            Debug.Log("This is the gear in this slot: " + GameManager.inventories[Player.character.characterName].gearSlots[slotID]);
            CharacterSheet.instance.ShowItemDetails(GameManager.inventories[Player.character.characterName].gearSlots[slotID],slotID,true);
        } else {
            Debug.Log("Empty Slot");
        }
    }
	
}
