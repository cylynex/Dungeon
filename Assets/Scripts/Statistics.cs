using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Statistics : MonoBehaviour {
    
    public static Dictionary<string, float> playerStatistics = new Dictionary<string, float>();
    public static bool isGameLoaded = false;

    // Create New Game
    public void CreateNewGame() {
        Debug.Log("creating a new game - stats");
        CreatePlayerStatistics();
    }


    void CreatePlayerStatistics() {
        Statistics.playerStatistics.Add("Player Deaths", 0f);
        Statistics.playerStatistics.Add("Dungeons Beat", 0f);
    }

}
