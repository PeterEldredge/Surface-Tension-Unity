using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SurfaceChange : MonoBehaviour {

    private enum surface
    {
        BOUNCE,
        SLIP,
        STICK
    }

    

    public Color ncol;

    void Start()
    {
        GetComponent<Renderer>().material.color = Color.blue;
    }

    void OnMouseDown()
    {
        GetComponent<Renderer>().material.color = ncol;
    }
}
