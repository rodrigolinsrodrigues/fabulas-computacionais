using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class checarPartitura : MonoBehaviour
{
   
    public GameObject[] NotaPart1, NotaPart2;
    public GameObject[] RepetPart1, RepetPart2;
    public GameObject ContadorP1, ContadorP2;
    GameObject  ContP1, ContP2;
    public string[] TipoNotaPart1, TipoNotaPart2;
    public bool check, part1, part1repet, part2,part2repet, ApenasPart1;
    public GameObject conluido;
    public Play play;
    public RepeatPart2 play2;
    GameObject  botãoClicado1Part1,  botãoClicado2Part1, botãoClicado1Part2, botãoClicado2Part2;
    // Start is called before the first frame update

    public void Update()
    {
        if(ApenasPart1 == true){Partitura1();}
        if(ApenasPart1 == false){Partitura1(); Partitura2();}
       

        if(check == true)
        {
            if((part1 == true || part1repet == true)){conluido.SetActive(true); StartCoroutine("Delay"); check = false;}
            if((part2 == true || part2repet == true)){conluido.SetActive(true); StartCoroutine("Delay"); check = false;}
            if((part1 == true || part1repet == true) && (part2 == true || part2repet == true)){conluido.SetActive(true); StartCoroutine("Delay"); check = false;}
        }
    }
    void Partitura1()
    {
        //Repetição
        if(RepetPart1[0] != null && RepetPart1[1] != null && ContP1 != null)
        {
        if((NotaPart1[0].GetComponent<AudioSource>().clip.name == TipoNotaPart1[0] && NotaPart1[1].GetComponent<AudioSource>().clip.name == TipoNotaPart1[1]) && 
        (RepetPart1[0] == botãoClicado1Part1 && RepetPart1[1] == botãoClicado2Part1 && ContP1 == ContadorP1))
        {
            part1repet = true;
        }
        if((NotaPart1[0].GetComponent<AudioSource>().clip.name != TipoNotaPart1[0] || NotaPart1[1].GetComponent<AudioSource>().clip.name != TipoNotaPart1[1]) ||
        (RepetPart1[0] != botãoClicado1Part1 || RepetPart1[1] != botãoClicado2Part1))
        {
            part1repet = false;
        } 
        }

        if(NotaPart1[0].GetComponent<AudioSource>().clip != null && NotaPart1[1].GetComponent<AudioSource>().clip != null &&  NotaPart1[2].GetComponent<AudioSource>().clip != null &&  NotaPart1[3].GetComponent<AudioSource>().clip != null)
        {
             //Normal
        if(NotaPart1[0].GetComponent<AudioSource>().clip.name == TipoNotaPart1[0] && NotaPart1[1].GetComponent<AudioSource>().clip.name == TipoNotaPart1[1]
        && NotaPart1[2].GetComponent<AudioSource>().clip.name == TipoNotaPart1[2] && NotaPart1[3].GetComponent<AudioSource>().clip.name == TipoNotaPart1[3])
        {
            part1 = true;
        } 

        if(NotaPart1[0].GetComponent<AudioSource>().clip.name != TipoNotaPart1[0] || NotaPart1[1].GetComponent<AudioSource>().clip.name != TipoNotaPart1[1]
        || NotaPart1[2].GetComponent<AudioSource>().clip.name != TipoNotaPart1[2] || NotaPart1[3].GetComponent<AudioSource>().clip.name != TipoNotaPart1[3])
        {
            part1 = false;
        } 
        }
    }

    void Partitura2()
    {
        if(RepetPart2[0] != null && RepetPart2[1] != null && ContP2 != null)
        {
        if((NotaPart2[0].GetComponent<AudioSource>().clip.name == TipoNotaPart2[0] && NotaPart2[1].GetComponent<AudioSource>().clip.name == TipoNotaPart2[1]) && 
        (RepetPart2[0] == botãoClicado1Part2 && RepetPart2[1] == botãoClicado2Part2 && ContP2 == ContadorP2))
        {
            part2repet = true;
        }
        if((NotaPart2[0].GetComponent<AudioSource>().clip.name != TipoNotaPart2[0] || NotaPart2[1].GetComponent<AudioSource>().clip.name != TipoNotaPart2[1]) ||
        (RepetPart2[0] != botãoClicado1Part2 || RepetPart2[1] != botãoClicado2Part2))
        {
            part2repet = false;
        }
        }

        if(NotaPart2[0].GetComponent<AudioSource>().clip != null && NotaPart2[1].GetComponent<AudioSource>().clip != null &&  NotaPart2[2].GetComponent<AudioSource>().clip != null &&  NotaPart2[3].GetComponent<AudioSource>().clip != null)
        {
        if(NotaPart2[0].GetComponent<AudioSource>().clip.name == TipoNotaPart2[0] && NotaPart2[1].GetComponent<AudioSource>().clip.name == TipoNotaPart2[1]
        && NotaPart2[2].GetComponent<AudioSource>().clip.name == TipoNotaPart2[2] && NotaPart2[3].GetComponent<AudioSource>().clip.name == TipoNotaPart2[3])
        {
            part2 = true;
        }
        if(NotaPart2[0].GetComponent<AudioSource>().clip.name != TipoNotaPart2[0] || NotaPart2[1].GetComponent<AudioSource>().clip.name != TipoNotaPart2[1]
        || NotaPart2[2].GetComponent<AudioSource>().clip.name != TipoNotaPart2[2] || NotaPart2[3].GetComponent<AudioSource>().clip.name != TipoNotaPart2[3])
        { 
            part2 = false;
        }
        }


    }
    IEnumerator Delay()
    {
        yield return new WaitForSeconds(3);
       // conluido.SetActive(false);
    }
    public void repet1Part1(GameObject botãoClicado)
    {
       RepetPart1[0] = botãoClicado;
       botãoClicado1Part1 = botãoClicado;
    }

    public void repet2Part1(GameObject botãoClicado)
    {
       RepetPart1[1] = botãoClicado;
       botãoClicado2Part1 = botãoClicado;
    }

    public void repet1Part2(GameObject botãoClicado)
    {
       RepetPart2[0] = botãoClicado;
       botãoClicado1Part2 = botãoClicado;
    }

    public void repet2Part2(GameObject botãoClicado)
    {
       RepetPart2[1] = botãoClicado;
       botãoClicado2Part2 = botãoClicado;
    }

    public void ContadorRepetição(GameObject ContadorSelecionado)
    {
        ContadorP1 = ContadorSelecionado;
        ContP1 = ContadorP1;
        Debug.Log(ContadorP1.name);
    }
    public void ContadorRepetição2(GameObject ContadorSelecionado)
    {
        ContadorP2 = ContadorSelecionado;
        ContP2 = ContadorP2;
        Debug.Log(ContadorP2.name);
    }
}
