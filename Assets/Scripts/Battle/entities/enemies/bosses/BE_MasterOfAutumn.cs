using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BE_MasterOfAutumn : BattleEntity
{
    public int ai_state;
    public bool second_phase;
    public BattleEntity throne;
    public bool onthrone;

    public int counterCount;

    public int lastSummonPos;

    public override void Initialize()
    {
        ai_state = 0;
        onthrone = false;

        moveset = new List<Move>()
        {
            gameObject.AddComponent<BM_MasterOfAutumn_ThornToss>(),
            gameObject.AddComponent<BM_MasterOfAutumn_PollenStorm>(),
            gameObject.AddComponent<BM_MasterOfAutumn_FlowerShuriken>(),
            gameObject.AddComponent<BM_MasterOfAutumn_Overgrowth>(),
            gameObject.AddComponent<BM_MasterOfAutumn_VineStab>(),
            gameObject.AddComponent<BM_MasterOfAutumn_FullBloom>(),
            gameObject.AddComponent<BM_MasterOfAutumn_VineField>(),
            gameObject.AddComponent<BM_MasterOfAutumn_Resummon>(),
            gameObject.AddComponent<BM_MasterOfAutumn_Fall>(),

            gameObject.AddComponent<BM_MasterOfAutumn_Hard_RootShake>(),
            gameObject.AddComponent<BM_MasterOfAutumn_Hard_RootDrain>(),
        };

        base.Initialize();
    }

    public override void ChooseMoveInternal()
    {
        counterCount = 0;
        //time to enter phase 2?
        if (!second_phase && hp <= (int)(maxHP * 2.001f / 3))
        {
            second_phase = true;
            currMove = moveset[3];
        }
        else
        {
            if (onthrone && !BattleControl.Instance.EntityValid(throne))
            {
                onthrone = false;
                currMove = moveset[8];
            }
            else
            {
                if (throne != null)
                {
                    switch (ai_state)
                    {
                        case 0:
                            currMove = moveset[4];
                            ai_state = 1;
                            break;
                        case 1:
                            currMove = moveset[5];
                            ai_state = 2;
                            break;
                        case 2:
                            currMove = moveset[6];
                            if (BattleControl.Instance.GetCurseLevel() > 0)
                            {
                                ai_state = 3;
                            }
                            else
                            {
                                ai_state = 0;
                            }
                            break;
                        case 3:
                            currMove = moveset[9];
                            ai_state = 0;
                            break;
                    }
                }
                else
                {
                    switch (ai_state)
                    {
                        case 0:
                            currMove = moveset[0];
                            ai_state = 1;
                            break;
                        case 1:
                            currMove = moveset[1];
                            ai_state = 2;
                            break;
                        case 2:
                            currMove = moveset[2];
                            if (BattleControl.Instance.GetCurseLevel() > 0)
                            {
                                ai_state = 3;
                            }
                            else
                            {
                                ai_state = 0;
                            }
                            break;
                        case 3:
                            currMove = moveset[8];
                            ai_state = 0;
                            break;
                    }
                }
            }
        }

        BasicTargetChooser();
    }


    public override IEnumerator PostMove()
    {
        //also reset here in case something weird happens
        counterCount = 0;
        yield return StartCoroutine(base.PostMove());
    }
    public override IEnumerator PreReact(Move move, BattleEntity target)
    {
        counterCount = 0;

        Effect_ReactionDefend();

        yield return new WaitForSeconds(0.5f);
    }
    public override bool ReactToEvent(BattleEntity target, BattleHelper.Event e, int previousReactions)
    {
        if (onthrone && (!BattleControl.Instance.EntityValid(throne) || throne.hp == 0))
        {
            onthrone = false;
            counterCount++;
            BattleControl.Instance.AddReactionMoveEvent(this, target.lastAttacker, moveset[8], true);
            return true;
        }

        if (second_phase && BattleControl.Instance.GetEntityByID(1) == null && BattleControl.Instance.GetEntityByID(3) == null)
        {
            counterCount++;
            BattleControl.Instance.AddReactionMoveEvent(this, target.lastAttacker, moveset[7], true);
            return true;
        }

        return false;
    }
}

