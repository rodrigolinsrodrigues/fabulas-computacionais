using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class trocarBotao : MonoBehaviour
{
    public GameObject BotaoPlay;
    // Start is called before the first frame update
    public void TrocarBotao()
    {
        GetComponent<Button>().interactable = false;
       EventSystem.current.SetSelectedGameObject(BotaoPlay);
    }

    public void TrocarBotaoDel()
    {
        GetComponent<Button>().interactable = false;
        EventSystem.current.SetSelectedGameObject(BotaoPlay);
    }
}
