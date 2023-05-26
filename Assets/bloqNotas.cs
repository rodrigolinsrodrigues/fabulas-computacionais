using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class bloqNotas : MonoBehaviour
{
    public GameObject[] Notas; //Pega todas as notas selecionaveis.
    public GameObject Concluido;
    public bool completo;
    public Play play;
    // Start is called before the first frame update
    // Update is called once per frame
    void Update()
    {
        if(Concluido.activeSelf)
        {
            completo = true;
        }
        if(Concluido.activeSelf == false)
        {
            completo = false;
        }
        if(play.tocar == true || completo == true)
        {
            for(int i = 0; i < Notas.Length; i++)
            {
                Notas[i].GetComponent<Button>().enabled = false;
            }
        }
       
        if(completo == true && play.tocar == false)
        {
            for(int i = 0; i < Notas.Length; i++)
            {
                Notas[i].GetComponent<Button>().enabled = false;
            }
        }
        if(completo == false && play.tocar == false)
        {
            for(int i = 0; i < Notas.Length; i++)
            {
                Notas[i].GetComponent<Button>().enabled = true;
            }
        }
    }
}
