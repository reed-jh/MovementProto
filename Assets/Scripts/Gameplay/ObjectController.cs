using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ObjectController : MonoBehaviour
{
    // Universal Parameters
    protected const float GRAVITY = -0.5f;
    public static bool PAUSED = false;

    // V3 or V2?
    public Vector3 velocity;

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
