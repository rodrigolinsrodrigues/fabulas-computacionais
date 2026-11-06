using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateObj : MonoBehaviour
{
    private Comandos comandos;
    public GameObject rotateObj, hand;
    public float checkDistance;
    public bool pressed = false, rotacionando = false, rot = false;
    Vector3 currentRotation = new Vector3();
    // Start is called before the first frame update
    void Start()
    {
        comandos = new Comandos();
        comandos.Actions.Enable();
    }

    // Update is called once per frame
    void Update()
    {
        if (rotacionando)
        {
            rotacionar(currentRotation);
        }


        RaycastHit hit;
        if (Physics.Raycast(hand.transform.position, hand.transform.forward, out hit, checkDistance))
        {
            if (hit.collider.tag == "Rotate" && pressed == false && rotacionando == false && Input.GetButtonDown("X"))
            {
                pressed = true;
                string name = hit.collider.name;
                rotateObj = GameObject.Find(name);
                currentRotation = rotateObj.transform.rotation.eulerAngles;
                TelemetryManager.Log("pillar_rotate", ("pillar", rotateObj.name), ("rotation_before", currentRotation));
                rotacionando = true;
                StartCoroutine("delay");
            }
            if (Input.GetButtonUp("X"))
            {
                pressed = false;
            }

        }
    }

    void rotacionar(Vector3 currentRotation)
    {
        float speed = 38f;
        if (rotateObj.gameObject.name.Contains("Pilar_Verde_Linear"))
        {
            Quaternion targetRotation = Quaternion.Euler(currentRotation.x, currentRotation.y + 90f, currentRotation.z);
            rotateObj.transform.rotation = Quaternion.Slerp(rotateObj.transform.rotation, targetRotation, Time.deltaTime * speed);
        }
        else
        {
            Quaternion targetRotation = Quaternion.Euler(currentRotation.x, currentRotation.y, currentRotation.z + 90f);
            rotateObj.transform.rotation = Quaternion.Slerp(rotateObj.transform.rotation, targetRotation, Time.deltaTime * speed);
        }

    }



    IEnumerator delay()
    {
        yield return new WaitForSeconds(1f);
        rotacionando = false;
    }
}
