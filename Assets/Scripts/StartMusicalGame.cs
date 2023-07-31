using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartMusicalGame : MonoBehaviour
{
    public GameObject level;
    public GameObject player;
    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
    }
    public void Open()
    {
        player.SetActive(false);
        level.SetActive(true);
    }

    public void Close()
    {
        player.SetActive(true);
        level.SetActive(false);
    }
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (Input.GetButtonDown("X"))
            {
                Open();
            }
        }
    }
}
