using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FecharEspaco : MonoBehaviour
{
    public GameObject[] Animados;
     public GameObject LE; 
    public bool OK = false;
    void Start()
    {
        LE = GameObject.FindWithTag("LE");
    }
    void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            OK = true;
            //LE.GetComponent<LeituraEspacos>().Check();
            Animados[0].GetComponent<Animator>().SetBool("Fechar", true);
            Animados[1].GetComponent<Animator>().SetBool("Fechar", true);
            Destroy(this.GetComponent<FecharEspaco>());
        }
    }
}
