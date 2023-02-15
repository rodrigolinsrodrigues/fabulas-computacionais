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

    // Start is called before the first frame update
    void Start()
    {
        comandos = new Comandos();
        comandos.Actions.Enable(); 
    }
    
     void OnTriggerStay(Collider other)
    {
        // Checa se o player entrou na area de colis~]ao e se n esta com nada em mãos
        if(other.GetComponent<Collider>().tag == "Player" && other.GetComponent<PickUp>().holding == false)
        {
            if(Recursos[0] != null)
            {
            Recursos[0].SetActive(true); // Ativar o objeto na cena para ser pego
            PodePegar = true;
            }
            
        }
        
        //Se o player pegou o obj
        if(other.GetComponent<Collider>().tag == "Player" && other.GetComponent<PickUp>().holding == true && PodePegar == true)
        {
        Recursos.RemoveAt(0); // remove da lista
        PodePegar = false;
        }

        //Se o player estiver segurando o obj para repor no Recipiente
        if(other.GetComponent<Collider>().tag == "Player" && other.GetComponent<PickUp>().holding == true)
        {
            PodePegar = false;
            PodeSoltar = true;
            ObjRecurso = other.transform.GetChild(1).gameObject; // pega o filho do player (mao)
            ObjRecurso = ObjRecurso.transform.GetChild(0).gameObject; // Pega o filho da mao (item)
            Debug.Log("Funcionando a leitura do filho");
        }

        //Se entrar na area com o obj e soltar no recipiente
        if(other.GetComponent<Collider>().tag == "Player" && other.GetComponent<PickUp>().holding == false && PodeSoltar == true)
        {
            ObjRecurso.transform.SetParent(area.transform);
           // Recursos[0].SetActive(false);
        
            Debug.Log("Adicionar objeto de volta");
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
        // if(other.GetComponent<Collider>().tag == "Player")
        //  {
        //     NPegou = other.GetComponent<PickUp>().holding = false;
        //     Pegou =  other.GetComponent<PickUp>().holding = true;
        //  }

   
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

