using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class trocarBotao : MonoBehaviour
{
    public Play play;
    public RepeatPart2 play2;
    public bool P1, P2;
    public GameObject BotaoPlay,X2;
    // Start is called before the first frame update
    public void TrocarBotao()
    {
        if(P1 == true)
        {
        GetComponent<Button>().interactable = false;
        EventSystem.current.SetSelectedGameObject(BotaoPlay);
        if(play.lista[0] != null && play.lista[1] != null )
        {
          EventSystem.current.SetSelectedGameObject(X2);
        } 
        }

        if(P2 == true)
        {
        GetComponent<Button>().interactable = false;
        EventSystem.current.SetSelectedGameObject(BotaoPlay);
        if(play2.lista[0] != null && play2.lista[1] != null )
        {
          EventSystem.current.SetSelectedGameObject(X2);
        } 
        }
       
       
    }

    public void TrocarBotaoDel()
    {
        GetComponent<Button>().interactable = false;
        EventSystem.current.SetSelectedGameObject(BotaoPlay);
    }
}
