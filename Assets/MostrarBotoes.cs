using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MostrarBotoes : MonoBehaviour
{
    public float Distance;
    public GameObject Guia;

    // Update is called once per frame
    void Start()
    {
        Guia = GameObject.FindWithTag("Guia");
       
    }
    void Update()
    {
        RaycastHit hit;
        if(Physics.Raycast(Guia.transform.position, Guia.transform.forward, out hit, Distance))
        {
            if (hit.collider.tag == "Launch" || hit.collider.tag == "Rotate" ||  hit.collider.tag == "Emit" ||  hit.collider.tag == "Lens" || hit.collider.tag == "door")
            {
               GameManager.Instance.ShowControlPopup(0);
            }
            else if(hit.collider.tag == "Break")
            {
                GameManager.Instance.ShowControlPopup(5);
            }
            else
            {
                GameManager.Instance.HideAllControlPopup();
            }
        }
    }

}
