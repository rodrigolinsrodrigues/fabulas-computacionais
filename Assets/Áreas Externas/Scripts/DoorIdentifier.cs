using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorIdentifier : MonoBehaviour
{
    public string levelName;
    public bool isAreaDoor;
    private void Start()
    {
        if (isAreaDoor)
        {
            switch (levelName)
            {
                case "Area2":

                    gameObject.SetActive(GameManager.Instance.GetCurrentStars() >= 2);

                    break;
                case "Area3":

                    gameObject.SetActive(GameManager.Instance.GetCurrentStars() >= 5);

                    break;
            }
        }
        else
        {
            if (GameManager.Instance.CurrentProgress(levelName))
            {
                gameObject.SetActive(false);
            }
        }
    }
}
