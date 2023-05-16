using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmitLight : MonoBehaviour
{
     private Comandos comandos;
    public GameObject Luz, hand;
    public Transform LuzActualPos;
    public float scaleFactor = 0.1f; // Fator de escala
    public float maxScaleY = 5f; // Limite máximo de escala em Y

    // Start is called before the first frame update
    void Start()
    {
        comandos = new Comandos();
        comandos.Actions.Enable(); 
        LuzActualPos.position = Luz.transform.position;
    }
    void Update()
    {
        RaycastHit hit;
        if (Physics.Raycast(hand.transform.position, hand.transform.forward, out hit, 2f))
        {
            if (hit.collider.tag == "Emit" && comandos.Actions.Interact.triggered)
            {
            Luz.SetActive(true);
            }
        }

        if(Luz.active)
        {
            // Obtenha a escala atual do objeto
            Vector3 currentScale = Luz.transform.localScale;

            // Calcule a escala desejada na direção Y
            float newScaleY = currentScale.y + scaleFactor;

            // Verifique se a nova escala excede o limite máximo em Y
            if (newScaleY <= maxScaleY)
            {
                // Aplique a nova escala apenas na direção Y
                Vector3 newScale = new Vector3(currentScale.x, newScaleY, currentScale.z);
                Luz.transform.localScale = newScale;
                
            }

        }

        if(Luz.active == false)
        {   
            Vector3 newScale = new Vector3(0.3f, 0.1f, 0.3f);
            Luz.transform.localScale = newScale;
            Luz.transform.position = LuzActualPos.position;
        }
    }
   
}
