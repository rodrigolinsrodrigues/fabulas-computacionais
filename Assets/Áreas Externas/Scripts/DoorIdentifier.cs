using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorIdentifier : MonoBehaviour
{
    public string levelName;
    public bool isAreaDoor;
    public GameObject noticeBoard, winBoard;
    private void Start()
    {
        if (isAreaDoor)
        {
            switch (levelName)
            {
                case "Area2":

                    noticeBoard.SetActive(GameManager.Instance.GetCurrentStars() < 2);
                    gameObject.GetComponent<BoxCollider>().enabled = GameManager.Instance.GetCurrentStars() >= 2;
                    break;
                case "Area3":
                    noticeBoard.SetActive(GameManager.Instance.GetCurrentStars() < 5);
                    gameObject.GetComponent<BoxCollider>().enabled = GameManager.Instance.GetCurrentStars() >= 5;

                    break;
            }
        }
        else
        {
            if (GameManager.Instance.CurrentProgress(levelName))
            {
                winBoard.SetActive(true);
                gameObject.GetComponent<BoxCollider>().enabled = false;
            }
        }
    }
}
