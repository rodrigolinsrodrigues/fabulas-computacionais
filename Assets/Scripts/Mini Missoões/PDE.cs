using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PDE : MonoBehaviour
{
    // PDE (Pontos de Entrega)
        //Script dedicado a controlar missões que envolvem a entrega de objetos para pontos especificos;
        // A ideia é que seja utilizado apenas para missões desse tipo, possuindo variações dependendo do resultado da missão;
        // Exemplo: Missões de entrega que liberam passagens. objetos ou outros objetivos. Basta adcionar aqui a condição especifica.

        public bool passagem,obj, concluido, checagem;
        public int layer;
        public GameObject carryObj, area, Objetivo, bloqueio;
        public int Objetos;
        public PDE função;
     void Update()
    {

        if(passagem == true) // Bool responsavel por gerenciar missão onde o o objetivo é liberar uma passagem
        {
            if(area.transform.childCount == Objetos)
            {
            concluido = true;
            Objetivo.SetActive(false);
            }
        } 

        if(obj == true) // Bool responsavel por gerenciar missão onde o o objetivo é liberar um objeto
       {
            if(area.transform.childCount == Objetos)
            {
            concluido = true;
            if(Objetivo != null)
            Objetivo.SetActive(true);
            }
       }
        

    }
     void OnTriggerStay(Collider other) 
    {
        // Codigo responsavel por identificar os objetos, pegando informações de Tag e Layers
        if(other.GetComponent<Collider>().tag == "Pickable" && other.GetComponent<Collider>().gameObject.layer == layer)
        {
        string name = other.GetComponent<Collider>().name;
        carryObj = GameObject.Find(name);
        carryObj.transform.SetParent(this.transform);
        carryObj.GetComponent<Rigidbody>().useGravity = true;
        carryObj.GetComponent<Rigidbody>().constraints = ~RigidbodyConstraints.FreezePosition;
        }
    }

    void OnTriggerExit(Collider other) 
    {
        if(other.GetComponent<Collider>().tag == "Player" && concluido == true)
         {
            bloqueio.SetActive(true);
            função.enabled = false;
         }
         
        
    }
}
