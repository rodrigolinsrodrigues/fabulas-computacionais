using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Play : MonoBehaviour
{
    // Codigo responsavel por todo o controle de execução da partitura, além de controlar parte do sistema de repetição, atrelado a um objeto Reprodutor
    public float playSpeed; // velocidade de reprodução
    public bool tocar = false, Partitura1, Partitura2, delete, fechar; // bools de tocar e identificar se o final da reprodução é na partitura 1 ou 2
    public GameObject noteObj, RefPonto; // Refeencias das notas e pontos
    public Transform Ponto0,Ponto1,Ponto2,Ponto3,Ponto4,Ponto5Pt1,Ponto5Pt2; // atrbuição dos pontos
    public GameObject botaoPlay, deletar, fundoDel, Nota1; 
    public GameObject[] ContadorRepeticao; // atribuição do botão de Play e do gameObjct contador que guarda a quantiade de repetições
    public GameObject fundoContador;
    public GameObject  ponteiro, repeticao, pontoRepet;
    public int qtdRepete = 0;

    //teste de loop
    public List<GameObject> lista = new List<GameObject>(); // lista de botoes clicados
    //public GameObject ultimo, primeiro;
    public GameObject[] pontos_;
    // Cor
    public Color original;

    //Checar Partitura
    public  checarPartitura CP;
     public int x,y;
    public GameObject[] Repet;

    public Sprite spriteOriginal, spriteNovo, x2,x3,x4;
    void Start()
    {
        if(Nota1) // Ao iniciar o codigo procura pelo botão Play e o pre seleciona para que o player possa navegar entre os botões
        EventSystem.current.SetSelectedGameObject(Nota1);

    }
    public void Update()
    {
        if(tocar == true)
        {
            //Bools reposnsaveis pelo controle do reprodutor, deve ser trocada no inspector caso sejam comportados mais que uma partitura, fazendo assim o reprodutor mudar o destino final.
            if(Partitura1 == true){Ponto5Pt1.tag = "Ponto5"; Ponto5Pt2.tag = "Untagged"; Pontos(Ponto5Pt1);}
            if(Partitura2 == true){Ponto5Pt1.tag = "Untagged"; Ponto5Pt2.tag = "Ponto5"; Pontos(Ponto5Pt2);}

            
        } 

        //loop
        if (lista.Count > 2) // se tiver mais que dois botoes
        {
        lista[0].transform.GetChild(1).gameObject.SetActive(false);
        lista[0].gameObject.GetComponent<Image>().sprite = spriteOriginal;
        lista.RemoveAt(0); // remove primeiro item da lista
        }

        if (delete == true) // se tiver mais que dois botoes
        {

        qtdRepete = 0;
        lista[0].transform.GetChild(1).gameObject.SetActive(false);
        lista[0].gameObject.GetComponent<Image>().sprite = spriteOriginal;
        lista.RemoveAt(0); // remove primeiro item da lista

        lista[1].transform.GetChild(1).gameObject.SetActive(false);
        lista[1].gameObject.GetComponent<Image>().sprite = spriteOriginal;
        lista.RemoveAt(1); // remove primeiro item da lista
        
        }
        MetodoDePlayMusica();
       
    }

     void OnTriggerEnter2D(Collider2D other) // Quando esse objeto entra em contato com as notas os sons são reproduzidos
    {
       
        if(other.gameObject.tag == "Notas")
        {
            string name = other.gameObject.name;
            noteObj = GameObject.Find(name);
            noteObj.GetComponent<AudioSource>().Play();
           // Debug.Log(noteObj.GetComponent<AudioSource>().clip.name);
        } 
        
        if(other.gameObject.tag == "Ponto5") // Ao colidir com esse ponto o reprodutor para de tocar
        {
            string name = other.gameObject.name;
            RefPonto = GameObject.Find(name);
            tocar = false; 
            if(tocar == false)
            {
                StartCoroutine("Delay");    
            } 
        }
        

    }

    public void Tocar() // Atrelado ao botão Play, faz com que a linha do reprodutor volte ao inicio e comece a execução
    {
        transform.position = Ponto0.position;
        tocar = true;
        CP.part1 = false;
        CP.part1repet = false;
        CP.part2 = false;
        CP.part2repet = false;
    }
    public void Pontos(Transform Ponto_) // Controlador de Pontos, os pontos nada mais são que os exatos pontos de intervalo das musicas, cada valor do ponto corresponde a seu numero em segundos.
    {
        if(transform.position.x != Ponto_.position.x)
        {
            transform.Translate(Vector3.right * Time.deltaTime * playSpeed);
        } 
    }

    void OnTriggerExit2D (Collider2D col) // Verifica as repetiçoes
    {
    if (col.gameObject.name == repeticao.gameObject.name) // checa se o ponteiro colidiu com o primeiro ponto de repetição
    {
    if (qtdRepete > 0) // se a quantidade de repetições for maior que 0
    {
        ponteiro.transform.position = pontoRepet.transform.position; // checa se o ponteiro colidiu com o ultimo ponto de reptição
        qtdRepete--; // dimniniu a quantidade de repetições
    }
    }
    }

    void MetodoDePlayMusica() // controla o esquema de repetições
    {
        // X e Y se tratam dos pontos de loop
        x = int.Parse(lista[0].gameObject.name);
        y = int.Parse(lista[1].gameObject.name);
        

    if(lista.Count == 2)
    {
        if(tocar == false)
        {
            for(int i = 0; i < ContadorRepeticao.Length; i++)
            {
            ContadorRepeticao[i].GetComponent<Button>().interactable = true;
            ContadorRepeticao[i].GetComponent<Image>().enabled = true;
            }
            fundoContador.SetActive(true);

            deletar.GetComponent<Button>().interactable = true;
            deletar.GetComponent<Image>().enabled = true;
            fundoDel.SetActive(true);

        }
       
        if (x > y) // vai separar os botoes entre ultimo e primeiro
        {
           
            repeticao = pontos_[x]; // Ultimo
            pontoRepet = pontos_[y]; // Primeiro
          
           if(qtdRepete == 0)
           {
            lista[0].gameObject.GetComponent<Image>().sprite = spriteNovo;
            lista[1].gameObject.GetComponent<Image>().sprite = spriteNovo;
           
           }
            if(qtdRepete == 1)
           {
            lista[0].gameObject.GetComponent<Image>().sprite = x2;
            lista[1].gameObject.GetComponent<Image>().sprite = x2;

            for(int i = 0; i < ContadorRepeticao.Length; i++)
            {
            ContadorRepeticao[i].GetComponent<Button>().interactable = false;
            ContadorRepeticao[i].GetComponent<Image>().enabled = false;
            }
            fundoContador.SetActive(false);
           }
            if(qtdRepete == 2)
           {
            lista[0].gameObject.GetComponent<Image>().sprite = x3;
            lista[1].gameObject.GetComponent<Image>().sprite = x3;

            for(int i = 0; i < ContadorRepeticao.Length; i++)
            {
            ContadorRepeticao[i].GetComponent<Button>().interactable = false;
            ContadorRepeticao[i].GetComponent<Image>().enabled = false;
            }
            fundoContador.SetActive(false);

           }
            if(qtdRepete == 3)
           {
            lista[0].gameObject.GetComponent<Image>().sprite = x4;
            lista[1].gameObject.GetComponent<Image>().sprite = x4;

            for(int i = 0; i < ContadorRepeticao.Length; i++)
            {
            ContadorRepeticao[i].GetComponent<Button>().interactable = false;
            ContadorRepeticao[i].GetComponent<Image>().enabled = false;
            }
            fundoContador.SetActive(false);
           }

            
        }
        else
        {
            repeticao = pontos_[y];
            pontoRepet = pontos_[x];
         
            if(qtdRepete == 0)
           {
            lista[0].gameObject.GetComponent<Image>().sprite = spriteNovo;
            lista[1].gameObject.GetComponent<Image>().sprite = spriteNovo;
           }
            if(qtdRepete == 1)
           {
            lista[0].gameObject.GetComponent<Image>().sprite = x2;
            lista[1].gameObject.GetComponent<Image>().sprite = x2;

            for(int i = 0; i < ContadorRepeticao.Length; i++)
            {
            ContadorRepeticao[i].GetComponent<Button>().interactable = false;
            ContadorRepeticao[i].GetComponent<Image>().enabled = false;
            }
            fundoContador.SetActive(false);

           }
            if(qtdRepete == 2)
           {
            lista[0].gameObject.GetComponent<Image>().sprite = x3;
            lista[1].gameObject.GetComponent<Image>().sprite = x3;

            for(int i = 0; i < ContadorRepeticao.Length; i++)
            {
           ContadorRepeticao[i].GetComponent<Button>().interactable = false;
            ContadorRepeticao[i].GetComponent<Image>().enabled = false;
            }
            fundoContador.SetActive(false);

           }
            if(qtdRepete == 3)
           {
            lista[0].gameObject.GetComponent<Image>().sprite = x4;
            lista[1].gameObject.GetComponent<Image>().sprite = x4;

            for(int i = 0; i < ContadorRepeticao.Length; i++)
            {
            ContadorRepeticao[i].GetComponent<Button>().interactable = false;
            ContadorRepeticao[i].GetComponent<Image>().enabled = false;
            }
            fundoContador.SetActive(false);
           }
        
        }
       
        for(int x = 0; x < Repet.Length; x++)
        {
            Repet[x].GetComponent<Button>().interactable = false;
        }
    }
    
    // antes de começar o ponteiro 
    }

    // Voids atribuidos aos botões que adicionam repetições
    public void X2(){qtdRepete = 1;}
    public void X3(){qtdRepete = 2;}
    public void X4(){qtdRepete = 3;}

   IEnumerator Delay()
    {
        yield return new WaitForSeconds(1);
        CP.check = true;
        yield return new WaitForSeconds(2);
        CP.part1 = false;
        CP.part1repet = false;
        CP.part2 = false;
        CP.part2repet = false;
    } 

   
}

