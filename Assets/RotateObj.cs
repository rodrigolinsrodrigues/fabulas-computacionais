using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateObj : MonoBehaviour
{
    private Comandos comandos;
    public GameObject rotateObj, hand;
    public float checkDistance;
    bool pressed = false;
    // Start is called before the first frame update
    void Start()
    {
        comandos = new Comandos();
        comandos.Actions.Enable(); 
    }
    
    // Update is called once per frame
    void Update()
    {
        RaycastHit hit;
        if (Physics.Raycast(hand.transform.position, hand.transform.forward, out hit, checkDistance))
        {
        if (hit.collider.tag == "Rotate" && pressed == false && comandos.Actions.Interact.triggered)
            {
                pressed = true;
                string name = hit.collider.name;
                rotateObj = GameObject.Find(name);
                Vector3 currentRotation = rotateObj.transform.rotation.eulerAngles;
                
                if(rotateObj.transform.rotation.eulerAngles.y == 0f)
                {  
               // rotateObj.GetComponent<Animator>().SetBool("0", true);
               // StartCoroutine(delay("0"));
                //rotateObj.GetComponent<Animator>().SetBool("270", false);
                    Quaternion newRotation = Quaternion.Euler(currentRotation.x, 90f, currentRotation.z);
                    rotateObj.transform.rotation = newRotation;
                }
                else 
                if(rotateObj.transform.rotation.eulerAngles.y == 90f)
                {
                   // rotateObj.GetComponent<Animator>().SetBool("90", true);
                  //  StartCoroutine(delay("90"));
                    //rotateObj.GetComponent<Animator>().SetBool("90", false);
                    Quaternion newRotation = Quaternion.Euler(currentRotation.x, 180f , currentRotation.z);
                    rotateObj.transform.rotation = newRotation;
                }
                else 
                if(rotateObj.transform.rotation.eulerAngles.y == 180f)
                {
                   // rotateObj.GetComponent<Animator>().SetBool("180", true);
                   // StartCoroutine(delay("180"));
                   // rotateObj.GetComponent<Animator>().SetBool("180", false);
                    Quaternion newRotation = Quaternion.Euler(currentRotation.x, 270f , currentRotation.z);
                    rotateObj.transform.rotation = newRotation;
                }
                else 
                if(rotateObj.transform.rotation.eulerAngles.y == 270f)
                {
                   // rotateObj.GetComponent<Animator>().SetBool("270", true);
                   // StartCoroutine(delay("270"));
                   // rotateObj.GetComponent<Animator>().SetBool("270", false);
                    Quaternion newRotation = Quaternion.Euler(currentRotation.x, 0f, currentRotation.z);
                   rotateObj.transform.rotation = newRotation;
                } 
            }
        if(comandos.Actions.Interact.ReadValue<float>() == 0)
        {
            pressed = false;
        }

        }
    }

    IEnumerator delay(string x)
    {
        yield return new WaitForSeconds(2f);
        rotateObj.GetComponent<Animator>().SetBool(x , false);
    }
}
