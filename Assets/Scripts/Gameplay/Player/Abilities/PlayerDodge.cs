using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDodge : PlayerAbility
{
    // Parameters
    [SerializeField] float dodgeDistance = 3f;
    [SerializeField] float dodgeTime = .1f;

    // Constructor
    public PlayerDodge(PlayerController p) : base(p, true)
    {
        cooldown = 1f;
    }

    public override void CheckInput()
    {
        input = Input.GetKeyDown(KeyCode.L);
    }

    public override void CheckPermitted()
    {
        // Set to true on ground
        permitted |= player.surfaceCollsions.Collisions.below;
    }

    protected override IEnumerator DoAbilityCoroutine()
    {
        doing = true;
        for (float elapsed = 0f; elapsed < dodgeTime; elapsed += Time.deltaTime)
        {
            // player.state.velocity.x = dodgeDistance * (player.state.facing ? 1 : -1) * Time.deltaTime / dodgeTime;
            player.delta.x = dodgeDistance * (player.state.facing ? 1 : -1) * Time.deltaTime / dodgeTime;
            yield return null;
        }
        doing = false;
    }
}
