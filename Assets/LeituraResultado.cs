using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeituraResultado : MonoBehaviour
{
    public GameObject[] Resultados;
    public GameObject Bloquear, fala, comecar;
    // Start is called before the first frame update
    
    public void ChecarResultado()
    {
        if(Resultados[0].gameObject.name.Contains("Pao_cortado") && Resultados[1].gameObject.name.Contains("Queijo_cortado") && Resultados[2].gameObject.name.Contains("Ovo_frito") && Resultados[3].gameObject.name.Contains("Cebola_cortado") && Resultados[4].gameObject.name.Contains("Pao_cortado"))
        {
            Debug.Log("Concluido");
            fala.SetActive(true);
            comecar.SetActive(false);
            Bloquear.GetComponent<Collider>().enabled = true;
            StartCoroutine("delay");
        }
        else
        {
            Debug.Log("Tente de novo");
        }
    }

    IEnumerator delay()
    {
        yield return new WaitForSeconds(5);
         fala.SetActive(false);
    }
}
