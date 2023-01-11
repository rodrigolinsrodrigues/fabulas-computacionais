using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityCRTL : MonoBehaviour
{
    // Codigo utilizado dentro do player !

    public GravityOrbit Gravity; // identifica qual gravidade esta atuando sob o personagem
    private Rigidbody rb;
    public float rotationSpeed = 20; // velocidade no qual o personagem troca de gravidade
    public Quaternion targetrotation;
    public  Vector3 gravityUp;
    public bool sobEfeito; // Indica se o objeto esta ou nao sob efeito da gravidade, pensado para objetos que estejam com a rotacao baseada na orbita do planeta, mas nao serao puxados pela gravidade. Ideal para plataformas suspensas no ar
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void FixedUpdate() 
    {
        if(Gravity) // caso haja contato com a orbita de um planeta
        {
             gravityUp = Vector3.zero;
            if(Gravity.FixedDirection)
            {
                gravityUp = Gravity.transform.up;
            }
            else
            {
                gravityUp = (transform.position - Gravity.transform.position).normalized;
            }

            Vector3 localUp = transform.up;

            targetrotation = Quaternion.FromToRotation(localUp,gravityUp) * transform.rotation;
            rb.GetComponent<Rigidbody>().rotation = targetrotation;

            transform.rotation = Quaternion.Lerp(transform.rotation, targetrotation, rotationSpeed * Time.deltaTime);
            // Forca da gravidade
            if(sobEfeito == true)
            {
                rb.AddForce((-gravityUp * Gravity.Gravity) * rb.mass);
            }
           


        }
    }
}
