using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SurfaceCheck : MonoBehaviour {

    /// <summary>
    /// Definition for direction object is facing
    /// </summary>
    public enum Direction
    {
        LEFT = -1,
        RIGHT = 1
    };

    /// <summary>
    /// Definition for surface the object is touching
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
        touchingLeftWall = Touching(Direction.LEFT, Surface.ALL);
        touchingRightWall = Touching(Direction.RIGHT, Surface.ALL);
	}

    /// <summary>
    /// Returns whether object is touching ground
    /// </summary>
    private bool TouchingGround(Surface surface)
    {
        BoxCollider2D collider = GetComponent<BoxCollider2D>();

        // Calculate bottom of object:
        // Bottom of BoxCollider + edgeRadius around collider (subtraction because in downward direction)
        float objectHeight = collider.bounds.size.y;
        float objectBottom = collider.bounds.center.y - (objectHeight / 2F) - collider.edgeRadius - .05f;

        // Calculate left edge of object:
        // Left edge of BoxCollider + 1/2 of edgeRadius (subtraction because in leftward direction)
        float objectXMin = collider.bounds.center.x - (collider.bounds.size.x / 2) - (collider.edgeRadius / 2);

        // Create vector positioned at bottom of object sprite
        Vector2 origin = new Vector2(objectXMin, objectBottom);

        float distance = collider.bounds.size.x + collider.edgeRadius;

        return IsTouching(origin, Vector2.up, distance, surface);
    }

    /// <summary>
    /// If object is touching an object, return it
    /// </summary>
    private bool Touching(Direction? direction, Surface surface)
    {
        BoxCollider2D collider = GetComponent<BoxCollider2D>();

        // Calculate bottom of object:
        // Bottom of BoxCollider
        float objectYMin = collider.bounds.center.y - (collider.bounds.size.y / 2f) - (collider.edgeRadius / 2f);

        // Calculate distance to left edge of object:
        // Half the collider + the radius + a little
        // Left or Right determines the side of the object the ray is being shot from
        float objectXMin = (collider.bounds.size.x / 2) + (collider.edgeRadius) + .03f;
        if (direction == Direction.LEFT)
        {
            objectXMin = collider.bounds.center.x - objectXMin;
        }
        else
        {
            objectXMin = collider.bounds.center.x + objectXMin;
        }

        // Create vector positioned at bottom of object sprite
        Vector2 origin = new Vector2(objectXMin, objectYMin);

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
