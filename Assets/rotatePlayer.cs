using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class rotatePlayer : MonoBehaviour
{   
    public Transform Player;
    private Comandos comandos;
    public float turnSmoothVelocity;
    public float TURNSMOOTHTIME = 0.135f;
    public float speed = 4f;
    public Transform cam;
    public Player p;
    public GravityCRTL gc;
    public float targetAngle;
    Quaternion playerRot;

    Vector3 moveDir;

    void Start()
    {
        comandos = new Comandos();
        comandos.Movement.Enable();
    }
    public Quaternion rotation;
    void Update()
    {
        if (p.moveDirection.magnitude > 0)
        {
            targetAngle = Mathf.Atan2(p.moveDirection.x, p.moveDirection.z) * Mathf.Rad2Deg + cam.eulerAngles.y;  // direcao que player vai rotacionar + onde a camera tiver olhando 
            Player.rotation = Quaternion.Euler(Player.eulerAngles.x, targetAngle, Player.eulerAngles.z); 
           
        }
    }
        

   
}
