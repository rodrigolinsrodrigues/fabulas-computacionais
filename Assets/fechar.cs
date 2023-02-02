using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
public class fechar : MonoBehaviour
{
    public bool podeFechar;
   public GameObject fechar_;
    // Update is called once per frame

     void Start()
    {
        podeFechar = false;
    }
    void Update()
    {
        if(fechar_.activeSelf == false && podeFechar == false)
        {
            EventSystem.current.SetSelectedGameObject(this.gameObject);
            podeFechar = true;
        }
    }
}
