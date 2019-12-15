using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class LoadDungeon : MonoBehaviour {

    public Dungeon dungeon;

    void OnMouseDown() {
        Debug.Log("clicked dungeon.  Load this one: "+dungeon.dungeonName);
        Player.currentDungeon = GetComponent<LoadDungeon>().dungeon;
        SceneManager.LoadScene("DungeonLoad");
    }


}