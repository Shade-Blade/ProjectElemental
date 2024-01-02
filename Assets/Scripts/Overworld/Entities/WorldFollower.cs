using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows;
using static BattleHelper;

//Entities that follow the player around (you may have multiple, but probably a max of 3 or 4 need to be supported)
//Note that certain actions will have to do stuff to these guys
public class WorldFollower : WorldEntity
{
    public int followerIndex = -1;

    //public MainManager.PlayerCharacter currentCharacter;
    public MainManager.SpriteID currentCharacter;

    public Transform followTarget;

    public float timeSinceLastJump;

    public List<float> jumpBufferTimes;
    //public float jumpBufferTime;
    public bool jumpBuffer;

    public float stuckTime;

    public float farTime;

    public float timeWalking;

    public bool zeroDistFollow; //reset on grounded

    public Vector2 scriptedMovement;


    public float timeSinceStepUp;

    //public bool useAllFriction = false;


    public const float JUMP_COOLDOWN = 0.8f;
    public const float STUCK_THRESHOLD = 0.1f;  //try to jump over obstacles
    public const float FAR_THRESHOLD = 1.5f;  //too far away from the follow target so warp

    public const float STUCK_MAX_TIME = 2.5f; //if you stay too far for too long you teleport
    public const float MAX_DISTANCE = 4f;   //(or if you get too far away)

    public const float TIME_WALKING_THRESHOLD = 0.2f;

    public const float STEP_UP_MINIMUM = 0.1f;
    public const float STEP_UP_BONUS = 0.06f;
    public const float STEP_UP_MAX_HEIGHT = 0.31f;
    public const float STEP_DOWN_MAX_HEIGHT = 0.31f;
    private const float STEP_UP_DOWN_DELAY = 0.05f; //normally the code for step up and step down fight each other

    //public const float JUMP_NUDGE = 0.05f;

    //public bool isGrounded = false;

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

