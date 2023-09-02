using System.Collections;
using System.Collections.Generic;
//using Microsoft.Unity.VisualStudio.Editor;
using UnityEngine;
using UnityEngine.UI;

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
    public Image itemName;
    public Sprite[] itemSprites;

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
                    itemName.gameObject.SetActive(false);
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
            itemName.gameObject.SetActive(false);
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
            SetNamePopup(heldObject.GetComponent<Item>().type);
            itemName.gameObject.SetActive(true);
        }
    }

    public void SetNamePopup(string type)
    {
        switch (type)
        {
            case "fry":
                itemName.sprite = itemSprites[0];
                break;
            case "cut":
                itemName.sprite = itemSprites[1];
                break;
            case "boxBread":
                itemName.sprite = itemSprites[2];
                break;
            case "egg":
                itemName.sprite = itemSprites[3];
                break;
            case "cheese":
                itemName.sprite = itemSprites[4];
                break;
            case "pineapple":
                itemName.sprite = itemSprites[5];
                break;
            case "orange":
                itemName.sprite = itemSprites[6];
                break;
            case "mango":
                itemName.sprite = itemSprites[7];
                break;
            case "mint":
                itemName.sprite = itemSprites[8];
                break;
            case "passionFruit":
                itemName.sprite = itemSprites[9];
                break;
            case "meat":
                itemName.sprite = itemSprites[10];
                break;
            case "bacon":
                itemName.sprite = itemSprites[11];
                break;
            case "ham":
                itemName.sprite = itemSprites[12];
                break;
            case "pepper":
                itemName.sprite = itemSprites[13];
                break;
            case "sesameBread":
                itemName.sprite = itemSprites[14];
                break;
            case "traditionalBread":
                itemName.sprite = itemSprites[15];
                break;
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
            if (!lerMesa.comecar && lerMesa.startButton.activeSelf)
            {
                GameManager.Instance.ShowControlPopup(1);
            }


            targetObject = other.gameObject;
            canCatch = true;
        }
        if (other.CompareTag("StartButton"))
        {
            if (Input.GetButtonDown("X") && !lerMesa.comecar && !isHoldingObject)
            {
                GameManager.Instance.HideAllControlPopup();
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
        if (other.CompareTag("Pickable"))
        {
            GameManager.Instance.HideAllControlPopup();
        }
    }
}

