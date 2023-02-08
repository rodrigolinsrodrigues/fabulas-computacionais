using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUp : MonoBehaviour
{
    //Script responsavel pela mecanica de pegar objetos
     private Comandos comandos; // acessa o sistema de inputs
     // Careegar Objetos
    public bool holding = false, rotacionar; // checa se o player esta segurando o obj (holding) e caso o obj precise alterar a rotação para se adequar(rotacionar)
    public GameObject carryObj; // nome dado a qualquer obj que for pego
    public Transform hand; // Posição do obj usado como mão do player
    
    public CapsuleCollider capsule;
    void Start()
    {
        //Acessos necessarios para inputs
        comandos = new Comandos();
        comandos.Actions.Enable(); 
    }
    void OnTriggerStay(Collider other) // enquanto estiver na area de colisão
    {
        //Se a tag do obj for a correta e o player pressionar o botão de pegar
       if(other.GetComponent<Collider>().tag == "Pickable" && holding == false && comandos.Actions.PickUp.ReadValue<float>() == 1)
       {
        
                string name = other.GetComponent<Collider>().name;
                carryObj = GameObject.Find(name);
                carryObj.transform.SetParent(hand);
                carryObj.transform.position = hand.position;
                this.capsule.enabled = true;
                if(carryObj.GetComponent<AjustarObj>().rotacionar == true) {carryObj.transform.rotation = hand.rotation;}

                carryObj.GetComponent<Rigidbody>().mass = 10;
                carryObj.GetComponent<Rigidbody>().isKinematic = true;
                carryObj.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePosition;
                carryObj.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation;
                carryObj.GetComponent<Rigidbody>().useGravity = false;
                holding = true;
       }

        //Se a tag do obj for a correta e o player soltar o botão de pegar
       if (other.GetComponent<Collider>().tag == "Pickable" && holding == true && comandos.Actions.PickUp.ReadValue<float>() == 0)
            {
                this.capsule.enabled = false;
                carryObj.transform.SetParent(null);
                carryObj.GetComponent<Rigidbody>().isKinematic = false;
                carryObj.GetComponent<Rigidbody>().useGravity = true;
                carryObj.GetComponent<Rigidbody>().constraints = ~RigidbodyConstraints.FreezePosition;
                //if(carryObj.transform.position.y < -2.4){carryObj.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePositionY;}
                holding = false;
            }
    }

}
