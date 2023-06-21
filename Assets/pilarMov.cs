using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class pilarMov : MonoBehaviour
{
   public float localInicialX, localInicialZ, localAtual;
   public float delay;

void Start()
{
    localInicialX = this.transform.position.x;
    localInicialZ = this.transform.position.z;
}
public void MudouPos()
{
    //localAtual = this.transform.position.x;
    if(this.transform.position.x != localInicialX)
    {
        StartCoroutine("Delay");
        localInicialX = this.transform.position.x;
    }

   /* if(this.gameObject.name.Contains("Pilar Vermelho") && this.transform.position.x != localInicialX || this.gameObject.name.Contains("Pilar Vermelho") && this.transform.position.z != localInicialZ)
    {
        StartCoroutine("Delay2");
        localInicialX = this.transform.position.x;
        localInicialZ = this.transform.position.z;
    } */
}
IEnumerator Delay()
{
yield return new WaitForSeconds(delay);
this.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
yield return new WaitForSeconds(0.5f);
this.GetComponent<Rigidbody>().constraints = ~RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
}

IEnumerator Delay2()
{
yield return new WaitForSeconds(delay);
this.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ;
yield return new WaitForSeconds(0.5f);
this.GetComponent<Rigidbody>().constraints = ~RigidbodyConstraints.FreezePositionX | ~RigidbodyConstraints.FreezePositionZ;
}

}
