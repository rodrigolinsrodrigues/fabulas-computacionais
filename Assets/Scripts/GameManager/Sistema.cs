using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Sistema : MonoBehaviour
{
    //Codigo simples de manipulação de cena, reload e fechar jogo
    private Comandos comandos;
    public GameObject[] fases;
    // Start is called before the first frame update
    void Start()
    {
        comandos = new Comandos();
        comandos.Enable(); 
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

    public void reload()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

}
