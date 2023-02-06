using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Repeat : MonoBehaviour
{
    public Play botRepet;
    public GameObject Ponto0,Ponto1,Ponto2,Ponto3,Ponto4;
    public void RepeteEsse() // chamar no botao
    {
        switch(this.gameObject.name)
        {
            case "0":
            botRepet.pontoRepet = Ponto0;
            break;

            case "1":
            botRepet.pontoRepet = Ponto1;
            break;

            case "2":
            botRepet.pontoRepet = Ponto2;
            break;

            case "3":
            botRepet.pontoRepet = Ponto3;
            break;

            case "4":
            botRepet.pontoRepet = Ponto4;
            break;
        }

        botRepet.lista.Add(this.gameObject);
    }
}
