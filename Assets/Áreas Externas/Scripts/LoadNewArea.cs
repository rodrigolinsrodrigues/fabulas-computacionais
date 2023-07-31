using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadNewArea : MonoBehaviour
{
    public Transform spawnPoint, playerTransform;
    public bool canTeleport = false;
    public string currentDoor;

    void Awake()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;

    }
    void Start()
    {
        spawnPoint = GameObject.Find(GameManager.Instance.spawnPoint).transform;
        StartCoroutine("test");
    }
    public void LoadArea(string areaName)
    {
        if (canTeleport)
        {
            canTeleport = false;
            GetComponent<BoxCollider>().enabled = false;
            GameManager.Instance.SetCurrentScene(areaName);
            SceneManager.LoadScene(areaName);
        }
    }

    private void Update()
    {
        if (canTeleport)
        {
            if (Input.GetButtonDown("X"))
            {
                LoadArea(currentDoor);
                canTeleport = false;
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
    IEnumerator test()
    {
        yield return new WaitForSecondsRealtime(1f);
        playerTransform.position = spawnPoint.position;
        playerTransform.rotation = spawnPoint.transform.rotation;
    }
}
