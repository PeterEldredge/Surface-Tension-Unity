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
    public Dictionary<GameController.material, SurfaceSpeeds> topSpeedMapping = new Dictionary<material, SurfaceSpeeds> {
        { material.NONE, new SurfaceSpeeds {
            defaultSpeed = 4F,
            upSlopeSpeed = 2.5F,
            pushSpeed = 1.5F,
            pullSpeed = 1.5F
        }},
        { material.SLIP, new SurfaceSpeeds {
            defaultSpeed = 8F,
            upSlopeSpeed = 6.5F,
            pushSpeed = 5.5F,
            pullSpeed = 5.5F
        }},
        { material.BOUNCE, new SurfaceSpeeds {
            defaultSpeed = 4F,
            upSlopeSpeed = 2.5F,
            pushSpeed = 1.5F,
            pullSpeed = 1.5F
        }},
        { material.STICK, new SurfaceSpeeds {
            defaultSpeed = 2F,
            upSlopeSpeed = .5F,
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
