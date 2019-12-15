using UnityEngine;
using System.Collections;

public class CameraGlue : MonoBehaviour {

    GameObject player;
    private Vector3 offset;

    void Start() {
        FindPlayer();
    }

    void LateUpdate() {
        if (player == null) {
            FindPlayer();
        }

        // Set the position of the camera's transform to be the same as the player's, but offset by the calculated offset distance.
        transform.position = player.transform.position + offset;
    }


    void FindPlayer() {
        player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) {
            offset = transform.position - player.transform.position;
        }
    }
}