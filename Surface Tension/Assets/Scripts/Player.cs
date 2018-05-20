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
    /// Rate of change of speed (based on player input)
    /// </summary>
    public float acceleration;

    //Velocity applied to player on jump
    public float jumpSpeed;

    //Affects the gravity of the player as they fall
    public float fallMultiplyer;

    //Affects the gravity of the player as they jump
    public float jumpMultiplyer;

    //Initializes pBody, this will be the player's Rigidbody2D component 
    Rigidbody2D pBody;

    //Initializes shortcut to WallCheck script on both RightCheck and LeftCheck
    public WallCheck rWallCheckShortcut;
    public WallCheck lWallCheckShortcut;

    /// <summary>
    /// Definition for direction player is facing
    /// </summary>
    public enum Direction {
        LEFT = -1,
        RIGHT = 1
    };

    /// <summary>
    /// Direction player is currently facing
    /// </summary>
    public Direction direction;

    /// <summary>
    /// True if player is currently moving
    /// </summary>
    protected bool moving;

    private Respawn respawn;

    void Awake()
    {
        pBody = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void FixedUpdate () 
	{
        // Get player input
        float horizontalInput = Input.GetAxis("Horizontal");

        // Move and animate character
        HandleMovement(horizontalInput);
        HandleAnimation(horizontalInput);
        HandleRespawn();

        // Check if stuck
        IsStuck();  
        TouchingGround();
	}

    void Update()
    {
        HandleJump();
    }

    /// <summary>
    /// Move player based on horizontal input
    /// </summary>
    /// <param name="horizontalInput">Horizontal input</param>
    private void HandleMovement(float horizontalInput) 
    {
        Vector2 newVelocity = pBody.velocity + new Vector2(horizontalInput * acceleration * Time.deltaTime, 0);
        /* 
        if(GetDirection(horizontalInput) != null) {
            pBody.sharedMaterial.friction = 0;
        }
        else {
            pBody.sharedMaterial.friction = .4F;
        }
*/
        if(Mathf.Abs(newVelocity.x) <= maxSpeed && !TouchingBlock(GetDirection(horizontalInput))) {
            pBody.velocity = newVelocity;
        }
        else if(TouchingGround() && TouchingBlock(GetDirection(horizontalInput))) {
            Debug.Log("pushing against block");
            pBody.velocity = new Vector2(horizontalInput * pushSpeed, pBody.velocity.y);
        }
        else if (TouchingGround()) {
            pBody.velocity = new Vector2(horizontalInput * maxSpeed, pBody.velocity.y);
        }

        // Debug.Log("Velocity: " + pBody.velocity);
        


        //Ensures player cannot get stuck on walls by preventing velocity towards a wall when directly next to it
        // If moving away from wall or not jumping
        // if ((GetDirection(movement) == Direction.RIGHT && !TouchingWall(Direction.RIGHT)) 
        // || (GetDirection(movement) == Direction.LEFT && !TouchingWall(Direction.LEFT)) 
        // || (TouchingGround() && !Input.GetButtonDown("Jump")))
        // {
        //     if (!(Input.GetButtonDown("Jump") 
        //     && GetDirection(movement) == Direction.RIGHT 
        //     && TouchingWall(Direction.RIGHT)) 

        //     || !(Input.GetButtonDown("Jump") 
        //     && GetDirection(movement) == Direction.LEFT 
        //     && TouchingWall(Direction.LEFT)))
        //     {
        //         pBody.velocity = new Vector2(movement, pBody.velocity.y);
        //     }
        // }
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
    /// Player jumps if conditions are met
    /// </summary>
    private void HandleJump() {
        if (Input.GetButtonDown("Jump") && TouchingGround())
        {
            pBody.velocity += Vector2.up * jumpSpeed;
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
        if (TouchingBlock(Direction.RIGHT) && TouchingBlock(Direction.LEFT) && pBody.velocity.y == 0 && !TouchingGround())
        {
            pBody.velocity =  -1 * Vector2.up * jumpSpeed;
        }
    }

    /// <summary>
    /// If player is touching a wall, return what side it's on
    /// </summary>
    private bool TouchingBlock(Direction? direction)
    {
        if(direction.Equals(Direction.RIGHT) && rWallCheckShortcut.isNextToWall) {
            return true;
        }
        else if(direction.Equals(Direction.LEFT) && lWallCheckShortcut.isNextToWall) {
            return true;
        }
        else return false;
    }

    /// <summary>
    /// Returns whether player is touching ground
    /// </summary>
    private bool TouchingGround() 
    {   
        // Calculate bottom of player:
        // Bottom of BoxCollider + edgeRadius around collider (subtraction because in downward direction)
        float playerHeight = GetComponent<BoxCollider2D>().bounds.size.y;
        float playerBottom = GetComponent<BoxCollider2D>().bounds.center.y - (playerHeight / 1.9F) - GetComponent<BoxCollider2D>().edgeRadius;

        // Create vector positioned at bottom of player sprite
        Vector2 origin = new Vector2(transform.position.x, playerBottom);

        // Calculate distance raycast will check for ground collision
        float distance = (playerHeight / 200);

        // Create raycast from bottom of player down towards ground
        RaycastHit2D raycast = Physics2D.Raycast(origin, Vector2.down, distance);
        if(raycast.collider != null) {
            // Debug.Log("Raycast collided with " + raycast.collider.gameObject.name);

            // Check if raycast hit ground
            if(raycast.collider.gameObject.layer == LayerMask.NameToLayer("Ground")) {
                return true;
            }
        }
        
        // return false if raycast didn't hit ground
        return false;
    }

    // If R is pressed, the player will respawn at the position of empty game object "Spawn Point"
    private void HandleRespawn()
    {
        respawn = GetComponent<Respawn>();
        respawn.manualRespawn();
    }
}
