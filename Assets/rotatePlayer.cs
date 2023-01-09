using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class rotatePlayer : MonoBehaviour
{   
    public Transform Player;
    public Transform cam;
    public Player p;
    public float targetAngle;

    void Start()
    {
        
    }
    void Update()
    {
        if (p.moveDirection.magnitude > 0)
        {
            targetAngle = Mathf.Atan2(p.moveDirection.x, p.moveDirection.z) * Mathf.Rad2Deg + cam.eulerAngles.y;  // direcao que player vai rotacionar + onde a camera tiver olhando 
            Player.rotation = Quaternion.Euler(Player.eulerAngles.x, targetAngle, Player.eulerAngles.z); 
           
        }
    }
        

   
}
