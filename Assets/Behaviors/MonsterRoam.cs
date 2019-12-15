using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterRoam : MonoBehaviour {

    // Time between random Roams
    float roamTime = 10f;
    [SerializeField]
    float roamTimer;

    float moveTime = 2f;
    float moveTimer;

    bool moving = false;
    int directionToRoam;
    float speed;

    void Start() {
        roamTimer = roamTime;
        speed = GetComponent<MonsterBase>().monsterObject.speed;
    }

	void Update () {

        if (moving == true) {
            // Roll the moving timer
            moveTimer -= Time.deltaTime;

            Move();

            if (moveTimer <= 0) {
                moving = false;
            }

        } else {
            // Not moving, can work on picking a new roam path
            roamTimer -= Time.deltaTime;

            if (roamTimer <= 0) {
                Roam();
                roamTimer = roamTime;
            }
        }
	}


    void Roam() {

        // Pick a random direction
        directionToRoam = Random.Range(1, 4);

        // Setup move variables
        moving = true;
        moveTimer = moveTime;
        roamTimer = roamTime;
    }


    void Move() {
        switch (directionToRoam) {
            case 1: // Go Up
                transform.Translate(Vector3.up * speed * Time.deltaTime);
                break;
            case 2: // Go Down
                transform.Translate(Vector3.down * speed *  Time.deltaTime);
                break;
            case 3: // Go Left
                transform.Translate(Vector3.left * speed *  Time.deltaTime);
                break;
            case 4: // Go Right
                transform.Translate(Vector3.right * speed *  Time.deltaTime);
                break;
        }
    }
}
