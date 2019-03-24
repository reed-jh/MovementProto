using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerClimb : PlayerAbility
{
    [SerializeField] float climbSpeed = 0.1f;
    [SerializeField] float wallJumpVelocityY = 0.25f;
    [SerializeField] float wallJumpVelocityX = 0.25f;

    bool isMoving;
    bool upOrDown; // true = up

    public PlayerClimb(PlayerController p) : base(p, true)
    {

    }

    public override void CheckInput()
    {
        input = Input.GetKey(KeyCode.K);
        isMoving = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S);
        upOrDown = Input.GetKey(KeyCode.W) ? true : false;
    }

    public override void CheckPermitted()
    {
        permitted = player.surfaceCollsions.Collisions.right || player.surfaceCollsions.Collisions.left;
    }

    protected override IEnumerator DoAbilityCoroutine()
    {
        doing = true;
        while (input && permitted)
        {
            //Climb up
            if (isMoving)
            {
                if (upOrDown)
                {
                    player.state.velocity.y = climbSpeed;
                }
                else
                {
                    player.state.velocity.y = -climbSpeed;
                }
            }
            else
            {
                player.state.velocity.y = 0;
            }
            // If jump while clinging: leap off wall
            if (player.abilities.jump.input)
            {
                player.state.velocity.y = wallJumpVelocityY;
                float jumpDirection = (player.surfaceCollsions.Collisions.left) ? 1 : -1;
                player.state.velocity.x = wallJumpVelocityX * jumpDirection;
            }
            yield return null;
        }
        doing = false;
    }
}