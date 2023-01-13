using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUp : MonoBehaviour
{
      private Comandos comandos;
     // Careegar Objetos
    public bool holding;
    public GameObject carryObj;
    public float pickupDistance;
    public Transform hand;

    // Start is called before the first frame update
    void Start()
    {
         comandos = new Comandos();
        comandos.Actions.Enable(); 
    }

    void OnTriggerStay(Collider other) 
    {
       if(other.GetComponent<Collider>().tag == "Pickable" && holding == false && comandos.Actions.PickUp.ReadValue<float>() == 1) 
       {
                string name = other.GetComponent<Collider>().name;
                carryObj = GameObject.Find(name);
                carryObj.transform.SetParent(hand);
                carryObj.GetComponent<Rigidbody>().useGravity = false;
                carryObj.transform.position = hand.position;
                holding = true;
       }
       
       else if (other.GetComponent<Collider>().tag == "Pickable" && holding == true && comandos.Actions.Drop.ReadValue<float>() == 1)
            {
                holding = false;
                carryObj.transform.SetParent(null);
                carryObj.GetComponent<Rigidbody>().useGravity = true;
            }
    }

}
