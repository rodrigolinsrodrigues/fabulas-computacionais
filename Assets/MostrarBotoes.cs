using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MostrarBotoes : MonoBehaviour
{
    public float Distance;
    public GameObject Guia;
    public GameObject Y, RB;
    // Update is called once per frame
    void Start()
    {
        Guia = GameObject.FindWithTag("Guia");
       
    }
    void Update()
    {
        RaycastHit hit;
        if(Physics.Raycast(Guia.transform.position, Guia.transform.forward, out hit, Distance))
        {
            if (hit.collider.tag == "Launch" || hit.collider.tag == "Rotate" ||  hit.collider.tag == "Emit" ||  hit.collider.tag == "Lens")
            {
               Y.SetActive(true);
              
            }
            else
            {
                Y.SetActive(false);
            }
            
            
            if(hit.collider.tag == "Break")
            {
                RB.SetActive(true);
               
            }
            else
            {
                RB.SetActive(false);
            }
        }
    }

}
