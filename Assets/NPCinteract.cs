using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCinteract : MonoBehaviour
{
    public GameObject Cam, fala, player;
    public int tempo;
    // Start is called before the first frame update
     void Update() 
    {
        if(this.GetComponent<Collider>().enabled == false)
        {
            player.GetComponent<PlayerController>().enabled = true;
        }
    }
    void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            StartCoroutine("Tempo");
            player = other.gameObject;
            player.GetComponent<PlayerController>().animator.SetBool("Andar", false);
            player.GetComponent<PlayerController>().animator.SetBool("Correr", false);
            player.GetComponent<PlayerController>().enabled = false;
            this.Cam.SetActive(true);
            this.fala.SetActive(true);
           
        }
       
    }
    IEnumerator Tempo()
    {
        yield return new WaitForSeconds(tempo);
        this.GetComponent<Collider>().enabled = false;
        this.Cam.SetActive(false);
        this.fala.SetActive(false);
    }
   
}
