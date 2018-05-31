using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SurfaceGUI : MonoBehaviour {

    Player player;

    //Creates an array to hold the materials that represent the surfaces
    public Material[] ChosenSurface;

	// Use this for initialization
	void Start () {
        player = GameObject.Find("Player").GetComponent<Player>();
	}
	
	// Update is called once per frame
	void Update () 
    {
        switch(player.equippedMaterial) {
            case GameController.material.BOUNCE:
                GetComponent<Renderer>().material = ChosenSurface[0];
                break;
            case GameController.material.SLIP:
                GetComponent<Renderer>().material = ChosenSurface[1];
                break;
            case GameController.material.STICK:
                GetComponent<Renderer>().material = ChosenSurface[2];
                break;
        }
    }
}
