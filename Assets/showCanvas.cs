using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class showCanvas : MonoBehaviour
{
    public GameObject Canvas;
   
   void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Light"))
        {
            Canvas.SetActive(true);
        }
    }
}
