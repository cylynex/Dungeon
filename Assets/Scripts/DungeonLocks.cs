using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class DungeonLocks : MonoBehaviour {

    // Locks and Timers
    public static Dictionary<string, int> unlockedCamps = new Dictionary<string, int>();
    public static Dictionary<string, DateTime> dungeonRespawnTimers = new Dictionary<string, DateTime>();

    void Start() {
        // Testing only
        // unlockedCamps.Add("Dungeon One", 2);
    }

    public int GetCampsOpen(string dungeonName) {

        if (unlockedCamps.ContainsKey(dungeonName)) {
            Debug.Log("found an entry for this dungeon " + dungeonName);
            return unlockedCamps[dungeonName];
        } else {
            Debug.Log("No entry for this dungeon " + dungeonName);
            return 0;
        }

        return 0;
    } 


    public bool FullSpawnCheck(string dungeonName) {
        if (dungeonRespawnTimers.ContainsKey(dungeonName)) {
            // Found the dungeon
            Debug.Log("This dungeon is only partially spawned.");

            // Check if timer has expired, remove if so.
            TimeSpan timeLeft = dungeonRespawnTimers[dungeonName] - System.DateTime.Now;
            float timeLeftInSeconds = Convert.ToSingle(timeLeft.TotalSeconds);
            if (timeLeftInSeconds <= 0) {
                Debug.Log("resetting dungeon: " + dungeonName);
                dungeonRespawnTimers.Remove(dungeonName);
                //unlockedCamps.Remove(dungeonName);
                unlockedCamps[dungeonName] = 1;
                return true;
            } else {
                Debug.Log("timer still going");
                return false;
            }
        } else {
            // No respawn timer.
            Debug.Log("This dungeon is fully spawned.  No timer was found.");
            return true;
        }
    }


    public void StartRespawnTimer() {
        DateTime respawnTime = DateTime.Now.AddMinutes(Player.currentDungeon.dungeonRespawnTime);
        dungeonRespawnTimers.Add(Player.currentDungeon.dungeonName,respawnTime);
        Debug.Log("Start a respawn timer: "+respawnTime);
    }

}
