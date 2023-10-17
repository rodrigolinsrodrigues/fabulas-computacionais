using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Analytics;
using Unity.VisualScripting;

public class DataManager : MonoBehaviour
{
    public float timePlayed;
    public int playerAge;
    public string playerGender;

    public List<LevelData> levelManager;

    /*
        função de soma de tempo de fases por tipo de fase, soma de todas as fases
    */
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

    async void Start()
    {
        try
        {
            await UnityServices.InitializeAsync();
        }
        catch (ConsentCheckException e)
        {
            Debug.Log(e.ToString());
        }
    }

    void Update()
    {
        if (GameManager.Instance.started)
        {
            timePlayed += Time.deltaTime;
        }
    }

    public void ResetAllData()
    {
        foreach (var item in levelManager)
        {
            item.Reset();
        }
    }

    public void AddNewLevelData(LevelData data)
    {
        levelManager.Add(data);
    }
    public void SaveData()
    {
        Dictionary<string, object> parameters = new Dictionary<string, object>()
        {
           {"Gender", playerGender},
        };

        AnalyticsService.Instance.CustomData("PlayerDataTest", parameters);

        AnalyticsService.Instance.Flush();
    }
}
