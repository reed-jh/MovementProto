using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformController : ObjectController
{
    void Start() {
        IEnumerator coroutine = MovementCoroutine();
        StartCoroutine(coroutine);
    }

    IEnumerator MovementCoroutine(){
        float timer = 0;

        // move left 2 seconds
        while (timer < 2)
        {
            velocity = Vector3.left;
            timer += Time.deltaTime;
            yield return null;
        }
        timer = 0;

        // move up 2 seconds
        while (timer < 2)
        {
            velocity = Vector3.up;
            timer += Time.deltaTime;
            yield return null;
        }
        timer = 0;

        // move up and right 2 seconds (diagonal)
        while (timer < 2)
        {
            velocity = (Vector3.up + Vector3.right);
            timer += Time.deltaTime;
            yield return null;
        }
        timer = 0;

        // move down 2 seconds
        while (timer < 2)
        {
            velocity = Vector3.down;
            timer += Time.deltaTime;
            yield return null;
        }
        timer = 0;
    }

    protected override void GetInput()
    {

    }

    protected override void GetState()
    {

    }

    protected override void HandleCollisions()
    {

    }

    protected override void Move()
    {
        transform.Translate(velocity * Time.deltaTime);
    }

    protected override void PerformActions()
    {

    }
}
