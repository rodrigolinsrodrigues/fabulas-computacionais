using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class switchCam : MonoBehaviour
{
    public GameObject[] Cam;
    // Start is called before the first frame update
   
    void OnTriggerEnter(Collider other)
    {
        if(other.GetComponent<Collider>().tag == "Player")
        {
            Cam[0].SetActive(false);
            Cam[1].SetActive(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
         if(other.GetComponent<Collider>().tag == "Player")
        {
            Cam[0].SetActive(true);
            Cam[1].SetActive(false);
        }
    }
}
