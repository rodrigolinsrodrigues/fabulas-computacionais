using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class checkLista : MonoBehaviour
{
    public Play play;
    public RepeatPart2 play2;
    public void  OnClikPart1()
    {
        if(play.lista.Count == 2)
        {
            for(int i = 0; i < play.ContadorRepeticao.Length; i++)
            {
            play.ContadorRepeticao[i].GetComponent<Button>().interactable = true;
            play.ContadorRepeticao[i].GetComponent<Image>().enabled = true;
            }
            play.fundoContador.SetActive(true);

            play.deletar.GetComponent<Button>().interactable = true;
            play.deletar.GetComponent<Image>().enabled = true;
            play.fundoDel.SetActive(true);
        }
    }

    public void  OnClikPart2()
    {
        if(play2.lista.Count == 2)
        {
            for(int i = 0; i < play2.ContadorRepeticao.Length; i++)
            {
            play2.ContadorRepeticao[i].GetComponent<Button>().interactable = true;
            play2.ContadorRepeticao[i].GetComponent<Image>().enabled = true;
            }
            play2.fundoContador.SetActive(true);

            play2.deletar.GetComponent<Button>().interactable = true;
            play2.deletar.GetComponent<Image>().enabled = true;
            play2.fundoDel.SetActive(true);
        }
    }
}
