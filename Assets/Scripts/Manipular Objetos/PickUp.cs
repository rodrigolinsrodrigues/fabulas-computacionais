using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUp : MonoBehaviour
{
    //Script responsavel pela mecanica de pegar objetos, atribuido ao Player
     private Comandos comandos; // acessa o sistema de inputs
     // Careegar Objetos
    public bool holding = false, podeSoltar; // checa se o player esta segurando o obj (holding) e verifica se o obj pode ser solto (podeSoltar)
    public GameObject carryObj; // nome dado a qualquer obj que for pego
    public Transform head,hand; // Posição do obj usado como mão do player
    
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
        
                if(carryObj.GetComponent<AjustarObj>().rotacionar == true) {carryObj.transform.rotation = hand.rotation;}
                carryObj.GetComponent<ChecarColisao>().comecar = true; 
                carryObj.GetComponent<Rigidbody>().mass = 10;
                carryObj.GetComponent<Rigidbody>().isKinematic = true;
                carryObj.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePosition;
                carryObj.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation;
                carryObj.GetComponent<Rigidbody>().useGravity = false;
                holding = true;
       }

        //Se a tag do obj for a correta e o player soltar o botão de pegar
       if (other.GetComponent<Collider>().tag == "Pickable" && holding == true && podeSoltar == true && comandos.Actions.PickUp.ReadValue<float>() == 0)
            {   
                carryObj.GetComponent<ChecarColisao>().comecar = false; 
                carryObj.transform.SetParent(null);
                carryObj.GetComponent<Rigidbody>().isKinematic = false;
                carryObj.GetComponent<Rigidbody>().useGravity = true;
                carryObj.GetComponent<Rigidbody>().constraints = ~RigidbodyConstraints.FreezePosition;
                holding = false;
            }
    }

}
