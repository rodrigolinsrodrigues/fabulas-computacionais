using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LootLocker.Requests;
using TMPro;
using UnityEngine.SceneManagement;

public class DataManager : MonoBehaviour
{
    public string schoolGrade;
    public string playerName;
    public int playerAge;
    public string playerGender;
    public List<LevelData> levelManager;
    public TextMeshProUGUI playerNameTxt;

    public static DataManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }

        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        Login();
    }

    public void Login()
    {
        LootLockerSDKManager.StartGuestSession((response) =>
        {
            if (response.success)
            {
                Debug.Log("Player was logged in");
            }
            else
            {
                Debug.Log("Could not start session");
            }
        });
    }

    public void SetPlayerNameOnUI(string playerName)
    {
        playerNameTxt.text = playerName;
    }

    public void ResetAllData()
    {
        foreach (var item in levelManager)
        {
            item.Reset();
        }
    }

    //create data content to send to LootLocker dashboard
    public void CreatingLevelData()
    {
        LootLockerSDKManager.CreatingAnAssetCandidate(playerName, (response) =>
        {
            if (response.success)
            {
                Debug.Log("Successfully created asset candidate with ID: " + response.asset_candidate_id);
                UploadingLevelData(response.asset_candidate_id);
            }
            else
            {
                Debug.Log("Error asset candidate");
            }
        }, null, null, null, 214127);
    }

    //send data content to LootLocker dashboard
    public void UploadingLevelData(int levelID)
    {
        string filePath = playerName + " GameData.csv";
        LootLocker.LootLockerEnums.FilePurpose textFileType = LootLocker.LootLockerEnums.FilePurpose.file;

        LootLockerSDKManager.UpdatingAnAssetCandidate(levelID, false, (updateresponse) =>
        {
            if (updateresponse.success)
            {
                LootLockerSDKManager.AddingFilesToAssetCandidates(levelID, filePath, playerName + " GameData.csv", textFileType, (textresponse) =>
                {
                    if (textresponse.success)
                    {
                        LootLockerSDKManager.UpdatingAnAssetCandidate(levelID, true, (updateresponse) => { }, playerName, null, null, null, 214127);
                    }
                    else
                    {
                        Debug.Log("Error updating asset candidate");
                    }
                });
            }
            else
            {
                Debug.Log("Error updating asset candidate");
            }
        }, playerName, null, null, null, 214127);
    }

    //save local data on device
    public void SaveDataLocal()
    {
        string filePath = playerName + " GameData.csv";
        StreamWriter writer = new StreamWriter(filePath, false);

        string info = playerName + "," + playerAge + "," + playerGender + "," + schoolGrade + ",";
        for (int i = 0; i < levelManager.Count; i += 3)
        {
            for (int j = i; j < i + 3; j++)
            {
                info += levelManager[j].attempCounter + ",";
            }
            info += " ,";
            for (int j = i; j < i + 3; j++)
            {
                info += levelManager[j].sucessPercent + ",";
            }
            info += " ,";
            for (int j = i; j < i + 3; j++)
            {
                info += Mathf.RoundToInt(levelManager[j].currentTime) + ",";
            }
            info += " ,";
        }


        writer.WriteLine(info);
        writer.Close();

        //CreatingLevelData();
        Debug.LogWarning(info);
    }
}
