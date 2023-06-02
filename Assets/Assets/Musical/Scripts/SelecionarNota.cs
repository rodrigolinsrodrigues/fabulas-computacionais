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
    public string nomeClipe;

    void Update()
    {
        if(SomDaNota.clip.name != null){nomeClipe = SomDaNota.clip.name;}
        if(SPI.ViolaoPart1 == true){ViolaoPart1Check();}
        if(SPI.HandpanPart1 == true){HandpanPart1Check();}
        if(SPI.FlautaPart1 == true){FlautaPart1Check();}
        if(SPI.ViolaoPart2 == true){ViolaoPart2Check();}
        if(SPI.HandpanPart2 == true){HandpanPart2Check();}
        if(SPI.FlautaPart2 == true){FlautaPart2Check();}
        
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
    
    void ViolaoPart1Check()
    {
       
        if (this.gameObject.name.Contains("Part 1") && nomeClipe.Contains("agudo_x1"))
        {
            SomDaNota.clip = SPI.SonsViolaoAgudo[0];
        }
        if (this.gameObject.name.Contains("Part 1") && nomeClipe.Contains("agudo_x2"))
        {
            SomDaNota.clip = SPI.SonsViolaoAgudo[1];
        }
        if (this.gameObject.name.Contains("Part 1") && nomeClipe.Contains("agudo_x3"))
        {
            SomDaNota.clip = SPI.SonsViolaoAgudo[2];
        }
        if (this.gameObject.name.Contains("Part 1") && nomeClipe.Contains("agudo_x4"))
        {
            SomDaNota.clip = SPI.SonsViolaoAgudo[3];
        }

        if (this.gameObject.name.Contains("Part 1") && nomeClipe.Contains("medio_x1"))
        {
            SomDaNota.clip = SPI.SonsViolaoMed[0];
        }
         if (this.gameObject.name.Contains("Part 1") && nomeClipe.Contains("medio_x2"))
        {
            SomDaNota.clip = SPI.SonsViolaoMed[1];
        }
         if (this.gameObject.name.Contains("Part 1") && nomeClipe.Contains("medio_x3"))
        {
            SomDaNota.clip = SPI.SonsViolaoMed[2];
        }
         if (this.gameObject.name.Contains("Part 1") && nomeClipe.Contains("medio_x4"))
        {
            SomDaNota.clip = SPI.SonsViolaoMed[3];
        }

         if (this.gameObject.name.Contains("Part 1") && nomeClipe.Contains("grave_x1"))
        {
            SomDaNota.clip = SPI.SonsViolaoGrave[0];
        }
        if (this.gameObject.name.Contains("Part 1") && nomeClipe.Contains("grave_x2"))
        {
            SomDaNota.clip = SPI.SonsViolaoGrave[1];
        }
        if (this.gameObject.name.Contains("Part 1") && nomeClipe.Contains("grave_x3"))
        {
            SomDaNota.clip = SPI.SonsViolaoGrave[2];
        }
        if (this.gameObject.name.Contains("Part 1") && nomeClipe.Contains("grave_x4"))
        {
            SomDaNota.clip = SPI.SonsViolaoGrave[3];
        }

    }
    void HandpanPart1Check()
    {
         if (this.gameObject.name.Contains("Part 1") && nomeClipe.Contains("agudo_x1"))
        {
            SomDaNota.clip = SPI.SonsHandpanAgudo[0];
        }
        if (this.gameObject.name.Contains("Part 1") && nomeClipe.Contains("agudo_x2"))
        {
            SomDaNota.clip = SPI.SonsHandpanAgudo[1];
        }
        if (this.gameObject.name.Contains("Part 1") && nomeClipe.Contains("agudo_x3"))
        {
            SomDaNota.clip = SPI.SonsHandpanAgudo[2];
        }
        if (this.gameObject.name.Contains("Part 1") && nomeClipe.Contains("agudo_x4"))
        {
            SomDaNota.clip = SPI.SonsHandpanAgudo[3];
        }

         if (this.gameObject.name.Contains("Part 1") && nomeClipe.Contains("medio_x1"))
        {
            SomDaNota.clip = SPI.SonsHandpanMed[0];
        }
         if (this.gameObject.name.Contains("Part 1") && nomeClipe.Contains("medio_x2"))
        {
            SomDaNota.clip = SPI.SonsHandpanMed[1];
        }
         if (this.gameObject.name.Contains("Part 1") && nomeClipe.Contains("medio_x3"))
        {
            SomDaNota.clip = SPI.SonsHandpanMed[2];
        }
         if (this.gameObject.name.Contains("Part 1") && nomeClipe.Contains("medio_x4"))
        {
            SomDaNota.clip = SPI.SonsHandpanMed[3];
        }

         if (this.gameObject.name.Contains("Part 1") && nomeClipe.Contains("grave_x1"))
        {
            SomDaNota.clip = SPI.SonsHandpanGrave[0];
        }
        if (this.gameObject.name.Contains("Part 1") && nomeClipe.Contains("grave_x2"))
        {
            SomDaNota.clip = SPI.SonsHandpanGrave[1];
        }
        if (this.gameObject.name.Contains("Part 1") && nomeClipe.Contains("grave_x3"))
        {
            SomDaNota.clip = SPI.SonsHandpanGrave[2];
        }
        if (this.gameObject.name.Contains("Part 1") && nomeClipe.Contains("grave_x4"))
        {
            SomDaNota.clip = SPI.SonsHandpanGrave[3];
        }

    }
    void ViolaoPart2Check()
    {
         if (this.gameObject.name.Contains("Part 2") && nomeClipe.Contains("nota_x1"))
        {
            SomDaNota.clip = SPI.SonsViolaoAgudo[0];
        }
        if (this.gameObject.name.Contains("Part 2") && nomeClipe.Contains("nota_x2"))
        {
            SomDaNota.clip = SPI.SonsViolaoAgudo[1];
        }
        if (this.gameObject.name.Contains("Part 2") && nomeClipe.Contains("nota_x3"))
        {
            SomDaNota.clip = SPI.SonsViolaoAgudo[2];
        }
        if (this.gameObject.name.Contains("Part 2") && nomeClipe.Contains("nota_x4"))
        {
            SomDaNota.clip = SPI.SonsViolaoAgudo[3];
        }

        if (this.gameObject.name.Contains("Part 2") && nomeClipe.Contains("nota_x1"))
        {
            SomDaNota.clip = SPI.SonsViolaoMed[0];
        }
         if (this.gameObject.name.Contains("Part 2") && nomeClipe.Contains("nota_x2"))
        {
            SomDaNota.clip = SPI.SonsViolaoMed[1];
        }
         if (this.gameObject.name.Contains("Part 2") && nomeClipe.Contains("nota_x3"))
        {
            SomDaNota.clip = SPI.SonsViolaoMed[2];
        }
         if (this.gameObject.name.Contains("Part 2") && nomeClipe.Contains("nota_x4"))
        {
            SomDaNota.clip = SPI.SonsViolaoMed[3];
        }

         if (this.gameObject.name.Contains("Part 2") && nomeClipe.Contains("nota_x1"))
        {
            SomDaNota.clip = SPI.SonsViolaoGrave[0];
        }
        if (this.gameObject.name.Contains("Part 2") && nomeClipe.Contains("nota_x2"))
        {
            SomDaNota.clip = SPI.SonsViolaoGrave[1];
        }
        if (this.gameObject.name.Contains("Part 2") && nomeClipe.Contains("nota_x3"))
        {
            SomDaNota.clip = SPI.SonsViolaoGrave[2];
        }
        if (this.gameObject.name.Contains("Part 2") && nomeClipe.Contains("nota_x4"))
        {
            SomDaNota.clip = SPI.SonsViolaoGrave[3];
        }
    }
    void HandpanPart2Check()
    {
         if (this.gameObject.name.Contains("Part 2") && nomeClipe.Contains("agudo_x1"))
        {
            SomDaNota.clip = SPI.SonsHandpanAgudo[0];
        }
        if (this.gameObject.name.Contains("Part 2") && nomeClipe.Contains("agudo_x2"))
        {
            SomDaNota.clip = SPI.SonsHandpanAgudo[1];
        }
        if (this.gameObject.name.Contains("Part 2") && nomeClipe.Contains("agudo_x3"))
        {
            SomDaNota.clip = SPI.SonsHandpanAgudo[2];
        }
        if (this.gameObject.name.Contains("Part 2") && nomeClipe.Contains("agudo_x4"))
        {
            SomDaNota.clip = SPI.SonsHandpanAgudo[3];
        }

         if (this.gameObject.name.Contains("Part 2") && nomeClipe.Contains("medio_x1"))
        {
            SomDaNota.clip = SPI.SonsHandpanMed[0];
        }
         if (this.gameObject.name.Contains("Part 2") && nomeClipe.Contains("medio_x2"))
        {
            SomDaNota.clip = SPI.SonsHandpanMed[1];
        }
         if (this.gameObject.name.Contains("Part 2") && nomeClipe.Contains("medio_x3"))
        {
            SomDaNota.clip = SPI.SonsHandpanMed[2];
        }
         if (this.gameObject.name.Contains("Part 2") && nomeClipe.Contains("medio_x4"))
        {
            SomDaNota.clip = SPI.SonsHandpanMed[3];
        }

         if (this.gameObject.name.Contains("Part 2") && nomeClipe.Contains("grave_x1"))
        {
            SomDaNota.clip = SPI.SonsHandpanGrave[0];
        }
        if (this.gameObject.name.Contains("Part 2") && nomeClipe.Contains("grave_x2"))
        {
            SomDaNota.clip = SPI.SonsHandpanGrave[1];
        }
        if (this.gameObject.name.Contains("Part 2") && nomeClipe.Contains("grave_x3"))
        {
            SomDaNota.clip = SPI.SonsHandpanGrave[2];
        }
        if (this.gameObject.name.Contains("Part 2") && nomeClipe.Contains("grave_x4"))
        {
            SomDaNota.clip = SPI.SonsHandpanGrave[3];
        }
    }
    void FlautaPart1Check()
    {
         if (this.gameObject.name.Contains("Part 1") && nomeClipe.Contains("agudo_x1"))
        {
            SomDaNota.clip = SPI.SonsFlautaAgudo[0];
        }
        if (this.gameObject.name.Contains("Part 1") && nomeClipe.Contains("agudo_x2"))
        {
            SomDaNota.clip = SPI.SonsFlautaAgudo[1];
        }
        if (this.gameObject.name.Contains("Part 1") && nomeClipe.Contains("agudo_x3"))
        {
            SomDaNota.clip = SPI.SonsFlautaAgudo[2];
        }
        if (this.gameObject.name.Contains("Part 1") && nomeClipe.Contains("agudo_x4"))
        {
            SomDaNota.clip = SPI.SonsFlautaAgudo[3];
        }

         if (this.gameObject.name.Contains("Part 1") && nomeClipe.Contains("medio_x1"))
        {
            SomDaNota.clip = SPI.SonsFlautaMed[0];
        }
         if (this.gameObject.name.Contains("Part 1") && nomeClipe.Contains("medio_x2"))
        {
            SomDaNota.clip = SPI.SonsFlautaMed[1];
        }
         if (this.gameObject.name.Contains("Part 1") && nomeClipe.Contains("medio_x3"))
        {
            SomDaNota.clip = SPI.SonsFlautaMed[2];
        }
         if (this.gameObject.name.Contains("Part 1") && nomeClipe.Contains("medio_x4"))
        {
            SomDaNota.clip = SPI.SonsFlautaMed[3];
        }

         if (this.gameObject.name.Contains("Part 1") && nomeClipe.Contains("grave_x1"))
        {
            SomDaNota.clip = SPI.SonsFlautaGrave[0];
        }
        if (this.gameObject.name.Contains("Part 1") && nomeClipe.Contains("grave_x2"))
        {
            SomDaNota.clip = SPI.SonsFlautaGrave[1];
        }
        if (this.gameObject.name.Contains("Part 1") && nomeClipe.Contains("grave_x3"))
        {
            SomDaNota.clip = SPI.SonsFlautaGrave[2];
        }
        if (this.gameObject.name.Contains("Part 1") && nomeClipe.Contains("grave_x4"))
        {
            SomDaNota.clip = SPI.SonsFlautaGrave[3];
        }

    }
    void FlautaPart2Check()
    {
         if (this.gameObject.name.Contains("Part 2") && nomeClipe.Contains("agudo_x1"))
        {
            SomDaNota.clip = SPI.SonsFlautaAgudo[0];
        }
        if (this.gameObject.name.Contains("Part 2") && nomeClipe.Contains("agudo_x2"))
        {
            SomDaNota.clip = SPI.SonsFlautaAgudo[1];
        }
        if (this.gameObject.name.Contains("Part 2") && nomeClipe.Contains("agudo_x3"))
        {
            SomDaNota.clip = SPI.SonsFlautaAgudo[2];
        }
        if (this.gameObject.name.Contains("Part 2") && nomeClipe.Contains("agudo_x4"))
        {
            SomDaNota.clip = SPI.SonsFlautaAgudo[3];
        }

         if (this.gameObject.name.Contains("Part 2") && nomeClipe.Contains("medio_x1"))
        {
            SomDaNota.clip = SPI.SonsFlautaMed[0];
        }
         if (this.gameObject.name.Contains("Part 2") && nomeClipe.Contains("medio_x2"))
        {
            SomDaNota.clip = SPI.SonsFlautaMed[1];
        }
         if (this.gameObject.name.Contains("Part 2") && nomeClipe.Contains("medio_x3"))
        {
            SomDaNota.clip = SPI.SonsFlautaMed[2];
        }
         if (this.gameObject.name.Contains("Part 2") && nomeClipe.Contains("medio_x4"))
        {
            SomDaNota.clip = SPI.SonsFlautaMed[3];
        }

         if (this.gameObject.name.Contains("Part 2") && nomeClipe.Contains("grave_x1"))
        {
            SomDaNota.clip = SPI.SonsFlautaGrave[0];
        }
        if (this.gameObject.name.Contains("Part 2") && nomeClipe.Contains("grave_x2"))
        {
            SomDaNota.clip = SPI.SonsFlautaGrave[1];
        }
        if (this.gameObject.name.Contains("Part 2") && nomeClipe.Contains("grave_x3"))
        {
            SomDaNota.clip = SPI.SonsFlautaGrave[2];
        }
        if (this.gameObject.name.Contains("Part 2") && nomeClipe.Contains("grave_x4"))
        {
            SomDaNota.clip = SPI.SonsFlautaGrave[3];
        }

    }

}

