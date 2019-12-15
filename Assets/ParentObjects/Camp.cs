using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "Location", menuName = "Locations / Create New Camp")]
public class Camp : ScriptableObject {

    public string campName;
    public int campID;
    public GameObject campMap;
    [TextArea(10,10)]
    public string campDescription;
    public WinCondition winCondition;


}
