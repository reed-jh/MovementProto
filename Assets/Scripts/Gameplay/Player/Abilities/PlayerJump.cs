using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerJump : PlayerAbility
{
    // Parameters
    [SerializeField] float jumpVelocity = 0.25f;

    // Constructor
    public PlayerJump(PlayerController con) : base(con)
    {
        cooldown = .3f;
    }

    public override void CheckInput()
    {
        input = Input.GetButtonDown("Jump") || Input.GetButtonUp("Jump");
    }

    public override void CheckPermitted()
    {
        // Set to true on ground
        permitted |= player.surfaceCollsions.Collisions.below;
    }

    protected override IEnumerator DoAbility()
    {
        Debug.Log("DoAbility Entered");

        bool canDoubleJump = false;
        permitted = false;
        player.state.velocity.y = jumpVelocity;
        canDoubleJump |= player.surfaceCollsions.Collisions.below;
        yield return new WaitForSeconds(cooldown);
        permitted |= canDoubleJump;
    }
}
