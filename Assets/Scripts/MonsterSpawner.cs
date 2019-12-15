using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterSpawner : MonoBehaviour {

    public Monster[] monsters;
    public int[] spawnChance;
    public float spawnTime;
    Monster monsterToSpawn;

    float spawnTimer;
    bool isSpawned = false;

    void Awake() {
        spawnTimer = 0;
    }

    void Update() {
        if (isSpawned == false) {
            if (spawnTimer == 0) {
                SpawnMonster();
            }
        } 
    }


    // Spawn a monster yo
    void SpawnMonster() {

        // Roll 1-100 and check the table to see what they got.
        int spawnWeight = Random.Range(1, 100);

        for (int i = 0; i < spawnChance.Length; i++) {
            
            if (spawnWeight < spawnChance[i]) {
                monsterToSpawn = monsters[i];
                GameObject spawnedMonster = (GameObject)Instantiate(monsterToSpawn.monsterPrefab, transform.position, Quaternion.identity);
                spawnedMonster.GetComponent<MonsterBase>().spawnPoint = gameObject;
                spawnedMonster.GetComponent<MonsterBase>().monsterObject = monsterToSpawn;
                spawnedMonster.transform.SetParent(transform, true);
                isSpawned = true;
            }
        }

        isSpawned = true;
    }

}
