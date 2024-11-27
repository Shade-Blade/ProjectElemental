using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.WSA;

//Used for npcs, enemies
public class WorldNPCEntity : WorldEntity, ITattleable, IStompTrigger, IInteractable
{
    public EncounterData encounter;

    public InteractTrigger interacter;
    public SphereCollider interactSphere;

    public GameObject interactIndicator;    //Cyan colored (Green is used for quest indication because the quest menu is green)
    public bool indicatorActive;    //(note: only the interact indicator, not quest indicator)

    public bool questIndicator;  //Quest indicator active instead of normal one

    public Vector3 startPosition;

    public float timeSinceInteractActive = 0;

    public bool wandering;  //wandering npcs walk around randomly
    public float wanderRadius;
    public float idleDuration;
    public float wanderDuration;
    public float durationVariance;  //if all npcs have the exact same numbers then they would move in perfect sync (very unnatural) (Durations are +- 0.5 * this in seconds)

    public string tattle;

    [SerializeField]
    protected bool currentlyWandering;
    [SerializeField]
    protected Vector2 wanderDir;
    [SerializeField]
    protected float idleTime;
    [SerializeField]
    protected float wanderTime;
    [SerializeField]
    protected Vector3 idlePos;
    [SerializeField]
    protected float stompLaunch;  //(upwards only) Note: negative = no launch
    [SerializeField]
    protected bool stompBoostable; //hold A for boost?

