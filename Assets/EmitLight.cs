using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmitLight : MonoBehaviour
{
     private Comandos comandos;
    public GameObject Luz, hand, Player;
    public GameObject[] Cams;
    public Transform LuzActualPos;
    public float scaleFactor = 0.1f; // Fator de escala
    public float maxScaleZ = 5f; // Limite máximo de escala em Y
    

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
            Player.GetComponent<PlayerController>().enabled = false;
            Cams[0].SetActive(false);
            Cams[1].SetActive(true);
            Luz.SetActive(true);
            scaleFactor = 0.008f;
            }
        }

        if(Luz.activeSelf)
        {
            // Obtenha a escala atual do objeto
            Vector3 currentScale = Luz.transform.localScale;

            // Calcule a escala desejada na direção Y
            float newScaleZ = currentScale.z + scaleFactor;

            // Verifique se a nova escala excede o limite máximo em Y
            if (newScaleZ <= maxScaleZ)
            {
                // Aplique a nova escala apenas na direção Y
                Vector3 newScale = new Vector3(currentScale.x, currentScale.y, newScaleZ);
                Luz.transform.localScale = newScale;
                
            }

        }

        if(Luz.activeSelf == false)
        {   
            Vector3 newScale = new Vector3(0.5f, 0.5f, 0.01f);
            Luz.transform.localScale = newScale;
            Luz.transform.position = LuzActualPos.position;
        }
    }
   
}
