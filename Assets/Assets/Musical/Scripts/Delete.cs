using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Delete : MonoBehaviour
{
    public Sprite original;
    public GameObject Coluna;
    public bool podeApagar = false;
    public HabilitarBotao HB;
    public AdicionarSom AS;
   /* void Update()
    {
        if(this.gameObject.GetComponent<AudioSource>().clip != null)
        {
            podeApagar = true;
            if(podeApagar == true) { Apagar();}
          
        }
    } */

    public void Pressionado(GameObject nota)
    {
        if(nota.gameObject.GetComponent<AudioSource>().clip != null)
        {
            podeApagar = true;
            HB.Desabilitar();
            if(podeApagar == true) {Apagar();}
        }
    }
    public void Apagar()
    {
        this.gameObject.GetComponent<AudioSource>().clip = null;
        this.gameObject.GetComponent<Image>().sprite = original;
        for(int i = 0; i < 3; i++)
        {
           Coluna.gameObject.transform.GetChild(i).GetComponent<Button>().interactable = true;
        }
       AS.pressionado = true;
    }
}
