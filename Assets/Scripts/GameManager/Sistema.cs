using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Sistema : MonoBehaviour
{
    private Comandos comandos;
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
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        if(comandos.System.Close.ReadValue<float>() == 1)
        {
            Application.Quit();
        }
       
    }
}
