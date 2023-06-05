using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DelRepet : MonoBehaviour
{
    public Play play;
    public RepeatPart2 play2;
    public checarPartitura CP;
    // Start is called before the first frame update
   public void Del()
   {
    play.delete = true;
    for(int i = 0; i < play.ContadorRepeticao.Length; i++)
        {
            play.ContadorRepeticao[i].GetComponent<Button>().interactable = false;
            play.ContadorRepeticao[i].GetComponent<Image>().enabled = false;
        }
    for(int x = 0; x < play.Repet.Length; x++)
        {
            play.Repet[x].GetComponent<Button>().interactable = true;
        }
    play.fundoContador.SetActive(false);
    play.deletar.GetComponent<Button>().interactable = false;
    play.deletar.GetComponent<Image>().enabled = false;
    play.fundoDel.SetActive(false);
    CP.RepetPart1[0] = null;
    CP.RepetPart1[1] = null;
    StartCoroutine("Delay");
    
   }

   public void Del2()
   {
        play2.delete = true;
        for(int i = 0; i < play2.ContadorRepeticao.Length; i++)
        {
            play2.ContadorRepeticao[i].GetComponent<Button>().interactable = false;
            play2.ContadorRepeticao[i].GetComponent<Image>().enabled = false;
        }
        for(int x = 0; x < play2.Repet.Length; x++)
        {
            play2.Repet[x].GetComponent<Button>().interactable = true;
        }
        play2.fundoContador.SetActive(false);
        play2.deletar.GetComponent<Button>().interactable = false;
        play2.deletar.GetComponent<Image>().enabled = false;
        play2.fundoDel.SetActive(false);
        StartCoroutine("Delay2");
   }

  IEnumerator Delay()
    {
        yield return new WaitForSeconds(0.5f);
        play.delete = false;

    }
    IEnumerator Delay2()
    {
        yield return new WaitForSeconds(0.5f);
        play2.delete = false;

    }
}
