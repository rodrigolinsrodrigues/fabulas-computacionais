using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeituraResultado : MonoBehaviour
{
    public GameObject[] Resultados;
    public GameObject Bloquear, fala;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
       
    }
    
    public void ChecarResultado()
    {
        if(Resultados[0].gameObject.name.Contains("Pao_cortado") && Resultados[1].gameObject.name.Contains("Queijo_cortado") && Resultados[2].gameObject.name.Contains("Ovo_frito") && Resultados[3].gameObject.name.Contains("Cebola_cortado") && Resultados[4].gameObject.name.Contains("Pao_cortado"))
        {
            Debug.Log("Concluido");
            fala.SetActive(true);
        }
        else
        {
            Debug.Log("Tente de novo");
        }
    }
}
