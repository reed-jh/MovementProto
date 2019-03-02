using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDuck : PlayerAbility
{
    public PlayerDuck(PlayerController p) : base(p, true)
    {
    }

    public override void CheckInput()
    {
        input = Input.GetKey(KeyCode.S);
    }

    public override void CheckPermitted()
    {
        permitted = player.surfaceCollsions.Collisions.below;
    }

    protected override IEnumerator DoAbilityCoroutine()
    {
        doing = true;
        player.gameObject.transform.Translate(new Vector3(0, -player.collider.bounds.size.y / 4, 0));
        player.gameObject.transform.localScale -= new Vector3(0, player.collider.bounds.size.y / 2, 0);
        while (input && permitted)
        {
            yield return null;
        }
        player.gameObject.transform.localScale += new Vector3(0, player.collider.bounds.size.y, 0);
        player.gameObject.transform.Translate(new Vector3(0, player.collider.bounds.size.y / 4, 0));
        doing = false;
    }
}
