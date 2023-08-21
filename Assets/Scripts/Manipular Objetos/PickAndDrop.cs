using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickAndDrop : MonoBehaviour
{
    public Transform handsPosition; // Referência ao objeto que define a posição das mãos do jogador
    private Comandos comandos;
    public GameObject heldObject, targetObject;
    public Snap snapTarget;
    public bool isHoldingObject, canCatch, canDrop, touchingSnap;
    public LerMesa lerMesa;
    public PlayerController playerLink;
    float timeToInteract = 0;

    private void Awake()
    {
        playerLink = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
        if (GameObject.FindObjectOfType<LerMesa>())
        {
            lerMesa = GameObject.FindObjectOfType<LerMesa>();
        }

        comandos = new Comandos();
        comandos.Enable();
        Cursor.visible = false;
    }

    private void Update()
    {
        if (Input.GetButtonDown("X") && timeToInteract <= 0)
        {
            CatchRelease();
            timeToInteract = 0.5f;
        }
        if (timeToInteract > 0)
        {
            timeToInteract -= Time.deltaTime;
        }
    }

    public void CatchRelease()
    {
        //release
        if (isHoldingObject)
        {
            if (canDrop)
            {
                Drop();
            }
        }
        else
        {
            //catch
            if (targetObject != null || touchingSnap)
            {
                Catch();
            }
        }
    }

    public void Drop()
    {
        if (touchingSnap)
        {
            if (snapTarget.obj == null)
            {
                if (snapTarget.isAction == heldObject.GetComponent<Item>().isAction)
                {
                    snapTarget.Recept(heldObject.GetComponent<Item>());
                    heldObject = null;
                    isHoldingObject = false;
                    playerLink.animator.SetBool("Pegar", false);
                    playerLink.animator.SetBool("Soltar", true);
                    canDrop = false;
                }
            }
        }
        else
        {
            heldObject.transform.SetParent(null);
            heldObject.GetComponent<Rigidbody>().isKinematic = false;
            heldObject.GetComponent<Rigidbody>().useGravity = true;
            Vector3 direction = playerLink.transform.forward;
            heldObject.transform.position = heldObject.transform.position + heldObject.transform.up * 1;
            heldObject = null;
            isHoldingObject = false;
            playerLink.animator.SetBool("Pegar", false);
            playerLink.animator.SetBool("Soltar", true);
            canDrop = false;
        }

    }

    public void Catch()
    {
        if (touchingSnap)
        {
            heldObject = snapTarget.Withdraw();
        }
        else
        {
            heldObject = targetObject;
        }

        if (heldObject != null)
        {
            heldObject.GetComponent<Rigidbody>().isKinematic = true;
            heldObject.transform.SetParent(handsPosition);
            heldObject.transform.localPosition = Vector3.zero;
            heldObject.transform.localRotation = new Quaternion(-0.05f, -0.7f, -0.7f, 0.05f);
            isHoldingObject = true;
            canCatch = false;
            canDrop = true;
            playerLink.animator.SetBool("Pegar", true);
            playerLink.animator.SetBool("Soltar", false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (heldObject != null)
        {
            if (other.GetComponent<Item>() && other.gameObject != heldObject || other.CompareTag("wall"))
            {
                canDrop = false;
            }
        }

    }
    private void OnTriggerStay(Collider other)
    {
        if (!isHoldingObject && other.CompareTag("Pickable"))
        {
            targetObject = other.gameObject;
            canCatch = true;
            GameManager.Instance.ShowControlPopup(1);
        }
        if (other.CompareTag("StartButton"))
        {
            GameManager.Instance.ShowControlPopup(0);
            if (Input.GetButtonDown("X") && !lerMesa.comecar && !isHoldingObject)
            {
                lerMesa.iniciar();
            }
        }

        if (other.GetComponent<Snap>() && snapTarget == null)
        {
            snapTarget = other.GetComponent<Snap>();
            touchingSnap = true;
            if (snapTarget.obj != null)
            {
                canDrop = false;
            }
        }
    }
    private void OnTriggerExit(Collider other)
    {
        targetObject = null;
        if (isHoldingObject && other.GetComponent<Item>())
        {
            canDrop = true;
        }
        if (other.GetComponent<Snap>() && snapTarget != null)
        {
            if (snapTarget.obj != null)
            {
                canDrop = true;
            }
            touchingSnap = false;
            snapTarget = null;
        }
        if (other.gameObject.CompareTag("wall"))
        {
            canDrop = true;
        }
        GameManager.Instance.HideAllControlPopup();
    }
}

