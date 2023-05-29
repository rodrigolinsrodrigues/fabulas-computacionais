using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HabilitarBotao : MonoBehaviour
{
    //Script utilizado para habilitar ou desabilitar a interação dos botoes de som e a sua imagem
    //São atrelados ao objeto nota correspondente, habilitado apertando no botão da nota, desabilitado apertando o botão fechar
    public GameObject CaixaDeSelecao;
   //  bool pressionado = false;
    // Start is called before the first frame update
    
    public void Habilitar()
    {
        {
            for(int i = 0; i < 4; i++)
            {
            CaixaDeSelecao.transform.GetChild(i).gameObject.GetComponent<Button>().interactable = true;
            CaixaDeSelecao.transform.GetChild(i).gameObject.GetComponent<Image>().enabled = true;
            }
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
