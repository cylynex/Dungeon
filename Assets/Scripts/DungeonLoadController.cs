using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;

public class DungeonLoadController : MonoBehaviour {

    public Dungeon dungeon;
    public Text dungeonName;
    public Text dungeonDescription;
    public Text dungeonRespawnTimer;
    public Text dungeonRespawnTimerText;
    float dungeonRespawnTimerActual;
    public Image dungeonBackgroundImage;

    [Header("Camp Stuff")]
    public GameObject lockedCampPrefab;
    public GameObject unlockedCampPrefab;
    public GameObject campHolder;

    public Text campDescription;
    public Text winConditions;
    public Text winConditionHeader;
    public Text playButton;

    public GameObject gameManager;

    DungeonLocks dungeonLocks;

    void Start() {

        dungeonLocks = GameManager.instance.GetComponent<DungeonLocks>();

        // Grab game manager for save / load as needed
        //gameManager = GameObject.FindGameObjectWithTag("GameController");

        // Get the dungeon from the static variable assigned to Player when setting up the level
        dungeon = Player.currentDungeon;

        // Setup the UI text stuff
        SetupLabels();

        // Setup the camps
        SetupCamps();
    }


    // Setup the text for the load screen
    void SetupLabels() {
        dungeonName.text = dungeon.dungeonName;
        dungeonName.color = dungeon.textColor1;

        dungeonDescription.text = dungeon.dungeonDescription;
        dungeonDescription.color = dungeon.textColor3;

        dungeonRespawnTimer.text = dungeon.dungeonRespawnTime.ToString();
        dungeonRespawnTimer.color = dungeon.textColor4;
        dungeonRespawnTimerText.color = dungeon.textColor4;

        dungeonBackgroundImage.GetComponent<Image>().sprite = dungeon.dungeonLoadBackground;
    }

    void Update() {
        if (DungeonLocks.unlockedCamps[dungeon.dungeonName] > 1) {
            // Past first camp, timer be ROLLIN baby
            if (dungeonRespawnTimerActual > 0) {
                dungeonRespawnTimerActual -= Time.deltaTime;
                Debug.Log(dungeonRespawnTimerActual);

                if (dungeonRespawnTimerActual <= 0) {
                    // TODO call for the reset here
                    SceneManager.LoadScene("DungeonLoad");
                }
                dungeonRespawnTimer.text = dungeonRespawnTimerActual.ToString("F2");
            }
        }
    }


    // Setup the camp buttons
    void SetupCamps() {

        int campsOpen = dungeonLocks.GetCampsOpen(dungeon.dungeonName);

        // Never been to this dungeon before, so add the record and list "1" as the first camp is always open.
        if (campsOpen == 0) {
            Debug.Log("Never been to this dungeon before.  Initializing it");
            DungeonLocks.unlockedCamps.Add(dungeon.dungeonName, 1);
        }

        // Check for Respawn Timers.  Basically if opencamps == 1 We know we don't really need to do this
        if (GameManager.instance.GetComponent<DungeonLocks>().FullSpawnCheck(dungeon.dungeonName) == false) {
            // There's an open dungeon run in progress.
            Debug.Log("Timer Found");

            // Setup the display timer
            //TimeSpan timeLeft = DungeonLocks.lockTimers[1] - System.DateTime.Now;
            Debug.Log("checking timer left for " + dungeon.dungeonName);
            TimeSpan timeLeft = DungeonLocks.dungeonRespawnTimers[dungeon.dungeonName] - System.DateTime.Now;
            dungeonRespawnTimer.text = timeLeft.ToString();
            dungeonRespawnTimerActual = Convert.ToSingle(timeLeft.TotalSeconds);

        } 

        for (int i = 0; i < dungeon.camps.Length; i++) {
            int currentCamp = i + 1;
            int thisDungeonOpenCamps = DungeonLocks.unlockedCamps[dungeon.dungeonName];

            GameObject thisButton;

            // Check locked / unlocked state of this camp and display appropriately.
            if (i < thisDungeonOpenCamps) {
                thisButton = Instantiate(unlockedCampPrefab, campHolder.transform.position, Quaternion.identity);
                Text buttonText = thisButton.GetComponentInChildren<Text>();
                buttonText.text = dungeon.camps[i].campName;
                buttonText.color = dungeon.textColor4;

                // Figure out if the camp is cleared already so we know whether or not to let them play it.
                if (currentCamp < thisDungeonOpenCamps) {
                    buttonText.text = "CLEARED";
                } else {
                    // Assign the camp object so it knows what to load if pressed.
                    thisButton.GetComponent<CampLoadController>().camp = dungeon.camps[i];

                    Button actualButton = thisButton.GetComponent<Button>();
                    Camp campToGoto = dungeon.camps[i];
                    actualButton.onClick.AddListener(() => { LoadCamp(campToGoto); });
                }

            } else {
                thisButton = Instantiate(lockedCampPrefab, campHolder.transform.position, Quaternion.identity);
            }

            thisButton.transform.SetParent(campHolder.transform, false);
        }
    }


    // Selected camp - camp data and prepare to go
    void LoadCamp(Camp campToGoto) {
        campDescription.text = campToGoto.campDescription;
        winConditions.text = campToGoto.winCondition.winConditionDescription;
        winConditionHeader.text = "Win Conditions";
        playButton.text = "Enter Camp";
        Button activeButton = playButton.GetComponent<Button>();
        activeButton.onClick.AddListener(() => { PlayCamp(campToGoto); });
    }


    // Load the actual camp and go
    void PlayCamp(Camp campToGoto) {
        Player.currentCamp = campToGoto;
        SceneManager.LoadScene("Dungeon");
        //SceneManager.LoadScene("IsometricDungeon");
    }
}
