using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickAndDrop : MonoBehaviour
{
    public Transform handsPosition; // Referência ao objeto que define a posição das mãos do jogador
    private Comandos comandos;
    public GameObject heldObject, targetObject;
    public bool isHoldingObject, canCatch;

    private void Awake()
    {
        comandos = new Comandos();
        comandos.Enable();
        Cursor.visible = false;
    }

    private void Update()
    {
        if (Input.GetButtonDown("X"))
        {
            CatchRelease();
        }
    }

    public void CatchRelease()
    {
        if (isHoldingObject)
        {
            Debug.Log("soltou");
            heldObject.transform.SetParent(null);
            heldObject.GetComponent<Rigidbody>().isKinematic = false;
            heldObject = null;
            isHoldingObject = false;
        }
        else
        {
            if (targetObject != null)
            {
                Debug.Log("pegou");
                // Se o objeto é interativo, pegue-o
                heldObject = targetObject;
                heldObject.GetComponent<Rigidbody>().isKinematic = true;
                heldObject.transform.SetParent(handsPosition); // Definir o objeto como filho da posição das mãos
                heldObject.transform.localPosition = Vector3.zero; // Define a posição local do objeto como (0, 0, 0)
                heldObject.transform.localRotation = Quaternion.identity; // Define a rotação local do objeto como a rotação padrão
                isHoldingObject = true;
                canCatch = false;
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!isHoldingObject && other.CompareTag("Pickable"))
        {
            targetObject = other.gameObject;
            canCatch = true;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        targetObject = null;
    }
}

