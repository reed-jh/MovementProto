using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ObjectController : MonoBehaviour
{
    // Universal Parameters
    protected const float GRAVITY = -0.5f;
    public static bool PAUSED = false;

    // Classes implementing ObjectController can call base.Update()
    // to use this per-frame update sequence
    protected virtual void Update()
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

    // TODO is this best handled here? Should it be moved local to combat system
    // Or should there be a child of ObjController like LivingController that includes
    // combatsystem and death etc?
    public abstract void Die();
}
