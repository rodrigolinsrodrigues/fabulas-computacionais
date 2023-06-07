using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bloqLados : MonoBehaviour
{
    public GameObject bloq;
    public bool pegou;

    void Update()
    {
        if(pegou == true)
        {
            bloq.SetActive(true);
        }
        else
        {
            bloq.SetActive(false);
        }
    }
   void OnTriggerStay(Collider other)
   {
         if(other.gameObject.tag == "Player")
         {
            GameObject player = other.gameObject;
            if(player.GetComponent<Push>().pushing == true)
            {
                pegou = true;
               
            }
        }
   }

  
   private void OnTriggerExit(Collider other)
   {
        if(other.gameObject.tag == "Player")
        {
       pegou = false;
        }
       
   }
}
