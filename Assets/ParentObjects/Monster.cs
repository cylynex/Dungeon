using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName="Monster", menuName="Monster / Add Monster")]
public class Monster : ScriptableObject {

    public string monsterName;
    public Sprite monsterImage;
    public Vector3 monsterScale;
    public GameObject monsterPrefab;

    [Header("Stats and Stuff")]
    public int level;
    public float hitPoints;
    public int damage;
    public float range;
    public float attackTime;
    public float aggroRange;
    public float speed;

    [Header("Resists")]
    public float resistPhysical;
    public float resistCold;
    public float resistFire;
    public float resistElectric;
    public float resistPoison;

    [Header("Loot")]
    public int goldLow;
    public int goldHigh;
    public LootTable lootTable;
    public float xpPerKill;

}