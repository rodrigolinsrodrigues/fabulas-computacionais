using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // controles (new input system)
    private Comandos comandos;
    // movimentação
    private CharacterController controller;
    private Vector2 inputs;
    private Transform cam; // direcao da camera
    public float curSpeed, walkSpeed = 4f, runSpeed = 8f, pushSpeed = 2f; // velocidades
    private float turnSmoothVelocity, TURNSMOOTHTIME = 0.135f; // velocidade de rotacao
    public float jumpHeigh = .1f;
   

    // Referencias a outros codigos
    // Empurrar Objetos
    public Push push;

    // gravidade
    [SerializeField] public bool isGrounded;
    [SerializeField] private LayerMask Ground; // layer dos objs q sao chão
    private Vector3 _velocity;
    private float gravidade = -20f;

    void Awake()
    {
        // configura variaveis
        controller = GetComponent<CharacterController>();
        cam = GameObject.FindWithTag("MainCamera").GetComponent<Transform>();
        // habilita controles
        comandos = new Comandos();
        comandos.Enable(); 
        Cursor.visible = false; 
    }

    void Update()
    {
        Movement();
        Gravity();
        Run();
        Jump();
    }

    void Movement() // movimentação baseada no input e direção da camera
    {
        inputs = comandos.Movement.Move.ReadValue<Vector2>();
        
        if (inputs.magnitude >= 0.1f) // se ta movendo pra qualquer direcao == apertou botão
        { 
            if(push.pushing == false)
            {
            float targetAngle = Mathf.Atan2(inputs.x, inputs.y) * Mathf.Rad2Deg + cam.eulerAngles.y;  // direcao que player vai rotacionar + onde a camera tiver olhando 
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, TURNSMOOTHTIME); // calcula o angulo e tempo pra virar smooth
            transform.rotation = Quaternion.Euler(-90f, angle, 0f); // rotaciona player
            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward; // vira pra direcao

            controller.Move(moveDir.normalized * curSpeed * Time.deltaTime); // actually move player
            }
           
           else
           {
                float targetAngle = Mathf.Atan2(inputs.x, inputs.y) * Mathf.Rad2Deg + transform.eulerAngles.y;
                Vector3 MoveDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
                controller.Move(MoveDirection.normalized * curSpeed * Time.deltaTime);
           }
        }
    }

    void Gravity() // adiciona gravidade
    {
        // checa se ta no chao
        isGrounded = Physics.CheckSphere(GameObject.FindWithTag("GCheck").transform.position, 0.2f, Ground, QueryTriggerInteraction.Ignore);
        _velocity.y += gravidade * Time.deltaTime; // direcao da gravidade
        controller.Move(_velocity * Time.deltaTime); // aplica gravidade

        if (isGrounded && _velocity.y < 0) 
            _velocity.y = -2f;
    }

    void Jump()
    {
        if (isGrounded && push.pushing == false && comandos.Movement.Jump.triggered) // se ta no chao
            _velocity.y = Mathf.Sqrt(jumpHeigh * -2f * gravidade);

    }

    void Run() // for surv
    {
        if (isGrounded && comandos.Movement.Run.ReadValue<float>() == 1)
            curSpeed = runSpeed;
        else
            curSpeed = walkSpeed;

        if(push.pushing == true)
            curSpeed = pushSpeed;
    }
}

