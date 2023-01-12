using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Guia : MonoBehaviour
{
     //Comeco - Controles
    private Comandos comandos;

    public float speed = 4f;
    public float rotationSpeed = 5f;
    // Start is called before the first frame update
    void Start()
    {
        comandos = new Comandos();
        comandos.Movement.Enable();
    }

    // Update is called once per frame
    void Update()
    {
         //Codigo responsavel por mover o jogador
        Vector2 move = comandos.Movement.Move.ReadValue<Vector2>();
        Vector3 moveDirection = new Vector3(move.x, 0, move.y).normalized;
        transform.Translate(moveDirection * speed * Time.deltaTime);


        Debug.Log(moveDirection);
        if(moveDirection != Vector3.zero)
        {
        Quaternion ToRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, ToRotation,rotationSpeed * Time.deltaTime);
        }
       
    }
}
