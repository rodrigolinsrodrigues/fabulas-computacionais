using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LerMesa : MonoBehaviour
{
    public GameObject[] Mesas;
    public int[] Insumos;
    public int[] Acoes;
    public bool comecar;
    public bool  Pao, Queijo, Ovo, Cortar, Fritar;
    public GameObject PCortado;
    
    // Update is called once per frame
    void Update()
    {
        if(comecar == true)
        {
            Debug.Log("Iniciar Leitura");
            for(int i = 0; i < Mesas.Length; i++)
            {
                if(Mesas[i].transform.GetChild(2).gameObject != null)
                {
                    if(Mesas[i].transform.GetChild(2).gameObject.layer == Insumos[0])
                    {
                    Pao = true;
                    if(Pao == true){Debug.Log(Mesas[i] + "É Pão");}
                    }
                    if(Mesas[i].transform.GetChild(2).gameObject.layer == Insumos[1])
                    {
                    Queijo = true;
                    if(Queijo == true){Debug.Log(Mesas[i] + "É Queijo");}
                    }
                    if(Mesas[i].transform.GetChild(2).gameObject.layer == Insumos[2])
                    {
                    Ovo = true;
                    if(Ovo == true){ Debug.Log(Mesas[i] + "É Ovo");}
                    }
                }
                if(Mesas[i].transform.GetChild(3).gameObject != null)
                {

                    if(Mesas[i].transform.GetChild(3).gameObject.layer == Acoes[0])
                    {
                    Cortar = true; 
                    if(Cortar == true){Debug.Log(Mesas[i] + "Cortar");}
                    }
                }

                 //Checar pão cortado
                if(Mesas[i].transform.GetChild(2).name.Contains("Pao") && Mesas[i].transform.GetChild(3).name.Contains("Cortar"))
                {
                Debug.Log("Pão Cortado");
                }












            }
        }

       


       
    }


    void Receita1()
    {
        if(Pao == true && Queijo == true)
        {
            comecar = false;
            Debug.Log("Pao com Queijo");
        }
    }
}
