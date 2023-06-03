using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ApagarNota : MonoBehaviour
{
    // Script para controlar as funções do botão de deletar a nota
    public GameObject fechar, apagar;
    public Sprite original;
    public GameObject Coluna;
    void Update()
    {
        if(this.gameObject.GetComponent<AudioSource>().clip != null && fechar.activeSelf == false)
        {
            apagar.SetActive(true);
        }
    }

    public void Apagar()
    {
        this.gameObject.GetComponent<AudioSource>().clip = null;
        this.gameObject.GetComponent<Image>().sprite = original;
        for(int i = 0; i < 3; i++)
        {
           Coluna.gameObject.transform.GetChild(i).GetComponent<Button>().interactable = true;
           Coluna.gameObject.transform.GetChild(i).gameObject.transform.GetChild(2).gameObject.transform.GetChild(0).GetComponent<SelecionarNota>().ColunaFechada = false;
        }
       
    }
    
}
