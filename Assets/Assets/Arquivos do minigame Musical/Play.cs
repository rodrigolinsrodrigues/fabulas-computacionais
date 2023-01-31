using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Play : MonoBehaviour
{
    public float playSpeed;
    public bool tocar = false;
    public GameObject noteObj;
    public Transform Ponto0,Ponto1,Ponto2,Ponto3,Ponto4;

    public void Update()
    {
    
        if(tocar == true)
        {
             Pontos(Ponto4);
        }
       
                //transform.position = new Vector3(playSpeed * Time.deltaTime, transform.position.y, transform.position.z);
                //transform.position = Ponto4.position * (playSpeed * transform.right * Time.deltaTime);
                //transform.position = Vector3.Lerp(transform.position, Ponto4.position, playSpeed * Time.deltaTime);
                //transform.Translate(Vector3.right * Time.deltaTime * playSpeed);
                // if(Vector3.Distance(Ponto4.position, transform.position) > 0.1f)
                // {
                // transform.Translate(Vector3.right * Time.deltaTime * playSpeed);
                // } 
               

            
    }

     void OnTriggerEnter2D(Collider2D other)
    {
       if(other.GetComponent<CircleCollider2D>().tag == "Notas")
       {
            string name = other.GetComponent<CircleCollider2D>().name;
            noteObj = GameObject.Find(name);
            noteObj.GetComponent<AudioSource>().Play();
       } 
    }

    public void Tocar(){tocar = true;}
    public void Pontos(Transform Ponto_)
    {
        if(transform.position.x != Ponto_.position.x)
        {
        transform.Translate(Vector3.right * Time.deltaTime * playSpeed);
        }
    }

    
}
