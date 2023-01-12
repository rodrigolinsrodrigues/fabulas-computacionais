using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class camRotate : MonoBehaviour
{
    public Transform target;
    public float xSpeed;
    public Player p;
    public Transform cam;
    
    // Update is called once per frame
    void Update()
    {
            transform.Translate(p.moveDirection.x * 2f * Time.deltaTime, 0, p.moveDirection.z * 2f * Time.deltaTime) ;
            transform.RotateAround(target.transform.position, transform.up, Input.GetAxis("Mouse X") * xSpeed);
            transform.RotateAround(target.transform.position, transform.right, Input.GetAxis("Mouse Y") * xSpeed);
            
        if (p.moveDirection.magnitude > 0)
        {
            target.rotation = Quaternion.Euler(target.eulerAngles.x, cam.eulerAngles.y, target.eulerAngles.z); 
        }

    }

}
