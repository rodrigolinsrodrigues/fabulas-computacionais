using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public GameObject homePanel, aboutPanel;
    public GameObject backAboutBTN, playMenuBTN;
    public void StartGame()
    {
        GameManager.Instance.StartGame();
        SceneManager.LoadScene("Area1");
    }

    public void About()
    {
        homePanel.SetActive(!homePanel.activeSelf);
        aboutPanel.SetActive(!aboutPanel.activeSelf);
        if (aboutPanel.activeSelf)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(backAboutBTN);
        }
        else
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(playMenuBTN);
        }
    }

    public void Exit()
    {
        Application.Quit();
    }
}
