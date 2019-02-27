using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(Collision))]

public class PlayerController : ObjectController
{
    // Player specific states/attributes

    protected Inputs inputs;

    protected new BoxCollider2D collider;
    protected Collision surfaceCollsions;

    [SerializeField] float jumpHeight = 4;
    [SerializeField] float timeToJumpApex = 0.4f;
    [SerializeField] float jumpCooldown = 0.3f; // just less than timeToJumpApex

    [SerializeField] float accelerationTimeAirborne = .2f;
    [SerializeField] float accelerationTimeGrounded = .1f;
    [SerializeField] float accelerationTimeCrawl    = .2f;
    [SerializeField] float accelerationTimeSlide    = .5f;

    [SerializeField] protected Vector3 velocity;

    [SerializeField] bool facing = true; //false = left, true = right
    [SerializeField] float dodgeVelocity = 3;
    [SerializeField] float dodgeCooldown = 1;

    [SerializeField] bool ducking = false;
    [SerializeField] float crawlSpeed = .05f;
    [SerializeField] protected float speed = 10;

    [SerializeField] float climbSpeed = 0.1f;

    float velocityXSmoothing;
    float gravity;
    float jumpVelocity;

    [Serializable] struct Abilities
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
    [SerializeField] Abilities abilities;


    // Start is called before the first frame update
    void Start()
    {
        abilities.ActivateAll();
        velocity = new Vector3();
        collider = GetComponent<BoxCollider2D>();
        surfaceCollsions = GetComponent<Collision>();
        // Check if init successful
        if (collider == null || surfaceCollsions == null)
        {
            Debug.Log("Failed initialization: " + gameObject.name);
        }


        // COPIED FROM OLD
        gravity = -(2 * jumpHeight / Mathf.Pow(timeToJumpApex, 2));
        jumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;
        //Debug.Log("Gravity: " + gravity);
        //Debug.Log("Jump Velocity: " + jumpVelocity);
        // END COPY
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
        //inputs.run      = Input.GetAxisRaw("Horizontal");
        inputs.duck       = Input.GetKey(KeyCode.S);
        inputs.jump       = Input.GetButtonDown("Jump") || Input.GetButtonUp("Jump");
        inputs.dodge      = Input.GetKeyDown(KeyCode.L) || Input.GetKeyUp(KeyCode.L);
        //inputs.attack   = Input.GetKeyDown(KeyCode.J) || Input.GetKeyUp(KeyCode.J);
        inputs.grab     = Input.GetKey(KeyCode.K);

        //inputs.pause = Input.GetKey(KeyCode.P);
    }

    protected override void PerformAction()
    {
        if (inputs.duck && surfaceCollsions.Collisions.below && !ducking)
        {
            IEnumerator coroutine = DoDuck();
            StartCoroutine(coroutine);
        }
    }

    private IEnumerator DoDuck()
    {
        //Debug.Log("Ducking");
        ducking = true;
        gameObject.transform.Translate(new Vector3(0, -collider.bounds.size.y / 4, 0));
        gameObject.transform.localScale -= new Vector3(0, collider.bounds.size.y / 2, 0);
        while (inputs.duck && abilities.duck)
        {
            yield return null;
        }
        //Debug.Log(inputs.duck);
        //Debug.Log(abilities.duck);
        gameObject.transform.localScale += new Vector3(0, collider.bounds.size.y, 0);
        gameObject.transform.Translate(new Vector3(0, collider.bounds.size.y / 4, 0));
        ducking = false;
    }

    protected override void SetVelocity()
    {
        float run = 0f;

        // jump
        if (abilities.jump && (inputs.jump))
        {
            //Debug.Log("Jumping");
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
            if (Input.GetKey(KeyCode.W))
            {
                velocity.y = climbSpeed;
            }
            else if (Input.GetKey(KeyCode.S))
            {
                velocity.y = -climbSpeed;
            }
            else
            {
                velocity.y = 0;
            }
            // If jump while clinging: leap off wall
            if (Input.GetKey(KeyCode.Space))
            {
                velocity.y = jumpVelocity * 0.5f;
                float jumpDirection = (surfaceCollsions.Collisions.left) ? 1 : -1;
                velocity.x = jumpVelocity * jumpDirection;
            }
        }
        // If we were on the ground last frame, we still want to set downward velocity
        // in order to detect floor collisions
        else
        {
            velocity.y += gravity * Time.deltaTime;
            run = Input.GetAxisRaw("Horizontal");
            if (run != 0)
            {
                facing = (run == 1) ? true : false;
            }
            //Debug.Log("Run: " + run);
        }

        if (ducking)
        {
            // sliding possible if running faster than crawl speed
            // sliding has much slower deceleration
            float targetVelocityX = run * crawlSpeed;
            velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing,
                                          (Mathf.Abs(velocity.x) > Mathf.Abs(targetVelocityX)) ? accelerationTimeSlide : accelerationTimeCrawl);
            //Debug.Log("Target Velocity: " + targetVelocityX);
            //Debug.Log("velocity.x: " + velocity.x);
            //Debug.Break();
        }
        else // running
        {
            float targetVelocityX = run * speed;
            velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing,
                                          (surfaceCollsions.Collisions.below ? accelerationTimeGrounded : accelerationTimeAirborne));
        }

        // Debounce
        if (Mathf.Abs(velocity.x) < 0.01)
        {
            velocity.x = 0;
            //Debug.Log("X debounce");
        }
        if (Mathf.Abs(velocity.y) < 0.01)
        {
            velocity.y = 0;
            //Debug.Log("Y debounce");
        }
    }

    private IEnumerator DoDodge()
    {
        abilities.dodge = false;
        velocity.y += jumpVelocity / 4;
        velocity.x += dodgeVelocity * (facing ? 1 : -1);
        yield return new WaitForSeconds(dodgeCooldown);
        abilities.dodge = true;
    }

    private IEnumerator DoJump()
    {
        bool canDoubleJump = false;
        abilities.jump = false;
        //Debug.Log("Jump disabled at " + Time.time);
        velocity.y = jumpVelocity;
        if (surfaceCollsions.Collisions.below)
        {
            //Debug.Log("Double jump allowed");
            canDoubleJump = true;
        }
        yield return new WaitForSeconds(jumpCooldown);
        if (canDoubleJump)
        {
            //Debug.Log("Jump enabled at " + Time.time);
            abilities.jump = true;
        }
    }

    protected override void HandleCollisions()
    {
        surfaceCollsions.DetectCollisions(ref velocity);
    }

    protected override void Move()
    {
        gameObject.transform.Translate(velocity);
    }

    // Struct to handle inputs
    protected struct Inputs
    {
        public float run;
        public bool duck,
                    jump,
                    dodge,
                    attack,
                    grab,
                    pause;

        public void clear()
        {
            run = 0;
            duck = false;
            jump = false;
            dodge = false;
            attack = false;
            grab = false;
            pause = false;
        }
    }
}
