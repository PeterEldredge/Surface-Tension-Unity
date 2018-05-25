using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SurfaceGUI : MonoBehaviour {

    Player player;
    Color scol;

	// Use this for initialization
	void Start () {
        player = GameObject.Find("Player").GetComponent<Player>();
        GetComponent<Renderer>().material.color = Color.grey;
	}
	
	// Update is called once per frame
	void Update () {
        if (player.equippedMaterial == SurfaceChange.material.BOUNCE)
        {
            scol = Color.blue;
        }
        if (player.equippedMaterial == SurfaceChange.material.SLIP)
        {
            scol = Color.red;
        }
        if (player.equippedMaterial == SurfaceChange.material.STICK)
        {
            scol = Color.yellow;
        }
        GetComponent<Renderer>().material.color = scol;
    }
}
