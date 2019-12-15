using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadDelay : MonoBehaviour {

    public float delay;
    public string sceneToload;

    void Start() {
        StartCoroutine(LoadLevelAfterDelay(delay));
    }

    IEnumerator LoadLevelAfterDelay(float delay) {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene("WorldMap");
    }
}
