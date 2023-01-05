using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class rotatePlayer : MonoBehaviour
{   
    public Transform Player;
    Comandos comandos;
    public float turnSpeed = 10f;
     public float speed = 4f;
    public Transform cam;

    Vector3 moveDir;
   // Start is called before the first frame update
    void Start()
    {
        comandos = new Comandos();
        comandos.Movement.Enable();
    }

    // Update is called once per frame
    void Update()
    {

        Vector2 move = comandos.Movement.Move.ReadValue<Vector2>();
        Vector3 moveDirection = new Vector3(move.x, 0, move.y).normalized;
        Player.transform.Translate(moveDirection * speed * Time.deltaTime);
       
        float sideMove = 0;
        float forwardMOve = 0;

        sideMove += move.x;
        forwardMOve += move.y;

        Vector3 sideMoveScreen = Quaternion.Euler(0f, cam.eulerAngles.y, 0f) * Vector3.right;
        Vector3 forwardMOveScreen = Quaternion.Euler(0f, cam.eulerAngles.y,0f) * Vector3.forward;

        moveDir = sideMoveScreen * sideMove + forwardMOveScreen * forwardMOve;

        Quaternion characterLookDir = Quaternion.LookRotation(moveDir);

        if(moveDir.magnitude > 0f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, characterLookDir,turnSpeed * Time.deltaTime);
        }
    }
        
       // Codigo de rapha

    //    private void FixedUpdate()
    // {
    //     frenteCamera = cameraHolderPos.transform.forward;
    //     direitaCamera = cameraHolderPos.transform.right;

    //     frenteCamera.y = 0f;
    //     direitaCamera.y = 0f;
    //     frenteCamera.Normalize();
    //     direitaCamera.Normalize();

    //     #if UNITY_STANDALONE_WIN || UNITY_EDITOR || UNITY_WEBGL
    //     h = Input.GetAxisRaw("Horizontal");
    //     v = Input.GetAxisRaw("Vertical");
    //     #endif

    //     #if UNITY_ANDROID
    //     h = joystick.Horizontal;
    //     v = joystick.Vertical;
    //     #endif
    //     Debug.Log(h);
    //     if (podeAndar)
    //     {
    //         if (h != 0 || v != 0)
    //         {
    //             Rotacionar();
    //         }
    //         else
    //         {
    //             Vector3 direcao = h * direitaCamera +
    //             v * frenteCamera;
    //             if (direcao != Vector3.zero)
    //             {
    //                 transform.LookAt(transform.position + direcao);
    //             }
    //         }

    //         Movimentacao(h, v);
    //     }
    // }

    // void Movimentacao(float h, float v)
    // {
    //     movimento = (h * direitaCamera + v * frenteCamera);
    //     if (movimento != Vector3.zero)
    //     {
    //         anim.SetInteger("estado", 1);
    //     }
    //     else
    //     {
    //         anim.SetInteger("estado", 0);
    //     }
    //     movimento = movimento.normalized * velocidadeAtual * Time.deltaTime;

    //     rigid.MovePosition(transform.position + movimento);
    // }
    // void Rotacionar()
    // {
    //     #if UNITY_ANDROID
    //     float h2 = joystick.Horizontal;
    //     float v2 = joystick.Vertical;
    //     #endif

    //     #if UNITY_STANDALONE_WIN || UNITY_EDITOR || UNITY_WEBGL
    //     float h2  = Input.GetAxisRaw("Horizontal");
    //     float v2  = Input.GetAxisRaw("Vertical");
    //     #endif

    //     Vector3 direcao = h2 * direitaCamera +
    //         v2 * frenteCamera;
    //     if (direcao != Vector3.zero)
    //     {
    //         transform.LookAt(transform.position + direcao);
    //     }
    // }
    // }

        

   
}
