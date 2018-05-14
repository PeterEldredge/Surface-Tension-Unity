using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Player : MonoBehaviour 
{
	/// <summary>
	/// Rate at which player moves
	/// </summary>
	public float moveSpeed;
	public float startMoveSpeed;
	public float startMoveThresh;
	public float slowMultiplyer;


	// Update is called once per frame
	void Update () 
	{
		Vector2 movement;
		float velocity = GetComponent<Rigidbody2D>().velocity.x;
		float horizontal = Input.GetAxis("Horizontal");
		// float vertical = Input.GetAxis("vertical");

		// Move actor in direction of input
		if (Mathf.Abs(velocity) < startMoveThresh){
			Debug.Log("under startMove threshold");
			movement = new Vector2 (horizontal * startMoveSpeed * Time.deltaTime, 0);
		} else {
			Debug.Log("surpassed startMove threshold");
        	movement = new Vector2 (horizontal * moveSpeed * Time.deltaTime, 0);
		}

		if (horizontal == 0 && velocity > 0){
			movement = new Vector2 (velocity * slowMultiplyer * Time.deltaTime, 0);
		}

        GetComponent<Rigidbody2D>().velocity += movement;
	}
}
