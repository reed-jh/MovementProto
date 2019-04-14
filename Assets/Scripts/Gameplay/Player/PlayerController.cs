using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(Collision))]

public class PlayerController : ObjectController
{
    // Player specific states/attributes
    public new BoxCollider2D collider;
    public Collision surfaceCollsions;

    // Helper variables
    float velocityXSmoothing;

    // Parameters and states
    [SerializeField] public PlayerControllerState state;
    //[SerializeField] public PlayerControllerInputs inputs;
    [SerializeField] public PlayerControllerParameters param;

    [SerializeField] public PlayerAbilities abilities;
    protected List<PlayerAbility> abilitiesList;

    // Start is called before the first frame update
    void Start()
    {
        //state.velocity = new Vector3();
        velocity = new Vector3();
        collider = GetComponent<BoxCollider2D>();
        surfaceCollsions = GetComponent<Collision>();

        initAbilities();
    }

    void initAbilities()
    {
        abilitiesList = new List<PlayerAbility>();

        abilities.jump = new PlayerJump(this);
        abilities.dodge = new PlayerDodge(this);
        abilities.run = new PlayerRun(this);
        abilities.climb = new PlayerClimb(this);
        abilities.duck = new PlayerDuck(this);

        abilitiesList.Add(abilities.dodge);
        abilitiesList.Add(abilities.jump);
        abilitiesList.Add(abilities.run);
        abilitiesList.Add(abilities.climb);
        abilitiesList.Add(abilities.duck);

        foreach (PlayerAbility ability in abilitiesList)
        {
            ability.Activate();
        }
    }

    // Update is called once per frame
    protected override void FixedUpdate()
    {
        base.FixedUpdate();
    }

    protected override void GetState()
    {
        foreach (PlayerAbility ability in abilitiesList)
        {
            ability.CheckPermitted();
        }
    }

    protected override void GetInput()
    {
        foreach (PlayerAbility ability in abilitiesList)
        {
            ability.CheckInput();
        }
    }

    protected override void PerformActions()
    {
        foreach (PlayerAbility ability in abilitiesList)
        {
            ability.Perform();
        }

        if (!abilities.dodge.doing && !abilities.climb.doing && !surfaceCollsions.Collisions.below) // will not fall while dodging
        {
            //state.velocity.y += GRAVITY * Time.deltaTime;
            velocity.y += GRAVITY * Time.deltaTime;
        }
    }

    protected override void HandleCollisions()
    {
        delta += velocity;
        //surfaceCollsions.DetectCollisions(ref state.velocity);
        surfaceCollsions.DetectCollisions(ref delta, ref velocity);
    }

    protected override void Move()
    {
        gameObject.transform.Translate(delta);
        delta = Vector3.zero;
    }

    [Serializable]
    public struct PlayerControllerState
    {
        // TODO duplicated in objcontroller
        //public Vector3 velocity;
        // TODO I probably want to extend this to include up/down as well
        public bool facing; //false = left, true = right
    }

    //TODO I may want to handle inputs out here still because sometimes they are used between abilities

    // Struct to handle inputs
    //[Serializable]
    //public struct PlayerControllerInputs
    //{
    //    public float run;
    //    public bool duck,
    //                up,
    //                jump,
    //                dodge,
    //                attack,
    //                grab,
    //                pause;

    //    public void clear()
    //    {
    //        run = 0;
    //        duck = false;
    //        up = false;
    //        jump = false;
    //        dodge = false;
    //        attack = false;
    //        grab = false;
    //        pause = false;
    //    }
    //}

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
            //jump = false;
            dodge = false;
            duck = false;
            grab = false;
        }
    }

    [Serializable]
    public struct PlayerAbilities
    {
        public PlayerJump jump;
        public PlayerRun run;
        public PlayerDodge dodge;
        public PlayerClimb climb;
        public PlayerDuck duck;
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
            public float dodgeDistance;
            public float dodgeTime;
            public float dodgeCooldown;
        }
    }
}
