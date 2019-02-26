using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Note to self, the controller should just make use of the collision functionality when needed
// So this collision script won't use an update method

public class Collision : MonoBehaviour
{
    // Collisions can be inspected publicly, but set only within class
    [SerializeField] public CollisionInfo Collisions;

    // Other hidden fields below

    public LayerMask collisionMask;

    // Thinking ahead, if we have pixel art, if the skin width is small enough
    // (e.g. 1/4 of a pixel), then we won't have to worry about imprecision in the collisions,
    // because all colliding objects will render on the same pixel line
    protected const float skinWidth = 0.01f;

    [SerializeField] protected int horizontalRayCount = 4;
    [SerializeField] protected int verticalRayCount = 4;

    protected float horizontalRaySpacing;
    protected float verticalRaySpacing;

    [HideInInspector]
    protected RaycastOrigins raycastOrigins;

    BoxCollider2D collider2d;

    public virtual void Start()
    {
        collider2d = GetComponentInParent<BoxCollider2D>();
        Collisions = new CollisionInfo();
        CalculateRaySpacing();
    }

    // A candidate movement vector is given to this function to detect Collisions
    public CollisionInfo DetectCollisions(ref Vector3 velocity)
    {
        Collisions.Reset();
        UpdateRaycastOrigins();
        CalculateRaySpacing();
        // TODO don't check if not moving on axis?

        // TODO actually, I think I need to check all axes on all frames, but some just check if something is touching (within skin)
        DetectHorizontalCollisions(ref velocity);
        DetectVerticalCollisions(ref velocity);
        return Collisions;
    }

