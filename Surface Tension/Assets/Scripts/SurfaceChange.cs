using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SurfaceChange : MonoBehaviour {

    public enum material
    {
        NONE,
        BOUNCE,
        SLIP,
        STICK
    }

    public Color ncol;
    private material currentS;
    Player p1;    

    void Start()
    {
        p1 = GameObject.Find("Player").GetComponent<Player>();
    }

    void OnMouseDown()
    {
        if (p1.equippedMaterial == material.BOUNCE)
        {
            ncol = Color.blue;
        }
        if (p1.equippedMaterial == material.SLIP)
        {
            ncol = Color.red;
        }
        if (p1.equippedMaterial == material.STICK)
        {
            ncol = Color.yellow;
        }

        GetComponent<Renderer>().material.color = ncol;
    }
}
