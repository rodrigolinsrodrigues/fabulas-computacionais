using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    //----------------------------------------------------------- Singleton -----------------------------------------------------------------------------------
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
    //--------------------------------------------------------- End Singleton -----------------------------------------------------------------------------------
    //----------------------------------------------------------- Variables -----------------------------------------------------------------------------------   
    [SerializeField]
    private string currentScene, previousScene;
    [SerializeField]
    private int starsAchieved;
    public string spawnPoint;
    public GameObject loadPanel;
    public List<string> levelFinisheds = new List<string>();
    public GameObject pausePanel;
    public int recipesStars, groupsStars, mirrorsStars, musicalStars;
    public TextMeshProUGUI recipesStarsTxt, groupsStarsTxt, mirrorsStarsTxt, musicalStarsTxt, playerNameTxt, timeCounterTxt;
    public GameObject[] controlPopups;
    public GameObject resumePauseBTN, controlsPanel;
    public bool canPause, started = false;
    public Image loadBG;
    public Sprite[] bgPool;
    public int area1Count, area2Count, area3Count;
    public float timeCounter;

    //-------------------------------------------------------- End Variables -----------------------------------------------------------------------------------

    void Start()
    {
        canPause = false;
    }

    void Update()
    {
        if (started)
        {
            timeCounter += Time.deltaTime;

            int hours = Mathf.FloorToInt(timeCounter / 3600);
            int minutes = Mathf.FloorToInt(timeCounter % 3600 / 60);
            int seconds = Mathf.FloorToInt(timeCounter % 60);

            timeCounterTxt.text = string.Format("{0:00}:{1:00}:{2:00}", hours, minutes, seconds);
        }



        if (Input.GetKeyDown(KeyCode.F4))
        {
            RestartScene();
        }

        if (Input.GetKeyDown(KeyCode.F1))
        {
            SceneManager.LoadScene("Area1");
        }
        if (Input.GetKeyDown(KeyCode.F2))
        {
            SceneManager.LoadScene("Area2");
        }
        if (Input.GetKeyDown(KeyCode.F3))
        {
            SceneManager.LoadScene("Area3");
        }

        if (Input.GetButtonDown("Start") && canPause && started)
        {
            TogglePaused();
            TogglePanel(pausePanel);
            if (pausePanel.activeSelf && controlsPanel.activeSelf)
            {
                controlsPanel.SetActive(false);
            }
            UpdateSelectedButton(resumePauseBTN);
            TelemetryManager.Log(pausePanel.activeSelf ? "pause_open" : "pause_close");
        }
        if (Input.GetButtonDown("Y"))
        {
            TelemetryManager.Log("mission_panel_toggle");
            OpenMission();
        }
    }

    //------------------------------------------------------- Management voids -----------------------------------------------------------------------------
    public void StartGame()
    {
        started = true;
        previousScene = "Area1";
        currentScene = "Area1";
        spawnPoint = "spawnPoint Area1";
        starsAchieved = 0;
        canPause = true;
        levelFinisheds.Clear();
        DataManager.Instance.ResetAllData();
        ResetStars();
        UpdateJewelsPanel();
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
            TelemetryManager.Log("jewel_earned",
                ("level", levelName),
                ("total_jewels", starsAchieved),
                ("game_time_s", timeCounter));

            if (levelName.Contains("Musical"))
            {
                musicalStars++;
            }
            else if (levelName.Contains("Receitas"))
            {
                recipesStars++;
            }
            else if (levelName.Contains("Espelhos"))
            {
                mirrorsStars++;
            }
            else if (levelName.Contains("Conjuntos"))
            {
                groupsStars++;
            }

            if (levelName.Contains("1"))
            {
                area1Count++;
            }
            else if (levelName.Contains("2"))
            {
                area2Count++;
            }
            else if (levelName.Contains("3"))
            {
                area3Count++;
            }
        }
        UpdateJewelsPanel();
    }
    public void ResetStars()
    {
        recipesStars = 0;
        groupsStars = 0;
        musicalStars = 0;
        mirrorsStars = 0;
    }
    public void UpdateJewelsPanel()
    {
        recipesStarsTxt.text = "" + recipesStars;
        groupsStarsTxt.text = "" + groupsStars;
        musicalStarsTxt.text = "" + musicalStars;
        mirrorsStarsTxt.text = "" + mirrorsStars;
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
        GameManager.Instance.canPause = false;
        int randonIndex = Random.Range(0, bgPool.Length);
        loadBG.sprite = bgPool[randonIndex];
        loadPanel.SetActive(true);
        Time.timeScale = 0;
        yield return new WaitForSecondsRealtime(1);
        Time.timeScale = 1;
        yield return new WaitForSecondsRealtime(1);
        loadPanel.SetActive(false);
        GameManager.Instance.canPause = true;
        OpenMission();
    }
    //--------------------------------------------------- End Management voids -----------------------------------------------------------------------------
    //------------------------------------------------------- Pause voids----------------------------------------------------------------------------
    public void TogglePanel(GameObject panel)
    {
        panel.SetActive(!panel.activeSelf);
    }
    public void UpdateSelectedButton(GameObject targetButton)
    {
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(targetButton);
    }
    public void TogglePaused()
    {
        if (Time.timeScale <= 0)
        {
            Time.timeScale = 1;
        }
        else
        {
            Time.timeScale = 0;
        }
    }
    public void RestartScene()
    {
        TelemetryManager.Log("scene_restart");
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    public void BackToHome()
    {
        started = false;
        if (TelemetryManager.Instance != null) TelemetryManager.Instance.EndSession("back_to_menu");
        SceneManager.LoadScene("MainMenu");
    }
    public void OpenMission()
    {
        if (GameObject.Find("CanvasMission") != null)
        {
            GameObject painel = GameObject.Find("CanvasMission");
            painel.transform.GetChild(0).gameObject.SetActive(!painel.transform.GetChild(0).gameObject.activeSelf);
        }
    }

    //----------------------------------------------------- End Pause voids ----------------------------------------------------------------------------
    //------------------------------------------------------- HUD voids----------------------------------------------------------------------------

    public void ShowControlPopup(int i)
    {
        controlPopups[i].SetActive(true);
    }
    public void HideAllControlPopup()
    {
        foreach (var popup in controlPopups)
        {
            popup.SetActive(false);
        }
    }
    //----------------------------------------------------- End Pause voids ----------------------------------------------------------------------------

}
