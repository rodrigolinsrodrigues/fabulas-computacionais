using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DescLuz : MonoBehaviour
{
    public GameObject Luz;
    public  GameObject text;
    public string Des;
    public bool Cristal;
    // Update is called once per frame
    void Update()
    {
        if(Cristal == false)
        {
            if(Luz.active){text.GetComponent<TextMeshPro>().text = Des;}
        }
       
    }

   
    void OnTriggerEnter(Collider other)
    {
        if(Cristal == true)
        {
        if(other.GetComponent<Collider>().tag == "Light")
        {
            text.GetComponent<TextMeshPro>().text = Des;
        }
        }
       
        
    }
}
