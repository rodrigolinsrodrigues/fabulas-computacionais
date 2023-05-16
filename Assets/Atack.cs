using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Atack : MonoBehaviour
{
    private Comandos comandos;
     // Lançar objetos
    public GameObject frontObj, hand;
    public float viewDistance;
    
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
        if (Physics.Raycast(hand.transform.position, hand.transform.forward, out hit, viewDistance))
        {
        if (hit.collider.tag == "Break" && comandos.Actions.Atack.triggered)
            {
                // Adicione uma forca a direcao em que o personagem esta olhando
                string name = hit.collider.name;
                frontObj = GameObject.Find(name);
                Destroy(frontObj);
            }
           
        }
    }
}