    public virtual void OnDrawGizmosSelected()
    {
        if (wandering)
        {
            float thickness = 0.01f;

            Matrix4x4 oldMatrix = Gizmos.matrix;
            Gizmos.color = new Color(0.2f, 0.2f, 0.2f, 0.5f); //this is gray, could be anything
            if (startPosition != Vector3.zero)
            {
                Gizmos.matrix = Matrix4x4.TRS(startPosition, transform.rotation, new Vector3(1, thickness, 1));
            }
            else
            {
                Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, new Vector3(1, thickness, 1));
            }

            if (wed != null && !wed.inactive)
            {
                Gizmos.DrawSphere(Vector3.zero, wed.wanderRadius);
            } else
            {
                Gizmos.DrawSphere(Vector3.zero, wanderRadius);
            }

            Gizmos.matrix = oldMatrix;
        }
    }

    public override void SetWorldEntityData(WorldEntityData wed)
    {
        base.SetWorldEntityData(wed);
        wanderDuration = wed.wanderDuration;
        idleDuration = wed.idleDuration;
        wanderRadius = wed.wanderRadius;
        durationVariance = wed.durationVariance;

        wandering = wed.wandering;
        wanderDuration = wed.wanderDuration;
        idleDuration = wed.idleDuration;
        wanderRadius = wed.wanderRadius;
        durationVariance = wed.durationVariance;

        stompLaunch = wed.stompLaunch;
        stompBoostable = wed.stompBoostable;
        tattle = wed.tattleString;

        if (interactRadius != 0 && interactSphere != null)
        {
            interactSphere.radius = interactRadius;
        }
    }

    public virtual void Start()
    {
        startPosition = transform.position;
        idlePos = startPosition;
        //forceKinematic = true;

        if (!wandering)
        {
            movementRotationDisabled = true;
        }
    }

    //current system: use text file shorthands to reference the correct line
    //(or: make your own file but it has to include a \n)
    //(or: just make a text line)
    public virtual string GetTattle()
    {
        //return "<prompt,Test text,1,Long long long text,2,A,3,3>";
        //return "<tail,w>Wilex text<next><tail,l>Luna text";
        string newtattle = FormattedString.ReplaceTextFileShorthand(tattle);
        return newtattle;
    }

    //debug stuff
    public override void SpriteAnimationUpdate()
    {
        if (ac == null)
        {
            return;
        }

        if (SpeakingAnimActive())
        {
            return;
        }

        if (mapScript.halted || mapScript.battleHalt)
        {
            return;
        }

        string animName = "";

        if (IsGrounded())
        {
            if (rb != null && rb.velocity.magnitude > 0.25f)
            {
                animName = "walk";
            }
            else
            {
                animName = "idle";
            }
        }
        else
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

    public override void WorldUpdate()
    {
        if (interacter.GetActive())
        {
            if (interactIndicator != null && !indicatorActive)
            {
                Destroy(interactIndicator);
            }

            if (interactIndicator == null)
            {
                //make it
                interactIndicator = Instantiate(Resources.Load<GameObject>("Overworld/Other/InteractIndicator"), gameObject.transform);
                if (height != 0)
                {
                    interactIndicator.transform.localPosition = Vector3.up * (0.325f + height/2) + Vector3.back * 0.01f;
                }
                else
                {
                    interactIndicator.transform.localPosition = Vector3.up * 0.7f + Vector3.back * 0.01f;
                }                
                indicatorActive = true;
            }

            trueFacingRotation = ConvertVectorToRotation(WorldPlayer.Instance.transform.position - transform.position) - MainManager.Instance.GetWorldspaceYaw();
            timeSinceInteractActive += Time.deltaTime;

            if (wandering)
            {
                currentlyWandering = false;
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
        } else
        {
            timeSinceInteractActive = 0;

            if (interactIndicator != null && indicatorActive)
            {
                Destroy(interactIndicator);
            }
            indicatorActive = false;

            if (interactIndicator == null && questIndicator)
            {
                //make it
                interactIndicator = Instantiate(Resources.Load<GameObject>("Overworld/Other/QuestIndicator"), gameObject.transform);
                if (height != 0)
                {
                    interactIndicator.transform.localPosition = Vector3.up * (0.325f + height/2) + Vector3.back * 0.01f;
                }
                else
                {
                    interactIndicator.transform.localPosition = Vector3.up * 0.7f + Vector3.back * 0.01f;
                }
            }
        }

        if (interactIndicator != null)
        {
            interactIndicator.transform.localEulerAngles = Vector3.up * MainManager.Instance.GetWorldspaceYaw();
        }

        Vector3 newVelocity = Vector3.zero;

        if (!wandering)
        {
            //Non wandering npcs always try to go to their original position
            float dist = (1 / 10f);

            float speedMult = (startPosition - transform.position).magnitude;
            if (speedMult > 0)
            {
                speedMult = speedMult * 1 / dist;
                if (speedMult > 1)
                {
                    speedMult = 1;
                }
            }

            float keepy = rb.velocity.y;
            newVelocity = (startPosition - transform.position).normalized * speedMult * 3; //GetSpeed()
            newVelocity.y = keepy;
        } else
        {
            //wandering logic
            float distance = (startPosition - transform.position).magnitude;
            Vector2 diff = (MainManager.XZProject(startPosition - transform.position)).normalized;

            if (currentlyWandering)
            {
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
                    currentlyWandering = false;
                    wanderTime = 0;
                    idleTime = durationVariance * (0.5f - Random.Range(0f,1f));

                    idlePos = transform.position;

                    //constrain idlePos to where it should be
                    if ((idlePos - startPosition).magnitude > wanderRadius)
                    {
                        idlePos = (idlePos - startPosition).normalized * wanderRadius + startPosition;
                    }

                    movementRotationDisabled = true;
                }
            } else
            {
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
                    currentlyWandering = true;
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
            }
        }

        intendedMovement = newVelocity;
        rb.velocity = newVelocity;
        //Debug.Log(rb.isKinematic);
    }

    public virtual void Interact()
    {
        //Debug.Log("Interact");
        StartCoroutine(MainManager.Instance.ExecuteCutscene(InteractCutscene()));
    }

    public virtual IEnumerator InteractCutscene()
    {
        yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(MainManager.Instance.testTextFile, 8, this));

        string menuResult = MainManager.Instance.lastTextboxMenuResult;

        //FormattedString.ParseArg(menuResult, "arg1");

        //string var = FormattedString.ParseArg(menuResult, "arg");

        string[] vars = new string[] { menuResult };

        //Debug.Log("B");

        yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(MainManager.Instance.testTextFile, 6, this, vars));
    }

    public override string RequestTextData(string request)
    {
        //return "StellarAether,SupremeDinner,SupremeDessert,Carrot,BerrySyrup,FlowerSyrup,HoneySyrup,RoyalSyrup,MiracleSyrup,StellarSyrup";
        //return "BerrySyrup|5 <coin>|FlowerSyrup|10 <coin>|HoneySyrup|25 <coin>|RoyalSyrup|50 <coin>|MiracleSyrup|100 <coin>|StellarSyrup|100 <coin>";
        //return "BerrySyrup|5 <coin>|FlowerSyrup|10 <coin>|HoneySyrup|25 <coin>|RoyalSyrup|50 <coin>|MiracleSyrup|100 <coin>|StellarSyrup|&l- <itemsprite,StellarSyrup> + <itemsprite,StellarSyrup><size,0>a";

        if (request.Equals("0"))
        {
            //name, right text, canUse, desc, max level, backgroundColor (invalid = no background)
            //(End of list = level descriptor)
            return "BerrySyrup|5 <coin>|false|desc <commonsprite,hp> 1|5|#ffffff|FlowerSyrup|10 <coin>|true|desc 2|5|X|HoneySyrup|25 <coin>|false|desc 3|5|X|RoyalSyrup|50 <coin>|true|desc 4|5|X|MiracleSyrup|100 <coin>|true|desc 5|5|X|StellarSyrup|<larrow> <itemsprite,StellarSyrup> + <itemsprite,StellarSyrup><size,0>a|true|desc 6|1|X|x |Menu descriptor";
        } else
        {
            string index = FormattedString.ParseArg(request, "arg2");
            int intIndex = -1;
            int.TryParse(index, out intIndex);

            index = FormattedString.ParseArg(request, "arg1");
            int intLevel = -1;
            int.TryParse(index, out intLevel);

            string[] names = new string[] { "BerrySyrup", "FlowerSyrup", "HoneySyrup", "RoyalSyrup", "MiracleSyrup", "StellarSyrup" };
            int[] list = new int[]{ 5, 10, 25, 50, 100, 0 };

            if (intIndex > -1)
            {
                string output = names[intIndex] + "|" + (list[intIndex] * intLevel) + " <coin>" + "|" + (intLevel > 2);
                if (intIndex == 5)
                {
                    return "nop";
                } else
                {
                    return output;
                }
            } else
            {
                return "nop";
            }
        }

    }

    public override void SendTextData(string data)
    {
        Debug.Log("Received data: " + data);
    }


    public virtual void Stomp(WorldPlayer.StompType stompType)
    {
        bool stompBoost = stompBoostable && InputManager.GetButton(InputManager.Button.A) && WorldPlayer.Instance.CanMove();
        if (stompLaunch > 0)
        {
            if (stompBoost)
            {
                WorldPlayer.Instance.Launch((1 + stompLaunch) * Vector3.up, 0);
            }
            else
            {
                WorldPlayer.Instance.Launch(stompLaunch * Vector3.up, 0);
            }
            //also make some special animations?
        }
    }
}
