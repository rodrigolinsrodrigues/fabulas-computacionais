using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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
    [SerializeField]
    private int starsAchieved;
    public string spawnPoint;
    public GameObject loadPanel;
    public List<string> levelFinisheds = new List<string>();
    public void StartGame()
    {
        previousScene = "Area1";
        currentScene = "Area1";
        spawnPoint = "spawnPoint Area1";
        starsAchieved = 0;
    }

    public string GetPreviousScene()
    {
        return previousScene;
    }

    public void SetCurrentScene(string name)
    {
        previousScene = "" + currentScene;
        currentScene = name;
        spawnPoint = "spawnPoint " + previousScene;
        StartCoroutine("LoadRoutine");
    }

    public void UpdateProgress(string levelName)
    {
        if (CurrentProgress(levelName) == false)
        {
            addStars();
            levelFinisheds.Add(levelName);
        }
    }
    public bool CurrentProgress(string levelName)
    {
        return levelFinisheds.Contains(levelName);
    }

    public int GetCurrentStars()
    {
        return starsAchieved;
    }
    public void addStars()
    {
        starsAchieved++;
    }
    IEnumerator LoadRoutine()
    {
        loadPanel.SetActive(true);
        Time.timeScale = 0;
        yield return new WaitForSecondsRealtime(1);
        Time.timeScale = 1;
        yield return new WaitForSecondsRealtime(1);
        loadPanel.SetActive(false);
    }
}
