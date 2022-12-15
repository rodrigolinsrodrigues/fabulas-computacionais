using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityCRTL : MonoBehaviour
{
    // Codigo utilizado dentro do player !
    public GravityOrbit Gravity; // identifica qual gravidade está atuando sob o personagem
    private Rigidbody rb;

    public float RotationSpeed = 20; // velocidade no qual o personagem troca de gravidade
    
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void FixedUpdate() 
    {
        if(Gravity) // caso haja contato com a orbita de um planeta
        {
            Vector3 gravityUp = Vector3.zero;
            if(Gravity.FixedDirection)
            {
                gravityUp = Gravity.transform.up;
            }
            else
            {
                gravityUp = (transform.position - Gravity.transform.position).normalized;
            }
           

            Vector3 localUp = transform.up;

            Quaternion targetrotation = Quaternion.FromToRotation(localUp,gravityUp) * transform.rotation;

            transform.up = Vector3.Lerp(transform.up, gravityUp, RotationSpeed * Time.deltaTime);

            // Força da gravidade
            rb.AddForce((-gravityUp * Gravity.Gravity) * rb.mass);


        }
    }
}