public class BE_VineThrone : BattleEntity
{
    int counterCount;
    public override void Initialize()
    {
        moveset = new List<Move>()
        {
            gameObject.AddComponent<BM_VinePlatform_Hard_CounterSoften>()
        };

        base.Initialize();
    }

    public override void ChooseMoveInternal()
    {
        counterCount = 0;
        //Does nothing
        currMove = null;
    }

    public override IEnumerator PostMove()
    {
        //also reset here in case something weird happens
        counterCount = 0;
        yield return StartCoroutine(base.PostMove());
    }

    public override IEnumerator PreReact(Move move, BattleEntity target)
    {
        counterCount = 0;

        Effect_ReactionDefend();

        yield return new WaitForSeconds(0.5f);
    }
    public override bool ReactToEvent(BattleEntity target, BattleHelper.Event e, int previousReactions)
    {
        if (BattleControl.Instance.GetCurseLevel() <= 0)
        {
            return false;
        }

        if (e == BattleHelper.Event.Hurt && target == this && counterCount <= 0)
        {
            counterCount++;
            BattleControl.Instance.AddReactionMoveEvent(this, target.lastAttacker, moveset[0]);
            return true;
        }

        return false;
    }
}

public class BE_GiantVine : BattleEntity
{
    int counterCount = 0;
    public int ai_state = 0;

    public int grabTargetPos = int.MinValue;
    public BattleEntity grabbed;
    public bool throwing;

    public override void Initialize()
    {
        ai_state = 2;   //Start on telegraph turn >:)
        //Note that the front vine will block the back vine from grabbing as it checks for the first vine marking a position

        grabTargetPos = int.MinValue;
        moveset = new List<Move> {  gameObject.AddComponent<BM_GiantVine_Slam>(),
                                    gameObject.AddComponent<BM_GiantVine_BigSlam>(),
                                    gameObject.AddComponent<BM_GiantVine_Telegraph>(),
                                    gameObject.AddComponent<BM_GiantVine_Grab>(),
                                    gameObject.AddComponent<BM_GiantVine_Constrict>(),
                                    gameObject.AddComponent<BM_GiantVine_Throw>(),
                                    gameObject.AddComponent<BM_GiantVine_Hard_LashOut>(),
                                 };

        base.Initialize();
    }

    public override void ChooseMoveInternal()
    {
        counterCount = 0;
        throwing = false;

        //do the grab stuff
        if (grabbed != null)
        {
            currMove = moveset[4];
        } else
        {
            //validation
            //Check for already grabbed players
            List<PlayerEntity> pel = BattleControl.Instance.GetPlayerEntities();
            bool grabbedPlayer = false;
            int grabbedPlayerPos = 0;
            for (int i = 0; i < pel.Count; i++)
            {
                if (pel[i].GetEntityProperty(BattleHelper.EntityProperties.NoTarget))
                {
                    grabbedPlayer = true;
                    grabbedPlayerPos = pel[i].posId;
                    break;
                }

                if (pel[i].GetEntityProperty(BattleHelper.EntityProperties.PositionMark))
                {
                    grabbedPlayer = true;
                    grabbedPlayerPos = pel[i].posId;
                    break;
                }

                if (pel[i].hp == 0)
                {
                    grabbedPlayer = true;
                    grabbedPlayerPos = pel[i].posId;
                    break;
                }
            }

            if (ai_state == 2)  //Telegraph
            {
                if (pel.Count < 2 || grabbedPlayer)
                {
                    //Bad: go back to 0
                    ai_state = 0;
                }
            }

            if (ai_state == 3)
            {
                //Other vine is trying to grab someone else?
                if (pel.Count < 2 || (grabbedPlayer && grabbedPlayerPos != grabTargetPos))
                {
                    //Go back to 1
                    ai_state = 1;
                }
            }

            switch (ai_state)
            {
                case 0:
                    if (BattleControl.Instance.GetCurseLevel() > 0)
                    {
                        currMove = moveset[6];
                    }
                    else
                    {
                        currMove = moveset[0];
                    }
                    ai_state = 1;
                    SetEntityProperty(BattleHelper.EntityProperties.StateCharge, false);
                    break;
                case 1:
                    currMove = moveset[1];
                    ai_state = 2;
                    SetEntityProperty(BattleHelper.EntityProperties.StateCharge, false);
                    break;
                case 2:
                    currMove = moveset[2];
                    ai_state = 3;
                    break;
                case 3:
                    currMove = moveset[3];
                    SetEntityProperty(BattleHelper.EntityProperties.StateCharge, false);
                    //No AI state change :)
                    //The throw away thing will reset ai state to 0
                    break;
            }
        }

        BasicTargetChooser();
    }

