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
    /// Mapping of materials to top speeds
    /// </summary>
    /// <returns></returns>
    public Dictionary<GameController.material, float> topSpeedMapping = new Dictionary<material, float> {
        { material.NONE, 6F },
        { material.BOUNCE, 6F },
        { material.SLIP, 10F },
        { material.STICK, 3F }
    };
}
