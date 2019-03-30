using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerCombat : CombatSystem
{
    PlayerController controller;

    GameObject punch;
    EdgeCollider2D punchCollider;
    [SerializeField] Rigidbody2D punchRigidbody;
    [SerializeField] float punchRange = 1f;
    [SerializeField] float punchTime = 0.1f;
    [SerializeField] float punchHitZone = .5f;

    bool doingPunch;

    //public PlayerCombat(PlayerController p)// : base(p)
    private void Start()
    {
        controller = GetComponentInParent<PlayerController>();
        if (controller == null)
        {
            Debug.LogError("Player CombatSystem unlinked to PlayerController");
        }

        punch = gameObject.transform.Find("Punch").gameObject;

        // Initialize health to 100
        health = 100;

        // Initialize punch
        punchCollider = punch.AddComponent<EdgeCollider2D>();
        punchCollider.enabled = false;
        Vector2[] pointsArr = { new Vector2(0, punchHitZone / 2), new Vector2(0, -punchHitZone / 2) };
        punchCollider.points = pointsArr;
        punchCollider.isTrigger = true;
        punchRigidbody = punch.AddComponent<Rigidbody2D>(); // needed for collision triggers
        punchRigidbody.isKinematic = false;
        punchRigidbody.gravityScale = 0;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.J))
        {
            Punch();
        }
    }

    public void Punch()
    {
        if (doingPunch) return;

        float punchOffset = .5f; // size of player hardcoded for now
        int direction = 1;
        if (!controller.state.facing)
        {
            direction = -1;
        }

        // Set offset in direction of punch (which way player is facing)
        punch.transform.position = new Vector2(punchOffset * direction, 0);

        IEnumerator coroutine = PunchCoroutine();
        StartCoroutine(coroutine);
    }

    IEnumerator PunchCoroutine()
    {
        doingPunch = true;
        punchCollider.enabled = true;
        float xOffset = .5f * (controller.state.facing ? 1 : -1); // size of player hardcoded for now
        for (float elapsed = 0f; elapsed < punchTime; elapsed += Time.deltaTime)
        {
            xOffset += punchRange * (controller.state.facing ? 1 : -1) * Time.deltaTime / punchTime;
            punch.transform.position = new Vector2(controller.transform.position.x + xOffset, controller.transform.position.y);

            // draw punch for visualizing without animation
            //Debug.DrawLine(
                //new Vector2(controller.transform.position.x + xOffset, controller.transform.position.y + (punchHitZone / 2)),
                //new Vector2(controller.transform.position.x + xOffset, controller.transform.position.y - (punchHitZone / 2)),
                //Color.magenta, .1f);

            yield return null;
        }
        punchCollider.enabled = false;
        doingPunch = false;
    }
}
