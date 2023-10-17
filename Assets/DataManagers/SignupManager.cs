using System;
using System.Globalization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class SignupManager : MonoBehaviour
{
    public string playerName;
    public string gender = "masculino";
    public int age;
    public InputField inputAge, inputName;

    public void LoadData()
    {

    }

    public void EnterName()
    {
        playerName = inputName.text.ToUpper();

    }
    public void SetBirthday()
    {
        int.TryParse(inputAge.text, out age);
    }

    public void SetGender(int option)
    {
        if (option == 0)
        {
            gender = "masculino";
        }
        else if (option == 1)
        {
            gender = "feminino";
        }
        else
        {
            gender = "outro";
        }
    }

    public void SaveAgeNGender()
    {
        DataManager.Instance.playerAge = age;
        DataManager.Instance.playerGender = gender;
    }
}