        //Fix bug with the shadows of newly spawned in entities
        DropShadowUpdate(true);
    }

    void Start()
    {
        for (int i = 0; i < WorldPlayer.Instance.followers.Count; i++)
        {
            if (this == WorldPlayer.Instance.followers[i])
            {
                followerIndex = i;
                if (i != 0)
                {
                    followTarget = WorldPlayer.Instance.followers[i - 1].transform;
                } else
                {
                    followTarget = WorldPlayer.Instance.transform;
                }
                break;
            }
        }

        if (subObject == null)
        {
            spriteID = ((MainManager.SpriteID)currentCharacter).ToString();
            MakeAnimationController();
        }

        isGrounded = true;
    }

    public override void WorldUpdate()
    {
        if (scriptedMovement != Vector2.zero)
        {
            ScriptedMoveUpdate();
            scriptedMovement = Vector2.zero;
        } else
        {
            FollowerMoveUpdate();
        }
    }

    public void ScriptedMoveUpdate()
    {
        //try to move for now
        Vector3 newVelocity = rb.velocity;
        Vector2 dir = scriptedMovement.normalized;

        newVelocity = dir.x * GetSpeed() * Vector3.right + dir.y * GetSpeed() * Vector3.forward + rb.velocity.y * Vector3.up;

        intendedMovement = newVelocity;
        rb.velocity = newVelocity;

        if (timeWalking > TIME_WALKING_THRESHOLD && (newVelocity.magnitude > STUCK_THRESHOLD && rb.velocity.magnitude < STUCK_THRESHOLD))
        {
            stuckTime += Time.deltaTime;

            if (timeSinceLastJump > JUMP_COOLDOWN && IsGrounded())
            {
                //try to nudge
                /*
                Vector3 testdir = Vector3.zero;
                testdir.x = newVelocity.x;
                testdir.z = newVelocity.z;
                RaycastHit testHit = HullCast(JUMP_NUDGE, -testdir);

                if (testHit.collider == null)
                {
                    transform.position += -testdir.normalized * JUMP_NUDGE;
                    Debug.Log("Nudge");
                }
                */

                jumpBufferTimes = new List<float>();
                //jumpBufferTime = 0;
                jumpBuffer = false;
                Jump(WorldPlayer.Instance.GetJumpImpulse());

                //Debug.Log(rb.velocity);

                //recalculate newVelocity?
                /*
                if (pspeed > speed)
                {
                    newVelocity = dir.x * pspeed * Vector3.right + dir.y * pspeed * Vector3.forward + rb.velocity.y * Vector3.up;
                }
                else
                {
                    newVelocity = dir.x * speed * Vector3.right + dir.y * speed * Vector3.forward + rb.velocity.y * Vector3.up;
                }
                */
            }
        }
        else
        {
            stuckTime = 0;
        }

        if (jumpBufferTimes != null)
        {
            jumpBufferTimes = null;
        }
        if (jumpBuffer && IsGrounded())
        {
            jumpBuffer = false;
        }

        if ((MainManager.XZProject(intendedMovement).magnitude == 0) ^ useAllFriction)
        {
            if (MainManager.XZProject(intendedMovement).magnitude == 0)
            {
                capsuleCollider.material = MainManager.Instance.allFrictionMaterial;
                useAllFriction = true;
            }
            else
            {
                capsuleCollider.material = MainManager.Instance.noFrictionMaterial;
                useAllFriction = false;
            }
        }

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
            RaycastHit checker;
            //Physics.Raycast(nearGroundMin + Vector3.up * extraHeight, Vector3.down, out checker, extraHeight + lowerHeight, PLAYER_LAYER_MASK);
            //Debug.DrawRay(nearGroundMin + Vector3.up * extraHeight, Vector3.down, Color.red, extraHeight);
            checker = HullCast(extraHeight + lowerHeight, nearGroundMin + Vector3.up * (height / 2) + Vector3.up * extraHeight, Vector3.down, PLAYER_LAYER_MASK);

            float delta = 0;

            float dotB = Vector3.Dot(MainManager.XZProjectReverse(dir), floorNormal);

            if (checker.collider != null)
            {
                delta = checker.point.y - nearGroundMin.y;
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
                }
            }
            //Debug.Log((checker.collider != null) + " " + delta + " " + dotB);
        }

        lastIntendedMovement = intendedMovement;
    }

    public void FollowerMoveUpdate()
    {
        //try to move for now
        Vector3 newVelocity = rb.velocity;
        Vector2 dir = Vector2.zero;

        Vector2 diff = MainManager.XZProject(followTarget.position - transform.position);
        float dist = diff.magnitude;

        if (zeroDistFollow && groundedTime > 0.1f)
        {
            zeroDistFollow = false;
        }

        if (zeroDistFollow)
        {
            if (dist > 0.025f)
            {
                dir = diff.normalized;
                timeWalking += Time.deltaTime;
            }
            else
            {
                timeWalking = 0;
            }
        }
        else
        {
            if (dist > WorldPlayer.Instance.followerDistance * 1.25f)
            {
                dir = diff.normalized;
                timeWalking += Time.deltaTime;
            }
            else if (dist > WorldPlayer.Instance.followerDistance)
            {
                dir = diff.normalized * 0.75f;
                timeWalking += Time.deltaTime;
            }
            else
            {
                timeWalking = 0;
            }
        }


        float pspeed = MainManager.XZProject(WorldPlayer.Instance.rb.velocity).magnitude;

        //if you're somehow going too fast, speed up the followers as well
        if (pspeed > GetSpeed())
        {
            float a = (transform.position - followTarget.position).magnitude;
            if (a > FAR_THRESHOLD)
            {
                //slightly increase pspeed in this case so that the followers catch up
                a -= FAR_THRESHOLD;
                a *= 4;
                if (a > 1)
                {
                    a = 1;
                }

                a *= 0.25f;
            } else
            {
                a = 0;
            }
            a += 1;
            newVelocity = dir.x * a * pspeed * Vector3.right + dir.y * a * pspeed * Vector3.forward + rb.velocity.y * Vector3.up;
        }
        else
        {
            newVelocity = dir.x * GetSpeed() * Vector3.right + dir.y * GetSpeed() * Vector3.forward + rb.velocity.y * Vector3.up;
        }

        intendedMovement = newVelocity;
        rb.velocity = newVelocity;

        //Debug.Log("also " + prevAttachedVel);

        //Debug.Log(rb.velocity + " " + lastFrameVel);

        if (timeWalking > TIME_WALKING_THRESHOLD && (newVelocity.magnitude > STUCK_THRESHOLD && rb.velocity.magnitude < STUCK_THRESHOLD))
        {
            stuckTime += Time.deltaTime;

            if (timeSinceLastJump > JUMP_COOLDOWN && IsGrounded())
            {
                //try to nudge
                /*
                Vector3 testdir = Vector3.zero;
                testdir.x = newVelocity.x;
                testdir.z = newVelocity.z;
                RaycastHit testHit = HullCast(JUMP_NUDGE, -testdir);

                if (testHit.collider == null)
                {
                    transform.position += -testdir.normalized * JUMP_NUDGE;
                    Debug.Log("Nudge");
                }
                */

                jumpBufferTimes = new List<float>();
                //jumpBufferTime = 0;
                jumpBuffer = false;
                Jump(WorldPlayer.Instance.GetJumpImpulse());

                //Debug.Log(rb.velocity);

                //recalculate newVelocity?
                /*
                if (pspeed > speed)
                {
                    newVelocity = dir.x * pspeed * Vector3.right + dir.y * pspeed * Vector3.forward + rb.velocity.y * Vector3.up;
                }
                else
                {
                    newVelocity = dir.x * speed * Vector3.right + dir.y * speed * Vector3.forward + rb.velocity.y * Vector3.up;
                }
                */
            }
        }
        else
        {
            stuckTime = 0;
        }

        if ((transform.position - followTarget.position).magnitude > FAR_THRESHOLD)
        {
            farTime += Time.deltaTime;
        }
        else
        {
            farTime = 0;
        }

        if (stuckTime > STUCK_MAX_TIME && (transform.position - followTarget.position).magnitude > MAX_DISTANCE)
        {
            SelfWarp();
        }

        if (farTime > STUCK_MAX_TIME)
        {
            SelfWarp();
        }

        if (jumpBufferTimes != null)
        {
            for (int i = 0; i < jumpBufferTimes.Count; i++)
            {
                jumpBufferTimes[i] -= Time.deltaTime;
                if (jumpBufferTimes[i] < 0)
                {
                    jumpBuffer = true;
                    jumpBufferTimes.RemoveAt(i);
                    i--;
                    continue;
                }
            }
        }
        /*
        if (jumpBufferTime > 0)
        {
            jumpBufferTime -= Time.deltaTime;
            if (jumpBufferTime < 0)
            {
                jumpBufferTime = 0;
                jumpBuffer = true;
            }
        }
        */
        if (jumpBuffer && IsGrounded())
        {
            jumpBuffer = false;
            Jump(WorldPlayer.Instance.GetJumpImpulse());
        }

        if ((MainManager.XZProject(intendedMovement).magnitude == 0) ^ useAllFriction)
        {
            if (MainManager.XZProject(intendedMovement).magnitude == 0)
            {
                capsuleCollider.material = MainManager.Instance.allFrictionMaterial;
                useAllFriction = true;
            }
            else
            {
                capsuleCollider.material = MainManager.Instance.noFrictionMaterial;
                useAllFriction = false;
            }
        }

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
            RaycastHit checker;
            //Physics.Raycast(nearGroundMin + Vector3.up * extraHeight, Vector3.down, out checker, extraHeight + lowerHeight, PLAYER_LAYER_MASK);
            //Debug.DrawRay(nearGroundMin + Vector3.up * extraHeight, Vector3.down, Color.red, extraHeight);
            checker = HullCast(extraHeight + lowerHeight, nearGroundMin + Vector3.up * (height / 2) + Vector3.up * extraHeight, Vector3.down, PLAYER_LAYER_MASK);

            float delta = 0;

            float dotB = Vector3.Dot(MainManager.XZProjectReverse(dir), floorNormal);

            if (checker.collider != null)
            {
                delta = checker.point.y - nearGroundMin.y;
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
                }
            }
            //Debug.Log((checker.collider != null) + " " + delta + " " + dotB);
        }

        lastIntendedMovement = intendedMovement;
    }

    //sets scripted input to the direction towards the point
    public void ScriptedMoveToFrame(Vector3 moveto)
    {
        scriptedMovement = MainManager.XZProject(moveto - transform.position);
    }

    //note: as soon as this ends the normal move logic takes over again
    public IEnumerator ScriptedMoveTo(Vector3 moveto, float maxtime = 5f)
    {
        float lifetime = 0;
        while (MainManager.XZProject(transform.position - moveto).magnitude > 0.1f)
        {
            lifetime += Time.deltaTime;
            if (lifetime > maxtime)
            {
                Debug.LogWarning("ScriptedMoveTo follower failsafe");
                transform.position = moveto;
                break;
            }

            scriptedMovement = MainManager.XZProject(moveto - transform.position).normalized;
            yield return null;
        }
    }

    public void TrySnapToFloor()
    {
        //snap to floor?
        RaycastHit hit = DownRaycast(0.3f);
        if (!AntiGravity() && hit.collider != null)
        {
            if (hit.normal.y > MIN_GROUND_NORMAL && LegalGround(hit.collider))
            {
                floorNormal = hit.normal;
                lastGroundedHeight = hit.point.y;
                lastHighestHeight = lastGroundedHeight;
                isGrounded = true;
                attached = hit.rigidbody;

                float height = 0.75f;

                transform.position = hit.point + Vector3.up * height / 2;

                float speed = rb.velocity.magnitude;
                float dot = Vector3.Dot(rb.velocity, hit.normal);
                if (dot > 0)
                {
                    rb.velocity = (rb.velocity - hit.normal * dot).normalized * speed;
                }
            }
        }
    }

    public override void SpriteRotationUpdate()
    {
        if (rb == null || subObject == null)
        {
            return;
        }

        //Debug.Log("monitor F " + trueFacingRotation);

        bool pastShowBack = showBack;

        Vector3 usedMovement = useIntended ? intendedMovement : rb.velocity;

        if ((usedMovement.x != 0 || usedMovement.z != 0) || pastTrueFacingRotation != trueFacingRotation)
        {
            if (!movementRotationDisabled && (usedMovement.x != 0 || usedMovement.z != 0))
            {
                trueFacingRotation = -Vector2.SignedAngle(Vector2.right, usedMovement.x * Vector2.right + usedMovement.z * Vector2.up);
                //transform this with respect to worldspace yaw
                trueFacingRotation -= MainManager.Instance.GetWorldspaceYaw();
                //Debug.Log("reset F " + trueFacingRotation);
            }

            if (trueFacingRotation < 0)
            {
                trueFacingRotation += 360;
            }

            while (trueFacingRotation < 0)
            {
                trueFacingRotation += 360;
            }
            while (trueFacingRotation > 360)
            {
                trueFacingRotation -= 360;
            }

            //Debug.Log(hiddenFacingRotation);

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
        if (followerIndex == 0 && MainManager.Instance.playerData.party.Count > 1)
        {
            if (WorldPlayer.Instance.switchRotation != 0)
            {
                correctedRotation = WorldPlayer.Instance.switchRotation;
            }
            else
            {
                correctedRotation = facingRotation;
            }
        }
        else
        {
            correctedRotation = facingRotation;
        }

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

        if (ac != null)
        {
            if (showBack)
            {
                ac.SendAnimationData("showback");
            }
            else
            {
                ac.SendAnimationData("unshowback");
            }

            if (facingRotation > 90 || facingRotation < -90)
            {
                ac.SendAnimationData("xflip");
            }
            else
            {
                ac.SendAnimationData("xunflip");
            }
        }
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
            }
            else
            {
                rb.velocity = rb.velocity + Vector3.up * 50 * Time.fixedDeltaTime * defaultJumpLift;
            }
        }

        timeSinceLastJump += Time.fixedDeltaTime;

        if (IsGrounded())
        {
            groundedTime += Time.deltaTime;
            airTime = 0;
            airFrames = 0;
        }
        else
        {
            groundedTime = 0;
            airTime += Time.deltaTime;
            airFrames++;
        }

        //note: followers don't have the big state system worldplayer has
        //change this later to include all special non-grounded states
        if (airFrames == 1 && (timeSinceLastJump > 0.1f) && timeSinceStepUp > STEP_UP_DOWN_DELAY)
        {
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
        }
        else
        {
            if (IsGrounded() && groundedTime > PLATFORM_VELOCITY_RESET_TIME)
            {
                momentumVel = Vector3.zero;
            }
        }

        if (conveyerVector != Vector3.zero && timeSinceLastJump > 0.1f && airTime > 0.1f)
        {
            applyDefaultJumpLift = false;
        }

        //conveyers that lift you off the ground need to properly set that up to avoid having the ground sticky stuff fighting back against it
        if (AntiGravity() && IsGrounded())
        {
            isGrounded = false;
            semisolidFloorActive = false;
            semisolidSnapBelow = false;
            airFrames = 2;  //the ground snap checks for this = 1
        }

        if (movementDamping != 0 && (timeSinceLastJump > 0.1f || timeSinceLastJump == 0))
        {
            rb.velocity = MainManager.EasingExponentialTime(rb.velocity, conveyerVector - conveyerVector.y * Vector3.up, 1 / movementDamping);
        }

        rb.velocity += conveyerVector;

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

        lastFrameVel = rb.velocity;
        timeSinceStepUp += Time.fixedDeltaTime;
        isGrounded = false;
        touchingNonGround = false;
        floorNormal = Vector3.zero;
        prevAttached = attached;
        attached = null;
        //prevAttachedVel = attachedVel;
        timeSinceLaunch += Time.fixedDeltaTime;
        attachedVel = Vector3.zero;
        //attachedPos = Vector3.zero;
        //attachedLocalPos = Vector3.zero;
    }

    public override void Jump(float jumpImpulse, float jumpLift = 0)
    {
        defaultJumpLift = jumpLift;
        if (jumpLift != 0)
        {
            applyDefaultJumpLift = true;
        }
        timeSinceLastJump = 0;
        isGrounded = false;
        semisolidFloorActive = false;
        semisolidSnapBelow = false;

        rb.velocity = rb.velocity - rb.velocity.y * Vector3.up + Vector3.up * jumpImpulse;

        if (attachedVel != Vector3.zero)
        {
            //Debug.Log("delta " + attachedVel);
            rb.velocity += attachedVel;
        }
        else
        {
            //Debug.Log("delta " + prevAttachedVel);
            rb.velocity += momentumVel;
        }

        /*
        if (!isGrounded)
        {
            rb.velocity = rb.velocity - rb.velocity.y * Vector3.up + Vector3.up * jumpImpulse;
        } else
        {
            //calculate with floor normal (but this is harder)

            Vector3 project(Vector3 a, Vector3 b)
            {
                return a - b * Vector3.Dot(a, b);
            }

            rb.velocity = rb.velocity - project(rb.velocity, floorNormal) + floorNormal * jumpImpulse;
        }
        */

        //Debug.Log(rb.velocity);
    }

    public void BufferJump(float bufferTime)
    {
        if (jumpBufferTimes == null)
        {
            jumpBufferTimes = new List<float>();
        }
        jumpBufferTimes.Add(bufferTime);
        //jumpBufferTime = bufferTime;
    }
    
    private void SelfWarp()
    {
        stuckTime = 0;
        farTime = 0;
        transform.position = followTarget.position;
        WorldPlayer.Instance.WarpFollowing(followerIndex);
    }

    public void WarpSetState(bool p_grounded, Vector3 p_floorNormal)
    {
        isGrounded = p_grounded;
        floorNormal = p_floorNormal;

        stuckTime = 0;
        farTime = 0;
        transform.position = followTarget.position;
    }
    public void Warp()
    {
        stuckTime = 0;
        farTime = 0;
        transform.position = followTarget.position;
    }

    public void Warp(Vector3 warpPoint)
    {
        stuckTime = 0;
        farTime = 0;
        transform.position = warpPoint;
    }

    public void SetFollowUntilGrounded()
    {
        zeroDistFollow = true;
    }

    public void ResetJumpBuffer()
    {
        jumpBufferTimes = new List<float>();
        //jumpBufferTime = 0;
        jumpBuffer = false;
    }

    public void SetIdentity(MainManager.SpriteID spriteID)
    {
        currentCharacter = spriteID;

        Destroy(subObject);
        this.spriteID = spriteID.ToString();
        MakeAnimationController();
    }

    public void Aetherize()
    {
        if (ac != null)
        {
            ac.SendAnimationData("aetherize");
        }
    }
    public void MatReset()
    {
        if (ac != null)
        {
            ac.SendAnimationData("matreset");
        }
    }
}
