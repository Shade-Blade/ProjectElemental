using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static BattleHelper;

//???
public interface IDashHopTrigger
{
    public void Bonk(Vector3 kickpos, Vector3 kicknormal);
}

public interface ISlashTrigger
{
    //bool = should this halt the animation
    //slashvector = direction to slash in (facing vector)
    //playerpos = player position
    public bool Slash(Vector3 slashvector, Vector3 playerpos);
}

public interface ISmashTrigger
{
    //bool = should this halt the animation
    //smashvector = direction to smash in (facing vector)
    //playerpos = player position
    public bool Smash(Vector3 smashvector, Vector3 playerpos);
}

public interface IStompTrigger
{
    public void Stomp(WorldPlayer.StompType stompType);
}

public interface IHeadHitTrigger
{
    public void HeadHit(WorldPlayer.StompType stompType);
}

public interface IUndigTrigger
{
    public void Undig();
}


//the character controlled in the overworld
//does not control followers
public class WorldPlayer : WorldEntity
{
    public static WorldPlayer Instance
    {
        get
        {
            if (instanceInternal == null)
            {
                instanceInternal = FindObjectOfType<WorldPlayer>();
                return instanceInternal;
            }
            return instanceInternal;
        }
    }
    private static WorldPlayer instanceInternal;

    //public float lastGroundedHeight;

    //public Vector3 floorNormal;

    public GameObject realOrientationObject;
    public GameObject floorNormalObject;
    public GameObject perpetualParticleObject;  //the illuminate, aetherize and dig actions produce perpetual particles
    public List<GameObject> stalePerpetualParticles;
    public GameObject swooshParticle;   //swoosh from slashing or smashing (Reference is kept so that stopping the swing looks correct)

    public GameObject interactIndicator;    //Cyan colored
    public bool indicatorActive;

    public CollisionCheckScript digCheck;
    public CollisionCheckScript aetherCheck;
    public CollisionCheckScript antiAetherCheck;
    public CollisionCheckScript lightCheck;
    public CollisionCheckScript antiLightCheck;
    public bool lastFloorNoDig; //is the floor you're on a no-dig floor? (Reset in the floor collision script, shouldn't ever become stale? (may be stale for 1 frame or something)

    public bool lastFloorDigThrough;

    public bool lastFloorSlippery;
    public bool lastFloorSticky;

    public bool lastFloorUnstable;  //unstable floor = don't reset the warp point

    public bool unstableZone;
    public bool noDigZone;
    public bool concealedZone;
    public float concealedTime;
    public bool noAetherZone;

    public float disabledTime;

    public float interactCooldown;

    public Vector3 lastFloorPos;

    //this is a line (0 = closest, 1 = second closest...)
    public List<WorldFollower> followers;

    //the 4 lights created by illuminate
    public List<Light> lights;

    public float followerDistance = FOLLOWER_DISTANCE;

    public const float EYE_HEIGHT = 0.175f; //transform.position + eye height   (note: origin of the WorldPlayer is in the center which is 0.375 off ground)
    public const float HEIGHT = 0.75f;      //bottom of hitbox = transform.position - height/2 (and top is in the other direction)

    //i.e. use the rigidbody physics instead of giving you control
    public bool useKinematics;

    public bool applyJumpLift;
    private float jumpLift = WILEX_JUMP_LIFT;
    private float jumpImpulse = WILEX_JUMP_IMPULSE;

    public bool canDoubleJump;
    private bool applyDoubleJumpLift;
    private float doubleJumpLift = DOUBLE_JUMP_LIFT;
    private float doubleJumpImpulse = DOUBLE_JUMP_IMPULSE;

    private float dashSpeed = DASH_SPEED;
    public bool applyDashJumpLift;
    private float dashJumpLift = DASH_JUMP_LIFT;
    private float dashJumpImpulse = DASH_JUMP_IMPULSE;

    public bool applySuperJumpLift;
    private float superJumpLift = SUPER_JUMP_LIFT;
    private float superJumpImpulse = SUPER_JUMP_IMPULSE;

    public bool applySuperKickLift;
    private float superKickLift = SUPER_KICK_LIFT;
    private float superKickImpulse = SUPER_KICK_IMPULSE;

    private float encounterCooldown = 0;
    public const float DEFAULT_ENCOUNTER_COOLDOWN = 1f;

    //public bool isGrounded = false;
    public bool hasTouchedObject = false;

    public MainManager.PlayerCharacter currentCharacter;

    /*
    public LayerMask notPlayerMask;
    public LayerMask playerMask;
    public LayerMask aetherMask;
    public LayerMask antiAetherMask;
    public LayerMask lightMask;
    public LayerMask antiLightMask;
    */

    //to do: actually implement this state system
    //but state systems are complicated and annoying
    //Problem: state system has a lot of overlap (all of the falling states)
    //Maybe a bunch of booleans is a better idea

    //note: to reduce some of these states I will combine them
    //Sprite stuff can still use other stuff to differentiate things more finely
    public enum ActionState
    {
        Neutral,   //walk/run/idle (no need to differentiate)
        Jump,   //becomes fall at the apex of jump
        Fall,
        LaunchFall, //non-controllable airborne state (note: no "launch" state, this state stays for ascending part and descending part)
        HazardFall, //Fall into a hazard zone (i.e. jump into water, fall off the map etc)
        HazardTouch,    //Grounded hazard state (touching a hazard, i.e. landing on spikes or lava)
        Land,
        Slash,
        Aetherize,
        AetherizeFall,
        DoubleJump,
        SuperJump,
        Smash,
        Illuminate,
        IlluminateFall,
        Dash,
        DashFall,
        Dig,
        NoClip,
        FreeCam,
        RevolvingCam,
        WallGrab,
        Climb,
        WallJump,
        Hover,
        SuperKick
    }

    public ActionState actionState;
    public float timeSinceActionChange;

    float timeSinceLastJump;
    bool enableDashLeniency;       //Used for making the dash input more lenient (allow for holding input)
                                   //Enable if you dash, disable in all other jump and airborne situations
    public float digTimer;
    public float undigTimer;

    public float animationStopDelay;

    //note: air time tracker already exists
    bool coyoteBool = false;    //Only enable if you are in a jumpable state, disable in all other cases (though the normal airborne state is neutral)

    float switchTime;       //Blocks certain moves from being used (would probably look wonky)
    public float switchRotation;
    bool switchBuffered;    //switch is not instant, so keep track of if it is incoming (true = coming after switchTime goes below half of switch duration)

    float attackTime;   //using slash or smash
    float visualAttackTime; //can be stopped
    bool attackInterrupted;

    public bool landShockwavePossible;  //shockwave on land? (true if you jump with Luna)

    public bool jumpStomp;
    public bool doubleJumpStomp;
    public bool superJumpStomp;
    public bool dashHopStomp;

    public bool movingPlatformFix;

    public enum StompType
    {
        Fall = 0,
        Jump,
        HeavyJump,
        DashHop,
        DoubleJump,
        SuperJump,
    }

    public float airspeedBonus = 1.2f;

    //not really going to need this much, just nice to have
    public float idleTime = 0;

    public float aetherTime = 0;
    public float lightTime = 0;

    public float notAetherTime = 0;
    public float notLightTime = 0;

    public float aetherShaderTime = 0;
    public float lightShaderTime = 0;
    public float bubbleShaderTime = 0;
    public float leafShaderTime = 0;

    public bool bubbleState = false;
    public bool leafState = false;

    public float pastHeight = 0;
    public float antiStuck = 0;
    public float antiStuckTime = 0;

    public float aetherLightCooldown = 0;
    public float reswingBuffer = 0;

    public float hazardTime = 0;
    public int hazardState = 0;

    public Vector2 scriptedInput;
    public bool scriptedAnimation;

    //public bool useAllFriction = false;

    public float timeSinceStepUp;

    public bool stuckToWall;
    public Vector3 wallVector;

    public bool canHover;
    public float hoverTime;

    public ITattleable zoneTattleTarget;


    //Debug
    //TODO: Set up real animation system
    /*
    public SpriteRenderer spriteRenderer;
    public Sprite wfrontSprite;
    public Sprite wbackSprite;
    public Sprite lfrontSprite;
    public Sprite lbackSprite;

    public Sprite curfrontSprite;
    public Sprite curbackSprite;
    */

    const float WALL_THRESHOLD = 0.1f;  //abs(y) < this
    const float WALL_STICK_VELOCITY = 0.25f; //(per sec) should be low

    const float HOVER_DURATION = 3f;

    const float SUPER_KICK_LIFT = 0.05f;
    const float SUPER_KICK_IMPULSE = 6.75f;


    const float ANTI_STUCK_TIME = 1.5f;   //if your y pos is the same for too long in air
    const float ANTI_STUCK_IMPULSE = 3f;    //should be as small as possible
    const float ANTI_STUCK_DISTANCE = 0.01f; //note: should be lower than what the anti stuck force applies

    public const float NPC_ANTI_STACK_VELOCITY = 1f; //stop you from standing on top of npcs (note: npcs can have a stomp trigger to launch you up)
    public const float NPC_STRONG_ANTI_STACK_VELOCITY = 2f; //stop you from standing on top of npcs (note: npcs can have a stomp trigger to launch you up)

    //physics based constants
    const float WILEX_JUMP_LIFT = 0.05f; //2.5 per s
    const float WILEX_JUMP_IMPULSE = 4f;
    const float LUNA_JUMP_LIFT = 0.025f; //1.25 per s //makes her jumps feel "heavier"
    const float LUNA_JUMP_IMPULSE = 3.5f;

    const float KERU_JUMP_LIFT = 0.09f; //4.5f per s
    const float KERU_JUMP_IMPULSE = 3.5f;
    const float ASTER_JUMP_LIFT = -0.01f; //-0.5 per s
    const float ASTER_JUMP_IMPULSE = 4.5f;

    const float DOUBLE_JUMP_LIFT = 0.025f;    //not sure why the double jump has different physics than the first jump
    const float DOUBLE_JUMP_IMPULSE = 4.5f;   //guess these values make it less floaty but similar vertical height gain
    const float DASH_SPEED = 6;
    const float DASH_JUMP_LIFT = 0.05f;
    const float DASH_JUMP_IMPULSE = 2.75f;
    const float SUPER_JUMP_LIFT = -0.1f;            //makes the upward part faster which should make it seem like a more powerful jump (?)
    const float SUPER_JUMP_IMPULSE = 9.25f;

    const float STICKY_FLOOR_JUMP_PENALTY = -1f;    //How much to reduce jump impulse by on sticky floors

    const float MIN_GROUND_DIG_NORMAL = 0.4f; //so a 1x/2y slope is climbable


    //time related constants
    const float LAND_TIME = 0.05f; //time taken to convert Land state to Idle
    const float COYOTE_TIME = 0.05f;    //3 frames
    const float BUFFER_JUMP_WINDOW = 0.075f;    //if you press the jump button ??? before landing, you jump anyway
    const float BUFFER_DASH_JUMP_WINDOW = 0.15f;    //buffer window for dashing (a lot wider to make it very easy to chain dashes)
    const float DASH_DIRECTIONAL_WINDOW = 0.125f;   //sidenote: very close to dash jump buffer window (but this is mostly coincidence)
    const float SWITCH_DURATION = 0.25f;

    public const float CONTROL_HOLD_TIME = 0.25f;   //how long to hold B to do the hold B abilities
    public const float HAZARD_ANIM_TIME = 0.4f; //How long to animate the hazard hit animation before the fade out then fade in (note: you will still be stationary until the fade in starts)


    //other threshold constants
    const float DOUBLE_JUMP_WINDOW_HIGHER = 1;      //upper bound for y vel (double jumping while ascending looks weird)
    const float DOUBLE_JUMP_WINDOW_LOWER = -5f;     //lower bound for y vel

    const float FOLLOWER_DISTANCE = 1f;
    const float REDUCED_FOLLOWER_DISTANCE = 0.25f;

    public const float JUMP_DELAY = 0.33f;  //followers jump after you jump

    //sword / hammer info
    public const float SWORD_ANIM_TIME = 0.3f;
    public const float SWORD_SWING_TIME = 0.2f;
    public const float SWORD_ANGLE_SPREAD = 150f;    //How far the sword swings around (note: hitbox sticks out making this slightly higher than expected)
    public const float SWORD_HITBOX_OFFSET = 0.6f;
    public const float SWORD_HITBOX_RADIUS = 0.325f;
    public const float SWORD_INTERRUPT_LENIENCY = 0.075f;   //Sword can pass through walls more easily (also makes it possible to swing sword next to a wall without it instantly bonking)

    public const float HAMMER_ANIM_TIME = 0.4f;
    public const float HAMMER_SWING_TIME = 0.25f;
    public const float HAMMER_ANGLE_SPREAD = 150f;  //How the hammer swings (Starts behind Luna)
    public const float HAMMER_DOWN_ANGLE = -30f;    //Lowest angle the hammer goes to
    public const float HAMMER_HITBOX_OFFSET = 0.625f;
    public const float HAMMER_HITBOX_RADIUS = 0.4f;
    public const float HAMMER_BIG_RADIUS = 0.75f;   //After the interrupt, the hitbox becomes a lot bigger (like it's making a shockwave or something)
    public const float HAMMER_INTERRUPT_LENIENCY = 0.075f;   //time where weapon moves can't be interrupted (Required to make the particles look right?)

    private const float DIG_TIME = 0.15f;
    private const float UNDIG_TIME = 0.15f;

    //Step up / down
    public const float STEP_UP_MINIMUM = 0.1f;
    public const float STEP_UP_BONUS = 0.06f;
    public const float STEP_UP_MAX_HEIGHT = 0.31f;
    public const float STEP_DOWN_MAX_HEIGHT = 0.31f;
    public const float STEP_UP_DOWN_DELAY = 0.05f; //normally the code for step up and step down fight each other

    public const float WARP_BARRIER_Y_COORD = -10f;    //Do not make maps with accessible points below here (note: bottomless pits covered with a Hazard Zone are fine)


    //Light stuff
    public const float LIGHT_INTENSITY = 1.6f;
    public const float AETHER_LIGHT_INTENSITY = 0.7f;
    public Color LIGHT_COLOR = new Color(1, 0.85f, 0.5f, 1);
    public Color AETHER_ANTI_COLOR = new Color(0.1f, 0.7f, 0.8f, 0);    //note: anti light needs to be inverted (because it subtracts the color)

    //public GameObject jumpSpark;
    //public GameObject jumpFire;

    /*
    public override void Awake()
    {
        Setup();
    }
    */

    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();
        /*
        actionState = ActionState.Neutral;
        currentCharacter = MainManager.PlayerCharacter.Wilex;
        jumpImpulse = WILEX_JUMP_IMPULSE;
        jumpLift = WILEX_JUMP_LIFT;

        trueFacingRotation = 0;
        targetFacingRotation = 0;
        */

        if (lights == null)
        {
            lights = new List<Light>(GetComponentsInChildren<Light>());
        }

        for (int i = 0; i < lights.Count; i++)
        {
            lights[i].enabled = false;
        }

        /*
        followers = new List<WorldFollower>();
        //will have to change when I set up the real map logic (entering a map)
        followers.Add(FindObjectOfType<WorldFollower>());
        */

