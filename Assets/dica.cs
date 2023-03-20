using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class dica : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject CPlayer, C1, C2;
    
    public void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            CPlayer.SetActive(false);
            C1.SetActive(true);
            StartCoroutine("c1");
           
        }
    }
    IEnumerator c1()
    {
        yield return new WaitForSeconds(3);
        C1.SetActive(false);
        C2.SetActive(true);
        StartCoroutine("c2");

    }
    IEnumerator c2()
    {
        yield return new WaitForSeconds(5);
        C2.SetActive(false);
        CPlayer.SetActive(true);
        this.gameObject.SetActive(false);
    }

}
