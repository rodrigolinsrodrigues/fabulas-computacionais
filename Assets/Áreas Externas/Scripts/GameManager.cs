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
    public TextMeshProUGUI recipesStarsTxt, groupsStarsTxt, mirrorsStarsTxt, musicalStarsTxt;
    public GameObject[] controlPopups;
    public GameObject resumePauseBTN, controlsPanel;
    public bool canPause = true;
    public Image loadBG;
    public Sprite[] bgPool;

    //-------------------------------------------------------- End Variables -----------------------------------------------------------------------------------

    void Start()
    {
        canPause = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Keypad0))
        {
            RestartScene();
        }
        if (Input.GetButtonDown("Start") && canPause)
        {
            TogglePaused();
            TogglePanel(pausePanel);
            if (pausePanel.activeSelf && controlsPanel.activeSelf)
            {
                controlsPanel.SetActive(false);
            }
            UpdateSelectedButton(resumePauseBTN);
        }
        if (Input.GetButtonDown("Y"))
        {
            OpenMission();
        }
    }

    //------------------------------------------------------- Management voids -----------------------------------------------------------------------------
    public void StartGame()
    {
        previousScene = "Area1";
        currentScene = "Area1";
        spawnPoint = "spawnPoint Area1";
        starsAchieved = 0;
        canPause = true;
        levelFinisheds.Clear();
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
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    public void BackToHome()
    {
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
