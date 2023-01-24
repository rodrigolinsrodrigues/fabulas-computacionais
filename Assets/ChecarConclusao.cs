using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChecarConclusao : MonoBehaviour
{

    public GameObject[] Missoes;
    public int Concluidos, Conclusoes;
    public GameObject Objetivo;
    public bool  AllTrue;
    // Start is called before the first frame update
   
    // Update is called once per frame
    void Update()
    {
        for(int i = 0; i < Missoes.Length; i++)
        {
            if(Missoes[i].GetComponent<PDE>().concluido == false)
            { 
                AllTrue = false;
                break;
            }
            else
            AllTrue = true;
            
            
        }
        

        // foreach (bool obj in Missoes) {
        //     if (Missoes.All(obj => obj.GetComponent<PDE>().concluiu))
        //     {
        //         Debug.Log(obj);
        //     }
        // }
        
        // if(Concluidos == Conclusoes)
        // {
        //     Debug.Log("Concluido");
        // }
    }


    
}
