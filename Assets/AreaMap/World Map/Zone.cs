using UnityEngine;
using System.Collections;
//using UnityEngine.UI;

[CreateAssetMenu(fileName="Zone", menuName="Zone / Create New Zone")]
public class Zone : ScriptableObject {

    public string zoneName;
    public GameObject zonePrefab;

}
