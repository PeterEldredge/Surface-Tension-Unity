using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SurfaceCheck : MonoBehaviour {

    /// <summary>
    /// Definition for direction player is facing
    /// </summary>
    public enum Direction
    {
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

    public bool touchingLeftWall = false;
    public bool touchingRightWall = false;

    // Update is called once per frame
    void Update () {
        touchingLeftWall = Touching(Direction.LEFT, Surface.GROUND);
        touchingRightWall = Touching(Direction.RIGHT, Surface.GROUND);
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
        }
        else
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
}
