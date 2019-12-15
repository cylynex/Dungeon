using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BarController : MonoBehaviour {

    public static BarController instance;

    public Text hpBarText;
    public Text xpBarText;
    public Text castBarText;

    public Image hpBar;
    public Image xpBar;
    public Image castBar;

    void Start() {
        if (instance != null) { Debug.Log("multiple bc found"); return; } else { instance = this; }
        Debug.Log("BC Initializing");
        // Grab bars if present
        if (GameObject.FindGameObjectWithTag("xpBar")) {
            Debug.Log("found an XP Bar to work with");
            xpBar = GameObject.FindGameObjectWithTag("xpBar").GetComponent<Image>();
        }

        if (GameObject.FindGameObjectWithTag("hpBar")) {
            Debug.Log("found a HP Bar to work with");
            hpBar = GameObject.FindGameObjectWithTag("hpBar").GetComponent<Image>();
            hpBarText = GameObject.FindGameObjectWithTag("hpBarText").GetComponent<Text>();
        }

        Debug.Log("BC Initialized");

        // Casting stuff not here yet TODO finish this part
        //castBar = GameObject.FindGameObjectWithTag("castBar").GetComponent<Image>();
        //castBarText = GameObject.FindGameObjectWithTag("castBarText").GetComponent<Text>();

        if (xpBar != null) {
            UpdateXPBar();
            UpdateHPBar();
        }

    }




    // Manage the XP Bar
    public void UpdateXPBar() {
        float percentXP = (Player.character.currentExperience / Player.character.expForNextLevel);
        float currentAmount = xpBar.fillAmount;
        xpBar.fillAmount = percentXP;
    }


    // Manage the HP Bar
    public void UpdateHPBar() {
        Debug.Log("updating HP bar: HP : " + Player.hitPoints + " - " + Player.character.modifiedHitPoints);
        float percentHitPoints = (Player.hitPoints / Player.character.modifiedHitPoints);
        hpBar.fillAmount = percentHitPoints;
        hpBarText.text = Player.hitPoints.ToString();
    }
}