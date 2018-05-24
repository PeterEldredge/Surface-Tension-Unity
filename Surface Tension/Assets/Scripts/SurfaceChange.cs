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
    Player player;    

    void Start()
    {
        player = GameObject.Find("Player").GetComponent<Player>();
    }

    void OnMouseDown()
    {
        if (player.equippedMaterial == material.BOUNCE)
        {
            ncol = Color.blue;
        }
        if (player.equippedMaterial == material.SLIP)
        {
            ncol = Color.red;
        }
        if (player.equippedMaterial == material.STICK)
        {
            ncol = Color.yellow;
        }
        GetComponent<Renderer>().material.color = ncol;
    }
}
