using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelecionarNota : MonoBehaviour
{
    public AudioSource SomDaNota;
    public GameObject Coluna;
    public SonsPorInstrumento SPI;

    void Update()
    {
       
    
         string nomeClipe = SomDaNota.clip.ToString();
        
        // Violao
        if (nomeClipe.Contains("agudo_x1") && SPI.Violao == true)
        {
            SomDaNota.clip = SPI.SonsViolaoAgudo[0];
        }
        if (nomeClipe.Contains("agudo_x2") && SPI.Violao == true)
        {
            SomDaNota.clip = SPI.SonsViolaoAgudo[1];
        }
        if (nomeClipe.Contains("agudo_x3") && SPI.Violao == true)
        {
            SomDaNota.clip = SPI.SonsViolaoAgudo[2];
        }
        if (nomeClipe.Contains("agudo_x4") && SPI.Violao == true)
        {
            SomDaNota.clip = SPI.SonsViolaoAgudo[3];
        }

         if (nomeClipe.Contains("medio_x1") && SPI.Violao == true)
        {
            SomDaNota.clip = SPI.SonsViolaoMed[0];
        }
         if (nomeClipe.Contains("medio_x2") && SPI.Violao == true)
        {
            SomDaNota.clip = SPI.SonsViolaoMed[1];
        }
         if (nomeClipe.Contains("medio_x3") && SPI.Violao == true)
        {
            SomDaNota.clip = SPI.SonsViolaoMed[2];
        }
         if (nomeClipe.Contains("medio_x4") && SPI.Violao == true)
        {
            SomDaNota.clip = SPI.SonsViolaoMed[3];
        }

         if (nomeClipe.Contains("grave_x1") && SPI.Violao == true)
        {
            SomDaNota.clip = SPI.SonsViolaoGrave[0];
        }
        if (nomeClipe.Contains("grave_x2") && SPI.Violao == true)
        {
            SomDaNota.clip = SPI.SonsViolaoGrave[1];
        }
        if (nomeClipe.Contains("grave_x3") && SPI.Violao == true)
        {
            SomDaNota.clip = SPI.SonsViolaoGrave[2];
        }
        if (nomeClipe.Contains("grave_x4") && SPI.Violao == true)
        {
            SomDaNota.clip = SPI.SonsViolaoGrave[3];
        }

        // Handpan
         if (nomeClipe.Contains("agudo_x1") && SPI.Handpan == true)
        {
            SomDaNota.clip = SPI.SonsHandpanAgudo[0];
        }
        if (nomeClipe.Contains("agudo_x2") && SPI.Handpan == true)
        {
            SomDaNota.clip = SPI.SonsHandpanAgudo[1];
        }
        if (nomeClipe.Contains("agudo_x3") && SPI.Handpan == true)
        {
            SomDaNota.clip = SPI.SonsHandpanAgudo[2];
        }
        if (nomeClipe.Contains("agudo_x4") && SPI.Handpan == true)
        {
            SomDaNota.clip = SPI.SonsHandpanAgudo[3];
        }

         if (nomeClipe.Contains("medio_x1") && SPI.Handpan == true)
        {
            SomDaNota.clip = SPI.SonsHandpanMed[0];
        }
         if (nomeClipe.Contains("medio_x2") && SPI.Handpan == true)
        {
            SomDaNota.clip = SPI.SonsHandpanMed[1];
        }
         if (nomeClipe.Contains("medio_x3") && SPI.Handpan == true)
        {
            SomDaNota.clip = SPI.SonsHandpanMed[2];
        }
         if (nomeClipe.Contains("medio_x4") && SPI.Handpan == true)
        {
            SomDaNota.clip = SPI.SonsHandpanMed[3];
        }

         if (nomeClipe.Contains("grave_x1") && SPI.Handpan == true)
        {
            SomDaNota.clip = SPI.SonsHandpanGrave[0];
        }
        if (nomeClipe.Contains("grave_x2") && SPI.Handpan == true)
        {
            SomDaNota.clip = SPI.SonsHandpanGrave[1];
        }
        if (nomeClipe.Contains("grave_x3") && SPI.Handpan == true)
        {
            SomDaNota.clip = SPI.SonsHandpanGrave[2];
        }
        if (nomeClipe.Contains("grave_x4") && SPI.Handpan == true)
        {
            SomDaNota.clip = SPI.SonsHandpanGrave[3];
        }

    }
    public void Solo()
    {
        for(int i = 0; i < 3; i++)
        {
            Coluna.gameObject.transform.GetChild(i).GetComponent<AudioSource>().enabled = false;
            Coluna.gameObject.transform.GetChild(i).GetComponent<Button>().interactable = false;
        }
        SomDaNota.clip = this.GetComponent<AudioSource>().clip;
        SomDaNota.enabled = true;
       
    }
    public void Dupla()
    {
     for(int i = 0; i < 3; i++)
        {
            Coluna.gameObject.transform.GetChild(i).GetComponent<AudioSource>().enabled = false;
            Coluna.gameObject.transform.GetChild(i).GetComponent<Button>().interactable = false;
        }
        SomDaNota.clip = this.GetComponent<AudioSource>().clip;
        SomDaNota.enabled = true;
     }
    public void tripla()
    {
         for(int i = 0; i < 3; i++)
        {
            Coluna.gameObject.transform.GetChild(i).GetComponent<AudioSource>().enabled = false;
            Coluna.gameObject.transform.GetChild(i).GetComponent<Button>().interactable = false;
        }
        SomDaNota.clip = this.GetComponent<AudioSource>().clip;
        SomDaNota.enabled = true;
    }
    public void Quadra()
    { 
        for(int i = 0; i < 3; i++)
        {
            Coluna.gameObject.transform.GetChild(i).GetComponent<AudioSource>().enabled = false;
            Coluna.gameObject.transform.GetChild(i).GetComponent<Button>().interactable = false;
        }
        SomDaNota.clip = this.GetComponent<AudioSource>().clip;
        SomDaNota.enabled = true;
    }
   
}
