using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using UnityEditor.AssetImporters;
using UnityEngine;

//special class for data setup of NPCs and enemies
//Note that super specific stuff shouldn't end up here (this should be for more generic data)
//Note that WorldPlayer and WorldFollower use hardcoded stuff and not this
[System.Serializable]
public class WorldEntityData
{
    //Ignore WED if true
    public bool inactive; //Unity doesn't allow nullable types in inspector >:(

    public string spriteID;
    //public MainManager.SpriteID spriteID;
    public float speed;
    public bool wandering;
    public float wanderRadius;
    public float idleDuration;
    public float wanderDuration;
    public float durationVariance;
    public float height;
    public float width;
    public float interactRadius;

    public bool stompBoostable;
    public float stompLaunch;

    //Can (Should) accept shorthand
    //  (Use file shorthand because that will avoid having to hunt down dialogue)
    public string tattleString;
    public List<string> talkStrings;
}

//World entities are all npcs, player characters, and enemies
public class WorldEntity : WorldObject, ITextSpeaker
{
    //Map entity ID (players get negative IDs)
    public int meid;

    public string spriteID;
    //public MainManager.SpriteID spriteID;

    public WorldEntityData wed;


    //variables not meant for the inspector are hidden
    //Can't remember if they all should be public but ehh


    //protected Animator animator;
    public GameObject subObject;        //where the animationcontroller object will go (If it is null, the correct animcontroller gets spawned
    public AnimationController ac;
    protected bool isSpeaking;

    protected bool hasCapsuleCollider;
    protected CapsuleCollider capsuleCollider;
    protected float width;
    protected float height;
    protected float interactRadius;

    public GameObject dropShadow;
    public bool noShadowUpdate; //disables the shadow raycasting thing (but if the entity is always grounded the check gets skipped anyways)

    //public MapScript mapScript;

    public float speed;


    public const int PLAYER_LAYER_MASK = 311;

    protected float targetFacingRotation;   //sprite
    protected float facingRotation;   //sprite
    protected bool showBack;
    protected float trueFacingRotation; //determined by movement
    protected float pastTrueFacingRotation;

    [HideInInspector]
    public Vector3 lastFrameVel;

    [HideInInspector]
    public bool isGrounded = false;
    [HideInInspector]
    public float groundedTime = 0;
    [HideInInspector]
    public float airTime = 0;
    [HideInInspector]
    public int airFrames = 0;
    [HideInInspector]
    public Vector3 floorNormal;
    [HideInInspector]
    public float lastGroundedHeight;
    [HideInInspector]
    public float lastHighestHeight; //highest point you have reached before being grounded (0 y velocity also resets this) (Note that this is feet pos similar to lastGroundedHeight)
    [HideInInspector]
    public bool touchingNonGround = false;

    [HideInInspector]
    public float timeSinceLaunch = 0;

    protected Rigidbody attached;
    protected Rigidbody prevAttached;
    protected float pavAirTime;         //reduces effect of momentum over time (reset by things that give momentum)
    protected Vector3 momentumVel;  //modifies the speed cap to give you bonus momentum, also skews your jump momentum
    protected Vector3 attachedVel;
    protected Vector3 attachedPos;
    protected Vector3 attachedLocalPos;

    [HideInInspector]
    public bool applyDefaultJumpLift;
    [HideInInspector]
    public float defaultJumpLift;

    //rps
    protected const float SPIN_RATE = 3;

    protected const float MIN_GROUND_NORMAL = 0.75f;

    protected const float PLATFORM_VELOCITY_RESET_TIME = 0.075f;

    public const bool DRAW_DEBUG_RAYS = false;

    [HideInInspector]
    public Vector3 intendedMovement;
    [HideInInspector]
    public Vector3 lastIntendedMovement;
    [HideInInspector]
    public bool useIntended = true;

    [HideInInspector]
    public bool useAllFriction = false;

    [HideInInspector]
    public bool movementRotationDisabled = false;

