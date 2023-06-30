using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FecharEspaco : MonoBehaviour
{
    public GameObject[] Animados;
   
    void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            Animados[0].GetComponent<Animator>().SetBool("Fechar", true);
            Animados[1].GetComponent<Animator>().SetBool("Fechar", true);
        }
    }

}
