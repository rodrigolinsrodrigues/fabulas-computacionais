using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Lente : MonoBehaviour
{
    public GameObject[] Uteis, Cams;
    private Comandos comandos;
    public GameObject hand, aura;
    public float distance, scaleFactor, maxScale;
    public Material material;
    public bool ativo = false;
    void Awake()
    {
        comandos = new Comandos();
        comandos.Actions.Enable(); 
    }
    
    private void Update()
    {
        Ray ray = new Ray(hand.transform.position, hand.transform.forward);
        RaycastHit hit;

        if(Physics.Raycast(hand.transform.position, hand.transform.forward, out hit, distance))
        {
            if(hit.collider.tag == "Lens" && comandos.Actions.Interact.triggered)
            {
                ativo = true;
                aura.SetActive(true);
                 StartCoroutine(Delay());
                Cams[0].SetActive(false);
                Cams[1].SetActive(true);
                StartCoroutine(Delay2());
                
            }
        }
       
        
        
    }
    
    IEnumerator Delay()
    {
        if(aura.activeSelf)
        {
            Vector3 currentScale = aura.transform.localScale;
            // Calcule a escala desejada na direção Y e X
            float newScaleX = currentScale.x + scaleFactor;
            float newScaleY = currentScale.y + scaleFactor;

            // Verifique se a nova escala excede o limite máximo em X ou Y
            if (newScaleX <= maxScale)
            {
                // Aplique a nova escala apenas na direção X e Y
                Vector3 newScale = new Vector3(newScaleX, newScaleY, currentScale.z);
                aura.transform.localScale = newScale;
                
            }
           
           
        }
        yield return new WaitForSeconds(2f);
        Destroy(aura);

    }
     IEnumerator Delay2()
    {
        yield return new WaitForSeconds(2f);
        for(int i = 0; i < Uteis.Length; i++)
                {
                    Uteis[i].GetComponent<Renderer>().material = material;
                    if(Uteis[i].gameObject.name.Contains("Pilar Verde"))
                    {
                        Uteis[i].gameObject.tag = "Rotate";
                    }
                    if(Uteis[i].gameObject.name.Contains("Pilar Azul - Linear"))
                    {
                        Uteis[i].gameObject.tag = "Pushable";
                    }
                }
        yield return new WaitForSeconds(2f);
        Cams[0].SetActive(true);
        Cams[1].SetActive(false);
        yield return new WaitForSeconds(2f);
        Destroy(GetComponent<Lente>());
    }
   
}
