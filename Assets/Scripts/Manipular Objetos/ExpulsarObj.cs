using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExpulsarObj : MonoBehaviour
{
    public GameObject obj;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void OnTriggerStay(Collider other) 
    {
         if(other.GetComponent<Collider>().tag == "Pickable" && other.transform.parent == null)
        {
            string name = other.GetComponent<Collider>().name;
            obj = GameObject.Find(name);
            obj.GetComponent<Rigidbody>().AddForce(-transform.forward * 2f, ForceMode.Impulse);
           
            
        }
    }
   
}
