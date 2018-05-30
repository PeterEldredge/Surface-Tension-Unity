using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlickSurface : SurfaceMaterial {

	/// <summary>
	/// Start is called on the frame when a script is enabled just before
	/// any of the Update methods is called the first time.
	/// </summary>
	void OnAwake()
	{
		// Declare type and initialize with movement speeds
		type = GameController.material.SLIP;
		InitializeSurfaceSpeeds(type);
	}
}
