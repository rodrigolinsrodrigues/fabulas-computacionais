using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateToCamera : MonoBehaviour
{
    public Camera cameraMain;
    private void Start()
    {
        cameraMain = Camera.main;
    }
    void Update()
    {
        transform.LookAt(transform.position + cameraMain.transform.rotation * Vector3.forward, cameraMain.transform.rotation * Vector3.up);
    }
}
