using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickDrop : MonoBehaviour
{
    public GameObject intObj; // Objeto interagivel
    public Rigidbody rb; 
    public bool inHand = false; // Checa se o obj foi pego 

  
    void Start()
    {
    intObj = GameObject.FindWithTag("PickDropObj");
    rb = intObj.GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void OnTriggerStay(Collider other) 
    {
        // Teste rapido de pegar e soltar
        // Falta atualizar para o esquema novo de inputs
        if(Input.GetKey(KeyCode.E) && other.gameObject.tag == "PickDropObj")
        {
            if(inHand == false)
            {
            intObj.transform.parent = this.transform.parent;
            rb.freezeRotation = true;
            rb.isKinematic = true;
            inHand = true;
            }
        }
        else if(Input.GetKey(KeyCode.Q) && other.gameObject.tag == "PickDropObj")
        {
            if(inHand == true)
            {
            intObj.transform.parent = null;
            rb.freezeRotation = false;
            rb.isKinematic = false;
            inHand = false;
            }
        }
        
    }
}
   

