using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SonsPorInstrumento : MonoBehaviour
{
    public bool Violao,Handpan;
    public AudioClip[] SonsViolaoAgudo, SonsViolaoMed, SonsViolaoGrave;
    public AudioClip[] SonsHandpanAgudo, SonsHandpanMed, SonsHandpanGrave;
    public GameObject[] Agudo, Agudox2,Agudox3,Agudox4;
    public GameObject[] Medio, Mediox2, Mediox3, Mediox4;
     public GameObject[] Grave, Gravex2,Gravex3,Gravex4;
    // Update is called once per frame
    void Update()
    {
            if(Violao == true)
            {
                Instrumentos(SonsViolaoAgudo, SonsViolaoMed, SonsViolaoGrave);
            }

            if(Handpan == true)
            {
                Instrumentos(SonsHandpanAgudo, SonsHandpanMed, SonsHandpanGrave);
            }
    }

    void Instrumentos(AudioClip[]Agudos, AudioClip[]Medios, AudioClip[]Graves)
    {

          Agudo = GameObject.FindGameObjectsWithTag("Agudo");
           for(int i = 0; i < Agudo.Length; i++)
           {
            Agudo[i].GetComponent<AudioSource>().clip = Agudos[0];
           }

            Agudox2 = GameObject.FindGameObjectsWithTag("Agudo x2");
           for(int i = 0; i < Agudox2.Length; i++)
           {
            Agudox2[i].GetComponent<AudioSource>().clip = Agudos[1];
           }

            Agudox3 = GameObject.FindGameObjectsWithTag("Agudo x3");
           for(int i = 0; i < Agudox3.Length; i++)
           {
            Agudox3[i].GetComponent<AudioSource>().clip = Agudos[2];
           }
            Agudox4 = GameObject.FindGameObjectsWithTag("Agudo x4");
           for(int i = 0; i < Agudox4.Length; i++)
           {
            Agudox4[i].GetComponent<AudioSource>().clip = Agudos[3];
           }

            Medio = GameObject.FindGameObjectsWithTag("Medio");
           for(int i = 0; i < Medio.Length; i++)
           {
            Medio[i].GetComponent<AudioSource>().clip = Medios[0];
           }

             Mediox2 = GameObject.FindGameObjectsWithTag("Medio x2");
           for(int i = 0; i < Mediox2.Length; i++)
           {
            Mediox2[i].GetComponent<AudioSource>().clip = Medios[1];
           }

             Mediox3 = GameObject.FindGameObjectsWithTag("Medio x3");
           for(int i = 0; i < Mediox3.Length; i++)
           {
            Mediox3[i].GetComponent<AudioSource>().clip = Medios[2];
           }
             Mediox4 = GameObject.FindGameObjectsWithTag("Medio x4");
           for(int i = 0; i < Mediox4.Length; i++)
           {
            Mediox4[i].GetComponent<AudioSource>().clip = Medios[3];
           }

            Grave = GameObject.FindGameObjectsWithTag("Grave");
           for(int i = 0; i < Grave.Length; i++)
           {
            Grave[i].GetComponent<AudioSource>().clip = Graves[0];
           }
             Gravex2 = GameObject.FindGameObjectsWithTag("Grave x2");
           for(int i = 0; i < Gravex2.Length; i++)
           {
            Gravex2[i].GetComponent<AudioSource>().clip = Graves[1];
           }
             Gravex3 = GameObject.FindGameObjectsWithTag("Grave x3");
           for(int i = 0; i < Gravex3.Length; i++)
           {
            Gravex3[i].GetComponent<AudioSource>().clip = Graves[2];
           }
             Gravex4 = GameObject.FindGameObjectsWithTag("Grave x4");
           for(int i = 0; i < Gravex4.Length; i++)
           {
            Gravex4[i].GetComponent<AudioSource>().clip = Graves[3];
           }

    }

     public void violao()
    { 
        Handpan = false;
        Violao = true;
    }
    public void handpan()
    {
        Violao = false;
        Handpan = true;
    }
 
}