        //ResetFollowerTrail();
        if (subObject == null)
        {
            spriteID = ((MainManager.SpriteID)currentCharacter).ToString();
            MakeAnimationController();
            if (height == 0 || width == 0)
            {
                SetColliderInformationWithAnimationController();
            }
        }
    }

    // Update is called once per frame
    public override void WorldUpdate()
    {
        timeSinceActionChange += Time.deltaTime;

        bool isIdle = (actionState == ActionState.Neutral);
        isIdle &= (MainManager.Instance.GetControlsEnabled() && InputManager.GetAxisHorizontal() == 0 && InputManager.GetAxisVertical() == 0);
        isIdle &= (switchTime == 0);

        //so that the button will hide the hud
        isIdle &= !InputManager.GetButtonDown(InputManager.Button.R);

        if (isIdle)
        {
            idleTime += Time.deltaTime;

            //Item sight
            if (MainManager.Instance.playerData.BadgeEquipped(Badge.BadgeType.ItemSight))
            {
                if (idleTime > 1f && (idleTime - Time.deltaTime) % 0.5f > (idleTime) % 0.5f)  //check if idletime is next to an increment of 0.25 seconds
                {
                    //this will track inactive collectibles
                    //so I am forced to always put them in logical places for where you can find them
                    WorldCollectibleScript[] items = FindObjectsOfType<WorldCollectibleScript>(true);
                    Vector3 nearestPos = items.Length > 0 ? items[0].transform.position : Vector3.positiveInfinity;
                    foreach (WorldCollectibleScript wcs in items)
                    {
                        if ((transform.position - nearestPos).magnitude > (transform.position - wcs.transform.position).magnitude)
                        {
                            nearestPos = wcs.transform.position;
                        }
                    }

                    float dist = (transform.position - nearestPos).magnitude;

                    bool doEffect = false;
                    if (dist < 3)
                    {
                        doEffect = true;
                    } else
                    {
                        doEffect = (idleTime % 1f > 0.4f);
                    }

                    //no dots to draw if you can't get anything in this map
                    if (items.Length == 0)
                    {
                        doEffect = false;
                    }

                    if (doEffect)
                    {
                        GameObject itemSightDot = Instantiate(Resources.Load<GameObject>("Overworld/Other/ItemSightDot"), MainManager.Instance.mapScript.transform);
                        itemSightDot.transform.position = transform.position;
                        ItemSightDotScript isds = itemSightDot.GetComponent<ItemSightDotScript>();

                        isds.velocity = (nearestPos - transform.position).normalized;
                    }
                }
            }
        }
        else
        {
            idleTime = 0;
        }

        //cleanup
        for (int i = 0; i < stalePerpetualParticles.Count; i++)
        {
            if (stalePerpetualParticles[i] == null)
            {
                stalePerpetualParticles.RemoveAt(i);
                i--;
            }
        }

        if (interactCooldown > 0)
        {
            interactCooldown -= Time.deltaTime;
            if (interactCooldown < 0)
            {
                interactCooldown = 0;
            }
        }

        if (interactCooldown <= 0 && MainManager.Instance.GetControlsEnabled())
        {
            if (MainManager.Instance.NonNPCInteractActive())
            {
                if (interactIndicator == null)
                {
                    //make it
                    interactIndicator = Instantiate(Resources.Load<GameObject>("Overworld/Other/InteractIndicator"), gameObject.transform);
                    interactIndicator.transform.localPosition = Vector3.up * 0.7f + Vector3.back * 0.01f;
                }
            } else
            {
                if (interactIndicator != null)
                {
                    Destroy(interactIndicator);
                    interactIndicator = null;
                }
            }
        }

        if (interactIndicator != null)
        {
            interactIndicator.transform.localEulerAngles = Vector3.up * MainManager.Instance.GetWorldspaceYaw();
        }

        //Debug.Log(actionState);

        //set up to run outside the controls check
        //not necessary*
        HazardUpdate();

        //note: has 1 thing that runs outside of the GetControlsEnabled check

        if (MainManager.Instance.GetControlsEnabled())
        {
            ControlUpdate();
            scriptedAnimation = false;
        }
        else 
        {
            switchBuffered = false;
            ScriptedUpdate();
        }
        SpecialUpdate();

        lastIntendedMovement = intendedMovement;

        if (switchTime > 0)
        {
            switchTime -= Time.deltaTime;

            float progress = (SWITCH_DURATION - switchTime) / SWITCH_DURATION;
            switchRotation = progress * 360f;
        }
        else
        {
            switchTime = 0;
            switchRotation = 0;
        }

        for (int i = 0; i < lights.Count; i++)
        {
            float time = Mathf.Floor(Time.time * 6f);
            lights[i].transform.localPosition = new Vector3(Mathf.Sin(time + i * Mathf.PI / 2) * 0.1f, 0.15f, Mathf.Cos(time + i * Mathf.PI / 2) * 0.1f);
        }

        if (encounterCooldown > 0)
        {
            encounterCooldown -= Time.deltaTime;
        } else
        {
            encounterCooldown = 0;
        }
    }

    //Potential future problem:
    //This is in Update()
    //  (when this is doing physics things so it should be in fixedupdate())
    void ControlUpdate()
    {
        Vector2 inputXY = Vector2.zero;

        inputXY.x = InputManager.GetAxisHorizontal();
        inputXY.y = InputManager.GetAxisVertical();
        if (inputXY.magnitude > 1)
        {
            inputXY = inputXY.normalized;
        }
        inputXY = MainManager.Instance.WorldspaceXZTransform(inputXY);


        if (MainManager.Instance.Cheat_NoClip && !MainManager.Instance.Cheat_RevolvingCam && !MainManager.Instance.Cheat_FreeCam)
        {
            if (actionState != ActionState.NoClip)
            {
                NoClipSetup();
                SetActionState(ActionState.NoClip);
            }
        } else
        {
            if (actionState == ActionState.NoClip)
            {
                SetActionState(ActionState.Fall);
            }
        }
        if (MainManager.Instance.Cheat_RevolvingCam && !MainManager.Instance.Cheat_FreeCam)
        {
            if (actionState != ActionState.RevolvingCam)
            {
                SetActionState(ActionState.RevolvingCam);
            }
        }
        else
        {
            if (actionState == ActionState.RevolvingCam)
            {
                SetActionState(ActionState.Fall);
            }
        }
        if (MainManager.Instance.Cheat_FreeCam)
        {
            if (actionState != ActionState.FreeCam)
            {
                SetActionState(ActionState.FreeCam);
            }
        }
        else
        {
            if (actionState == ActionState.FreeCam)
            {
                SetActionState(ActionState.Fall);
            }
        }


        //grounded = force neutral for fall states and stuff
        switch (actionState)
        {
            case ActionState.Jump:
            case ActionState.DoubleJump:
            case ActionState.SuperJump:
            case ActionState.Dash:
            case ActionState.Land:
            case ActionState.Aetherize:
            case ActionState.Illuminate:
            case ActionState.Dig:
            case ActionState.Slash:
            case ActionState.Smash:
            case ActionState.HazardFall:
            case ActionState.HazardTouch:
            case ActionState.WallJump:
            case ActionState.Hover:
            case ActionState.NoClip:
            case ActionState.FreeCam:
            case ActionState.RevolvingCam:
                break;
            default:
                if (IsGrounded())
                {
                    SetActionState(ActionState.Neutral);
                }
                break;
        }

        Vector3 currVel = rb.velocity;
        Vector3 newVelocity = rb.velocity;

        //Debug.Log(prevAttachedVel);

        switch (actionState)
        {
            case ActionState.SuperJump:
                float sairspeed = GetSpeed() * 0.025f * (60 * Time.deltaTime);

                //
                newVelocity = (rb.velocity.x + inputXY.x * sairspeed) * Vector3.right
                            + (rb.velocity.z + inputXY.y * sairspeed) * Vector3.forward
                            + rb.velocity.y * Vector3.up;
                newVelocity = EllipticalSpeedCap(newVelocity, GetSpeed());
                /*
                Vector2 sairxz = new Vector2(newVelocity.x, newVelocity.z);
                if (sairxz.magnitude > speed)
                {
                    sairxz = sairxz.normalized * speed;
                    newVelocity = (sairxz.x) * Vector3.right
                                + (sairxz.y) * Vector3.forward
                                + rb.velocity.y * Vector3.up;
                }
                */
                break;
            case ActionState.Dash:
            case ActionState.DashFall:
                float dairspeed = GetSpeed() * 0.05f * (60 * Time.deltaTime);
                if (hasTouchedObject)
                {
                    dairspeed = GetSpeed() * 0.025f * (60 * Time.deltaTime);
                }

                //
                newVelocity = (rb.velocity.x + inputXY.x * dairspeed) * Vector3.right
                            + (rb.velocity.z + inputXY.y * dairspeed) * Vector3.forward
                            + rb.velocity.y * Vector3.up;
                newVelocity = EllipticalSpeedCap(newVelocity, GetDashSpeed());
                /*
                //Vector2 dairxz = new Vector2(newVelocity.x, newVelocity.z);
                if (dairxz.magnitude > dashSpeed)
                {
                    dairxz = dairxz.normalized * dashSpeed;
                    newVelocity = (dairxz.x) * Vector3.right
                                + (dairxz.y) * Vector3.forward
                                + rb.velocity.y * Vector3.up;
                }
                */
                break;
            case ActionState.Fall:
            case ActionState.Jump:
            case ActionState.DoubleJump:
            case ActionState.AetherizeFall:
            case ActionState.IlluminateFall:
            case ActionState.WallGrab:
            case ActionState.WallJump:
            case ActionState.SuperKick:
                float airspeed = GetSpeed() * 0.15f * (60 * Time.deltaTime);
                if (hasTouchedObject)
                {
                    airspeed = GetSpeed() * 0.025f * (60 * Time.deltaTime);
                }
                if (actionState == ActionState.WallJump)
                {
                    airspeed = GetSpeed() * 0.025f * (60 * Time.deltaTime);
                }

                //
                newVelocity = (rb.velocity.x + inputXY.x * airspeed) * Vector3.right
                            + (rb.velocity.z + inputXY.y * airspeed) * Vector3.forward
                            + rb.velocity.y * Vector3.up;
                newVelocity = EllipticalSpeedCap(newVelocity, GetSpeed() * airspeedBonus);
                /*
                Vector2 airxz = new Vector2(newVelocity.x, newVelocity.z);
                if (airxz.magnitude > speed * airspeedBonus)
                {
                    airxz = airxz.normalized * speed * airspeedBonus;
                    newVelocity = (airxz.x) * Vector3.right
                                + (airxz.y) * Vector3.forward
                                + rb.velocity.y * Vector3.up;
                }
                */
                if (actionState == ActionState.WallGrab)
                {
                    rb.velocity += -wallVector * WALL_STICK_VELOCITY * Time.deltaTime;

                    if (KeruAsterJump() && currentCharacter == MainManager.PlayerCharacter.Luna)
                    {
                        rb.velocity += Physics.gravity * -0.5f * Time.deltaTime;    //slightly improper but close enough
                    }
                }
                break;
            case ActionState.Slash:
            case ActionState.Smash:
                //Brake your movement
                //Slash gets a tiny bit of movement control
                Vector2 weaponXY = inputXY * 0.5f;
                if (actionState == ActionState.Smash)
                {
                    weaponXY = Vector2.zero;
                }

                if (lastFloorSlippery || lastMovementDamping != 0)
                {
                    float icyMult = 1.2f;
                    float icyMovement = 0.025f * (60 * Time.deltaTime);
                    newVelocity = rb.velocity.x * Vector3.right + rb.velocity.z * Vector3.forward + weaponXY.x * (GetSpeed() * icyMovement) * Vector3.right + weaponXY.y * (GetSpeed() * icyMovement) * Vector3.forward + rb.velocity.y * Vector3.up;

                    newVelocity = EllipticalSpeedCap(newVelocity, GetDashSpeed() * icyMult);
                    /*
                    Vector2 icymove = new Vector2(newVelocity.x, newVelocity.z);
                    if (icymove.magnitude > dashSpeed * icyMult)
                    {
                        icymove = icymove.normalized * dashSpeed * icyMult;
                        newVelocity = (icymove.x) * Vector3.right
                                    + (icymove.y) * Vector3.forward
                                    + rb.velocity.y * Vector3.up;
                    }
                    */
                }
                else
                {
                    float weaponspeed = GetSpeed() * 0.15f * (60 * Time.deltaTime);

                    //newVelocity += prevAttachedVel;

                    Vector3 desiredVelocity = momentumVel + weaponXY.x * GetSpeed() * Vector3.right + weaponXY.y * GetSpeed() * Vector3.forward + rb.velocity.y * Vector3.up;
                    if ((newVelocity - desiredVelocity).magnitude < weaponspeed)
                    {
                        newVelocity = desiredVelocity;
                    }
                    else
                    {
                        newVelocity = newVelocity - (newVelocity - desiredVelocity).normalized * weaponspeed;
                    }

                    //stop problem with flying off moving platforms
                    //newVelocity -= prevAttachedVel;
                    if (!movingPlatformFix)
                    {
                        newVelocity -= momentumVel;
                        movingPlatformFix = true;
                    }
                }

                //Collided with an object, so by extension that stops you as well since you are holding the weapon
                //(Gives a better sense of impact)
                if (attackInterrupted)
                {
                    newVelocity = Vector3.zero;
                }
                break;
            case ActionState.Dig:
                float digspeed = GetSpeed() * 0.65f;
                newVelocity = inputXY.x * digspeed * Vector3.right + inputXY.y * digspeed * Vector3.forward + rb.velocity.y * Vector3.up;
                break;
            case ActionState.Land:
            case ActionState.Neutral:
                //Debug.Log(lastMovementDamping);
                //Debug.Log(lastFloorSlippery);
                if (lastFloorSlippery || lastMovementDamping != 0)
                {
                    float icyMult = 1.2f;
                    float icyMovement = 0.025f * (60 * Time.deltaTime);
                    newVelocity = rb.velocity.x * Vector3.right + rb.velocity.z * Vector3.forward + inputXY.x * (GetSpeed() * icyMovement) * Vector3.right + inputXY.y * (GetSpeed() * icyMovement) * Vector3.forward + rb.velocity.y * Vector3.up;

                    newVelocity = EllipticalSpeedCap(newVelocity, GetDashSpeed() * icyMult);
                    /*
                    Vector2 icymove = new Vector2(newVelocity.x, newVelocity.z);                        
                    if (icymove.magnitude > dashSpeed * icyMult)
                    {
                        icymove = icymove.normalized * dashSpeed * icyMult;
                        newVelocity = (icymove.x) * Vector3.right
                                    + (icymove.y) * Vector3.forward
                                    + rb.velocity.y * Vector3.up;
                    }
                    */
                }
                else if (lastFloorSticky)
                {
                    float stickyMult = 0.75f;
                    newVelocity = inputXY.x * GetSpeed() * stickyMult * Vector3.right + inputXY.y * GetSpeed() * stickyMult * Vector3.forward + rb.velocity.y * Vector3.up;
                }
                else
                {
                    newVelocity = inputXY.x * GetSpeed() * Vector3.right + inputXY.y * GetSpeed() * Vector3.forward + rb.velocity.y * Vector3.up;
                }
                break;
            case ActionState.Aetherize:
            case ActionState.Illuminate:
                float slowspeed = GetSpeed() * 0.65f;
                newVelocity = inputXY.x * slowspeed * Vector3.right + inputXY.y * slowspeed * Vector3.forward + rb.velocity.y * Vector3.up;
                break;
            case ActionState.HazardTouch:
                newVelocity = Vector3.zero;
                break;
            case ActionState.NoClip:
                newVelocity = inputXY.x * GetDashSpeed() * 2.5f * Vector3.right + inputXY.y * GetDashSpeed() * 2.5f * Vector3.forward;
                if (InputManager.GetButton(InputManager.Button.A))
                {
                    newVelocity += GetDashSpeed() * 2.5f * Vector3.up;
                }
                if (InputManager.GetButton(InputManager.Button.Z))
                {
                    newVelocity -= GetDashSpeed() * 2.5f * Vector3.up;
                }
                break;
            case ActionState.Hover:
                float hairspeed = GetSpeed() * 0.15f * (60 * Time.deltaTime);
                if (hasTouchedObject)
                {
                    hairspeed = GetSpeed() * 0.025f * (60 * Time.deltaTime);
                }

                //
                newVelocity = (rb.velocity.x + inputXY.x * hairspeed) * Vector3.right
                            + (rb.velocity.z + inputXY.y * hairspeed) * Vector3.forward
                            + rb.velocity.y * Vector3.up;
                newVelocity = EllipticalSpeedCap(newVelocity, GetSpeed() * airspeedBonus);
                newVelocity = newVelocity.x * Vector3.right + newVelocity.z * Vector3.forward + MainManager.EasingExponentialTime(rb.velocity.y, 0, 0.1f) * Vector3.up;
                break;
            case ActionState.Climb:
                //translate the input space to the plane of the normal
                Vector2 inputXYAlt = Vector2.zero;
                inputXYAlt.x = InputManager.GetAxisHorizontal();
                inputXYAlt.y = InputManager.GetAxisVertical();

                Vector3 newright = Vector3.Cross(Vector3.up, wallVector);
                newright = -newright.normalized;

                Vector3 newup = Vector3.Cross(wallVector, newright);
                newup = -newup.normalized;

                float climbspeed = GetSpeed();

                newVelocity = newright * inputXYAlt.x * (climbspeed) + newup * inputXYAlt.y * (climbspeed) + (-wallVector) * WALL_STICK_VELOCITY;
                break;
        }

        FixMaterials(inputXY);

        intendedMovement = newVelocity;
        rb.velocity = newVelocity;

        GroundStickyCalculation(inputXY);

        StepUpStairs(inputXY);
    }

    void ScriptedUpdate()
    {
        //don't?
        if (actionState == ActionState.LaunchFall)
        {
        } else
        {
            rb.velocity = scriptedInput.x * GetSpeed() * Vector3.right + scriptedInput.y * GetSpeed() * Vector3.forward + rb.velocity.y * Vector3.up;
        }

        scriptedInput = Vector3.zero;
        disabledTime = 0.5f;

        FixMaterials(scriptedInput);

        intendedMovement = rb.velocity;

        GroundStickyCalculation(scriptedInput);
        StepUpStairs(scriptedInput);
    }

    void SpecialUpdate()
    {
        if (disabledTime > 0)
        {
            disabledTime -= Time.deltaTime;
        } else
        {
            disabledTime = 0;
        }

        //tick aethertime and lighttime
        if (IsAether())
        {
            aetherTime += Time.deltaTime;
            notAetherTime = 0;

            if (aetherShaderTime >= 0.25f)
            {
                aetherShaderTime = 0.25f;
            }
            else
            {
                aetherShaderTime += Time.deltaTime;
                if (aetherShaderTime < 0)
                {
                    aetherShaderTime += 2 * Time.deltaTime;
                }
            }
        }
        else
        {
            notAetherTime += Time.deltaTime;
            aetherTime = 0;

            if (aetherShaderTime <= -0.25f)
            {
                aetherShaderTime = -0.25f;
            }
            else
            {
                aetherShaderTime -= Time.deltaTime;
                if (aetherShaderTime > 0)
                {
                    aetherShaderTime -= 2 * Time.deltaTime;
                }
            }
        }

        if (IsLight())
        {
            lightTime += Time.deltaTime;
            notLightTime = 0;

            if (lightShaderTime >= 0.25f)
            {
                lightShaderTime = 0.25f;
            }
            else
            {
                lightShaderTime += Time.deltaTime;
                if (lightShaderTime < 0)
                {
                    lightShaderTime += 2 * Time.deltaTime;
                }
            }
        }
        else
        {
            notLightTime += Time.deltaTime;
            lightTime = 0;

            if (lightShaderTime <= -0.25f)
            {
                lightShaderTime = -0.25f;
            }
            else
            {
                lightShaderTime -= Time.deltaTime;
                if (lightShaderTime > 0)
                {
                    lightShaderTime -= 2 * Time.deltaTime;
                }
            }
        }


        //others
        if (airTime > 0.05f)    //cloud platforms are being jittery (if I make a floor of them you can stand on it, not intended)
        {
            if (bubbleShaderTime >= 0.25f)
            {
                bubbleShaderTime = 0.25f;
            }
            else
            {
                bubbleShaderTime += 2 * Time.deltaTime;
                if (bubbleShaderTime < 0)
                {
                    bubbleShaderTime = 0;
                    //bubbleShaderTime += 2 * Time.deltaTime;
                }
            }
            if (!bubbleState)
            {
                CloudSetup();
                bubbleState = true;
            }
        }
        else
        {
            if (bubbleShaderTime <= -0.25f)
            {
                bubbleShaderTime = -0.25f;
            }
            else
            {
                bubbleShaderTime -= 2 * Time.deltaTime;
                if (bubbleShaderTime > 0)
                {
                    bubbleShaderTime = 0;
                    //bubbleShaderTime -= 2 * Time.deltaTime;
                }
            }
            if (bubbleState)
            {
                BubbleSetup();
                bubbleState = false;
            }
        }

        if (rb.velocity.y > 0.1f)
        {
            if (leafShaderTime >= 0.25f)
            {
                leafShaderTime = 0.25f;
            }
            else
            {
                leafShaderTime += 3 * Time.deltaTime;
                if (leafShaderTime < 0)
                {
                    leafShaderTime = 0;
                    //leafShaderTime += 2 * Time.deltaTime;
                }
            }
        }
        else
        {
            if (leafShaderTime <= -0.25f)
            {
                leafShaderTime = -0.25f;
            }
            else
            {
                leafShaderTime -= Time.deltaTime;
                if (leafShaderTime > 0)
                {
                    leafShaderTime = 0;
                    //leafShaderTime -= 2 * Time.deltaTime;
                }
            }
        }

        if (leafShaderTime < 0)
        {
            if (!leafState)
            {
                LeafSetup();
                leafState = true;
            }
        }
        else
        {
            if (leafState)
            {
                LeafUnsetup();
                leafState = false;
            }
        }
    }

    void FixMaterials(Vector2 inputXY)
    {
        if ((inputXY.magnitude == 0 && !(lastFloorSlippery || lastMovementDamping != 0 || !IsGrounded())) ^ useAllFriction)
        {
            if (inputXY.magnitude == 0 && !(lastFloorSlippery || lastMovementDamping != 0 || !IsGrounded()))
            {
                characterCollider.material = MainManager.Instance.allFrictionMaterial;
                useAllFriction = true;
            }
            else
            {
                characterCollider.material = MainManager.Instance.noFrictionMaterial;
                useAllFriction = false;
            }
        }
    }

    void GroundStickyCalculation(Vector2 inputXY)
    {
        //Perform ground sticky calculation to stop you from flying off slopes weirdly
        //Also stop you from sliding downhill (because I turned off friction to make speed work consistently)
        switch (actionState)
        {
            case ActionState.Land:
            case ActionState.Neutral:
            case ActionState.Aetherize:
            case ActionState.Illuminate:
                //case ActionState.Dig:     Special exception
                //version 1
                /*
                RaycastHit hit = DownRaycast(0.1f);
                if (hit.point.y < transform.position.y - HEIGHT/2)
                {
                    rb.velocity = rb.velocity - rb.velocity.y * Vector3.up + Vector3.down * STICKY_FORCE;
                }
                */

                //version 2
                //This needs to be based on input dir and slope normal (so that going uphill and downhill work properly)

                float dot = Vector3.Dot(MainManager.XZProjectReverse(inputXY), floorNormal);

                //Positive dot = going downhill, negative = uphill
                //Zero dot is neutral (which is neither uphill or downhill)
                //  Input dir is in the XZ plane (plane of no change in altitude), and the plane of vectors that are perpendicular to the normal intersects the input dir in 2 places
                //  (and these are contour lines)

                //Debug.Log(floorNormal + " " + dot);


                //hacky
                //if (floorNormal.y < 0.8f)
                //{
                //    dot *= 1.4f;
                //}

                //need to adjust this so that the velocity vector is pointed below the slope (so you stay on the incline without flying off)

                //rb.velocity = rb.velocity - rb.velocity.y * Vector3.up + Vector3.down * (STICKY_FORCE * dot + STICKY_CONST);

                //get an uphill vector for friction calculation

                //Vector3 uphill = Vector3.ProjectOnPlane(Vector3.up, floorNormal);

                //rb.velocity += uphill * STICKY_FRICTION;


                //version 3: redirect the existing velocity to move along the slope (note: this makes hill speed same as grounded)
                //note 2: this is very sticky, if you have a ton of y velocity here it will remove it all

                if (!(lastFloorSlippery || lastMovementDamping != 0))
                {
                    Vector3 project(Vector3 a, Vector3 b)
                    {
                        return a - b * Vector3.Dot(a, b);
                    }

                    float magA = (rb.velocity.x * Vector3.right + rb.velocity.z * Vector3.forward).magnitude;

                    rb.velocity = project(rb.velocity.x * Vector3.right, floorNormal) + project(rb.velocity.z * Vector3.forward, floorNormal);

                    float magB = rb.velocity.magnitude;

                    //Debug.Log(rb.velocity.x);

                    if (dot > 0 && magB > 0)
                    {
                        rb.velocity *= magA / magB;
                    }
                }
                break;
        }
    }
    void StepUpStairs(Vector2 inputXY)
    {
        //Step up stairs (note: only up direction, snap to floor is used for down direction)
        Vector3 nearGroundMin;  //Lower bound point

        //calculation:

        //bottom
        nearGroundMin = transform.position + Vector3.down * (height / 2);

        Vector3 xzvel = rb.velocity;
        xzvel -= Vector3.up * xzvel.y;
        float y = rb.velocity.y;

        y /= xzvel.magnitude;
        xzvel = xzvel.normalized;

        xzvel += y * Vector3.up;
        nearGroundMin += xzvel * (width * 0.5f);

        float extraHeight = STEP_UP_MAX_HEIGHT;

        float lowerHeight = 0;

        //Debug.Log(isGrounded + " " + touchingNonGround);
        if (IsGrounded() && touchingNonGround)
        {
            //try to raycast
            RaycastHit checkerPre;
            //Physics.Raycast(transform.position + Vector3.down * (height / 2) + Vector3.up * extraHeight, xzvel, out checkerPre, (width + 0.15f), PLAYER_LAYER_MASK);
            checkerPre = HullCast(width * 0.5f, transform.position + Vector3.down * (height / 2) + Vector3.up * extraHeight, xzvel, PLAYER_LAYER_MASK);

            RaycastHit checker;
            //Physics.Raycast(nearGroundMin + Vector3.up * extraHeight, Vector3.down, out checker, extraHeight + lowerHeight, PLAYER_LAYER_MASK);
            //Debug.DrawRay(nearGroundMin + Vector3.up * extraHeight, Vector3.down, Color.yellow, extraHeight);
            checker = HullCast(extraHeight + lowerHeight, nearGroundMin + Vector3.up * (height / 2) + Vector3.up * extraHeight, Vector3.down, PLAYER_LAYER_MASK);
            float delta = 0;

            float dotB = Vector3.Dot(MainManager.XZProjectReverse(inputXY), floorNormal);

            //Debug.Log((checker.collider != null) + " " + (checkerPre.collider == null));
            if (checker.collider != null && checkerPre.collider == null)
            {
                delta = checker.point.y - nearGroundMin.y;
                //Debug.Log(delta);
                //weird wedges don't trigger step check
                if (delta > STEP_UP_MINIMUM && delta < STEP_UP_MAX_HEIGHT && dotB < 0.3f)
                {
                    delta += STEP_UP_BONUS;

                    //Last step check
                    RaycastHit checkerB = HullCast(delta, Vector3.up);
                    if (checkerB.collider == null)
                    {
                        transform.position += Vector3.up * delta;

                        //Debug.Log("Adjust by " + delta);
                        timeSinceStepUp = 0;
                    }
                    else
                    {
                        //Debug.Log("Fail last check");
                    }
                }
            }
        }
    }


    //Cap the velocity to the cap (but the previous platform adds a boost)
    //Note: Bad things happen if the foci are too far apart (distance between them is higher than distance / 2)
    //Note 2: Only caps XZ velocity
    public Vector3 EllipticalSpeedCap(Vector3 targetVel, float cap)
    {
        //Debug.Log(targetVel + " " + prevAttachedVel + " cap: " + cap);
        Vector3 focusA = Vector3.zero;
        Vector3 focusB = momentumVel;
        Vector3 output = targetVel;
        targetVel.y = 0;
        focusA.y = 0;
        focusB.y = 0;

        //Apply lerp
        focusB = MLerp(focusB);

        float trueCap = cap + (focusB).magnitude;

        if (focusA == focusB)
        {
            //Degenerate case
            Vector3 delta = targetVel - focusA;

            delta.y = 0;

            if (targetVel.magnitude > trueCap)
            {
                targetVel = targetVel.normalized * trueCap;
                output = targetVel.x * Vector3.right + targetVel.z * Vector3.forward + output.y * Vector3.up;
                return output;
            } else
            {
                return output;
            }
        } else
        {
            //wacky formula
            float dot = Vector3.Dot(focusB, targetVel);
            dot /= targetVel.magnitude;
            dot /= focusB.magnitude;
            float e = (trueCap - cap) / trueCap;

            float ellipseDistance = targetVel.magnitude / ((1 - e) / (1 - e * dot));
            //Debug.Log("Elliptical distance: " + ellipseDistance + " cap: " + trueCap);

            if (ellipseDistance > trueCap) {
                //cap
                targetVel = targetVel * (trueCap / ellipseDistance);
                output = targetVel.x * Vector3.right + targetVel.z * Vector3.forward + output.y * Vector3.up;
                //Debug.Log("Cap to " + output);
                return output;
            }
            else
            {
                return output;
            }
        }
    }

    public void HazardUpdate()
    {
        //Work with hazards and stuff

        switch (actionState)
        {
            case ActionState.HazardFall:
            case ActionState.HazardTouch:
                HazardInnerUpdate();
                break;
            default:
                hazardState = 0;
                hazardTime = 0;
                break;
        }
    }
    public void HazardInnerUpdate()
    {
        IEnumerator Fadeout()
        {
            yield return StartCoroutine(MainManager.Instance.FadeToBlack());
            hazardState = 3;
        }

        IEnumerator Fadein()
        {
            mapScript.HazardReset();
            transform.position = lastFloorPos;
            rb.velocity = Vector3.zero;
            SetActionState(ActionState.Neutral);
            if (!MainManager.Instance.Cheat_SplitParty)
            {
                FollowerWarpSetState();
            }
            yield return StartCoroutine(MainManager.Instance.UnfadeToBlack());
        }

        //Debug.Log(hazardState + " " + hazardTime);
        //0 = tick up animation timer
        //1 = fadeout
        //2 = wait for fadeout
        //3 = fadein
        //4 = wait for fadein (*not really used since the coroutine immediately ends the hazard state)
        switch (hazardState)
        {
            case 0:
                if (hazardTime > HAZARD_ANIM_TIME)
                {
                    hazardState = 1;
                } else
                {
                    hazardTime += Time.deltaTime;
                }
                break;
            case 1:
                StartCoroutine(Fadeout());
                hazardState = 2;
                break;
            case 3:
                //stop unfair stuff with spawning inside enemies
                //but if you quickly jump into a hazard to keep up the cooldown you might be able to cheese stuff?
                //Seems impractical though
                encounterCooldown = 0.5f;
                StartCoroutine(Fadein());
                hazardState = 4;
                break;
        }
    }

    public void WarpFollowing(int followerIndex)
    {
        for (int i = followerIndex + 1; i < followers.Count; i++)
        {
            followers[i].Warp();
        }
    }

    public float MinGroundNormal()
    {
        if (actionState == ActionState.Dig)
        {
            return MIN_GROUND_DIG_NORMAL;
        } else
        {
            return MIN_GROUND_NORMAL;
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        ProcessTrigger(other);
    }
    public void OnTriggerStay(Collider other)
    {
        ProcessTrigger(other);
    }

    public void ProcessTrigger(Collider other)
    {
        if (undigTimer > 0)
        {
            IUndigTrigger ud = other.gameObject.GetComponent<IUndigTrigger>();
            if (ud != null)
            {
                ud.Undig();
            }
        }
    }

    public override void ProcessCollision(Collision collision)
    {
        hasTouchedObject = true;

        float accumulatedGroundedHeight = 0;
        Vector3 accumulatedFloorNormal = Vector3.zero;
        int accumulatedGroundedCount = 0;

        float accumulatedNonGroundedHeight = 0;
        float accumulatedNonGroundedCount = 0;
        Vector3 accumulatedNonGrounded = Vector3.zero;

        bool headHit = false;

        Vector3 accumulatedNPC = Vector3.zero;
        float accumulatedNPCCount = 0;

        float accumulatedWallCount = 0;
        Vector3 accumulatedWall = Vector3.zero;

        foreach (ContactPoint contact in collision.contacts)
        {
            if (contact.normal.normalized[1] > MinGroundNormal() && LegalGround(collision)) //0.75 = ~40 degree incline is maximum (0.75 is a slope that is roughly a 8-9 triangle (that triangle is barely not traversable))
            {
                accumulatedGroundedCount++;
                accumulatedGroundedHeight += contact.point.y;
                accumulatedFloorNormal += contact.normal.normalized;


                //DoLanding(collision);
            }
            else
            {
                accumulatedNonGroundedCount++;
                accumulatedNonGrounded += contact.normal.normalized;
                accumulatedNonGroundedHeight += contact.point.y;
                //Bonk into things (dash hop -> aerial state also)
                switch (actionState)
                {
                    case ActionState.Dash:
                    case ActionState.DashFall:
                        SetActionState(ActionState.Fall);
                        TryDashHopKick(collision, contact.point, contact.normal);
                        break;
                }

                touchingNonGround = true;
            }

            if (contact.normal.normalized[1] > 0.5f && collision.transform.CompareTag("NPC"))
            {
                accumulatedNPCCount++;
                //need to use this number instead so that the xz component is never 0 (but the collision normal might end up being 0)
                accumulatedNPC += (contact.otherCollider.transform.position - contact.thisCollider.transform.position); //contact.normal.normalized;
            }

            if (contact.normal.normalized[1] < -MinGroundNormal())
            {
                headHit = true;
            }

            if (Mathf.Abs(contact.normal.normalized[1]) < WALL_THRESHOLD)
            {
                accumulatedWallCount++;
                accumulatedWall += contact.normal.normalized;
            }

            if (accumulatedWallCount > 0)
            {
                accumulatedWall /= accumulatedWallCount;
                if (!headHit && !IsGrounded() && accumulatedWall.magnitude > 0.5f && !collision.collider.CompareTag("InvisibleWall"))
                {
                    stuckToWall = true;
                    wallVector = accumulatedWall.normalized;
                }
            }
        }

        if (headHit && !IsGrounded() && lastFrameVel.y > 0)
        {
            TryHeadHit(collision);
        }

        bool landed = false;
        if (!AntiGravity() && !HazardState() && accumulatedGroundedCount > 0 && Vector3.Dot(rb.velocity.normalized, accumulatedFloorNormal.normalized) < 0.2f)
        {
            lastGroundedHeight = accumulatedGroundedHeight / accumulatedGroundedCount;
            lastHighestHeight = lastGroundedHeight;
            floorNormal = accumulatedFloorNormal.normalized;
            attached = collision.rigidbody;
            landed = true;
            DoLanding(collision);
        }

        //Special case to stop you from getting trapped in a wedge spot
        //(but maps shouldn't have any wedge spots anyway)
        if (!IsGrounded() && accumulatedNonGroundedCount > 0)
        {
            accumulatedNonGrounded = accumulatedNonGrounded.normalized;
            bool legalStomp = false;
            if (!AntiGravity() && !HazardState() && accumulatedNonGrounded.y > MinGroundNormal() && LegalGround(collision) && Vector3.Dot(rb.velocity.normalized, accumulatedNonGrounded.normalized) < 0.2f)
            {
                legalStomp = true;
                lastGroundedHeight = accumulatedNonGroundedHeight / accumulatedNonGroundedCount;
                lastHighestHeight = lastGroundedHeight;
                floorNormal = accumulatedNonGrounded;
                attached = collision.rigidbody;

                landed = true;
                DoLanding(collision);
            }

            if (!legalStomp && !AntiGravity() && !HazardState() && accumulatedNonGrounded.y > 0.4)
            {
                TryStomp(collision);
            }
        }

        if (accumulatedNPCCount > 0)
        {
            Vector3 push = accumulatedNPC / accumulatedNPCCount;
            push.y = 0;
            push = -push.normalized;

            //Debug.Log(push);

            Vector3 xz = rb.velocity;
            xz.y = 0;

            float magnitude = xz.magnitude;
            float dot = Vector3.Dot(xz, push);

            float k = 1 - dot;

            float pushPower = magnitude * k;

            if (encounterCooldown > 0)
            {
                if (pushPower < NPC_ANTI_STACK_VELOCITY)
                {
                    rb.velocity += NPC_ANTI_STACK_VELOCITY * Time.fixedDeltaTime * push * 10;
                }
            }
            else
            {
                if (pushPower < NPC_STRONG_ANTI_STACK_VELOCITY)
                {
                    rb.velocity += NPC_STRONG_ANTI_STACK_VELOCITY * Time.fixedDeltaTime * push * 10;
                }
            }

            //Debug.Log(rb.velocity + " pushed by " + accumulatedNPC + " force " + (NPC_ANTI_STACK_VELOCITY * Time.fixedDeltaTime * push * 10));
        }

        //Ouch
        bool hc = HazardCollisionCheck(collision);
        if (!HazardState() && hc)
        {
            SetActionState(ActionState.HazardTouch);
            //Debug.Log("Hazard");
        }

        if (landed && groundedTime <= 0)
        {
            //landing animation
            IEnumerator StompAnimation()
            {
                if (landAnimation)
                {
                    yield break;
                }

                landAnimation = true;
                float time = 0;
                float duration = 0.10f;
                float stompscale = 0.35f / height; // 0.25f;
                while (time < duration)
                {
                    time += Time.deltaTime;
                    float lerpVal = Mathf.Min(2 * (time / duration), 2 - 2 * (time / duration));
                    ac.transform.localScale = Vector3.Lerp(Vector3.one, (Vector3.up) * (1 - stompscale) + (Vector3.right + Vector3.forward) * (1 + stompscale), lerpVal);
                    //need to position things lower to make it look correct
                    //ac.transform.localPosition = Vector3.down * 0.5f * (height * (lerpVal * stompscale));
                    yield return null;
                }

                ac.transform.localScale = Vector3.one;
                landAnimation = false;
            }
            StartCoroutine(StompAnimation());
        }
    }

    public void DoLanding(Collision collision)
    {
        //forbid landing right after launch to fix a bug
        if (timeSinceLaunch < Time.fixedDeltaTime * 2)
        {
            return;
        }

        //lastGroundedHeight = contact.point.y;
        //floorNormal = contact.normal.normalized;
        isGrounded = true;
        canDoubleJump = false;
        canHover = false;

        SetFloorTags(collision);

        //Convert aerial states into their corresponding grounded state
        //first, double check for lack of y velocity (may lead to problems later though, if you land on an uphill slope without triggering this with negative velocity)
        if (timeSinceLastJump > 0.06f || rb.velocity.y <= 0)
        {
            switch (actionState)
            {
                case ActionState.Fall:
                case ActionState.LaunchFall:
                case ActionState.Jump:
                case ActionState.DoubleJump:
                case ActionState.SuperJump:
                case ActionState.WallJump:
                case ActionState.Dash:
                case ActionState.DashFall:
                case ActionState.WallGrab:
                case ActionState.Climb:
                    SetActionState(ActionState.Land);
                    if (landShockwavePossible)
                    {
                        landShockwavePossible = false;
                        TryLandShockwave(collision);
                    }
                    TryStomp(collision);
                    groundedTime = 0;
                    break;
                case ActionState.AetherizeFall:
                    SetActionState(ActionState.Aetherize);
                    groundedTime = 0;
                    break;
                case ActionState.IlluminateFall:
                    SetActionState(ActionState.Illuminate);
                    groundedTime = 0;
                    break;
                case ActionState.HazardFall:
                    SetActionState(ActionState.HazardTouch);
                    groundedTime = 0;
                    break;
            }
        }
        else
        {
            //Double jump is really not supposed to be touching anything so add a check also
            //Dash is very prone to landing uphill since it is a nearly horizontal jump

            switch (actionState)
            {
                case ActionState.DoubleJump:
                    //case ActionState.Dash:
                    //case ActionState.DashFall:
                    SetActionState(ActionState.Land);
                    groundedTime = 0;
                    break;
            }
        }
    }

    public override void DoSemisolidLanding(Vector3 position, Vector3 normal, Rigidbody attached, bool snapBelow)
    {
        usedFirstBounce = false;
        semisolidFloorActive = true;
        semisolidFloorNormal = normal;
        semisolidFloorPosition = position;
        floorNormal = semisolidFloorNormal;
        lastGroundedHeight = semisolidFloorPosition.y;
        lastHighestHeight = lastGroundedHeight;
        this.semisolidSnapBelow = snapBelow;
        this.attached = attached;


        canDoubleJump = false;
        canHover = false;

        //Convert aerial states into their corresponding grounded state
        //first, double check for lack of y velocity (may lead to problems later though, if you land on an uphill slope without triggering this with negative velocity)
        if (timeSinceLastJump > 0.06f || rb.velocity.y <= 0)
        {
            switch (actionState)
            {
                case ActionState.Fall:
                case ActionState.LaunchFall:
                case ActionState.Jump:
                case ActionState.DoubleJump:
                case ActionState.SuperJump:
                case ActionState.WallJump:
                case ActionState.Dash:
                case ActionState.DashFall:
                case ActionState.WallGrab:
                case ActionState.Climb:
                    SetActionState(ActionState.Land);
                    if (landShockwavePossible)
                    {
                        landShockwavePossible = false;
                    }
                    groundedTime = 0;
                    break;
                case ActionState.AetherizeFall:
                    SetActionState(ActionState.Aetherize);
                    groundedTime = 0;
                    break;
                case ActionState.IlluminateFall:
                    SetActionState(ActionState.Illuminate);
                    groundedTime = 0;
                    break;
                case ActionState.HazardFall:
                    SetActionState(ActionState.HazardTouch);
                    groundedTime = 0;
                    break;
            }
        }
        else
        {
            //Double jump is really not supposed to be touching anything so add a check also
            //Dash is very prone to landing uphill since it is a nearly horizontal jump

            switch (actionState)
            {
                case ActionState.DoubleJump:
                    //case ActionState.Dash:
                    //case ActionState.DashFall:
                    SetActionState(ActionState.Land);
                    groundedTime = 0;
                    break;
            }
        }
    }

    public void SetFloorTags(Collision collision)
    {
        SetFloorTags(collision.collider);
    }
    public void SetFloorTags(Rigidbody rigidbody)
    {
        SetFloorTags(rigidbody.GetComponent<Collider>());
    }
    public void SetFloorTags(Collider collider)
    {
        if (collider.CompareTag("NoDig"))
        {
            lastFloorNoDig = true;
        }
        else
        {
            lastFloorNoDig = false;
        }

        if (collider.CompareTag("DigThrough"))
        {
            lastFloorDigThrough = true;
        }
        else
        {
            lastFloorDigThrough = false;
        }

        if (collider.CompareTag("Slippery"))
        {
            lastFloorSlippery = true;
            lastFloorNoDig = true;
        }
        else
        {
            lastFloorSlippery = false;
        }

        if (collider.CompareTag("Sticky"))
        {
            lastFloorSticky = true;
        }
        else
        {
            lastFloorSticky = false;
        }

        if (collider.CompareTag("Unstable") || collider.CompareTag("Launcher") || collider.CompareTag("Hazard") || collider.CompareTag("InvisibleWall"))
        {
            lastFloorUnstable = true;
        }
        else
        {
            lastFloorUnstable = false;
        }
    }

    public bool HazardCollisionCheck(Collision collision)
    {
        if (collision.collider.CompareTag("Hazard"))
        {
            return true;
        }

        return false;
    }

    public override float GetSpeed()
    {
        float bonusSpeed = 1;
        bool hasteStep = MainManager.Instance.playerData.BadgeEquipped(Badge.BadgeType.HasteStep);
        if (hasteStep)
        {
            bonusSpeed = 1f + 0.2f * MainManager.Instance.playerData.BadgeEquippedCount(Badge.BadgeType.HasteStep);
        }
        return speed * bonusSpeed;
    }
    public float GetDashSpeed()
    {
        float bonusSpeed = 1;
        bool hasteStep = MainManager.Instance.playerData.BadgeEquipped(Badge.BadgeType.HasteStep);
        if (hasteStep)
        {
            bonusSpeed = 1f + 0.2f * MainManager.Instance.playerData.BadgeEquippedCount(Badge.BadgeType.HasteStep);
        }
        return dashSpeed * bonusSpeed;
    }

    public override void WorldFixedUpdate()
    {
        timeSinceLastJump += Time.fixedDeltaTime;

        if (digTimer > 0)
        {
            digTimer -= Time.fixedDeltaTime;
        }
        else
        {
            digTimer = 0;
        }
        if (undigTimer > 0)
        {
            undigTimer -= Time.fixedDeltaTime;
        } else
        {
            undigTimer = 0;
        }

        //FollowerTrailUpdate();

        PerpetualParticleUpdate();

        SpriteRotationUpdate();

        SpriteAnimationUpdate();

        DropShadowUpdate();

        //jumping on the first possible frame will set you to non-grounded and you would always fail this check even on legal ground
        //so it has to be before jumping is checked
        if (!Unstable() && !HazardState())
        {
            //positions halfway off the edge are not safe
            //note: start the raycast slightly above the floor or else it doesn't see the floor sometimes for some reason
            RaycastHit test = DownRaycast(0.05f, 0.025f);
            if (test.collider != null)
            {
                lastFloorPos = transform.position;
            }
        }


        if (concealedZone)
        {
            concealedTime += Time.fixedDeltaTime;
        } else
        {
            concealedTime = 0;
        }


        //Note: watch out for problems relating to switching right before controls are disabled
        if (switchTime < SWITCH_DURATION / 2 && switchBuffered)
        {
            MainManager.Instance.playerData.SwitchOrder();
            MainManager.Instance.ResetHUD();
            //who is front?
            SetIdentity(MainManager.Instance.playerData.party[0].entityID);
            followers[0].SetIdentity(MainManager.Instance.playerData.party[1].GetSpriteID());
            switchBuffered = false;

            if (MainManager.Instance.Cheat_SplitParty)
            {
                Vector3 followerPos = followers[0].transform.position;
                //not guaranteed to be stationary, so copy velocity too
                Vector3 followerVel = followers[0].rb.velocity;
                float trueRotation = followers[0].trueFacingRotation;
                followers[0].transform.position = gameObject.transform.position;
                followers[0].rb.velocity = rb.velocity;
                followers[0].SetTrueFacingRotation(trueFacingRotation);
                rb.velocity = followerVel;
                gameObject.transform.position = followerPos;
                SetTrueFacingRotation(trueRotation);
            }
        }

        switch (actionState)
        {
            case ActionState.AetherizeFall:
                AetherUpdate();
                break;
            case ActionState.IlluminateFall:
                LightUpdate();
                break;
            case ActionState.Hover:
                if (hoverTime < HOVER_DURATION)
                {
                    hoverTime += Time.fixedDeltaTime;
                }

                if (hoverTime >= HOVER_DURATION || !InputManager.GetButton(InputManager.Button.A))
                {
                    SetActionState(ActionState.Fall);
                }
                break;
            case ActionState.Climb:
                if (!stuckToWall || !InputManager.GetButton(InputManager.Button.A))
                {
                    SetActionState(ActionState.Fall);
                }
                break;
        }

        if (IsGrounded())
        {
            groundedTime += Time.fixedDeltaTime;
            airTime = 0;
            airFrames = 0;
            if (actionState == ActionState.Land && groundedTime > LAND_TIME)
            {
                SetActionState(ActionState.Neutral);
            }

            if (MainManager.Instance.GetControlsEnabled())
            {
                bool tattle = false;
                //tattle
                //add some leniency because the input is being wacky in build
                //(Note: likely a problem with me putting input code in fixed update)
                bool bufferTattle = disabledTime == 0 && InputManager.GetButton(InputManager.Button.Y) && (!InputManager.GetButtonHeldLonger(InputManager.Button.Y, CONTROL_HOLD_TIME));
                if ((InputManager.GetButtonDown(InputManager.Button.Y) || bufferTattle) && InNeutralState())
                {
                    TryTattle();
                    tattle = true;
                }


                //switching out
                //still a "neutral" state but there are rules against stuff
                //note: has special buffer too (but I don't need to use the complicated window that jump buffering has since there is no way to instant cancel a switch)
                //used a wider window than jump buffer since you are probably not going to be switching as often
                bool bufferSwitch = disabledTime == 0 && InputManager.GetButton(InputManager.Button.Z) && (!InputManager.GetButtonHeldLonger(InputManager.Button.Z, CONTROL_HOLD_TIME));
                if (followers != null && followers.Count > 0 && MainManager.Instance.playerData.party.Count > 1 && (InputManager.GetButtonDown(InputManager.Button.Z) || bufferSwitch) && InNeutralState() && !tattle)
                {
                    switch (actionState)
                    {
                        case ActionState.Land:
                        case ActionState.Neutral:
                            MainManager.Instance.PlaySound(gameObject, MainManager.Sound.Menu_CharacterSwap);
                            switchTime = SWITCH_DURATION;
                            switchBuffered = true;
                            enableDashLeniency = false;
                            break;
                    }
                }
                
                //Undig special thing
                if (actionState == ActionState.Neutral && undigTimer > 0)
                {
                    subObject.transform.localPosition = Vector3.down * 0.325f * ((undigTimer / DIG_TIME)) + Vector3.down * height / 2;
                }
                if (actionState != ActionState.Neutral)
                {
                    undigTimer = 0;
                    subObject.transform.localPosition = Vector3.zero + Vector3.down * height / 2;
                }

                coyoteBool = false;
                switch (actionState)
                {
                    case ActionState.Dig:
                        if (InputManager.GetButton(InputManager.Button.A) && !lastFloorNoDig && !noDigZone) //note: if you go onto illegal ground but somehow fail the TryUndig check sus things happen
                        {
                            DigUpdate();
                        }
                        else
                        {
                            TryUndig();
                        }
                        break;
                    case ActionState.Aetherize:
                    //case ActionState.AetherFall:
                        if (!noAetherZone && (InputManager.GetButton(InputManager.Button.B) || !aetherCheck.CollisionCheck()))
                        {
                            AetherUpdate();
                        }
                        else
                        {
                            DeAether();
                            SetActionState(ActionState.Neutral);
                        }
                        break;
                    case ActionState.Illuminate:
                    //case ActionState.LightHoldFall:
                        if (InputManager.GetButton(InputManager.Button.B) || !lightCheck.CollisionCheck())
                        {
                            LightUpdate();
                        }
                        else
                        {
                            DeLight();
                            SetActionState(ActionState.Neutral);
                        }
                        break;
                    case ActionState.Slash:
                        if (attackTime < SWORD_ANIM_TIME)
                        {
                            attackTime += Time.fixedDeltaTime;
                            if (!attackInterrupted)
                            {
                                visualAttackTime += Time.fixedDeltaTime;
                            }
                        }

                        if (reswingBuffer != 0 || InputManager.GetButtonPressShorter(InputManager.Button.B, CONTROL_HOLD_TIME))    // || bufferWeapon)
                        {
                            reswingBuffer += Time.fixedDeltaTime;
                        }

                        if (attackTime >= SWORD_ANIM_TIME)
                        {
                            //attackTime = 0;
                            //visualAttackTime = 0;
                            bool reswingA = reswingBuffer != 0 && reswingBuffer < CONTROL_HOLD_TIME;
                            SetActionState(ActionState.Neutral);
                            //ResetWeapon();
                            if (reswingA)
                            {
                                //Debug.Log("reswing");
                                SetActionState(ActionState.Slash);
                                StartSlash();
                            }
                        }
                        break;
                    case ActionState.Smash:
                        if (attackTime < HAMMER_ANIM_TIME)
                        {
                            attackTime += Time.fixedDeltaTime;
                            if (!attackInterrupted)
                            {
                                visualAttackTime += Time.fixedDeltaTime;
                            }
                        }

                        if (reswingBuffer != 0 || InputManager.GetButtonPressShorter(InputManager.Button.B, CONTROL_HOLD_TIME))    // || bufferWeapon)
                        {
                            reswingBuffer += Time.fixedDeltaTime;
                        }

                        if (attackTime >= HAMMER_ANIM_TIME)
                        {
                            //attackTime = 0;
                            bool reswingB = reswingBuffer != 0 && reswingBuffer < CONTROL_HOLD_TIME;
                            SetActionState(ActionState.Neutral);
                            //ResetWeapon();
                            if (reswingB)
                            {
                                //Debug.Log("reswing");
                                SetActionState(ActionState.Smash);
                                StartSmash();
                            }
                        }
                        break;
                    default:
                        coyoteBool = true;
                        break;
                }


                //Quite complex condition
                //so you press the jump button a few frames early and it will still register right

                //Why this condition?
                //Get a held button to make it less weird (so you never jump with A not pressed)
                //make sure A was not held for longer than the given window (i.e. you started pressing A some time in the past few moments)
                //Given window is the min of the normal window and time since last jump + 0.02
                //Without the time since last jump check, it may be possible to jump twice with one input, which seems wrong (you have to instantly land from a jump though)

                bool bufferJump = disabledTime == 0 && InputManager.GetButton(InputManager.Button.A) && (!InputManager.GetButtonHeldLonger(InputManager.Button.A, timeSinceLastJump + 0.02f < BUFFER_JUMP_WINDOW ? timeSinceLastJump + 0.02f : BUFFER_JUMP_WINDOW));
                bool dashBufferJump = disabledTime == 0 && InputManager.GetButton(InputManager.Button.A) && (!InputManager.GetButtonHeldLonger(InputManager.Button.A, timeSinceLastJump + 0.02f < BUFFER_DASH_JUMP_WINDOW ? timeSinceLastJump + 0.02f : BUFFER_DASH_JUMP_WINDOW));

                //A tap input (similar idea to how the buffer jump input is checked)
                bool dashJoystickInput = (!InputManager.GetNeutralJoystick() && InputManager.GetTimeSinceNeutralJoystick() < DASH_DIRECTIONAL_WINDOW);
                dashJoystickInput |= (enableDashLeniency && !InputManager.GetNeutralJoystick() && (groundedTime < BUFFER_DASH_JUMP_WINDOW));
                dashJoystickInput &= disabledTime == 0;

                bool interact = false;
                //Attempt to interact with stuff (if so then ignore everything below)
                //note: has bufferjump here for leniency (input is weird in build, probably another fixed update problem)
                if (InNeutralState() && (InputManager.GetButtonDown(InputManager.Button.A) || bufferJump) && !tattle && interactCooldown == 0)
                {
                    interactCooldown = 0.05f;
                    interact = MainManager.Instance.TryInteract();
                }

                //Debug.Log(interact);

                aetherLightCooldown += Time.fixedDeltaTime;

                //Special: allow jumping but forbid certain other actions out of a switch
                if (InNeutralState(true) && !interact && !tattle)
                {
                    //the timer check prevents glitchy thing where you jump under a low ceiling
                    //may also be linked to the "jump and double jump on the same frame" bug I was having earlier
                    if (timeSinceLastJump > 2 * Time.fixedDeltaTime && (InputManager.GetButtonDown(InputManager.Button.A) || bufferJump))
                    {
                        //which jump?
                        bool didMove = false;

                        if (!KeruAsterJump() && currentCharacter == MainManager.PlayerCharacter.Wilex)
                        {
                            if (Unlocked_SuperJump() && !didMove && InputManager.SpinInput())
                            {
                                didMove = true;
                                StartSuperJump();
                            }
                        }

                        if (!KeruAsterJump() && currentCharacter == MainManager.PlayerCharacter.Luna)
                        {
                            if (Unlocked_Dig() && switchTime <= 0 && !didMove && InputManager.SpinInput())
                            {
                                didMove = true;
                                if (!lastFloorNoDig)    //note: spinput on illegal ground will eat your jump input
                                {
                                    SetActionState(ActionState.Dig);
                                    Dig();
                                }
                            }

                            if (Unlocked_DashHop() && !didMove && dashJoystickInput)
                            {
                                didMove = true;
                                StartDashHop();
                            }
                        }                     
                        
                        if (KeruAsterJump() && currentCharacter == MainManager.PlayerCharacter.Luna)
                        {
                            if (Unlocked_DashHop() && switchTime <= 0 && !didMove && InputManager.SpinInput())
                            {
                                HoverJump();
                                HoverSetup();
                                SetActionState(ActionState.Hover);
                            }
                        }

                        if (!didMove && !applyDashJumpLift)
                        {
                            didMove = true;
                            StartJump();
                        }
                    } else if (timeSinceLastJump > 2 * Time.fixedDeltaTime && (InputManager.GetButtonDown(InputManager.Button.A) || dashBufferJump))
                    {
                        //the dash hop window is more lenient
                        if (!KeruAsterJump() && currentCharacter == MainManager.PlayerCharacter.Luna)
                        {
                            if (Unlocked_DashHop() && dashJoystickInput)
                            {
                                StartDashHop();
                            }
                        }
                    }
                    else if (switchTime <= 0 && disabledTime == 0 && InputManager.GetButtonPressShorter(InputManager.Button.B, CONTROL_HOLD_TIME))    // || bufferWeapon)
                    {
                        if (Unlocked_Slash() && currentCharacter == MainManager.PlayerCharacter.Wilex)
                        {
                            SetActionState(ActionState.Slash);
                            StartSlash();
                        }

                        if (Unlocked_Smash() && currentCharacter == MainManager.PlayerCharacter.Luna)
                        {
                            SetActionState(ActionState.Smash);
                            StartSmash();
                        }
                    } else if (switchTime <= 0 && aetherLightCooldown > Time.deltaTime + 0.1f + Time.fixedDeltaTime && InputManager.GetButtonHeldLonger(InputManager.Button.B, CONTROL_HOLD_TIME))
                    {
                        //don't question my math
                        if (Unlocked_Aetherize() && !noAetherZone && currentCharacter == MainManager.PlayerCharacter.Wilex && antiAetherCheck.CollisionCheck())
                        {
                            SetActionState(ActionState.Aetherize);
                            AetherStart();
                        }

                        if (Unlocked_Illuminate() && currentCharacter == MainManager.PlayerCharacter.Luna && antiLightCheck.CollisionCheck())
                        {
                            SetActionState(ActionState.Illuminate);
                            LightStart();
                        }
                    }
                }
            }
        } else
        {
            groundedTime = 0;
            airTime += Time.fixedDeltaTime;
            airFrames++;

            floorNormal = Vector3.zero;

            if (airFrames > 1 && timeSinceStepUp > STEP_UP_DOWN_DELAY)
            {
                attached = null;
                switch (actionState)
                {
                    case ActionState.Dig:
                        //UnDig();
                        SetActionState(ActionState.Fall);
                        enableDashLeniency = false;
                        break;
                    case ActionState.Slash:
                    case ActionState.Smash:
                        //ResetWeapon();
                        SetActionState(ActionState.Fall);
                        enableDashLeniency = false;
                        break;
                    case ActionState.Land:
                    case ActionState.Neutral:
                        SetActionState(ActionState.Fall);
                        enableDashLeniency = false;
                        break;
                    case ActionState.Aetherize:
                        SetActionState(ActionState.AetherizeFall);
                        enableDashLeniency = false;
                        break;
                    case ActionState.Illuminate:
                        SetActionState(ActionState.IlluminateFall);
                        enableDashLeniency = false;
                        break;
                    case ActionState.Fall:
                    case ActionState.DashFall:
                        break;
                    case ActionState.WallGrab:
                        if (!stuckToWall)
                        {
                            SetActionState(ActionState.Fall);
                        }
                        break;
                    default:
                        coyoteBool = false;
                        break;
                }

                if (stuckToWall)
                {
                    //don't stick if in some weird state
                    switch (actionState)
                    {
                        case ActionState.Jump:
                        case ActionState.DashFall:
                        case ActionState.Fall:
                        case ActionState.SuperKick:
                        case ActionState.Hover:
                            SetActionState(ActionState.WallGrab);
                            break;
                    }
                }

                //wall stickiness
                if (actionState == ActionState.WallGrab)
                {
                    if (KeruAsterJump())
                    {
                        if (currentCharacter == MainManager.PlayerCharacter.Wilex)
                        {
                            if (Unlocked_SuperJump() && InputManager.GetButton(InputManager.Button.A))
                            {
                                ClimbSetup();
                                SetActionState(ActionState.Climb);
                            }
                        }
                        if (currentCharacter == MainManager.PlayerCharacter.Luna)
                        {
                            bool bufferWallJump = InputManager.GetButton(InputManager.Button.A) && (!InputManager.GetButtonHeldLonger(InputManager.Button.A, timeSinceLastJump + 0.02f < BUFFER_JUMP_WINDOW ? timeSinceLastJump + 0.02f : BUFFER_JUMP_WINDOW));
                            if (timeSinceLastJump > 2 * Time.fixedDeltaTime && Unlocked_Dig() && (InputManager.GetButtonDown(InputManager.Button.A) || bufferWallJump))
                            {
                                StartWallJump();
                            }
                        }
                    }
                }

                if (KeruAsterJump() && actionState == ActionState.WallGrab)
                {
                    if (currentCharacter == MainManager.PlayerCharacter.Luna)
                    {
                        bool bufferWallJumpB = InputManager.GetButton(InputManager.Button.A) && (!InputManager.GetButtonHeldLonger(InputManager.Button.A, timeSinceLastJump + 0.02f < BUFFER_JUMP_WINDOW ? timeSinceLastJump + 0.02f : BUFFER_JUMP_WINDOW));
                        if (timeSinceLastJump > 2 * Time.fixedDeltaTime && Unlocked_Dig() && stuckToWall && (InputManager.GetButtonDown(InputManager.Button.A) || bufferWallJumpB))
                        {
                            StartWallJump();
                        }
                    }
                }
            }

            //coyote jumping
            if (airTime < COYOTE_TIME && coyoteBool || timeSinceStepUp < STEP_UP_DOWN_DELAY)
            {
                bool dashJoystickInput = (!InputManager.GetNeutralJoystick() && InputManager.GetTimeSinceNeutralJoystick() < DASH_DIRECTIONAL_WINDOW);
                dashJoystickInput |= (enableDashLeniency && (groundedTime < BUFFER_DASH_JUMP_WINDOW));

                //Note: buffering doesn't really make sense here?
                //adding it anyway in case fixed update doesn't see button down
                bool bufferJump = disabledTime == 0 && InputManager.GetButton(InputManager.Button.A) && (!InputManager.GetButtonHeldLonger(InputManager.Button.A, Time.fixedDeltaTime * 2.5f));
                if (timeSinceLastJump > 2 * Time.fixedDeltaTime && (InputManager.GetButtonDown(InputManager.Button.A) || bufferJump))
                {
                    //which jump?
                    bool didMove = false;

                    if (!KeruAsterJump() && currentCharacter == MainManager.PlayerCharacter.Wilex)
                    {
                        if (Unlocked_SuperJump() && !didMove && InputManager.SpinInput())
                        {
                            didMove = true;
                            StartSuperJump();
                        }
                    }

                    if (!KeruAsterJump() && currentCharacter == MainManager.PlayerCharacter.Luna)
                    {
                        if (Unlocked_Dig() && switchTime <= 0 && !didMove && InputManager.SpinInput())
                        {
                            didMove = true;
                            //Note: don't dig in midair
                        }

                        if (Unlocked_DashHop() && !didMove && dashJoystickInput)
                        {
                            didMove = true;
                            StartDashHop();
                        }
                    }

                    //if you didn't do any of the above special things, just normal jump
                    if (!didMove && !applyDashJumpLift)
                    {
                        didMove = true;
                        StartJump();
                    }
                }
            } else
            {
                if (stuckToWall && actionState == ActionState.WallGrab)
                {
                    if (KeruAsterJump() && currentCharacter == MainManager.PlayerCharacter.Luna)
                    {
                        if (timeSinceLastJump > 2 * Time.fixedDeltaTime && Unlocked_Dig() && switchTime <= 0 && InputManager.GetButtonDown(InputManager.Button.A))
                        {
                            StartWallJump();
                        }
                    }
                }
            }
        }

        if (!IsGrounded() || lastFloorSlippery || lastMovementDamping != 0 || groundedTime < PLATFORM_VELOCITY_RESET_TIME)
        {
            pavAirTime += Time.fixedDeltaTime;
        }

        if (applyDashJumpLift)
        {
            if (!applyDoubleJumpLift)
            {
                canDoubleJump = true;
                canHover = true;
            }
            if (rb.velocity.y <= 0)// || isGrounded)
            {
                switch (actionState)
                {
                    case ActionState.Dash:
                        SetActionState(ActionState.DashFall);
                        break;
                }
                applyDashJumpLift = false;
            }
            else
            {
                //SetActionState(ActionState.Dash);
                rb.velocity = rb.velocity + Vector3.up * Time.fixedDeltaTime * 50 * dashJumpLift;
            }

            applyJumpLift = false;
        }

        if (applySuperJumpLift)
        {
            if (rb.velocity.y <= 0)// || isGrounded)
            {
                switch (actionState)
                {
                    case ActionState.SuperJump:
                        SetActionState(ActionState.Fall);
                        break;
                }
                applySuperJumpLift = false;
            }
            else
            {
                SetActionState(ActionState.SuperJump);
                rb.velocity = rb.velocity + Vector3.up * Time.fixedDeltaTime * 50 * superJumpLift;
            }

            applyJumpLift = false;
        }

        if (applySuperKickLift)
        {
            if (rb.velocity.y <= 0)// || isGrounded)
            {
                switch (actionState)
                {
                    case ActionState.SuperKick:
                        SetActionState(ActionState.Fall);
                        break;
                }
                applySuperKickLift = false;
            }
            else
            {
                SetActionState(ActionState.SuperKick);
                rb.velocity = rb.velocity + Vector3.up * Time.fixedDeltaTime * 50 * superKickLift;
            }

            applySuperKickLift = false;
        }

        if (applyJumpLift)
        {
            if (!applyDoubleJumpLift)
            {
                canDoubleJump = true;
                canHover = true;
            }
            if (rb.velocity.y <= 0)// || isGrounded)
            {
                switch (actionState)
                {
                    case ActionState.Jump:
                        SetActionState(ActionState.Fall);
                        break;
                }
                applyJumpLift = false;
            } else
            {
                if (!applyDoubleJumpLift && actionState != ActionState.WallJump)
                {
                    SetActionState(ActionState.Jump);
                }
                rb.velocity = rb.velocity + Vector3.up * Time.fixedDeltaTime * 50 * jumpLift;
            }
        }


        if (applyDoubleJumpLift)
        {
            if (rb.velocity.y <= 0)// || isGrounded)
            {
                switch (actionState)
                {
                    case ActionState.DoubleJump:
                        SetActionState(ActionState.Fall);
                        break;
                }
                applyDoubleJumpLift = false;
            }
            else
            {
                SetActionState(ActionState.DoubleJump);
                rb.velocity = rb.velocity + Vector3.up * Time.fixedDeltaTime * 50 * doubleJumpLift;
            }
        }

        if (currentCharacter == MainManager.PlayerCharacter.Wilex)// && switchTime <= 0)
        {
            if (!KeruAsterJump() && Unlocked_DoubleJump() && rb.velocity.y < DOUBLE_JUMP_WINDOW_HIGHER && rb.velocity.y > DOUBLE_JUMP_WINDOW_LOWER)
            {
                bool bufferJump = disabledTime == 0 && InputManager.GetButton(InputManager.Button.A) && (!InputManager.GetButtonHeldLonger(InputManager.Button.A, Time.fixedDeltaTime * 2.5f));
                //don't double jump right next to the floor (note that glitchy ultra jumps may still be possible on edges?)
                if (timeSinceLastJump > 0.05f + Time.deltaTime && MainManager.Instance.GetControlsEnabled() && (InputManager.GetButtonDown(InputManager.Button.A) || bufferJump) && canDoubleJump)
                {
                    StartDoubleJump();
                }
            }

            if (KeruAsterJump() && Unlocked_DoubleJump())
            {
                float evalTime = 0.05f;
                Vector3 delta = Vector3.zero;
                Vector3 xdelta = Vector3.zero;

                //need to use integrals to calculate this
                //compute xz component
                xdelta += MainManager.XZProjectPreserve(rb.velocity) * evalTime;
                delta = xdelta;
                xdelta += xdelta.normalized * 0.125f;

                delta += Vector3.up * rb.velocity.y * evalTime;
                //apply the gravity part of the integral
                //(0.5 * g)x^2
                delta += Physics.gravity * evalTime * evalTime * 0.5f;

                RaycastHit outputA;
                Physics.Raycast(transform.position, xdelta.normalized, out outputA, xdelta.magnitude, 311);
                RaycastHit outputB;
                Physics.Raycast(transform.position + Vector3.down * 0.375f, delta.normalized, out outputB, delta.magnitude, 311);

                if (outputA.collider == null && outputB.collider != null)
                {
                    bool bufferJumpC = InputManager.GetButton(InputManager.Button.A) && (!InputManager.GetButtonHeldLonger(InputManager.Button.A, timeSinceLastJump + 0.02f < BUFFER_JUMP_WINDOW ? timeSinceLastJump + 0.02f : BUFFER_JUMP_WINDOW));
                    if (MainManager.Instance.GetControlsEnabled() && (InputManager.GetButtonDown(InputManager.Button.A) || bufferJumpC))
                    {
                        StartSuperKick();
                    }
                }
            }
        }

        if (currentCharacter == MainManager.PlayerCharacter.Luna)
        {
            if (KeruAsterJump() && Unlocked_DashHop())
            {
                if (MainManager.Instance.GetControlsEnabled())
                {
                    bool bufferJump = disabledTime == 0 && InputManager.GetButton(InputManager.Button.A) && (!InputManager.GetButtonHeldLonger(InputManager.Button.A, Time.fixedDeltaTime * 2.5f));
                    if (switchTime <= 0 && InputManager.SpinInput() && (InputManager.GetButtonDown(InputManager.Button.A) || bufferJump) && canHover)
                    {
                        HoverSetup();
                        SetActionState(ActionState.Hover);
                    }
                }
            }
        }

        if (airFrames == 1 && GroundedState() && timeSinceStepUp > STEP_UP_DOWN_DELAY)
        {
            //snap to floor?
            TrySnapToFloor();
        }

        if (semisolidFloorActive != lastSemiSolidFloorActive && airTime > 0.25f && timeSinceLastJump > 0.4f)
        {
            transform.position = Vector3.up * (height / 2) + Vector3.ProjectOnPlane(transform.position - semisolidFloorPosition, semisolidFloorNormal) + semisolidFloorPosition;
        }
        if (semisolidFloorActive)
        {
            lastGroundedHeight = transform.position.y;
            lastHighestHeight = lastGroundedHeight;
            if (!Unstable() && !HazardState())
            {
                lastFloorPos = transform.position;
            }
            /*
            if (rb.velocity.y < -Physics.gravity.y * Time.fixedDeltaTime)
            {
                rb.velocity = rb.velocity.z * Vector3.forward + rb.velocity.x * Vector3.right + (-Physics.gravity.y * Time.fixedDeltaTime * Vector3.up);
            }
            */

            if (semisolidSnapBelow)
            {
                float snapDot = Vector3.Dot(transform.position - semisolidFloorPosition - Vector3.up * (height), semisolidFloorNormal);
                if (snapDot < 0)
                {
                    transform.position = Vector3.ProjectOnPlane(transform.position - semisolidFloorPosition - Vector3.up * (height), semisolidFloorNormal) + semisolidFloorPosition + Vector3.up * (height);
                }
            }

            //unfortunately life is not that simple
            float dotProduct = Vector3.Dot((rb.velocity + Physics.gravity * Time.fixedDeltaTime), semisolidFloorNormal);

            if  (dotProduct < 0)
            {
                //replace with something that results in a dot product of 0
                Vector3 newVelocity = Vector3.ProjectOnPlane((rb.velocity), semisolidFloorNormal);
                rb.velocity = newVelocity - Physics.gravity * Time.fixedDeltaTime;
            }
        }

        if (rb != null && !IsGrounded())
        {
            if (rb.velocity.y == 0)
            {
                lastHighestHeight = transform.position.y - height / 2;
            }
            else
            {
                if (transform.position.y - height / 2 > lastHighestHeight)
                {
                    lastHighestHeight = transform.position.y - height / 2;
                }
            }
        }

        if (oneWayVector != Vector3.zero)
        {
            //gravity and the allowable motion both offset the plane to project to
            //(The gravity part makes it so that you don't slip through upward facing ones)
            float dotProduct = Vector3.Dot((rb.velocity + Physics.gravity * Time.fixedDeltaTime + oneWayVector * -oneWayMinAllowable), oneWayVector);

            if (dotProduct < 0)
            {
                //replace with something that results in a dot product of 0
                Vector3 newVelocity = Vector3.ProjectOnPlane((rb.velocity + Physics.gravity * Time.fixedDeltaTime + oneWayVector * -oneWayMinAllowable), oneWayVector);
                rb.velocity = newVelocity - Physics.gravity * Time.fixedDeltaTime + oneWayVector * oneWayMinAllowable;
            }
        }

        if (attached != null)
        {
            AttachUpdate();
        } else
        {
            if (IsGrounded() && !(lastFloorSlippery || lastMovementDamping != 0) && groundedTime > PLATFORM_VELOCITY_RESET_TIME)
            {
                momentumVel = Vector3.zero;
                pavAirTime = 0;
            }
        }

        movingPlatformFix = false;

        //Antistuck applies if you are stuck at a certain Y position but aren't grounded
        antiStuck = transform.position.y - pastHeight;
        pastHeight = transform.position.y;

        //Debug.Log(antiStuck + " " + antiStuckTime);

        //should anti stuck apply
        //no anti stuck in cutscenes?
        if (actionState != ActionState.Hover && actionState != ActionState.NoClip && airTime > 0 && Mathf.Abs(antiStuck) / Time.fixedDeltaTime < ANTI_STUCK_DISTANCE && !scriptedAnimation)
        {
            antiStuckTime += Time.fixedDeltaTime;
        } else
        {
            antiStuckTime = 0;
        }
        if (antiStuckTime > ANTI_STUCK_TIME)
        {
            rb.velocity = rb.velocity.x * Vector3.right + rb.velocity.z * Vector3.forward + ANTI_STUCK_IMPULSE * Vector3.up;
            Debug.Log("Antistuck");
        }

        if (transform.position.y < WARP_BARRIER_Y_COORD && !HazardState())
        {
            HazardZoneTouch();
        }

        //conveyers that lift you off the ground need to properly set that up to avoid having the ground sticky stuff fighting back against it
        if (AntiGravity() && IsGrounded())
        {
            isGrounded = false;
            semisolidFloorActive = false;
            semisolidSnapBelow = false;
            airFrames = 2;  //the ground snap checks for this = 1
        }

        if (conveyerVector != Vector3.zero && timeSinceLastJump > 0.1f && airTime > 0.1f)
        {
            applyDashJumpLift = false;
            //applyDefaultJumpLift = false;
            applyDoubleJumpLift = false;
            applySuperJumpLift = false;
            applyJumpLift = false;
        }

        if (movementDamping != 0 && (timeSinceLastJump > 0.1f || timeSinceLastJump == 0))
        {
            rb.velocity = MainManager.EasingExponentialTime(rb.velocity, conveyerVector - conveyerVector.y * Vector3.up, 1 / movementDamping);
        }

        //Note: slippery conveyers are a bit weird
        if (!rb.isKinematic)    //shuts up annoying warnings
        {
            rb.velocity += conveyerVector;
        }

        lastConveyerVector = conveyerVector;
        conveyerVector = Vector3.zero;

        oneWayVector = Vector3.zero;
        oneWayMinAllowable = 0;

        lastSemiSolidFloorActive = semisolidFloorActive;
        semisolidFloorActive = false;
        semisolidSnapBelow = false;
        semisolidFloorNormal = Vector3.zero;

        lastMovementDamping = movementDamping;
        movementDamping = 0;

        stuckToWall = false;
        lastFrameVel = rb.velocity;
        timeSinceStepUp += Time.fixedDeltaTime;
        isGrounded = false;
        touchingNonGround = false;
        floorNormal = Vector3.zero;
        hasTouchedObject = false;
        prevAttached = attached;
        attached = null;
        //prevAttachedVel = attachedVel;
        attachedVel = Vector3.zero;
        //attachedPos = Vector3.zero;
        //attachedLocalPos = Vector3.zero;
        unstableZone = false;
        noDigZone = false;
        concealedZone = false;
        noAetherZone = false;
        timeSinceLaunch += Time.fixedDeltaTime;
        zoneTattleTarget = null;
    }

    public void TrySnapToFloor()
    {
        RaycastHit hit = DownRaycast(STEP_DOWN_MAX_HEIGHT);

        if (!AntiGravity() && !HazardState() && hit.collider != null && Vector3.Dot(rb.velocity, hit.normal) < 0.2f)
        {
            if (hit.normal.y > MinGroundNormal() && LegalGround(hit.collider))
            {
                floorNormal = hit.normal;
                lastGroundedHeight = hit.point.y;
                lastHighestHeight = lastGroundedHeight;
                attached = hit.rigidbody;
                isGrounded = true;

                transform.position = hit.point + Vector3.up * HEIGHT / 2;

                float speed = rb.velocity.magnitude;
                float dot = Vector3.Dot(rb.velocity, hit.normal);
                if (dot > 0)
                {
                    rb.velocity = (rb.velocity - hit.normal * dot).normalized * speed;
                }
                Debug.Log("snap " + hit.point + rb.velocity);
            }
        }
    }

    public override void AttachUpdate()
    {
        if (attached.isKinematic || attached.mass > rb.mass)
        {
            if (attached == prevAttached)
            {
                Vector3 connectionMovement = attached.transform.TransformPoint(attachedLocalPos) - attachedPos;

                if (attachedPos == Vector3.zero || attachedLocalPos == Vector3.zero)
                {
                    connectionMovement = Vector3.zero;
                }
                //low frame rate is causing problems somehow
                attachedVel = connectionMovement / Time.fixedDeltaTime;
            }
            else
            {
                attachedVel = Vector3.zero;
            }
            attachedPos = transform.position;
            attachedLocalPos = attached.transform.InverseTransformPoint(
                attachedPos
            );

            if (attachedVel.y > 0)
            {
                attachedVel.y *= 0.2f;
            }
            else
            {
                attachedVel.y *= 2f;
            }

            if (IsGrounded() && timeSinceLastJump > 0.1f && !(lastFloorSlippery || lastMovementDamping != 0))
            {
                rb.velocity += attachedVel;
                if (attachedVel.y > 0)
                {
                    rb.velocity += Vector3.down * attachedVel.y;
                }
            }

            momentumVel = attachedVel;
            pavAirTime = 0;

            //Debug.Log(attachedVel + " " + (attached.transform.TransformPoint(attachedLocalPos) - attachedPos));
        }
    }

    public override void SpriteRotationUpdate()
    {
        bool pastShowBack = showBack;

        //Debug.Log("monitor P " + trueFacingRotation);

        Vector3 usedMovement = Vector3.zero;
        if (MainManager.Instance.GetControlsEnabled())
        {
            usedMovement = useIntended ? intendedMovement : rb.velocity;
        } else
        {
            usedMovement = scriptedInput.x * Vector3.right + scriptedInput.y * Vector3.forward;
        }

        //Locks the true facing rotation
        bool lockRotation = false;

        switch (actionState)
        {
            case ActionState.Smash:
            case ActionState.Slash:
                lockRotation = true;
                break;
        }

        midFacingRotation = trueFacingRotation - MainManager.Instance.GetWorldspaceYaw();
        while (midFacingRotation < 0)
        {
            midFacingRotation += 360;
        }
        while (midFacingRotation > 360)
        {
            midFacingRotation -= 360;
        }

        if (!lockRotation && (MainManager.XZProject(usedMovement).magnitude > 0.01f) || pastMidFacingRotation != midFacingRotation)
        {
            //don't rotate in hazard state, if rotation is disabled, or if you are still
            if (!movementRotationDisabled && !HazardState() && (MainManager.XZProject(usedMovement).magnitude > 0.01f))
            {
                trueFacingRotation = -Vector2.SignedAngle(Vector2.right, usedMovement.x * Vector2.right + usedMovement.z * Vector2.up);
                while (trueFacingRotation < 0)
                {
                    trueFacingRotation += 360;
                }
                while (trueFacingRotation >= 360)
                {
                    trueFacingRotation -= 360;
                }
            }
            //Debug.Log(movementRotationDisabled + " " + trueFacingRotation);

            //Worldspace yaw needs to be added back
            realOrientationObject.transform.eulerAngles = (trueFacingRotation) * Vector3.up;

            //Debug.Log(trueFacingRotation);

            while (midFacingRotation < 0)
            {
                midFacingRotation += 360;
            }
            while (midFacingRotation >= 360)
            {
                midFacingRotation -= 360;
            }

            //Debug.Log(hiddenFacingRotation);
        }

        //going straight back or forward is a little weird, so don't rotate in a 10 degree range
        bool norotate = false;
        if (midFacingRotation > 85f && midFacingRotation < 95f)
        {
            norotate = true;
        }
        if (midFacingRotation > 265f && midFacingRotation < 275f)
        {
            norotate = true;
        }

        if (!norotate)
        {
            targetFacingRotation = 0;
            if (midFacingRotation > 90 && midFacingRotation < 270)
            {
                targetFacingRotation = 180;
            }
        }



        bool lrdir = targetFacingRotation == 0; //true = left to right
        bool ccRotation = showBack ^ lrdir;

        float oldFacing = facingRotation;

        if (facingRotation != targetFacingRotation)
        {
            if (ccRotation)
            {
                //minus
                facingRotation -= SPIN_RATE * 360 * Time.deltaTime;

                if (oldFacing > targetFacingRotation && facingRotation < targetFacingRotation)
                {
                    facingRotation = targetFacingRotation;
                }

                if (oldFacing > targetFacingRotation - 360 && facingRotation < targetFacingRotation - 360)
                {
                    facingRotation = targetFacingRotation;
                }

                //apply wraparound after to make sure the above conditions make sense
                if (facingRotation < 0)
                {
                    facingRotation += 360;
                }
            }
            else
            {
                //plus
                facingRotation += SPIN_RATE * 360 * Time.deltaTime;

                if (oldFacing < targetFacingRotation && facingRotation > targetFacingRotation)
                {
                    facingRotation = targetFacingRotation;
                }

                if (oldFacing < targetFacingRotation + 360 && facingRotation > targetFacingRotation + 360)
                {
                    facingRotation = targetFacingRotation;
                }

                //apply wraparound after to make sure the above conditions make sense
                if (facingRotation >= 360)
                {
                    facingRotation -= 360;
                }
            }
        }

        //Debug.Log(hiddenFacingRotation + " " + targetFacingRotation + " " + facingRotation);

        //need to add a correction for some reason (Lighting fix)
        float correctedRotation = facingRotation + switchRotation;
        /*
        bool specialShowBack = false;
        if (switchRotation != 0)
        {
            //hacky fix 

            if (switchRotation > 180)
            {
                correctedRotation = facingRotation + switchRotation - 180;
                specialShowBack = true;
            }
            else
            {
                correctedRotation = facingRotation + switchRotation;
            }
        }
        else
        {
            correctedRotation = facingRotation;
        }
        */


        //by convention, 0 and 180 (straight right and left) are front facing
        //may want to add some leeway?            

        if (correctedRotation != 180 && correctedRotation != 360 && correctedRotation != 0)
        {
            showBack = correctedRotation < 360 && correctedRotation > 180;
        } else
        {
            showBack = midFacingRotation < 360 && midFacingRotation > 180;
        }

        while (correctedRotation > 360)
        {
            correctedRotation -= 360;
        }
        float rotationB = correctedRotation;

        while (correctedRotation > 90)
        {
            correctedRotation -= 180;
        }
        while (correctedRotation < -90)
        {
            correctedRotation += 180;
        }

        //transform this with respect to worldspace yaw
        correctedRotation += MainManager.Instance.GetWorldspaceYaw();

        subObject.transform.eulerAngles = Vector3.up * (correctedRotation);


        //TODO: make this use real animation stuff
        /*
        if (showBack)
        {
            spriteRenderer.sprite = curbackSprite;
        }
        else
        {
            spriteRenderer.sprite = curfrontSprite;
        }
        if (facingRotation > 90 || facingRotation < -90)
        {
            spriteRenderer.flipX = true;
        }
        else
        {
            spriteRenderer.flipX = false;
        }
        */

        if (ac != null)
        {
            if (showBack)
            {
                ac.SendAnimationData("showback");
            } else
            {
                ac.SendAnimationData("unshowback");
            }

            if (actionState == ActionState.Slash)
            {
                //important: the slash animations shouldn't be flipped
                ac.SendAnimationData("xunflip");
            } else
            {
                if ((rotationB > 90 || rotationB < -90) && (rotationB < 270 && rotationB > -270))
                {
                    ac.SendAnimationData("xflip");
                }
                else
                {
                    ac.SendAnimationData("xunflip");
                }
            }
        }
    }

    public override void SpriteAnimationUpdate()
    {
        if (ac == null)
        {
            return;
        }

        if (SpeakingAnimActive() || scriptedAnimation)
        {
            return;
        }

        if (mapScript.halted || mapScript.battleHalt)
        {
            return;
        }

        string animName = "";

        if (animationStopDelay > 0)
        {
            animationStopDelay -= Time.deltaTime;
            if (animationStopDelay < 0)
            {
                animationStopDelay = 0;
                ac.StopAnimation();
            }
        } else
        {
            animationStopDelay = 0;
        }

        if (actionState != ActionState.Smash && actionState != ActionState.Slash)
        {
            ac.ResetAnimationSpeed();
            animationStopDelay = 0;
        }

        switch (actionState)
        {
            case ActionState.Neutral:
                if (undigTimer > 0)
                {
                    animName = "undig";
                } else
                {
                    if (rb.velocity.magnitude > 0.25f)
                    {
                        animName = "walk";
                    }
                    else
                    {
                        animName = "idle";
                    }
                }
                break;
            case ActionState.HazardTouch:
            case ActionState.HazardFall:
                animName = "hurt";
                break;
            case ActionState.Jump:
            case ActionState.WallJump:
                animName = "jump";
                break;
            case ActionState.SuperKick:
            case ActionState.SuperJump:
            case ActionState.DoubleJump:
            case ActionState.Dash:
                animName = "dashjump";
                break;
            case ActionState.DashFall:
                animName = "dashfall";
                break;
            case ActionState.Dig:
                if (digTimer > 0)
                {
                    animName = "dig";
                }
                else
                {
                    //??? sprite size is shrank to basically nothing so it doesn't really matter
                }
                break;
            case ActionState.Slash:
                //calculate stuff
                //note that this has the same angles as smash but 8 possible animations instead of 5
                animName = GetSlashAnim();
                break;
            case ActionState.Smash:
                //calculate stuff
                animName = GetSmashAnim();
                break;
            case ActionState.Aetherize:
            case ActionState.Illuminate:
                if (rb.velocity.magnitude > 0.25f)
                {
                    animName = "weaponholdwalk";
                }
                else
                {
                    animName = "weaponholdidle";
                }
                break;
            case ActionState.IlluminateFall:
            case ActionState.AetherizeFall:
                if (rb != null && rb.velocity.y > 0)
                {
                    animName = "weaponholdjump";
                }
                else
                {
                    animName = "weaponholdfall";
                }
                break;
            case ActionState.Fall:
            case ActionState.LaunchFall:
                if (rb != null && rb.velocity.y > 0)
                {
                    animName = "jump";
                }
                else
                {
                    animName = "fall";
                }
                break;
            default:
                animName = "idle";
                break;
        }

        if (ac != null)
        {
            if (animName.Equals("idle"))
            {
                ShowIdleAnimation();
            }
            else
            {
                ac.SetAnimation(animName);
            }
        }
    }


    public void DestroyPerpetualParticleObject()
    {
        for (int i = 0; i < stalePerpetualParticles.Count; i++)
        {
            if (stalePerpetualParticles[i] != null)
            {
                Destroy(stalePerpetualParticles[i]);
            }
        }
    }
    public void PerpetualParticleUpdate()
    {
        //wrong state = delete
        if (perpetualParticleObject != null)
        {
            bool keep = false;
            switch (actionState)
            {
                case ActionState.Aetherize:
                case ActionState.AetherizeFall:
                case ActionState.Illuminate:
                case ActionState.IlluminateFall:
                case ActionState.Dig:
                case ActionState.SuperJump:
                    keep = true;
                    break;
            }

            if (!keep)
            {
                perpetualParticleObject.GetComponent<ParticleSystem>().Stop();
                //Destroy(perpetualParticleObject);
                perpetualParticleObject = null;
            }
        }

        //super jump trail
        if (perpetualParticleObject != null && actionState == ActionState.SuperJump)
        {
            if (rb.velocity.y < 1)
            {
                perpetualParticleObject.GetComponent<ParticleSystem>().Stop();
                //Destroy(perpetualParticleObject);
                perpetualParticleObject = null;
            }
        }

        if (IsGrounded())
        {
            Vector3 ortho = Vector3.Cross(floorNormal, Vector3.right);
            if (ortho.magnitude == 0)   //hairy ball theorem fix
            {
                ortho = Vector3.Cross(floorNormal, Vector3.right + Vector3.forward);
            }
            floorNormalObject.transform.rotation = Quaternion.LookRotation(ortho, floorNormal);
            floorNormalObject.transform.position = lastFloorPos + Vector3.down * 0.375f;
        }
        else
        {
            floorNormalObject.transform.rotation = Quaternion.identity;
            floorNormalObject.transform.position = lastFloorPos + Vector3.down * 0.375f;            //so that you can't jump up and make the particles become weird
        }
    }

    //if you don't hold A, try to get out of the ground
    //Note that this is not always allowed (because the normal and dig hitboxes don't fit in the same places)
    //Note that currently illegal floors will override the space check (may lead to wall clips? but not really if illegal ground and legal ground are spaced right)
    public void TryUndig()
    {
        if (digCheck.CollisionCheck() || lastFloorNoDig || noDigZone)
        {
            //UnDig();
            SetActionState(ActionState.Neutral);
        }
    }

    //Go into dig state
    //Later it will have particle effects and won't be instantaneous (but the hitbox stuff will be)
    public void Dig()
    {
        GameObject eoI = null;
        eoI = Instantiate(Resources.Load<GameObject>("VFX/Overworld/Player/Effect_Dig"), realOrientationObject.transform);
        eoI.transform.localPosition = Vector3.down * 0.375f;
        eoI.transform.localRotation = Quaternion.identity;

        GameObject eo = null;
        eo = Instantiate(Resources.Load<GameObject>("VFX/Overworld/Player/Effect_Perpetual_Dig"), realOrientationObject.transform);
        eo.transform.localPosition = Vector3.down * 0.375f;
        eo.transform.localRotation = Quaternion.identity;

        //decided to instead just mess with settings to mimic what it should look like instead of using this
        //since the collision plane thing would mess up if you moved upwards after digging
        //ParticleSystem ps = eo.GetComponent<ParticleSystem>();
        //ps.collision.SetPlane(0, floorNormalObject.transform);

        MainManager.Instance.PlaySound(gameObject, MainManager.Sound.SFX_Overworld_Dig);
        MainManager.Instance.PlaySound(gameObject, MainManager.Sound.SFX_Overworld_DigPerpetual);

        perpetualParticleObject = eo;
        stalePerpetualParticles.Add(eo);

        //collision stuff
        if (characterCollider is CapsuleCollider cc)
        {
            cc.center = Vector3.down * 0.3125f;
            cc.radius = 0.0625f;
            cc.height = 0.125f;
        }

        if (!MainManager.Instance.Cheat_SplitParty)
        {
            for (int i = 0; i < followers.Count; i++)
            {
                followers[i].gameObject.SetActive(false);
            }
        }

        digTimer = DIG_TIME;

        TrySnapToFloor();
    }

    //will make some particle effects later to indicate where you are
    public void DigUpdate()
    {
        if (digTimer != 0)
        {
            subObject.transform.localPosition = Vector3.down * 0.325f * (1 - (digTimer / DIG_TIME)) + Vector3.down * height / 2;
        }
        else
        {
            subObject.transform.localScale = Vector3.one * 0.001f;
            subObject.transform.localPosition = Vector3.down * 0.325f + Vector3.down * height / 2;
        }
        if (lastFloorDigThrough)
        {
            transform.position += Vector3.down * 0.75f;
            //UnDig();
            SetActionState(ActionState.Fall);
        }
    }

    //Go from dig state to neutral
    //Later it will have particle effects and won't be instantaneous (but the hitbox stuff will be)
    public void UnDig()
    {
        undigTimer = UNDIG_TIME;
        if (IsGrounded())
        {
            GameObject eoI = null;
            eoI = Instantiate(Resources.Load<GameObject>("VFX/Overworld/Player/Effect_Dig"), realOrientationObject.transform);
            eoI.transform.position = lastFloorPos + Vector3.down * 0.375f;
            eoI.transform.localRotation = Quaternion.identity;
        }

        if (perpetualParticleObject != null)
        {
            perpetualParticleObject.GetComponent<ParticleSystem>().Stop();
            //Destroy(perpetualParticleObject);
            perpetualParticleObject = null;
        }

        subObject.transform.localScale = Vector3.one;
        subObject.transform.localPosition = Vector3.zero + Vector3.down * height / 2;

        if (characterCollider is CapsuleCollider cc) {
            cc.center = Vector3.zero;
            cc.radius = 0.25f;
            cc.height = 0.75f;
        }

        MainManager.Instance.StopSound(gameObject, MainManager.Sound.SFX_Overworld_DigPerpetual);
        MainManager.Instance.PlaySound(gameObject, MainManager.Sound.SFX_Overworld_Undig);

        if (!MainManager.Instance.Cheat_SplitParty)
        {
            for (int i = 0; i < followers.Count; i++)
            {
                followers[i].gameObject.SetActive(true);
            }
            FollowerWarpSetState();
        }
    }

    public void TryDashHopKick(Collision collision, Vector3 kickpos, Vector3 kicknormal)
    {
        //Debug.Log(actionState);
        if (collision.transform.GetComponent<IDashHopTrigger>() != null)
        {
            collision.transform.GetComponent<IDashHopTrigger>().Bonk(kickpos, kicknormal);
        }
        if (collision.rigidbody != null)
        {
            collision.rigidbody.AddForce(-8 * (kicknormal + Vector3.up * 0.2f), ForceMode.Impulse);
        }

        GameObject effect = Instantiate(Resources.Load<GameObject>("VFX/Overworld/Player/Effect_SmallShockwave"), MainManager.Instance.mapScript.transform);
        effect.transform.position = kickpos;
        effect.transform.rotation = Quaternion.LookRotation(kicknormal) * Quaternion.FromToRotation(Vector3.up, Vector3.forward);
        //Debug.Log("Bonk: " + kickpos + " with normal " + kicknormal);
    }

    public string GetSlashAnim()
    {
        string animName = "";
        //calculate stuff
        //note that this has the same angles as smash but 8 possible animations instead of 5
        if ((midFacingRotation < 22.5f || midFacingRotation > 337.5f))
        {
            animName = "slash_e";
        }
        if ((midFacingRotation > 157.5f && midFacingRotation < 202.5f))
        {
            animName = "slash_w";
        }
        if ((midFacingRotation < 337.5f && midFacingRotation > 292.5f))
        {
            animName = "slash_ne";
        }
        if ((midFacingRotation > 202.5f && midFacingRotation < 247.5f))
        {
            animName = "slash_nw";
        }
        if ((midFacingRotation < 67.5f && midFacingRotation > 22.5f))
        {
            animName = "slash_se";
        }
        if ((midFacingRotation > 112.5f && midFacingRotation < 157.5f))
        {
            animName = "slash_sw";
        }
        if ((midFacingRotation > 247.5f && midFacingRotation < 292.5f))
        {
            animName = "slash_n";
        }
        if ((midFacingRotation > 67.5f && midFacingRotation < 112.5f))
        {
            animName = "slash_s";
        }
        return animName;
    }
    public void StartSlash()
    {
        ac.ResetAnimationSpeed();
        animationStopDelay = 0;
        ac.SetAnimation(GetSlashAnim(), true);

        //Debug.Log("slash");
        attackTime = 0;
        visualAttackTime = 0;
        attackInterrupted = false;
        reswingBuffer = 0;

        GameObject eoI = null;
        switch (SwordLevel())
        {
            default:
            case 0:
                eoI = Instantiate(Resources.Load<GameObject>("VFX/Overworld/Player/Effect_SwordSlash0"), realOrientationObject.transform);
                break;
            case 1:
                eoI = Instantiate(Resources.Load<GameObject>("VFX/Overworld/Player/Effect_SwordSlash1"), realOrientationObject.transform);
                break;
            case 2:
                eoI = Instantiate(Resources.Load<GameObject>("VFX/Overworld/Player/Effect_SwordSlash2"), realOrientationObject.transform);
                break;
        }
        eoI.transform.localPosition = Vector3.up * 0.1f;
        eoI.transform.localRotation = Quaternion.identity;
        swooshParticle = eoI;
        MainManager.Instance.PlaySound(gameObject, MainManager.Sound.SFX_Overworld_Slash);
    }

    public string GetSmashAnim()
    {
        string animName = "";
        if ((midFacingRotation < 22.5f || midFacingRotation > 337.5f) || (midFacingRotation > 157.5f && midFacingRotation < 202.5f))
        {
            animName = "smash_e";
        }
        if ((midFacingRotation < 337.5f && trueFacingRotation > 292.5f) || (midFacingRotation > 202.5f && midFacingRotation < 247.5f))
        {
            animName = "smash_ne";
        }
        if ((midFacingRotation < 67.5f && trueFacingRotation > 22.5f) || (midFacingRotation > 112.5f && midFacingRotation < 157.5f))
        {
            animName = "smash_se";
        }
        if ((midFacingRotation > 247.5f && midFacingRotation < 292.5f))
        {
            animName = "smash_n";
        }
        if ((midFacingRotation > 67.5f && midFacingRotation < 112.5f))
        {
            animName = "smash_s";
        }
        return animName;
    }
    public void StartSmash()
    {
        ac.ResetAnimationSpeed();
        animationStopDelay = 0;
        ac.SetAnimation(GetSmashAnim(), true);

        //Debug.Log("smash");
        attackTime = 0;
        visualAttackTime = 0;
        attackInterrupted = false;
        reswingBuffer = 0;

        GameObject eoI = null;
        switch (HammerLevel())
        {
            default:
            case 0:
                eoI = Instantiate(Resources.Load<GameObject>("VFX/Overworld/Player/Effect_HammerSwoosh0"), realOrientationObject.transform);
                break;
            case 1:
                eoI = Instantiate(Resources.Load<GameObject>("VFX/Overworld/Player/Effect_HammerSwoosh1"), realOrientationObject.transform);
                break;
            case 2:
                eoI = Instantiate(Resources.Load<GameObject>("VFX/Overworld/Player/Effect_HammerSwoosh2"), realOrientationObject.transform);
                break;
        }
        eoI.transform.localPosition = Vector3.zero;
        eoI.transform.localRotation = Quaternion.identity;
        swooshParticle = eoI;
        MainManager.Instance.PlaySound(gameObject, MainManager.Sound.SFX_Overworld_Smash);
    }

    public void ResetWeapon()
    {
        attackTime = 0;
        visualAttackTime = 0;
        attackInterrupted = false;
        reswingBuffer = 0;

        if (swooshParticle != null)
        {
            Destroy(swooshParticle);
        }
    }

    public float GetAttackTime()
    {
        return attackTime;
    }

    public float GetVisualAttackTime()
    {
        return visualAttackTime;
    }

    public void InterruptWeapon(Vector3 weaponLastPos, Vector3 weaponGlobalPos)
    {
        if (swooshParticle != null && !attackInterrupted)
        {
            animationStopDelay = 0.015f;
            if (actionState == ActionState.Smash)
            {
                swooshParticle.GetComponent<Effect_Swoosh>().StopRotation();

                //Also spawn impact
                GameObject effect;
                switch (HammerLevel())
                {
                    default:
                    case 0:
                        effect = Instantiate(Resources.Load<GameObject>("VFX/Overworld/Player/Effect_HammerShockwave0"), MainManager.Instance.mapScript.transform);
                        break;
                    case 1:
                        effect = Instantiate(Resources.Load<GameObject>("VFX/Overworld/Player/Effect_HammerShockwave1"), MainManager.Instance.mapScript.transform);
                        break;
                    case 2:
                        effect = Instantiate(Resources.Load<GameObject>("VFX/Overworld/Player/Effect_HammerShockwave2"), MainManager.Instance.mapScript.transform);
                        break;
                }

                MainManager.Instance.StandardCameraShake(0.1f, 0.1f);

                //"slightly wrong" interpolation
                //"correct" interpolation is to use the arc the hammer travels through but that is annoying
                Vector3 midpoint = (weaponLastPos + weaponGlobalPos) / 2;

                //fix
                Vector3 delta = Vector3.Cross(Vector3.up, FacingVector());   //points perpendicular to weapon and up plane
                //Debug.Log((midpoint - transform.position) + " -> " + delta);
                delta = Vector3.Cross((midpoint - transform.position), delta);       //points down relative to weapon

                delta = -delta.normalized * HAMMER_HITBOX_RADIUS;
                //Debug.Log(delta);

                effect.transform.position = midpoint + delta;

                effect.transform.rotation = Quaternion.LookRotation(delta) * Quaternion.FromToRotation(Vector3.up, Vector3.back);
                MainManager.Instance.StopSound(gameObject, MainManager.Sound.SFX_Overworld_Smash);
                MainManager.Instance.PlaySound(gameObject, MainManager.Sound.SFX_Overworld_SmashHit);
            }
            else
            {
                swooshParticle.GetComponent<Effect_Swoosh>().StopRotationSword();
                MainManager.Instance.StopSound(gameObject, MainManager.Sound.SFX_Overworld_Slash);
                MainManager.Instance.PlaySound(gameObject, MainManager.Sound.SFX_Overworld_SlashHit);
            }
        }
        //Interrupt slash or smash in the middle of its animation
        attackInterrupted = true;
    }

    public void TryLandShockwave(Collision collision)
    {

    }

    public void TryHeadHit(Collision collision)
    {
        if (collision.transform.GetComponent<IHeadHitTrigger>() != null)
        {
            IHeadHitTrigger hh = collision.transform.GetComponent<IHeadHitTrigger>();

            if (superJumpStomp)
            {
                hh.HeadHit(StompType.SuperJump);
            }
            else if (doubleJumpStomp)
            {
                hh.HeadHit(StompType.DoubleJump);
            }
            else if (dashHopStomp)
            {
                hh.HeadHit(StompType.DashHop);
            }
            else if (jumpStomp && currentCharacter == MainManager.PlayerCharacter.Luna)
            {
                hh.HeadHit(StompType.HeavyJump);
            }
            else if (jumpStomp)
            {
                hh.HeadHit(StompType.Jump);
            }
            else
            {
                hh.HeadHit(StompType.Fall);
            }
        }
    }

    public void TryStomp(Collision collision)
    {
        //Debug.Log("Stomp try");
        if (collision.transform.GetComponent<IStompTrigger>() != null)
        {
            //Debug.Log("Stomp success");
            IStompTrigger st = collision.transform.GetComponent<IStompTrigger>();

            if (superJumpStomp)
            {
                st.Stomp(StompType.SuperJump);
            }
            else if (doubleJumpStomp)
            {
                st.Stomp(StompType.DoubleJump);
            }
            else if (dashHopStomp)
            {
                st.Stomp(StompType.DashHop);
            }
            else if (jumpStomp && currentCharacter == MainManager.PlayerCharacter.Luna)
            {
                st.Stomp(StompType.HeavyJump);
            }
            else if (jumpStomp)
            {
                st.Stomp(StompType.Jump);
            }
            else
            {
                st.Stomp(StompType.Fall);
            }
        }

        jumpStomp = false;
        doubleJumpStomp = false;
        superJumpStomp = false;
        dashHopStomp = false;
    }


    public void AetherStart()
    {
        //Debug.Log("aether start");
        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Aether"), true);
        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("IgnorePlayer"), LayerMask.NameToLayer("Aether"), true);
        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("AntiAether"), false);
        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("IgnorePlayer"), LayerMask.NameToLayer("AntiAether"), false);
        if (!MainManager.Instance.Cheat_SplitParty)
        {
            FollowerWarpSetState();
        }
        foreach (Light l in lights)
        {
            l.enabled = true;
            l.intensity = 0;
            l.color = AETHER_ANTI_COLOR;
        }

        GameObject eoI = null;
        eoI = Instantiate(Resources.Load<GameObject>("VFX/Overworld/Player/Effect_Aetherize"), realOrientationObject.transform);
        eoI.transform.localPosition = Vector3.zero;
        eoI.transform.localRotation = Quaternion.identity;

        GameObject eo = null;
        eo = Instantiate(Resources.Load<GameObject>("VFX/Overworld/Player/Effect_Perpetual_Aetherize"), realOrientationObject.transform);
        eo.transform.localPosition = Vector3.zero;
        eo.transform.localRotation = Quaternion.identity;
        perpetualParticleObject = eo;
        stalePerpetualParticles.Add(eo);

        if (ac != null)
        {
            ac.SendAnimationData("aetherize");
        }
        for (int i = 0; i < followers.Count; i++)
        {
            followers[i].SendAnimationData("aetherize");
        }

        MainManager.Instance.PlaySound(gameObject, MainManager.Sound.SFX_Effect_Ethereal);
    }

    public void LightStart()
    {
        //Debug.Log("light start");
        if (!MainManager.Instance.Cheat_SplitParty)
        {
            FollowerWarpSetState();
        }
        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Light"), true);
        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("IgnorePlayer"), LayerMask.NameToLayer("Light"), true);
        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("AntiLight"), false);
        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("IgnorePlayer"), LayerMask.NameToLayer("AntiLight"), false);
        foreach (Light l in lights)
        {
            l.enabled = true;
            l.intensity = 0;
            l.color = LIGHT_COLOR;
        }

        GameObject eoI = null;
        eoI = Instantiate(Resources.Load<GameObject>("VFX/Overworld/Player/Effect_Illuminate"), realOrientationObject.transform);
        eoI.transform.localPosition = Vector3.zero;
        eoI.transform.localRotation = Quaternion.identity;

        GameObject eo = null;
        eo = Instantiate(Resources.Load<GameObject>("VFX/Overworld/Player/Effect_Perpetual_Illuminate"), realOrientationObject.transform);
        eo.transform.localPosition = Vector3.zero;
        eo.transform.localRotation = Quaternion.identity;
        perpetualParticleObject = eo;
        stalePerpetualParticles.Add(eo);

        if (ac != null)
        {
            ac.SendAnimationData("illuminate");
        }
        for (int i = 0; i < followers.Count; i++)
        {
            followers[i].SendAnimationData("illuminate");
        }

        MainManager.Instance.PlaySound(gameObject, MainManager.Sound.SFX_Effect_Illuminate);
    }

    public void AetherUpdate()
    {
        followerDistance = REDUCED_FOLLOWER_DISTANCE;
        foreach (Light l in lights)
        {
            l.intensity = aetherShaderTime * 4 * AETHER_LIGHT_INTENSITY;
        }
    }

    public void LightUpdate()
    {
        followerDistance = REDUCED_FOLLOWER_DISTANCE;
        foreach (Light l in lights)
        {
            l.intensity = lightShaderTime * 4 * LIGHT_INTENSITY;
        }
    }

    public void DeAether()
    {
        if (perpetualParticleObject != null)
        {
            ParticleSystem ps = perpetualParticleObject.GetComponent<ParticleSystem>();
            ps.Stop();
            //Destroy(perpetualParticleObject);
            perpetualParticleObject = null;
        }

        aetherLightCooldown = 0;
        followerDistance = FOLLOWER_DISTANCE;
        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Aether"), false);
        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("IgnorePlayer"), LayerMask.NameToLayer("Aether"), false);
        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("AntiAether"), true);
        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("IgnorePlayer"), LayerMask.NameToLayer("AntiAether"), true);

        foreach (Light l in lights)
        {
            l.enabled = false;
        }

        if (ac != null)
        {
            ac.SendAnimationData("matreset");
        }
        for (int i = 0; i < followers.Count; i++)
        {
            followers[i].SendAnimationData("matreset");
        }
    }

    public void DeLight()
    {
        if (perpetualParticleObject != null)
        {
            perpetualParticleObject.GetComponent<ParticleSystem>().Stop();
            //Destroy(perpetualParticleObject);
            perpetualParticleObject = null;
        }

        aetherLightCooldown = 0;
        followerDistance = FOLLOWER_DISTANCE;
        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Light"), false);
        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("IgnorePlayer"), LayerMask.NameToLayer("Light"), false);
        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("AntiLight"), true);
        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("IgnorePlayer"), LayerMask.NameToLayer("AntiLight"), true);
        foreach (Light l in lights)
        {
            l.enabled = false;
        }

        if (ac != null)
        {
            ac.SendAnimationData("matreset");
        }
        for (int i = 0; i < followers.Count; i++)
        {
            followers[i].SendAnimationData("matreset");
        }
    }

    public void BubbleSetup()
    {
        //Debug.Log("ba");
        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Bubble"), false);
        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("IgnorePlayer"), LayerMask.NameToLayer("Bubble"), false);
        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Cloud"), true);
        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("IgnorePlayer"), LayerMask.NameToLayer("Cloud"), true);
    }

    public void CloudSetup()
    {
        //Debug.Log("ca");
        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Bubble"), true);
        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("IgnorePlayer"), LayerMask.NameToLayer("Bubble"), true);
        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Cloud"), false);
        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("IgnorePlayer"), LayerMask.NameToLayer("Cloud"), false);
    }

    public void LeafSetup()
    {
        //Debug.Log("la");
        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Leaf"), false);
        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("IgnorePlayer"), LayerMask.NameToLayer("Leaf"), false);
    }

    public void LeafUnsetup()
    {
        //Debug.Log("lb");
        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Leaf"), true);
        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("IgnorePlayer"), LayerMask.NameToLayer("Leaf"), true);
    }

    public void NoClipSetup()
    {
        characterCollider.isTrigger = true;
        rb.useGravity = false;
    }

    public void NoClipUnsetup()
    {
        characterCollider.isTrigger = false;
        rb.useGravity = true;
    }

    public void HoverJump()
    {
        //Debug.Log("Hover jump");
        //Debug.Log("Jump " + timeSinceLastJump + " " + Time.deltaTime + " " + Time.time);
        rb.velocity = intendedMovement - rb.velocity.y * Vector3.up + Vector3.up * 1.5f;

        PlatformJumpMomentum();

        applyJumpLift = false;
        //SetActionState(ActionState.Hover);
        isGrounded = false; //hacky fix
        semisolidFloorActive = false;
        semisolidSnapBelow = false;
        timeSinceLastJump = 0;
        canDoubleJump = false;
        canHover = false;
        jumpStomp = false;
        enableDashLeniency = false;
    }

    public void HoverSetup()
    {
        //Debug.Log("setup");
        rb.useGravity = false;
        hoverTime = 0;
    }

    public void HoverUnsetup()
    {
        //Debug.Log("unsetup");
        rb.useGravity = true;
        hoverTime = 0;
    }

    public void ClimbSetup()
    {
        rb.useGravity = false;
    }

    public void ClimbUnsetup()
    {
        rb.useGravity = true;
    }

    public bool KeruAsterJump()
    {
        return MainManager.Instance.Cheat_KeruAsterJump;
    }

    public bool Unlocked_DashHop()
    {
        PlayerData.PlayerDataEntry pde = MainManager.Instance.playerData.GetPlayerDataEntry(EntityID.Luna);
        return pde == null ? false : pde.jumpLevel >= 1;
    }

    public bool Unlocked_Dig()
    {
        PlayerData.PlayerDataEntry pde = MainManager.Instance.playerData.GetPlayerDataEntry(EntityID.Luna);
        return pde == null ? false : pde.jumpLevel >= 2;
    }

    public bool Unlocked_DoubleJump()
    {
        PlayerData.PlayerDataEntry pde = MainManager.Instance.playerData.GetPlayerDataEntry(EntityID.Wilex);
        return pde == null ? false : pde.jumpLevel >= 1;
    }

    public bool Unlocked_SuperJump()
    {
        PlayerData.PlayerDataEntry pde = MainManager.Instance.playerData.GetPlayerDataEntry(EntityID.Wilex);
        return pde == null ? false : pde.jumpLevel >= 2;
    }

    public bool Unlocked_Slash()
    {
        PlayerData.PlayerDataEntry pde = MainManager.Instance.playerData.GetPlayerDataEntry(EntityID.Wilex);
        return pde == null ? false : pde.weaponLevel >= 0;
    }

    public bool Unlocked_Smash()
    {
        PlayerData.PlayerDataEntry pde = MainManager.Instance.playerData.GetPlayerDataEntry(EntityID.Luna);
        return pde == null ? false : pde.weaponLevel >= 0;
    }

    public bool Unlocked_Aetherize()
    {
        PlayerData.PlayerDataEntry pde = MainManager.Instance.playerData.GetPlayerDataEntry(EntityID.Wilex);
        return pde == null ? false : pde.weaponLevel >= 2;
    }
    public bool Unlocked_Illuminate()
    {
        PlayerData.PlayerDataEntry pde = MainManager.Instance.playerData.GetPlayerDataEntry(EntityID.Luna);
        return pde == null ? false : pde.weaponLevel >= 2;
    }

    public int SwordLevel()
    {
        PlayerData.PlayerDataEntry pde = MainManager.Instance.playerData.GetPlayerDataEntry(EntityID.Wilex);
        return pde == null ? 0 : pde.weaponLevel;
    }
    public int HammerLevel()
    {
        PlayerData.PlayerDataEntry pde = MainManager.Instance.playerData.GetPlayerDataEntry(EntityID.Luna);
        return pde == null ? 0 : pde.weaponLevel;
    }

    public void HazardZoneTouch()
    {
        SetActionState(ActionState.HazardFall);
    }

    public void HazardObjectTouch()
    {
        SetActionState(ActionState.HazardTouch);
    }


    //A lot of actions require a neutral state to do
    public bool InNeutralState(bool allowSwitch = false)
    {
        switch (actionState)
        {
            case ActionState.Land:
            case ActionState.Neutral:
                return allowSwitch || switchTime <= 0;
        }
        return false;
    }

    public bool GroundedState()
    {
        switch (actionState)
        {
            case ActionState.Neutral:
            case ActionState.Land:
            case ActionState.Aetherize:
            case ActionState.Illuminate:
            case ActionState.Dig:
            case ActionState.Slash:
            case ActionState.Smash:
            case ActionState.HazardTouch:
                return true;
        }

        return false;
    }

    public bool Unstable()
    {
        return !IsGrounded() || lastFloorUnstable || unstableZone;
    }
    
    public bool HazardState()
    {
        switch (actionState)
        {
            case ActionState.HazardTouch:
            case ActionState.HazardFall:
                return true;
        }

        return false;
    }

    public bool NonControlledState()
    {
        switch (actionState)
        {
            case ActionState.LaunchFall:
            case ActionState.HazardTouch:
            case ActionState.HazardFall:
            case ActionState.FreeCam:
            case ActionState.RevolvingCam:
                return true;
        }

        return false;
    }

    public bool CanMove()
    {
        return !NonControlledState() && MainManager.Instance.GetControlsEnabled();
    }

    //whether enemies can initiate normal encounters (scripted stuff can ignore this mostly)
    public bool NoTouchEncounter(bool ignoreDig = false, bool ignoreAether = false)
    {
        if (!CanMove())
        {
            return true;
        }

        if (!ignoreDig)
        {
            switch (actionState)
            {
                case ActionState.Dig:
                    return true;
                    //return !digCheck.CollisionCheck();  //note: exiting an encounter forces an undig, so don't allow undig in illegal situations
            }
        }

        if (!ignoreAether)
        {
            switch (actionState)
            {
                case ActionState.Aetherize:
                case ActionState.AetherizeFall:
                    return true;
            }
        }

        return encounterCooldown > 0;
    }

    public bool Concealed(bool ignoreConceal = false, bool ignoreDig = false, bool ignoreAether = false)
    {
        if (concealedZone && !ignoreConceal)
        {
            return true;
        }

        if (!ignoreDig)
        {
            switch (actionState)
            {
                case ActionState.Dig:
                    return true;
            }
        }

        if (!ignoreAether)
        {
            if (!ignoreAether)
            {
                switch (actionState)
                {
                    case ActionState.Aetherize:
                    case ActionState.AetherizeFall:
                        return true;
                }
            }
        }

        return false;
    }

    public void SetActionState(ActionState actionState)
    {
        if (MainManager.Instance.Cheat_OverworldHazardImmunity && actionState == ActionState.HazardFall)
        {
            return;
        }
        if (MainManager.Instance.Cheat_OverworldHazardImmunity && actionState == ActionState.HazardTouch)
        {
            return;
        }

        //Debug.Log(this.actionState + " -> " + actionState);

        timeSinceActionChange = 0;
        /*
        if (actionState == ActionState.HazardFall)
        {
            Debug.Log(actionState);
        }
        */

        //Some states set this to true
        movementRotationDisabled = false;
        switch (actionState)
        {
            case ActionState.Smash:
            case ActionState.Slash:
                movementRotationDisabled = true;
                break;
        }

        //enforce certain things
        //the "exit state" functions occur here
        //(Note that this means they should not be called directly since the set state will activate them)
        switch (this.actionState)
        {
            case ActionState.Dig:
                switch (actionState)
                {
                    case ActionState.Dig:
                        break;
                    default:
                        UnDig();
                        break;
                }
                break;
            case ActionState.Aetherize:
            case ActionState.AetherizeFall:
                switch (actionState)
                {
                    case ActionState.Aetherize:
                    case ActionState.AetherizeFall:
                        break;
                    default:
                        DeAether();
                        break;
                }
                break;
            case ActionState.Illuminate:
            case ActionState.IlluminateFall:
                switch (actionState)
                {
                    case ActionState.Illuminate:
                    case ActionState.IlluminateFall:
                        break;
                    default:
                        DeLight();
                        break;
                }
                break;
            case ActionState.Smash:
            case ActionState.Slash:
                switch (actionState)
                {
                    case ActionState.Smash:
                    case ActionState.Slash:
                        break;
                    default:
                        ResetWeapon();
                        break;
                }
                break;
            case ActionState.HazardFall:
            case ActionState.HazardTouch:
                hazardTime = 0;
                hazardState = 0;
                break;
            case ActionState.NoClip:
                NoClipUnsetup();
                break;
            case ActionState.Hover:
                HoverUnsetup();
                break;
            case ActionState.Climb:
                ClimbUnsetup();
                break;
        }

        this.actionState = actionState;
    }

    public void SetIdentity(BattleHelper.EntityID entityID)
    {
        switch (entityID)
        {
            case BattleHelper.EntityID.Wilex:
                currentCharacter = MainManager.PlayerCharacter.Wilex;
                if (KeruAsterJump())
                {
                    jumpImpulse = ASTER_JUMP_IMPULSE;
                    jumpLift = ASTER_JUMP_LIFT;
                } else
                {
                    jumpImpulse = WILEX_JUMP_IMPULSE;
                    jumpLift = WILEX_JUMP_LIFT;
                }
                //sprite.color = new Color(1f, 0.8f, 0.8f);
                //curfrontSprite = wfrontSprite;
                //curbackSprite = wbackSprite;
                break;
            case BattleHelper.EntityID.Luna:
                currentCharacter = MainManager.PlayerCharacter.Luna;
                if (KeruAsterJump())
                {
                    jumpImpulse = KERU_JUMP_IMPULSE;
                    jumpLift = KERU_JUMP_LIFT;
                }
                else
                {
                    jumpImpulse = LUNA_JUMP_IMPULSE;
                    jumpLift = LUNA_JUMP_LIFT;
                }
                //sprite.color = new Color(0.8f, 1f, 0.8f);
                //curfrontSprite = lfrontSprite;
                //curbackSprite = lbackSprite;
                break;
        }
        spriteID = ((MainManager.SpriteID)currentCharacter).ToString();
        Destroy(subObject);
        MakeAnimationController();
        if (height == 0 || width == 0)
        {
            SetColliderInformationWithAnimationController();
        }
    }

    /*
    //Drive the trail around
    public void FollowerTrailUpdate()
    {
        if (rb.isKinematic || mapScript.GetHalted())
        {
            return;
        }

        if (followerTrail.Get(0) == Vector3.negativeInfinity || (followerTrail.Get(0) - transform.position).magnitude > trailResolution)
        {
            followerTrail.Push(transform.position);
        }
    }

    public void ResetFollowerTrail()
    {
        for (int i = 0; i < 30; i++)
        {
            followerTrail.Push(Vector3.negativeInfinity);   //by convention this is a null value
        }
    }
    */

    //sets scripted input to the direction towards the point
    public void ScriptedMoveToFrame(Vector3 moveto)
    {
        scriptedInput = MainManager.XZProject(moveto - transform.position);
    }

    public IEnumerator ScriptedMoveTo(Vector3 moveto, float maxtime = 5f)
    {
        float lifetime = 0;
        while (MainManager.XZProject(transform.position - moveto).magnitude > 0.1f)
        {
            lifetime += Time.deltaTime;
            if (lifetime > maxtime)
            {
                Debug.LogWarning("ScriptedMoveTo player failsafe");
                transform.position = moveto;
                break;
            }

            scriptedInput = MainManager.XZProject(moveto - transform.position).normalized;
            yield return null;
        }
    }


    public void FollowerWarpSetState()
    {
        for (int i = 0; i < followers.Count; i++)
        {
            followers[i].WarpSetGroundState(IsGrounded(), floorNormal);
        }
    }

    public void FollowerWarp()
    {
        for (int i = 0; i < followers.Count; i++)
        {
            followers[i].Warp();
        }
    }

    //warp them in a line
    //(usually used for map transitions)
    public void FollowerWarp(Vector3 offset)
    {
        Vector3 start = transform.position;
        for (int i = 0; i < followers.Count; i++)
        {
            start += offset;
            followers[i].Warp(start);
        }
    }

    public void FollowerSetZDF()
    {
        for (int i = 0; i < followers.Count; i++)
        {
            followers[i].SetFollowUntilGrounded();
        }
    }

    public void FollowerResetBufferJump()
    {
        for (int i = 0; i < followers.Count; i++)
        {
            followers[i].ResetJumpBuffer();
        }
    }

    public void StartJump()
    {
        //Debug.Log("Jump " + timeSinceLastJump + " " + Time.deltaTime + " " + Time.time);
        rb.velocity = intendedMovement - rb.velocity.y * Vector3.up + Vector3.up * jumpImpulse;

        MainManager.Instance.PlaySound(gameObject, MainManager.Sound.SFX_Overworld_JumpGeneric);

        if (lastFloorSticky)
        {
            rb.velocity += Vector3.up * STICKY_FLOOR_JUMP_PENALTY * 0.5f;
        }

        /*
        if (!isGrounded)
        {
            rb.velocity = intendedMovement - rb.velocity.y * Vector3.up + Vector3.up * jumpImpulse;
        } else
        {
            Vector3 project(Vector3 a, Vector3 b)
            {
                return a - b * Vector3.Dot(a, b);
            }

            rb.velocity = intendedMovement - project(rb.velocity, floorNormal) + floorNormal * jumpImpulse;
        }
        */

        //???
        //Debug.Log(attachedVel);
        PlatformJumpMomentum();

        applyJumpLift = true;
        SetActionState(ActionState.Jump);
        isGrounded = false; //hacky fix
        semisolidFloorActive = false;
        semisolidSnapBelow = false;
        timeSinceLastJump = 0;
        canDoubleJump = true;
        canHover = true;
        jumpStomp = true;

        TransmitJump();

        if (currentCharacter == MainManager.PlayerCharacter.Luna)
        {
            landShockwavePossible = true;
        }

        enableDashLeniency = false;
    }

    public void StartDoubleJump()
    {
        //Debug.Log("double jump " + timeSinceLastJump + " " + Time.deltaTime + " " + Time.time);
        rb.velocity = intendedMovement - rb.velocity.y * Vector3.up + Vector3.up * doubleJumpImpulse;

        //no sticky penalty since there is no floor

        GameObject effect = Instantiate(Resources.Load<GameObject>("VFX/Overworld/Player/Effect_Jump_Spark"), MainManager.Instance.mapScript.transform);
        effect.transform.position = transform.position;

        MainManager.Instance.PlaySound(gameObject, MainManager.Sound.SFX_Overworld_JumpDouble);

        applyJumpLift = true;
        applyDoubleJumpLift = true;
        SetActionState(ActionState.DoubleJump);
        isGrounded = false; //hacky fix
        semisolidFloorActive = false;
        semisolidSnapBelow = false;
        timeSinceLastJump = 0;
        canDoubleJump = false;
        canHover = false;
        doubleJumpStomp = true;

        //followers[i].isGrounded = false;
        if (!MainManager.Instance.Cheat_SplitParty)
        {
            FollowerWarpSetState();
            FollowerSetZDF();
            for (int i = 0; i < followers.Count; i++)
            {
                followers[i].Jump(doubleJumpImpulse, doubleJumpLift);
            }
        }
    }

    public void StartDashHop()
    {
        Vector2 inputXY = Vector2.zero;
        inputXY.x = InputManager.GetAxisHorizontal();
        inputXY.y = InputManager.GetAxisVertical();
        if (inputXY.magnitude > 1)
        {
            inputXY = inputXY.normalized;
        }
        inputXY = MainManager.Instance.WorldspaceXZTransform(inputXY);

        Vector3 newVelocity = inputXY.x * GetDashSpeed() * Vector3.right + inputXY.y * GetDashSpeed() * Vector3.forward + Vector3.up * dashJumpImpulse;

        MainManager.Instance.PlaySound(gameObject, MainManager.Sound.SFX_Overworld_JumpDash);

        if (lastFloorSticky)
        {
            float stickyMult = 0.65f;   //airspeed gets dragged forward so this needs to be lower to compensate
            newVelocity = inputXY.x * GetDashSpeed() * stickyMult * Vector3.right + inputXY.y * GetDashSpeed() * stickyMult * Vector3.forward + Vector3.up * dashJumpImpulse;
            newVelocity += Vector3.up * STICKY_FLOOR_JUMP_PENALTY * 0.5f;
        }

        rb.velocity = newVelocity;

        //???
        PlatformJumpMomentum();

        dashHopStomp = true;

        /*
        if (!isGrounded)
        {
            rb.velocity = newVelocity;
        }
        else
        {
            Vector3 project(Vector3 a, Vector3 b)
            {
                return a - b * Vector3.Dot(a, b);
            }

            rb.velocity = inputXY.x * dashSpeed * project(Vector3.right, floorNormal) + inputXY.y * dashSpeed * project(Vector3.forward, floorNormal) + floorNormal * dashJumpImpulse;
        }
        */

        GameObject effect = Instantiate(Resources.Load<GameObject>("VFX/Overworld/Player/Effect_SmallShockwave"), MainManager.Instance.mapScript.transform);
        effect.transform.position = transform.position + Vector3.down * ((height / 2) + 0.05f);
        effect.transform.rotation = Quaternion.LookRotation(floorNormal) * Quaternion.FromToRotation(Vector3.up, Vector3.forward);

        GameObject effectB = Instantiate(Resources.Load<GameObject>("VFX/Overworld/Player/Effect_DashDust"), MainManager.Instance.mapScript.transform);
        effectB.transform.position = transform.position + Vector3.down * ((height / 2) + 0.05f);
        effectB.transform.rotation = Quaternion.LookRotation(inputXY.x * -Vector3.right + inputXY.y * -Vector3.forward);

        applyDashJumpLift = true;
        SetActionState(ActionState.Dash);
        isGrounded = false; //hacky fix
        semisolidFloorActive = false;
        semisolidSnapBelow = false;
        timeSinceLastJump = 0;

        enableDashLeniency = true;

        TransmitJump();

        MainManager.Instance.StartCoroutine(MainManager.Instance.CameraJump((inputXY.x * Vector3.right + inputXY.y * Vector3.forward) * 0.3f, 0.35f));
    }

    public void StartSuperJump()
    {
        //rb.velocity = rb.velocity - rb.velocity.y * Vector3.up + Vector3.up * superJumpImpulse;
        //start off with 0 momentum (supposed to be a vertical jump)s
        rb.velocity = Vector3.up * superJumpImpulse;

        if (lastFloorSticky)
        {
            rb.velocity += Vector3.up * STICKY_FLOOR_JUMP_PENALTY;
        }

        //???
        PlatformJumpMomentum();

        MainManager.Instance.PlaySound(gameObject, MainManager.Sound.SFX_Overworld_JumpSuper);

        /*
        if (!isGrounded)
        {
            rb.velocity = Vector3.up * superJumpImpulse;
        }
        else
        {
            rb.velocity = floorNormal * superJumpImpulse;
        }
        */

        GameObject effect = Instantiate(Resources.Load<GameObject>("VFX/Overworld/Player/Effect_Jump_Flame"), MainManager.Instance.mapScript.transform);
        effect.transform.position = transform.position + Vector3.down * ((height / 2) + 0.05f);

        GameObject eo = null;
        eo = Instantiate(Resources.Load<GameObject>("VFX/Overworld/Player/Effect_FireParticleTrail"), realOrientationObject.transform);
        eo.transform.localPosition = Vector3.down * 0.375f;
        eo.transform.localRotation = Quaternion.identity;

        GameObject eoB = null;
        eoB = Instantiate(Resources.Load<GameObject>("VFX/Overworld/Player/Effect_FireTrail"), gameObject.transform);
        eoB.transform.localPosition = Vector3.down * 0.325f + MainManager.Instance.Camera.transform.forward * 0.01f;
        eoB.transform.localRotation = Quaternion.identity;

        //decided to instead just mess with settings to mimic what it should look like instead of using this
        //since the collision plane thing would mess up if you moved upwards after digging
        //ParticleSystem ps = eo.GetComponent<ParticleSystem>();
        //ps.collision.SetPlane(0, floorNormalObject.transform);

        perpetualParticleObject = eo;
        stalePerpetualParticles.Add(eo);

        applySuperJumpLift = true;
        SetActionState(ActionState.SuperJump);
        isGrounded = false; //hacky fix
        semisolidFloorActive = false;
        semisolidSnapBelow = false;
        timeSinceLastJump = 0;

        superJumpStomp = true;

        if (!MainManager.Instance.Cheat_SplitParty)
        {
            FollowerWarpSetState();
            FollowerSetZDF();
            //TransmitJump();
            for (int i = 0; i < followers.Count; i++)
            {
                //followers[i].floorNormal = floorNormal;
                followers[i].Jump(superJumpImpulse, superJumpLift);
            }
        }

        enableDashLeniency = false;

        MainManager.Instance.StartCoroutine(MainManager.Instance.StandardCameraShake(0.25f, 0.2f, 0.5f, 0.5f));
    }

    public void StartWallJump()
    {
        //Debug.Log("Jump " + timeSinceLastJump + " " + Time.deltaTime + " " + Time.time);

        Vector3 wallLateral = MainManager.XZProject(wallVector);
        wallLateral = wallLateral.normalized;

        rb.velocity = wallLateral.x * GetSpeed() * airspeedBonus * Vector3.right + wallLateral.z * GetSpeed() * airspeedBonus * Vector3.forward + Vector3.up * jumpImpulse;

        if (lastFloorSticky)
        {
            rb.velocity += Vector3.up * STICKY_FLOOR_JUMP_PENALTY * 0.5f;
        }

        MainManager.Instance.PlaySound(gameObject, MainManager.Sound.SFX_Overworld_JumpDash);

        /*
        if (!isGrounded)
        {
            rb.velocity = intendedMovement - rb.velocity.y * Vector3.up + Vector3.up * jumpImpulse;
        } else
        {
            Vector3 project(Vector3 a, Vector3 b)
            {c
                return a - b * Vector3.Dot(a, b);
            }

            rb.velocity = intendedMovement - project(rb.velocity, floorNormal) + floorNormal * jumpImpulse;
        }
        */

        //???
        //Debug.Log(attachedVel);
        PlatformJumpMomentum();

        //followers[i].isGrounded = false;
        if (!MainManager.Instance.Cheat_SplitParty)
        {
            FollowerWarpSetState();
            FollowerSetZDF();
            for (int i = 0; i < followers.Count; i++)
            {
                followers[i].Jump(jumpImpulse, jumpLift);
            }
        }

        applyJumpLift = true;
        SetActionState(ActionState.WallJump);
        isGrounded = false; //hacky fix
        semisolidFloorActive = false;
        semisolidSnapBelow = false;
        timeSinceLastJump = 0;
        canDoubleJump = false;
        canHover = true;
        jumpStomp = true;

        enableDashLeniency = false;
    }

    public void StartSuperKick()
    {
        //rb.velocity = rb.velocity - rb.velocity.y * Vector3.up + Vector3.up * superJumpImpulse;
        //start off with 0 momentum (supposed to be a vertical jump)s
        rb.velocity = Vector3.up * superKickImpulse;

        if (lastFloorSticky)
        {
            rb.velocity += Vector3.up * STICKY_FLOOR_JUMP_PENALTY;
        }

        //???
        PlatformJumpMomentum();

        MainManager.Instance.PlaySound(gameObject, MainManager.Sound.SFX_Overworld_JumpSuper);

        /*
        if (!isGrounded)
        {
            rb.velocity = Vector3.up * superJumpImpulse;
        }
        else
        {
            rb.velocity = floorNormal * superJumpImpulse;
        }
        */

        applySuperKickLift = true;
        SetActionState(ActionState.SuperKick);
        isGrounded = false; //hacky fix
        semisolidFloorActive = false;
        semisolidSnapBelow = false;
        timeSinceLastJump = 0;

        doubleJumpStomp = true;

        if (!MainManager.Instance.Cheat_SplitParty)
        {
            FollowerWarpSetState();
            FollowerSetZDF();
            //TransmitJump();
            for (int i = 0; i < followers.Count; i++)
            {
                //followers[i].floorNormal = floorNormal;
                followers[i].Jump(superKickImpulse, superKickLift);
            }
        }        
    }

    public void PlatformJumpMomentum()
    {
        if (pavAirTime < PLATFORM_VELOCITY_RESET_TIME || Vector3.Dot(momentumVel, rb.velocity) >= 0)
        {
            rb.velocity += MLerp(momentumVel);
        }
    }

    public void TransmitJump()
    {
        for (int i = 0; i < followers.Count; i++)
        {
            followers[i].BufferJump(JUMP_DELAY * (i + 1));
        }
    }

    public ActionState GetActionState()
    {
        return actionState;
    }

    public bool IsAether()
    {
        return actionState == ActionState.Aetherize || actionState == ActionState.AetherizeFall;
    }

    public bool IsLight()
    {
        return actionState == ActionState.Illuminate || actionState == ActionState.IlluminateFall;
    }

    public float GetAetherTime()
    {
        return aetherTime;
    }

    public float GetLightTime()
    {
        return lightTime;
    }

    public float GetNotAetherTime()
    {
        return notAetherTime;
    }

    public float GetNotLightTime()
    {
        return notLightTime;
    }

    public float GetAetherShaderTime()
    {
        return aetherShaderTime;
    }

    public float GetLightShaderTime()
    {
        return lightShaderTime;
    }

    public float GetBubbleShaderTime()
    {
        return bubbleShaderTime;
    }

    public float GetLeafShaderTime() {
        return leafShaderTime;
    }

    public float GetJumpImpulse()
    {
        return jumpImpulse;
    }
    public float GetTimeSinceLastJump()
    {
        return timeSinceLastJump;
    }

    public override void Launch(Vector3 launchVelocity, float momentumStrength = 0)
    {
        base.Launch(launchVelocity, momentumStrength);
        SetActionState(ActionState.Fall);
        //Debug.Log(rb.velocity + " " + prevAttachedVel);
    }

    public override void ScriptedLaunch(Vector3 launchVelocity, float momentumStrength = 0)
    {
        base.ScriptedLaunch(launchVelocity, momentumStrength);
        SetActionState(ActionState.LaunchFall);
    }

    public Vector3 GetEyePoint()
    {
        return transform.position + Vector3.up * EYE_HEIGHT;    //implicitly has a +0.375
    }

    //delta is rightward displacement
    //cross of up and facing is a rightward vector I think (relative to the facing direction)
    public RaycastHit FacingRaycast(float maxDist, int layerMask = PLAYER_LAYER_MASK, QueryTriggerInteraction q = QueryTriggerInteraction.Ignore, float delta = 0)
    {
        RaycastHit output;
        Vector3 start = GetEyePoint();

        Vector3 cross = Vector3.Cross(Vector3.up, FacingVector());

        Physics.Raycast(start + cross * delta, FacingVector(), out output, maxDist, layerMask, q);

        if (output.collider != null)
        {
#pragma warning disable CS0162
            if (DRAW_DEBUG_RAYS)
            {
                //Debug.Log("draw ray");
                Debug.DrawRay(start + cross * delta, output.point - start, Color.white, 0.5f, true);
            }
        } else
        {
            if (DRAW_DEBUG_RAYS)
            {
                Debug.DrawRay(start + cross * delta, FacingVector() * maxDist, Color.red, 0.5f, true);
            }
        }
        //Debug.Log((output.collider != null) + " " + start + " " + (output.point));
        return output;
    }
    public RaycastHit FacingRaycastAngleDelta(float maxDist, int layerMask = PLAYER_LAYER_MASK, QueryTriggerInteraction q = QueryTriggerInteraction.Ignore, float delta = 0)
    {
        RaycastHit output;
        Vector3 start = GetEyePoint();

        Vector3 newFacing = FacingVector();

        Quaternion rotation = Quaternion.Euler(0, delta, 0);

        newFacing = rotation * newFacing;

        Physics.Raycast(start, newFacing, out output, maxDist, layerMask, q);

        if (output.collider != null)
        {
            #pragma warning disable CS0162
            if (DRAW_DEBUG_RAYS)
            {
                //Debug.Log("draw ray");
                Debug.DrawRay(start, output.point - start, Color.white, 0.5f, true);
            }
        }
        else
        {
            if (DRAW_DEBUG_RAYS)
            {
                Debug.DrawRay(start, newFacing * maxDist, Color.red, 0.5f, true);
            }
        }
        //Debug.Log((output.collider != null) + " " + start + " " + (output.point));
        return output;
    }
    public RaycastHit FacingRaycastVOffset(float maxDist, float voffset, int layerMask = PLAYER_LAYER_MASK, QueryTriggerInteraction q = QueryTriggerInteraction.Ignore, float delta = 0)
    {
        RaycastHit output;
        Vector3 start = GetEyePoint() + Vector3.up * voffset;

        Vector3 cross = Vector3.Cross(Vector3.up, FacingVector());

        Physics.Raycast(start + cross * delta, FacingVector(), out output, maxDist, layerMask, q);

        if (output.collider != null)
        {
#pragma warning disable CS0162
            if (DRAW_DEBUG_RAYS)
            {
                //Debug.Log("draw ray");
                Debug.DrawRay(start + cross * delta, output.point - start, Color.white, 0.5f, true);
            }
        }
        else
        {
            if (DRAW_DEBUG_RAYS)
            {
                Debug.DrawRay(start + cross * delta, FacingVector() * maxDist, Color.red, 0.5f, true);
            }
        }
        //Debug.Log((output.collider != null) + " " + start + " " + (output.point));
        return output;
    }

    public void TryTattle()
    {
        //Debug.Log("Tattle");
        
        int layerMask = PLAYER_LAYER_MASK;
        RaycastHit rcHit = FacingRaycastVOffset(0.75f, -0.35f, layerMask);

        //is raycasthit a legal tattle target?
        ITattleable tattleTarget = null;

        if (rcHit.collider != null)
        {
            tattleTarget = rcHit.collider.gameObject.GetComponent<ITattleable>();
        }

        //Try again
        if (tattleTarget == null)
        {
            rcHit = FacingRaycastAngleDelta(0.75f, layerMask, QueryTriggerInteraction.Ignore, 30);
            if (rcHit.collider != null)
            {
                tattleTarget = rcHit.collider.gameObject.GetComponent<ITattleable>();
            }
        }

        //Try again
        if (tattleTarget == null)
        {
            rcHit = FacingRaycastAngleDelta(0.75f, layerMask, QueryTriggerInteraction.Ignore, -30);
            if (rcHit.collider != null)
            {
                tattleTarget = rcHit.collider.gameObject.GetComponent<ITattleable>();
            }
        }


        if (tattleTarget != null)
        {
            //Object tattle
            //StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(MainManager.Instance.testTextFile, 0, this));
            string tattle = tattleTarget.GetTattle();

            //use text shorthand?
            string[][] file = null;
            int index = -1;
            (file, index) = FormattedString.GetTextFileFromShorthandFull(tattle);

            //file is passed by reference so no chance of memory leak or using too much memory (?)

            if (file == null)
            {
                if (FormattedString.IsStringCSV(tattle))
                {
                    //make a new file
                    StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(MainManager.CSVParse(tattle), index, this));
                } else
                {
                    //make a new 1 liner file
                    string[][] oneliner = new string[1][];
                    oneliner[0] = new string[1];
                    oneliner[0][0] = tattle;
                    StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(oneliner, 0, this));
                }
            } else
            {
                //Read that file
                StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(file, index, this));
            }
        }
        else
        {
            if (zoneTattleTarget != null)
            {
                //Object tattle
                //StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(MainManager.Instance.testTextFile, 0, this));
                string tattle = zoneTattleTarget.GetTattle();

                //use text shorthand?
                string[][] file = null;
                int index = -1;
                (file, index) = FormattedString.GetTextFileFromShorthandFull(tattle);

                //file is passed by reference so no chance of memory leak or using too much memory (?)

                if (file == null)
                {
                    if (FormattedString.IsStringCSV(tattle))
                    {
                        //make a new file
                        StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(MainManager.CSVParse(tattle), index, this));
                    }
                    else
                    {
                        //make a new 1 liner file
                        string[][] oneliner = new string[1][];
                        oneliner[0] = new string[1];
                        oneliner[0][0] = tattle;
                        StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(oneliner, 0, this));
                    }
                }
                else
                {
                    //Read that file
                    StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(file, index, this));
                }
            } else
            {
                //Map tattle
                //StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(MainManager.Instance.testTextFile, 3, this));

                string tattle = mapScript.GetTattle(); //tattleTarget.GetTattle();

                //use text shorthand?
                string[][] file = null;
                int index = -1;
                (file, index) = FormattedString.GetTextFileFromShorthandFull(tattle);

                //file is passed by reference so no chance of memory leak or using too much memory (?)

                if (file == null)
                {
                    if (FormattedString.IsStringCSV(tattle))
                    {
                        //make a new file
                        StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(MainManager.CSVParse(tattle), index, this));
                    }
                    else
                    {
                        //make a new 1 liner file
                        string[][] oneliner = new string[1][];
                        oneliner[0] = new string[1];
                        oneliner[0][0] = tattle;
                        StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(oneliner, 0, this));
                    }
                }
                else
                {
                    //Read that file
                    StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(file, index, this));
                }

                //StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(MainManager.Instance.testTextFile, 3, this));

            }
        }
    }

    public void ResetEncounterCooldown()    //Use when you should not get the cooldown (map transitions, various cutscenes)
    {
        encounterCooldown = 0;
    }
    public void SetEncounterCooldown()      //When the cooldown should apply (fleeing battles)
    {
        encounterCooldown = DEFAULT_ENCOUNTER_COOLDOWN;
    }
    public float GetEncounterCooldown()
    {
        return encounterCooldown;
    }


    public override void EnableSpeakingAnim()
    {
        isSpeaking = true;
        scriptedAnimation = true;
        SetAnimation("talk", true);
    }
    public override void DisableSpeakingAnim()
    {
        isSpeaking = false;
        scriptedAnimation = false;
        ShowIdleAnimation(true);
    }

    public void ShowIdleAnimation(bool force = false)
    {
        if (MainManager.Instance.playerData.ShowDangerAnim(currentCharacter) && !showBack)  //I don't have idleweak_back
        {
            SetAnimation("idleweak", force);
        } else
        {
            SetAnimation("idle", force);
        }
    }
}
