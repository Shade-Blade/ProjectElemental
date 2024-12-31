using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
//using static WorldPlayer;

public interface IWorldBattleEntity
{
    public EncounterData GetEncounter();
    public BattleStartArguments GetBattleStartArguments();

    public void HandleBattleOutcome(BattleHelper.BattleOutcome outcome);
}

public class WorldEnemyEntity : WorldEntity, IWorldBattleEntity, IStompTrigger, IDashHopTrigger, ISlashTrigger, ISmashTrigger
{
    public string areaFlag;
    public bool touchEncounter; //true for most enemies
    public EncounterData encounter;
    public BattleStartArguments bsa = new BattleStartArguments();

    public int frameTimer = -1;
    public int firstStrikeTimer = -1;

    protected float ignoreTime;

    public enum ActionState
    {
        Idle,
        Wandering,
        Chasing,
        Swooping
    }

    public ActionState state;
    public float stateTime;

    public override void Start()
    {
        base.Start();
        if (!(areaFlag == null || areaFlag.Length == 0))
        {
            if (MainManager.Instance.GetAreaFlag(areaFlag))
            {
                Destroy(gameObject);
            }
        }
        stateTime = 0;
        state = ActionState.Idle;
    }


    public void ChangeState(ActionState state)
    {
        this.state = state;
        stateTime = 0;
    }

    // Update is called once per frame
    public override void WorldUpdate()
    {
        //this only really comes into play with enemies with real AI instead of this tester enemy
        if (ignoreTime > 0)
        {
            ignoreTime -= Time.deltaTime;
        }
        else
        {
            ignoreTime = 0;
        }

        stateTime += Time.deltaTime;

        BattleSetupUpdate();
    }

    public void BattleSetupUpdate()
    {
        //Cancel the battle start if this is active
        //(also don't allow battles to start while controls are disabled because that seems like it would cause problems)
        if (WorldPlayer.Instance.NoTouchEncounter() || MainManager.Instance.GetControlsDisabled())
        {
            frameTimer = -1;
            firstStrikeTimer = -1;
        }

        //delay battle start by 2 frames to make it possible for stomping to work (though the ordering of the OnCollisionEnter calls may make it possible without this)
        if (frameTimer >= 0)
        {
            frameTimer++;
        }

        if (firstStrikeTimer >= 0)
        {
            firstStrikeTimer++;
        }

        if (frameTimer > 2)
        {
            frameTimer = -1;
            firstStrikeTimer = -1;
            MainManager.Instance.mapScript.StartBattle(this);
        }
        if (firstStrikeTimer > 2)
        {
            frameTimer = -1;
            firstStrikeTimer = -1;
            bsa.move = BattleStartArguments.FirstStrikeMove.Default;
            bsa.firstStrikePosId = BattleStartArguments.FIRSTSTRIKE_FRONTMOST_ENEMY;
            WorldPlayer.Instance.SetAnimation("hurt", true);
            MainManager.Instance.mapScript.StartBattle(this);
        }
    }

