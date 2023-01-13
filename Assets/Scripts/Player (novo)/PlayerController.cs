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

    // Empurrar Objetos
    public bool pushing;
    public GameObject pushableObj;
    public float pushDistance;
    public float pushForce = 5.0f;

    // Lançar objetos
     public GameObject launchObj;
    public float launchDistance;
    public float launchForce = 5.0f;

    // Careegar Objetos
    public bool holding;
    public GameObject carryObj;
    public float pickupDistance;
    public Transform hand;

    // gravidade
    [SerializeField] private bool isGrounded;
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
        Push();
        Launch();
        //PickUp();
    }

    void Movement() // movimentação baseada no input e direção da camera
    {
        inputs = comandos.Movement.Move.ReadValue<Vector2>();
        
        if (inputs.magnitude >= 0.1f) // se ta movendo pra qualquer direcao == apertou botão
        { 
            float targetAngle = Mathf.Atan2(inputs.x, inputs.y) * Mathf.Rad2Deg + cam.eulerAngles.y;  // direcao que player vai rotacionar + onde a camera tiver olhando 
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, TURNSMOOTHTIME); // calcula o angulo e tempo pra virar smooth
            transform.rotation = Quaternion.Euler(0f, angle, 0f); // rotaciona player
            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward; // vira pra direcao

            controller.Move(moveDir.normalized * curSpeed * Time.deltaTime); // actually move player
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
        if (isGrounded && comandos.Movement.Jump.triggered) // se ta no chao
            _velocity.y = Mathf.Sqrt(jumpHeigh * -2 * gravidade);
    }

    void Run() // for surv
    {
        if (isGrounded && comandos.Movement.Run.ReadValue<float>() == 1)
            curSpeed = runSpeed;
        else
            curSpeed = walkSpeed;

        if(pushing == true)
            curSpeed = pushSpeed;
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
                curSpeed = pushSpeed;
                pushing = true;
                
                
            }
            else if(hit.collider.tag == "Pushable" && pushing == true && comandos.Actions.Push.ReadValue<float>() == 0)
            {
                pushableObj.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
                pushableObj.transform.SetParent(null);
                curSpeed = walkSpeed;
                pushing = false;
                pushableObj = null;
               
            }
        }
    }

    void Launch()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, launchDistance))
        {
            if (hit.collider.tag == "Launch" && comandos.Actions.Push.ReadValue<float>() == 1)
            {
                // Adicione uma forca a direcao em que o personagem esta olhando
                string name = hit.collider.name;
                launchObj = GameObject.Find(name);
                launchObj.GetComponent<Rigidbody>().AddForce(transform.forward * launchForce, ForceMode.Impulse);
            }
        }

    }

     void OnTriggerStay(Collider other) 
    {
       if(other.GetComponent<Collider>().tag == "Pickable" && holding == false && comandos.Actions.PickUp.ReadValue<float>() == 1) 
       {
                string name = other.GetComponent<Collider>().name;
                carryObj = GameObject.Find(name);
                carryObj.transform.SetParent(hand);
                carryObj.GetComponent<Rigidbody>().useGravity = false;
                carryObj.transform.position = hand.position;
                holding = true;
       }
       
       else if (other.GetComponent<Collider>().tag == "Pickable" && holding == true && comandos.Actions.Drop.ReadValue<float>() == 1)
            {
                holding = false;
                carryObj.transform.SetParent(null);
                carryObj.GetComponent<Rigidbody>().useGravity = true;
            }
}
}

