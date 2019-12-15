using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "Location", menuName = "Locations / Create New Dungeon")]
public class Dungeon : ScriptableObject {

    [Header("Basics")]
    public string dungeonName;
    public int dungeonID;
    [TextArea(10,10)]
    public string dungeonDescription;
    public Sprite dungeonLoadBackground;
    public Color textColor1;
    public Color textColor2;
    public Color textColor3;
    public Color textColor4;

    [Header("Timers")]
    public float dungeonRespawnTime;

    [Header("Camp Data")]
    public Camp[] camps;


}