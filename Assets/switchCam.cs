using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
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
            Cam[0].GetComponent<CinemachineFreeLook>().Priority = 1;
            Cam[1].GetComponent<CinemachineVirtualCamera>().Priority = 10;
        }
    }

    void OnTriggerExit(Collider other)
    {
         if(other.GetComponent<Collider>().tag == "Player")
        {
            Cam[0].SetActive(true);
            Cam[1].SetActive(false);
            Cam[0].GetComponent<CinemachineFreeLook>().Priority = 10;
            Cam[1].GetComponent<CinemachineVirtualCamera>().Priority = 1;
        }
    }
}
