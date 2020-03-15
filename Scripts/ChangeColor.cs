using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeColor : MonoBehaviour
{

    public Color color;
    // Start is called before the first frame update
    void Start()
    {
        //MaterialPropertyBlock block = new MaterialPropertyBlock();
        //block.SetColor("_BaseColor", color);
        GetComponent<Renderer>().material.SetColor("_BaseColor", color);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
