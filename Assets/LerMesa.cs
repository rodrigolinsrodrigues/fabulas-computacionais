using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LerMesa : MonoBehaviour
{
    public GameObject[] Mesas;
    public int[] Insumos;
    public int[] Acoes;
    public bool comecar, parar;
    public GameObject Player;
    public Transform[] LocalPLayer;

    public GameObject[] Cams;
    // public string[] Receita1;
    // public string[] Receita2;
    
   
    void Update()
    {
        if(comecar == true && parar == false)
        {
            StartCoroutine("TempoDeEspera");
            Player.GetComponent<PlayerController>().enabled = false;
            Player.GetComponent<PickUp>().enabled = false;
            Cams[0].SetActive(false);
            Cams[1].SetActive(true);
           

        }
        if(parar == true)
        {
        comecar = false; 
        Player.GetComponent<PlayerController>().enabled = true;
        Player.GetComponent<PickUp>().enabled = true;
        Cams[0].SetActive(true);
        Cams[1].SetActive(false);
        parar = false;
        }
       
    }

    IEnumerator TempoDeEspera()
    {
        Debug.Log("Iniciar Leitura");
           
            for(int i = 0; i < Mesas.Length; i++)
            {
                if(Mesas[i].transform.childCount < 3)
                {
                    Debug.Log("Parar");
                    parar = true;
                    comecar = false;
                    break;
                }

                else
                {
                      // Insumos
                    switch(Mesas[i].transform.GetChild(2).gameObject.layer)
                    {
                        case 10:
                        Debug.Log(Mesas[i] + "É Pão");
                        Player.transform.position = LocalPLayer[i].position;
                        break;

                        case 8:
                        Debug.Log(Mesas[i] + "É Queijo");
                        Player.transform.position = LocalPLayer[i].position;
                        break;

                        case 7:
                        Debug.Log(Mesas[i] + "É Queijo");
                        Player.transform.position = LocalPLayer[i].position;
                        break;

                    }
                      yield return new WaitForSeconds(5);
                    

                    // if(Mesas[i].transform.GetChild(2).gameObject.layer == Insumos[0])
                    // {
                   
                    // Debug.Log(Mesas[i] + "É Pão");
                    // }

                    // else if(Mesas[i].transform.GetChild(2).gameObject.layer == Insumos[1])
                    // {
                    // Player.transform.position = LocalPLayer[i].position;
                    // Debug.Log(Mesas[i] + "É Queijo");
                    // }

                    // else if(Mesas[i].transform.GetChild(2).gameObject.layer == Insumos[2])
                    // {
                    // Player.transform.position = LocalPLayer[i].position;
                    // Debug.Log(Mesas[i] + "É Ovo");
                    // }
                    
                    //Ações
                    // if(Mesas[i].transform.GetChild(3).gameObject.layer == Acoes[0])
                    // {
                    // Player.transform.position = LocalPLayer[i].position;
                    // Debug.Log(Mesas[i] + "Cortar");
                    // }

                  

                //Checar combinações
                // if(Mesas[i].transform.GetChild(2).name.Contains("Pao") && Mesas[i].transform.GetChild(3).name.Contains("Cortar"))
                // {
                // Debug.Log("Pão Cortado");
                // }
                 
                }
                
            }
             Debug.Log("Parar");
            parar = true;
    }
     void Receita1()
        {
       
        }
}
