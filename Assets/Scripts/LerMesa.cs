using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LerMesa : MonoBehaviour
{
    public GameObject[] Mesas;
    public GameObject Pao1,Obj2,Obj3,Obj4,Pao2;
    public Transform[] LocalPLayer;
    public GameObject[] Cams;
    public bool comecar, parar;
    public GameObject Player;
    public bool PCortado,PQueimado,QCortado,QQueimado,CCortada,CQueimada,OCortado,OFrito;
    public GameObject LR; // Acessa o banco de modelos de insumos
    public LeituraResultado LeituraFinal;
    void Update()
    {
        if(parar == true)
        {
        LeituraFinal.ChecarResultado();
        Player.GetComponent<PlayerController>().animator.SetBool("Cortar", false);
        Player.GetComponent<PlayerController>().animator.SetBool("Fritar", false);
        comecar = false; 
        Player.GetComponent<PlayerController>().enabled = true;
        Player.GetComponent<PickUp>().enabled = true;
        Cams[0].SetActive(true);
        Cams[1].SetActive(false);
        parar = false;
        PCortado = false;
        PQueimado = false;
        QCortado = false;
        QQueimado = false;
        CCortada = false;
        CQueimada = false;
        OCortado = false;
        OFrito = false;
        }
    }
    public void iniciar()
    {
        if(comecar == true && parar == false)
        {
        StartCoroutine("TempoDeEspera");
        Player.GetComponent<PlayerController>().enabled = false;
        Player.GetComponent<PickUp>().enabled = false;
        Cams[0].SetActive(false);
        Cams[1].SetActive(true);
        }
      
    }
    public IEnumerator TempoDeEspera()
    {
           
    for(int i = 0; i < Mesas.Length; i++)
        {
               
            if(Mesas[i].transform.childCount < 3)
                {
                    Debug.Log("Parar");
                    parar = true;
                    comecar = false;
                    break;
                }
                else
                {
                    Player.transform.position = LocalPLayer[i].position;
                    // Insumos
                    switch(Mesas[i].transform.GetChild(2).gameObject.layer)
                    {
                        
                        //Pão
                        case 10:
                        Debug.Log(Mesas[i] + "É Pão");
                        break;
                        //Queijo
                        case 8:
                        Debug.Log(Mesas[i] + "É Ovo");
                        break;
                        //Ovo
                        case 7:
                        Debug.Log(Mesas[i] + "É Queijo");
                        break;
                        
                    }
                    if(Mesas[i].transform.childCount > 3 && Mesas[i].transform.GetChild(3)!= null)
                    {
                        switch(Mesas[i].transform.GetChild(3).gameObject.layer)
                        {
                        case 12:
                        Debug.Log(Mesas[i] + "Cortar");
                        Player.GetComponent<PlayerController>().animator.SetBool("Fritar", false);
                        Player.GetComponent<PlayerController>().animator.SetBool("Cortar", true);
                        break;

                        case 13:
                        Debug.Log(Mesas[i] + "Fritar");
                        Player.GetComponent<PlayerController>().animator.SetBool("Fritar", true);
                        Player.GetComponent<PlayerController>().animator.SetBool("Cortar", false);
                        break;
                        } 
                    }
                    yield return new WaitForSeconds(5);
                
                //Checar combinações
                if(Mesas[i].transform.childCount > 3)
                {
                    //Insumos Cortados
                        //Pão Cortado
                    if(Mesas[i].transform.GetChild(2).name.Contains("Pao") && Mesas[i].transform.GetChild(3).name.Contains("Cortar"))
                    {
                    Debug.Log("Pão Cortado");
                    Mesas[i].transform.GetChild(2).gameObject.GetComponent<MeshFilter>().mesh = LR.GetComponent<ListaDeRecursos>().Pao[0].gameObject.GetComponent<MeshFilter>().sharedMesh;
                    Mesas[i].transform.GetChild(2).gameObject.GetComponent<MeshRenderer>().materials = LR.GetComponent<ListaDeRecursos>().Pao[0].gameObject.GetComponent<MeshRenderer>().sharedMaterials;
                    Pao1.GetComponent<MeshFilter>().mesh = LR.GetComponent<ListaDeRecursos>().Pao[0].gameObject.GetComponent<MeshFilter>().sharedMesh;
                    Pao1.GetComponent<MeshRenderer>().materials = LR.GetComponent<ListaDeRecursos>().Pao[0].gameObject.GetComponent<MeshRenderer>().sharedMaterials;
                    Pao1.gameObject.name = LR.GetComponent<ListaDeRecursos>().Pao[0].gameObject.name;
                    PCortado = true;
                    }
                       //Pão Queimado
                    else if(Mesas[i].transform.GetChild(2).name.Contains("Pao") && Mesas[i].transform.GetChild(3).name.Contains("Fritar"))
                    {
                    Debug.Log("Pão Queimado");
                    Mesas[i].transform.GetChild(2).gameObject.GetComponent<MeshFilter>().mesh = LR.GetComponent<ListaDeRecursos>().Pao[2].gameObject.GetComponent<MeshFilter>().sharedMesh;
                    Mesas[i].transform.GetChild(2).gameObject.GetComponent<MeshRenderer>().materials = LR.GetComponent<ListaDeRecursos>().Pao[2].gameObject.GetComponent<MeshRenderer>().sharedMaterials;
                    Pao1.GetComponent<MeshFilter>().mesh = LR.GetComponent<ListaDeRecursos>().Pao[2].gameObject.GetComponent<MeshFilter>().sharedMesh;
                    Pao1.GetComponent<MeshRenderer>().materials = LR.GetComponent<ListaDeRecursos>().Pao[2].gameObject.GetComponent<MeshRenderer>().sharedMaterials;
                    Pao1.gameObject.name =  LR.GetComponent<ListaDeRecursos>().Pao[2].gameObject.name;
                    PQueimado = true;
                    }
                        //Queijo Cortado
                    if(Mesas[i].transform.GetChild(2).name.Contains("Queijo") && Mesas[i].transform.GetChild(3).name.Contains("Cortar"))
                    {
                    Debug.Log("Queijo Cortado");
                    Mesas[i].transform.GetChild(2).gameObject.GetComponent<MeshFilter>().mesh = LR.GetComponent<ListaDeRecursos>().Queijo[0].gameObject.GetComponent<MeshFilter>().sharedMesh;
                    Mesas[i].transform.GetChild(2).gameObject.GetComponent<MeshRenderer>().materials = LR.GetComponent<ListaDeRecursos>().Queijo[0].gameObject.GetComponent<MeshRenderer>().sharedMaterials;
                    Obj2.GetComponent<MeshFilter>().mesh = LR.GetComponent<ListaDeRecursos>().Queijo[0].gameObject.GetComponent<MeshFilter>().sharedMesh;
                    Obj2.GetComponent<MeshRenderer>().materials = LR.GetComponent<ListaDeRecursos>().Queijo[0].gameObject.GetComponent<MeshRenderer>().sharedMaterials;
                    Obj2.gameObject.name = LR.GetComponent<ListaDeRecursos>().Queijo[0].gameObject.name;
                    QCortado = true;
                    }
                        //Queijo Queimado
                    else if(Mesas[i].transform.GetChild(2).name.Contains("Queijo") && Mesas[i].transform.GetChild(3).name.Contains("Fritar"))
                    {
                    Debug.Log("Queijo Queimado");
                    Mesas[i].transform.GetChild(2).gameObject.GetComponent<MeshFilter>().mesh = LR.GetComponent<ListaDeRecursos>().Queijo[1].gameObject.GetComponent<MeshFilter>().sharedMesh;
                    Mesas[i].transform.GetChild(2).gameObject.GetComponent<MeshRenderer>().materials = LR.GetComponent<ListaDeRecursos>().Queijo[1].gameObject.GetComponent<MeshRenderer>().sharedMaterials;
                    Obj2.GetComponent<MeshFilter>().mesh = LR.GetComponent<ListaDeRecursos>().Queijo[1].gameObject.GetComponent<MeshFilter>().sharedMesh;
                    Obj2.GetComponent<MeshRenderer>().materials = LR.GetComponent<ListaDeRecursos>().Queijo[1].gameObject.GetComponent<MeshRenderer>().sharedMaterials;
                    Obj2.gameObject.name =  LR.GetComponent<ListaDeRecursos>().Queijo[1].gameObject.name;
                    QCortado = true;
                    }
                        // Ovo Frito
                    if(Mesas[i].transform.GetChild(2).name.Contains("Ovo") && Mesas[i].transform.GetChild(3).name.Contains("Fritar"))
                    {
                    Debug.Log("Ovo Frito");
                    Mesas[i].transform.GetChild(2).gameObject.GetComponent<MeshFilter>().mesh = LR.GetComponent<ListaDeRecursos>().Ovo[0].gameObject.GetComponent<MeshFilter>().sharedMesh;
                    Mesas[i].transform.GetChild(2).gameObject.GetComponent<MeshRenderer>().materials = LR.GetComponent<ListaDeRecursos>().Ovo[0].gameObject.GetComponent<MeshRenderer>().sharedMaterials;
                    Obj3.GetComponent<MeshFilter>().mesh = LR.GetComponent<ListaDeRecursos>().Ovo[0].gameObject.GetComponent<MeshFilter>().sharedMesh;
                    Obj3.GetComponent<MeshRenderer>().materials = LR.GetComponent<ListaDeRecursos>().Ovo[0].gameObject.GetComponent<MeshRenderer>().sharedMaterials;
                    Obj3.gameObject.name = LR.GetComponent<ListaDeRecursos>().Ovo[0].gameObject.name;
                    OFrito = true;
                    }
                        // Ovo Cortado
                    else if(Mesas[i].transform.GetChild(2).name.Contains("Ovo") && Mesas[i].transform.GetChild(3).name.Contains("Cortar"))
                    {
                    Debug.Log("Ovo Cortado");
                    Mesas[i].transform.GetChild(2).gameObject.GetComponent<MeshFilter>().mesh = LR.GetComponent<ListaDeRecursos>().Ovo[1].gameObject.GetComponent<MeshFilter>().sharedMesh;
                    Mesas[i].transform.GetChild(2).gameObject.GetComponent<MeshRenderer>().materials = LR.GetComponent<ListaDeRecursos>().Ovo[1].gameObject.GetComponent<MeshRenderer>().sharedMaterials;
                    Obj3.GetComponent<MeshFilter>().mesh = LR.GetComponent<ListaDeRecursos>().Ovo[1].gameObject.GetComponent<MeshFilter>().sharedMesh;
                    Obj3.GetComponent<MeshRenderer>().materials = LR.GetComponent<ListaDeRecursos>().Ovo[1].gameObject.GetComponent<MeshRenderer>().sharedMaterials;
                    Obj3.gameObject.name = LR.GetComponent<ListaDeRecursos>().Ovo[1].gameObject.name;
                    OCortado = true;
                    }
                    
                        //Cebola Cortada
                    if(Mesas[i].transform.GetChild(2).name.Contains("Cebola") && Mesas[i].transform.GetChild(3).name.Contains("Cortar"))
                    {
                    Debug.Log("Cebola Cortada");
                    Mesas[i].transform.GetChild(2).gameObject.GetComponent<MeshFilter>().mesh = LR.GetComponent<ListaDeRecursos>().Cebola[0].gameObject.GetComponent<MeshFilter>().sharedMesh;
                    Mesas[i].transform.GetChild(2).gameObject.GetComponent<MeshRenderer>().materials = LR.GetComponent<ListaDeRecursos>().Cebola[0].gameObject.GetComponent<MeshRenderer>().sharedMaterials;
                    Obj4.GetComponent<MeshFilter>().mesh = LR.GetComponent<ListaDeRecursos>().Cebola[0].gameObject.GetComponent<MeshFilter>().sharedMesh;
                    Obj4.GetComponent<MeshRenderer>().materials = LR.GetComponent<ListaDeRecursos>().Cebola[0].gameObject.GetComponent<MeshRenderer>().sharedMaterials;
                    Obj4.gameObject.name = LR.GetComponent<ListaDeRecursos>().Cebola[0].gameObject.name;
                    CCortada = true;
                    }
                        //Cebola Frita
                    else if(Mesas[i].transform.GetChild(2).name.Contains("Cebola") && Mesas[i].transform.GetChild(3).name.Contains("Fritar"))
                    {
                    Debug.Log("Cebola Frita");
                    Mesas[i].transform.GetChild(2).gameObject.GetComponent<MeshFilter>().mesh = LR.GetComponent<ListaDeRecursos>().Cebola[1].gameObject.GetComponent<MeshFilter>().sharedMesh;
                    Mesas[i].transform.GetChild(2).gameObject.GetComponent<MeshRenderer>().materials = LR.GetComponent<ListaDeRecursos>().Cebola[1].gameObject.GetComponent<MeshRenderer>().sharedMaterials;
                    Obj4.GetComponent<MeshFilter>().mesh = LR.GetComponent<ListaDeRecursos>().Cebola[1].gameObject.GetComponent<MeshFilter>().sharedMesh;
                    Obj4.GetComponent<MeshRenderer>().materials = LR.GetComponent<ListaDeRecursos>().Cebola[1].gameObject.GetComponent<MeshRenderer>().sharedMaterials;
                    Obj4.gameObject.name = LR.GetComponent<ListaDeRecursos>().Cebola[1].gameObject.name;
                    CQueimada = true;
                    }   
                }  
            }
            Debug.Log("Parar");
            if(PCortado == true)
            {
                Pao2.GetComponent<MeshFilter>().mesh = LR.GetComponent<ListaDeRecursos>().Pao[0].gameObject.GetComponent<MeshFilter>().sharedMesh;
                Pao2.GetComponent<MeshRenderer>().materials = LR.GetComponent<ListaDeRecursos>().Pao[0].gameObject.GetComponent<MeshRenderer>().sharedMaterials;
                Pao2.gameObject.name =  LR.GetComponent<ListaDeRecursos>().Pao[0].gameObject.name;
            }
            else if(PQueimado == true)
            {
                Pao2.GetComponent<MeshFilter>().mesh = LR.GetComponent<ListaDeRecursos>().Pao[2].gameObject.GetComponent<MeshFilter>().sharedMesh;
                Pao2.GetComponent<MeshRenderer>().materials = LR.GetComponent<ListaDeRecursos>().Pao[2].gameObject.GetComponent<MeshRenderer>().sharedMaterials;
                Pao2.gameObject.name =  LR.GetComponent<ListaDeRecursos>().Pao[2].gameObject.name;
            }
            //parar = true;
           
            
    }
    
}
}

               