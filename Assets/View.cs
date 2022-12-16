using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class View : MonoBehaviour
{
    private Comandos comandos;

    public float speed = 4f;
    public float sensitive;
    float xRotation, yRotation;
    GameObject ombro;
    Transform ancor; // mesmo q o ombro
    
    void Awake()
    {
    comandos = new Comandos();
    comandos.Gameplay.Enable();
    }
    // Start is called before the first frame update
    void Start()
    {
        ombro = this.gameObject;
        ancor = this.transform;
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 r = comandos.Gameplay.Cam.ReadValue<Vector2>();//  Pega o Inut do analogico direito
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        ombro.transform.localRotation =  Quaternion.Euler(xRotation, 0f, 0f);
        ancor.Rotate(Vector3.up * r.x);
    }
}
