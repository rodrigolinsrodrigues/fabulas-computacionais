using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class proxFase : MonoBehaviour
{
    public bool Musical, Retornar, Proxima;
    void OnTriggerEnter(Collider other)
    {
        if(Musical == true)
        {
            SceneManager.LoadScene(2);
        }

        if(Proxima == true)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }

        if(Retornar == true)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
        }
    }
}
