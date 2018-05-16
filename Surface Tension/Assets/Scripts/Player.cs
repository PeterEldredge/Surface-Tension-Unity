using System.Collections;
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
    IsGrounded isGroundedShortcut;

    //Initializes shortcut to WallCheck script on both RightCheck and LeftCheck
    WallCheck rWallCheckShortcut;
    WallCheck lWallCheckShortcut;

    float movement;

    void Awake()
    {
        pBody = GetComponent<Rigidbody2D>();

        //Is the player on the ground
        isGroundedShortcut = GameObject.Find("GroundCheck").gameObject.GetComponent<IsGrounded>();

        rWallCheckShortcut = GameObject.Find("RightCheck").GetComponent<WallCheck>();
        lWallCheckShortcut = GameObject.Find("LeftCheck").GetComponent<WallCheck>();
    }

    // Update is called once per frame
    void Update () 
	{
        //Determinesif the player will move and at what speed
        movement = Input.GetAxis("Horizontal") * moveSpeed;

        //Ensures player cannot get stuck on walls by preventing velocity towards a wall when directly next to it
        if ((movement > 0 && !rWallCheckShortcut.isNextToWall) || (movement < 0 && !lWallCheckShortcut.isNextToWall) || (isGroundedShortcut.isGrounded && !Input.GetButtonDown("Jump")))
        {
            if ((Input.GetButtonDown("Jump") && movement > 0 && rWallCheckShortcut.isNextToWall) || (Input.GetButtonDown("Jump") && movement < 0 && lWallCheckShortcut.isNextToWall))
            {
                pBody.velocity = new Vector2(0, pBody.velocity.y);
            } else
            {
                pBody.velocity = new Vector2(movement, pBody.velocity.y);
            }
        }
        else
        {
            pBody.velocity = new Vector2(0, pBody.velocity.y); //?
        }  

        //Jump
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
}
