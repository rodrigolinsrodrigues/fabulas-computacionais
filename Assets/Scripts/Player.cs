using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    //Começo - Controles
   Comandos comandos;


    // Começo - Mecanica
    public GameObject planet;
    public GameObject guia;
    public float speed = 4f;
    public float  jumpHeight = 1.2f; 
    float gravity = 10;
    float distanceToGround;
    public bool onGround = false;
    public Rigidbody rb;
    Vector3 groundNormal;
    // Final - Mecanica


    private void Awake()
    {
        comandos.Gameplay.Enable();
        Cursor.lockState = CursorLockMode.Locked;
    }
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
    }

    // Update is called once per frame
    void Update()
    {
        // Movimento - (OLD)
        float x = Input.GetAxis("Horizontal") * speed * Time.deltaTime;
        float z = Input.GetAxis("Vertical") * speed * Time.deltaTime;
        transform.Translate(x,0,z);
        



        // GroundControl
        RaycastHit hit = new RaycastHit();
        if(Physics.Raycast(transform.position, -transform.up, out hit, 10))
        {
            distanceToGround = hit.distance;
            groundNormal = hit.normal;

            if(distanceToGround <= 0.6f)
            {
                onGround = true;
            }
            else
            {
                onGround = false;
            }

            //rotação
            Quaternion toRotation = Quaternion.FromToRotation(transform.up, groundNormal) * transform.rotation;
            transform.rotation = toRotation;
        }
    }

    void FixedUpdate()
    {
        // Gravidade
        Vector3 gravDirection = (transform.position - planet.transform.position).normalized;
            if(onGround == false)
            {
                rb.AddForce(gravDirection * -gravity);
            }
        //    // if(comandos.Gameplay.Jump.performed += Jump)
        //     {
        //          Jump();
        //     }
           
    }

    // public void Jump(InputAction.Call)
    // {
    //     //Pulo
    //     if(onGround == true)
    //     {
    //         rb.AddForce(transform.up * 20000 * jumpHeight * Time.deltaTime);
    //     }
    // }
}
