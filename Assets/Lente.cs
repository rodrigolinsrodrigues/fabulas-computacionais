using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lente : MonoBehaviour
{
    public GameObject[] Uteis, Inuteis;
    private Comandos comandos;
    public GameObject hand;
    public float distance;
    void Awake()
    {
        comandos = new Comandos();
        comandos.Actions.Enable(); 
    }
    
    private void Update()
    {
        Ray ray = new Ray(hand.transform.position, hand.transform.forward);
        RaycastHit hit;

        if(Physics.Raycast(hand.transform.position, hand.transform.forward, out hit, distance))
        {
        if(hit.collider.tag == "Lens" && comandos.Actions.Interact.triggered)
        {
            Debug.Log("Player na area");
            for(int i = 0; i < Inuteis.Length; i++)
            {
                Inuteis[i].SetActive(false);
                Uteis[i].SetActive(true);
            }
             Destroy(GetComponent<Lente>());
        }
        }
        



    }
   

       
    
}