    public override IEnumerator DoEvent(BattleHelper.Event eventID)
    {
        yield return StartCoroutine(DefaultEventHandler(eventID));
    }

    public override bool CanMove()
    {
        ValidateEffects();

        Effect.EffectType[] effectList =
            new Effect.EffectType[] {
                Effect.EffectType.Freeze,
                Effect.EffectType.Sleep,
                Effect.EffectType.TimeStop,
            };

        foreach (Effect.EffectType e in effectList)
        {
            if (GetEffectEntry(e) != null)
            {
                return false;
            }
        }
        if (hp <= 0)
        {
            if (counterCount > 0)
            {
                return true;
            }
            DeathCheck();   //may break stuff later but it shouldn't?
            return false;
        }
        return true;
    }

    public override IEnumerator PostMove()
    {
        //also reset here in case something weird happens
        counterCount = 0;
        yield return StartCoroutine(base.PostMove());
    }
    public override IEnumerator PreReact(Move move, BattleEntity target)
    {
        counterCount = 0;

        Effect_ReactionCounter();

        yield return new WaitForSeconds(0.5f);
    }
    public override IEnumerator DefaultDeathEvent()
    {
        if (counterCount > 0)
        {
            yield break;
        }

        if (grabbed == null && grabTargetPos != int.MinValue)
        {
            //unmark
            BattleEntity grabbedB = BattleControl.Instance.GetEntityByID(grabTargetPos);
            if (grabbedB != null)
            {
                grabbedB.SetEntityProperty(BattleHelper.EntityProperties.PositionMark, false);
            }
        }

        yield return StartCoroutine(base.DefaultDeathEvent());
    }
    public override bool ReactToEvent(BattleEntity target, BattleHelper.Event e, int previousReactions)
    {
        if ((target.hp == 0 || e == BattleHelper.Event.Death) && target == this && grabbed != null && counterCount <= 0)
        {
            counterCount++;
            BattleControl.Instance.AddReactionMoveEvent(this, target.lastAttacker, moveset[5]);
            return true;
        }
        if (grabbed != null && !throwing && grabbed.hp == 0 && counterCount <= 0)
        {
            throwing = true;
            counterCount++;
            BattleControl.Instance.AddReactionMoveEvent(this, target.lastAttacker, moveset[5]);
            return true;
        }

        return false;
    }
}

public class BM_GiantVine_Slam : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.GiantVine_Slam;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemy);

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        if (!BattleControl.Instance.EntityValid(caller.curTarget))
        {
            caller.curTarget = null;
        }

        yield return StartCoroutine(caller.SpinHeavy(Vector3.up * 360, 0.5f));

        if (caller.curTarget != null)
        {
            if (caller.GetAttackHit(caller.curTarget, 0))
            {
                caller.DealDamage(caller.curTarget, 6, BattleHelper.DamageType.Normal, 0, BattleHelper.ContactLevel.Contact);
            }
            else
            {
                caller.InvokeMissEvents(caller.curTarget);
            }
        }
    }
}

public class BM_GiantVine_BigSlam : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.GiantVine_BigSlam;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemy, true);

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        List<BattleEntity> targets = BattleControl.Instance.GetEntitiesSorted(caller, GetBaseTarget());

        yield return StartCoroutine(caller.SpinHeavy(Vector3.up * 360, 1f));

        foreach (BattleEntity t in targets)
        {
            if (caller.GetAttackHit(t, 0))
            {
                caller.DealDamage(t, 6, BattleHelper.DamageType.Normal, 0, BattleHelper.ContactLevel.Contact);
            }
            else
            {
                caller.InvokeMissEvents(t);
            }
        }
    }
}

