using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadNewArea : MonoBehaviour
{
    public Transform spawnPoint, playerTransform;
    public bool canTeleport = false;
    public string currentDoor;
    public LevelDataManager levelData;

    void Awake()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;

    }
    void Start()
    {
        if (FindObjectOfType<LevelDataManager>())
        {
            levelData = FindObjectOfType<LevelDataManager>();
        }

        spawnPoint = GameObject.Find(GameManager.Instance.spawnPoint).transform;
        StartCoroutine("test");
    }
    public void LoadArea(string areaName)
    {
        if (canTeleport)
        {
            canTeleport = false;
            TelemetryManager.Log("door_enter", ("door", areaName));
            GetComponent<BoxCollider>().enabled = false;
            GameManager.Instance.SetCurrentScene(areaName);
            GameManager.Instance.HideAllControlPopup();
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
                if (levelData != null)
                {
                    DataManager.Instance.SaveDataLocal();
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("door"))
        {
            currentDoor = other.GetComponent<DoorIdentifier>().levelName;
            canTeleport = true;
            TelemetryManager.Log("door_approach", ("door", currentDoor));
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("door"))
        {
            if (canTeleport) TelemetryManager.Log("door_leave", ("door", currentDoor));
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
