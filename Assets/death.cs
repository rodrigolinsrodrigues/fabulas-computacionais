using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class death : MonoBehaviour
{
   public Sistema Sis;

   
   void OnTriggerEnter(Collider other)
   {
        if(other.CompareTag("Player"))
        {
            Sis.reload();
        }
   }
}
