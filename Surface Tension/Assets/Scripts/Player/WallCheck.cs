using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallCheck : MonoBehaviour {

    public bool isNextToWall = false;
    public bool isNextToObject = false;

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Object")
        {
            isNextToObject = true;
        }
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            isNextToWall = true;
        }
    }

    void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.tag == "Object")
        {
            isNextToObject = true;
        }
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
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
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            isNextToObject = false;
        }
    }
}
