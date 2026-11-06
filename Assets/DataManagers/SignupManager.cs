using System;
using System.Globalization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class SignupManager : MonoBehaviour
{
    public string playerName;
    public string gender, schoolGrade;
    public int age;
    public InputField inputAge, inputName, inputSchoolGrade;

    void Start()
    {
        SetGender(0);
    }
    public void EnterName()
    {
        playerName = inputName.text.ToUpper();

    }
    public void SetBirthday()
    {
        int.TryParse(inputAge.text, out age);
    }

    public void SetSchoolGrade()
    {
        schoolGrade = inputSchoolGrade.text;
    }

    public void SetGender(int option)
    {
        if (option == 0)
        {
            gender = "m";
        }
        else if (option == 1)
        {
            gender = "f";
        }
        else
        {
            gender = "outro";
        }
    }

    /// <summary>Pode ser ligado a um Toggle "Autorizo o uso da câmera" no painel de cadastro.</summary>
    public void SetCameraConsent(bool value)
    {
        if (AffectManager.Instance != null)
        {
            AffectManager.Instance.SetConsent(value);
        }
    }

    public void SavePersonalDataAndStart()
    {
        DataManager.Instance.playerName = playerName;
        DataManager.Instance.SetPlayerNameOnUI(playerName);
        DataManager.Instance.playerAge = age;
        DataManager.Instance.playerGender = gender;
        DataManager.Instance.schoolGrade = schoolGrade;

        if (age != 0 && playerName != "" && playerName.Length >= 3 && schoolGrade != "")
        {
            // Learning analytics: inicia a sessão (e a câmera, se habilitada) antes da Area1.
            if (TelemetryManager.Instance != null)
            {
                TelemetryManager.Instance.StartSession(playerName, age, gender, schoolGrade);
            }

            SceneManager.LoadScene("Area1");
            GameManager.Instance.StartGame();
        }
    }
}

