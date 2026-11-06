using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelDataManager : MonoBehaviour
{
    public LevelData currentLevelData;
    void Start()
    {
        LoadCurrentLevelData();
        TelemetryManager.Log("puzzle_start",
            ("level_type", currentLevelData.levelType),
            ("area", currentLevelData.currentArea),
            ("previous_attempts", currentLevelData.attempCounter),
            ("previous_time_s", currentLevelData.currentTime),
            ("already_won", currentLevelData.hasWin));
    }

    void Update()
    {
        if (!currentLevelData.hasWin)
        {
            currentLevelData.currentTime += Time.deltaTime;
        }
    }

    //carrega os dados da fase, caso o jogador já tenha passado por ela sem ter finalizado
    public void LoadCurrentLevelData()
    {
        if (DataManager.Instance.levelManager.Count > 0)
        {
            for (int i = 0; i < DataManager.Instance.levelManager.Count; i++)
            {
                if (DataManager.Instance.levelManager[i].levelType == currentLevelData.levelType &&
                    DataManager.Instance.levelManager[i].currentArea == currentLevelData.currentArea)
                {
                    currentLevelData.currentTime = DataManager.Instance.levelManager[i].currentTime;
                    currentLevelData.attempCounter = DataManager.Instance.levelManager[i].attempCounter;
                }
            }
        }
    }

    public void WinLevel()
    {
        currentLevelData.hasWin = true;
        currentLevelData.winTime = currentLevelData.currentTime;
        currentLevelData.sucessPercent = Mathf.RoundToInt(100 / currentLevelData.attempCounter);
        TelemetryManager.Log("puzzle_complete",
            ("level_type", currentLevelData.levelType),
            ("area", currentLevelData.currentArea),
            ("attempts", currentLevelData.attempCounter),
            ("time_s", currentLevelData.winTime));
    }

    public void GetAttempt()
    {
        currentLevelData.attempCounter ++;
        TelemetryManager.Log("puzzle_attempt",
            ("level_type", currentLevelData.levelType),
            ("area", currentLevelData.currentArea),
            ("attempt", currentLevelData.attempCounter),
            ("time_s", currentLevelData.currentTime));
        DataManager.Instance.SaveDataLocal();
    }
}
