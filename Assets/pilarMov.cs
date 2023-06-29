using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class pilarMov : MonoBehaviour
{
   public float localInicialX, localInicialZ, localAtual;
   public float delay;
   public bool ifX, ifZ;
   public GameObject Player;
   public RigidbodyConstraints OriginalConstraints;

void Start()
{
    OriginalConstraints = this.GetComponent<Rigidbody>().constraints;
    Player = GameObject.FindWithTag("Player");
    
}

public void MudouPos()
{
    if(Player.GetComponent<Launch>().Empurrou == true)
    {
        StartCoroutine("Delay");
    }
    
}
IEnumerator Delay()
{
    yield return new WaitForSeconds(delay);
    this.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
    yield return new WaitForSeconds(0.5f);
    this.GetComponent<Rigidbody>().constraints = OriginalConstraints;
}

IEnumerator Delay2()
{
    yield return new WaitForSeconds(delay);
    this.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
    yield return new WaitForSeconds(0.5f);
    this.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionY | ~RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
}

}
