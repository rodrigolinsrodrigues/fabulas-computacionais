using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MudarSpriteNota : MonoBehaviour
{
    public Sprite[] Sons;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(this.gameObject.GetComponent<AudioSource>().clip.name.Contains("x1"))
        {
           this.gameObject.GetComponent<Image>().sprite = Sons[0];
        }
        if(this.gameObject.GetComponent<AudioSource>().clip.name.Contains("x2"))
        {
           this.gameObject.GetComponent<Image>().sprite = Sons[1];
        }
        if(this.gameObject.GetComponent<AudioSource>().clip.name.Contains("x3"))
        {
           this.gameObject.GetComponent<Image>().sprite = Sons[2];
        }
        if(this.gameObject.GetComponent<AudioSource>().clip.name.Contains("x4"))
        {
           this.gameObject.GetComponent<Image>().sprite = Sons[3];
        }
        


    }
}
