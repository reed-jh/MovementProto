using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ObjectController : MonoBehaviour
{
    // Universal Parameters
    protected const float GRAVITY = -0.5f;

    // Classes implementing ObjectController can call base.Update()
    // to use this per-frame update sequence
    protected virtual void Update()
    {
        GetState();
        GetInput();
        PerformAction();
        SetVelocity();
        HandleCollisions();
        Move();
    }

    protected abstract void GetState();
    protected abstract void GetInput();
    protected abstract void PerformAction();
    protected abstract void SetVelocity();
    protected abstract void HandleCollisions();
    protected abstract void Move();
}
