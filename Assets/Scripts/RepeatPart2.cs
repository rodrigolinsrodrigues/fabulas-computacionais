using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class RepeatPart2 : MonoBehaviour
{
    // script de reptição apenas da partitura 2
    // Start is called before the first frame update
    public Transform Ponto0,Ponto1,Ponto2,Ponto3,Ponto4,Ponto5Pt1,Ponto5Pt2;
    public GameObject[] ContadorRepeticao;

    public GameObject  ponteiro, repeticao, pontoRepet, deletar;
    public int qtdRepete = 0;
    public bool delete;
    //teste de loop
    public List<GameObject> lista = new List<GameObject>(); // lista de botoes clicados
    //public GameObject ultimo, primeiro;
    public GameObject[] pontos_;
    // Cor
    public Color original;

     public int x,y;
     public GameObject[] Repet;

     public Sprite spriteOriginal, spriteNovo, x2,x3,x4;
    // Update is called once per frame
    void Update()
    {
         //loop
        if (lista.Count > 2) // se tiver mais que dois botoes
        {
        var CorOriginal = GetComponent<Button>().colors;
        CorOriginal.normalColor = original;

        lista[0].transform.GetChild(1).gameObject.SetActive(false);
        lista[0].gameObject.GetComponent<Button>().colors = CorOriginal;
        lista[0].gameObject.GetComponent<Image>().sprite = spriteOriginal;
        lista.RemoveAt(0); // remove primeiro item da lista
        }

        if(delete == true)
        {

        var CorOriginal = GetComponent<Button>().colors;
        CorOriginal.normalColor = original;

        qtdRepete = 0;
        lista[0].transform.GetChild(1).gameObject.SetActive(false);
       // lista[0].gameObject.GetComponent<Button>().colors = CorOriginal;
        lista[0].gameObject.GetComponent<Image>().sprite = spriteOriginal;
        lista.RemoveAt(0); // remove primeiro item da lista

        lista[1].transform.GetChild(1).gameObject.SetActive(false);
        //lista[1].gameObject.GetComponent<Button>().colors = CorOriginal;
        lista[1].gameObject.GetComponent<Image>().sprite = spriteOriginal;
        lista.RemoveAt(1); // remove primeiro item da lista

        }

        MetodoDePlayMusica();
    }

     void OnTriggerExit2D (Collider2D col) // Verifica as repetiçoes
    {
    if (col.gameObject.name == repeticao.gameObject.name) // checa se o ponteiro colidiu com o primeiro ponto de repetição
    {
    if (qtdRepete > 0) // se a quantidade de repetições for maior que 0
    {
        ponteiro.transform.position = pontoRepet.transform.position; // checa se o ponteiro colidiu com o ultimo ponto de reptição
        qtdRepete--; // dimniniu a quantidade de repetições
    }
    }
    }

    void MetodoDePlayMusica() // controla o esquema de repetições
    {
        // X e Y se tratam dos pontos de loop
        x = int.Parse(lista[0].gameObject.name);
        y = int.Parse(lista[1].gameObject.name);
        
        var CorSelecionada = GetComponent<Button>().colors;
        CorSelecionada.normalColor = Color.red;


    if(lista.Count == 2)
    {
       deletar.GetComponent<Image>().enabled = true;
       deletar.GetComponent<Button>().enabled = true;
        if (x > y) // vai separar os botoes entre ultimo e primeiro
        {
           
            repeticao = pontos_[x]; // Ultimo
            pontoRepet = pontos_[y]; // Primeiro

            if(qtdRepete == 0)
           {
            lista[0].gameObject.GetComponent<Image>().sprite = spriteNovo;
            lista[1].gameObject.GetComponent<Image>().sprite = spriteNovo;
           }
            if(qtdRepete == 1)
           {
            lista[0].gameObject.GetComponent<Image>().sprite = x2;
            lista[1].gameObject.GetComponent<Image>().sprite = x2;
           }
            if(qtdRepete == 2)
           {
            lista[0].gameObject.GetComponent<Image>().sprite = x3;
            lista[1].gameObject.GetComponent<Image>().sprite = x3;
           }
            if(qtdRepete == 3)
           {
            lista[0].gameObject.GetComponent<Image>().sprite = x4;
            lista[1].gameObject.GetComponent<Image>().sprite = x4;
           }

           // lista[0].gameObject.GetComponent<Button>().colors = CorSelecionada;
           // lista[1].gameObject.GetComponent<Button>().colors = CorSelecionada;
            
        }
        else
        {
            repeticao = pontos_[y];
            pontoRepet = pontos_[x];
             if(qtdRepete == 0)
           {
            lista[0].gameObject.GetComponent<Image>().sprite = spriteNovo;
            lista[1].gameObject.GetComponent<Image>().sprite = spriteNovo;
           }
            if(qtdRepete == 1)
           {
            lista[0].gameObject.GetComponent<Image>().sprite = x2;
            lista[1].gameObject.GetComponent<Image>().sprite = x2;
           }
            if(qtdRepete == 2)
           {
            lista[0].gameObject.GetComponent<Image>().sprite = x3;
            lista[1].gameObject.GetComponent<Image>().sprite = x3;
           }
            if(qtdRepete == 3)
           {
            lista[0].gameObject.GetComponent<Image>().sprite = x4;
            lista[1].gameObject.GetComponent<Image>().sprite = x4;
           }

           // lista[0].gameObject.GetComponent<Button>().colors = CorSelecionada;
           // lista[1].gameObject.GetComponent<Button>().colors = CorSelecionada;
           

        }
        for(int i = 0; i <ContadorRepeticao.Length; i++)
        {
            ContadorRepeticao[i].GetComponent<Button>().interactable = true;
        }
        
        for(int x = 0; x < Repet.Length; x++)
        {
            Repet[x].GetComponent<Button>().interactable = false;
        }
    }
   
    // antes de começar o ponteiro 
    }

    // Voids atribuidos aos botões que adicionam repetições
    public void X2(){qtdRepete = 1;}
    public void X3(){qtdRepete = 2;}
    public void X4(){qtdRepete = 3;}
}
