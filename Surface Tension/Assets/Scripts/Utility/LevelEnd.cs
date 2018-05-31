using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelEnd : MonoBehaviour {

    // Must give the name of the next scene in the inspector
    public string NextLevel;

    // Upon an object's collider that has the attached tag of "Player", the next scene is loaded
    // These scenes must be queued with through File -> Build Settings and then add the scenes to the
    // "Scenes in Build" section. 
    public void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            SceneManager.LoadScene(NextLevel);
        }
    }
    
}
