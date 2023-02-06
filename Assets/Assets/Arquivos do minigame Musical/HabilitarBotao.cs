using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HabilitarBotao : MonoBehaviour
{
    public GameObject CaixaDeSelecao;
    // Start is called before the first frame update
    void Start()
    {
        //CaixaDeSelecao = this.transform.GetChild(1).GetComponent<GameObject>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Habilitar()
    {
        for(int i = 0; i < 4; i++)
        {
        CaixaDeSelecao.transform.GetChild(i).gameObject.GetComponent<Button>().interactable = true;
        CaixaDeSelecao.transform.GetChild(i).gameObject.GetComponent<Image>().enabled = true;

        }
    }

     public void Desabilitar()
    {
        for(int i = 0; i < 4; i++)
        {
        CaixaDeSelecao.transform.GetChild(i).gameObject.GetComponent<Button>().interactable = false;
        CaixaDeSelecao.transform.GetChild(i).gameObject.GetComponent<Image>().enabled = false;

        }
    }

}
