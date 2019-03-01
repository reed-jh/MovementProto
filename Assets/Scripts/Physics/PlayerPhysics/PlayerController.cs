﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(Collision))]

public class PlayerController : ObjectController
{
    // Player specific states/attributes

    protected new BoxCollider2D collider;
    protected Collision surfaceCollsions;

    float velocityXSmoothing;

    [SerializeField] PlayerControllerState state;
    [SerializeField] PlayerControllerAbilities abilities;
    [SerializeField] PlayerControllerInputs inputs;
    [SerializeField] PlayerControllerParameters param;

    /*
     * TODO possible design update:
     * 
     * Each ability (e.g. jump, duck, dodge)
     * has its own class inherited from PlayerAbility
     * Each PlayerAbility has checks for
     *  - AbilityAqcuired (can the player double jump)
     *  - AbilityPermitted (player has only jumped once and is airborne)
     *  - AbilityInput (player has pressed the spacebar)    
     * And a PerformAbility() function that executes the ability, taking in the PlayerState struct and Abilities list
     * This way we can clean up this controller function by just looping through all abilities and doing their checks    
     */   

    // Start is called before the first frame update
    void Start()
    {
        abilities.ActivateAll();
        state.velocity = new Vector3();
        collider = GetComponent<BoxCollider2D>();
        surfaceCollsions = GetComponent<Collision>();
        // Check if init successful
        if (collider == null || surfaceCollsions == null)
        {
            Debug.Log("Failed initialization: " + gameObject.name);
        }
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
    }

    protected override void GetState()
    {
        // Can jump?
        if (!inputs.jump && abilities.jump == false && surfaceCollsions.Collisions.below)
        {
            //Debug.Break();
            //Debug.Log("Jump enabled (on ground) at " + Time.time);
            abilities.jump = true;
        }

        // Can duck only on the ground
        abilities.duck = surfaceCollsions.Collisions.below;

        // Can cling only on walls
        abilities.grab = surfaceCollsions.Collisions.right || surfaceCollsions.Collisions.left;
    }

    protected override void GetInput()
    {
        //return;
        // All this method does is get what buttons are being pressed.
        // We don't know what to do with that without knowing state/collisions
        inputs.clear();
        // TODO does keydown + keyup make sense for all these? How is this normally handled?
        // replace run with left/right
        // replace duck with "down"
        inputs.run      = Input.GetAxisRaw("Horizontal");
        inputs.duck     = Input.GetKey(KeyCode.S);
        inputs.up       = Input.GetKey(KeyCode.W);
        inputs.jump     = Input.GetButtonDown("Jump") || Input.GetButtonUp("Jump");
        inputs.dodge    = Input.GetKeyDown(KeyCode.L) || Input.GetKeyUp(KeyCode.L);
        //inputs.attack = Input.GetKeyDown(KeyCode.J) || Input.GetKeyUp(KeyCode.J);
        inputs.grab     = Input.GetKey(KeyCode.K);

        //inputs.pause = Input.GetKey(KeyCode.P);
    }

    protected override void PerformAction()
    {
        if (inputs.duck && surfaceCollsions.Collisions.below && !state.ducking)
        {
            IEnumerator coroutine = DoDuck();
            StartCoroutine(coroutine);
        }
    }

    private IEnumerator DoDuck()
    {
        state.ducking = true;
        gameObject.transform.Translate(new Vector3(0, -collider.bounds.size.y / 4, 0));
        gameObject.transform.localScale -= new Vector3(0, collider.bounds.size.y / 2, 0);
        while (inputs.duck && abilities.duck)
        {
            yield return null;
        }
        gameObject.transform.localScale += new Vector3(0, collider.bounds.size.y, 0);
        gameObject.transform.Translate(new Vector3(0, collider.bounds.size.y / 4, 0));
        state.ducking = false;
    }

