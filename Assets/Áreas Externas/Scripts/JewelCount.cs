using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class JewelCount : MonoBehaviour
{
    public TextMeshProUGUI countTxt;
    public GameObject[] jewels, doors;
    public int areaID;
    void Start()
    {
        switch (areaID)
        {
            case 1:
                countTxt.text = "" + GameManager.Instance.area1Count;
                break;
            case 2:
                countTxt.text = "" + GameManager.Instance.area2Count;
                break;
            case 3:
                countTxt.text = "" + GameManager.Instance.area3Count;
                break;
        }
        for (int i = 0; i < doors.Length; i++)
        {
            jewels[i].SetActive(doors[i].activeSelf);
        }
    }
}
