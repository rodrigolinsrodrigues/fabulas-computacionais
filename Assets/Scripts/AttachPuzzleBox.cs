using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttachPuzzleBox : MonoBehaviour
{
    public GameObject Area, portao;
    public GameObject Rose;
    // Start is called before the first frame update
    void Update() 
    {
        if(Area.transform.childCount == 1)
        {
            portao.GetComponent<Animator>().SetBool("Completo", true);
        }

    }
    void OnTriggerEnter(Collider other) 
    {
        if(other.CompareTag("Pickable") && other.gameObject.layer == 17)
        {
            if(Rose.GetComponent<PickUp>().holding == false)
            {
            other.transform.SetParent(Area.transform);
            other.transform.position = Area.transform.position;
            other.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
            other.GetComponent<Rigidbody>().isKinematic = true;
            other.transform.Rotate(0,0,0);
            other.gameObject.layer = 0;
            }
            
        }
    }
}
