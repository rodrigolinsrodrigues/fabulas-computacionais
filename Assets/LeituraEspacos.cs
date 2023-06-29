using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeituraEspacos : MonoBehaviour
{   //Script novo para leitura de Espaços de snap 
    public GameObject[] Espacos; //Engloba todas os espaços a serem lidos, espaços podem ser de diversos tipos;
    public GameObject[] Objetos; //Arrya usado pra coletar todos os objetos de interação do puzzle e desativar as interações uma vez que o jogador acerte o puzzle;
    public GameObject PopUp; // Pega o Canvas com o popUp de conclusão
    public bool Concluido = false; // Bool que determina o acerto;
    public bool Fase1, Fase2, Fase3, Fase4; //Bools que separam a verficação por cada uma das fases
    
    public void Check()
    {   
        if(Fase1 ==  true)
        {
            if(Espacos[0].transform.GetChild(1).GetComponent<Tipo>().tipo == Espacos[0].GetComponent<SnapNovo>().Tipo && 
            Espacos[1].transform.GetChild(1).GetComponent<Tipo>().tipo == Espacos[1].GetComponent<SnapNovo>().Tipo && 
            Espacos[2].transform.GetChild(1).GetComponent<Tipo>().tipo == Espacos[2].GetComponent<SnapNovo>().Tipo && 
            Espacos[3].transform.GetChild(1).GetComponent<Tipo>().tipo == Espacos[3].GetComponent<SnapNovo>().Tipo)
            {
                Concluido = true;
            }

            if(Concluido == true)
            {
               PopUp.SetActive(true);
               for(int i = 0; i < Objetos.Length; i ++)
               {
                    Objetos[i].tag = "Untagged";
               }
               StartCoroutine("Delay");
            }
        }
       
    }


    IEnumerator Delay()
    {
        yield return new WaitForSeconds(3f);
        PopUp.SetActive(false);
    }
}
