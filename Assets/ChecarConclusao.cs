using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChecarConclusao : MonoBehaviour
{

    public GameObject[] Missoes;
    //public bool podeIncrementar;
    public GameObject Objetivo;
    public int Concluidos;
    // Start is called before the first frame update
   
    // Update is called once per frame
    void Update()
        {
            if(Concluidos == Missoes.Length)
            {
            Objetivo.SetActive(true);
            }
        }
    
}
