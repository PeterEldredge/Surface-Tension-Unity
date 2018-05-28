using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Surface : MonoBehaviour
{
    /// <summary>
    /// Top speed the player can move on this surface
    /// </summary>
    public float topSpeed;

    public enum Type
    {
        SLICK,
        BOUNCY
    }

    /// <summary>
    /// Type of surface
    /// </summary>
    public Type type;
	
}
