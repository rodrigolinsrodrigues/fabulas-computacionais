using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MudarSpriteNota : MonoBehaviour
{
   //Script utilizado para trocar os sprites das notas de acordo com a faixa de audio escolhida
    public Sprite[] Sons;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
      if(this.gameObject.GetComponent<AudioSource>().clip != null)
      {
         if(this.gameObject.GetComponent<AudioSource>().clip.name.Contains("Violao_agudo_x1"))
        {
           this.gameObject.GetComponent<Image>().sprite = Sons[0];
        }
         if(this.gameObject.GetComponent<AudioSource>().clip.name.Contains("Handpan_agudo_x1"))
        {
           this.gameObject.GetComponent<Image>().sprite = Sons[1];
        }
         if(this.gameObject.GetComponent<AudioSource>().clip.name.Contains("Violao_medio_x1"))
        {
           this.gameObject.GetComponent<Image>().sprite = Sons[0];
        }
         if(this.gameObject.GetComponent<AudioSource>().clip.name.Contains("Handpan_medio_x1"))
        {
           this.gameObject.GetComponent<Image>().sprite = Sons[1];
        }
         if(this.gameObject.GetComponent<AudioSource>().clip.name.Contains("Violao_grave_x1"))
        {
           this.gameObject.GetComponent<Image>().sprite = Sons[0];
        }
         if(this.gameObject.GetComponent<AudioSource>().clip.name.Contains("Handpan_grave_x1"))
        {
           this.gameObject.GetComponent<Image>().sprite = Sons[1];
        }
         // Flauta
         if(this.gameObject.GetComponent<AudioSource>().clip.name.Contains("Flauta_agudo_x1"))
        {
           this.gameObject.GetComponent<Image>().sprite = Sons[2];
        }
         if(this.gameObject.GetComponent<AudioSource>().clip.name.Contains("Flauta_medio_x1"))
        {
           this.gameObject.GetComponent<Image>().sprite = Sons[2];
        }
         if(this.gameObject.GetComponent<AudioSource>().clip.name.Contains("Flauta_grave_x1"))
        {
           this.gameObject.GetComponent<Image>().sprite = Sons[2];
        }


      }
        
    }
}
