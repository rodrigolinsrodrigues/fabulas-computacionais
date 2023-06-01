using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class mudarSpritesLoop : MonoBehaviour
{
   public Play play;
   public  RepeatPart2 play2;


    public void OnClick()
    {
        if(play != null)
        {
        play.lista[0].gameObject.GetComponent<Image>().sprite = this.gameObject.GetComponent<Image>().sprite;
        play.lista[1].gameObject.GetComponent<Image>().sprite = this.gameObject.GetComponent<Image>().sprite;
        }

        if(play2!= null)
        {
        play2.lista[0].gameObject.GetComponent<Image>().sprite = this.gameObject.GetComponent<Image>().sprite;
        play2.lista[1].gameObject.GetComponent<Image>().sprite = this.gameObject.GetComponent<Image>().sprite;
        }
       
       
    }
}
