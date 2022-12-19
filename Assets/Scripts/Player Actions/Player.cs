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
    bool canJump;
    public float rotSpeed;

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
       
        Vector3 moveDirection = new Vector3(move.x, 0, move.y).normalized;
        Vector3 lookDirection = moveDirection + gameObject.transform.position;
        // Vector3 look = new Vector3(transform.rotation.x, lookDirection.y, transform.rotation.z);
        gameObject.transform.LookAt(lookDirection);
        transform.Translate(moveDirection * speed * Time.deltaTime, Space.World);



        // float horDirection = Input.GetAxisRaw("Horizontal");
        // Vector3 rotationAngle = new Vector3(0, 1, 0); 
        // rb.AddRelativeTorque(rotationAngle * (150 * horDirection));

        // float heading = Mathf.Atan2(move.x, move.y) * Mathf.Rad2Deg;
        // rb.rotation = Quaternion.Euler(0, heading, 0);

        // Vector3 rotate = new Vector3(move.x, 0, move.y);
        // rotate.Normalize();

        // if(move != Vector2.zero)
        // {
        //     Quaternion toRotation = Quaternion.LookRotation(move, Vector2.up);
        //     transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, rotSpeed * Time.deltaTime);
        // }
    }
    
    void FixedUpdate() 
    {
        // Codigo responsavel pelo pulo
        if(comandos.Gameplay.Jump.ReadValue<float>() == 1 && GroundCheck() && canJump == true)
        {
            rb.AddForce(transform.up * jumpforce,ForceMode.Impulse);
            canJump = false;
        }
        
    }
    bool GroundCheck()
    {
        // Checa se o player esta pisando no chão, evitando pulos no ar
        if(Physics.BoxCast(transform.position,Boxsize, - transform.up,transform.rotation,MaxDistance,LayerMask))
        {
            canJump = true;
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
