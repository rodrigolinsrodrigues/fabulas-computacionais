using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class limitarNotas : MonoBehaviour
{
    public GameObject[] Notas;
    public float Quantidade;
    public TextMeshProUGUI restam;
    // Update is called once per frame
    void Update()
    {
        restam.text = "x " +  Quantidade.ToString();
        if(Quantidade == 0f)
        {
            for(int i = 0; i < Notas.Length; i++)
            {
                Notas[i].GetComponent<Button>().interactable = false;
            }
        }


         for(int i = 0; i < Notas.Length; i++)
        {
            if(Notas[i].transform.GetChild(2).gameObject.transform.GetChild(0).GetComponent<SelecionarNota>().ColunaFechada == false && Quantidade != 0)
            {
            Notas[i].GetComponent<Button>().interactable = true; 
            }
            if(Notas[i].transform.GetChild(2).gameObject.transform.GetChild(0).GetComponent<SelecionarNota>().ColunaFechada == true && Quantidade != 0)
            {
            Notas[i].GetComponent<Button>().interactable = false;   
            } 
        }
    }
    public void Add()
    {
        Quantidade = Quantidade + 1f;
    }
}
