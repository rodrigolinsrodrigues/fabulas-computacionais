using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

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
    [SerializeField]
    private string currentScene, previousScene;
    private int starsAchieved;
    public GameObject loadPanel, player;
    public Transform spawnPoint;
    public void StartGame()
    {
        SetCurrentScene("Area1");
        SetCurrentStars(0);
    }

    public string GetPreviousScene()
    {
        return previousScene;
    }
    public void SetCurrentScene(string name)
    {
        StartCoroutine("LoadRoutine");
        if (currentScene != "")
        {
            previousScene = currentScene;
        }
        currentScene = name;
    }

    public int GetCurrentStars()
    {
        return starsAchieved;
    }
    public void SetCurrentStars(int value)
    {
        starsAchieved = value;
    }
    IEnumerator LoadRoutine()
    {
        loadPanel.SetActive(true);
        Time.timeScale = 0;
        yield return new WaitForSecondsRealtime(1.5f);
        if (GameObject.FindGameObjectWithTag("Player") != null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
            if (GetPreviousScene() != "")
            {
                spawnPoint = GameObject.Find("spawnPoint " + GetPreviousScene()).transform;
            }
            player.transform.position = spawnPoint.position;
            player.transform.rotation = spawnPoint.transform.rotation;
        }
        Time.timeScale = 1;
        loadPanel.SetActive(false);


    }
}
