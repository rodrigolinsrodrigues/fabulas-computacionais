using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ApagarNota : MonoBehaviour
{
    public GameObject fechar, apagar;
    public Sprite original;
    void Update()
    {
        if(this.gameObject.GetComponent<AudioSource>().clip != null && fechar.activeSelf == false)
        {
            apagar.SetActive(true);
        }
    }


    public void Apagar()
    {
        this.gameObject.GetComponent<AudioSource>().clip = null;
        this.gameObject.GetComponent<Image>().sprite = original;
    }
    
}
