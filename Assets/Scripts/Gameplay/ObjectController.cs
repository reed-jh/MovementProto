using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ObjectController : MonoBehaviour
{
    // Universal Parameters
    protected const float GRAVITY = -0.5f;
    public static bool PAUSED = false;

    /// <summary>
    /// The velocity of the object, which is influced by acceleration and persists between frames.
    /// Note that actual movement per frame is determined by the 'delta' variable.
    /// </summary>
    public Vector3 velocity;
    /// <summary>
    /// The displacement of the object for a single frame.
    /// </summary>
    public Vector3 delta;

    // Classes implementing ObjectController can call base.Update()
    // to use this per-frame update sequence
    protected virtual void FixedUpdate()
    {
        // If all objects are required to use this base update method,
        // then a pause will be univeral for all moving game objects
        if (!PAUSED)
        {
            GetState();
            GetInput();
            PerformActions();
            HandleCollisions();
            Move();
        }
    }

    protected abstract void GetState();
    protected abstract void GetInput();
    protected abstract void PerformActions();
    protected abstract void HandleCollisions();
    protected abstract void Move();
}
