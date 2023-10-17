using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelData", menuName = "DataManagers/LevelDataScriptables", order = 1)]
public class LevelData : ScriptableObject
{
    public string levelType;
    public int currentArea, attempCounter;
    public float currentTime, winTime, sucessPercent;
    public bool hasWin = false;

    public void Reset()
    {
        currentTime = 0;
        attempCounter = 0;
        winTime = 0;
        sucessPercent = 0;
        hasWin = false;
    }
}
