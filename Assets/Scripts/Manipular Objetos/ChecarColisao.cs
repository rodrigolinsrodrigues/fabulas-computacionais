using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChecarColisao : MonoBehaviour
{
    // Codigo atribuido aos objetos que podem ser carregados, verica quando o player pode ou não soltalos depenendo de onde estão colidindo
    public PickUp codPickUp;
    public bool comecar = false;
    
    void OnCollisionStay(Collision other) 
    {
        if(comecar == true)
        {
        if(other.collider.tag == "Colisao" || other.collider.tag == "Pickable")
        {
            codPickUp.podeSoltar = false;
        }
        else{codPickUp.podeSoltar = true;}
        }
    
    }

    void OnCollisionExit(Collision other)
    {
       codPickUp.podeSoltar = true;
    
    }
}


