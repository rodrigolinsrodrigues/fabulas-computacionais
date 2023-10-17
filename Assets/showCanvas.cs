using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Cinemachine;

public class showCanvas : MonoBehaviour
{
    public GameObject Redoma, Player, winCanvas;
    public GameObject[] pilares, Cams;
    bool bloq, concluido;
    public LevelDataManager dataManager;

    void Start()
    {
        dataManager = FindObjectOfType<LevelDataManager>();
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Light"))
        {
            for (int i = 0; i < pilares.Length; i++)
            {
                pilares[i].gameObject.tag = "Untagged";
            }
            Redoma.GetComponent<Animator>().SetBool("abrir", true);
            GameManager.Instance.HideAllControlPopup();
            //Codigo para definir no GameManager que a fase foi concluida
            GameManager.Instance.UpdateProgress(SceneManager.GetActiveScene().name);
            dataManager.WinLevel();
            StartCoroutine(Delay1());
        }


    }

    IEnumerator Delay1()
    {
        GameManager.Instance.canPause = false;
        yield return new WaitForSeconds(3f);
        Cams[0].SetActive(false);
        Cams[1].SetActive(true);
        Cams[1].GetComponent<CinemachineVirtualCamera>().Priority = 10;
        Cams[2].GetComponent<CinemachineFreeLook>().Priority = 1;
        yield return new WaitForSeconds(3f);
        Cams[1].SetActive(false);
        // Cams[2].SetActive(true);
        Cams[1].GetComponent<CinemachineVirtualCamera>().Priority = 1;
        Cams[2].GetComponent<CinemachineFreeLook>().Priority = 10;
        yield return new WaitForSeconds(1f);
        winCanvas.SetActive(true);
        yield return new WaitForSeconds(3f);
        winCanvas.SetActive(false);
        Player.GetComponent<PlayerController>().enabled = true;
        GameManager.Instance.canPause = true;
    }

}
