using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
public class EmitLight : MonoBehaviour
{
     private Comandos comandos;
    public GameObject Luz, hand, Player;
    public GameObject[] Cams, Pilares;
    public Transform LuzActualPos;
    public float scaleFactor; // Fator de escala
    public float maxScaleZ; // Limite máximo de escala em Y
    public Lente lente;
    bool lightOff;

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
        if (Physics.Raycast(hand.transform.position, hand.transform.forward, out hit, 4f))
        {
            if (hit.collider.tag == "Emit" && comandos.Actions.Interact.triggered && lightOff == true)
            {
            Player.GetComponent<PlayerController>().enabled = false;
           if(Cams[0] != null && Cams[1] != null)
            {
              //  Cams[0].SetActive(false);
                Cams[1].SetActive(true);
                Cams[0].GetComponent<CinemachineFreeLook>().Priority = 1;
                Cams[1].GetComponent<CinemachineVirtualCamera>().Priority = 10;
            }

            Luz.SetActive(true);
            scaleFactor = 0.005f;
            }
           
        }

        if(Luz.activeSelf)
        {
            lightOff = false;
            //this.gameObject.tag = "Untagged";
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

            if(Luz.transform.localScale.z >= 0.9f)
            {
                StartCoroutine(Delay());
            }
            for(int i = 0; i < Pilares.Length; i++)
            {
                Pilares[i].tag = "Untagged";
            }
           
        }

        if(Luz.activeSelf == false)
        {   
            lightOff = true;
           // this.gameObject.tag = "Emit";
            Vector3 newScale = new Vector3(0.5f, 0.5f, 0.01f);
            Luz.transform.localScale = newScale;
            Luz.transform.position = LuzActualPos.position;

            Player.GetComponent<PlayerController>().enabled = true;
            if(Cams[0] != null && Cams[1] != null)
            {
              //  Cams[0].SetActive(true);
                Cams[1].SetActive(false);
                Cams[0].GetComponent<CinemachineFreeLook>().Priority = 10;
                Cams[1].GetComponent<CinemachineVirtualCamera>().Priority = 1;
            }

        //    if(lente.ativo == true)
        //    {
        //      for(int i = 0; i < Pilares.Length; i++)
        //         {
        //         if(Pilares[i].gameObject.name.Contains("Pilar Verde"))
        //         {
        //             Pilares[i].gameObject.tag = "Rotate";
        //         }
        //         if(Pilares[i].gameObject.name.Contains("Pilar Azul - Linear"))
        //         {
        //             Pilares[i].gameObject.tag = "Launch";
        //         }
        //         }
        //    }
           
        }
    }
    
    private  IEnumerator  Delay()
    {
        yield return new WaitForSeconds(4f);
        Luz.SetActive(false);
    }
}
