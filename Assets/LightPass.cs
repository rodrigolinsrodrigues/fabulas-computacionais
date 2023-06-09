using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightPass : MonoBehaviour
{
    public GameObject ThisLight;
    public Transform ThisLightPos, ThisLightScale;
    public float maxScaleY,scaleFactor;
    public bool ScaleY, ScaleX;
    
    void Start()
    {
        ThisLightScale.localScale = ThisLight.transform.localScale;
        ThisLightPos.position = ThisLight.transform.position;

    }
    void OnTriggerEnter(Collider other)
    {
        if(other.GetComponent<Collider>().tag == "Light")
        {
            Debug.Log("Pegou");
            ThisLight.SetActive(true);
            
        }
    }
     void Update()
    {
        if(ThisLight.activeSelf)
        {
            // Obtenha a escala atual do objeto
            Vector3 currentScale = ThisLight.transform.localScale;

            // Calcule a escala desejada na direção Y
            float newScaleZ = currentScale.z + scaleFactor;

            // Verifique se a nova escala excede o limite máximo em Y
            if (newScaleZ <= maxScaleY)
            {
                // Aplique a nova escala apenas na direção Y
                Vector3 newScale = new Vector3(currentScale.x, currentScale.y, newScaleZ);
                ThisLight.transform.localScale = newScale;
                
            }

            
            if(ThisLight.transform.localScale.z >= maxScaleY)
            {
                StartCoroutine(Delay());
            }
        }

         if(ThisLight.activeSelf == false)
        {
            ThisLight.transform.localScale = ThisLightScale.localScale;
            ThisLight.transform.position = ThisLightPos.position;
        }
    }

     private  IEnumerator  Delay()
    {
        yield return new WaitForSeconds(4f);
        ThisLight.SetActive(false);
    }

}


      /*  if(ScaleY == true)
        {
        if(ThisLight.active)
        {
            // Obtenha a escala atual do objeto
            Vector3 currentScale = ThisLight.transform.localScale;

            // Calcule a escala desejada na direção Y
            float newScaleY = currentScale.y + scaleFactor;

            // Verifique se a nova escala excede o limite máximo em Y
            if (newScaleY <= maxScaleY)
            {
                // Aplique a nova escala apenas na direção Y
                Vector3 newScale = new Vector3(currentScale.x, newScaleY, currentScale.z);
                ThisLight.transform.localScale = newScale;
                
            }
        }

        if(ThisLight.active == false)
        {
            ThisLight.transform.localScale = ThisLightScale.localScale;
            ThisLight.transform.position = ThisLightPos.position;
        }
        }
       
       if(ScaleX == true)
       {
         if(ThisLight.active)
        {
            // Obtenha a escala atual do objeto
            Vector3 currentScale = ThisLight.transform.localScale;

            // Calcule a escala desejada na direção Y
            float newScaleX = currentScale.x + scaleFactor;

            // Verifique se a nova escala excede o limite máximo em Y
            if (newScaleX <= maxScaleY)
            {
                // Aplique a nova escala apenas na direção Y
                Vector3 newScale = new Vector3(newScaleX,currentScale.y, currentScale.z);
                ThisLight.transform.localScale = newScale;
                
            }
        } */

       
       
   
   

