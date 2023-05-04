using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelecionarNota : MonoBehaviour
{
    //Script resposavel por controlar todo esquema de atrbuição de sons as notas disponiveis
    public AudioSource SomDaNota;
    public GameObject Coluna;
    public SonsPorInstrumento SPI;
    private string nomeClipe;
    void Update()
    {
        if(SomDaNota != null)
        {
            nomeClipe = SomDaNota.clip.ToString();
        }
       
        //Notas Partitura 1
        // Violao
        if (SPI.ViolaoPart1 == true && this.gameObject.name.Contains("Part 1") && nomeClipe.Contains("agudo_x1"))
        {
            SomDaNota.clip = SPI.SonsViolaoAgudo[0];
        }
        if (SPI.ViolaoPart1 == true && this.gameObject.name.Contains("Part 1") && nomeClipe.Contains("agudo_x2"))
        {
            SomDaNota.clip = SPI.SonsViolaoAgudo[1];
        }
        if (SPI.ViolaoPart1 == true && this.gameObject.name.Contains("Part 1") && nomeClipe.Contains("agudo_x3"))
        {
            SomDaNota.clip = SPI.SonsViolaoAgudo[2];
        }
        if (SPI.ViolaoPart1 == true && this.gameObject.name.Contains("Part 1") && nomeClipe.Contains("agudo_x4"))
        {
            SomDaNota.clip = SPI.SonsViolaoAgudo[3];
        }

        if (SPI.ViolaoPart1 == true && this.gameObject.name.Contains("Part 1") && nomeClipe.Contains("medio_x1"))
        {
            SomDaNota.clip = SPI.SonsViolaoMed[0];
        }
         if (SPI.ViolaoPart1 == true && this.gameObject.name.Contains("Part 1") && nomeClipe.Contains("medio_x2"))
        {
            SomDaNota.clip = SPI.SonsViolaoMed[1];
        }
         if (SPI.ViolaoPart1 == true && this.gameObject.name.Contains("Part 1") && nomeClipe.Contains("medio_x3"))
        {
            SomDaNota.clip = SPI.SonsViolaoMed[2];
        }
         if (SPI.ViolaoPart1 == true && this.gameObject.name.Contains("Part 1") && nomeClipe.Contains("medio_x4"))
        {
            SomDaNota.clip = SPI.SonsViolaoMed[3];
        }

         if (SPI.ViolaoPart1 == true && this.gameObject.name.Contains("Part 1") && nomeClipe.Contains("grave_x1"))
        {
            SomDaNota.clip = SPI.SonsViolaoGrave[0];
        }
        if (SPI.ViolaoPart1 == true && this.gameObject.name.Contains("Part 1") && nomeClipe.Contains("grave_x2"))
        {
            SomDaNota.clip = SPI.SonsViolaoGrave[1];
        }
        if (SPI.ViolaoPart1 == true && this.gameObject.name.Contains("Part 1") && nomeClipe.Contains("grave_x3"))
        {
            SomDaNota.clip = SPI.SonsViolaoGrave[2];
        }
        if (SPI.ViolaoPart1 == true && this.gameObject.name.Contains("Part 1") && nomeClipe.Contains("grave_x4"))
        {
            SomDaNota.clip = SPI.SonsViolaoGrave[3];
        }

        // Handpan
         if (SPI.HandpanPart1 == true && this.gameObject.name.Contains("Part 1") && nomeClipe.Contains("agudo_x1"))
        {
            SomDaNota.clip = SPI.SonsHandpanAgudo[0];
        }
        if (SPI.HandpanPart1 == true && this.gameObject.name.Contains("Part 1") && nomeClipe.Contains("agudo_x2"))
        {
            SomDaNota.clip = SPI.SonsHandpanAgudo[1];
        }
        if (SPI.HandpanPart1 == true && this.gameObject.name.Contains("Part 1") && nomeClipe.Contains("agudo_x3"))
        {
            SomDaNota.clip = SPI.SonsHandpanAgudo[2];
        }
        if (SPI.HandpanPart1 == true && this.gameObject.name.Contains("Part 1") && nomeClipe.Contains("agudo_x4"))
        {
            SomDaNota.clip = SPI.SonsHandpanAgudo[3];
        }

         if (SPI.HandpanPart1 == true && this.gameObject.name.Contains("Part 1") && nomeClipe.Contains("medio_x1"))
        {
            SomDaNota.clip = SPI.SonsHandpanMed[0];
        }
         if (SPI.HandpanPart1 == true && this.gameObject.name.Contains("Part 1") && nomeClipe.Contains("medio_x2"))
        {
            SomDaNota.clip = SPI.SonsHandpanMed[1];
        }
         if (SPI.HandpanPart1 == true && this.gameObject.name.Contains("Part 1") && nomeClipe.Contains("medio_x3"))
        {
            SomDaNota.clip = SPI.SonsHandpanMed[2];
        }
         if (SPI.HandpanPart1 == true && this.gameObject.name.Contains("Part 1") && nomeClipe.Contains("medio_x4"))
        {
            SomDaNota.clip = SPI.SonsHandpanMed[3];
        }

         if (SPI.HandpanPart1 == true && this.gameObject.name.Contains("Part 1") && nomeClipe.Contains("grave_x1"))
        {
            SomDaNota.clip = SPI.SonsHandpanGrave[0];
        }
        if (SPI.HandpanPart1 == true && this.gameObject.name.Contains("Part 1") && nomeClipe.Contains("grave_x2"))
        {
            SomDaNota.clip = SPI.SonsHandpanGrave[1];
        }
        if (SPI.HandpanPart1 == true && this.gameObject.name.Contains("Part 1") && nomeClipe.Contains("grave_x3"))
        {
            SomDaNota.clip = SPI.SonsHandpanGrave[2];
        }
        if (SPI.HandpanPart1 == true && this.gameObject.name.Contains("Part 1") && nomeClipe.Contains("grave_x4"))
        {
            SomDaNota.clip = SPI.SonsHandpanGrave[3];
        }


        
        //Notas Partitura 2
        // Violao
        if (SPI.ViolaoPart2 == true && this.gameObject.name.Contains("Part 2") && nomeClipe.Contains("nota_x1"))
        {
            SomDaNota.clip = SPI.SonsViolaoAgudo[0];
        }
        if (SPI.ViolaoPart2 == true && this.gameObject.name.Contains("Part 2") && nomeClipe.Contains("nota_x2"))
        {
            SomDaNota.clip = SPI.SonsViolaoAgudo[1];
        }
        if (SPI.ViolaoPart2 == true && this.gameObject.name.Contains("Part 2") && nomeClipe.Contains("nota_x3"))
        {
            SomDaNota.clip = SPI.SonsViolaoAgudo[2];
        }
        if (SPI.ViolaoPart2 == true && this.gameObject.name.Contains("Part 2") && nomeClipe.Contains("nota_x4"))
        {
            SomDaNota.clip = SPI.SonsViolaoAgudo[3];
        }

        if (SPI.ViolaoPart2 == true && this.gameObject.name.Contains("Part 2") && nomeClipe.Contains("nota_x1"))
        {
            SomDaNota.clip = SPI.SonsViolaoMed[0];
        }
         if (SPI.ViolaoPart2 == true && this.gameObject.name.Contains("Part 2") && nomeClipe.Contains("nota_x2"))
        {
            SomDaNota.clip = SPI.SonsViolaoMed[1];
        }
         if (SPI.ViolaoPart2 == true && this.gameObject.name.Contains("Part 2") && nomeClipe.Contains("nota_x3"))
        {
            SomDaNota.clip = SPI.SonsViolaoMed[2];
        }
         if (SPI.ViolaoPart2 == true && this.gameObject.name.Contains("Part 2") && nomeClipe.Contains("nota_x4"))
        {
            SomDaNota.clip = SPI.SonsViolaoMed[3];
        }

         if (SPI.ViolaoPart2 == true && this.gameObject.name.Contains("Part 2") && nomeClipe.Contains("nota_x1"))
        {
            SomDaNota.clip = SPI.SonsViolaoGrave[0];
        }
        if (SPI.ViolaoPart2 == true && this.gameObject.name.Contains("Part 2") && nomeClipe.Contains("nota_x2"))
        {
            SomDaNota.clip = SPI.SonsViolaoGrave[1];
        }
        if (SPI.ViolaoPart2 == true && this.gameObject.name.Contains("Part 2") && nomeClipe.Contains("nota_x3"))
        {
            SomDaNota.clip = SPI.SonsViolaoGrave[2];
        }
        if (SPI.ViolaoPart2 == true && this.gameObject.name.Contains("Part 2") && nomeClipe.Contains("nota_x4"))
        {
            SomDaNota.clip = SPI.SonsViolaoGrave[3];
        }

        // Handpan
         if (SPI.HandpanPart2 == true && this.gameObject.name.Contains("Part 2") && nomeClipe.Contains("agudo_x1"))
        {
            SomDaNota.clip = SPI.SonsHandpanAgudo[0];
        }
        if (SPI.HandpanPart2 == true && this.gameObject.name.Contains("Part 2") && nomeClipe.Contains("agudo_x2"))
        {
            SomDaNota.clip = SPI.SonsHandpanAgudo[1];
        }
        if (SPI.HandpanPart2 == true && this.gameObject.name.Contains("Part 2") && nomeClipe.Contains("agudo_x3"))
        {
            SomDaNota.clip = SPI.SonsHandpanAgudo[2];
        }
        if (SPI.HandpanPart2 == true && this.gameObject.name.Contains("Part 2") && nomeClipe.Contains("agudo_x4"))
        {
            SomDaNota.clip = SPI.SonsHandpanAgudo[3];
        }

         if (SPI.HandpanPart2 == true && this.gameObject.name.Contains("Part 2") && nomeClipe.Contains("medio_x1"))
        {
            SomDaNota.clip = SPI.SonsHandpanMed[0];
        }
         if (SPI.HandpanPart2 == true && this.gameObject.name.Contains("Part 2") && nomeClipe.Contains("medio_x2"))
        {
            SomDaNota.clip = SPI.SonsHandpanMed[1];
        }
         if (SPI.HandpanPart2 == true && this.gameObject.name.Contains("Part 2") && nomeClipe.Contains("medio_x3"))
        {
            SomDaNota.clip = SPI.SonsHandpanMed[2];
        }
         if (SPI.HandpanPart2 == true && this.gameObject.name.Contains("Part 2") && nomeClipe.Contains("medio_x4"))
        {
            SomDaNota.clip = SPI.SonsHandpanMed[3];
        }

         if (SPI.HandpanPart2 == true && this.gameObject.name.Contains("Part 2") && nomeClipe.Contains("grave_x1"))
        {
            SomDaNota.clip = SPI.SonsHandpanGrave[0];
        }
        if (SPI.HandpanPart2 == true && this.gameObject.name.Contains("Part 2") && nomeClipe.Contains("grave_x2"))
        {
            SomDaNota.clip = SPI.SonsHandpanGrave[1];
        }
        if (SPI.HandpanPart2 == true && this.gameObject.name.Contains("Part 2") && nomeClipe.Contains("grave_x3"))
        {
            SomDaNota.clip = SPI.SonsHandpanGrave[2];
        }
        if (SPI.HandpanPart2 == true && this.gameObject.name.Contains("Part 2") && nomeClipe.Contains("grave_x4"))
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
