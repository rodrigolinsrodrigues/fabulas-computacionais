using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


public class MainMenu : MonoBehaviour
{
    public GameObject homePanel, aboutPanel, signupPanel;
    public GameObject backAboutBTN, playMenuBTN;

    public void OpenSignup()
    {
        signupPanel.SetActive(!signupPanel.activeSelf);
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
