using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    //Comeco - Controles
    private Comandos comandos;

    //Movimento do player
    public float speed = 4f;
    public Transform Target;
    public float  jumpforce = 5f; 
    public bool Running;
    public bool canJump;
    public Rigidbody rb;

    //Teste de colisao com o chao
    public Vector3 Boxsize;
    public float MaxDistance;
    public LayerMask LayerMask;

    // Empurrar objetos
    public GameObject pushableObj; 
    public float pushDistance; // Controla a distancia em que o player precisa estar do objeto para interagir
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
        rb = GetComponent<Rigidbody>();
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
    
    }

    bool GroundCheck()
    {
        // Checa se o player esta pisando no chão, evitando pulos no ar
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
       Vector3 moveDirection = new Vector3(move.x, 0, move.y).normalized;
       transform.Translate(moveDirection * speed * Time.deltaTime);

       //transform.rotation = Target.rotation;
        

    }
        

    void Run()
    {
        //Codigo de corrida do player
        if(comandos.Movement.Run.ReadValue<float>() == 1 && GroundCheck() && pushing == false)
        {
            this.speed = 8f; 
            //Debug.Log(speed + "Correndo");
        }
        else 
        if(comandos.Movement.Run.ReadValue<float>() == 0)
        {
            this.speed = 4f;
            if(pushing == true)
            {
                this.speed = 2f;
                //Debug.Log(speed +"Carregando");
            }
        }

    }
    void Jump()
    {
         // Codigo responsavel pelo pulo
        if(comandos.Movement.Jump.triggered && GroundCheck() && canJump == true)
        {
            Debug.Log("Pulo");
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
            if (hit.collider.tag == "Pushable" && pushing == false && comandos.Actions.Push.ReadValue<float>() == 1 )
            {
                
                pushableObj.transform.SetParent(this.transform);
                this.speed = 2f;// exemplo de reducao de velocidade
                pushing = true;
                
                
            }
            else if(hit.collider.tag == "Pushable" && pushing == true && comandos.Actions.Push.ReadValue<float>() == 0)
            {
            
                pushableObj.transform.SetParent(null);
                this.speed = 4f;
                pushing = false;
               
            }
        }
    }
    
}
