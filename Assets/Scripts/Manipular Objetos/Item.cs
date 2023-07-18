using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    public bool isAction;
    public string type;
    public MeshFilter meshFilter;
    public int recipeID; 

    private void Start() {
        meshFilter = GetComponent<MeshFilter>();
    }
}
