using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadNewArea : MonoBehaviour
{
    public Transform spawnPoint;
    public bool canTeleport = false;
    public string currentDoor;

    public void LoadArea(string areaName)
    {
        GameManager.Instance.SetCurrentScene(areaName);
        SceneManager.LoadScene(areaName);
    }
    private void Update()
    {
        if (canTeleport)
        {
            if (Input.GetButtonDown("X"))
            {
                LoadArea(currentDoor);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("door"))
        {
            currentDoor = other.GetComponent<DoorIdentifier>().levelName;
            canTeleport = true;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("door"))
        {
            currentDoor = "";
            canTeleport = false;
        }
    }
}
