using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakObj : MonoBehaviour
{
    private Comandos comandos;
    // Lançar objetos
    public GameObject pedacos, hand, BreakableObj;
    public float viewDistance, breakForce;
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
        if (Physics.Raycast(hand.transform.position, hand.transform.forward, out hit, viewDistance))
        {
            if (hit.collider.tag == "Break" && Input.GetButtonDown("A"))
            {
                string name = hit.collider.name;
                BreakableObj = GameObject.Find(name);
                Quebrar();
            }

        }
    }

    public void Quebrar()
    {
        GameObject Pedacos = Instantiate(pedacos, BreakableObj.transform.position, BreakableObj.transform.rotation);

        foreach (Rigidbody rb in Pedacos.GetComponentsInChildren<Rigidbody>())
        {
            Vector3 force = (rb.transform.position - BreakableObj.transform.position).normalized * breakForce;
        }

        BreakableObj.SetActive(false);
    }

    IEnumerator Delay()
    {
        yield return new WaitForSeconds(4f);

    }
}
