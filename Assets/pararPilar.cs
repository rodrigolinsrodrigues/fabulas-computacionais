using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class pararPilar : MonoBehaviour
{
    public GameObject Pilar;
    // Start is called before the first frame update
    
    void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Launch"))
        {
            string name = other.gameObject.name;
            Pilar = GameObject.Find(name);
            Pilar.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezePositionZ;
        }
    }
}
