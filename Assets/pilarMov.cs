using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class pilarMov : MonoBehaviour
{
    public GameObject Bloq;
  
   private void OnTriggerEnter(Collider other)
   {
        if(other.gameObject.name == "Pilar Azul - Linear")
        {
            Bloq.SetActive(true);
        }
   }
}
