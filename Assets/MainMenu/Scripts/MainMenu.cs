using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public GameObject homePanel, aboutPanel;
    public void StartGame()
    {
        GameManager.Instance.StartGame();
        SceneManager.LoadScene("Area1");
    }

    public void About()
    {
        homePanel.SetActive(!homePanel.activeSelf);
        aboutPanel.SetActive(!aboutPanel.activeSelf);
    }

    public void Exit()
    {
        Application.Quit();
    }
}
