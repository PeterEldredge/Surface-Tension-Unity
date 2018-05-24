using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Player : MonoBehaviour 
{
    /// <summary>
    /// Maximum speed player can move (modified by ground contact with surface)
    /// </summary>
    public float defaultSpeed;

    /// <summary>
    /// Speed at which player pushes a block
    /// </summary>
    public float pushSpeed;

    /// <summary>
    /// Speed at which player pulls a block
    /// </summary>
    public float pullSpeed;

    /// <summary>
    /// Speed at which player navigates a slope
    /// </summary>
    public float slopeSpeed;

    float horizontalInput;

    public float maxHeightVelocity;
    public float minHeightVelocity;

    public float upGravity;
    public float downGravity;

    public float grabLeniency;

    private bool applyMaxUpwards = false;
    private bool applyMinUpwards = false;

    private bool objectAgainstWall = false;

    private State currentState;
    private State previousState;

    public SurfaceChange.material equippedMaterial;

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
    /// Definition for surfaces the player is touching
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
    /// Holds information about the current frame to drive movement and animations
    /// </summary>
    public struct State
    {
        public Action action;
        public Direction direction;
        public GameObject grabbedObject;
        public GameObject groundObject;
    };

    /// <summary>
    /// Definition for the states the player could be in
    /// </summary>
    public enum Action
    {
        NORMAL, // Nothing is blocking the player and they may move at notmal speed
        PUSHING, // The player is pushing an object 
        PULLING, // The player is pulling an object
        UPSLOPE, // The player is currently walking up a slope
        AGAINSTWALL // The player is up against a wall and cannot move in that direction
    };

    /// <summary>
    /// Direction player is currently facing
    /// </summary>
    // private Direction animDirection;
    private Direction? inputDirection;
    private Direction? oppositeDirection;

    /// <summary>
    /// True if player is currently moving
    /// </summary>
    protected bool moving;

    private Respawn respawn;

    void Start()
    {
        // Variable Initializations
        pBody = GetComponent<Rigidbody2D>();

        // Script Initializations
        respawn = GetComponent<Respawn>();
    }

    // Update is called once per frame
    void Update()
    {
        objectAgainstWall = false;

        // Get player input
        horizontalInput = Input.GetAxis("Horizontal");
        inputDirection = GetDirection(horizontalInput);
        oppositeDirection = GetDirection(-1 * horizontalInput);

        //Checks jump key
        JumpDown();

        // Set the player's current state
        SetCurrentState();

        // Move the character
        HandleMovement(horizontalInput);

        HandleRespawn();

        HandleSurface();
    }

    // Update called at a fixed delta time
    void FixedUpdate () 
	{
        // Handles changing y velocity
        HandleJump();

        // Check if stuck 
        IsStuck();

        // Animate the character
        HandleAnimation(horizontalInput);

        previousState = currentState;
    }

    // Collects/sets info for the frame
    private void SetCurrentState()
    {
        currentState.direction = inputDirection ?? previousState.direction;
        if (Touching(inputDirection, Surface.OBJECT, grabLeniency) && (TouchingGround(Surface.GROUND)) && (Input.GetKey(KeyCode.LeftShift)))
        {
            currentState.action = Action.PUSHING; // The player is pushing an object
            currentState.grabbedObject = GrabbedObjectCast(currentState.direction, currentState.action); // Finds the object the player is currently grabbing
        }
        else if (Touching(oppositeDirection, Surface.OBJECT, grabLeniency) && (TouchingGround(Surface.GROUND)) && (Input.GetKey(KeyCode.LeftShift)))
        {
            currentState.action = Action.PULLING; // The player is pulling an object
            currentState.grabbedObject = GrabbedObjectCast(oppositeDirection, currentState.action);
        }
        else if (Touching(inputDirection, Surface.SLOPE, 0) && TouchingGround(Surface.ALL))
        {
            currentState.action = Action.UPSLOPE; // The player is walking up a slope
            currentState.grabbedObject = null;
        }
        else if ((Touching(inputDirection, Surface.ALL, 0) && !TouchingGround(Surface.ALL))
            || (Touching(inputDirection, Surface.ALL, 0) && TouchingGround(Surface.ALL))
            || objectAgainstWall)
        {
            currentState.action = Action.AGAINSTWALL; // The player is agaist a wall
            currentState.grabbedObject = null;
        }
        else
        {
            currentState.action = Action.NORMAL;
            currentState.grabbedObject = null;
        }
    }

    /// <summary>
    /// Move player based on horizontal input
    /// </summary>
    /// <param name="horizontalInput">Horizontal input</param>
    private void HandleMovement(float horizontalInput)
    {
        float distAway;
        float moveSpeed = defaultSpeed;
        switch (currentState.action)
        {
            case Action.PUSHING:
                distAway = horizontalInput * pushSpeed * Time.deltaTime;
                moveSpeed = pushSpeed;
                if (!objectAgainstWall)
                {
                    currentState.grabbedObject.transform.Translate(distAway, 0, 0);
                }
                break;
            case Action.PULLING:
                distAway = horizontalInput * pullSpeed * Time.deltaTime;
                moveSpeed = pullSpeed;
                if(!Touching(inputDirection, Surface.ALL, 0))
                {
                    currentState.grabbedObject.transform.Translate(distAway, 0, 0);
                }
                break;
            case Action.UPSLOPE:
                moveSpeed = slopeSpeed;
                break;
            case Action.AGAINSTWALL:
                moveSpeed = 0;
                break;
        }
        pBody.velocity = new Vector2(horizontalInput * moveSpeed, pBody.velocity.y);
    }

    /// <summary>
    /// Animate player based on horizontal input
    /// </summary>
    private void HandleAnimation(float horizontalInput)
    {
        moving = GetDirection(horizontalInput * pBody.velocity.x) != null;

        GetComponent<Animator>().SetInteger("Direction", (int)currentState.direction);
        GetComponent<Animator>().SetBool("Moving", moving);
        GetComponent<Animator>().SetBool("Pushing", currentState.action == Action.PUSHING);
        
        if(currentState.direction.Equals(Direction.LEFT)) {
            GetComponent<SpriteRenderer>().flipX = true;
        }
        else if(currentState.direction.Equals(Direction.RIGHT)) {
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
        if (Touching(Direction.RIGHT, Surface.ALL, 0) && Touching(Direction.LEFT, Surface.ALL, 0) && pBody.velocity.y == 0 && !TouchingGround(Surface.ALL))
        {
            pBody.velocity =  -1 * Vector2.up * 5;
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

        return IsTouching(origin, Vector2.right, distance, surface);
    }

    /// <summary>
    /// If player is touching an object, return it
    /// </summary>
    private bool Touching(Direction? direction, Surface surface, float leniency)
    {
        BoxCollider2D collider = GetComponent<BoxCollider2D>();

        // Calculate bottom of player:
        // Bottom of BoxCollider
        float playerYMin = collider.bounds.center.y - (collider.bounds.size.y / 2f) - (collider.edgeRadius / 2f);

        // Calculate distance to left edge of player:
        // Half the collider + the radius + a little
        // Left or Right determines the side of the player the ray is being shot from
        float playerXMin = (collider.bounds.size.x / 2) + (collider.edgeRadius) + .05f + leniency;

        // If checking left, subtract the distance
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

        return IsTouching(origin, Vector2.up, distance, surface);
    }

    /// <summary>
    /// Runs and returns whether the raycast hit the desired target
    /// </summary>
    private bool IsTouching(Vector2 origin, Vector2 direction, float distance, Surface surface)
    {
        if (surface == Surface.ALL)
        {
            if (ObjectCast(origin, direction, distance) != Surface.NONE)
            {
                return true;
            }
            else if (GroundCast(origin, direction, distance) != Surface.NONE)
            {
                return true;
            }
        }
        else if (ObjectCast(origin, direction, distance) == surface)
        {
            return true;
        }
        else if (GroundCast(origin, direction, distance) == surface)
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
                if ((raycast.collider.gameObject.GetComponent<SurfaceCheck>().touchingLeftWall &&  horizontalInput < 0) ||
                    raycast.collider.gameObject.GetComponent<SurfaceCheck>().touchingRightWall && horizontalInput > 0)
                {
                    objectAgainstWall = true;
                }
                return Surface.OBJECT;
            }
        }
        return Surface.NONE;
    }

    private GameObject GrabbedObjectCast(Direction? direction, Action action)
    {
        BoxCollider2D collider = GetComponent<BoxCollider2D>();

        // Calculate bottom of player:
        // Bottom of BoxCollider
        float playerYMin = collider.bounds.center.y - (collider.bounds.size.y / 2f) - (collider.edgeRadius / 2f);

        // Calculate distance to left edge of player:
        // Half the collider + the radius + a little
        // Left or Right determines the side of the player the ray is being shot from
        float playerXMin = (collider.bounds.size.x / 2) + (collider.edgeRadius) + .05f;

        // If checking left, subtract the distance
        if (direction == Direction.LEFT)
        {
            playerXMin = collider.bounds.center.x - playerXMin - grabLeniency;
        }
        else
        {
            playerXMin = collider.bounds.center.x + playerXMin + grabLeniency;
        }

        // Create vector positioned at bottom of player sprite
        Vector2 origin = new Vector2(playerXMin, playerYMin);

        float distance = collider.bounds.size.y + collider.edgeRadius;

        RaycastHit2D raycast = Physics2D.Raycast(origin, Vector2.up, distance, LayerMask.GetMask("Object"));
        return raycast.collider.gameObject;
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
        // respawn.manualRespawn();
        respawn.manualReset();
    }
    private void HandleSurface()
    {
        pickSurface();
    }

    public void pickSurface()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            equippedMaterial = SurfaceChange.material.BOUNCE;
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            equippedMaterial = SurfaceChange.material.SLIP;
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            equippedMaterial = SurfaceChange.material.STICK;
        }
    }
}