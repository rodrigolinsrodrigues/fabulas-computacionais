using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class rotatePlayer : MonoBehaviour
{   
    public Rigidbody rb;
    public Transform cam;
    public Player p;
     private float turnSmoothVelocity, TURNSMOOTHTIME = 0.135f; // velocidade de rotacao
    void Update()
    {   
        if (p.moveDirection.magnitude > 0)
        {
            float targetAngle = Mathf.Atan2(p.moveDirection.x, p.moveDirection.z) * Mathf.Rad2Deg + cam.eulerAngles.y;  // direcao que player vai rotacionar + onde a camera tiver olhando 
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, TURNSMOOTHTIME); // calcula o angulo e tempo pra virar smooth
            transform.rotation = Quaternion.Euler(0f, angle, 0f); // rotaciona player
            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward; // vira pra direcao
        }
    }

           

        

   
}
