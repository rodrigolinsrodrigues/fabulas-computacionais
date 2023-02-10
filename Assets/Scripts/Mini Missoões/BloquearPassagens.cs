using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BloquearPassagens : MonoBehaviour
{
    //Codigo responsvel por controlar os gatuilhos de bloqueio em areas de missões. utilizado para missões de teste, mas pode ser incrementado e usado como base para outras
    public GameObject carryObj, area;
    public bool Cubo,Esfera; // Checa se a area de colisão é determinada por um campo esferico ou quadrado
    public PDE Layer;
    // Start is called before the first frame update
    

    void OnTriggerStay(Collider other)
    {
    if(other.GetComponent<Collider>().tag == "Pickable" && other.GetComponent<Collider>().gameObject.layer != Layer.layer)
    {
        string name = other.GetComponent<Collider>().name;
        carryObj = GameObject.Find(name);
        if(Esfera == true)
        {
            area.GetComponent<SphereCollider>().isTrigger = false;
        }
        if(Cubo == true)
        {
            area.GetComponent<BoxCollider>().isTrigger = false;
        }
    }
    else if(other.GetComponent<Collider>().tag == "Pickable" && other.GetComponent<Collider>().gameObject.layer == Layer.layer)
    {
        if(Esfera == true)
        {
            area.GetComponent<SphereCollider>().isTrigger = true;
        }
        if(Cubo == true)
        {
            area.GetComponent<BoxCollider>().isTrigger = true;
        }
    }
    }
}