public class BM_GiantVine_Telegraph : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.GiantVine_Telegraph;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemyLowFrontmost);

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        if (!BattleControl.Instance.EntityValid(caller.curTarget))
        {
            caller.curTarget = null;
            yield break;
        }

        yield return StartCoroutine(caller.SpinHeavy(Vector3.up * 360, 0.25f));

        BE_GiantVine gv = (BE_GiantVine)caller;

        gv.grabTargetPos = caller.curTarget.posId;
        gv.curTarget.SetEntityProperty(BattleHelper.EntityProperties.PositionMark, true);
        gv.SetEntityProperty(BattleHelper.EntityProperties.StateCharge, true);
    }
}

public class BM_GiantVine_Grab : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.GiantVine_Grab;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemy);

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        if (!BattleControl.Instance.EntityValid(caller.curTarget))
        {
            caller.curTarget = null;
            yield break;
        }

        yield return StartCoroutine(caller.SpinHeavy(Vector3.up * 360, 0.25f));

        BE_GiantVine gv = (BE_GiantVine)caller;

        //Complicated things

        List<BattleEntity> playerParty = BattleControl.Instance.GetEntities((e) => BattleControl.IsPlayerControlled(e, true));
        gv.grabbed = BattleControl.Instance.GetEntityByID(gv.grabTargetPos);
        gv.grabbed.SetEntityProperty(BattleHelper.EntityProperties.PositionMark, false);

        if (gv.grabbed.hp == 0 || !caller.GetAttackHit(gv.grabbed, 0))
        {
            //Fail to grab
            gv.grabbed = null;
            yield break;
        }

        gv.grabbed.SetEntityProperty(BattleHelper.EntityProperties.NoCount, true);
        gv.grabbed.SetEntityProperty(BattleHelper.EntityProperties.NoTarget, true);

        Vector3 pos = gv.ApplyScaledOffset(Vector3.up * 0.75f);
        yield return StartCoroutine(gv.grabbed.Move(pos, 8));

        //Swap positions
        Vector3 tempHPos = playerParty[0].homePos;
        Vector3 tempPos = playerParty[0].transform.position;
        //Not swapping posIds makes things still work
        int tempID = playerParty[0].posId;

        BattleControl.Instance.SwapEffectCasters(playerParty);

        //wacky coroutine juggling
        List<bool> boolList = new List<bool>();

        for (int i = 0; i < playerParty.Count; i++)
        {
            boolList.Add(false);
            if (i == playerParty.Count - 1)
            {
                playerParty[i].homePos = tempHPos;
                //playerParty[i].transform.position = tempPos;
                playerParty[i].posId = tempID;
            }
            else
            {
                playerParty[i].homePos = playerParty[i + 1].homePos;
                //playerParty[i].transform.position = playerParty[i + 1].transform.position;
                playerParty[i].posId = playerParty[i + 1].posId;
            }
        }

        IEnumerator SpecialMove(int index)
        {
            if (playerParty[index].posId != gv.grabbed.posId)
            {
                yield return StartCoroutine(playerParty[index].Move(playerParty[index].homePos));
            }
            boolList[index] = true;
        }

        bool CheckBoolList()
        {
            for (int i = 0; i < boolList.Count; i++)
            {
                if (boolList[i] == false)
                {
                    return false;
                }
            }
            return true;
        }

        for (int i = 0; i < playerParty.Count; i++)
        {
            StartCoroutine(SpecialMove(i));
        }

        yield return new WaitUntil(() => CheckBoolList());
    }
}

public class BM_GiantVine_Constrict : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.GiantVine_Constrict;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.Self);

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        BE_GiantVine gv = (BE_GiantVine)caller;
        yield return StartCoroutine(caller.SpinHeavy(Vector3.up * 360, 0.5f));

        if (gv.grabbed != null)
        {
            if (caller.GetAttackHit(gv.grabbed, 0))
            {
                caller.DealDamage(gv.grabbed, 9, BattleHelper.DamageType.Normal, 0, BattleHelper.ContactLevel.Contact);
            }
            else
            {
                caller.InvokeMissEvents(gv.grabbed);
            }
        }
    }
}

