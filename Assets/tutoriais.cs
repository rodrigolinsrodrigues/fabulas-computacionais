using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class tutoriais : MonoBehaviour
{
    public GameObject tutorial;
    public int tempo;
    // Start is called before the first frame update
    void Start()
    {
        tutorial.SetActive(true);
        StartCoroutine("Tempo");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator Tempo()
    {
        yield return new WaitForSeconds(tempo);
        tutorial.SetActive(false);
    }
}
