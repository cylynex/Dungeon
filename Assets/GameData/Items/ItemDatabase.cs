using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName="ItemDB",menuName="Add Item DB")]
public class ItemDatabase : ScriptableObject {

    public Item[] items;

    public Item GetItem(int itemID) {
        foreach (var item in items) {
            if (item != null && item.itemID == itemID) return item;
        }
        return null;
    }

    public Item GetItemByName(string itemName) {
        foreach (var item in items) {
            if (item != null && item.itemName == itemName) return item;
        }
        return null;
    }

}
