using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbrirMiniGame : MonoBehaviour
{
      private Comandos comandos; // acessa o sistema de inputs
      public GameObject MiniGame;
    // Start is called before the first frame update
    void Start()
    {
        comandos = new Comandos();
        comandos.Actions.Enable(); 
    }

    void OnTriggerStay(Collider other)
    {
        if(other.CompareTag("Player") && comandos.Actions.PickUp.ReadValue<float>() == 1)
        {
            Debug.Log(other.name);
            MiniGame.SetActive(true);
        }
    }
}
