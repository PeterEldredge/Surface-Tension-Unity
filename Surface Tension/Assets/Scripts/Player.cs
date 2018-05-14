using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Player : MonoBehaviour 
{
	/// <summary>
	/// Rate at which player moves
	/// </summary>
	public float moveSpeed;


	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () 
	{
		float horizontal = Input.GetAxis("Horizontal");
		// float vertical = Input.GetAxis("vertical");

		// Move actor in direction of input
        Vector3 movement = new Vector2 (horizontal * moveSpeed, 0);
        gameObject.transform.position += movement;
	}
}
