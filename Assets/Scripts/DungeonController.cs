using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class DungeonController : MonoBehaviour {

    Camp camp;
    GameObject playerSpawnPoint;
    GameObject mapPrefab;
    public GameObject player;
    bool isCampStarted = false;

    GameObject gameManager;
    public static DungeonController instance;

    // Accounting for mobs
    int totalMobs = 0;
    int mobsAlive = 0;
    int mobsKilled = 0;

    // Number of win conditions that must be met to "win"
    int numberWinConditions = 0;

    // Win Condition variables
    bool killNumberofMobs = false;
    int numberMobsToKill;

    Player playerScript;

    [Header("Panels")]
    public GameObject loseScreen;
    public GameObject winScreen;
    public GameObject treasureScreen;
    public Text dungeonName;
    public Text campName;
    public Text timer;

    [Header("Loot")]
    public Transform lootHolder;
    public Transform treasureLootHolder;
    public GameObject lootPrefab;
    public GameObject goldLootPrefab;
    public int lootGold;
    public List<Item> lootBox = new List<Item>();
    bool itemAdded;
    public float lootXP;

    // goofy chat thing
    List<string> chat = new List<string>();

    // Combat Log
    [Header("Logs")]
    List<string> actionLog = new List<string>();
    public Text actionLogText;
    string logText;

    // Targeting
    public Image targetHPBar;
    public Text targetHPNumber;
    public Text targetLevel;
    public Text targetName;
    Monster currentMonsterTargetObject;
    GameObject currentMonsterTargetGO;

    void Awake() {

        Time.timeScale = 1f;
        
        // Dungeon controller instance
        if (instance != null) { Debug.Log("multiple DC attempted - stopping."); return; } else { instance = this; }

        // Drop the map down
        camp = Player.currentCamp;
        mapPrefab = camp.campMap;
        Debug.Log("Instantiating the Level: " + mapPrefab);
        GameObject theMap = (GameObject)Instantiate(mapPrefab,new Vector3(0,0,0), Quaternion.identity);

        // Player Setup
        playerSpawnPoint = GameObject.FindGameObjectWithTag("PlayerSpawn");
        GameObject thePlayer = Instantiate(player, playerSpawnPoint.transform.position, Quaternion.identity);

        gameManager = GameObject.FindGameObjectWithTag("GameController");

        // Clear the action log, which should be clear anyway but I'm a stickler.
        actionLog.Clear();

        // Reset all loot and logs just to be safe
        lootXP = 0;
        actionLog.Clear();
        lootBox.Clear();

        // Locate the player
        playerScript = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();

        StartCoroutine("SetupWinConditions");

    }


    void Update() {
        if (isCampStarted) {
            // Camp has started can actually check for win conditions and shit now.
        }
    }


    // Give player Experience
    public void GivePlayerExperience(float amount) {
        Debug.Log("giving player experience: " + amount);
        playerScript.AddExperience(amount);
    }


    // Update the Action Log
    public void UpdateActionLog(string logData) {
        actionLog.Add(logData);
        if (actionLog.Count > 15) {
            actionLog.RemoveAt(0);
        }

        GetActionLogData();
        actionLogText.text = logText;
    }


    // Get the action log data and format it
    void GetActionLogData() {
        logText = "";
        for (int i = 0; i < actionLog.Count; i++) {
            logText += actionLog[i]+"\r\n";
        }
    }




    // Handle sorting the win conditions
    IEnumerator SetupWinConditions() {
        yield return new WaitForSeconds(1);
        // Kill % of mobs
        if (camp.winCondition.killPercent) {
            killNumberofMobs = true;
            GetNumberMobsInCamp();
            numberMobsToKill = (int)(totalMobs * camp.winCondition.percentEnemiesKilled) / 100;
            Debug.Log("% Kill Condition: Bottom Line: You have to kill this many mobs: " + numberMobsToKill);
            numberWinConditions++;
        }

        // Kill Fixed Number of mobs
        else if (camp.winCondition.killNumber) {
            Debug.Log("Kill fixed number of mobs: " + camp.winCondition.numberEnemiesKilled);
            killNumberofMobs = true;
            numberMobsToKill = camp.winCondition.numberEnemiesKilled;
            numberWinConditions++;
        }

        // Start the camp
        isCampStarted = true;

    }


    // Get total number of mobs in the camp
    void GetNumberMobsInCamp() {
        GameObject[] mobs = GameObject.FindGameObjectsWithTag("Enemy");
        totalMobs = mobs.Length;
        mobsAlive = totalMobs;
    }


    // When mob dies, it notifies dungeon controller here
    public void MobHasDied() {
        mobsAlive--;
        mobsKilled++;
        CheckWinConditions();
        ResetTargetBar();
    }


    // Reset the target bar
    void ResetTargetBar() {
        Debug.Log("reset the target bar");
        targetHPBar.fillAmount = 0;         targetName.text = " no target ";         targetHPNumber.text = "";         targetLevel.text = "";
    }


    // Set Target (from player)
    public void SetTargetBar(GameObject monster) {
        currentMonsterTargetObject = monster.GetComponent<MonsterBase>().monsterObject;
        currentMonsterTargetGO = monster.gameObject;

        float percentHitPoints = (currentMonsterTargetGO.GetComponent<MonsterBase>().hitPoints / currentMonsterTargetObject.hitPoints);
        targetHPBar.fillAmount = percentHitPoints;
        targetHPNumber.text = (percentHitPoints * 100) + " %".ToString();

        targetLevel.text = currentMonsterTargetObject.level.ToString();
        targetName.text = currentMonsterTargetObject.monsterName;
    }


    // Set the HP bar in player UI when targeted
    public void UpdateTargetHPBar() {
        float percentHitPoints = (currentMonsterTargetGO.GetComponent<MonsterBase>().hitPoints / currentMonsterTargetObject.hitPoints);
        targetHPBar.fillAmount = percentHitPoints;
        targetHPNumber.text = (percentHitPoints * 100) + " %".ToString();
    }


    // Check the Win Conditions for this camp and see if we have won yet
    void CheckWinConditions() {
        Debug.Log("heard a mob died.  Checking win conditions here...");

        if (killNumberofMobs) { CheckKillNumberofMobs(); }

        if (numberWinConditions == 0) {
            Debug.Log("You have won!");
            // Check for max number of camps, increment if there's another one past what is already open

            // Check if this was the first camp in the dungeon.  If so, start the respawn timer.
            if (DungeonLocks.unlockedCamps[Player.currentDungeon.dungeonName] == 1) {
                GameManager.instance.GetComponent<DungeonLocks>().StartRespawnTimer();
            }

            // Open next camp if there is one
            if (DungeonLocks.unlockedCamps[Player.currentDungeon.dungeonName] < Player.currentDungeon.camps.Length) {
                // There are more camps to unlock
                Debug.Log("Open next Camp.");
            } else {
                // This was the last camp, they have beaten the dungeon TODO dungeon finish bonus shit
                Debug.Log("Dungeon is beaten.");
            }

            // Increment the counter regardless
            DungeonLocks.unlockedCamps[Player.currentDungeon.dungeonName] += 1;

            // Do the win stuff
            StartCoroutine("WinCamp");

        }

    }


    // Check if Percent of mobs is met for win condition
    void CheckKillNumberofMobs() {
        if (mobsKilled >= numberMobsToKill) {
            Debug.Log("have beaten this win condition");
            numberWinConditions--;
        } else {
            Debug.Log("keep killing not there yet");
        }

    }

    public void Lose() {
        loseScreen.active = true;
    }


    void ChatWindow() {
        
    }


    public void UpdateCombatLog(string newLogItem) {

        /*
        combatLog.Add(newLogItem);
        int max = combatLog.Count - 1;
        Debug.Log("last slot: " + max);
        if (max > 6) { max = 6; }
        if (max == 0) { max = 1; }

        for (int i = 0; i < 7; i++) {
            Debug.Log("showing combat log slot: " + max);
            logText += combatLog[max] + " - ";
            max--;
        }

        //combatLogDisplay.text = logText;

        Debug.Log(logText);
        */
    }


    // Add Gold Loot
    public void AddGoldToLoot(int amount) {
        lootGold += amount;
        Debug.Log("Gold Added to pile.  Current stash: " + lootGold);
    }


    // Add Loot Loot 
    public void AddLoot(Item drop, int quantity) {

        drop.quantity = quantity;
        Debug.Log("Drop to add: " + drop.itemName + " - equip? " + drop.isEquippable);

        bool added = false;
        Debug.Log("adding " + drop.itemName + " to loot");

        // Loop through all loot see if this is a duplicate
        // If this is the first item being added, just add it it can't be a duplicate

        if (added == false) {
            if (lootBox.Count == 0) {
                Debug.Log("first item");
                lootBox.Add(drop);
                added = true; // not that it matters here.
            } else {
                // Otherwise make sure its not a duplicate
                for (int i = 0; i < lootBox.Count; i++) {
                    if (added == false) {
                        Debug.Log(lootBox[i].itemName + " - " + drop.itemName);
                        if (lootBox[i].itemName == drop.itemName) {
                            Debug.Log("found a duplicate");
                            lootBox[i].quantity += quantity;
                            Debug.Log("amount is now: " + lootBox[i].quantity);
                            added = true;
                        }
                    }
                }

                // If added is still false after checking all inventory, add a new item
                if (added == false) {
                    Debug.Log("Checked whole inventory - add a new item");
                    lootBox.Add(drop);
                    added = true;
                }
            }
        }

    }


    // Win Stuff
    IEnumerator WinCamp() {
        yield return new WaitForSeconds(2);
        winScreen.active = true;

        dungeonName.text = Player.currentDungeon.dungeonName;
        campName.text = camp.campName;

        // Give the player the accumulated rewards now
        RewardGold();
        RewardLoot();

        // Save the game
        gameManager.GetComponent<Save>().SaveGame();

        Time.timeScale = 0f;
    }


    // WIN - Reward the gold
    void RewardGold() {
        GameManager.gold += lootGold;
        GameObject goldReward = (GameObject)Instantiate(goldLootPrefab, lootHolder.position, Quaternion.identity);
        goldReward.transform.SetParent(lootHolder,false);
        Text[] goldText = goldReward.GetComponentsInChildren<Text>();
        Debug.Log("total gold found: " + lootGold);
        goldText[0].text = lootGold.ToString();
    }


    // WIN - Reward the Loot
    void RewardLoot() {
        Debug.Log("loot reward here");

        Debug.Log("size: " + lootBox.Count);

        for (int i = 0; i < lootBox.Count; i++) {
            GameObject thisLoot = (GameObject)Instantiate(lootPrefab, lootHolder.position, Quaternion.identity);
            thisLoot.transform.SetParent(lootHolder, false);

            Image lootImage = thisLoot.GetComponentInChildren<Image>();
            lootImage.sprite = lootBox[i].itemIcon;

            Text[] lootText = thisLoot.GetComponentsInChildren<Text>();
            lootText[0].text = lootBox[i].quantity.ToString();
            lootText[1].text = lootBox[i].itemName;

            // Add one for each item in there.
            for (int q = 0; q < lootBox[i].quantity; q++) {
                CharacterSheet.instance.AddItemToInventory(lootBox[i]);
            }
        }
    }


    // Treasure Chest - Show loot just dont give it to player yet (TODO combine with reward methods)
    public void ShowLoot() {

        // Make sure holder is empty first
        foreach (Transform child in treasureLootHolder.transform) { GameObject.Destroy(child.gameObject); }

        // Show treasure window
        treasureScreen.active = true;

        // Pause Game
        Time.timeScale = 0f;

        // Gold
        GameObject goldReward = (GameObject)Instantiate(goldLootPrefab, treasureLootHolder.position, Quaternion.identity);
        goldReward.transform.SetParent(treasureLootHolder, false);
        Text[] goldText = goldReward.GetComponentsInChildren<Text>();
        goldText[0].text = lootGold.ToString();

        // Loot
        for (int i = 0; i < lootBox.Count; i++) {
            GameObject thisLoot = (GameObject)Instantiate(lootPrefab, treasureLootHolder.position, Quaternion.identity);
            thisLoot.transform.SetParent(treasureLootHolder, false);

            Image lootImage = thisLoot.GetComponentInChildren<Image>();
            lootImage.sprite = lootBox[i].itemIcon;

            Text[] lootText = thisLoot.GetComponentsInChildren<Text>();
            lootText[0].text = lootBox[i].quantity.ToString();
            lootText[1].text = lootBox[i].itemName;

        }
    }


    // Resume Game
    public void ResumeGame() {
        treasureScreen.active = false;
        Time.timeScale = 1f;
    }
}
