using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StopScale : MonoBehaviour
{
    public EmitLight emit;
    public LightPass LP;
    public bool Emissor, Espelho, Crystal;
    public GameObject[] OtherLights;
   // public string ligth = "Lights";
    // Start is called before the first frame update
   
  void Start()
  {
   
  }
    void OnTriggerEnter(Collider other)
    {
        if(Emissor == true)
        {
        if(other.GetComponent<Collider>().tag == "Mirror")
        {
            emit.scaleFactor = 0f;
        }

        if(other.GetComponent<Collider>().tag != "Mirror")
        {
            this.gameObject.SetActive(false);
        }

        if(other.GetComponent<Collider>().tag == null)
        {
            StartCoroutine(Delay());
        }
        }


        if(Espelho == true)
        {
        if(other.GetComponent<Collider>().tag == "Mirror")
        {
          
            LP.scaleFactor = 0f;
        }

        if(other.GetComponent<Collider>().tag != "Mirror" || other.GetComponent<Collider>().tag == null)
        {
            StartCoroutine(Delay2());
           /* for(int i = 0; i < OtherLights.Length; i++)
            {
                OtherLights[i].SetActive(false);
            }
            LP.scaleFactor = 0.1f;
            this.gameObject.SetActive(false); */
           
        }
        }

        if(Crystal == true)
        {
        if(other.GetComponent<Collider>().tag == "Cristal")
        {
            LP.scaleFactor = 0;
        }
        }
       
    }

     private  IEnumerator  Delay()
    {
        yield return new WaitForSeconds(4f);
        this.gameObject.SetActive(false);
    }

     private  IEnumerator  Delay2()
    {
        yield return new WaitForSeconds(4f);
        this.gameObject.SetActive(false);
      /*  for(int i = 0; i < OtherLights.Length; i++)
        {
            OtherLights[i].gameObject.SetActive(false);
        } */
        LP.scaleFactor = 0.1f;
       
    }
}
