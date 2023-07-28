using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MudarBotao : MonoBehaviour
{
    public void ChangeSelectedButton(Button newSelectedButton)
    {
    EventSystem.current.SetSelectedGameObject(newSelectedButton.gameObject);
    }   
}
