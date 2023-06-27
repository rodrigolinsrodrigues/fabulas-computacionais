using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MudarOrdem : MonoBehaviour
{
   
    public GameObject EssePilar;
    public GameObject[] Texts;

    void Update()
    {
       if(Texts[0].GetComponent<TextMeshPro>().text == "" && Texts[1].GetComponent<TextMeshPro>().text != "")
       {
            EssePilar.GetComponent<DescLuz>().text = Texts[0];
            Texts[0].GetComponent<TextMeshPro>().text = EssePilar.GetComponent<DescLuz>().Des;
       }

       else if(Texts[0].GetComponent<TextMeshPro>().text != "" && Texts[1].GetComponent<TextMeshPro>().text == "")
       {
            EssePilar.GetComponent<DescLuz>().text = Texts[1];
            Texts[0].GetComponent<TextMeshPro>().text = EssePilar.GetComponent<DescLuz>().Des;
       }
    }
}
