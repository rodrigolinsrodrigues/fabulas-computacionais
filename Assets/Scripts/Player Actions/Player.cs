using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    //Comeco - Controles
    private Comandos comandos;

    //Movimento do player
    Rigidbody rb;
    public GravityCRTL gc;
    public Transform cam;
    public float speed;
    public float jumpforce = 5f; 
    public bool Running;
    public bool canJump;
    public Vector3 moveDirection;
    Vector3 playerF;
    Vector3 playerR;

    //Teste de colisao com o chao
    public Vector3 Boxsize;
    public float MaxDistance;
    public LayerMask LayerMask;

    // Empurrar objetos
    public GameObject pushableObj; 
    public float pushDistance;
    public bool pushing;

    // Carregar Objetos

    
    private void Awake()
    {
        comandos = new Comandos();
        comandos.Movement.Enable();
        comandos.Actions.Enable();
        
        Cursor.visible = false; 
    }
    
    void Start()
    {
        rb = this.GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {   
        Walk();
        Run();
        Jump();
        Push();
    }
    
    void FixedUpdate() 
    {
        // Move o player pelo rigidbody
        rb.MovePosition(rb.position + (moveDirection.z * transform.forward * speed * Time.deltaTime) + (moveDirection.x * transform.right * speed * Time.deltaTime)); 
    }

    bool GroundCheck()
    {
        // Checa se o player esta pisando no chao, evitando pulos no ar
        if(Physics.BoxCast(transform.position,Boxsize, - transform.up,transform.rotation,MaxDistance,LayerMask))
        {
            if(pushing == false)
            canJump = true;
            return true;
        }
        else 
        {
            if(pushing == true)
            canJump = false;
            return false;
        }

    }
     void OnDrawGizmos() 
    {
        // cria uma caixa de colisão como guia
        Gizmos.color = Color.red;
        Gizmos.DrawCube(transform.position - transform.up * MaxDistance,Boxsize);
    }
   
    void Walk()
    {
        //Codigo responsavel por mover o jogador
       Vector2 move = comandos.Movement.Move.ReadValue<Vector2>();
       moveDirection = new Vector3(move.x, 0, move.y).normalized;
    }

    void Run()
    {
        //Codigo de corrida do player
        if(comandos.Movement.Run.ReadValue<float>() == 1 && GroundCheck() && pushing == false)
        {
            speed = 8f; 
        }
        else 
        if(comandos.Movement.Run.ReadValue<float>() == 0)
        {
            speed = 4f;
            if(pushing == true)
            {
                speed = 2f;
            }
        }

    }
    void Jump()
    {
         // Codigo responsavel pelo pulo
        if(comandos.Movement.Jump.triggered && GroundCheck() && canJump == true)
        {
            rb.AddForce(transform.up * jumpforce,ForceMode.Impulse);
            canJump = false;
        }
           
        
    }
    void Push()
    {
        //Codigo responsavel por empurrar objetos
            //Verifique se ha um objeto com o tag "Pushable" na frente do personagem
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, pushDistance))
        {
            if (hit.collider.tag == "Pushable" && pushing == false && comandos.Actions.Push.ReadValue<float>() == 1)
            {
                string name = hit.collider.name;
                pushableObj = GameObject.Find(name);
                pushableObj.GetComponent<Rigidbody>().constraints = ~RigidbodyConstraints.FreezePositionX | ~RigidbodyConstraints.FreezePositionY | ~RigidbodyConstraints.FreezePositionZ;
                pushableObj.transform.SetParent(this.transform);
                speed = 2f;
                pushing = true;
                
                
            }
            else if(hit.collider.tag == "Pushable" && pushing == true && comandos.Actions.Push.ReadValue<float>() == 0)
            {
                pushableObj.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
                pushableObj.transform.SetParent(null);
                speed = 4f;
                pushing = false;
                pushableObj = null;
               
            }
        }
    }
    
}
