using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Player : MonoBehaviour 
{
    public GameController.SurfaceSpeeds surfaceSpeeds;

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
    public float grabRaycastLeniency;

    /// <summary>
    /// Padding value applied when performing ground raycast
    /// </summary>
    public float groundRaycastLeniency;

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
    /// If true, the player is currently holding the grab button
    /// </summary>
    private bool grabbing = false;

    /// <summary>
    /// If true, maintain the player's horizontal velocity
    /// </summary>
    private bool maintainVelocity = false;

    /// <summary>
    /// The material of the last ground surface interacted with
    /// </summary>
    private GameController.material lastGroundMat = GameController.material.NONE;

    /// <summary>
    /// Holds the current state of the player
    /// </summary>
    private State currentState;

    /// <summary>
    /// Holds the state of the player from the last frame
    /// </summary>
    private State previousState;

    /// <summary>
    /// The player's currently equipped material
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

        public float defaultSpeed; // The speed the player walks on a normal surface
        public float pushSpeed; // The speed the player pushes
        public float pullSpeed; // The speed the player pulls
        public float slopeSpeed; // The speed the player walks up slopes

        public Vector2 velocity; // Stores the player's velocity at the end of the frame
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
    }

    // Update is called once per frame
    void Update()
    {
        objectAgainstWall = false;

        HandleInput();
    }

    // Update called at a fixed delta time
    void FixedUpdate () 
	{
        objectAgainstWall = false;

        // Update data in currentState
        UpdateState();

        HandleMovement(horizontalInput);

        HandleAnimation();

        // Bounce if landed on bouncy surface
        BounceCheck();

        currentState.velocity = pBody.velocity;
        previousState = currentState;

        // Handles changing y velocity
        HandleJump();

        // Check if stuck 
        IsStuck();
    }

    /// <summary>
    /// Updates data stored in currentState
    /// </summary>
    private void UpdateState()
    {
        // Set the player's current surrondings
        SetCurrentSurroundings();

        // Set the player's current action
        SetCurrentAction();

        // Set player move speeds depending on ground surface
        InitializeSurfaceSpeeds();
    }

    /// <summary>
    /// Handle player input
    /// </summary>
    private void HandleInput()
    {
        // Check if player pressed Respawn button
        respawn.HandleRespawn();

        // Get player input (horizontal)
        horizontalInput = Input.GetAxis("Horizontal");
        currentState.direction = GetDirection(horizontalInput) ?? previousState.direction;
        currentState.oppDirection = GetDirection(-1 * horizontalInput) ?? previousState.oppDirection;

        // Grab object
        grabbing = Input.GetButton("Grab");

        // Checks jump key
        JumpDown();

        // Update equipped surface based on input
        EquipSurface();
    }

    /// <summary>
    /// Performs raycasts to check surfaces player is contacting, and stores those GameObjects in currentState
    /// </summary>
    private void SetCurrentSurroundings()
    {
        float appliedGrabLeniency = 0;     

        // If the player is currently holding the grab button, apply leniency to the object cast check
        if (grabbing) {
            appliedGrabLeniency = grabRaycastLeniency;
        }

        // Check for left/right collisions on Objects layer
        currentState.objFaceDir = RayCheck(currentState.direction, "Object", appliedGrabLeniency);
        currentState.objOppDir = RayCheck(currentState.oppDirection, "Object", appliedGrabLeniency);

        // Check for left/right collisions on Ground layer
        currentState.surfFaceDir = RayCheck(currentState.direction, "Ground", 0);
        currentState.surfOppDir = RayCheck(currentState.oppDirection, "Ground", 0);

        // Check surface below player on both layers
        currentState.surfGround = RayCheck(Direction.DOWN, null, groundRaycastLeniency);
    }

    /// <summary>
    /// Collects/sets action for the frame
    /// </summary>
    private void SetCurrentAction()
    {
        // Pushing
        if (currentState.objFaceDir && (GetTag(currentState.surfGround) == "Slope" || GetTag(currentState.surfGround) == "Ground") && grabbing && horizontalInput != 0)
        {
            currentState.action = Action.PUSHING;
            
            // Get pushed object
            currentState.grabbedObject = currentState.objFaceDir; 
        }
        // Pulling
        else if (currentState.objOppDir && (GetTag(currentState.surfGround) == "Ground") && grabbing && horizontalInput != 0)
        {
            currentState.action = Action.PULLING;
            
            // Get pulled object
            currentState.grabbedObject = currentState.objOppDir;
        }
        // Going up slope
        else if (GetTag(currentState.surfFaceDir) == "Slope" && currentState.surfGround)
        {
            currentState.action = Action.UPSLOPE;
            currentState.grabbedObject = null;
        }
        // Against wall
        else if (currentState.objFaceDir || currentState.surfFaceDir)
        {
            currentState.action = Action.AGAINSTWALL;
            currentState.grabbedObject = null;
        }
        // Normal
        else
        {
            currentState.action = Action.NORMAL;
            currentState.grabbedObject = null;
        }

        // Check if pushing object against wall
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
    /// If grounded on a surface, assign its surfaceSpeeds to the player movement speeds
    /// </summary>
    private void InitializeSurfaceSpeeds()
    {
        SurfaceMaterial groundSurface;
        if (currentState.surfGround != null && LayerMask.LayerToName(currentState.surfGround.layer) == "Ground") {
            groundSurface = currentState.surfGround.GetComponent<SurfaceMaterial>();
            surfaceSpeeds = groundSurface.surfaceSpeeds;

            if (groundSurface.type == GameController.material.BOUNCE)
            {
                maintainVelocity = true;
            }
            else if (groundSurface.type == GameController.material.NONE)
            {
                maintainVelocity = false;
            }

            currentState.defaultSpeed = surfaceSpeeds.defaultSpeed;
            currentState.pushSpeed = surfaceSpeeds.pushSpeed;
            currentState.pullSpeed = surfaceSpeeds.pullSpeed;
            currentState.slopeSpeed = surfaceSpeeds.upSlopeSpeed;
        }
    }

    /// <summary>
    /// Trigger a bounce if player lands on bouncy surface
    /// </summary>
    private void BounceCheck()
    {
        if (GetMaterial(currentState.surfGround) == GameController.material.BOUNCE &&
            !previousState.surfGround && 
            Mathf.Abs(previousState.velocity.y) > 4f)
        {
            lastGroundMat = GameController.material.BOUNCE;
            pBody.velocity = new Vector2(pBody.velocity.x, Mathf.Sqrt((upGravity * previousState.velocity.y * previousState.velocity.y) / (downGravity)) + 1f);
        }
    }

    /// <summary>
    /// Move player based on horizontal input
    /// </summary>
    /// <param name="horizontalInput">Horizontal input</param>
    private void HandleMovement(float horizontalInput)
    {
        float distAway;
        float moveSpeed = currentState.defaultSpeed;
        switch (currentState.action)
        {
            case Action.PUSHING:
                moveSpeed = currentState.pushSpeed;
                if (!objectAgainstWall)
                {
                    currentState.grabbedObject.GetComponent<Rigidbody2D>().velocity = new Vector2(moveSpeed * horizontalInput, 0);
                }
                break;
            case Action.PULLING:
                distAway = horizontalInput * currentState.pullSpeed * Time.fixedDeltaTime;
                moveSpeed = currentState.pullSpeed;
                if(!currentState.surfFaceDir && !currentState.objFaceDir)
                {
                    currentState.grabbedObject.transform.Translate(distAway, 0, 0);
                }
                break;
            case Action.UPSLOPE:
                moveSpeed = currentState.slopeSpeed;
                break;
            case Action.AGAINSTWALL:
                moveSpeed = 0;
                break;
        }
        if (moveSpeed < Mathf.Abs(previousState.velocity.x) && maintainVelocity) //&& currentState.action != Action.AGAINSTWALL)
        {
            moveSpeed = previousState.velocity.x;
        }
        else
        {
            maintainVelocity = false;
        }
        pBody.velocity = new Vector2(horizontalInput * moveSpeed, pBody.velocity.y);
    }

    /// <summary>
    /// Animate player based on input
    /// </summary>
    private void HandleAnimation()
    {
        moving = GetDirection(horizontalInput * pBody.velocity.x) != null;
        bool pushing = currentState.action == Action.PUSHING;
        bool pulling = currentState.action == Action.PULLING;

        bool jumping = pBody.velocity.y > 2f;
        bool falling = pBody.velocity.y < -2f;
        
        if (!jumping && !falling && !currentState.surfGround)
        {
            jumping = true;
        }

        GetComponent<Animator>().SetInteger("Direction", (int)currentState.direction);
        GetComponent<Animator>().SetBool("Moving", moving);
        GetComponent<Animator>().SetBool("Pushing", pushing);
        GetComponent<Animator>().SetBool("Pulling", pulling);
        GetComponent<Animator>().SetBool("Jumping", jumping);
        GetComponent<Animator>().SetBool("Falling", falling);
        
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
        if (Input.GetButtonDown("Jump") && currentState.surfGround)
        {
            applyMaxUpwards = true;
        }
        // Variable jump RIP
        /*if (Input.GetButtonUp("Jump") && lastGroundMat != GameController.material.BOUNCE)
        {
            applyMinUpwards = true;
        }*/
    }

    /// <summary>
    /// Applies velocity in the y direction to the player and sets the velocity of the player's fall
    /// </summary>
    private void HandleJump()
    {
        if (applyMaxUpwards && currentState.surfGround)
        {
            if (GetMaterial(currentState.surfGround) == GameController.material.BOUNCE)
            {
                // If jumping on a bouncy surface, double maxHeightVelocity
                pBody.velocity = new Vector2(pBody.velocity.x, 1.4f * maxHeightVelocity);
            }
            else
            {
                pBody.velocity = new Vector2(pBody.velocity.x, maxHeightVelocity);
            }
            applyMaxUpwards = false;
        }
        // Variable jump RIP
        /*else if (applyMinUpwards)
        {
            if (minHeightVelocity < pBody.velocity.y)
            {
                pBody.velocity = new Vector2(pBody.velocity.x, minHeightVelocity);
            }
            applyMinUpwards = false;
        }*/

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
    /// If player is stuck above ground, applies the jump velocity downwards to dislodge player
    /// </summary>
    private void IsStuck()
    {
        if ((currentState.surfFaceDir || currentState.objFaceDir) && (currentState.surfOppDir || currentState.objOppDir) && Mathf.Abs(pBody.velocity.y) <= .1f && !currentState.surfGround)
        {
            pBody.velocity =  -1 * Vector2.up * 8;
        }
    }

    /// <summary>
    /// Performs raycast alongside player collider
    /// </summary>
    /// <param name="direction">Direction of raycast</param>
    /// <param name="layerMaskName">Layer to check for collision</param>
    /// <param name="leniency">Leniency applied for ground/object raycasts (zero unless direction = down or pressing Grab)</param>
    /// <returns>Returns: Any GameObject it collides with</returns>
    private GameObject RayCheck(Direction direction, string layerMaskName, float leniency)
    {
        // Raycast parameters
        float rayDistance;
        Vector2 rayOrigin;
        Vector2 rayDirection;
        RaycastHit2D raycast;

        // X,Y points used to calculate raycast origin
        float playerBottom;
        float originXPos;

        BoxCollider2D collider = GetComponent<BoxCollider2D>();

        if (direction == Direction.DOWN) {
            // Calculate bottom of player:
            // Bottom of BoxCollider + edgeRadius around collider + leniency (subtraction because in downward direction)
            playerBottom = collider.bounds.min.y - collider.edgeRadius - leniency;

            // Calculate left edge of player (don't need leniency here):
            // Edge of BoxCollider + 1/2 of edgeRadius
            // Starts on the side opposite the direction the player is facing
            if (currentState.direction == Direction.RIGHT)
            {
                originXPos = collider.bounds.min.x - (collider.edgeRadius / 2);
                rayDirection = Vector2.right;
            } else
            {
                originXPos = collider.bounds.max.x + (collider.edgeRadius / 2);
                rayDirection = Vector2.left;
            }

            // Distance = width + edge radius
            rayDistance = collider.bounds.size.x + collider.edgeRadius;
        }
        else {
            // Calculate bottom of player (don't need leniency here):
            // Bottom of BoxCollider
            playerBottom = collider.bounds.min.y - (collider.edgeRadius / 2f);

            // Calculate distance to left edge of player:
            // Half the collider + the radius + leniency
            originXPos = (collider.bounds.size.x / 2) + (collider.edgeRadius) + .05f + leniency;

            // Left or Right determines the side of the player the ray is being shot from
            if (direction == Direction.LEFT) {
                originXPos = collider.bounds.center.x - originXPos;
            }
            else {
                originXPos = collider.bounds.center.x + originXPos;
            }
            
            rayDirection = Vector2.up;

            // Distance = height + edge radius
            rayDistance = collider.bounds.size.y + collider.edgeRadius;
        }

        // Calculate raycast origin
        rayOrigin = new Vector2(originXPos, playerBottom);

        // Perform raycast (leave out layermask if null)
        if(layerMaskName != null) {
            raycast = Physics2D.Raycast(rayOrigin, rayDirection, rayDistance, LayerMask.GetMask(layerMaskName));
        }
        else {
            raycast = Physics2D.Raycast(rayOrigin, rayDirection, rayDistance);
        }
        Debug.DrawRay(rayOrigin, rayDirection * rayDistance, Color.magenta);
        
        
        // Check for collision
        if (raycast.collider != null) {
            if (direction == Direction.DOWN && layerMaskName != "Object") {
                lastGroundMat = GetMaterial(raycast.collider.gameObject);
            }

            return raycast.collider.gameObject;
        }
        else {
            return null;
        }
    }

    //Function to hold surface changing code
    private void EquipSurface()
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

    /// <summary>
    /// Gets the material of a Game Object
    /// </summary>
    private GameController.material GetMaterial(GameObject gameObject)
    {
        if (gameObject)
        {
            if (gameObject.layer == LayerMask.NameToLayer("Ground"))
            {
                return gameObject.GetComponent<SurfaceMaterial>().type;
            }
        }
        return GameController.material.NONE;
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
}