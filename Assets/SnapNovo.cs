using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnapNovo : MonoBehaviour
{
    //Script com o sistema de Snap novo, ainda em construção
    public string Tipo; // Detrmina o tipo do objeto que deve ser o filho correto;
    public GameObject Espaco; //Pega esse proprio objeto;
    public Transform local; // Local onde o objeto que foi solto na area ira ficar atrelado;
    public GameObject LE; // GameObject com o script que lê os acertos;
    void Start()
    {
        LE = GameObject.FindWithTag("LE");
        Espaco = this.gameObject;
    }
    // Update is called once per frame
    void OnTriggerStay(Collider other)
    {
        if(other.CompareTag("Pickable"))
        {
            if(other.transform.parent == null && Espaco.gameObject.transform.childCount < 2)
            {
                other.transform.SetParent(Espaco.gameObject.transform);
                other.transform.position = local.position;
                other.gameObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
                LE.GetComponent<LeituraEspacos>().Check();
            }
            
        }
    }

    void OnTriggerExit(Collider other)
    {
        if(other.CompareTag("Pickable"))
        {
            if(other.transform.parent != Espaco.gameObject.transform)
            {
                LE.GetComponent<LeituraEspacos>().Check();
            }
           
        }
    }
}
