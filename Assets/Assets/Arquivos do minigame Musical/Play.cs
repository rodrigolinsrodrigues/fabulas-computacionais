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
    public Transform Ponto0,Ponto1,Ponto2,Ponto3,Ponto4;
    public GameObject botaoPlay;
    public GameObject repeticao, ponteiro, pontoRepet;
    public int qtdRepete = 1;

    void Start()
    {
        if(botaoPlay)
        EventSystem.current.SetSelectedGameObject(botaoPlay);

    }
    public void Update()
    {
        if(tocar == true)
        {
            Pontos(Ponto4);
        }  
    }

     void OnTriggerEnter2D(Collider2D other)
    {
       if(other.gameObject.tag == "Notas")
       {
            string name = other.gameObject.name;
            noteObj = GameObject.Find(name);
            noteObj.GetComponent<AudioSource>().Play();
       } 

        if(other.gameObject.tag == "Ponto4")
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

        
}