public class BM_GiantVine_Throw : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.GiantVine_Throw;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.Self);

    public override IEnumerator ExecuteOutOfTurn(BattleEntity caller, BattleEntity causer, int level = 1)
    {
        BE_GiantVine gv = (BE_GiantVine)caller;
        yield return StartCoroutine(caller.SpinHeavy(Vector3.up * 360, 0.5f));

        if (gv.grabbed != null)
        {
            //Do the opposite of grab
            gv.grabbed.SetEntityProperty(BattleHelper.EntityProperties.NoCount, false);
            gv.grabbed.SetEntityProperty(BattleHelper.EntityProperties.NoTarget, false);

            yield return StartCoroutine(gv.grabbed.Jump(gv.grabbed.homePos, 2, 0.5f));

            gv.grabbed = null;
            gv.grabTargetPos = int.MinValue;
            gv.ai_state = 0;
        }

        if (caller.hp == 0)
        {
            caller.SetAnimation("dead");
            yield return StartCoroutine(caller.DefaultDeathEvent());
            yield break;
        }
        gv.throwing = false;
    }
}

public class BM_GiantVine_Hard_LashOut : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.GiantVine_Hard_LashOut;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemy);

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        if (!BattleControl.Instance.EntityValid(caller.curTarget))
        {
            caller.curTarget = null;
        }

        yield return StartCoroutine(caller.SpinHeavy(Vector3.up * 360, 0.5f));

        if (caller.curTarget != null)
        {
            if (caller.GetAttackHit(caller.curTarget, 0))
            {
                caller.DealDamage(caller.curTarget, 6, BattleHelper.DamageType.Earth, 0, BattleHelper.ContactLevel.Contact);
            }
            else
            {
                caller.InvokeMissEvents(caller.curTarget);
            }
        }
    }
}



public class BM_MasterOfAutumn_ThornToss : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.MasterOfAutumn_ThornToss;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemy);

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        if (!BattleControl.Instance.EntityValid(caller.curTarget))
        {
            caller.curTarget = null;
        }

        yield return StartCoroutine(caller.SpinHeavy(Vector3.up * 360, 0.5f));

        if (caller.curTarget != null)
        {
            if (caller.GetAttackHit(caller.curTarget, 0))
            {
                for (int i = 0; i < 3; i++)
                {
                    if (i > 0)
                    {
                        yield return new WaitForSeconds(0.25f);
                    }
                    caller.DealDamage(caller.curTarget, 3, BattleHelper.DamageType.Normal, 0, BattleHelper.ContactLevel.Infinite);
                }
            }
            else
            {
                caller.InvokeMissEvents(caller.curTarget);
            }
        }
    }
}

public class BM_MasterOfAutumn_PollenStorm : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.MasterOfAutumn_PollenStorm;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemy, true);

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        List<BattleEntity> targets = BattleControl.Instance.GetEntitiesSorted(caller, GetBaseTarget());

        yield return StartCoroutine(caller.SpinHeavy(Vector3.up * 360, 0.5f));

        foreach (BattleEntity t in targets)
        {
            if (caller.GetAttackHit(t, 0))
            {
                caller.DealDamage(t, 4, BattleHelper.DamageType.Earth, 0, BattleHelper.ContactLevel.Infinite);
                caller.InflictEffect(t, new Effect(Effect.EffectType.Dizzy, 1, 2));
            }
            else
            {
                //Miss
                caller.InvokeMissEvents(t);
            }
        }
    }
}

public class BM_MasterOfAutumn_FlowerShuriken : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.MasterOfAutumn_FlowerShuriken;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemy, true);

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        List<BattleEntity> targets = BattleControl.Instance.GetEntitiesSorted(caller, GetBaseTarget());

        yield return StartCoroutine(caller.SpinHeavy(Vector3.up * 360, 0.5f));

        foreach (BattleEntity t in targets)
        {
            if (caller.GetAttackHit(t, 0))
            {
                caller.DealDamage(t, 6, BattleHelper.DamageType.Earth, 0, BattleHelper.ContactLevel.Infinite);
            }
            else
            {
                //Miss
                caller.InvokeMissEvents(t);
            }
        }
    }
}

