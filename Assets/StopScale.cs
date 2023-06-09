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
        
        }


        if(Espelho == true)
        {
        if(other.GetComponent<Collider>().tag == "Mirror")
        {
            this.LP.scaleFactor = 0f;
        }

        if(other.GetComponent<Collider>().tag != "Mirror")
        {
            Debug.Log((this.gameObject.name) + (other.gameObject.name));
           // LP.scaleFactor = 0f;
            for(int i = 0; i < OtherLights.Length; i++)
            {
            OtherLights[i].gameObject.SetActive(false);
            } 
            this.gameObject.SetActive(false);
            
        }
       
        }

        if(Crystal == true)
        {
        if(other.GetComponent<Collider>().tag == "Cristal")
        {
            LP.scaleFactor = 0;
        }
        if(other.GetComponent<Collider>().tag != "Cristal")
        {
            Debug.Log((this.gameObject.name) + (other.gameObject.name));
           // LP.scaleFactor = 0f;
            for(int i = 0; i < OtherLights.Length; i++)
            {
            OtherLights[i].gameObject.SetActive(false);
            } 
            this.gameObject.SetActive(false);
            
        }
        }
       
    }

     private  IEnumerator  Delay()
    {
        yield return new WaitForSeconds(4f);
        this.gameObject.SetActive(false);
    }

    public void  DeleteAll()
    {
        for(int i = 0; i < OtherLights.Length; i++)
        {
            OtherLights[i].gameObject.SetActive(false);
        } 
        
    }
}
