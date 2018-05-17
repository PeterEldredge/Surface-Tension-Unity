﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Player : MonoBehaviour 
{
    //Horizontal velocity multiplyer
    public float moveSpeed;

    //Velocity applied to player on jump
    public float jumpVelocity;

    //Affects the gravity of the player as they fall
    public float fallMultiplyer;

    //Affects the gravity of the player as they jump
    public float jumpMultiplyer;

    //Initializes pBody, this will be the player's Rigidbody2D component 
    Rigidbody2D pBody;

    //Initializes shortcut to IsGrounded script
    public IsGrounded isGroundedShortcut;

    //Initializes shortcut to WallCheck script on both RightCheck and LeftCheck
    public WallCheck rWallCheckShortcut;
    public WallCheck lWallCheckShortcut;

    float movement;

    private enum Direction {
        LEFT = -1,
        RIGHT = 1,
        IDLE = 0
    };

    private Direction direction;

    void Awake()
    {
        pBody = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update () 
	{
        // Get player input
        float horizontalInput = Input.GetAxis("Horizontal") * moveSpeed;

        // Move and animate character
        HandleMovement(horizontalInput);
        HandleAnimation(horizontalInput);
        HandleJump();

        // Check if stuck
        IsStuck();  
	}

    private void HandleAnimation(float movement)
    {
        Direction direction = GetDirection(movement);
        GetComponent<Animator>().SetInteger("Direction", (int)direction);
    }

    private void HandleMovement(float movement) 
    {
        //Ensures player cannot get stuck on walls by preventing velocity towards a wall when directly next to it

        // If moving away from wall or not jumping
        if ((GetDirection(movement) == Direction.RIGHT && !TouchingWall(Direction.RIGHT)) || (GetDirection(movement) == Direction.LEFT && !TouchingWall(Direction.LEFT)) || (isGroundedShortcut.isGrounded && !Input.GetButtonDown("Jump")))
        {
            if (!(Input.GetButtonDown("Jump") && GetDirection(movement) == Direction.RIGHT && TouchingWall(Direction.RIGHT)) || !(Input.GetButtonDown("Jump") && GetDirection(movement) == Direction.LEFT && TouchingWall(Direction.LEFT)))
            {
                pBody.velocity = new Vector2(movement, pBody.velocity.y);
            }
        }
    }

    private void HandleJump() {
        if (Input.GetButtonDown("Jump") && isGroundedShortcut.isGrounded)
        {
            pBody.velocity = Vector2.up * jumpVelocity;
        }

        //Multiplies the affects of gravity on the player given specific instances
        if (pBody.velocity.y < 0) //If player falling, increase gravity affect
        {
            pBody.velocity += Vector2.up * Physics2D.gravity.y * (fallMultiplyer) * Time.deltaTime;
        } else if (pBody.velocity.y > 0 && !Input.GetButton("Jump")) 
        {
            pBody.velocity += Vector2.up * Physics2D.gravity.y * (jumpMultiplyer) * Time.deltaTime;
        }
    }

    private Direction GetDirection(float movement) {
        if(movement > 0) {
            return Direction.RIGHT;
        }
        else if(movement < 0) {
            return Direction.LEFT;
        }
        else return Direction.IDLE;
    }

    //Check to see if all stuck conditions are met, if so it applies the jump velocity downwards to the player
    private void IsStuck()
    {
        if (TouchingWall(Direction.RIGHT) && TouchingWall(Direction.LEFT) && pBody.velocity.y == 0 && !isGroundedShortcut.isGrounded)
        {
            pBody.velocity =  -1 * Vector2.up * jumpVelocity;
        }
    }

    /// <summary>
    /// If player is touching a wall, return what side it's on
    /// </summary>
    private bool TouchingWall(Direction direction)
    {
        if(direction.Equals(Direction.RIGHT) && rWallCheckShortcut.isNextToWall) {
            return true;
        }
        else if(direction.Equals(Direction.LEFT) && lWallCheckShortcut.isNextToWall) {
            return true;
        }
        else return false;
    }

    
}