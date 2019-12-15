using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MapController : MonoBehaviour {

    public Zone thisZone;

    void OnMouseDown() {
        Player.currentZone = thisZone;
        SceneManager.LoadScene("Zone");
    }

}