    void DetectHorizontalCollisions(ref Vector3 velocity)
    {
        // Detect collisions in all directions
        // Either skin width or movement length

        // TODO direct skinwidth min in all directions

        for (int directionX = -1; directionX <= 1; directionX += 2)
        {
            float moveVelocity = (Mathf.Sign(velocity.x) == directionX) ? Mathf.Abs(velocity.x) : 0;
            float rayLength = moveVelocity + skinWidth * 2;
            for (int i = 0; i < horizontalRayCount; i++)
            {
                Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
                //rayOrigin.x += skinWidth * ((directionX == -1) ? 1 : -1);
                rayOrigin += Vector2.up * (horizontalRaySpacing * i);

                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);

                Debug.DrawRay(rayOrigin, Vector2.right * directionX * rayLength, Color.red);

                if (hit)
                {
                    if (moveVelocity > 0)
                    {
                        velocity.x = (hit.distance - skinWidth) * directionX;
                        rayLength = hit.distance;

                        //Vector3 oldVel = velocity;

                        //// DEBUG
                        //Vector3 point = new Vector3(rayOrigin.x, rayOrigin.y, 0) + velocity;

                        //if (GameObject.Find("Surface").GetComponent<BoxCollider2D>().bounds.Contains(point))
                        //{
                        //    Vector2 mover = new Vector2(rayOrigin.x, (rayOrigin.y + 0.25f));

                        //    Debug.DrawRay(rayOrigin, Vector2.right * directionX * rayLength, Color.red);

                        //    Debug.DrawRay(mover, Vector2.right * directionX * (hit.distance - skinWidth), Color.green);
                        //    Debug.Log("Point in Surface");

                        //    // printouts
                        //    Debug.Log("rayOrigin             = (" + rayOrigin.x + ", " + rayOrigin.y + ")");
                        //    // manually calc.
                        //    Debug.Log("Proper ray origin     = (" + (gameObject.transform.position.x + 0.5 - skinWidth) + ", " + (gameObject.transform.position.y - 0.5 - skinWidth) + ")");

                        //    Debug.Log("Velocity.x (original) = " + velocity.x);
                        //    Debug.Log("Velocity.x (updated)  = " + (hit.distance - skinWidth) * directionX);
                        //    Debug.Log("hit.distance          = " + hit.distance);
                        //    Debug.Log("hit.distance - skinWidth = " + (hit.distance - skinWidth));
                        //    Debug.Log("directionX            = " + directionX);
                        //    Debug.Break();
                        //}
                        ////else
                        ////Debug.Log("Point not in Surface");
                        //for (int p = 1; p < 4; p++)
                        //{
                        //    if (GameObject.Find("Surface (" + p + ")").GetComponent<BoxCollider2D>().bounds.Contains(point))
                        //    {
                        //        Debug.DrawRay(rayOrigin, Vector2.right * directionX * rayLength, Color.red);

                        //        Vector2 mover = new Vector2(rayOrigin.x, (rayOrigin.y + 0.25f));

                        //        Debug.DrawRay(mover, Vector2.right * directionX * (hit.distance - skinWidth), Color.green);
                        //        Debug.Log("Point in Surface" + p);

                        //        // printouts
                        //        Debug.Log("rayOrigin             = (" + rayOrigin.x + ", " + rayOrigin.y + ")");
                        //        // manually calc.
                        //        Debug.Log("Proper ray origin     = (" + (gameObject.transform.position.x + 0.5 - skinWidth) + ", " + (gameObject.transform.position.y - 0.5 - skinWidth) + ")");

                        //        Debug.Log("Velocity.x (original) = " + velocity.x);
                        //        Debug.Log("Velocity.x (updated)  = " + (hit.distance - skinWidth) * directionX);
                        //        Debug.Log("hit.distance          = " + hit.distance);
                        //        Debug.Log("hit.distance - skinWidth = " + (hit.distance - skinWidth));
                        //        Debug.Log("directionX            = " + directionX);
                        //        Debug.Break();
                        //    }
                        //    //else
                        //    //Debug.Log("Point not in Surface" + p);
                        //}
                    }

                    Collisions.left  |= directionX == -1;
                    Collisions.right |= directionX == 1;

                }
            }
        }
    }

    void DetectVerticalCollisions(ref Vector3 velocity)
    {
        for (int directionY = -1; directionY <= 1; directionY += 2)
        {
            float moveVelocity = (Mathf.Sign(velocity.y) == directionY) ? Mathf.Abs(velocity.y) : 0;
            float rayLength = moveVelocity + skinWidth * 2;

            for (int i = 0; i < verticalRayCount; i++)
            {
                Vector2 rayOrigin = (directionY == -1) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
                // subract skin width so ray is cast from within 
                //rayOrigin.y += skinWidth * ((directionY == -1) ? 1 : -1);
                rayOrigin += Vector2.right * (verticalRaySpacing * i + velocity.x);

                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, collisionMask);

                Debug.DrawRay(rayOrigin, Vector2.up * directionY * rayLength, Color.red);

                if (hit)
                {
                    if (moveVelocity > 0)
                    {
                        velocity.y = (hit.distance - skinWidth) * directionY;
                        rayLength = hit.distance;
                    }

                    Collisions.below |= directionY == -1;
                    Collisions.above |= directionY == 1;
                }
                else
                {
                    //Debug.Log("No hit");
                }
            }
        }
    }

    protected void UpdateRaycastOrigins()
    {
        Bounds bounds = collider2d.bounds;
        bounds.Expand(skinWidth * -2); // Shrink bounds by skinwidth

        // TODO because I'm shooting from (bounds - skinwidth), the raycasts aren't at the corners...
        // so instead, shoot from the corners but subtract skinwidth from the actual ray shooting bit
        // in the detect collision functions
        // But, if i shoot from the very corners, then the bottom corners will detect left/right collisions with the ground at all times
        // (I tried this)
        // If I want closer to corners, just decrease skinwidth

        raycastOrigins.bottomLeft = new Vector2(bounds.min.x, bounds.min.y);
        raycastOrigins.bottomRight = new Vector2(bounds.max.x, bounds.min.y);
        raycastOrigins.topLeft = new Vector2(bounds.min.x, bounds.max.y);
        raycastOrigins.topRight = new Vector2(bounds.max.x, bounds.max.y);

        //Debug.Log(raycastOrigins);
    }

    protected void CalculateRaySpacing()
    {
        Bounds bounds = collider2d.bounds;
        bounds.Expand(skinWidth * -2); // Shrink bounds by skinwidth

        horizontalRayCount = Mathf.Clamp(horizontalRayCount, 2, int.MaxValue);
        verticalRayCount = Mathf.Clamp(verticalRayCount, 2, int.MaxValue);

        horizontalRaySpacing = bounds.size.y / (horizontalRayCount - 1);
        verticalRaySpacing = bounds.size.x / (verticalRayCount - 1);
    }

    protected struct RaycastOrigins
    {
        public Vector2 topLeft,
                       topRight,
                       bottomLeft,
                       bottomRight;
    }

    [System.Serializable]
    public struct CollisionInfo
    {
        public bool left,
                    right,
                    above,
                    below;
            
        public void Reset()
        {
            left = false;
            right = false;
            above = false;
            below = false;
        }
    }
}
