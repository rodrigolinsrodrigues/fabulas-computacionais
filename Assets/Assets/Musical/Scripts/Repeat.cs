using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Repeat : MonoBehaviour
{
    //Script responsavel por pegar as informações dos botões de repetição e atrivuir os intervalos aos "POntos", que são transforms que existem para guiar o tempo de cada nota
    public Play botRepet;
    public RepeatPart2 botRepet2;
    public GameObject Ponto0,Ponto1,Ponto2,Ponto3,Ponto4;

    public void RepeteEssePt1() // chamar no botao
    {
       
        switch(this.gameObject.name)
        {
            case "0":
            botRepet.pontoRepet = Ponto0;
            break;

            case "1":
            botRepet.pontoRepet = Ponto1;
            break;

            case "2 - Pt1":
            botRepet.pontoRepet = Ponto2;
            break;

            case "3 - Pt1":
            botRepet.pontoRepet = Ponto3;
            break;

            case "4 - Pt1":
            botRepet.pontoRepet = Ponto4;
            break;
        }
        botRepet.lista.Add(this.gameObject);
    

    }

    public void RepeteEssePt2()
    {
        switch(this.gameObject.name)
        {
            case "0":
            botRepet2.pontoRepet = Ponto0;
            break;

            case "1":
            botRepet2.pontoRepet = Ponto1;
            break;

            case "2 - Pt1":
            botRepet2.pontoRepet = Ponto2;
            break;

            case "3 - Pt1":
            botRepet2.pontoRepet = Ponto3;
            break;

            case "4 - Pt1":
            botRepet2.pontoRepet = Ponto4;
            break;
        }
        botRepet2.lista.Add(this.gameObject);
    }
       
}
