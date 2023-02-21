using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Snap : MonoBehaviour
{
    public GameObject carryObj, Lugar1, Lugar2;
    public int Acoes1;
    // Start is called before the first frame update
    void OnTriggerStay(Collider other) 
    {
        if(other.GetComponent<Collider>().tag == "Pickable"  && other.gameObject.layer != Acoes1 ) //|| other.gameObject.layer != Acoes2)
        {
            string name = other.GetComponent<Collider>().name;
            carryObj = GameObject.Find(name);
        }
        if(carryObj != null)
        {
            if(carryObj.transform.parent == null)
            {
            carryObj.transform.SetParent(this.transform);
            carryObj.transform.position = Lugar1.transform.position;
            carryObj.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePosition;
            carryObj.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation;
            carryObj.GetComponent<Rigidbody>().useGravity = false;
            carryObj.GetComponent<Rigidbody>().isKinematic = true;
            carryObj.GetComponent<ChecarColisao>().comecar = true;
            }
        }
         if(other.GetComponent<Collider>().tag == "Pickable" && other.gameObject.layer == Acoes1) //|| other.gameObject.layer == Acoes2)
        {
            string name = other.GetComponent<Collider>().name;
            carryObj = GameObject.Find(name);
        }
        if(carryObj != null)
        {
            if(carryObj.transform.parent == null)
            {
            carryObj.transform.SetParent(this.transform);
            carryObj.transform.position = Lugar2.transform.position;
            carryObj.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePosition;
            carryObj.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation;
            carryObj.GetComponent<Rigidbody>().useGravity = false;
            carryObj.GetComponent<Rigidbody>().isKinematic = true;
            carryObj.GetComponent<ChecarColisao>().comecar = true;
            }
        }


    }

    void OnTriggerExit(Collider other)
    {
        if(carryObj != null)
        {
            carryObj = null;  
        }
      
    }
    
}
