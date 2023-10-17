using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Snap : MonoBehaviour
{
    public GameObject obj, objPosition;
    public bool isAction, isOccupied, isCorrect;
    public PickAndDrop pickup;
    public string correctType, currentType;
    public int recipeID; 

    void Start()
    {
        pickup = GameObject.Find("Hand").GetComponent<PickAndDrop>();
    }
    public void Recept(Item item)
    {
        if (item.isAction == isAction)
        {
            obj = item.gameObject;
            isOccupied = true;
            currentType = item.type;
            item.GetComponent<Outline>().enabled = true;
            item.GetComponent<Outline>().OutlineColor = Color.yellow;
            Verify();
            FreezeObj(obj);
        }
    }
    public GameObject Withdraw()
    {
        GameObject clone = null;
        if (obj != null)
        {
            clone = obj;
            clone.GetComponent<Outline>().enabled = false;
            obj = null;
            isOccupied = false;
            currentType = "";
            Verify();
        }
        return clone;
    }

    public void FreezeObj(GameObject obj)
    {
        obj.transform.SetParent(this.transform);
        obj.transform.position = objPosition.transform.position;
        //obj.transform.rotation = new Quaternion(-0.05f, -0.7f, -0.7f, 0.05f);
        obj.transform.rotation = objPosition.transform.rotation;
        obj.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePosition;
        obj.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation;
        obj.GetComponent<Rigidbody>().useGravity = false;
        obj.GetComponent<Rigidbody>().isKinematic = true;
    }
    public void Verify()
    {
        isCorrect = currentType == correctType;
    }
}
