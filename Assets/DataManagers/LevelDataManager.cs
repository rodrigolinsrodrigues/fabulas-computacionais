using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelDataManager : MonoBehaviour
{
    public LevelData currentLevelData;
    void Start()
    {
        LoadCurrentLevelData();
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
    }

    public void GetAttempt()
    {
        currentLevelData.attempCounter ++;
        DataManager.Instance.SaveDataLocal();
    }
}
