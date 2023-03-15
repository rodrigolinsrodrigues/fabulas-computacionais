using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class checarPartitura : MonoBehaviour
{
   
    public GameObject nota1, nota2, nota3, nota4;
    public string nota1tipo, nota2tipo, nota3tipo, nota4tipo;
    public GameObject repet1, repet2, repet3, repet4, repet5;
    public bool check;
    public GameObject MiniGame, TeladeJogo, conluido;
    // Start is called before the first frame update

    public void Start()
    {

    }
    public void Update()
    {
        if(check == true)
        {
        if(nota1.GetComponent<AudioSource>().clip.name.Contains(nota1tipo) && nota2.GetComponent<AudioSource>().clip.name.Contains(nota2tipo) && nota3.GetComponent<AudioSource>().clip.name.Contains(nota3tipo) && nota4.GetComponent<AudioSource>().clip.name.Contains(nota4tipo))
        {
            Debug.Log("Correto");
            check = false;
            MiniGame.SetActive(false);
            TeladeJogo.GetComponent<AbrirMiniGame>().enabled = false;
            conluido.SetActive(true);
            StartCoroutine("Delay");
        }
        }
    }

    IEnumerator Delay()
    {
        yield return new WaitForSeconds(3);
        conluido.SetActive(false);
    }
  
}
