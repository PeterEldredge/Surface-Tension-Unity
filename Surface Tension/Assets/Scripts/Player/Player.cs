using System;
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

    /// <summary>
    /// The horizontal input from the player every frame
    /// </summary>
    float horizontalInput;

    /// <summary>
    /// The velocity that will get the player to their max jump height
    /// </summary>
    public float maxHeightVelocity;

    /// <summary>
    /// The velocity that will get the player to their min jump height/how high they will go after releasing the jump button
    /// </summary>
    public float minHeightVelocity;

    /// <summary>
    /// The gravity scale that affects the player when their rigidbody's y velocity is greater than 0
    /// </summary>
    public float upGravity;

    /// <summary>
    /// The gravity scale that affects the player when their rigidbody's y velocity is less than 0
    /// </summary>
    public float downGravity;

    /// <summary>
    /// An additional value added on to the origin of a ray cast to make grab distances a little more lenient in certain cases
    /// </summary>
    public float grabLeniency;

    /// <summary>
    /// If true, on the next fixed frame the maxHeightVelocity will be applied to the player
    /// Changes to true when jump button is pushed, false after velocity has been set.
    /// </summary>
    private bool applyMaxUpwards = false;

    /// <summary>
    /// If true, on the next fixed frame the minHeightVelocity will be applied to the player
    /// Changes to true when jump button is released, false after velocity has been set
    /// </summary>
    private bool applyMinUpwards = false;

    /// <summary>
    /// If true, the object the player is currently running into is against a wall
    /// </summary>
    private bool objectAgainstWall = false;

    /// <summary>
    /// If true, the object the player is currently holding the grab button
    /// </summary>
    private bool grabbing = false;

    /// <summary>
    /// Holds the current state of the player
    /// </summary>
    private State currentState;

    /// <summary>
    /// Holds the state of the player from the last frame
    /// </summary>
    private State previousState;

    /// <summary>
    /// The player's currently equiped material
    /// </summary>
    public GameController.material equippedMaterial;

    //Initializes pBody, this will be the player's Rigidbody2D component 
    Rigidbody2D pBody;

    /// <summary>
    /// Definition for direction player is facing
    /// </summary>
    public enum Direction {
        LEFT = -1,
        RIGHT = 1,
        DOWN = 2
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
        public Direction oppDirection;
        public GameObject grabbedObject;
        
        public GameObject objFaceDir; // The object the player is facing
        public GameObject surfFaceDir; // The surface the player is facing    
        public GameObject objOppDir; // The object opposite the side the player is facing
        public GameObject surfOppDir; // The surface opposite the side the player is facing   
        public GameObject surfGround; // The game object on the ground
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

        // Initializes equipped material to "bounce"
        equippedMaterial = GameController.material.BOUNCE;
    }

    // Update is called once per frame
    void Update()
    {
        //Cleans up all variables
        CleanUp();

        //Gets all player inputs
        GetInput();

        // Set the player's current surrondings
        SetCurrentSurroundings();

        // Set the player's current action
        SetCurrentAction();

        // Move the character
        HandleMovement(horizontalInput);

        HandleRespawn();

        HandleSurface();
    }

    // Update called at a fixed delta time
    void FixedUpdate () 
	{
        SetTopSpeed();
        
        // Handles changing y velocity
        HandleJump();

        // Check if stuck 
        IsStuck();

        // Animate the character
        HandleAnimation();

        previousState = currentState;
    }

    /// <summary>
    /// Sets defaultSpeed to 
    /// </summary>
    private void SetTopSpeed()
    {
        // TODO: get top speed from surface player is standing on, and assign to player
    }

    /// <summary>
    /// Cleans up changed variables from the previous frame
    /// </summary>
    private void CleanUp()
    {
        objectAgainstWall = false;
        grabbing = false;
    }

    /// <summary>
    /// Gets the tag of a Game Object
    /// </summary>
    private string GetTag(GameObject gameObject)
    {
        if (gameObject != null)
        {
            return gameObject.tag;
        }
        else
        {
            return null;
        }
    }

    private void GetInput()
    {
        // Get player input (horizontal)
        horizontalInput = Input.GetAxis("Horizontal");
        currentState.direction = GetDirection(horizontalInput) ?? previousState.direction;
        currentState.oppDirection = GetDirection(-1 * horizontalInput) ?? previousState.oppDirection;

        grabbing = Input.GetButton("Grab");

        //Checks jump key
        JumpDown();
    }

    private void SetCurrentSurroundings()
    {
        float appliedLeniency = 0;     

        if (grabbing) // If the player is currently holding the grab button, apply leniency to the object cast check
        {
            appliedLeniency = grabLeniency;
        }
        currentState.objFaceDir = rayCheck(currentState.direction, "Object", appliedLeniency);
        currentState.objOppDir = rayCheck(currentState.oppDirection, "Object", appliedLeniency);

        currentState.surfFaceDir = rayCheck(currentState.direction, "Ground", 0);
        currentState.surfOppDir = rayCheck(currentState.oppDirection, "Ground", 0);

        currentState.surfGround = rayCheck(Direction.DOWN, null, 0);
    }

    // Collects/sets action for the frame
    private void SetCurrentAction()
    {
        if (currentState.objFaceDir && (GetTag(currentState.surfGround) == "Slope" || GetTag(currentState.surfGround) == "Ground") && grabbing)
        {
            currentState.action = Action.PUSHING; // The player is pushing an object
            currentState.grabbedObject = currentState.objFaceDir; // Finds the object the player is currently grabbing
        }
        else if (currentState.objOppDir && (GetTag(currentState.surfGround) == "Ground") && grabbing)
        {
            currentState.action = Action.PULLING; // The player is pulling an object
            currentState.grabbedObject = currentState.objOppDir;
        }
        else if (GetTag(currentState.surfFaceDir) == "Slope" && currentState.surfGround)
        {
            currentState.action = Action.UPSLOPE; // The player is walking up a slope
            currentState.grabbedObject = null;
        }
        else if ((currentState.objFaceDir || currentState.surfFaceDir) || objectAgainstWall)
        {
            currentState.action = Action.AGAINSTWALL; // The player is agaist a wall
            currentState.grabbedObject = null;
        }
        else
        {
            currentState.action = Action.NORMAL;
            currentState.grabbedObject = null;
        }

        //Sets if the object is against a wall in the direction the player is pushing it
        if (currentState.action == Action.PUSHING)
        {
            if (currentState.direction == Direction.LEFT)
            {
                objectAgainstWall = currentState.grabbedObject.GetComponent<SurfaceCheck>().touchingLeftWall;
            }
            else if (currentState.direction == Direction.RIGHT)
            {
                objectAgainstWall = currentState.grabbedObject.GetComponent<SurfaceCheck>().touchingRightWall;
            }
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
                if(!currentState.surfFaceDir && !currentState.objFaceDir)
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
    private void HandleAnimation()
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
    /// Sets when the player presses the jump key and when they release it
    /// </summary>
    private void JumpDown()
    {
        if (Input.GetButtonDown("Jump") && currentState.surfGround)// && currentState.action != Action.UPSLOPE)
        {
            applyMaxUpwards = true;
        }
        if (Input.GetButtonUp("Jump"))
        {
            applyMinUpwards = true;
        }
    }

    /// <summary>
    /// Applies velocity in the y direction to the player and sets the velocity of the player's fall
    /// </summary>
    private void HandleJump()
    {
        if (applyMaxUpwards && currentState.surfGround)
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

        if (!currentState.surfGround)
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
        if ((currentState.surfFaceDir || currentState.objFaceDir) && (currentState.surfOppDir || currentState.objOppDir) && Mathf.Abs(pBody.velocity.y) <= .1f && !currentState.surfGround)
        {
            pBody.velocity =  -1 * Vector2.up * 8;
        }
    }

    private GameObject rayCheck(Direction direction, string layerMaskName, float leniency)
    {
        float distance;
        Vector2 origin;
        Vector2 rayDirection;
        RaycastHit2D raycast;

        if (direction != Direction.DOWN) // Means that the direction is to the sides, so we do a raycast starting from the bottom of the desired side that shoots upwards
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
            }
            else
            {
                playerXMin = collider.bounds.center.x + playerXMin;
            }

            // Create vector positioned at bottom of player sprite
            origin = new Vector2(playerXMin, playerYMin);

            rayDirection = Vector2.up;

            distance = collider.bounds.size.y + collider.edgeRadius;
            
            raycast = Physics2D.Raycast(origin, rayDirection, distance, LayerMask.GetMask(layerMaskName));
        }
        else
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
            origin = new Vector2(playerXMin, playerBottom);

            rayDirection = Vector2.right;

            distance = collider.bounds.size.x + collider.edgeRadius;

            raycast = Physics2D.Raycast(origin, rayDirection, distance);
        }
        Debug.DrawRay(origin, rayDirection * distance, Color.magenta);
        if (raycast.collider != null)
        {
            return raycast.collider.gameObject;
        }
        else
        {
            return null;
        }
    }

    // If R is pressed, the player will respawn at the position of empty game object "Spawn Point"
    private void HandleRespawn()
    {
        // respawn.manualRespawn();
        respawn.manualReset();
    }

    //Function to hold surface changing code
    private void HandleSurface()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            equippedMaterial = GameController.material.BOUNCE;
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            equippedMaterial = GameController.material.SLIP;
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            equippedMaterial = GameController.material.STICK;
        }
    }
}