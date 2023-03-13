using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChecarConclusao : MonoBehaviour
{
    // Script utilizado em situações de Missões PDE que precisem de uma verificação extra
    public GameObject[] Missoes; // lista de missões a serem realizadas;
    public GameObject Objetivo1, objetivo2, Cam; // Objetivo final das missões;
    public int Concluidos; // Checa as missões concluidas
    
    IEnumerator Delay()
    {
        Cam.SetActive(true);
        Objetivo1.SetActive(true);
        objetivo2.SetActive(false);
        yield return new WaitForSeconds(3);
        Cam.SetActive(false);
        this.GetComponent<ChecarConclusao>().enabled = false;
    }
    void Update()
        {
            if(Concluidos == Missoes.Length) // Verifica o numero de missões concluidas baseada na quantidade de missões descritas no Inspector, e libera o objetivo da missão
            {
               StartCoroutine("Delay");
               
            }
        }
    
}