    protected override void SetVelocity()
    {
        // TODO replace with inputs.

        // jump
        if (abilities.jump && (inputs.jump))
        {
            IEnumerator coroutine = DoJump();
            StartCoroutine(coroutine);
        }

        // dodge
        if (abilities.dodge && (inputs.dodge))
        {
            IEnumerator coroutine = DoDodge();
            StartCoroutine(coroutine);
        }

        // If grabbing wall, don't fall
        if (inputs.grab && abilities.grab)
        {
            //Climb up
            // TODO add smoothdamp to climbing?
            // maybe remove smoothdamp from running
            if (inputs.up)
            {
                state.velocity.y = param.speed.climbSpeed;
            }
            else if (inputs.duck)
            {
                state.velocity.y = -param.speed.climbSpeed;
            }
            else
            {
                state.velocity.y = 0;
            }
            // If jump while clinging: leap off wall
            if (inputs.jump)
            {
                state.velocity.y = param.jump.jumpVelocity * 0.5f;
                float jumpDirection = (surfaceCollsions.Collisions.left) ? 1 : -1;
                state.velocity.x = param.jump.jumpVelocity * jumpDirection;
            }
        }
        // If we were on the ground last frame, we still want to set downward velocity
        // in order to detect floor collisions
        else
        {
            state.velocity.y += GRAVITY * Time.deltaTime;
            if (inputs.run != 0)
            {
                state.facing = (inputs.run == 1) ? true : false;
            }
            //Debug.Log("Run: " + run);
        }

        if (state.ducking)
        {
            // sliding possible if running faster than crawl speed
            // sliding has much slower deceleration
            float targetVelocityX = inputs.run * param.speed.crawlSpeed;
            state.velocity.x = Mathf.SmoothDamp(state.velocity.x, targetVelocityX, ref velocityXSmoothing,
                                          (Mathf.Abs(state.velocity.x) > Mathf.Abs(targetVelocityX)) ? param.accel.accelerationTimeSlide : param.accel.accelerationTimeCrawl);
        }
        else // running
        {
            if (surfaceCollsions.Collisions.below)
            {
                // REMOVED smoothdamp from running for the time being
                //  This give running a more arcadey feel, which I like
                //  Also could have set accel to 0

                // TODO this overwrites dodge velocity, so I'm inelegantly patching that
                if (!inputs.dodge)
                    state.velocity.x = inputs.run * param.speed.runSpeed;
            }
            else // airborne
            {
                // TODO currently this slows down player to floatspeed, even if running
                float targetVelocityX = inputs.run * param.speed.floatSpeed;
                state.velocity.x = Mathf.SmoothDamp(state.velocity.x, targetVelocityX, ref velocityXSmoothing, param.accel.accelerationTimeAirborne);
            }
        }

        /*
        // Debounce - deltaTime factored in so high framerates do not cause halting
        if (Mathf.Abs(state.velocity.x) < 0.5 * Time.deltaTime)
        {
            state.velocity.x = 0;
            //Debug.Log("X debounce");
        }
        if (Mathf.Abs(state.velocity.y) < 0.5 * Time.deltaTime)
        {
            state.velocity.y = 0;
            //Debug.Log("Y debounce");
        }
        */
    }

    private IEnumerator DoDodge()
    {
        abilities.dodge = false;
        state.velocity.y += param.dodge.dodgeVelocityY;
        state.velocity.x = param.dodge.dodgeVelocityX * (state.facing ? 1 : -1);
        yield return new WaitForSeconds(param.dodge.dodgeCooldown);
        abilities.dodge = true;
    }

    private IEnumerator DoJump()
    {
        bool canDoubleJump = false;
        abilities.jump = false;
        //Debug.Log("Jump disabled at " + Time.time);
        state.velocity.y = param.jump.jumpVelocity;
        if (surfaceCollsions.Collisions.below)
        {
            //Debug.Log("Double jump allowed");
            canDoubleJump = true;
        }
        yield return new WaitForSeconds(param.jump.jumpCooldown);
        if (canDoubleJump)
        {
            //Debug.Log("Jump enabled at " + Time.time);
            abilities.jump = true;
        }
    }

    protected override void HandleCollisions()
    {
        surfaceCollsions.DetectCollisions(ref state.velocity);
    }

    protected override void Move()
    {
        gameObject.transform.Translate(state.velocity);
    }

    [Serializable]
    public struct PlayerControllerState
    {
        public Vector3 velocity;
        // TODO I probably want to extend this to include up/down as well
        public bool facing; //false = left, true = right
        public bool ducking;
    }

    // Struct to handle inputs
    [Serializable]
    public struct PlayerControllerInputs
    {
        public float run;
        public bool duck,
                    up,
                    jump,
                    dodge,
                    attack,
                    grab,
                    pause;

        public void clear()
        {
            run = 0;
            duck = false;
            up = false;
            jump = false;
            dodge = false;
            attack = false;
            grab = false;
            pause = false;
        }
    }

    [Serializable]
    public struct PlayerControllerAbilities
    {
        public bool jump;
        public bool dodge;
        public bool duck;
        public bool grab;
        public void ActivateAll()
        {
            jump = true;
            dodge = true;
            duck = true;
            grab = true;
        }
        public void DeactivateAll()
        {
            jump = false;
            dodge = false;
            duck = false;
            grab = false;
        }
    }

    [Serializable]
    public struct PlayerControllerParameters
    {
        public SpeedParameters speed;
        public JumpParameters jump;
        public AccelerationParameters accel;
        public DodgeParameters dodge;

        [Serializable]
        public struct SpeedParameters
        {
            public float runSpeed;
            public float climbSpeed;
            public float floatSpeed;
            public float crawlSpeed;
        }

        [Serializable]
        public struct JumpParameters
        {
            public float jumpVelocity;
            public float jumpCooldown;
        }

        [Serializable]
        public struct AccelerationParameters
        {
            public float accelerationTimeAirborne;
            public float accelerationTimeGrounded;
            public float accelerationTimeCrawl;
            public float accelerationTimeSlide;
        }

        [Serializable]
        public struct DodgeParameters
        {
            public float dodgeVelocityX;
            public float dodgeVelocityY;
            public float dodgeCooldown;
        }
    }
}
