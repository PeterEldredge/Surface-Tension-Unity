using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour {

	// List of our surfaces as enums
    public enum material
    {
        NONE,
        BOUNCE,
        SLIP,
        STICK
    }

	/// <summary>
	/// Reference to player
	/// </summary>
	public Player player;

    /// <summary>
    /// Mapping of materials to movement speeds
    /// </summary>
    /// <returns></returns>
    public Dictionary<material, SurfaceSpeeds> speedMapping = new Dictionary<material, SurfaceSpeeds> {
        { material.NONE, new SurfaceSpeeds {
            defaultSpeed = 4f,
            upSlopeSpeed = 2.5f,
            pushSpeed = 1.5f,
            pullSpeed = 1f
        }},
        { material.SLIP, new SurfaceSpeeds {
            defaultSpeed = 8f,
            upSlopeSpeed = 6.5f,
            pushSpeed = 5.5f,
            pullSpeed = 5.5f
        }},
        { material.BOUNCE, new SurfaceSpeeds {
            defaultSpeed = 4f,
            upSlopeSpeed = 2.5f,
            pushSpeed = 1.5f,
            pullSpeed = 1.5f
        }},
        { material.STICK, new SurfaceSpeeds {
            defaultSpeed = 2f,
            upSlopeSpeed = .5f,
            pushSpeed = 0,
            pullSpeed = 0
        }}
    };

    public struct SurfaceSpeeds {
        public float defaultSpeed;
        public float upSlopeSpeed;
        public float pushSpeed;
        public float pullSpeed;
    }
}
