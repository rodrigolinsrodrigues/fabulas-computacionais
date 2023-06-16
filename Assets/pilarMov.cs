using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class pilarMov : MonoBehaviour
{
   public float localInicial, localAtual;
    public float delay;

void Start()
{
    localInicial = this.transform.position.x;
}
public void MudouPos()
{
    //localAtual = this.transform.position.x;
    if(this.transform.position.x != localInicial)
    {
        StartCoroutine("Delay");
        localInicial = this.transform.position.x;
    }
}
IEnumerator Delay()
{
yield return new WaitForSeconds(delay);
this.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezePositionZ;

yield return new WaitForSeconds(0.5f);
this.GetComponent<Rigidbody>().constraints = ~RigidbodyConstraints.FreezePositionX;
}
}
