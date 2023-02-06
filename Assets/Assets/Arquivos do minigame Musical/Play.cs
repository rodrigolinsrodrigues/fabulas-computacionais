using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Play : MonoBehaviour
{
    public float playSpeed;
    public bool tocar = false;
    public GameObject noteObj, RefPonto;
    public Transform Ponto0,Ponto1,Ponto2,Ponto3,Ponto4, Ponto5;
    public GameObject botaoPlay;
    public GameObject repeticao, ponteiro, pontoRepet;
    public int qtdRepete = 1;

    //teste de loop
    public List<GameObject> lista = new List<GameObject>(); // lista de botoes clicados
    public GameObject ultimo, primeiro;
    public GameObject[] pontos_;

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
        lista.RemoveAt(0); // remove primeiro item da lista
        }

        MetodoDePlayMusica();
    }

     void OnTriggerEnter2D(Collider2D other)
    {
       if(other.gameObject.tag == "Notas")
       {
            string name = other.gameObject.name;
            noteObj = GameObject.Find(name);
            noteObj.GetComponent<AudioSource>().Play();
       } 

        if(other.gameObject.tag == "Ponto5")
       {
            string name = other.gameObject.name;
            RefPonto = GameObject.Find(name);
            tocar = false;  
       } 

    }

    public void Tocar()
    {
        transform.position = Ponto0.position;
        tocar = true;
    }
    public void Pontos(Transform Ponto_)
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
        int x,y;
        x = int.Parse(lista[0].gameObject.name);
        y = int.Parse(lista[1].gameObject.name);

    // antes de começar o ponteiro
    if (x > y) // vai separar os botoes entre ultimo e primeiro
    {
        repeticao = pontos_[y];
        pontoRepet = pontos_[x];
    }
    else
    {
        repeticao = pontos_[y];
        pontoRepet = pontos_[x];
    }
        
    }
}