public class BM_MasterOfAutumn_Overgrowth : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.MasterOfAutumn_Overgrowth;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.Self);

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        //Heal for 10, summon burrow trap and sundew and vine platform
        BE_MasterOfAutumn vb = (BE_MasterOfAutumn)caller;

        if (BattleControl.Instance.GetEntityByID(1) == null)
        {
            BattleEntity a = BattleControl.Instance.SummonEntity(BattleHelper.EntityID.GiantVine, 1);
            //No level grinding from the superboss here
            a.level = caller.level;
            vb.InflictEffectForce(a, new Effect(Effect.EffectType.Cooldown, 1, Effect.INFINITE_DURATION));
            if (BattleControl.Instance.GetCurseLevel() > 0)
            {
                vb.InflictEffectForce(a, new Effect(Effect.EffectType.Soften, 1, 2));
            }
        }
        if (BattleControl.Instance.GetEntityByID(3) == null)
        {
            BattleEntity b = BattleControl.Instance.SummonEntity(BattleHelper.EntityID.GiantVine, 3);
            //No level grinding from the superboss here
            b.level = caller.level;
            vb.InflictEffectForce(b, new Effect(Effect.EffectType.Cooldown, 1, Effect.INFINITE_DURATION));
            if (BattleControl.Instance.GetCurseLevel() > 0)
            {
                vb.InflictEffectForce(b, new Effect(Effect.EffectType.Soften, 1, 2));
            }
        }

        vb.HealHealth(10);
        vb.posId = 7;
        BattleEntity platform = BattleControl.Instance.SummonEntity(BattleHelper.EntityID.VineThrone, 2);
        vb.throne = platform;
        vb.onthrone = true;
        vb.homePos = platform.transform.position + platform.height * Vector3.up;
        vb.SetEntityProperty(BattleHelper.EntityProperties.Grounded, false);
        vb.SetEntityProperty(BattleHelper.EntityProperties.NoTarget, true);
        yield return StartCoroutine(vb.Move(vb.homePos, 8));
    }
}

public class BM_MasterOfAutumn_VineStab : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.MasterOfAutumn_VineStab;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemy);

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        if (!BattleControl.Instance.EntityValid(caller.curTarget))
        {
            caller.curTarget = null;
        }

        yield return StartCoroutine(caller.SpinHeavy(Vector3.up * 360, 0.5f));

        if (caller.curTarget != null)
        {
            if (caller.GetAttackHit(caller.curTarget, 0))
            {
                for (int i = 0; i < 2; i++)
                {
                    if (i > 0)
                    {
                        yield return new WaitForSeconds(0.25f);
                    }
                    caller.DealDamage(caller.curTarget, 7, BattleHelper.DamageType.Normal, 0, BattleHelper.ContactLevel.Infinite);
                }
            }
            else
            {
                caller.InvokeMissEvents(caller.curTarget);
            }
        }
    }
}

public class BM_MasterOfAutumn_FullBloom : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.MasterOfAutumn_FullBloom;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemy, true);

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        List<BattleEntity> targets = BattleControl.Instance.GetEntitiesSorted(caller, GetBaseTarget());

        yield return StartCoroutine(caller.SpinHeavy(Vector3.up * 360, 0.5f));

        foreach (BattleEntity t in targets)
        {
            if (caller.GetAttackHit(t, 0))
            {
                caller.DealDamage(t, 8, BattleHelper.DamageType.Earth, 0, BattleHelper.ContactLevel.Infinite);
                caller.InflictEffect(t, new Effect(Effect.EffectType.Dizzy, 1, 2));
            }
            else
            {
                //Miss
                caller.InvokeMissEvents(t);
            }
        }
    }
}

