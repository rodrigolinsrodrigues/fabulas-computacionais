using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    //Começo - Controles
    private Comandos comandos;

    public float speed = 4f;
    public float  jumpforce = 5f; 

    //Teste de colisão com o chão
    public Vector3 Boxsize;
    public float MaxDistance;
    public LayerMask LayerMask;

    public Rigidbody rb;

    private void Awake()
    {
        comandos = new Comandos();
        comandos.Gameplay.Enable();
        // Cursor.lockState = CursorLockMode.Locked;
    }
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 move = comandos.Gameplay.Move.ReadValue<Vector2>();
        transform.Translate(move.x * speed * Time.deltaTime, 0 ,move.y * speed * Time.deltaTime);
    }
    
    void FixedUpdate() 
    {
        // Codigo responsavel pelo pulo
        if(comandos.Gameplay.Jump.ReadValue<float>() == 1 && GroundCheck())
        {
            rb.AddForce(transform.up * jumpforce,ForceMode.Impulse);
        }
        
    }
    bool GroundCheck()
    {
        // Checa se o player esta pisando no chão, evitando pulos no ar
        if(Physics.BoxCast(transform.position,Boxsize, - transform.up,transform.rotation,MaxDistance,LayerMask))
        {
            return true;
        }
        else
        {
            return false;
        }

    }
     void OnDrawGizmos() 
    {
        // cria uma caixa de colisão como guia
        Gizmos.color = Color.red;
        Gizmos.DrawCube(transform.position - transform.up * MaxDistance,Boxsize);
    }

}
