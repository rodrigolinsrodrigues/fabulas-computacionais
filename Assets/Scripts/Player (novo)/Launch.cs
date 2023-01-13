using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Launch : MonoBehaviour
{
     private Comandos comandos;

     // Lançar objetos
     public GameObject launchObj;
    public float launchDistance;
    public float launchForce = 5.0f;
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
        if (Physics.Raycast(transform.position, transform.forward, out hit, launchDistance))
        {
            if (hit.collider.tag == "Launch" && comandos.Actions.Push.ReadValue<float>() == 1)
            {
                // Adicione uma forca a direcao em que o personagem esta olhando
                string name = hit.collider.name;
                launchObj = GameObject.Find(name);
                launchObj.GetComponent<Rigidbody>().AddForce(transform.forward * launchForce, ForceMode.Impulse);
            }
        }
    }
}
