using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SurfaceChange : MonoBehaviour {

    //List of our surfaces as enums
    public enum material
    {
        NONE,
        BOUNCE,
        SLIP,
        STICK
    }

    //new color, will be changed to materials once the surfaces are implemented
    public Color ncol;
    //original color, necessary for returning the surface to its original state with right click
    public Color ocol;
    //instance of player object
    Player player;    

    void Start()
    {
        player = GameObject.Find("Player").GetComponent<Player>();
        ncol = GetComponent<Renderer>().material.color;
        ocol = GetComponent<Renderer>().material.color;
    }

    //For right clicking to remove surfaces
    void OnMouseOver()
    {
        if (Input.GetMouseButton(1))
        {
            GetComponent<Renderer>().material.color = ocol;
        }
    }

    //Only works for left clicking, for changing surfaces
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
