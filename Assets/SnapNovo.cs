using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnapNovo : MonoBehaviour
{
   public string Tipo;
    public GameObject Objeto;
    public Transform local;
   // public bool Ok;
   
    // Update is called once per frame
    void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Pickable"))
        {
            Objeto = other.gameObject;
            if(Objeto.transform.parent == null)
            {
                Objeto.transform.SetParent(this.gameObject.transform);
                Objeto.transform.position = local.position;
            }
        }
    }
}