    [HideInInspector]
    public Vector3 oneWayVector;
    [HideInInspector]
    public float oneWayMinAllowable;
    [HideInInspector]
    public Vector3 conveyerVector;  //Constantly reset, use OnCollisionStay code to constantly set it
    [HideInInspector]
    public Vector3 lastConveyerVector;
    [HideInInspector]
    public float movementDamping;   //uses exponential thing to reduce velocity (0 = no damping though)
    [HideInInspector]
    public float lastMovementDamping;
    [HideInInspector]
    public Vector3 semisolidFloorNormal;
    [HideInInspector]
    public Vector3 semisolidFloorPosition;  //this and the normal are used to fix your position onto the plane
    [HideInInspector]
    public bool semisolidFloorActive;       //so I can make triggers into floors
    [HideInInspector]
    public bool lastSemiSolidFloorActive;
    [HideInInspector]
    public bool semisolidSnapBelow;


    public virtual void OnDrawGizmos()
    {
        Gizmos.color = Color.gray;
        if (wed == null || wed.inactive)
        {
            Gizmos.color = Color.black;
        }
        Gizmos.DrawSphere(transform.position + Vector3.up * 0.5f, 0.1f);

        Gizmos.color = Color.white;
        Gizmos.DrawLine(transform.position + new Vector3(0f, 0f, 0f), transform.position + new Vector3(0, 0.5f, 0f));
        Gizmos.DrawLine(transform.position + new Vector3(-0.5f, 0.25f, 0f), transform.position + new Vector3(0.5f, 0.25f, 0f));
        Gizmos.DrawLine(transform.position + new Vector3(0f, 0f, 0f), transform.position + new Vector3(-0.5f, -0.5f, 0f));
        Gizmos.DrawLine(transform.position + new Vector3(0f, 0f, 0f), transform.position + new Vector3(0.5f, -0.5f, 0f));

        Gizmos.color = Color.white;
        Gizmos.DrawLine(transform.position + Vector3.up * 0.5f, transform.position + Vector3.up * 0.5f + FacingVector());
    }


    public override void Awake()
    {
        if (wed != null && !wed.inactive)
        {
            SetWorldEntityData(wed);
        }

        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.WakeUp();
        }
        attached = null;
        prevAttached = null;
        //if (GetComponent<CapsuleCollider>() != null)
        //{
            capsuleCollider = GetComponent<CapsuleCollider>();
        //}

        if (capsuleCollider != null)
        {
            height = capsuleCollider.height;
            width = capsuleCollider.radius;
        }

        mapScript = FindObjectOfType<MapScript>();

        if (subObject != null && ac == null)
        {
            ac = subObject.GetComponent<AnimationController>();
        }
        //subObject = transform.GetChild(0).gameObject;   //hardcoded for now

        if (subObject == null)
        {
            MakeAnimationController();
        }

