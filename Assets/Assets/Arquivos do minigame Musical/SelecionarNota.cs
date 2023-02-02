using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelecionarNota : MonoBehaviour
{
    public AudioSource SomDaNota;
    public GameObject Coluna;
    // Start is called before the first frame update

    public void Solo()
    {
        for(int i = 0; i < 3; i++)
        {
            Coluna.gameObject.transform.GetChild(i).GetComponent<AudioSource>().enabled = false;
        }
        SomDaNota.clip = this.GetComponent<AudioSource>().clip;
        SomDaNota.enabled = true;
    }
    public void Dupla()
    {
     for(int i = 0; i < 3; i++)
        {
            Coluna.gameObject.transform.GetChild(i).GetComponent<AudioSource>().enabled = false;
        }
        SomDaNota.clip = this.GetComponent<AudioSource>().clip;
        SomDaNota.enabled = true;
     }
    public void tripla()
    {
         for(int i = 0; i < 3; i++)
        {
            Coluna.gameObject.transform.GetChild(i).GetComponent<AudioSource>().enabled = false;
        }
        SomDaNota.clip = this.GetComponent<AudioSource>().clip;
        SomDaNota.enabled = true;
    }
    public void Quadra()
    { 
        for(int i = 0; i < 3; i++)
        {
            Coluna.gameObject.transform.GetChild(i).GetComponent<AudioSource>().enabled = false;
        }
        SomDaNota.clip = this.GetComponent<AudioSource>().clip;
        SomDaNota.enabled = true;
    }
   
}
