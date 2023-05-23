using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class animObj : MonoBehaviour
{
    public float rotationSpeed = 10f; // Velocidade de rotação em graus por segundo

    void Update()
    {
        // Rotacionar o objeto em torno do eixo Z
        transform.Rotate(Vector3.forward * rotationSpeed * Time.deltaTime);
    }
}
