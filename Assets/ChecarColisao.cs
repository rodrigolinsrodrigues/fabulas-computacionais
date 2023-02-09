using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChecarColisao : MonoBehaviour
{
    public PickUp codPickUp;
    public bool comecar = false;
    void OnCollisionEnter(Collision other)
    {
        // if(comecar == true)
        // {
        // if(other.collider.name != null)
        // {
        //     codPickUp.podeSoltar = false;
        // }
        // else{codPickUp.podeSoltar = true;}
        // }
       
    }
}
