using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class pegarSprite : MonoBehaviour
{
    public GameObject nota;
    // Update is called once per frame
    void Update()
    {
        this.gameObject.GetComponent<Image>().sprite = nota.GetComponent<Image>().sprite;
    }
}
