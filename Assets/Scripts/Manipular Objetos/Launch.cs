using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Launch : MonoBehaviour
{
    //Codigo de lançar objetos, atribuido ao Player
     private Comandos comandos;

     // Lançar objetos
     public GameObject launchObj, hand;
    public float launchDistance;
    public float launchForce;
    // Start is called before the first frame update
    void Start()
    {
        comandos = new Comandos();
        comandos.Actions.Enable(); 
    }
    
    // Update is called once per frame
    void Update()
    {
        RaycastHit hit;
        if (Physics.Raycast(hand.transform.position, hand.transform.forward, out hit, launchDistance))
        {
        if (hit.collider.tag == "Launch" && comandos.Actions.Push.triggered)
            {
                // Adicione uma forca a direcao em que o personagem esta olhando
                string name = hit.collider.name;
                launchObj = GameObject.Find(name);
                launchObj.GetComponent<Rigidbody>().AddForce(transform.forward * launchForce, ForceMode.Impulse);
            }
           
        }
    }
}
