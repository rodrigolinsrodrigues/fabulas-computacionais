using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Snap : MonoBehaviour
{
    public GameObject carryObj, obj1,obj2, Lugar1, Lugar2;
     public bool InsumoOk,AcaoOK;
    public int Acoes = 14;
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
            if(other.gameObject.layer != Acoes && InsumoOk == false)
            {
            obj1 = carryObj;
            obj1.transform.SetParent(this.transform);
            obj1.transform.position = Lugar1.transform.position;
            InsumoOk = true;
            obj1.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePosition;
            obj1.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation;
            obj1.GetComponent<Rigidbody>().useGravity = false;
            obj1.GetComponent<Rigidbody>().isKinematic = true;
            }

            else if(other.gameObject.layer == Acoes && AcaoOK == false)
            {
            obj2 = carryObj;
            obj2.transform.SetParent(this.transform);
            obj2.transform.position = Lugar2.transform.position;
            AcaoOK = true;
            obj2.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePosition;
            obj2.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation;
            obj2.GetComponent<Rigidbody>().useGravity = false;
            obj2.GetComponent<Rigidbody>().isKinematic = true;
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
        if(other.GetComponent<Collider>().tag == "Pickable")
        {
        if(InsumoOk == true && obj1.transform.parent != this.transform)
        {
            Debug.Log("Obj1");
          InsumoOk = false;
          obj1 = null;
        }
         if(AcaoOK == true && obj2.transform.parent != this.transform)
        {
            Debug.Log("Obj2");
          AcaoOK = false;
          obj2 = null;
        }
        }
    }
}
