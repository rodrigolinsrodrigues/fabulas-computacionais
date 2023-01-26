using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUp : MonoBehaviour
{
     private Comandos comandos;
     // Careegar Objetos
    public bool holding = false, rotacionar;
    public GameObject carryObj;
    public float pickupDistance;
    public Transform hand;
    public PlayerController player;
   

    // Start is called before the first frame update
    void Start()
    {
         comandos = new Comandos();
        comandos.Actions.Enable(); 
    }
   void Update() {
      
    }
    void OnTriggerStay(Collider other) 
    {
       if(other.GetComponent<Collider>().tag == "Pickable" && holding == false && comandos.Actions.PickUp.ReadValue<float>() == 1)
       {
        
                string name = other.GetComponent<Collider>().name;
                carryObj = GameObject.Find(name);
                carryObj.transform.SetParent(hand);
                carryObj.transform.position = hand.position;

                if(carryObj.GetComponent<AjustarObj>().rotacionar == true) { carryObj.transform.rotation = hand.rotation;}

                carryObj.GetComponent<Rigidbody>().mass = 10;
                carryObj.GetComponent<Rigidbody>().isKinematic = true;
                carryObj.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePosition;
                carryObj.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation;
                carryObj.GetComponent<Rigidbody>().useGravity = false;
                holding = true;
       }

       if (other.GetComponent<Collider>().tag == "Pickable" && holding == true && comandos.Actions.PickUp.ReadValue<float>() == 0)
            {
                carryObj.transform.SetParent(null);
                 carryObj.GetComponent<Rigidbody>().isKinematic = false;
                carryObj.GetComponent<Rigidbody>().useGravity = true;
                carryObj.GetComponent<Rigidbody>().constraints = ~RigidbodyConstraints.FreezePosition;
                holding = false;
            }
    }

}
