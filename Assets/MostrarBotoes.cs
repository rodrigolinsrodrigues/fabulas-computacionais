using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MostrarBotoes : MonoBehaviour
{
    private void OnTriggerStay(Collider other)
    {
        if (other.tag == "Launch" || other.tag == "Rotate" || other.tag == "Emit" || other.tag == "Lens" || other.tag == "door" || other.CompareTag("StartButton"))
        {
            GameManager.Instance.ShowControlPopup(0);
        }
        else if (other.tag == "Break")
        {
            GameManager.Instance.ShowControlPopup(5);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Launch" || other.tag == "Rotate" || other.tag == "Emit" || other.tag == "Lens" || other.tag == "door" || other.tag == "Break" || other.CompareTag("StartButton"))
        {
            GameManager.Instance.HideAllControlPopup();
        }
    }
}
