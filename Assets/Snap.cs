using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Snap : MonoBehaviour
{
    public GameObject carryObj;
    // Start is called before the first frame update
    void OnTriggerStay(Collider other) 
    {
        if(other.GetComponent<Collider>().tag == "Pickable")
        {
            string name = other.GetComponent<Collider>().name;
            carryObj = GameObject.Find(name);
        }
        if(carryObj.transform.parent == null)
        {
            carryObj.transform.SetParent(this.transform);
            carryObj.transform.position = this.transform.position;
            carryObj.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePosition;
            carryObj.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation;
            carryObj.GetComponent<Rigidbody>().useGravity = false;
        }
        if(other.GetComponent<Collider>().tag != "Pickable")
        {
            carryObj = null;
        }
    }
    
}
