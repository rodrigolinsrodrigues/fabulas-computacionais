using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Play : MonoBehaviour
{
    // Codigo responsavel por todo o controle de execução da partitura, além de controlar parte do sistema de repetição, atrelado a um objeto Reprodutor
    public float playSpeed;
    public bool tocar = false;
    public GameObject noteObj, RefPonto;
    public Transform Ponto0,Ponto1,Ponto2,Ponto3,Ponto4, Ponto5;
    public GameObject botaoPlay, ContadorRepeticao;
    public GameObject repeticao, ponteiro, pontoRepet;
    public int qtdRepete = 0;

    //teste de loop
    public List<GameObject> lista = new List<GameObject>(); // lista de botoes clicados
    public GameObject ultimo, primeiro;
    public GameObject[] pontos_;

    // Cor
    public Color original;
     int x,y;

    void Start()
    {
        if(botaoPlay)
        EventSystem.current.SetSelectedGameObject(botaoPlay);

    }
    public void Update()
    {
        if(tocar == true)
        {
            Pontos(Ponto5);
        }  

        //loop
        if (lista.Count > 2) // se tiver mais que dois botoes
        {
        var CorOriginal = GetComponent<Button>().colors;
        CorOriginal.normalColor = original;

        lista[0].transform.GetChild(1).gameObject.SetActive(false);
        lista[0].gameObject.GetComponent<Button>().colors = CorOriginal;
        lista.RemoveAt(0); // remove primeiro item da lista
        }

        MetodoDePlayMusica();
    }

     void OnTriggerEnter2D(Collider2D other) // Quando esse objeto entra em contato com as notas os sons são reproduzidos
    {
       if(other.gameObject.tag == "Notas")
       {
            string name = other.gameObject.name;
            noteObj = GameObject.Find(name);
            noteObj.GetComponent<AudioSource>().Play();
       } 

        if(other.gameObject.tag == "Ponto5") // Ao colidir com esse ponto o reprodutor para de tocar
       {
            string name = other.gameObject.name;
            RefPonto = GameObject.Find(name);
            tocar = false;  
       } 

    }

    public void Tocar() // Atrelado ao botão Play, faz com que a linha do reprodutor volte ao inicio e comece a execução
    {
        transform.position = Ponto0.position;
        tocar = true;
    }
    public void Pontos(Transform Ponto_) // Controlador de Pontos, os pontos nada mais são que os exatos pontos de intervalo das musicas, cada valor do ponto corresponde a seu numero em segundos.
    {
        if(transform.position.x != Ponto_.position.x)
        {
            transform.Translate(Vector3.right * Time.deltaTime * playSpeed);
        } 
    }

    void OnTriggerExit2D (Collider2D col)
    {
    if (col.gameObject.name == repeticao.gameObject.name)
    {
    if (qtdRepete > 0)
    {
        ponteiro.transform.position = pontoRepet.transform.position;
        qtdRepete--;
    }
    }
    }

    void MetodoDePlayMusica()
    {
        x = int.Parse(lista[0].gameObject.name);
        y = int.Parse(lista[1].gameObject.name);
        
        var CorSelecionada = GetComponent<Button>().colors;
        CorSelecionada.normalColor = Color.red;

        

    if(lista.Count == 2)
    {
        
        if (x > y) // vai separar os botoes entre ultimo e primeiro
        {
           

            repeticao = pontos_[x]; // Ultimo
            pontoRepet = pontos_[y]; // Primeiro
            lista[0].transform.GetChild(1).gameObject.SetActive(true);
            lista[1].transform.GetChild(1).gameObject.SetActive(true);
            lista[0].gameObject.GetComponent<Button>().colors = CorSelecionada;
            lista[1].gameObject.GetComponent<Button>().colors = CorSelecionada;
            
        }
        else
        {
            repeticao = pontos_[y];
            pontoRepet = pontos_[x];
            lista[0].transform.GetChild(1).gameObject.SetActive(true);
            lista[1].transform.GetChild(1).gameObject.SetActive(true);
            lista[0].gameObject.GetComponent<Button>().colors = CorSelecionada;
            lista[1].gameObject.GetComponent<Button>().colors = CorSelecionada;
           

        }
        ContadorRepeticao.SetActive(true);
    }
    else
    {
         ContadorRepeticao.SetActive(false);
    }
    // antes de começar o ponteiro 
    }

    // Voids atribuidos aos botões que adicionam repetições
    public void X2(){qtdRepete = 1;}
    public void X3(){qtdRepete = 2;}
    public void X4(){ qtdRepete = 3;}
}

