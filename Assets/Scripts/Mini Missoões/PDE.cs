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
        public int layer; // Layer do objeto correspondente a missão;
        public GameObject carryObj, area, Objetivo, bloqueio;
        public int Objetos; // Faz a contagem de objetos necessarios para a conclusão da missão;
        public PDE funcao; // Referencia o codigo atual;
        public ChecarConclusao CC; // Faz referencia ao codigo de checagem de missões PDE;
     void Update()
    {

        if(passagem == true) // Bool responsavel por gerenciar missão onde o o objetivo é liberar uma passagem
        {
            if(area.transform.childCount == Objetos)
            {
            if(checagem == false)
            Objetivo.SetActive(false);
            }
        } 

        if(obj == true) // Bool responsavel por gerenciar missão onde o o objetivo é liberar um objeto
       {
            if(area.transform.childCount == Objetos)
            {
            if(checagem == false)
            Objetivo.SetActive(true);
            }
       }
        

    }
     void OnTriggerStay(Collider other) // Quando o jogador entra na area da missão com o objeto correto em mãos
    {
        // Codigo responsavel por identificar os objetos, pegando informações de Tag e Layers
        if(other.GetComponent<Collider>().tag == "Pickable" && other.GetComponent<Collider>().gameObject.layer == layer) // Checa a Tag e a layer do objeto correspondente a missão
        {
        string name = other.GetComponent<Collider>().name;
        carryObj = GameObject.Find(name);
        if(carryObj.transform.parent == null)
        {
        carryObj.transform.SetParent(this.transform);
        carryObj.GetComponent<Rigidbody>().useGravity = true;
        carryObj.GetComponent<Rigidbody>().constraints = ~RigidbodyConstraints.FreezePosition;
        }
        }
    }

    void OnTriggerExit(Collider other) // Quando o player sair da area da missão
    {
        if(other.GetComponent<Collider>().tag == "Player" && area.transform.childCount == Objetos) // identifica o player pela tag
         {
            bloqueio.SetActive(true); // Ativa o bloqueio da area, impedindo o player de interagir com ela novamente
            if(CC != null && concluido == false) // Se o codigo de checagem não for nulo e as conclusões forem falsas
            {
                concluido = true;
                CC.Concluidos += 1; // adciona mais um a lista de objetivos concluidos
            }
            funcao.enabled = false; //Desativa esse codigo, ja que n será mais utlizado, consumindo menos processamento
        }
            
    }
}
