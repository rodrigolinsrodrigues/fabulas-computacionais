using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class StartMusicalGame : MonoBehaviour
{
    public GameObject level;
    public GameObject player;
    public GameObject startButton;
    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
    }
    public void Open()
    {
        player.SetActive(false);
        level.SetActive(true);
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(startButton);
        GameManager.Instance.canPause = false;
    }

    IEnumerator DelayOnStart()
    {
        yield return new WaitForSeconds(0.1f);
        Open();
    }

    public void Close()
    {
        player.SetActive(true);
        level.SetActive(false);
        GameManager.Instance.canPause = true;
    }
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager.Instance.ShowControlPopup(0);
            if (Input.GetButtonDown("X") && !level.activeSelf)
            {
                StartCoroutine("DelayOnStart");

            }
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager.Instance.HideAllControlPopup();
        }
    }
}
