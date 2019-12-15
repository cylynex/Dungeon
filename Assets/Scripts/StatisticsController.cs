using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatisticsController : MonoBehaviour {

    public GameObject statPrefab;
    public Transform statHolder;

    void Start() {

        // Loop through all items in player stats and display
        foreach (KeyValuePair<string, float> stat in Statistics.playerStatistics) {
            Debug.Log(stat.Key + " - " + stat.Value);
            // instantiate prefab and insert data
        }

    }

    /*
    void ShowStatistics() {
        //tPlayerDeaths.text = "Player Deaths: " + Statistics.playerDeaths;
        GameObject thisStatBox = (GameObject)Instantiate(statPrefab, statHolder.position, Quaternion.identity);
        thisStatBox.transform.SetParent(statHolder,false);
        Text thisText[] = thisStatBox.GetComponentInChildren<Text>();
        thisText[0].text = 
    }
    */
}
