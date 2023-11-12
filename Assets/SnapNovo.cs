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
    public int Contagem; // Deteremina a contagem de filhos sufientes para encher os espaços;
    public bool LocalEspecifico; // Bool que checa se a fase precisa de um local especfico pra que os ovbjetos sejam posicionados ou não;
    void Start()
    {
        LE = GameObject.FindWithTag("LE");
        Espaco = this.gameObject;
    }
    // Update is called once per frame
    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Pickable"))
        {
            if (other.transform.parent == null && Espaco.gameObject.transform.childCount < Contagem)
            {
                other.transform.SetParent(Espaco.gameObject.transform);
                if (LocalEspecifico == true)
                {
                    other.transform.position = local.position;
                    other.transform.rotation = local.rotation;
                    //other.gameObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
                    LE.GetComponent<LeituraEspacos>().Check();
                }
                else
                {
                    LE.GetComponent<LeituraEspacos>().Check();
                }
                //other.GetComponent<Outline>().enabled = true;
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Pickable"))
        {
            if (other.transform.parent != Espaco.gameObject.transform)
            {
                //LE.GetComponent<LeituraEspacos>().Check();
                //other.GetComponent<Outline>().enabled = false;
            }
        }
    }
}
