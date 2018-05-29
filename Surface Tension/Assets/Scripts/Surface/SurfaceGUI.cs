using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SurfaceGUI : MonoBehaviour {

    Player player;

	// Use this for initialization
	void Start () {
        player = GameObject.Find("Player").GetComponent<Player>();
        GetComponent<Renderer>().material.color = Color.grey;
	}
	
	// Update is called once per frame
	void Update () 
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