    public override void ProcessCollision(Collision collision)
    {
        if (!touchEncounter)
        {
            return;
        }

        if (touchEncounter && frameTimer == -1)
        {
            WorldPlayer wp = collision.collider.GetComponent<WorldPlayer>();
            if (wp != null && !wp.NoTouchEncounter())
            {
                //Debug.Log(wp.undigTimer);
                if (wp.undigTimer > 0)
                {
                    frameTimer = -1;
                    firstStrikeTimer = -1;
                    bsa.move = BattleStartArguments.FirstStrikeMove.Dig;
                    bsa.firstStrikePosId = BattleStartArguments.FIRSTSTRIKE_FRONTMOST_ALLY;
                    SetAnimation("hurt", true);
                    MainManager.Instance.mapScript.StartBattle(this);
                }
                else
                {
                    if (collision.contacts[0].normal.x > 0.5)
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
        }


        base.ProcessCollision(collision);
    }

    public EncounterData GetEncounter()
    {
        return encounter;
    }
    public BattleStartArguments GetBattleStartArguments()
    {       
        return bsa;
    }
    public virtual void HandleBattleOutcome(BattleHelper.BattleOutcome outcome)
    {
        if (gameObject == null)
        {
            return;
        }

        frameTimer = -1;
        firstStrikeTimer = -1;
        bsa = new BattleStartArguments();
        if (outcome == BattleHelper.BattleOutcome.Win)
        {
            DeathDrops();
            DeathFlags();
            Destroy(gameObject);
        }
        else
        {
            Particle_Miss();
            ignoreTime = 1f;
        }
    }

    public void DeathDrops()
    {
        //Debug.Log(MainManager.Instance.coinDrops + " " + MainManager.Instance.dropItemType);
        BattleControl.CreateDeathSmokeOverworld(width, height, transform.position);
        MainManager.Instance.DropCoins(MainManager.Instance.coinDrops, transform.position, Vector3.up * 6, 3);
        MainManager.Instance.DropItems(MainManager.Instance.dropItemType, MainManager.Instance.dropItemCount, transform.position, Vector3.up * 6 - 1.5f * FacingVector(), Vector3.up * 6, 3);
    }

    public virtual void DeathFlags()
    {
        if (!(areaFlag == null || areaFlag.Length == 0))
        {
            MainManager.Instance.SetAreaFlag(areaFlag, true);
        }
    }

    public void Stomp(WorldPlayer.StompType stompType)
    {
        if (!touchEncounter)
        {
            return;
        }

        if (WorldPlayer.Instance.GetEncounterCooldown() > 0)
        {
            return;
        }

        switch (stompType)
        {
            case WorldPlayer.StompType.DoubleJump:
                bsa.move = BattleStartArguments.FirstStrikeMove.DoubleJump;
                break;
            case WorldPlayer.StompType.SuperJump:
                bsa.move = BattleStartArguments.FirstStrikeMove.SuperJump;
                break;
            case WorldPlayer.StompType.DashHop:
                bsa.move = BattleStartArguments.FirstStrikeMove.DashHop;
                break;
            default:
                bsa.move = BattleStartArguments.FirstStrikeMove.Stomp;
                break;
        }
        bsa.firstStrikePosId = BattleStartArguments.FIRSTSTRIKE_FRONTMOST_ALLY;
        SetAnimation("hurt", true);
        MainManager.Instance.mapScript.StartBattleOrOverkill(encounter.GetOverkillLevel(), this);
    }

    public bool Slash(Vector3 slashvector, Vector3 playerpos)
    {
        if (!touchEncounter)
        {
            return true;
        }

        if (WorldPlayer.Instance.GetEncounterCooldown() > 0)
        {
            return false;
        }

        bsa.move = BattleStartArguments.FirstStrikeMove.Weapon;
        bsa.firstStrikePosId = BattleStartArguments.FIRSTSTRIKE_FRONTMOST_ALLY;
        SetAnimation("hurt", true);
        MainManager.Instance.mapScript.StartBattleOrOverkill(encounter.GetOverkillLevel(), this);
        return true;
    }

    public bool Smash(Vector3 smashvector, Vector3 playerpos)
    {
        if (!touchEncounter)
        {
            return true;
        }

        if (WorldPlayer.Instance.GetEncounterCooldown() > 0)
        {
            return false;
        }

        bsa.move = BattleStartArguments.FirstStrikeMove.Weapon;
        bsa.firstStrikePosId = BattleStartArguments.FIRSTSTRIKE_FRONTMOST_ALLY;
        SetAnimation("hurt", true);
        MainManager.Instance.mapScript.StartBattleOrOverkill(encounter.GetOverkillLevel(), this);
        return true;
    }

    public void Bonk(Vector3 kickpos, Vector3 kicknormal)
    {
        if (!touchEncounter)
        {
            return;
        }

        if (WorldPlayer.Instance.GetEncounterCooldown() > 0)
        {
            return;
        }

        bsa.move = BattleStartArguments.FirstStrikeMove.DashHop;
        bsa.firstStrikePosId = BattleStartArguments.FIRSTSTRIKE_FRONTMOST_ALLY;
        SetAnimation("hurt", true);
        MainManager.Instance.mapScript.StartBattleOrOverkill(encounter.GetOverkillLevel(), this);
    }


    public bool AggroCheck(float maxRadius) //Max radius stops them from trying to chase you halfway across the map (currently it should be roughly wanderRadius + 4?)
    {
        return AggroCheck(FacingVector(), maxRadius);
    }
    public bool AggroCheck(Vector3 facingVector, float maxRadius)   //Flying enemies should have viewcones pointed closer to the ground (so you can't hide directly below them)
    {
        bool stealthStep = MainManager.Instance.playerData.BadgeEquipped(Badge.BadgeType.StealthStep);
        int stealthStepCount = MainManager.Instance.playerData.BadgeEquippedCount(Badge.BadgeType.StealthStep);

        float dot = Vector3.Dot(facingVector, (WorldPlayer.Instance.transform.position - transform.position));

        if ((WorldPlayer.Instance.transform.position - transform.position).magnitude > (stealthStep ? maxRadius * 0.75f : maxRadius))
        {
            return false;
        }

        if (!RaycastCheck())
        {
            return false;
        }

        float maxDot = -0.5f + 0.8f * (stealthStepCount / (1f + stealthStepCount));


        if (dot > maxDot)    //makes them somewhat hard to bypass but you can still sneak up on their back in a 120 degree range (-0.5)
        {
            return !WorldPlayer.Instance.Concealed();
        }
        return false;
    }
    public bool RaycastCheck()
    {
        //this raycast passes through npcs and player
        //so not hitting = no obstructions
        if (Physics.Raycast(transform.position, (WorldPlayer.Instance.transform.position - transform.position), (WorldPlayer.Instance.transform.position - transform.position).magnitude, 1))
        {
            return false;
        }

        return true;
    }
}