        //Fix bug with the shadows of newly spawned in entities
        DropShadowUpdate(true);
    }

    public virtual void SetWorldEntityData(WorldEntityData wed)
    {
        spriteID = wed.spriteID;
        speed = wed.speed;
        height = wed.height;
        width = wed.width;
        if (height != 0)
        {
            Debug.LogWarning("Zero height in World Entity Data");
            capsuleCollider.height = height;
        }
        if (width != 0)
        {
            Debug.LogWarning("Zero width in World Entity Data");
            capsuleCollider.radius = width; //note this (width is half of what it "should" be)
        }
        interactRadius = wed.interactRadius;
    }

    /*
    public virtual void Update()
    {
        rb.isKinematic = mapScript.GetHalted();
    }
    */

    public virtual float GetSpeed()
    {
        return speed;
    }

    public override void WorldFixedUpdate()
    {
        SpriteRotationUpdate();

        SpriteAnimationUpdate();

        DropShadowUpdate();

        if (applyDefaultJumpLift)
        {
            if (rb.velocity.y <= 0)// || isGrounded)
            {
                applyDefaultJumpLift = false;
            } else
            {
                rb.velocity = rb.velocity + Vector3.up * defaultJumpLift;
            }
        }

        if (IsGrounded())
        {
            groundedTime += Time.fixedDeltaTime;
            airTime = 0;
            airFrames = 0;
        } else
        {
            groundedTime = 0;
            airTime += Time.fixedDeltaTime;
            pavAirTime += Time.fixedDeltaTime;
            airFrames++;
        }

        if (semisolidFloorActive != lastSemiSolidFloorActive && airTime > 0.25f)
        {
            transform.position = Vector3.up * (height / 2) + Vector3.ProjectOnPlane(transform.position - semisolidFloorPosition, semisolidFloorNormal) + semisolidFloorPosition;
        }
        if (semisolidFloorActive)
        {
            lastGroundedHeight = transform.position.y;
            lastHighestHeight = lastGroundedHeight;
            /*
            if (rb.velocity.y < -Physics.gravity.y * Time.fixedDeltaTime)
            {
                rb.velocity = rb.velocity.z * Vector3.forward + rb.velocity.x * Vector3.right + (-Physics.gravity.y * Time.fixedDeltaTime * Vector3.up);
            }
            */

            if (semisolidSnapBelow)
            {
                float snapDot = Vector3.Dot(transform.position - semisolidFloorPosition - Vector3.up * (height / 2), semisolidFloorNormal);
                if (snapDot < 0)
                {
                    transform.position = Vector3.up * (height / 2) + Vector3.ProjectOnPlane(transform.position - semisolidFloorPosition - Vector3.up * (height / 2), semisolidFloorNormal) + semisolidFloorPosition + Vector3.up * (height / 2);
                }
            }

            //unfortunately life is not that simple
            float dotProduct = Vector3.Dot((rb.velocity + Physics.gravity * Time.fixedDeltaTime), semisolidFloorNormal);

            if (dotProduct < 0)
            {
                //replace with something that results in a dot product of 0
                Vector3 newVelocity = Vector3.ProjectOnPlane((rb.velocity + Physics.gravity * Time.fixedDeltaTime), semisolidFloorNormal);
                rb.velocity = newVelocity - Physics.gravity * Time.fixedDeltaTime;
            }
        }

        if (rb != null && !IsGrounded())
        {
            if (rb.velocity.y == 0)
            {
                lastHighestHeight = transform.position.y - height/2;
            } else
            {
                if (transform.position.y - height/2 > lastHighestHeight)
                {
                    lastHighestHeight = transform.position.y - height/2;
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
            if (IsGrounded() && groundedTime > PLATFORM_VELOCITY_RESET_TIME)
            {
                momentumVel = Vector3.zero;
                pavAirTime = 0;
            }
        }

        //conveyers that lift you off the ground need to properly set that up to avoid having the ground sticky stuff fighting back against it
        if (AntiGravity() && IsGrounded())
        {
            isGrounded = false;
            semisolidFloorActive = false;
            semisolidSnapBelow = false;
            airFrames = 2;  //the ground snap checks for this = 1
            airTime = 0.1f;
        }

        if (movementDamping != 0)
        {
            rb.velocity = MainManager.EasingExponentialTime(rb.velocity, conveyerVector - conveyerVector.y * Vector3.up, 1 / movementDamping);
        }

        if (rb != null)
        {
            lastFrameVel = rb.velocity;
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

        isGrounded = false;
        touchingNonGround = false;
        floorNormal = Vector3.zero;
        prevAttached = attached;
        attached = null;
        //prevAttachedVel = attachedVel;
        attachedVel = Vector3.zero;
        //attachedPos = Vector3.zero;
        //attachedLocalPos = Vector3.zero;
        timeSinceLaunch += Time.fixedDeltaTime;
    }

    public virtual void SetMomentumVel(Vector3 vel)
    {
        momentumVel = vel;
    }

    public virtual void AttachUpdate()
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
            } else
            {
                attachedVel.y *= 2f;
            }

            if (IsGrounded())
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

    public virtual void SpriteRotationUpdate()
    {
        if (rb == null || subObject == null)
        {
            return;
        }

        bool pastShowBack = showBack;

        Vector3 usedMovement = useIntended ? intendedMovement : rb.velocity;

        //Debug.Log("a" + trueFacingRotation);
        if (pastTrueFacingRotation != trueFacingRotation || usedMovement.x != 0 || usedMovement.z != 0)
        {
            if (!movementRotationDisabled && (usedMovement.x != 0 || usedMovement.z != 0))
            {
                trueFacingRotation = -Vector2.SignedAngle(Vector2.right, usedMovement.x * Vector2.right + usedMovement.z * Vector2.up);
                //transform this with respect to worldspace yaw
                trueFacingRotation -= MainManager.Instance.GetWorldspaceYaw();
            }

            if (trueFacingRotation < 0)
            {
                trueFacingRotation += 360;
            }

            //Debug.Log(hiddenFacingRotation);

            while (trueFacingRotation < 0)
            {
                trueFacingRotation += 360;
            }
            while (trueFacingRotation > 360)
            {
                trueFacingRotation -= 360;
            }

            //going straight back or forward is a little weird, so don't rotate in a 10 degree range
            bool norotate = false;
            if (trueFacingRotation > 85f && trueFacingRotation < 95f)
            {
                norotate = true;
            }
            if (trueFacingRotation > 265f && trueFacingRotation < 275f)
            {
                norotate = true;
            }

            if (!norotate)
            {
                targetFacingRotation = 0;
                if (trueFacingRotation > 90 && trueFacingRotation < 270)
                {
                    targetFacingRotation = 180;
                }
            }

            //by convention, 0 and 180 (straight right and left) are front facing
            //may want to add some leeway?            

            showBack = trueFacingRotation < 360 && trueFacingRotation > 180;

            if ((pastShowBack ^ showBack))
            {
                //hardcode for now
                facingRotation = targetFacingRotation;
            }

            pastShowBack = showBack;
        }
        //Debug.Log("b" + trueFacingRotation);

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
                if (facingRotation > 360)
                {
                    facingRotation -= 360;
                }
            }
        }

        //need to add a correction for some reason (Lighting fix)
        float correctedRotation = facingRotation;

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

        pastTrueFacingRotation = trueFacingRotation;
    }

    public virtual void SpriteAnimationUpdate()
    {
        if (ac == null)
        {
            return;
        }

        if (SpeakingAnimActive())
        {
            return;
        }

        string animName = "";

        if (IsGrounded())
        {
            if (rb != null && rb.velocity.magnitude > 0.25f)
            {
                animName = "walk";
            } else
            {
                animName = "idle";
            }
        } else
        {
            if (rb != null && rb.velocity.y > 0)
            {
                animName = "jump";
            }
            else
            {
                animName = "fall";
            }
        }

        if (ac != null)
        {
            ac.SetAnimation(animName);
        }
    }

    public void DropShadowUpdate(bool force = false)
    {
        if (!force && (dropShadow == null || noShadowUpdate || rb.velocity == Vector3.zero))    //assumes that the floor doesn't drop from under you (but you would also have to be perfectly stationary)
        {
            return;
        }

        if (dropShadow == null)
        {
            return;
        }

        float slopeHeight = 0;

        if (IsGrounded())
        {
            slopeHeight = MainManager.XZProject(floorNormal).magnitude;
            slopeHeight *= 1.5f * width;
        } else
        {
            //arbitrary stuff to make sure you cast shadows on slopes properly
            slopeHeight = 1f;
            slopeHeight *= 1.5f * width;
        }

        float shadowHeightDelta = slopeHeight + 0.01f;

        //calculated dynamically so that casting shadows off edges isn't messed up

        float shadowHeight = 5 - shadowHeightDelta;

        if (IsGrounded()) //should speed things up significantly
        {
            shadowHeight = 0.1f;
        } else
        {
            RaycastHit rc = DownRaycast(5, 0.1f);


            if (rc.collider != null)
            {
                shadowHeight = rc.distance;
            }

        }
        shadowHeight += shadowHeightDelta;


        dropShadow.transform.localPosition = Vector3.down * ((height / 2) + shadowHeight / 2 - 0.005f);
        dropShadow.transform.localScale = (Vector3.right + Vector3.forward) * width * 2f + Vector3.up * (shadowHeight); 
    }

    public Vector3 MLerp(Vector3 inputVel)
    {
        float lerpCoeff = pavAirTime / 2;
        if (lerpCoeff > 1)
        {
            lerpCoeff = 1;
        }
        //quadratic thing (but keeps 0 -> 0, 1 -> 1)
        //lerpCoeff = 2 * lerpCoeff - lerpCoeff * lerpCoeff;

        inputVel = Vector3.Lerp(inputVel, Vector3.zero, lerpCoeff);

        return inputVel;
    }

    public override void ProcessCollision(Collision collision)
    {
        float accumulatedGroundHeight = 0;
        int accumulatedGround = 0;
        Vector3 accumulatedFloorNormal = Vector3.zero;

        float accumulatedNonGroundHeight = 0;
        int accumulatedNonGround = 0;
        Vector3 accumulatedNonGroundNormal = Vector3.zero;

        foreach (ContactPoint contact in collision.contacts)
        {
            if (contact.normal.normalized[1] > MIN_GROUND_NORMAL && LegalGround(collision)) //basically, any incline less than about 40 degrees can be jumped from
            {
                accumulatedGroundHeight += contact.point.y;
                accumulatedGround++;
                accumulatedFloorNormal += contact.normal.normalized;
            } else
            {
                accumulatedNonGroundHeight += contact.point.y;
                accumulatedNonGround++;
                accumulatedNonGroundNormal += contact.normal.normalized;

                touchingNonGround = true;
            }
        }

        if (!AntiGravity() && accumulatedGround > 0)
        {
            lastGroundedHeight = accumulatedGroundHeight / accumulatedGround;
            lastHighestHeight = lastGroundedHeight;
            isGrounded = true;
            floorNormal = accumulatedFloorNormal.normalized;
            attached = collision.rigidbody;
        }

        if (!AntiGravity() && accumulatedNonGround > 0)
        {
            if (accumulatedNonGroundNormal.normalized.y > MIN_GROUND_NORMAL && LegalGround(collision))
            {
                lastGroundedHeight = accumulatedNonGroundHeight / accumulatedNonGround;
                lastHighestHeight = lastGroundedHeight;
                isGrounded = true;
                floorNormal = accumulatedNonGroundNormal;
                attached = collision.rigidbody;
            }
        }
    }

    public virtual void DoSemisolidLanding(Vector3 position, Vector3 normal, Rigidbody attached, bool snapBelow)
    {
        semisolidFloorActive = true;
        semisolidFloorNormal = normal;
        semisolidFloorPosition = position;
        floorNormal = semisolidFloorNormal;
        lastGroundedHeight = semisolidFloorPosition.y;
        lastHighestHeight = lastGroundedHeight;
        this.semisolidSnapBelow = snapBelow;
        this.attached = attached;
    }

    public bool AntiGravity()
    {
        float g = -Physics.gravity.y * Time.fixedDeltaTime;
        return conveyerVector.y > g || lastConveyerVector.y > g;
    }

    public bool IsGrounded()
    {
        return isGrounded || semisolidFloorActive;
    }

    //is this surface hardcoded not to be considered ground?
    public bool LegalGround(Collision collision)
    {
        return LegalGround(collision.collider);
    }

    public bool LegalGround(Collider collider)
    {
        if (collider.CompareTag("NPC"))
        {
            return false;
        }

        if (collider.CompareTag("Collectible"))
        {
            return false;
        }

        if (collider.CompareTag("Launcher"))
        {
            return false;
        }

        if (collider.gameObject.layer == LayerMask.NameToLayer("Cloud"))
        {
            return false;
        }

        return true;
    }

    public virtual void Jump(float jumpImpulse, float jumpLift = 0)
    {
        this.defaultJumpLift = jumpLift;
        if (jumpLift != 0)
        {
            applyDefaultJumpLift = true;
        }

        rb.velocity = rb.velocity - rb.velocity.y * Vector3.up + Vector3.up * jumpImpulse;

        /*
        if (!isGrounded)
        {
            rb.velocity = rb.velocity - rb.velocity.y * Vector3.up + Vector3.up * jumpImpulse;
        }
        else
        {
            //calculate with floor normal
            Vector3 project(Vector3 a, Vector3 b)
            {
                return a - b * Vector3.Dot(a, b);
            }

            rb.velocity = rb.velocity - project(floorNormal, rb.velocity) + floorNormal * jumpImpulse;
        }
        */
    }

    public virtual void Launch(Vector3 launchVelocity, float momentumStrength = 0)
    {
        rb.velocity = launchVelocity;
        if (momentumStrength != 0 && timeSinceLaunch > 0.05f)
        {
            momentumVel = MLerp(momentumVel);
            momentumVel += momentumStrength * (launchVelocity + launchVelocity.y * Vector3.down);
            pavAirTime = 0;
        }
        isGrounded = false;
        semisolidFloorActive = false;
        semisolidSnapBelow = false;
        airFrames = 2;
        timeSinceLaunch = 0;
    }

    public virtual void ScriptedLaunch(Vector3 launchVelocity, float momentumStrength = 0)
    {
        rb.velocity = launchVelocity;
        if (momentumStrength != 0 && timeSinceLaunch > 0.05f)
        {
            momentumVel = MLerp(momentumVel);
            momentumVel += momentumStrength * (launchVelocity + launchVelocity.y * Vector3.down);
            pavAirTime = 0;
        }
        isGrounded = false;
        semisolidFloorActive = false;
        semisolidSnapBelow = false;
        airFrames = 2;
        timeSinceLaunch = 0;
    }

    //Call this when subobject is null (to make the right animation controller)
    public void MakeAnimationController()
    {
        AnimationController ac = null;

        MainManager.SpriteID realSprite;
        Enum.TryParse(spriteID, true, out realSprite);
        if (realSprite == MainManager.SpriteID.Wilex && !spriteID.Equals("Wilex"))
        {
            Debug.LogError("Could not parse sprite " + spriteID);
        }
        ac = MainManager.CreateAnimationController(realSprite, gameObject);

        this.ac = ac;
        subObject = ac.gameObject;
    }


    public virtual Vector3 GetTextTailPosition()
    {
        return transform.position + Vector3.up * height;
    }

    public virtual Vector3 GetCenterPosition()
    {
        return transform.position + Vector3.up * (height / 2);
    }

    public virtual string RequestTextData(string request)
    {
        return "";
    }

    public virtual void SendTextData(string data)
    {

    }

    public virtual void EnableSpeakingAnim()
    {
        isSpeaking = true;       
        SetAnimation("talk");
    }
    public virtual bool SpeakingAnimActive()
    {
        return isSpeaking;
    }
    public virtual void DisableSpeakingAnim()
    {
        isSpeaking = false;
        SetAnimation("idle");   //note: no way to check for what the real last anim was at this point
    }

    public virtual void SetAnimation(string name)
    {
        if (ac != null)
        {
            ac.SetAnimation(name);
        }
    }
    public virtual void SendAnimationData(string data)
    {
        if (ac != null)
        {
            ac.SendAnimationData(data);
        }
    }

    public virtual void TextBleep()
    {

    }


    public virtual void SetFacing(Vector3 facingTarget)
    {
        SetTrueFacingRotation(ConvertVectorToRotation(facingTarget - transform.position));
    }

    public virtual void EmoteEffect(TagEntry.Emote emote)
    {
        switch (emote)
        {
            case TagEntry.Emote.Alert:
                Particle_Alert();
                break;
            case TagEntry.Emote.Question:
                Particle_Miss();
                break;
            case TagEntry.Emote.AngryFizzle:
                Particle_GiveUp();
                break;
        }
    }

    public void Particle_Alert()
    {
        GameObject eo = Instantiate(Resources.Load<GameObject>("VFX/Overworld/Effect_Enemy_Alert"), gameObject.transform);
        eo.transform.position = transform.position;
        if (height != 0)
        {
            eo.transform.position += Vector3.down + Vector3.up * height;
        }
        EffectScript_Generic es_g = eo.GetComponent<EffectScript_Generic>();
        es_g.Setup(1, 1);
    }
    public void Particle_Miss()
    {
        GameObject eo = Instantiate(Resources.Load<GameObject>("VFX/Overworld/Effect_Enemy_Miss"), gameObject.transform);
        eo.transform.position = transform.position;
        if (height != 0)
        {
            eo.transform.position += Vector3.down + Vector3.up * height;
        }
        EffectScript_Generic es_g = eo.GetComponent<EffectScript_Generic>();
        es_g.Setup(1, 1);
    }
    public void Particle_GiveUp()
    {
        GameObject eo = Instantiate(Resources.Load<GameObject>("VFX/Overworld/Effect_Enemy_GiveUp"), gameObject.transform);
        eo.transform.position = transform.position;
        if (height != 0)
        {
            eo.transform.position += Vector3.down + Vector3.up * height;
        }
        EffectScript_Generic es_g = eo.GetComponent<EffectScript_Generic>();
        es_g.Setup(1, 1);
    }


    public float GetTrueFacingRotation()
    {
        return trueFacingRotation;
    }

    public void SetTrueFacingRotation(float rot)
    {
        if (rot > 360)
        {
            rot -= 360;
        }
        if (rot < 0)
        {
            rot += 360;
        }
        //transform this with respect to worldspace yaw?
        //rot -= MainManager.Instance.GetWorldspaceYaw();

        //Debug.Log("set " + rot);
        trueFacingRotation = rot;
    }

    public float GetFacingRotation()
    {
        return facingRotation;
    }

    public float ConvertVectorToRotation(Vector2 xz)    //vector -> trueFacingRotation such that the facing vector faces in that direction
    {
        return -Vector2.SignedAngle(Vector2.right, xz.x * Vector2.right + xz.y * Vector2.up);
    }
    public float ConvertVectorToRotation(Vector3 xz)    //vector -> trueFacingRotation such that the facing vector faces in that direction (only xz is checked)
    {
        return -Vector2.SignedAngle(Vector2.right, xz.x * Vector2.right + xz.z * Vector2.up);
    }

    public Vector3 FacingVector()
    {
        return Vector3.right * Mathf.Cos(trueFacingRotation * Mathf.PI / 180) + Vector3.back * Mathf.Sin(trueFacingRotation * Mathf.PI / 180);
    }
    public Vector3 FeetPosition()
    {
        return transform.position - (Vector3.down * (height / 2));
    }
    public RaycastHit DownRaycast(float maxDist, float upDelta = 0, int layerMask = PLAYER_LAYER_MASK, QueryTriggerInteraction q = QueryTriggerInteraction.Ignore)
    {
        RaycastHit output;
        Vector3 start = transform.position + Vector3.down * height / 2 + Vector3.up * upDelta;
        Physics.Raycast(start, Vector3.down, out output, maxDist, layerMask, q);

        if (output.collider != null)
        {
#pragma warning disable CS0162
            if (DRAW_DEBUG_RAYS)
            {
                Debug.DrawRay(start, output.point - start, Color.white, 0.5f, true);
            }
        }
        else
        {
            if (DRAW_DEBUG_RAYS)
            {
                Debug.DrawRay(start, Vector3.down * maxDist, Color.red, 0.5f, true);
            }
        }
        //Debug.Log((output.collider != null) + " " + start + " " + (output.point));
        return output;
    }
    public RaycastHit HullCast(float maxDist, Vector3 vector, int layerMask = PLAYER_LAYER_MASK, QueryTriggerInteraction q = QueryTriggerInteraction.Ignore)
    {
        return HullCast(maxDist, transform.position, vector, layerMask, q);
    }

    public RaycastHit HullCast(float maxDist, Vector3 start, Vector3 vector, int layerMask = PLAYER_LAYER_MASK, QueryTriggerInteraction q = QueryTriggerInteraction.Ignore)
    {
        RaycastHit output;
        //Vector3 start = transform.position; //+ Vector3.down * HEIGHT / 2;
        Vector3 delta = Vector3.up * ((height - width * 2) / 2);

        Vector3 dir = vector.normalized;

        Physics.CapsuleCast(start + delta, start - delta, width, dir, out output, maxDist, layerMask, q);

        if (output.collider != null)
        {
            if (DRAW_DEBUG_RAYS)
            {
                Debug.DrawRay(start + (height / 2) * Vector3.up, output.point - start, Color.white, 0.5f, true);
                Debug.DrawRay(start, output.point - start, Color.white, 0.5f, true);
                Debug.DrawRay(start - (height / 2) * Vector3.up, output.point - start, Color.white, 0.5f, true);
                Debug.DrawRay(output.point - start + (height / 2) * Vector3.up, -(height) * Vector3.up, Color.white, 0.5f, true);
            }
            //Gizmos.color = Color.white;
            //Gizmos.DrawSphere(output.point + delta, width);
            //Gizmos.DrawSphere(output.point - delta, width);
        }
        else
        {
            if (DRAW_DEBUG_RAYS)
            {
                Debug.DrawRay(start + (height / 2) * Vector3.up, dir * maxDist, Color.red, 0.5f, true);
                Debug.DrawRay(start, dir * maxDist, Color.red, 0.5f, true);
                Debug.DrawRay(start - (height / 2) * Vector3.up, dir * maxDist, Color.red, 0.5f, true);
                Debug.DrawRay(dir * maxDist - start + (height / 2) * Vector3.up, -(height) * Vector3.up, Color.red, 0.5f, true);
            }
            //Gizmos.color = Color.red;
            //Gizmos.DrawSphere(dir * maxDist + delta, width);
            //Gizmos.DrawSphere(dir * maxDist - delta, width);
        }
        //Debug.Log((output.collider != null) + " " + start + " " + (output.point));
        return output;
    }
    public Collider[] HullTest(Vector3 location, int layerMask = PLAYER_LAYER_MASK, QueryTriggerInteraction q = QueryTriggerInteraction.Ignore)
    {
        Collider[] output;
        Vector3 start = location; //transform.position; //+ Vector3.down * HEIGHT / 2;
        Vector3 delta = Vector3.up * ((height - width * 2) / 2);

        output = Physics.OverlapCapsule(start + delta, start - delta, width, layerMask, q);

        if (output != null)
        {
            //Gizmos.color = Color.white;
            //Gizmos.DrawSphere(start + delta, width);
            //Gizmos.DrawSphere(start - delta, width);
            if (DRAW_DEBUG_RAYS)
            {
                Debug.DrawRay(start + (height / 2) * Vector3.up, -(height) * Vector3.up, Color.white, 0.5f, true);
                Debug.DrawRay(start + (height / 2) * Vector3.up + Vector3.right * width, -(height) * Vector3.up, Color.white, 0.5f, true);
                Debug.DrawRay(start + (height / 2) * Vector3.up - Vector3.right * width, -(height) * Vector3.up, Color.white, 0.5f, true);
                Debug.DrawRay(start + (height / 2) * Vector3.up + Vector3.forward * width, -(height) * Vector3.up, Color.white, 0.5f, true);
                Debug.DrawRay(start + (height / 2) * Vector3.up - Vector3.forward * width, -(height) * Vector3.up, Color.white, 0.5f, true);

                Debug.DrawRay(start + (height / 2) * Vector3.up + Vector3.right * width, -Vector3.right * width * 2, Color.white, 0.5f, true);
                Debug.DrawRay(start - (height / 2) * Vector3.up + Vector3.right * width, -Vector3.right * width * 2, Color.white, 0.5f, true);
                Debug.DrawRay(start + (height / 2) * Vector3.up + Vector3.forward * width, -Vector3.forward * width * 2, Color.white, 0.5f, true);
                Debug.DrawRay(start - (height / 2) * Vector3.up + Vector3.forward * width, -Vector3.forward * width * 2, Color.white, 0.5f, true);
            }
        }
        else
        {
            //Gizmos.color = Color.red;
            //Gizmos.DrawSphere(start + delta, width);
            //Gizmos.DrawSphere(start - delta, width);
            if (DRAW_DEBUG_RAYS)
            {
                Debug.DrawRay(start + (height / 2) * Vector3.up, start - (height / 2) * Vector3.up, Color.red, 0.5f, true);
                Debug.DrawRay(start + (height / 2) * Vector3.up + Vector3.right * width, -(height) * Vector3.up, Color.red, 0.5f, true);
                Debug.DrawRay(start + (height / 2) * Vector3.up - Vector3.right * width, -(height) * Vector3.up, Color.red, 0.5f, true);
                Debug.DrawRay(start + (height / 2) * Vector3.up + Vector3.forward * width, -(height) * Vector3.up, Color.red, 0.5f, true);
                Debug.DrawRay(start + (height / 2) * Vector3.up - Vector3.forward * width, -(height) * Vector3.up, Color.red, 0.5f, true);

                Debug.DrawRay(start + (height / 2) * Vector3.up + Vector3.right * width, -Vector3.right * width * 2, Color.red, 0.5f, true);
                Debug.DrawRay(start - (height / 2) * Vector3.up + Vector3.right * width, -Vector3.right * width * 2, Color.red, 0.5f, true);
                Debug.DrawRay(start + (height / 2) * Vector3.up + Vector3.forward * width, -Vector3.forward * width * 2, Color.red, 0.5f, true);
                Debug.DrawRay(start - (height / 2) * Vector3.up + Vector3.forward * width, -Vector3.forward * width * 2, Color.red, 0.5f, true);
            }
        }
        //Debug.Log((output.collider != null) + " " + start + " " + (output.point));
        return output;
    }
}
