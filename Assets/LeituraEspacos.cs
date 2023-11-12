using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Cinemachine;

public class LeituraEspacos : MonoBehaviour
{   //Script novo para leitura de Espaços de snap 
    GameObject Player;
    public GameObject[] Cams;
    public GameObject[] Espacos; //Engloba todas os espaços a serem lidos, espaços podem ser de diversos tipos;
    public GameObject[] Objetos; //Arrya usado pra coletar todos os objetos de interação do puzzle e desativar as interações uma vez que o jogador acerte o puzzle;
    public GameObject PopUp; // Pega o Canvas com o popUp de conclusão
    public GameObject Redoma; // Acessa a Redoma da Joia;
    public bool Concluido = false; // Bool que determina o acerto;
    public bool Fase1, Fase2, Fase3, Fase4; //Bools que separam a verficação por cada uma das fases
    public GameObject NPC; //Controla / libera um Npc na fase caso seja necessario; 
    // public GameObject[] Animados; // Objetos animados na cena, a animação será ativada nesse script caso seja necessario

    public LevelDataManager dataManager;

    void Start()
    {
        Player = GameObject.FindWithTag("Player");

        dataManager = FindObjectOfType<LevelDataManager>();
    }
    public void Check()
    {
        if (Fase1 == true)
        {
            if (Espacos[0].transform.childCount >= 2 && Espacos[1].transform.childCount >= 2 && Espacos[2].transform.childCount >= 2 && Espacos[3].transform.childCount >= 2)
            {
                dataManager.GetAttempt();
                if (Espacos[0].transform.GetChild(1).GetComponent<Tipo>().tipo == Espacos[0].GetComponent<SnapNovo>().Tipo &&
                Espacos[1].transform.GetChild(1).GetComponent<Tipo>().tipo == Espacos[1].GetComponent<SnapNovo>().Tipo &&
                Espacos[2].transform.GetChild(1).GetComponent<Tipo>().tipo == Espacos[2].GetComponent<SnapNovo>().Tipo &&
                Espacos[3].transform.GetChild(1).GetComponent<Tipo>().tipo == Espacos[3].GetComponent<SnapNovo>().Tipo)
                {
                    Concluido = true;
                }
            }
        }

        if (Fase2 == true)
        {
            if (Espacos[0].transform.childCount >= 6 && Espacos[1].transform.childCount >= 4)
            {
                dataManager.GetAttempt();
                if (Espacos[0].transform.GetChild(0).GetComponent<Tipo>().tipo == Espacos[0].GetComponent<SnapNovo>().Tipo &&
                Espacos[0].transform.GetChild(1).GetComponent<Tipo>().tipo == Espacos[0].GetComponent<SnapNovo>().Tipo &&
                Espacos[0].transform.GetChild(2).GetComponent<Tipo>().tipo == Espacos[0].GetComponent<SnapNovo>().Tipo &&
                Espacos[0].transform.GetChild(3).GetComponent<Tipo>().tipo == Espacos[0].GetComponent<SnapNovo>().Tipo &&
                Espacos[0].transform.GetChild(4).GetComponent<Tipo>().tipo == Espacos[0].GetComponent<SnapNovo>().Tipo &&
                Espacos[0].transform.GetChild(5).GetComponent<Tipo>().tipo == Espacos[0].GetComponent<SnapNovo>().Tipo &&
                Espacos[1].transform.GetChild(0).GetComponent<Tipo>().tipo == Espacos[1].GetComponent<SnapNovo>().Tipo &&
                Espacos[1].transform.GetChild(1).GetComponent<Tipo>().tipo == Espacos[1].GetComponent<SnapNovo>().Tipo &&
                Espacos[1].transform.GetChild(2).GetComponent<Tipo>().tipo == Espacos[1].GetComponent<SnapNovo>().Tipo &&
                Espacos[1].transform.GetChild(3).GetComponent<Tipo>().tipo == Espacos[1].GetComponent<SnapNovo>().Tipo)
                {
                    GameObject.Find("Hand").GetComponent<PickUp>().enabled = false;
                    NPC.SetActive(true);

                    if (NPC.GetComponent<FecharEspaco>().OK == true)
                    {
                        Concluido = true;
                    }
                }
            }
        }

        // if (Fase3 == true)
        // {

        //     if (Espacos[0].transform.GetChild(1).GetComponent<Tipo>().tipo == Espacos[0].GetComponent<SnapNovo>().Tipo &&
        //     Espacos[1].transform.GetChild(1).GetComponent<Tipo>().tipo == Espacos[1].GetComponent<SnapNovo>().Tipo &&
        //     Espacos[2].transform.GetChild(1).GetComponent<Tipo>().tipo == Espacos[2].GetComponent<SnapNovo>().Tipo &&
        //     Espacos[3].transform.GetChild(1).GetComponent<Tipo>().tipo == Espacos[3].GetComponent<SnapNovo>().Tipo &&
        //     Espacos[4].transform.GetChild(1).GetComponent<Tipo>().tipo == Espacos[4].GetComponent<SnapNovo>().Tipo &&
        //     Espacos[5].transform.GetChild(1).GetComponent<Tipo>().tipo == Espacos[5].GetComponent<SnapNovo>().Tipo &&
        //     Espacos[6].transform.GetChild(1).GetComponent<Tipo>().tipo == Espacos[6].GetComponent<SnapNovo>().Tipo &&
        //     Espacos[7].transform.GetChild(1).GetComponent<Tipo>().tipo == Espacos[7].GetComponent<SnapNovo>().Tipo &&
        //     Espacos[8].transform.GetChild(1).GetComponent<Tipo>().tipo == Espacos[8].GetComponent<SnapNovo>().Tipo &&
        //     Espacos[9].transform.GetChild(1).GetComponent<Tipo>().tipo == Espacos[9].GetComponent<SnapNovo>().Tipo &&
        //     Espacos[10].transform.GetChild(1).GetComponent<Tipo>().tipo == Espacos[10].GetComponent<SnapNovo>().Tipo &&
        //     Espacos[11].transform.GetChild(1).GetComponent<Tipo>().tipo == Espacos[11].GetComponent<SnapNovo>().Tipo &&
        //     Espacos[12].transform.GetChild(1).GetComponent<Tipo>().tipo == Espacos[12].GetComponent<SnapNovo>().Tipo &&
        //     Espacos[13].transform.GetChild(1).GetComponent<Tipo>().tipo == Espacos[13].GetComponent<SnapNovo>().Tipo &&
        //     Espacos[14].transform.GetChild(1).GetComponent<Tipo>().tipo == Espacos[14].GetComponent<SnapNovo>().Tipo)
        //     {
        //         Concluido = true;
        //     }

        // }

        if (Fase3 == true)
        {
            if (Espacos[0].transform.childCount > 1 &&
            Espacos[1].transform.childCount > 1 &&
            Espacos[2].transform.childCount > 1 &&
            Espacos[3].transform.childCount > 1 &&
            Espacos[4].transform.childCount > 1 &&
            Espacos[5].transform.childCount > 1 &&
            Espacos[6].transform.childCount > 1 &&
            Espacos[7].transform.childCount > 1 &&
            Espacos[8].transform.childCount > 1 &&
            Espacos[9].transform.childCount > 1 &&
            Espacos[10].transform.childCount > 1 &&
            Espacos[11].transform.childCount > 1 &&
            Espacos[12].transform.childCount > 1 &&
            Espacos[13].transform.childCount > 1)
            {
                dataManager.GetAttempt();
                if (Espacos[0].transform.GetChild(1).GetComponent<Tipo>().tipo == Espacos[0].GetComponent<SnapNovo>().Tipo &&
                Espacos[1].transform.GetChild(1).GetComponent<Tipo>().tipo == Espacos[1].GetComponent<SnapNovo>().Tipo &&
                Espacos[2].transform.GetChild(1).GetComponent<Tipo>().tipo == Espacos[2].GetComponent<SnapNovo>().Tipo &&
                Espacos[3].transform.GetChild(1).GetComponent<Tipo>().tipo == Espacos[3].GetComponent<SnapNovo>().Tipo &&
                Espacos[4].transform.GetChild(1).GetComponent<Tipo>().tipo == Espacos[4].GetComponent<SnapNovo>().Tipo &&
                Espacos[5].transform.GetChild(1).GetComponent<Tipo>().tipo == Espacos[5].GetComponent<SnapNovo>().Tipo &&
                Espacos[6].transform.GetChild(1).GetComponent<Tipo>().tipo == Espacos[6].GetComponent<SnapNovo>().Tipo &&
                Espacos[7].transform.GetChild(1).GetComponent<Tipo>().tipo == Espacos[7].GetComponent<SnapNovo>().Tipo &&
                Espacos[8].transform.GetChild(1).GetComponent<Tipo>().tipo == Espacos[8].GetComponent<SnapNovo>().Tipo &&
                Espacos[9].transform.GetChild(1).GetComponent<Tipo>().tipo == Espacos[9].GetComponent<SnapNovo>().Tipo &&
                Espacos[10].transform.GetChild(1).GetComponent<Tipo>().tipo == Espacos[10].GetComponent<SnapNovo>().Tipo &&
                Espacos[11].transform.GetChild(1).GetComponent<Tipo>().tipo == Espacos[11].GetComponent<SnapNovo>().Tipo &&
                Espacos[12].transform.GetChild(1).GetComponent<Tipo>().tipo == Espacos[12].GetComponent<SnapNovo>().Tipo &&
                Espacos[13].transform.GetChild(1).GetComponent<Tipo>().tipo == Espacos[13].GetComponent<SnapNovo>().Tipo)
                {
                    Concluido = true;
                }
            }
        }

        if (Concluido == true)
        {
            Player.GetComponent<PlayerController>().enabled = false;
            Cams[0].GetComponent<CinemachineFreeLook>().Priority = 1;
            Cams[1].SetActive(true);
            Cams[1].GetComponent<CinemachineVirtualCamera>().Priority = 10;
            Redoma.GetComponent<Animator>().SetBool("abrir", true);
            GameObject.Find("Hand").GetComponent<PickUp>().enabled = false;

            //Codigo para definir no GameManager que a fase foi concluida
            GameManager.Instance.UpdateProgress(SceneManager.GetActiveScene().name);
            dataManager.WinLevel();
            StartCoroutine("Delay");
        }
    }


    IEnumerator Delay()
    {
        GameManager.Instance.HideAllControlPopup();
        GameManager.Instance.canPause = false;
        yield return new WaitForSeconds(3f);
        Cams[0].GetComponent<CinemachineFreeLook>().Priority = 10;
        Cams[1].SetActive(false);
        Cams[1].GetComponent<CinemachineVirtualCamera>().Priority = 1;
        PopUp.SetActive(true);
        yield return new WaitForSeconds(4f);
        Player.GetComponent<PlayerController>().enabled = true;
        PopUp.SetActive(false);
        GameManager.Instance.canPause = true;
    }
}
