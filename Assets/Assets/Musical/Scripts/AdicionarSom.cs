using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdicionarSom : MonoBehaviour
{
   bool pressionado = false;
    public HabilitarBotao HB;
    public GameObject fundo;
   public void PressionadoTrue()
   {
       pressionado = true;
       if(pressionado == true)
       {
        fundo.SetActive(true);
        HB.Habilitar();
       }
        
   }

    public void PressionadoFalse()
   {
        fundo.SetActive(false);
        HB.Desabilitar();
         pressionado = false;
        
        
   }
}
