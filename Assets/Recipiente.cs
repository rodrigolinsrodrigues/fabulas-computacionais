using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Recipiente : MonoBehaviour
{
     private Comandos comandos;
  // public GameObject[] Recursos;
   public bool PodePegar, PodeSoltar;
   public GameObject ObjRecurso;
    public List<GameObject> Recursos = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        comandos = new Comandos();
        comandos.Actions.Enable(); 
    }
     void Update() 
    {
        //Recursos[0] = this.transform.GetChild(0).gameObject;
    }
     void OnTriggerStay(Collider other)
    {
        if(other.GetComponent<Collider>().tag == "Player" && other.GetComponent<PickUp>().holding == false)
        {
            if(Recursos[0] != null)
            {
            Recursos[0].SetActive(true);
            PodePegar = true;
            }
            
        }

        if(other.GetComponent<Collider>().tag == "Player" && other.GetComponent<PickUp>().holding == true && PodePegar == true)
        {
        Recursos.RemoveAt(0);
        PodePegar = false;
        }


        if(other.GetComponent<Collider>().tag == "Player" && other.GetComponent<PickUp>().holding == true)
        {
            PodePegar = false;
            PodeSoltar = true;
            ObjRecurso = other.transform.GetChild(1).gameObject;
            if(other.GetComponent<Collider>().tag == "Player" && other.GetComponent<PickUp>().holding == false && PodePegar == false && PodeSoltar == true)
            {
             ObjRecurso.transform.parent = this.transform.parent;
            }
        }

    }      
        // if(other.GetComponent<Collider>().tag == "Player" && other.GetComponent<PickUp>().holding == false)
        // {
        //     for(int i = 0; i < Recursos.Length; i++)
        //     {
        //         if(Recursos[i] != null)
        //         {
        //         Recursos[i].SetActive(true);
        //         break;
        //         } 
        //     }
        // }
       
   
    void OnTriggerExit(Collider other)
    {  
        // for(int i = 0; i < Recursos.Length; i++)
        // {
        //     if(other.GetComponent<PickUp>().holding == true)
        //     {
        //         if(this.transform.GetChild(i) == null){Recursos[i] = null; break;}
        //     }
           
            
        // }
       
      
    }
    // void OnTriggerStay(Collider other)
    // {
    //     // Player
    //     if(other.GetComponent<Collider>().tag == "Player" && other.GetComponent<PickUp>().holding == false)
    //     {
    //             if(Recursos[0] != null)
    //             {
    //                 Recursos[0].SetActive(true);
    //                 if(other.GetComponent<PickUp>().holding == true)
    //                 {
    //                     Recursos[0] = null; 
    //                 }
                   
    //             }
    //             else
    //             if(Recursos[0] == null && Recursos[1] != null)
    //             {
    //                 Recursos[1].SetActive(true);
    //                 if(other.GetComponent<PickUp>().holding == true)
    //                 {
    //                     Recursos[1] = null; 
    //                 }
                    
    //             }
    //             else
    //             if(Recursos[1] == null && Recursos[2] != null)
    //             {
    //                 Recursos[2].SetActive(true);
    //                 if(this.transform.GetChild(2) == null){Recursos[2] = null;}
                    
    //             }
    //             else
    //             if(Recursos[2] == null && Recursos[3] != null)
    //             {
    //                 Recursos[3].SetActive(true);
    //                 if(this.transform.GetChild(3) == null){Recursos[3] = null;}
                    
    //             }
    //             else
    //             if(Recursos[3] != null && Recursos[4] != null)
    //             {
    //                 Recursos[4].SetActive(true);
    //                 if(this.transform.GetChild(4) == null){Recursos[4] = null;}
                    
    //             }  
    //     }
    // }


}

