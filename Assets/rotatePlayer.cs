using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class rotatePlayer : MonoBehaviour
{
    private Comandos comandos;
    public Transform Player;
    public float rotSpeed = 0.5f;
    public Quaternion currentRotation;
   
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
        Vector2 movRot = new Vector2(move.x, move.y).normalized;

        if(comandos.Movement.Move.ReadValue<Vector2>() != Vector2.zero)
        {
        
            transform.Rotate(0,movRot.x * rotSpeed * Time.deltaTime,0);
            currentRotation = transform.rotation;
        }
        else
        {
           transform.rotation = currentRotation;
        }
       Debug.Log(move);
      
        
        
        
    }
        

   
}
