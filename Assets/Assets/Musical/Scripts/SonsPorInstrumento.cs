using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SonsPorInstrumento : MonoBehaviour
{
  //Script que serve como banco de dados para todos os registros de audio do mini game musical, possuindo todas as faixas existentes e podendo ser incrementado para novas faixas, seguindo o mesmo padrão
    public bool ViolaoPart1,HandpanPart1,ViolaoPart2,HandpanPart2; // Bools para seleção de instrumento em cada partitura
    public AudioClip[] SonsViolaoAgudo, SonsViolaoMed, SonsViolaoGrave; // Arrays com todas as faixas de audio do violão
    public AudioClip[] SonsHandpanAgudo, SonsHandpanMed, SonsHandpanGrave; // Arrays com todas as faixas de audio do Handpan
    public GameObject[] Agudo, Agudox2,Agudox3,Agudox4; 
    public GameObject[] Medio, Mediox2, Mediox3, Mediox4;
     public GameObject[] Grave, Gravex2,Gravex3,Gravex4;
    // Update is called once per frame
    void Update()
    {
      //Partitura 1
      if(ViolaoPart1 == true)
      {
        InstrumentosPart1(SonsViolaoAgudo, SonsViolaoMed, SonsViolaoGrave);
      }
      if(HandpanPart1 == true)
      {
        InstrumentosPart1(SonsHandpanAgudo, SonsHandpanMed, SonsHandpanGrave);
      }

      //Partitura 2
      if(ViolaoPart2 == true)
      {
        InstrumentosPart2(SonsViolaoAgudo, SonsViolaoMed, SonsViolaoGrave);
      }

      if(HandpanPart2 == true)
      {
        InstrumentosPart2(SonsHandpanAgudo, SonsHandpanMed, SonsHandpanGrave);
      } 
    }

    void InstrumentosPart1(AudioClip[]Agudos, AudioClip[]Medios, AudioClip[]Graves)
    {
      //Agudo = GameObject.Find.name.Contains("Part 1 - Agudo");
      for(int i = 0; i < Agudo.Length; i++)
      {
      Agudo[i].GetComponent<AudioSource>().clip = Agudos[0];
      }

      //Agudox2 = GameObject.FindGameObjectsWithTag("Agudo x2");
      for(int i = 0; i < Agudox2.Length; i++)
      {
      Agudox2[i].GetComponent<AudioSource>().clip = Agudos[1];
      }

      //Agudox3 = GameObject.FindGameObjectsWithTag("Agudo x3");
      for(int i = 0; i < Agudox3.Length; i++)
      {
      Agudox3[i].GetComponent<AudioSource>().clip = Agudos[2];
      }
      //Agudox4 = GameObject.FindGameObjectsWithTag("Agudo x4");
      for(int i = 0; i < Agudox4.Length; i++)
      {
      Agudox4[i].GetComponent<AudioSource>().clip = Agudos[3];
      }

      //Medio = GameObject.FindGameObjectsWithTag("Medio");
      for(int i = 0; i < Medio.Length; i++)
      {
      Medio[i].GetComponent<AudioSource>().clip = Medios[0];
      }

      //Mediox2 = GameObject.FindGameObjectsWithTag("Medio x2");
      for(int i = 0; i < Mediox2.Length; i++)
      {
      Mediox2[i].GetComponent<AudioSource>().clip = Medios[1];
      }

      //Mediox3 = GameObject.FindGameObjectsWithTag("Medio x3");
      for(int i = 0; i < Mediox3.Length; i++)
      {
      Mediox3[i].GetComponent<AudioSource>().clip = Medios[2];
      }
      //Mediox4 = GameObject.FindGameObjectsWithTag("Medio x4");
      for(int i = 0; i < Mediox4.Length; i++)
      {
      Mediox4[i].GetComponent<AudioSource>().clip = Medios[3];
      }

      //Grave = GameObject.FindGameObjectsWithTag("Grave");
      for(int i = 0; i < Grave.Length; i++)
      {
      Grave[i].GetComponent<AudioSource>().clip = Graves[0];
      }
      //Gravex2 = GameObject.FindGameObjectsWithTag("Grave x2");
      for(int i = 0; i < Gravex2.Length; i++)
      {
      Gravex2[i].GetComponent<AudioSource>().clip = Graves[1];
      }
      //Gravex3 = GameObject.FindGameObjectsWithTag("Grave x3");
      for(int i = 0; i < Gravex3.Length; i++)
      {
      Gravex3[i].GetComponent<AudioSource>().clip = Graves[2];
      }
      //Gravex4 = GameObject.FindGameObjectsWithTag("Grave x4");
      for(int i = 0; i < Gravex4.Length; i++)
      {
      Gravex4[i].GetComponent<AudioSource>().clip = Graves[3];
      }

    }

    void InstrumentosPart2(AudioClip[]Agudos, AudioClip[]Medios, AudioClip[]Graves)
    {

      //Agudo = GameObject.FindGameObjectsWithTag("Agudo");
      for(int i = 0; i < Agudo.Length; i++)
      {
      Agudo[i].GetComponent<AudioSource>().clip = Agudos[0];
      }

      //Agudox2 = GameObject.FindGameObjectsWithTag("Agudo x2");
      for(int i = 0; i < Agudox2.Length; i++)
      {
      Agudox2[i].GetComponent<AudioSource>().clip = Agudos[1];
      }

      //Agudox3 = GameObject.FindGameObjectsWithTag("Agudo x3");
      for(int i = 0; i < Agudox3.Length; i++)
      {
      Agudox3[i].GetComponent<AudioSource>().clip = Agudos[2];
      }
      //Agudox4 = GameObject.FindGameObjectsWithTag("Agudo x4");
      for(int i = 0; i < Agudox4.Length; i++)
      {
      Agudox4[i].GetComponent<AudioSource>().clip = Agudos[3];
      }

      //Medio = GameObject.FindGameObjectsWithTag("Medio");
      for(int i = 0; i < Medio.Length; i++)
      {
      Medio[i].GetComponent<AudioSource>().clip = Medios[0];
      }

      //Mediox2 = GameObject.FindGameObjectsWithTag("Medio x2");
      for(int i = 0; i < Mediox2.Length; i++)
      {
      Mediox2[i].GetComponent<AudioSource>().clip = Medios[1];
      }

      //Mediox3 = GameObject.FindGameObjectsWithTag("Medio x3");
      for(int i = 0; i < Mediox3.Length; i++)
      {
      Mediox3[i].GetComponent<AudioSource>().clip = Medios[2];
      }
      //Mediox4 = GameObject.FindGameObjectsWithTag("Medio x4");
      for(int i = 0; i < Mediox4.Length; i++)
      {
      Mediox4[i].GetComponent<AudioSource>().clip = Medios[3];
      }

      //Grave = GameObject.FindGameObjectsWithTag("Grave");
      for(int i = 0; i < Grave.Length; i++)
      {
      Grave[i].GetComponent<AudioSource>().clip = Graves[0];
      }
      //Gravex2 = GameObject.FindGameObjectsWithTag("Grave x2");
      for(int i = 0; i < Gravex2.Length; i++)
      {
      Gravex2[i].GetComponent<AudioSource>().clip = Graves[1];
      }
      //Gravex3 = GameObject.FindGameObjectsWithTag("Grave x3");
      for(int i = 0; i < Gravex3.Length; i++)
      {
      Gravex3[i].GetComponent<AudioSource>().clip = Graves[2];
      }
      //Gravex4 = GameObject.FindGameObjectsWithTag("Grave x4");
      for(int i = 0; i < Gravex4.Length; i++)
      {
      Gravex4[i].GetComponent<AudioSource>().clip = Graves[3];
      }

    }
    

    //Partitura 1
     public void violaoPart1()
    { 
        HandpanPart1 = false;
        ViolaoPart1 = true;
    }
    public void handpanPart1()
    {
        ViolaoPart1 = false;
        HandpanPart1 = true;
    }
    // Partitura 2
     public void violaoPart2()
    { 
        HandpanPart2 = false;
        ViolaoPart2 = true;
    }
    public void handpanPart2()
    {
        ViolaoPart2 = false;
        HandpanPart2 = true;
    }
}
