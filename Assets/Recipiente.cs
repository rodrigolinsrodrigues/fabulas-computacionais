using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Recipiente : MonoBehaviour
{
     private Comandos comandos;
   public GameObject[] Recursos;
    // public List<GameObject> Recursos = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        comandos = new Comandos();
        comandos.Actions.Enable(); 
    }

    void OnTriggerStay(Collider other)
    {
        // Player
        if(other.GetComponent<Collider>().tag == "Player" && other.GetComponent<PickUp>().holding == false)
        {
            for(int i = 0; i < Recursos.Length; i++)
            {
                this.transform.GetChild(i);
                Recursos[0] = this.transform.GetChild(0).gameObject; 
                Recursos[1] = this.transform.GetChild(1).gameObject; 
                Recursos[2] = this.transform.GetChild(2).gameObject; 
                Recursos[3] = this.transform.GetChild(3).gameObject; 
                Recursos[4] = this.transform.GetChild(4).gameObject; 
                if(Recursos[0] != null)
                {
                    Recursos[0].SetActive(true);
                    if(this.transform.GetChild(0) == null){Recursos[0] = null;}
                    
                }
                if(Recursos[1] != null)
                {
                    Recursos[1].SetActive(true);
                    if(this.transform.GetChild(1) == null){Recursos[1] = null;}
                    
                }
                if(Recursos[2] != null)
                {
                    Recursos[2].SetActive(true);
                    if(this.transform.GetChild(2) == null){Recursos[2] = null;}
                    
                }
                if(Recursos[3] != null)
                {
                    Recursos[3].SetActive(true);
                    if(this.transform.GetChild(3) == null){Recursos[3] = null;}
                    
                }
                if(Recursos[3] != null)
                {
                    Recursos[3].SetActive(true);
                    if(this.transform.GetChild(3) == null){Recursos[3] = null;}
                    
                }

            }
           
            
        }

    }
    //   void OnTriggerExit(Collider other)
    //     {
    //         for(int i = 0; i < Recursos.Length; i++)
    //         {
    //         if(other.GetComponent<Collider>().tag == "Player" && other.GetComponent<PickUp>().holding == true)
    //         {
    //             Recursos[i] = null;
    //         }
    //         }
            
    //     }
}

