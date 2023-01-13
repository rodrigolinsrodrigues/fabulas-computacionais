using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Push : MonoBehaviour
{
     private Comandos comandos;
     // Referencia ao Player
    public PlayerController player;
      // Empurrar Objetos
    public bool pushing;
    public GameObject pushableObj;
    public float pushDistance;
   
    // Start is called before the first frame update
    void Awake()
    {
        comandos = new Comandos();
        comandos.Actions.Enable(); 
    }

    // Update is called once per frame
    void Update()
    {
         //Codigo responsavel por empurrar objetos
            //Verifique se ha um objeto com o tag "Pushable" na frente do personagem
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, pushDistance))
        {
            if (hit.collider.tag == "Pushable" && pushing == false && comandos.Actions.Push.ReadValue<float>() == 1)
            {
                string name = hit.collider.name;
                pushableObj = GameObject.Find(name);
                pushableObj.GetComponent<Rigidbody>().constraints = ~RigidbodyConstraints.FreezePositionX | ~RigidbodyConstraints.FreezePositionY | ~RigidbodyConstraints.FreezePositionZ;
                pushableObj.transform.SetParent(this.transform);
                player.curSpeed = player.pushSpeed;
                pushing = true;
                
                
            }
            else if(hit.collider.tag == "Pushable" && pushing == true && comandos.Actions.Push.ReadValue<float>() == 0)
            {
                pushableObj.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
                pushableObj.transform.SetParent(null);
                player.curSpeed = player.walkSpeed;
                pushing = false;
                pushableObj = null;
               
            }
        }
    }
}
