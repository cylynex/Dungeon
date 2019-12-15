using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WinCondition", menuName = "Create New Win Condition")]
public class WinCondition : ScriptableObject {

    [TextArea(5, 10)]
    public string winConditionDescription;

    public bool killPercent;
    public float percentEnemiesKilled;

    public bool killNumber;
    public int numberEnemiesKilled;

}
