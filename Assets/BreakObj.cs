using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakObj : MonoBehaviour
{
    // Lançar objetos
    public GameObject pedacos, hand;
    public float breakForce;
    public PlayerController playerController;
    public bool canBreak;

    void Start()
    {
        playerController = FindObjectOfType<PlayerController>();
    }

    void Update()
    {
        if (canBreak && Input.GetButtonDown("A"))
        {
            playerController.animator.SetTrigger("Atacar");
            GameManager.Instance.HideAllControlPopup();
            StartCoroutine("Delay");
        }
    }

    public void Quebrar()
    {
        GameObject Pedacos = Instantiate(pedacos, new Vector3(transform.position.x, transform.position.y + 10, transform.position.z), Quaternion.identity);

        foreach (Rigidbody rb in Pedacos.GetComponentsInChildren<Rigidbody>())
        {
            rb.AddForce(transform.up * breakForce/2);
            rb.AddForce(hand.transform.forward * breakForce);
        }

        gameObject.SetActive(false);
        GameManager.Instance.HideAllControlPopup();
    }

    IEnumerator Delay()
    {
        yield return new WaitForSeconds(0.5f);
        Quebrar();
    }

    void OnTriggerStay(Collider other)
    {
        if (other.GetComponent<LoadNewArea>() != null)
        {
            canBreak = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<LoadNewArea>() != null)
        {
            canBreak = false;
        }
    }
}
