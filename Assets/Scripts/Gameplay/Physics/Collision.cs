using System;
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

    // TODO include multiple collision masks
    // How to handle different behavior with each?
    // Struct with LayerMask and "constrainMovement" bool?
    // Then other options can just be added to the struct (like, notify collider)
    public LayerMask collisionMask;

    [SerializeField] public List<Collider2D> wallCollisions;
    [SerializeField] public List<Collider2D> platformCollisions;

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

    struct CollisionVector
    {
        public bool horz;
        public bool pos;
    }

    public virtual void Start()
    {
        collider2d = GetComponentInParent<BoxCollider2D>();
        Collisions = new CollisionInfo();
        CalculateRaySpacing();

        //wallCollisions = new HashSet<Collider2D>();
    }

    // A candidate movement vector is given to this function to detect Collisions

    // TODO create a flag for detect collisions that "Constrains velocity"
    // so, when this is true, we actually stop the object from moving because it hit something

        // Thinking ahead, I should revise this code so that two moving objects don't fly past each other.
        // Would assuming that each object does it's complete update before the next object ensure this
        // automatically? Obj1 would literally move before obj2 checked for collisions....?

        // Also moving platforms? Getting pushed by objects?

    public void DetectCollisions(ref Vector3 delta, ref Vector3 velocity)
    {
        Collisions.Reset();
        UpdateRaycastOrigins();
        CalculateRaySpacing();
        // TODO don't check if not moving on axis?

        // TODO shouldn't the rays shoot diagonally since you don't move x then y, but both together?
        // But how would I do that with the opposite side of the player?.....

        // TODO actually, I think I need to check all axes on all frames, but some just check if something is touching (within skin)
        platformCollisions.Clear();
        DetectHorizontalCollisions(ref delta, ref velocity);
        DetectVerticalCollisions(ref delta, ref velocity);
    }

    private void DetectMovingPlatform(PlatformController obj, CollisionVector colVec, ref Vector3 delta, ref Vector3 velocity, bool below = false)
    {
        int colVecInt = colVec.pos ? 1 : -1;

        // Check push
        if (colVec.horz && (Mathf.Sign(obj.velocity.x * colVecInt)) == -1)
        {
            float candidateVelocity = delta.x + obj.velocity.x * Time.deltaTime;
            if (colVec.pos) {
                delta.x = Mathf.Min(delta.x, candidateVelocity);
            } else {
                delta.x = Mathf.Max(delta.x, candidateVelocity);
            }
        }
        else if (!colVec.horz && (Mathf.Sign(obj.velocity.y * colVecInt)) == -1)
        {
            float candidateVelocity = delta.y + obj.velocity.y * Time.deltaTime;
            if (colVec.pos) {
                delta.y = Mathf.Min(delta.y, candidateVelocity);
            } else {
                // Shitty workaround for a bug:
                // Y velocity doesn't get reset to 0 each frame, but gets reduced by gravity,
                // so instead of using the velocity attribute (which should maybe be paired with am instantaneous delta attr)
                // I'm going to just push the player up (if candidate velocity is chosen)
                //velocity.y = Mathf.Max(velocity.y, candidateVelocity);
                if (candidateVelocity > delta.y) {
                    //transform.Translate(new Vector2(0,candidateVelocity));
                    delta.y = candidateVelocity;
                }

            }
        }

        // Check carry
        if (below && obj.velocity.x != 0)
        { 
            // probably should be obj.delta
            delta.x += obj.velocity.x * Time.deltaTime;
        }

        // TODO, what if this velocity causes a collision? Right now that's not getting handled
    }

    // Can Horz/Vert code be merged into a single function, or at least part of it
    // lots of code copy right now
    // This would make the DetectMovingPlatform calls better
    void DetectHorizontalCollisions(ref Vector3 delta, ref Vector3 velocity)
    {
        wallCollisions.Clear();

        for (int directionX = -1; directionX <= 1; directionX += 2)
        {
            float moveVelocity = (Mathf.Sign(delta.x) == directionX) ? Mathf.Abs(delta.x) : 0;
            float rayLength = moveVelocity + skinWidth * 2;
            for (int i = 0; i < horizontalRayCount; i++)
            {
                Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
                rayOrigin += Vector2.up * (horizontalRaySpacing * i);

                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);

                Debug.DrawRay(rayOrigin, Vector2.right * directionX * rayLength, Color.red);

                if (hit)
                {
                    // Add wall collisions for later ledge-grabbing detection
                    if (!wallCollisions.Contains(hit.collider))
                        wallCollisions.Add(hit.collider);

                    // Inelegant, we don't want to update velocity for already collided platforms
                    if (moveVelocity > 0 && !platformCollisions.Contains(hit.collider))
                    {
                        delta.x = (hit.distance - skinWidth) * directionX;
                        rayLength = hit.distance;
                    }

                    Collisions.left  |= directionX == -1;
                    Collisions.right |= directionX == 1;

                    CollisionVector colVec;
                    colVec.horz = true;
                    colVec.pos = Collisions.right;

                    // If collider is a movable object
                    // Might be better to check tags than component
                    if (hit.collider.gameObject.GetComponent<PlatformController>() != null && !platformCollisions.Contains(hit.collider))
                    {
                        platformCollisions.Add(hit.collider);
                        DetectMovingPlatform(hit.collider.gameObject.GetComponent<PlatformController>(), colVec, ref delta, ref velocity);
                    }
                }
            }
        }
    }

    void DetectVerticalCollisions(ref Vector3 delta, ref Vector3 velocity)
    {
        for (int directionY = -1; directionY <= 1; directionY += 2)
        {
            float moveVelocity = (Mathf.Sign(delta.y) == directionY) ? Mathf.Abs(delta.y) : 0;
            float rayLength = moveVelocity + skinWidth * 2;

            for (int i = 0; i < verticalRayCount; i++)
            {
                Vector2 rayOrigin = (directionY == -1) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
                rayOrigin += Vector2.right * (verticalRaySpacing * i + delta.x);

                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, collisionMask);

                Debug.DrawRay(rayOrigin, Vector2.up * directionY * rayLength, Color.red);

                if (hit)
                {
                    if (moveVelocity > 0 && !platformCollisions.Contains(hit.collider))
                    {
                        delta.y = (hit.distance - skinWidth) * directionY;
                        rayLength = hit.distance;
                    }

                    Collisions.below |= directionY == -1;
                    Collisions.above |= directionY == 1;

                    if (Collisions.above) {
                        velocity.y = Mathf.Min(0, velocity.y);
                    }

                    CollisionVector colVec;
                    colVec.horz = false;
                    colVec.pos = Collisions.above;

                    // If collider is a movable object
                    // Might be better to check tags than component
                    if (hit.collider.gameObject.GetComponent<PlatformController>() != null && !platformCollisions.Contains(hit.collider))
                    {
                        platformCollisions.Add(hit.collider);
                        DetectMovingPlatform(hit.collider.gameObject.GetComponent<PlatformController>(), colVec, ref delta, ref velocity, Collisions.below);
                    }
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
    }

    // TODO merge this into UpdateRaycastOrigins
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
