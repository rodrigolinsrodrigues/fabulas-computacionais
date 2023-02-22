using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Snap : MonoBehaviour
{
    public GameObject carryObj, Lugar1, Lugar2, essaMesa;
    public int Acoes = 14;
    public bool objNaArea;
     public PickUp pickup;
   
   void OnTriggerEnter(Collider other)
    {
        if(this.transform.childCount == 4)
        {
            Debug.Log("Bloqueada");
            pickup.podeSoltar = false;
        }
    }
    void OnTriggerStay(Collider other) 
    {
        if(other.GetComponent<Collider>().tag == "Pickable")
        {
            string name = other.GetComponent<Collider>().name;
            carryObj = GameObject.Find(name);
        }
        if(carryObj != null)
        {
            if(carryObj.transform.parent == null)
            {
            if(other.gameObject.layer != Acoes)
            {
            carryObj.transform.SetParent(this.transform);
            carryObj.transform.position = Lugar1.transform.position;
            carryObj.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePosition;
            carryObj.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation;
            carryObj.GetComponent<Rigidbody>().useGravity = false;
            carryObj.GetComponent<Rigidbody>().isKinematic = true;
            }

            else if(other.gameObject.layer == Acoes)
            {
            carryObj.transform.SetParent(this.transform);
            carryObj.transform.position = Lugar2.transform.position;
            carryObj.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePosition;
            carryObj.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation;
            carryObj.GetComponent<Rigidbody>().useGravity = false;
            carryObj.GetComponent<Rigidbody>().isKinematic = true;
            }
            }
           
        }
        
    }

    void OnTriggerExit(Collider other)
    {
         if(this.transform.childCount == 4)
        {
            Debug.Log("Desbloqueada");
            pickup.podeSoltar = true;
        }
        if(carryObj != null)
        {
            carryObj = null;
        }
    }
}
