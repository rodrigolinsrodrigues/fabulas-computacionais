using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChecarColisao : MonoBehaviour
{
    // Codigo atribuido aos objetos que podem ser carregados, verica quando o player pode ou não soltalos depenendo de onde estão colidindo
    public GameObject Player;
    public bool comecar = false;
    
    void Start()
    {
        Player = GameObject.FindWithTag("Player");
    }
    void OnCollisionStay(Collision other) 
    {
        if(comecar == true)
        {
            if(other.collider.tag == "Colisao" || other.collider.tag == "Pickable")
            {
                Player.GetComponent<PickUp>().podeSoltar = false;
            }
            else{ Player.GetComponent<PickUp>().podeSoltar = true;}
        }
    }

    void OnCollisionExit(Collision other)
    {
        Player.GetComponent<PickUp>().podeSoltar = true;
        
    }

  
}


