using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class LoadScene : MonoBehaviour {

    public void ChangeScene(string sceneToLoad) {
        SceneManager.LoadScene(sceneToLoad);
    }

}