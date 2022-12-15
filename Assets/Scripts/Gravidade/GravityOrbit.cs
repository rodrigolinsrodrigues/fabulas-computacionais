using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityOrbit : MonoBehaviour
{
    // Codigo utilizado nos planetas
        // Controla a orbita dos planetas
    public float Gravity;
    public bool FixedDirection; // se a gravidade desse objeto apenas puxa o player para baixo
    
    // Start is called before the first frame update
    private void OnTriggerEnter(Collider other) 
    {
        if(other.GetComponent<GravityCRTL>())
        {
            //se o objeto que colidir tem o script de gravidade, esse planeta será a base da sua gravidade
            other.GetComponent<GravityCRTL>().Gravity = this.GetComponent<GravityOrbit>();


        }
    }
}
