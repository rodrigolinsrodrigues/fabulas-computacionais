using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class checkRepet : MonoBehaviour
{
    //Script que serve como uma referencia para o botão de repetição, é usado em apenas um botão especifico que é determinado de acordo com o level design
   public checarPartitura CP;
    public void OnClick(GameObject EsseBotao) // Pega o click do botão
    {
        if(CP.RepetPart1[0] == null) // Checa se o espaço correspondende do botão esta disponivel
        {
            CP.RepetPart1[0] = EsseBotao;
            CP.botãoClicado1Part1 =  CP.RepetPart1[0];
            CP.notap11 = 2;
        }
        else
        if(CP.RepetPart1[0] != null && CP.RepetPart1[1] != EsseBotao) // Checa se o segundo espaço correspondende do botão esta disponivel
        {
            CP.RepetPart1[1] = EsseBotao;
            CP.botãoClicado2Part1 =  CP.RepetPart1[1]; // aplica esse botão ao primeiro espaço disponivel

        }

       
    }
}
