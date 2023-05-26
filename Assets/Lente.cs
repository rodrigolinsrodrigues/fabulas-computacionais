using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lente : MonoBehaviour
{
    public GameObject[] Uteis, Inuteis;
    private Comandos comandos;
    public GameObject hand, aura;
    public float distance, scaleFactor, maxScale;
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
                aura.SetActive(true);
                for(int i = 0; i < Inuteis.Length; i++)
                {
                Inuteis[i].SetActive(false);
                Uteis[i].SetActive(true);
                }
            }
        }
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
           
            StartCoroutine(Delay());
        }
        
        
    }
    
    IEnumerator Delay()
    {
        yield return new WaitForSeconds(5f);
        Destroy(GetComponent<Lente>());
        Destroy(aura);

    }
}
