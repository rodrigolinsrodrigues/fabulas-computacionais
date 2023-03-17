using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComecarLeitura : MonoBehaviour
{
     private Comandos comandos; 
    public LerMesa LM;

   void Start()
    {
        comandos = new Comandos();
        comandos.Actions.Enable(); 
    }
   void OnTriggerStay(Collider other)
    {
        if(comandos.Actions.Interact.ReadValue<float>() == 1)
        LM.iniciar();
        LM.comecar = true;
        
    }
}
