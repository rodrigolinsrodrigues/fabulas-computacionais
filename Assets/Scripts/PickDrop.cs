using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickDrop : MonoBehaviour
{
    public Rigidbody rb; 
    public bool inHand = false; // Checa se o obj foi pego 

    void OnTriggerStay(Collider other) 
    {
        rb = other.gameObject.GetComponent<Rigidbody>();
        // Teste rapido de pegar e soltar
        // Falta atualizar para o esquema novo de inputs
        if(Input.GetKey(KeyCode.E) && other.gameObject.tag == "PickDropObj")
        {
            if(inHand == false)
            {
            other.transform.parent = this.transform.parent;
            rb.freezeRotation = true;
            rb.isKinematic = true;
            inHand = true;
            }
        }
        else if(Input.GetKey(KeyCode.Q) && other.gameObject.tag == "PickDropObj")
        {
            if(inHand == true)
            {
            other.transform.parent = null;
            rb.freezeRotation = false;
            rb.isKinematic = false;
            inHand = false;
            }
        }
        
    }
}
   

