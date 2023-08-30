using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DescLuz : MonoBehaviour
{
    public GameObject Luz;
    public GameObject text;
    public GameObject text2;
    public GameObject text3;
    public string Des;
    public bool Cristal;
    public bool PodeMudar;
    public Material material;
    // Update is called once per frame
    void Update()
    {
        if (Cristal == false)
        {
            if (Luz.activeSelf) { if (text != null) { text.GetComponent<TextMeshPro>().text = Des; } }

        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (Cristal == true)
        {
            if (other.GetComponent<Collider>().tag == "Light")
            {
                if (text.GetComponent<TextMeshPro>().text == "") { text.GetComponent<TextMeshPro>().text = Des; }
                else if (text.GetComponent<TextMeshPro>().text != "") { text2.GetComponent<TextMeshPro>().text = Des; }
                else if (text.GetComponent<TextMeshPro>().text != "" && text2.GetComponent<TextMeshPro>().text != "") { text3.GetComponent<TextMeshPro>().text = Des; }
                material.EnableKeyword("_EMISSION");
            }
        }
        if (PodeMudar == true)
        {
            if (other.GetComponent<Collider>().tag == "Light")
            {
                if (text.GetComponent<TextMeshPro>().text == "") { text.GetComponent<TextMeshPro>().text = Des; }
                else if (text.GetComponent<TextMeshPro>().text != "") { text2.GetComponent<TextMeshPro>().text = Des; }
                // else if(text2.GetComponent<TextMeshPro>().text != ""){text3.GetComponent<TextMeshPro>().text = Des;}
            }
        }
    }
}
