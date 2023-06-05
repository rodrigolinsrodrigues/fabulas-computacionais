using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class fecharTudo : MonoBehaviour
{
    //Script que fecha todos os sub menus de repetição ao termino da sessão
    public GameObject[] ContadoresDeRepet;
    public GameObject Fundo1, Fundo2;
    public GameObject Deletar1, Deletar2;
    // Start is called before the first frame update
    void Start()
    {
        
        for(int i = 0; i < ContadoresDeRepet.Length; i ++)
        {
            ContadoresDeRepet[i].SetActive(false);
        }
        Deletar1.SetActive(false);
        Deletar2.SetActive(false);
        Fundo1.SetActive(false);
        Fundo2.SetActive(false);
        Fundo1.GetComponent<Image>().enabled = false;
        Fundo2.GetComponent<Image>().enabled = false;

       
    }

}
