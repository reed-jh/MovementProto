using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ObjectController : MonoBehaviour
{
    // Physical state of object
    [SerializeField] protected float GRAVITY = 10; // TBD
    //[SerializeField] protected bool grounded; // etc -- TODO how to describe obj state?
    [SerializeField] protected float speed = 10; // for running etc.

    //[SerializeField] protected Vector3 velocity;

    //protected Collision collision;

    // Update is called once per frame
    protected virtual void Update()
    {
        GetState(); // ? is this needed?
        // It might be needed to handle things like dead character
        // TODO having issue with checking state from previous frame, so setting state at the end of moving/colliding
        GetInput();
        PerformAction();
        SetVelocity();
        HandleCollisions();
        Move();
    }

    protected virtual void GetState() { }
    protected virtual void GetInput() { }
    protected virtual void PerformAction() { }
    protected virtual void SetVelocity() { } // Will hem in velocity if collision
    protected virtual void HandleCollisions() { }
    protected virtual void Move() { }
}
