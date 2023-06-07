using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class showCanvas : MonoBehaviour
{
    public GameObject Canvas, mural, Redoma;
    public GameObject[] pilares, Cams;
    bool bloq, concluido;

   
   void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Light"))
        {
            Canvas.SetActive(true);
            mural.GetComponent<BoxCollider>().enabled = true;
            Redoma.GetComponent<Animator>().SetBool("abrir", true);
           // StartCoroutine(Delay1());
        }

       
    }

   /* IEnumerator Delay1()
    {
       // yield return new WaitForSeconds(5f);
      //  Cams[0].SetActive(false);
      //  Cams[1].SetActive(true);
      //  yield return new WaitForSeconds(4f);
       
      //  Cams[1].SetActive(false);
      //  Cams[2].SetActive(true);
       // yield return new WaitForSeconds(1f);
       // bloq = true;
      //  Player.GetComponent<PlayerController>().enabled = true;
      //  yield return new WaitForSeconds(4f);
       // Canvas.SetActive(false);
    } */

}
