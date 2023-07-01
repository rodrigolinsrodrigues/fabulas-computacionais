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
    public Transform hand; // Posição do obj usado como mão do player
    Animator animator;
    void Start()
    {
        //Acessos necessarios para inputs
        comandos = new Comandos();
        comandos.Actions.Enable(); 
        animator = GetComponent<Animator>();
    }
    
    void OnTriggerStay(Collider other) // enquanto estiver na area de colisão
    {
        //Se a tag do obj for a correta e o player pressionar o botão de pegar
       if(other.GetComponent<Collider>().tag == "Pickable" && holding == false && comandos.Actions.PickUp.ReadValue<float>() == 1)
       {
            holding = true;
            animator.SetBool("Pegar", true);
            animator.SetBool("Soltar", false);
            string name = other.GetComponent<Collider>().name;
            carryObj = GameObject.Find(name);
            carryObj.transform.SetParent(hand);
            carryObj.transform.position = hand.position;
            carryObj.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
            if(carryObj.GetComponent<AjustarObj>().rotacionar == true) {carryObj.transform.rotation = hand.rotation;}
            carryObj.GetComponent<ChecarColisao>().comecar = true; 
            //carryObj.GetComponent<Rigidbody>().mass = 10;
            // carryObj.GetComponent<Rigidbody>().isKinematic = true;     
       }

        //Se a tag do obj for a correta e o player soltar o botão de pegar
       else if (holding == true && podeSoltar == true && comandos.Actions.PickUp.ReadValue<float>() == 0)
        {   
            animator.SetBool("Soltar", true);
            animator.SetBool("Pegar", false);
            carryObj.GetComponent<ChecarColisao>().comecar = false; 
            carryObj.GetComponent<Rigidbody>().constraints = ~RigidbodyConstraints.FreezePositionY;
            carryObj.transform.SetParent(null);
            carryObj = null;
            podeSoltar = false;
            StartCoroutine("Delay");
           // carryObj.GetComponent<Rigidbody>().isKinematic = false;
            //carryObj.GetComponent<Rigidbody>().useGravity = true;    
        }
    }

    IEnumerator Delay()
    {
        yield return new WaitForSeconds(1f);
        holding = false;


    }
}
