using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName="LootTable", menuName="Add Loot Table")]
public class LootTable : ScriptableObject {

    public Item[] drops;
    public int[] dropChance;

}
