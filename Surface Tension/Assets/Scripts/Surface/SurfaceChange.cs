using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SurfaceChange : MonoBehaviour {

    //instance of player object
    Player player;    

    void Start()
    {
        player = GameObject.FindWithTag("GameController").GetComponent<GameController>().player;
    }

    //For right clicking to remove surfaces
    void OnMouseOver()
    {
        // Left click
        if (Input.GetMouseButton(0))
        {
            ChangeMaterial();
        }
        // Right click
        else if (Input.GetMouseButton(1))
        {
            GetComponent<Renderer>().material.color = Color.white;
        }
    }

    //Only works for left clicking, for changing surfaces
    void ChangeMaterial()
    {
        switch(player.equippedMaterial) {
            case GameController.material.BOUNCE:
                GetComponent<Renderer>().material.color = Color.blue;
                break;
            case GameController.material.SLIP:
                GetComponent<Renderer>().material.color = Color.red;
                break;
            case GameController.material.STICK:
                GetComponent<Renderer>().material.color = Color.yellow;
                break;
        }
    }
}
