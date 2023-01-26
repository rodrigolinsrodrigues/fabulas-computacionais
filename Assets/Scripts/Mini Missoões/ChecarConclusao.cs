using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChecarConclusao : MonoBehaviour
{
    // Script utilizado em situações de Missões PDE que precisem de uma verificação extra
    public GameObject[] Missoes; // lista de missões a serem realizadas;
    public GameObject Objetivo; // Objetivo final das missões;
    public int Concluidos; // Checa as missões concluidas
    public bool ativar, deativar;
   
    void Update()
        {
            if(Concluidos == Missoes.Length) // Verifica o numero de missões concluidas baseada na quantidade de missões descritas no Inspector, e libera o objetivo da missão
            {
                if(ativar == true)
                {
                    Objetivo.SetActive(true);
                }
                if(deativar == true)
                {
                    Objetivo.SetActive(false);
                }
           
            }
        }
    
}
