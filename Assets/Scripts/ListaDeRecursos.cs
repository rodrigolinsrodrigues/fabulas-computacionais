using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ListaDeRecursos : MonoBehaviour
{
    [Header("Sandwiches")]
    [Space]
    public GameObject[] traditionalBread;
    public GameObject[] sesameBread;
    public GameObject[] boxBread;
    public GameObject[] egg;
    public GameObject[] cheese;
    public GameObject[] meat;
    public GameObject[] bacon;
    public GameObject[] ham;
    public GameObject[] pepper;

    [Header("Juices")]
    [Space]
    public GameObject[] orange;
    public GameObject[] pineapple;
    public GameObject[] mango;
    public GameObject[] mint;
    public GameObject[] passionFruit;

    public GameObject SelectFood(string name, int state)
    {
        switch (name)
        {
            case "traditionalBread":
                return traditionalBread[state];

            case "sesameBread":
                return sesameBread[state];

            case "boxBread":
                return boxBread[state];

            case "egg":
                return egg[state];

            case "cheese":
                return cheese[state];

            case "meat":
                return meat[state];

            case "bacon":
                return bacon[state];

            case "ham":
                return ham[state];

            case "pepper":
                return pepper[state];

            case "orange":
                return orange[state];

            case "pineapple":
                return pineapple[state];

            case "mango":
                return mango[state];

            case "mint":
                return mint[state];

            case "passionFruit":
                return passionFruit[state];
        }
        
        return null;
    }

}

