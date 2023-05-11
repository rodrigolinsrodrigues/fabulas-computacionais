using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class checarPartitura : MonoBehaviour
{
   
    public GameObject[] NotaPart1, NotaPart2;
    public string[] TipoNotaPart1, TipoNotaPart2;
    public bool check, part1, part2, ApenasPart1;
    public GameObject conluido;
    public int vezes;
    public Play play;
    // Start is called before the first frame update

    public void Update()
    {
        if(ApenasPart1 == true){Partitura1();}
        if(ApenasPart1 == false){ Partitura1(); Partitura2();}
       

        if(check == true)
        {
            if(part1 == true || part1 == true && part2 == true)
            {
            check = false;
            conluido.SetActive(true);
            StartCoroutine("Delay");
            }

        }
    }
    void Partitura1()
    {
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

    void Partitura2()
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
    IEnumerator Delay()
    {
        yield return new WaitForSeconds(3);
       // conluido.SetActive(false);
    }
    
}
