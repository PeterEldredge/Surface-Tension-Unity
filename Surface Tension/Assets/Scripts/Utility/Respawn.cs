using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Respawn : MonoBehaviour {

    public void HandleRespawn()
    {
        if (Input.GetButtonDown("Restart"))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    //When the player attached with this script is met with a killbox, the scene resets
    public void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Kill Box")
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
