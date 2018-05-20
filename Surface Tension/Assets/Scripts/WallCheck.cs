using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallCheck : MonoBehaviour {

    public bool isNextToWall = false;

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Object")
        {
            isNextToWall = true;
        }
    }

    void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.tag == "Object")
        {
            isNextToWall = true;
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "Object")
        {
            isNextToWall = false;
        }
    }
}
