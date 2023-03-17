using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Recipiente : MonoBehaviour
{
     private Comandos comandos; // acessar controles do Player
  // public GameObject[] Recursos;
   public bool PodePegar, PodeSoltar; //Verificação
   public GameObject ObjRecurso, area; // Nome dado a objetos
    public List<GameObject> Recursos = new List<GameObject>(); // Lista contendo os objetos
     public int layer;
    // Start is called before the first frame update
    void Start()
    {
        comandos = new Comandos();
        comandos.Actions.Enable(); 
    }

     void OnTriggerStay(Collider other)
    {
        // Checa se o player entrou na area de colis~]ao e se n esta com nada em mãos
        if(other.GetComponent<Collider>().tag == "Player" && other.GetComponent<PickUp>().holding == false && Recursos[0] != null && comandos.Actions.Interact.ReadValue<float>() == 1)
        {
            // Ativar o objeto na cena para ser pego
            Recursos[0].SetActive(true);
            PodePegar = true;
             //Se o player pegou o obj
        }
       
        //Se o player estiver segurando o obj para repor no Recipiente
        if(other.GetComponent<Collider>().tag == "Pickable" && other.gameObject.layer == layer && other.gameObject.transform.parent == null)
        {
            Recursos.Add(other.gameObject);
            other.gameObject.transform.position = area.transform.position;
            other.gameObject.SetActive(false);
            other.gameObject.transform.SetParent(area.transform);
            PodePegar = true;
          
        }
        
    }   

     void OnTriggerExit(Collider other)
    {  
        if(Recursos[0] != null)
        {
            if(Recursos[0].transform.parent != area.transform)
            {
             Recursos.RemoveAt(0); // ??
             PodePegar = false; // remove da lista
            }
        }
          
    }  

}

