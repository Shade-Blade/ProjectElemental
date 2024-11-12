using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldEnemy_Chaser : WorldEnemyEntity
{
    public Vector3 startPosition;

    public float timeSinceInteractActive = 0;

    public float wanderRadius;
    public float idleDuration;
    public float wanderDuration;
    public float durationVariance;  //if all npcs have the exact same numbers then they would move in perfect sync (very unnatural) (Durations are +- 0.5 * this in seconds)

    [SerializeField]
    protected Vector2 wanderDir;
    [SerializeField]
    protected float idleTime;
    [SerializeField]
    protected float wanderTime;
    [SerializeField]
    protected Vector3 idlePos;

    //protected float ignoreTime;

    public void OnDrawGizmosSelected()
    {
        float thickness = 0.01f;

        Matrix4x4 oldMatrix = Gizmos.matrix;
        Gizmos.color = new Color(0.2f, 0.6f, 0.2f, 0.3f); //this is gray, could be anything
        if (startPosition != Vector3.zero)
        {
            Gizmos.matrix = Matrix4x4.TRS(startPosition, transform.rotation, new Vector3(1, thickness, 1));
            if (wed != null && !wed.inactive)
            {
                Gizmos.DrawSphere(Vector3.zero, wed.wanderRadius * 1.25f);
            }
            else
            {
                Gizmos.DrawSphere(Vector3.zero, wanderRadius * 1.25f);
            }
            Gizmos.color = new Color(0.2f, 0.2f, 0.2f, 0.5f); //this is gray, could be anything
            Gizmos.matrix = Matrix4x4.TRS(startPosition, transform.rotation, new Vector3(1, thickness, 1));
        }
        else
        {
            Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, new Vector3(1, thickness, 1));
            if (wed != null && !wed.inactive)
            {
                Gizmos.DrawSphere(Vector3.zero, wed.wanderRadius * 1.25f);
            }
            else
            {
                Gizmos.DrawSphere(Vector3.zero, wanderRadius * 1.25f);
            }
            Gizmos.color = new Color(0.2f, 0.2f, 0.2f, 0.5f); //this is gray, could be anything
            Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, new Vector3(1, thickness, 1));
        }

        if (wed != null && !wed.inactive)
        {
            Gizmos.DrawSphere(Vector3.zero, wed.wanderRadius);
        }
        else
        {
            Gizmos.DrawSphere(Vector3.zero, wanderRadius);
        }

        Gizmos.matrix = oldMatrix;
    }

    public override void SetWorldEntityData(WorldEntityData wed)
    {
        spriteID = wed.spriteID;
        speed = wed.speed;
        wanderDuration = wed.wanderDuration;
        idleDuration = wed.idleDuration;
        wanderRadius = wed.wanderRadius;
        durationVariance = wed.durationVariance;
        height = wed.height;
        width = wed.width;
        SetColliderInformation();
    }

    public override void Start()
    {
        base.Start();
        startPosition = transform.position;
        idlePos = startPosition;
    }

    public override void OnCollisionEnter(Collision collision)
    {
        if (touchEncounter)
        {
            WorldPlayer wp = collision.collider.GetComponent<WorldPlayer>();
            if (wp != null && !wp.NoTouchEncounter())
            {
                //if you are facing away the dot product will be negative
                //(the contact point points towards the enemy)
                if (Vector3.Dot(wp.FacingVector(), collision.contacts[0].normal) < -0.4f)
                {
                    firstStrikeTimer = 0;
                }
                else
                {
                    frameTimer = 0;
                }
                //firstStrikeTimer = 0;
                //MainManager.Instance.mapScript.StartBattle(this);
            }
        }

        ProcessCollision(collision);
    }

    public override void WorldUpdate()
    {
        Vector3 newVelocity = Vector3.zero;

        //wandering logic
        float distance = (startPosition - transform.position).magnitude;
        Vector2 diff = (MainManager.XZProject(startPosition - transform.position)).normalized;

        Vector2 diffPlayer = (MainManager.XZProject(WorldPlayer.Instance.transform.position - transform.position)).normalized;

        if (ignoreTime > 0)
        {
            ignoreTime -= Time.deltaTime;
        } else
        {
            ignoreTime = 0;
        }

        switch (state)
        {
            case ActionState.Idle:
                idleTime += Time.deltaTime;

                //try to stay at the idle pos using the stationary code
                //Non wandering npcs always try to go to their original position
                float distB = (1 / 10f);

                float speedMultB = (idlePos - transform.position).magnitude;
                if (speedMultB > 0)
                {
                    speedMultB = speedMultB * 1 / distB;
                    if (speedMultB > 1)
                    {
                        speedMultB = 1;
                    }
                }

                float keepyB = rb.velocity.y;
                newVelocity = (idlePos - transform.position).normalized * speedMultB * 3; //GetSpeed();
                newVelocity.y = keepyB;
                //Debug.Log("Components: " + ((idlePos - transform.position).normalized) + " " + speedMultB + " " + GetSpeed());
                //Debug.Log("Move with " + newVelocity + " towards home at " + idlePos + " vs " + transform.position + " start = " + startPosition);

                //

                if (idleTime > idleDuration)
                {
                    ChangeState(ActionState.Wandering);
                    wanderTime = durationVariance * (0.5f - Random.Range(0f, 1f));
                    idleTime = 0;

                    //decide where to wander
                    float wanderAngle = Random.Range(0f, Mathf.PI * 2);
                    wanderDir = Vector2.up * Mathf.Sin(wanderAngle) + Vector2.right * Mathf.Cos(wanderAngle);
                    movementRotationDisabled = false;

                    if (distance > wanderRadius && Vector3.Dot(diff, wanderDir) < 0)
                    {
                        wanderDir = -wanderDir; //this necessarily gives a direction that goes back towards the center
                        //skew it a little bit to avoid the npc from hitting the radius again

                        wanderDir += diff * 0.5f;
                        wanderDir = wanderDir.normalized;
                    }
                }
                if (distance < wanderRadius && AggroCheck(wanderRadius + 4) && ignoreTime <= 0 && !WorldPlayer.Instance.NoTouchEncounter())
                {
                    ChangeState(ActionState.Chasing);
                    wanderTime = 0;
                    movementRotationDisabled = false;
                    Particle_Alert();
                }
                break;
            case ActionState.Wandering:
                wanderTime += Time.deltaTime;

                //move
                newVelocity = speed * (wanderDir.x * Vector3.right + wanderDir.y * Vector3.forward) + rb.velocity.y * Vector3.up;

                //if you escape the bounds, stop wandering early
                //but if you are walking back towards the center you can keep wandering (so that it corrects itself)
                //but too far = go idle (use the idle logic thing to fix position?)
                if (distance > wanderRadius + 3 || (distance > wanderRadius && Vector3.Dot(diff, wanderDir) < 0))
                {
                    wanderTime = wanderDuration + 1;
                }

                if (wanderTime > wanderDuration)
                {
                    ChangeState(ActionState.Idle);
                    wanderTime = 0;
                    idleTime = durationVariance * (0.5f - Random.Range(0f, 1f));

                    idlePos = transform.position;

                    //constrain idlePos to where it should be
                    if ((idlePos - startPosition).magnitude > wanderRadius)
                    {
                        idlePos = (idlePos - startPosition).normalized * wanderRadius + startPosition;
                    }

                    movementRotationDisabled = true;
                }
                if (distance < wanderRadius && AggroCheck(wanderRadius + 4) && ignoreTime <= 0 && !WorldPlayer.Instance.NoTouchEncounter())
                {
                    ChangeState(ActionState.Chasing);
                    wanderTime = 0;
                    movementRotationDisabled = false;
                    Particle_Alert();
                }
                break;
            case ActionState.Chasing:
                //Same as wander logic except it goes towards you
                //move
                newVelocity = speed * (diffPlayer.x * Vector3.right + diffPlayer.y * Vector3.forward) + rb.velocity.y * Vector3.up;

                //if you escape the bounds, stop wandering early
                //but if you are walking back towards the center you can keep wandering (so that it corrects itself)
                //but too far = go idle (use the idle logic thing to fix position?)
                if (distance > wanderRadius + 3 || (distance > (wanderRadius * 1.25f) && Vector3.Dot(diff, diffPlayer) < 0))
                {
                    wanderTime = wanderDuration + 1;
                }

                if (wanderTime > wanderDuration)
                {
                    Particle_GiveUp();
                    ignoreTime = 0.4f;
                    ChangeState(ActionState.Idle);
                    wanderTime = 0;
                    idleTime = idleDuration + 1;    //force an immediate wandering setup

                    idlePos = transform.position;

                    //constrain idlePos to where it should be
                    if ((idlePos - startPosition).magnitude > wanderRadius)
                    {
                        idlePos = (idlePos - startPosition).normalized * wanderRadius + startPosition;
                    }

                    movementRotationDisabled = true;
                } else
                {
                    //note: notouchencounter and concealed have a lot of overlap (but hiding will make you concealed but still touch encounterable)
                    if (!RaycastCheck() || WorldPlayer.Instance.Concealed() || ignoreTime > 0 || WorldPlayer.Instance.NoTouchEncounter())
                    {
                        Particle_Miss();
                        ignoreTime = 0.4f;
                        ChangeState(ActionState.Idle);
                        wanderTime = 0;

                        if (distance < wanderRadius)
                        {
                            idleTime = 0;
                        }
                        else
                        {
                            idleTime = idleDuration + 1;    //force an immediate wandering setup
                        }

                        idlePos = transform.position;

                        //constrain idlePos to where it should be
                        if ((idlePos - startPosition).magnitude > wanderRadius)
                        {
                            idlePos = (idlePos - startPosition).normalized * wanderRadius + startPosition;
                        }

                        movementRotationDisabled = true;
                    }
                }
                break;
        }

        intendedMovement = newVelocity;
        rb.velocity = newVelocity;
        //Debug.Log(rb.isKinematic);

        //Failsafe
        if (transform.position.y < WorldPlayer.WARP_BARRIER_Y_COORD)
        {
            transform.position = startPosition;
            rb.velocity = Vector3.up;
        }

        BattleSetupUpdate();
    }
}
