using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Sistema : MonoBehaviour
{
    //Codigo simples de manipulação de cena, reload e fechar jogo
    private Comandos comandos;
    public GameObject[] fases;
    public bool Luz, Musical;
    public string NomeCena;
    // Start is called before the first frame update
    void Start()
    {
        comandos = new Comandos();
        comandos.Enable(); 
        NomeCena = SceneManager.GetActiveScene().name;
    }

    // Update is called once per frame
    void Update()
    {
        if(comandos.System.Restart.ReadValue<float>()== 1)
        {
           reload();
        }
        if(comandos.System.Close.ReadValue<float>() == 1)
        {
            Application.Quit();
        }

        if(Luz == true)
        {
            CenasLuz();
        }

        if(Musical == true)
        {
            CenasMusical();
        }


    }

    public void reload()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void CenasLuz()
    {
        if(NomeCena != "CaminhoDaLuz_Fase1" && Input.GetKeyDown(KeyCode.Alpha1))
        {
            SceneManager.LoadScene("CaminhoDaLuz_Fase1");
        }
         if(NomeCena != "CaminhoDaLuz_Fase2" && Input.GetKeyDown(KeyCode.Alpha2))
        {
            SceneManager.LoadScene("CaminhoDaLuz_Fase2");
        }
         if(NomeCena != "CaminhoDaLuz_Fase3" && Input.GetKeyDown(KeyCode.Alpha3))
        {
            SceneManager.LoadScene("CaminhoDaLuz_Fase3");
        }
         if(NomeCena != "CaminhoDaLuz_Fase4" && Input.GetKeyDown(KeyCode.Alpha4))
        {
            SceneManager.LoadScene("CaminhoDaLuz_Fase4");
        }
    }

    void CenasMusical()
    {
        if(Input.GetKeyDown(KeyCode.Alpha1))
        {
            fases[0].gameObject.SetActive(true);
            fases[1].gameObject.SetActive(false);
            fases[2].gameObject.SetActive(false);
            fases[3].gameObject.SetActive(false);
        }
         if(Input.GetKeyDown(KeyCode.Alpha2))
        {
            fases[0].gameObject.SetActive(false);
            fases[1].gameObject.SetActive(true);
            fases[2].gameObject.SetActive(false);
            fases[3].gameObject.SetActive(false);
        }
         if(Input.GetKeyDown(KeyCode.Alpha3))
        {
            fases[0].gameObject.SetActive(false);
            fases[1].gameObject.SetActive(false);
            fases[2].gameObject.SetActive(true);
            fases[3].gameObject.SetActive(false);
        }
         if(Input.GetKeyDown(KeyCode.Alpha4))
        {
            fases[0].gameObject.SetActive(false);
            fases[1].gameObject.SetActive(false);
            fases[2].gameObject.SetActive(false);
            fases[3].gameObject.SetActive(true);
        }
    }
}
