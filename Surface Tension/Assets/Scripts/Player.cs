using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Player : MonoBehaviour 
{
    /// <summary>
    /// Maximum speed player can move (modified by ground contact with surface)
    /// </summary>
    public float maxSpeed;

    /// <summary>
    /// Speed at which player pushes a block
    /// </summary>
    public float pushSpeed;

    /// <summary>
    /// Speed at which player navigates a slope
    /// </summary>
    public float slopeSpeed;

    float horizontalInput;

    public float maxHeightVelocity;
    public float minHeightVelocity;

    public float upGravity;
    public float downGravity;

    private bool applyMaxUpwards = false;
    private bool applyMinUpwards = false;

    private Vector2 prevVeclocity;


    //Initializes pBody, this will be the player's Rigidbody2D component 
    Rigidbody2D pBody;

    /// <summary>
    /// Definition for direction player is facing
    /// </summary>
    public enum Direction {
        LEFT = -1,
        RIGHT = 1
    };

    /// <summary>
    /// Definition for surface the player is touching
    /// </summary>
    public enum Surface
    {
        OBJECT,
        GROUND,
        SLOPE,
        ALL,
        NONE
    };

    /// <summary>
    /// Direction player is currently facing
    /// </summary>
    private Direction direction;

    /// <summary>
    /// True if player is currently moving
    /// </summary>
    protected bool moving;

    private Respawn respawn;

    void Awake()
    {
        pBody = GetComponent<Rigidbody2D>();
        prevVeclocity = pBody.velocity;
    }

    // Update is called once per frame
    void Update()
    {
        // Get player input
        horizontalInput = Input.GetAxis("Horizontal");

        //Checks jump key
        JumpDown();

        HandleRespawn();
    }

    // Update called at a fixed delta time
    void FixedUpdate () 
	{

        // Move and animate character
        HandleMovement(horizontalInput);
        HandleAnimation(horizontalInput);

        //Handles changing y velocity
        HandleJump();

        // Check if stuck 
        IsStuck();
	}

    /// <summary>
    /// Move player based on horizontal input
    /// </summary>
    /// <param name="horizontalInput">Horizontal input</param>
    private void HandleMovement(float horizontalInput)
    {
        //Ensures player cannot get stuck on walls by preventing velocity towards a wall when directly next to it
        //If moving away from wall or not jumping
        Direction? inputDirection = GetDirection(horizontalInput);

        //Stores velocity the player will move at
        float moveSpeed = maxSpeed;

        //Figure out raycasts that can check for multiple surfaces!
        if (Touching(inputDirection, Surface.OBJECT) && (TouchingGround(Surface.GROUND) || TouchingGround(Surface.SLOPE)))
        {
            moveSpeed += pushSpeed;
        }
        if (Touching(inputDirection, Surface.SLOPE) && TouchingGround(Surface.ALL))
        {
            moveSpeed += slopeSpeed;
        }
        if ((Touching(inputDirection, Surface.GROUND) && !Touching(inputDirection, Surface.OBJECT)) 
            || (Touching(inputDirection, Surface.ALL) && !TouchingGround(Surface.ALL)))
        {
            moveSpeed = 0;
        }

        pBody.velocity = new Vector2(horizontalInput * moveSpeed, pBody.velocity.y);
        prevVeclocity = pBody.velocity;
    }

    /// <summary>
    /// Animate player based on horizontal input
    /// </summary>
    /// <param name="movement">Horizontal input</param>
    private void HandleAnimation(float movement)
    {
        direction = GetDirection(movement) ?? direction;
        moving = GetDirection(movement) != null;

        GetComponent<Animator>().SetInteger("Direction", (int)direction);
        GetComponent<Animator>().SetBool("Moving", moving);
        
        if(direction.Equals(Direction.LEFT)) {
            GetComponent<SpriteRenderer>().flipX = true;
        }
        else if(direction.Equals(Direction.RIGHT)) {
            GetComponent<SpriteRenderer>().flipX = false;
        }
    }

    /// <summary>
    /// Returns if the jump key is being held down
    /// </summary>
    private void JumpDown()
    {
        if ((Input.GetKeyDown(KeyCode.Space) || Input.GetButtonDown("Jump")) && TouchingGround(Surface.ALL))
        {
            applyMaxUpwards = true;
        }
        if (Input.GetKeyUp(KeyCode.Space) || Input.GetButtonUp("Jump"))
        {
            applyMinUpwards = true;
        }
    }

    private void HandleJump()
    {
        if (applyMaxUpwards && TouchingGround(Surface.ALL))
        {
            pBody.velocity = new Vector2(pBody.velocity.x, maxHeightVelocity);
            applyMaxUpwards = false;
        }
        else if (applyMinUpwards)
        {
            if (minHeightVelocity < pBody.velocity.y)
            {
                pBody.velocity = new Vector2(pBody.velocity.x, minHeightVelocity);
            }
            applyMinUpwards = false;
        }

        if (!TouchingGround(Surface.ALL))
        {
            if (pBody.velocity.y > 0)
            {
                pBody.gravityScale = upGravity;
            }
            else if (pBody.velocity.y < 0)
            {
                pBody.gravityScale = downGravity;
            }
            else
            {
                pBody.gravityScale = 1f;
            }
        }
    }

    /// <summary>
    /// Returns direction of given movement
    /// </summary>
    /// <param name="movement">Player movement</param>
    private Direction? GetDirection(float movement) {
        if(movement > 0) {
            return Direction.RIGHT;
        }
        else if(movement < 0) {
            return Direction.LEFT;
        }
        else return null;
    }

    //Check to see if all stuck conditions are met, if so it applies the jump velocity downwards to the player
    private void IsStuck()
    {
        if (!Touching(Direction.RIGHT, Surface.ALL) && !Touching(Direction.LEFT, Surface.ALL) && pBody.velocity.y == 0 && !TouchingGround(Surface.ALL))
        {
            pBody.velocity =  -1 * Vector2.up * 10;
        }
    }

    /// <summary>
    /// Returns whether player is touching ground
    /// </summary>
    private bool TouchingGround(Surface surface)
    {
        BoxCollider2D collider = GetComponent<BoxCollider2D>();

        // Calculate bottom of player:
        // Bottom of BoxCollider + edgeRadius around collider (subtraction because in downward direction)

        float playerHeight = collider.bounds.size.y;
        float playerBottom = collider.bounds.center.y - (playerHeight / 2F) - collider.edgeRadius - .05f;

        // Calculate left edge of player:
        // Left edge of BoxCollider + 1/2 of edgeRadius (subtraction because in leftward direction)
        float playerXMin = collider.bounds.center.x - (collider.bounds.size.x / 2) - (collider.edgeRadius / 2);

        // Create vector positioned at bottom of player sprite
        Vector2 origin = new Vector2(playerXMin, playerBottom);

        float distance = collider.bounds.size.x + collider.edgeRadius;

        if (surface == Surface.ALL)
        {
            if (ObjectCast(origin, Vector2.right, distance) != Surface.NONE)
            {
                return true;
            }
            else if (GroundCast(origin, Vector2.right, distance) != Surface.NONE)
            {
                return true;
            }
        }
        else if (ObjectCast(origin, Vector2.right, distance) == surface)
        {
            return true;
        }
        else if (GroundCast(origin, Vector2.right, distance) == surface)
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// If player is touching an object, return it
    /// </summary>
    private bool Touching(Direction? direction, Surface surface)
    {
        BoxCollider2D collider = GetComponent<BoxCollider2D>();

        // Calculate bottom of player:
        // Bottom of BoxCollider
        float playerYMin = collider.bounds.center.y - (collider.bounds.size.y / 2f) - (collider.edgeRadius / 2f);

        // Calculate distance to left edge of player:
        // Half the collider + the radius + a little
        // Left or Right determines the side of the player the ray is being shot from
        float playerXMin = (collider.bounds.size.x / 2) + (collider.edgeRadius) + .05f;
        if (direction == Direction.LEFT)
        {
            playerXMin = collider.bounds.center.x - playerXMin;
        } else
        {
            playerXMin = collider.bounds.center.x + playerXMin;
        }

        // Create vector positioned at bottom of player sprite
        Vector2 origin = new Vector2(playerXMin, playerYMin);

        float distance = collider.bounds.size.y + collider.edgeRadius;
        if (surface == Surface.ALL)
        {
            if (ObjectCast(origin, Vector2.up, distance) != Surface.NONE)
            {
                return true;
            }
            else if (GroundCast(origin, Vector2.up, distance) != Surface.NONE)
            {
                return true;
            }
        }
        else if (ObjectCast(origin, Vector2.up, distance) == surface)
        {
            return true;
        }
        else if (GroundCast(origin, Vector2.up, distance) == surface)
        {
            return true;
        }
        return false;
    }

    private Surface ObjectCast(Vector2 origin, Vector2 direction, float distance)
    {
        RaycastHit2D raycast = Physics2D.Raycast(origin, direction, distance, LayerMask.GetMask("Object"));
        if (raycast.collider != null)
        {
            if (raycast.collider.tag == "Object")
            {
                return Surface.OBJECT;
            }
        }
        return Surface.NONE;
    }

    private Surface GroundCast(Vector2 origin, Vector2 direction, float distance)
    {
        RaycastHit2D raycast = Physics2D.Raycast(origin, direction, distance, LayerMask.GetMask("Ground"));
        if (raycast.collider != null)
        {
            if (raycast.collider.tag == "Slope")
            {
                return Surface.SLOPE;
            }
            else if (raycast.collider.tag == "Ground")
            {
                return Surface.GROUND;
            }
        }
        return Surface.NONE;
    }

    // If R is pressed, the player will respawn at the position of empty game object "Spawn Point"
    private void HandleRespawn()
    {
        respawn = GetComponent<Respawn>();
        respawn.manualRespawn();
    }
}