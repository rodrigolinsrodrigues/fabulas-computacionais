using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class deleteObj : MonoBehaviour
{
    public float opacidadeAlvo = 0f;
    public float tempoDelay = 8f;
    void Start()
    {
        StartCoroutine(DestruirFilhos());
    }

    System.Collections.IEnumerator DestruirFilhos()
    {
        yield return new WaitForSeconds(tempoDelay);

        foreach (Transform child in transform)
        {
            Renderer childRenderer = child.GetComponent<Renderer>();
            if (childRenderer != null)
            {
                Material childMaterial = childRenderer.material;
                Color corAlvo = childMaterial.color;
                corAlvo.a = opacidadeAlvo;
                childMaterial.color = corAlvo;
            }

            child.GetComponent<Collider>().enabled = false;
            yield return new WaitForSeconds(2f);
          
        }
        Destroy(gameObject);
    }
}