public class BM_MasterOfAutumn_VineField : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.MasterOfAutumn_VineField;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemy, true);

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        List<BattleEntity> targets = BattleControl.Instance.GetEntitiesSorted(caller, GetBaseTarget());

        yield return StartCoroutine(caller.SpinHeavy(Vector3.up * 360, 0.5f));

        foreach (BattleEntity t in targets)
        {
            if (caller.GetAttackHit(t, 0))
            {
                caller.DealDamage(t, 10, BattleHelper.DamageType.Earth, 0, BattleHelper.ContactLevel.Infinite);
            }
            else
            {
                //Miss
                caller.InvokeMissEvents(t);
            }
        }
    }
}

public class BM_MasterOfAutumn_Resummon : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.MasterOfAutumn_Resummon;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.Self);

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        BE_MasterOfAutumn vb = (BE_MasterOfAutumn)caller;

        if (BattleControl.Instance.GetEntityByID(1) == null && BattleControl.Instance.GetEntityByID(3) == null)
        {
            if (vb.lastSummonPos == 1)
            {
                BattleEntity a = BattleControl.Instance.SummonEntity(BattleHelper.EntityID.GiantVine, 3);
                //No level grinding from the superboss here
                a.level = caller.level;
                //a.SetEntityProperty(BattleHelper.EntityProperties.NoCount);
                vb.InflictEffectForce(a, new Effect(Effect.EffectType.Cooldown, 1, Effect.INFINITE_DURATION));
                if (BattleControl.Instance.GetCurseLevel() > 0)
                {
                    vb.InflictEffectForce(a, new Effect(Effect.EffectType.Soften, 1, 2));
                }
            }
            else
            {
                BattleEntity a = BattleControl.Instance.SummonEntity(BattleHelper.EntityID.GiantVine, 1);
                //No level grinding from the superboss here
                a.level = caller.level;
                vb.InflictEffectForce(a, new Effect(Effect.EffectType.Cooldown, 1, Effect.INFINITE_DURATION));
                if (BattleControl.Instance.GetCurseLevel() > 0)
                {
                    vb.InflictEffectForce(a, new Effect(Effect.EffectType.Soften, 1, 2));
                }
            }
        }

        yield return null;
    }
    public override IEnumerator ExecuteOutOfTurn(BattleEntity caller, BattleEntity target, int level = 1)
    {
        BE_MasterOfAutumn vb = (BE_MasterOfAutumn)caller;

        if (BattleControl.Instance.GetEntityByID(1) == null && BattleControl.Instance.GetEntityByID(3) == null)
        {
            if (vb.lastSummonPos == 1)
            {
                BattleEntity a = BattleControl.Instance.SummonEntity(BattleHelper.EntityID.GiantVine, 3);
                //No level grinding from the superboss here
                a.level = caller.level;
                vb.InflictEffectForce(a, new Effect(Effect.EffectType.Cooldown, 1, Effect.INFINITE_DURATION));
            }
            else
            {
                BattleEntity a = BattleControl.Instance.SummonEntity(BattleHelper.EntityID.GiantVine, 1);
                //No level grinding from the superboss here
                a.level = caller.level;
                vb.InflictEffectForce(a, new Effect(Effect.EffectType.Cooldown, 1, Effect.INFINITE_DURATION));
            }
        }

        yield return null;
    }
}

