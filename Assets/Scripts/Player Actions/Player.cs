using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    //Começo - Controles
    // Comandos comandos;


    public float speed = 4f;
    public float  jumpforce = 10f; 

    //Teste de colisão com o chão
    public Vector3 Boxsize;
    public float MaxDistance;
    public LayerMask LayerMask;

    public Rigidbody rb;
    private void Awake()
    {
       // comandos.Gameplay.Enable();
        Cursor.lockState = CursorLockMode.Locked;
    }
    void Start()
    {
        rb = GetComponent<Rigidbody>();
      
    }

    // Update is called once per frame
    void Update()
    {
        // Movimento - (OLD)
        float x = Input.GetAxis("Horizontal") * speed * Time.deltaTime;
        float z = Input.GetAxis("Vertical") * speed * Time.deltaTime;
        transform.Translate(x,0,z);
        
       
    }
    
    void FixedUpdate() 
    {
        // Codigo responsavel pelo pulo
        if (Input.GetKeyDown(KeyCode.Space) )//&& GroundCheck())
            {
                rb.AddForce(Vector3.up * jumpforce,ForceMode.Impulse);
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
