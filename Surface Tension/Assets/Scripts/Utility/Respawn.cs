using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Respawn : MonoBehaviour {

    public GameObject spawnPoint;

	// Use this for initialization
	public void manualRespawn() {
        
        if (Input.GetKeyDown(KeyCode.R))
        {
            transform.position = spawnPoint.transform.position;
        }
    }

    public void manualReset()
    {
        if (Input.GetButtonDown("Restart"))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
	
}
