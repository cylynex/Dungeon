using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZoneController : MonoBehaviour {

    //public GameObject zonePrefab;
    Zone zone;

    void Start() {
        zone = Player.currentZone;

        // Instantiate the zone prefab
        Instantiate(zone.zonePrefab, transform.position, Quaternion.identity);  

    }

}
