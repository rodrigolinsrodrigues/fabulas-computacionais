using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class showCanvas : MonoBehaviour
{
    public GameObject Canvas, Player;
    public GameObject[] pilares, Cams;
    bool bloq, concluido;

   
   void Update()
   {
    
        // if(bloq == true)
        // {
        //     for(int i = 0; i < pilares.Length; i++)
        //     {
        //     pilares[i].GetComponent<canMove>().move = false;
        //     }
        // }
   }
   void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Light"))
        {
           
            StartCoroutine(Delay1());
            
           
        }

       
    }

    IEnumerator Delay1()
    {
        yield return new WaitForSeconds(5f);
        Cams[0].SetActive(false);
        Cams[1].SetActive(true);
        yield return new WaitForSeconds(4f);
        Canvas.SetActive(true);
        Cams[1].SetActive(false);
        Cams[2].SetActive(true);
        yield return new WaitForSeconds(1f);
       // bloq = true;
        Player.GetComponent<PlayerController>().enabled = true;
        yield return new WaitForSeconds(4f);
        Canvas.SetActive(false);
    }

}