public class BM_MasterOfAutumn_Fall : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.MasterOfAutumn_Fall;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.Self);

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        BE_MasterOfAutumn vb = (BE_MasterOfAutumn)caller;

        vb.posId = 2;
        vb.homePos = vb.homePos + vb.homePos.y * Vector3.down;
        yield return StartCoroutine(vb.Move(vb.homePos, 8));

        vb.throne = null;
        vb.InflictEffect(vb, new Effect(Effect.EffectType.DefenseBoost, 1, Effect.INFINITE_DURATION));
        //Front vine is guaranteed, back vine is super curse exclusive
        if (BattleControl.Instance.GetEntityByID(1) == null)
        {
            BattleEntity a = BattleControl.Instance.SummonEntity(BattleHelper.EntityID.GiantVine, 1);
            //No level grinding from the superboss here
            a.level = caller.level;
            vb.InflictEffectForce(a, new Effect(Effect.EffectType.Cooldown, 1, Effect.INFINITE_DURATION));
        }
        if (BattleControl.Instance.GetCurseLevel() > 0)
        {
            if (BattleControl.Instance.GetEntityByID(3) == null)
            {
                BattleEntity b = BattleControl.Instance.SummonEntity(BattleHelper.EntityID.GiantVine, 3);
                //No level grinding from the superboss here
                b.level = caller.level;
                vb.InflictEffectForce(b, new Effect(Effect.EffectType.Cooldown, 1, Effect.INFINITE_DURATION));
            }
        }
        vb.SetEntityProperty(BattleHelper.EntityProperties.NoTarget, false);
        vb.SetEntityProperty(BattleHelper.EntityProperties.NoCount, false);
        vb.SetEntityProperty(BattleHelper.EntityProperties.Grounded, true);
    }
    public override IEnumerator ExecuteOutOfTurn(BattleEntity caller, BattleEntity target, int level = 1)
    {
        BE_MasterOfAutumn vb = (BE_MasterOfAutumn)caller;

        vb.posId = 2;
        vb.homePos = vb.homePos + vb.homePos.y * Vector3.down;
        yield return StartCoroutine(vb.Move(vb.homePos, 8));

        vb.throne = null;
        vb.InflictEffect(vb, new Effect(Effect.EffectType.DefenseBoost, 1, Effect.INFINITE_DURATION));

        //Front vine is guaranteed, back vine is super curse exclusive
        if (BattleControl.Instance.GetEntityByID(1) == null)
        {
            BattleEntity a = BattleControl.Instance.SummonEntity(BattleHelper.EntityID.GiantVine, 1);
            //No level grinding from the superboss here
            a.level = caller.level;
            vb.InflictEffectForce(a, new Effect(Effect.EffectType.Cooldown, 1, Effect.INFINITE_DURATION));
        }
        if (BattleControl.Instance.GetCurseLevel() > 0)
        {
            if (BattleControl.Instance.GetEntityByID(3) == null)
            {
                BattleEntity b = BattleControl.Instance.SummonEntity(BattleHelper.EntityID.GiantVine, 3);
                //No level grinding from the superboss here
                b.level = caller.level;
                vb.InflictEffectForce(b, new Effect(Effect.EffectType.Cooldown, 1, Effect.INFINITE_DURATION));
            }
        }
        vb.SetEntityProperty(BattleHelper.EntityProperties.NoTarget, false);
        vb.SetEntityProperty(BattleHelper.EntityProperties.NoCount, false);
        vb.SetEntityProperty(BattleHelper.EntityProperties.Grounded, true);
    }
}

public class BM_MasterOfAutumn_Hard_RootShake : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.MasterOfAutumn_Hard_RootShake;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemy, true);

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        List<BattleEntity> targets = BattleControl.Instance.GetEntitiesSorted(caller, GetBaseTarget());

        yield return StartCoroutine(caller.SpinHeavy(Vector3.up * 360, 0.5f));

        foreach (BattleEntity t in targets)
        {
            if (caller.GetAttackHit(t, 0))
            {
                caller.DealDamage(t, 4, BattleHelper.DamageType.Earth, 0, BattleHelper.ContactLevel.Infinite);
                caller.InflictEffect(t, new Effect(Effect.EffectType.Defocus, 4, Effect.INFINITE_DURATION));
            }
            else
            {
                //Miss
                caller.InvokeMissEvents(t);
            }
        }
    }
}

public class BM_MasterOfAutumn_Hard_RootDrain : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.MasterOfAutumn_Hard_RootDrain;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemy, true);

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        List<BattleEntity> targets = BattleControl.Instance.GetEntitiesSorted(caller, GetBaseTarget());

        yield return StartCoroutine(caller.SpinHeavy(Vector3.up * 360, 0.5f));

        foreach (BattleEntity t in targets)
        {
            if (caller.GetAttackHit(t, 0))
            {
                caller.DealDamage(t, 6, BattleHelper.DamageType.Earth, (ulong)BattleHelper.DamageProperties.HPDrainOneToOne, BattleHelper.ContactLevel.Infinite);
                caller.InflictEffect(t, new Effect(Effect.EffectType.Exhausted, 1, 2));
            }
            else
            {
                //Miss
                caller.InvokeMissEvents(t);
            }
        }
    }
}