using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class rotatePlayer : MonoBehaviour
{
    Comandos comandos;
    public float turnSpeed = 10f;

    // Start is called before the first frame update
    void Start()
    {
        comandos = new Comandos();
        comandos.Movement.Enable();
    }

    // Update is called once per frame
    void Update()
    {
        // Vector2 move = comandos.Movement.Move.ReadValue<Vector2>();
        // Vector3 moveDirection = new Vector3(move.x, 0, 0).normalized;

        // if(comandos.Movement.Move.ReadValue<Vector2>() != Vector2.zero)
        // {
        //     transform.Rotate(Vector3.up * moveDirection * turnSpeed * Time.deltaTime);
        // }
        // else
        // {
        //    transform.localRotation = Quaternion.Euler(0, 0, 0);
        // }

        if(Input.GetAxisRaw("Horizontal") != 0)
        {
            transform.Rotate(Vector3.up * Input.GetAxis("Horizontal") * turnSpeed * Time.deltaTime);
        }
        else
        {
           transform.localRotation = Quaternion.Euler(0, 0, 0);
        }
       
       
    }

        

   
}
