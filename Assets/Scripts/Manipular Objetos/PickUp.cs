using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PickUp : MonoBehaviour
{
    //Script responsavel pela mecanica de pegar objetos, atribuido ao Player
    private Comandos comandos; // acessa o sistema de inputs
                               // Careegar Objetos
    public bool holding = false, podeSoltar; // checa se o player esta segurando o obj (holding) e verifica se o obj pode ser solto (podeSoltar)
    public GameObject carryObj, targetObj; // nome dado a qualquer obj que for pego
    public Transform hand; // Posição do obj usado como mão do player
    Animator animator;
    public PlayerController player;
    float timeToInteract = 0;
    public LeituraEspacos leituraEspacos;
    void Start()
    {
        //Acessos necessarios para inputs
        comandos = new Comandos();
        comandos.Actions.Enable();
        animator = GetComponent<Animator>();
        player = FindObjectOfType<PlayerController>();
        leituraEspacos = FindObjectOfType<LeituraEspacos>();
    }
    void Update()
    {
        if (Input.GetButtonDown("X") && targetObj != null && timeToInteract <= 0)
        {
            CatchDrop(targetObj);
            timeToInteract = 0.5f;
        }
        if (timeToInteract > 0)
        {
            timeToInteract -= Time.deltaTime;
        }
    }

    void OnTriggerStay(Collider other) // enquanto estiver na area de colisão
    {
        if (holding)
        {
            if (other.tag == "Colisao" || other.tag == "wall" || other.tag == "Pickable" && other.gameObject != targetObj)
            {
                podeSoltar = false;
            }
        }
        else
        {
            if (SceneManager.GetActiveScene().name == "Conjuntos Fase 2")
            {
                if (other.CompareTag("Pickable") && !leituraEspacos.NPC.activeSelf)
                {
                    if (!leituraEspacos.Concluido)
                    {
                        GameManager.Instance.ShowControlPopup(1);
                    }
                    targetObj = other.gameObject;
                }
            }
            else
            {
                if (other.CompareTag("Pickable"))
                {
                    if (!leituraEspacos.Concluido)
                    {
                        GameManager.Instance.ShowControlPopup(1);
                    }
                    targetObj = other.gameObject;
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!holding)
        {
            if (other.CompareTag("Pickable"))
            {
                GameManager.Instance.HideAllControlPopup();
                targetObj = null;
            }
        }
        else
        {
            if (other.tag == "Colisao" || other.tag == "wall" || other.tag == "Pickable" && other.gameObject != targetObj)
            {
                podeSoltar = true;
            }
        }
    }

    public void CatchDrop(GameObject other)
    {
        if (!holding)
        {
            GameManager.Instance.HideAllControlPopup();
            holding = true;
            podeSoltar = true;
            player.animator.SetBool("Pegar", true);
            player.animator.SetBool("Soltar", false);
            carryObj = other;
            carryObj.transform.SetParent(hand);
            carryObj.transform.position = hand.position;
            carryObj.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
        }
        else
        {
            if (podeSoltar)
            {
                player.animator.SetBool("Soltar", true);
                player.animator.SetBool("Pegar", false);
                carryObj.GetComponent<Rigidbody>().constraints = ~RigidbodyConstraints.FreezePositionY;
                carryObj.transform.position = hand.position + hand.forward * 2;
                carryObj.transform.SetParent(null);
                carryObj = null;
                podeSoltar = false;
                holding = false;
            }
        }
    }
}
