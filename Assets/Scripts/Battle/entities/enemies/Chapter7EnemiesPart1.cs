using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BE_Plateshell : BattleEntity
{
    public override void Initialize()
    {
        moveset = new List<Move> { gameObject.AddComponent<BM_Plateshell_Slam>(), gameObject.AddComponent<BM_Plateshell_RageFireball>() };

        base.Initialize();
    }

    public override void ChooseMoveInternal()
    {
        currMove = moveset[(posId + BattleControl.Instance.turnCount - 1) % 2];
        BasicTargetChooser();
    }
}

public class BM_Plateshell_Slam : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Plateshell_Slam;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemy);

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        if (!BattleControl.Instance.EntityValid(caller.curTarget))
        {
            caller.curTarget = null;
        }

        if (caller.curTarget != null)
        {
            Vector3 tpos = caller.curTarget.ApplyScaledOffset(caller.curTarget.stompOffset);
            Vector3 spos = transform.position;

            yield return StartCoroutine(caller.JumpHeavy(tpos, 2, 0.5f, -0.25f));

            if (caller.GetAttackHit(caller.curTarget, 0))
            {
                caller.DealDamage(caller.curTarget, 4, 0, 0, BattleHelper.ContactLevel.Contact);
                yield return StartCoroutine(caller.JumpHeavy(caller.homePos, 2, 0.5f, 0.15f));
            }
            else
            {
                caller.InvokeMissEvents(caller.curTarget);

                //extrapolate the move curve
                yield return StartCoroutine(caller.ExtrapolateJumpHeavy(spos, tpos, 2, 0.5f, -0.25f));
                yield return StartCoroutine(caller.Move(caller.homePos));
            }
        }

        yield return StartCoroutine(caller.Move(caller.homePos));
    }
}

public class BM_Plateshell_RageFireball : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Plateshell_RageFireball;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemy);

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        if (!BattleControl.Instance.EntityValid(caller.curTarget))
        {
            caller.curTarget = null;
        }

        yield return StartCoroutine(caller.Spin(Vector3.up * 360, 0.5f));

        if (caller.curTarget != null)
        {
            if (caller.GetAttackHit(caller.curTarget, BattleHelper.DamageType.Fire))
            {
                bool hasStatus = caller.curTarget.HasStatus();
                caller.DealDamage(caller.curTarget, 5, BattleHelper.DamageType.Fire, 0, BattleHelper.ContactLevel.Infinite);
                if (!hasStatus)
                {
                    caller.InflictEffect(caller.curTarget, new Effect(Effect.EffectType.Berserk, 1, 3), caller.posId);
                }
                if (BattleControl.Instance.GetCurseLevel() > 0)
                {
                    caller.InflictEffectBuffered(caller, new Effect(Effect.EffectType.Focus, 2, Effect.INFINITE_DURATION));
                    caller.InflictEffectBuffered(caller, new Effect(Effect.EffectType.Sunder, 1, Effect.INFINITE_DURATION));
                }
            }
            else
            {
                caller.InvokeMissEvents(caller.curTarget);
            }
        }
    }
}

public class BE_Speartongue : BattleEntity
{
    int counterCount;
    public override void Initialize()
    {
        moveset = new List<Move> { gameObject.AddComponent<BM_Speartongue_TongueStab>(), gameObject.AddComponent<BM_Speartongue_Shroud>(), gameObject.AddComponent<BM_Speartongue_Stare>(), gameObject.AddComponent<BM_Shared_Hard_CounterHide>() };

        base.Initialize();
    }

    public override void ChooseMoveInternal()
    {
        //also reset here in case something weird happens
        counterCount = 0;

        currMove = moveset[(posId + BattleControl.Instance.turnCount - 1) % 3];
        if (BattleControl.Instance.GetCurseLevel() > 0)
        {
            //No shroud
            if (currMove == moveset[1])
            {
                currMove = moveset[0];
            }
        }

        //special ethereal targetting?
        bool stare = false;

        //shroud yourself instead of stare
        if (currMove != moveset[1])
        {
            List<BattleEntity> bl = BattleControl.Instance.GetEntities(this, new TargetArea(TargetArea.TargetAreaType.LiveEnemy));
            TargetStrategy strategy = new TargetStrategy(TargetStrategy.TargetStrategyType.FrontMost);
            bl.Sort((a, b) => strategy.selectorFunction(a, b));

            for (int i = 0; i < bl.Count; i++)
            {
                if (bl[i].HasEffect(Effect.EffectType.Ethereal))
                {
                    currMove = moveset[2];
                    curTarget = bl[i];
                    stare = true;
                    break;
                }
            }
        }

        if (!stare)
        {
            BasicTargetChooser();
        }
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
            BattleControl.Instance.AddReactionMoveEvent(this, target.lastAttacker, moveset[3]);
            return true;
        }

        return false;
    }
}

public class BM_Speartongue_TongueStab : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Speartongue_TongueStab;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemy);

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        if (!BattleControl.Instance.EntityValid(caller.curTarget))
        {
            caller.curTarget = null;
        }

        yield return StartCoroutine(caller.Spin(Vector3.up * 360, 1f));

        List<BattleEntity> targets = BattleControl.Instance.GetEntitiesSorted(caller, GetBaseTarget());

        if (caller.GetAttackHit(caller.curTarget, BattleHelper.DamageType.Dark))
        {
            bool hasStatus = caller.curTarget.HasStatus();
            caller.DealDamage(caller.curTarget, 5, BattleHelper.DamageType.Dark, 0, BattleHelper.ContactLevel.Contact);
            if (!hasStatus)
            {
                caller.InflictEffect(caller.curTarget, new Effect(Effect.EffectType.Poison, 1, 2));
            }
        }
        else
        {
            //Miss
            caller.InvokeMissEvents(caller.curTarget);
        }
    }
}

public class BM_Speartongue_Shroud : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Speartongue_Shroud;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.Self);

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        yield return StartCoroutine(caller.Spin(Vector3.up * 360, 0.25f));
        caller.InflictEffect(caller, new Effect(Effect.EffectType.Ethereal, 1, 2));
    }
}

public class BM_Speartongue_Stare : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Speartongue_Stare;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemy);

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        if (!BattleControl.Instance.EntityValid(caller.curTarget))
        {
            caller.curTarget = null;
        }

        yield return StartCoroutine(caller.Spin(Vector3.up * 360, 1f));

        List<BattleEntity> targets = BattleControl.Instance.GetEntitiesSorted(caller, GetBaseTarget());

        if (caller.GetAttackHit(caller.curTarget, BattleHelper.DamageType.Light))
        {
            caller.DealDamage(caller.curTarget, 4, BattleHelper.DamageType.Light, 0, BattleHelper.ContactLevel.Infinite);
        }
        else
        {
            //Miss
            caller.InvokeMissEvents(caller.curTarget);
        }
    }
}

public class BE_Chaintail : BattleEntity
{
    public override void Initialize()
    {
        moveset = new List<Move> { gameObject.AddComponent<BM_Chaintail_ShockClaw>(), gameObject.AddComponent<BM_Chaintail_TailWhip>(), gameObject.AddComponent<BM_Chaintail_Hard_PowerFlash>() };

        base.Initialize();
    }

    public override void ChooseMoveInternal()
    {
        if (BattleControl.Instance.GetCurseLevel() > 0)
        {
            currMove = moveset[(posId + BattleControl.Instance.turnCount - 1) % 3];
        }
        else
        {
            currMove = moveset[(posId + BattleControl.Instance.turnCount - 1) % 2];
        }
        BasicTargetChooser();
    }
}

public class BM_Chaintail_ShockClaw : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Chaintail_ShockClaw;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemy);

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        if (!BattleControl.Instance.EntityValid(caller.curTarget))
        {
            caller.curTarget = null;
        }

        if (caller.curTarget != null)
        {
            Vector3 itpos = Vector3.negativeInfinity;
            bool backflag = false;
            if (!BattleControl.Instance.IsFrontmostLow(caller, caller.curTarget))
            {
                itpos = BattleControl.Instance.GetFrontmostLow(caller).transform.position + Vector3.back * 0.5f;
                backflag = true;
            }

            //Debug.Log(itpos);

            Vector3 tpos = caller.curTarget.transform.position + ((caller.width / 2) + (caller.curTarget.width / 2)) * Vector3.right;

            if (backflag)
            {
                yield return StartCoroutine(caller.Move(itpos));
                yield return StartCoroutine(caller.Move(tpos));
            }
            else
            {
                yield return StartCoroutine(caller.Move(tpos));
            }

            yield return StartCoroutine(caller.Spin(Vector3.up * 360, 0.25f));
            if (caller.GetAttackHit(caller.curTarget, BattleHelper.DamageType.Air))
            {
                bool hasStatus = caller.curTarget.HasStatus();
                if (BattleControl.Instance.GetCurseLevel() > 0)
                {
                    caller.DealDamage(caller.curTarget, 5, BattleHelper.DamageType.Air, (ulong)(BattleHelper.DamageProperties.EPLossTwoToOne), BattleHelper.ContactLevel.Contact);
                }
                else
                {
                    caller.DealDamage(caller.curTarget, 5, BattleHelper.DamageType.Air, 0, BattleHelper.ContactLevel.Contact);
                }
                if (!hasStatus)
                {
                    caller.InflictEffect(caller.curTarget, new Effect(Effect.EffectType.Paralyze, 1, 2));
                }
            }
            else
            {
                caller.InvokeMissEvents(caller.curTarget);
            }
        }

        yield return StartCoroutine(caller.Move(caller.homePos));
    }
}

public class BM_Chaintail_TailWhip : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Chaintail_TailWhip;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemy);

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        if (!BattleControl.Instance.EntityValid(caller.curTarget))
        {
            caller.curTarget = null;
        }

        if (caller.curTarget != null)
        {
            Vector3 itpos = Vector3.negativeInfinity;
            bool backflag = false;
            if (!BattleControl.Instance.IsFrontmostLow(caller, caller.curTarget))
            {
                itpos = BattleControl.Instance.GetFrontmostLow(caller).transform.position + Vector3.back * 0.5f;
                backflag = true;
            }

            //Debug.Log(itpos);

            Vector3 tpos = caller.curTarget.transform.position + ((caller.width / 2) + (caller.curTarget.width / 2)) * Vector3.right;

            if (backflag)
            {
                yield return StartCoroutine(caller.Move(itpos));
                yield return StartCoroutine(caller.Move(tpos));
            }
            else
            {
                yield return StartCoroutine(caller.Move(tpos));
            }

            yield return StartCoroutine(caller.Spin(Vector3.up * 360, 0.5f));
            if (caller.GetAttackHit(caller.curTarget, 0))
            {
                caller.DealDamage(caller.curTarget, 9, 0, 0, BattleHelper.ContactLevel.Contact);
            }
            else
            {
                caller.InvokeMissEvents(caller.curTarget);
            }
        }

        yield return StartCoroutine(caller.Move(caller.homePos));
    }
}

public class BM_Chaintail_Hard_PowerFlash : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Chaintail_Hard_PowerFlash;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemy);

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        if (!BattleControl.Instance.EntityValid(caller.curTarget))
        {
            caller.curTarget = null;
        }

        if (caller.curTarget != null)
        {
            Vector3 itpos = Vector3.negativeInfinity;
            bool backflag = false;
            if (!BattleControl.Instance.IsFrontmostLow(caller, caller.curTarget))
            {
                itpos = BattleControl.Instance.GetFrontmostLow(caller).transform.position + Vector3.back * 0.5f;
                backflag = true;
            }

            //Debug.Log(itpos);

            Vector3 tpos = caller.curTarget.transform.position + ((caller.width / 2) + (caller.curTarget.width / 2)) * Vector3.right;

            if (backflag)
            {
                yield return StartCoroutine(caller.Move(itpos));
                yield return StartCoroutine(caller.Move(tpos));
            }
            else
            {
                yield return StartCoroutine(caller.Move(tpos));
            }

            yield return StartCoroutine(caller.Spin(Vector3.up * 360, 0.5f));
            if (caller.GetAttackHit(caller.curTarget, BattleHelper.DamageType.Light))
            {
                caller.DealDamage(caller.curTarget, 5, BattleHelper.DamageType.Light, 0, BattleHelper.ContactLevel.Contact);
                caller.InflictEffect(caller, new Effect(Effect.EffectType.Supercharge, 1, 3));
            }
            else
            {
                caller.InvokeMissEvents(caller.curTarget);
            }
        }

        yield return StartCoroutine(caller.Move(caller.homePos));
    }
}

public class BE_Sawcrest : BattleEntity
{
    public bool sawActive;

    int counterCount;
    public override void Initialize()
    {
        moveset = new List<Move> { gameObject.AddComponent<BM_Shared_Bite>(), gameObject.AddComponent<BM_Sawcrest_SawRush>(), gameObject.AddComponent<BM_Sawcrest_CounterSawToggle>(), gameObject.AddComponent<BM_Sawcrest_Hard_DeepCut>(), gameObject.AddComponent<BM_Sawcrest_Hard_CounterRevUp>() };

        base.Initialize();
    }

    public void SetSawState(bool state)
    {
        sawActive = state;

        //debuggish/prototypeish way of doing things
        SpriteRenderer s = subObject.GetComponentInChildren<SpriteRenderer>();

        if (sawActive)
        {
            s.color = new Color(0.75f, 0.35f, 0.2f);
        } else
        {
            s.color = new Color(0.6f, 0.5f, 0.2f);
        }
    }

    public override void SetEncounterVariables(string variable)
    {
        if (variable != null && variable.Contains("active"))
        {
            sawActive = true;
        }
        SetSawState(sawActive);
    }

    public override void ChooseMoveInternal()
    {
        //also reset here in case something weird happens
        counterCount = 0;

        if (sawActive)
        {
            if (BattleControl.Instance.GetCurseLevel() <= 0)
            {
                currMove = moveset[1];
            }
            else
            {
                currMove = moveset[(posId + BattleControl.Instance.turnCount - 1) % 3 == 0 ? 3 : 1];
            }
        }
        else
        {
            currMove = moveset[0];
        }

        BasicTargetChooser();
    }

    public override void TryContactHazard(BattleEntity target, BattleHelper.ContactLevel contact, BattleHelper.DamageType type, int damage)
    {
        EnvironmentalContactHazards(target, contact, type, damage);

        if (!sawActive)
        {
            return;
        }

        if (target.CanTriggerContactHazard(contact, type, damage))
        {
            //do

            //Check for contact hazard immunity list
            //(prevents multihits on the same target from hurting multiple times)
            //(does not prevent multitarget moves from doing the same!)

            if (target.contactImmunityList.Contains(posId))
            {
                return;
            }

            if (contact <= BattleHelper.ContactLevel.Contact)
            {
                DealDamage(target, 4, BattleHelper.DamageType.Normal, (ulong)BattleHelper.DamageProperties.StandardContactHazard, BattleHelper.ContactLevel.Contact);
                target.contactImmunityList.Add(posId);
            }
        }
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
    public override bool ReactToEvent(BattleEntity target, BattleHelper.Event e, int previousReactions)
    {
        if (e == BattleHelper.Event.Hurt && target == this && counterCount <= 0)
        {
            counterCount++;

            //which move to do?
            if (sawActive || BattleControl.Instance.GetCurseLevel() <= 0)
            {
                BattleControl.Instance.AddReactionMoveEvent(this, target.lastAttacker, moveset[2]);
            } else
            {
                BattleControl.Instance.AddReactionMoveEvent(this, target.lastAttacker, moveset[4]);
            }

            return true;
        }

        return false;
    }
}

public class BM_Sawcrest_SawRush : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Sawcrest_SawRush;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemy);

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        //get list of entities to pass through
        List<BattleEntity> targets = BattleControl.Instance.GetEntitiesSorted(caller, GetBaseTarget());

        //need 2 lists
        List<BattleEntity> targetsB = new List<BattleEntity>(targets);

        Vector3 target = transform.position + Vector3.left * 16f;

        //doing multiple things at once
        //Reuses Luna's dash through code with some changes

        bool move = true;
        IEnumerator MoveTracked()
        {
            yield return caller.Move(target, caller.entitySpeed);
            move = false;
        }

        StartCoroutine(MoveTracked());

        while (move)
        {
            //note: distance between player characters is 1.5f
            for (int i = 0; i < targets.Count; i++)
            {
                if (targets[i].homePos.x > caller.transform.position.x + 0.5f)
                {
                    //Debug.Log(targets[i]);
                    if (caller.GetAttackHit(targets[i], 0))
                    {
                        caller.DealDamage(targets[i], 4, BattleHelper.DamageType.Normal, 0, BattleHelper.ContactLevel.Weapon);
                    }
                    else
                    {
                        caller.InvokeMissEvents(targets[i]);
                    }
                    targets.RemoveAt(i);
                    i--;
                }
            }
            for (int i = 0; i < targetsB.Count; i++)
            {
                if (targetsB[i].homePos.x > caller.transform.position.x - 0.5f)
                {
                    //Debug.Log(targets[i]);
                    if (caller.GetAttackHit(targetsB[i], 0))
                    {
                        caller.DealDamage(targetsB[i], 4, BattleHelper.DamageType.Normal, 0, BattleHelper.ContactLevel.Weapon);
                    }
                    else
                    {
                        caller.InvokeMissEvents(targetsB[i]);
                    }
                    targetsB.RemoveAt(i);
                    i--;
                }
            }
            yield return null;
        }

        yield return StartCoroutine(caller.Move(caller.homePos));
    }
}

public class BM_Sawcrest_Hard_DeepCut : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Sawcrest_Hard_DeepCut;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemy);


    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        if (!BattleControl.Instance.EntityValid(caller.curTarget))
        {
            caller.curTarget = null;
        }

        if (caller.curTarget != null)
        {
            Vector3 itpos = Vector3.negativeInfinity;
            bool backflag = false;
            if (!BattleControl.Instance.IsFrontmostLow(caller, caller.curTarget))
            {
                itpos = BattleControl.Instance.GetFrontmostLow(caller).transform.position + Vector3.back * 0.5f;
                backflag = true;
            }

            //Debug.Log(itpos);

            Vector3 tpos = caller.curTarget.transform.position + ((caller.width / 2) + (caller.curTarget.width / 2)) * Vector3.right;

            if (backflag)
            {
                yield return StartCoroutine(caller.Move(itpos));
                yield return StartCoroutine(caller.Move(tpos));
            }
            else
            {
                yield return StartCoroutine(caller.Move(tpos));
            }

            if (caller.GetAttackHit(caller.curTarget, 0))
            {
                yield return StartCoroutine(caller.Squish(0.067f, 0.2f));

                caller.DealDamage(caller.curTarget, 9, BattleHelper.DamageType.Normal, 0, BattleHelper.ContactLevel.Weapon);
                caller.InflictEffect(caller.curTarget, new Effect(Effect.EffectType.Soulbleed, 1, 3));
                StartCoroutine(caller.RevertScale(0.1f));
            }
            else
            {
                caller.InvokeMissEvents(caller.curTarget);
            }
        }

        yield return StartCoroutine(caller.Move(caller.homePos));
    }
}

public class BM_Sawcrest_CounterSawToggle : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Sawcrest_CounterSawToggle;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.Self);

    public override IEnumerator ExecuteOutOfTurn(BattleEntity caller, BattleEntity target, int level = 1)
    {
        yield return StartCoroutine(caller.Spin(Vector3.up * 360, 0.5f));
        if (caller is BE_Sawcrest sc)
        {
            sc.SetSawState(!sc.sawActive);
        }
    }
}

public class BM_Sawcrest_Hard_CounterRevUp : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Sawcrest_Hard_CounterRevUp;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemy);

    public override IEnumerator ExecuteOutOfTurn(BattleEntity caller, BattleEntity target, int level = 1)
    {
        caller.curTarget = target;
        if (!BattleControl.Instance.EntityValid(caller.curTarget))
        {
            caller.curTarget = null;
        }

        yield return StartCoroutine(caller.Spin(Vector3.up * 360, 0.5f));
        if (caller is BE_Sawcrest sc)
        {
            sc.SetSawState(!sc.sawActive);
        }


        if (caller.curTarget != null)
        {
            if (caller.GetAttackHit(caller.curTarget, BattleHelper.DamageType.Dark))
            {
                caller.DealDamage(caller.curTarget, 2, BattleHelper.DamageType.Dark, 0, BattleHelper.ContactLevel.Infinite);
            }
            else
            {
                caller.InvokeMissEvents(caller.curTarget);
            }
        }

        if (caller.hp == 0)
        {
            caller.SetAnimation("dead");
            yield return StartCoroutine(caller.DefaultDeathEvent());
            yield break;
        }
    }
}

public class BE_Coiler : BattleEntity
{
    int counterCount;
    public override void Initialize()
    {
        moveset = new List<Move> { gameObject.AddComponent<BM_Coiler_Slap>(), gameObject.AddComponent<BM_Coiler_Charge>(), gameObject.AddComponent<BM_Coiler_ElectroStorm>(), gameObject.AddComponent<BM_Coiler_Hard_CounterRollerShell>() };

        base.Initialize();
    }

    public override void ChooseMoveInternal()
    {
        //also reset here in case something weird happens
        counterCount = 0;


        if (GetEntityProperty(BattleHelper.EntityProperties.StateCharge))
        {
            SetEntityProperty(BattleHelper.EntityProperties.StateCharge, false);
            currMove = moveset[2];
        }
        else
        {
            currMove = moveset[(posId + BattleControl.Instance.turnCount - 1) % 2];
        }

        BasicTargetChooser();
    }

    public override void TryContactHazard(BattleEntity target, BattleHelper.ContactLevel contact, BattleHelper.DamageType type, int damage)
    {
        EnvironmentalContactHazards(target, contact, type, damage);

        if (!GetEntityProperty(BattleHelper.EntityProperties.StateCharge))
        {
            return;
        }

        if (target.CanTriggerContactHazard(contact, type, damage))
        {
            //do

            //Check for contact hazard immunity list
            //(prevents multihits on the same target from hurting multiple times)
            //(does not prevent multitarget moves from doing the same!)

            if (target.contactImmunityList.Contains(posId))
            {
                return;
            }

            //Weapon range contact hazard :)
            if (contact <= BattleHelper.ContactLevel.Weapon)
            {
                DealDamage(target, 4, BattleHelper.DamageType.Air, (ulong)BattleHelper.DamageProperties.StandardContactHazard, BattleHelper.ContactLevel.Contact);
                target.contactImmunityList.Add(posId);
            }
        }
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
    public override bool ReactToEvent(BattleEntity target, BattleHelper.Event e, int previousReactions)
    {
        if (BattleControl.Instance.GetCurseLevel() <= 0 || !GetEntityProperty(BattleHelper.EntityProperties.StateCharge))
        {
            return false;
        }

        if (e == BattleHelper.Event.Hurt && target == this && counterCount <= 0)
        {
            counterCount++;
            BattleControl.Instance.AddReactionMoveEvent(this, target.lastAttacker, moveset[3]);
            return true;
        }

        return false;
    }
}

public class BM_Coiler_Slap : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Coiler_Slap;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemy);

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        if (!BattleControl.Instance.EntityValid(caller.curTarget))
        {
            caller.curTarget = null;
        }

        if (caller.curTarget != null)
        {
            Vector3 itpos = Vector3.negativeInfinity;
            bool backflag = false;
            if (!BattleControl.Instance.IsFrontmostLow(caller, caller.curTarget))
            {
                itpos = BattleControl.Instance.GetFrontmostLow(caller).transform.position + Vector3.back * 0.5f;
                backflag = true;
            }

            //Debug.Log(itpos);

            Vector3 tpos = caller.curTarget.transform.position + ((caller.width / 2) + (caller.curTarget.width / 2)) * Vector3.right;

            if (backflag)
            {
                yield return StartCoroutine(caller.Move(itpos));
                yield return StartCoroutine(caller.Move(tpos));
            }
            else
            {
                yield return StartCoroutine(caller.Move(tpos));
            }

            yield return StartCoroutine(caller.Spin(Vector3.up * 360, 0.25f));
            if (caller.GetAttackHit(caller.curTarget, 0))
            {
                caller.DealDamage(caller.curTarget, 5, BattleHelper.DamageType.Normal, 0, BattleHelper.ContactLevel.Contact);
            }
            else
            {
                caller.InvokeMissEvents(caller.curTarget);
            }
        }

        yield return StartCoroutine(caller.Move(caller.homePos));
    }
}

public class BM_Coiler_Charge : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Coiler_Charge;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.Self);

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        yield return StartCoroutine(caller.Spin(Vector3.up * 360, 0.25f));
        caller.SetEntityProperty(BattleHelper.EntityProperties.StateCharge);
    }
}

public class BM_Coiler_ElectroStorm : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Coiler_ElectroStorm;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemy);

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        if (!BattleControl.Instance.EntityValid(caller.curTarget))
        {
            caller.curTarget = null;
        }

        yield return StartCoroutine(caller.Spin(Vector3.up * 360, 1f));

        List<BattleEntity> targets = BattleControl.Instance.GetEntitiesSorted(caller, GetBaseTarget());

        foreach (BattleEntity t in targets)
        {
            if (caller.GetAttackHit(t, BattleHelper.DamageType.Air))
            {
                caller.DealDamage(t, 8, BattleHelper.DamageType.Air, 0, BattleHelper.ContactLevel.Infinite);
                if (BattleControl.Instance.GetCurseLevel() > 0)
                {
                    caller.InflictEffect(t, new Effect(Effect.EffectType.ArcDischarge, 1, 3));
                }
            }
            else
            {
                //Miss
                caller.InvokeMissEvents(t);
            }
        }
    }
}

public class BM_Coiler_Hard_CounterRollerShell : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Coiler_Hard_CounterRollerShell;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemy);

    public override IEnumerator ExecuteOutOfTurn(BattleEntity caller, BattleEntity target, int level = 1)
    {
        caller.curTarget = target;
        if (!BattleControl.Instance.EntityValid(caller.curTarget))
        {
            caller.curTarget = null;
        }

        if (caller.curTarget != null)
        {
            Vector3 itpos = Vector3.negativeInfinity;
            bool backflag = false;
            if (!BattleControl.Instance.IsFrontmostLow(caller, caller.curTarget))
            {
                itpos = BattleControl.Instance.GetFrontmostLow(caller).transform.position + Vector3.back * 0.5f;
                backflag = true;
            }

            //Debug.Log(itpos);

            Vector3 tpos = caller.curTarget.transform.position + ((caller.width / 2) + (caller.curTarget.width / 2)) * Vector3.right;

            if (backflag)
            {
                yield return StartCoroutine(caller.Move(itpos, caller.entitySpeed * 2));
                yield return StartCoroutine(caller.Move(tpos, caller.entitySpeed * 2));
            }
            else
            {
                yield return StartCoroutine(caller.Move(tpos, caller.entitySpeed * 2));
            }

            if (caller.GetAttackHit(caller.curTarget, 0))
            {
                yield return StartCoroutine(caller.Squish(0.067f, 0.2f));
                caller.DealDamage(caller.curTarget, 4, BattleHelper.DamageType.Normal, 0, BattleHelper.ContactLevel.Contact);
                StartCoroutine(caller.RevertScale(0.1f));
            }
            else
            {
                caller.InvokeMissEvents(caller.curTarget);
            }
        }

        yield return StartCoroutine(caller.Move(caller.homePos));
    }
}

public class BE_Drillbeak : BattleEntity
{
    int counterCount;

    public override void Initialize()
    {
        moveset = new List<Move> { gameObject.AddComponent<BM_Drillbeak_Drill>(), gameObject.AddComponent<BM_Drillbeak_Hard_DreadStab>() };

        base.Initialize();
    }

    public override void ChooseMoveInternal()
    {
        if (moveset.Count == 0)
        {
            currMove = null;
        }
        else
        {
            if (BattleControl.Instance.GetCurseLevel() > 0)
            {
                currMove = moveset[(posId + BattleControl.Instance.turnCount - 1) % 3 == 0 ? 1 : 0];
            } else
            {
                currMove = moveset[0];
            }
        }

        //Special case targetting: Make sure you don't target an ethereal enemy
        //If it is impossible not to target an ethereal enemy, give up and choose normally
        List<BattleEntity> bl = BattleControl.Instance.GetEntitiesSorted(this, currMove.GetTargetArea(this));
        bl = bl.FindAll((e) => (e.posId != posId && !e.HasEffect(Effect.EffectType.Ethereal)));

        if (bl.Count == 0)
        {
            BasicTargetChooser();
        }
        else
        {
            curTarget = bl[BattleControl.Instance.GetPsuedoRandom(bl.Count, posId + 6)];
        }
    }

    public override IEnumerator DoEvent(BattleHelper.Event eventID)
    {
        yield return StartCoroutine(DefaultEventHandler_Flying(eventID));
    }
}

public class BM_Drillbeak_Drill : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Drillbeak_Drill;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemy);

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        //fly back up        
        yield return StartCoroutine(caller.FlyingFlyBackUp());

        //Debug.Log(caller.id + " jump");
        if (!BattleControl.Instance.EntityValid(caller.curTarget))
        {
            caller.curTarget = null;
        }

        if (caller.curTarget != null)
        {
            Vector3 offset = Vector3.right * 3f + Vector3.up * 2f;
            Vector3 tposA = caller.curTarget.transform.position + offset;
            Vector3 tposend = caller.curTarget.transform.position + ((caller.width / 2) + (caller.curTarget.width / 2)) * Vector3.right + (caller.curTarget.height / 2) * Vector3.up;


            bool backFlag = false;

            if (!BattleControl.Instance.IsFrontmostLow(caller, caller.curTarget))
            {
                tposA = BattleControl.Instance.GetFrontmostLow(caller).transform.position + offset + Vector3.back * 0.5f;
                backFlag = true;
            }

            Vector3 tposmid = (tposA + tposend) / 2 + Vector3.down * 0.5f;

            float dist = tposA.x - tposend.x - 0.25f;

            yield return StartCoroutine(caller.Move(tposA));

            Vector3[] positions = new Vector3[] { tposA, tposmid, tposend };

            if (caller.GetAttackHit(caller.curTarget, 0))
            {
                yield return StartCoroutine(caller.FollowBezierCurve(backFlag ? 0.4f : 0.3f, (float a) => MainManager.EasingQuadratic(a, -0.2f), positions));

                //yield return StartCoroutine(caller.Squish(0.067f, 0.2f));

                caller.DealDamage(caller.curTarget, 3, BattleHelper.DamageType.Normal, 0, BattleHelper.ContactLevel.Contact);
                for (int i = 0; i < 3; i++)
                {
                    yield return new WaitForSeconds(0.2f);
                    caller.DealDamage(caller.curTarget, 3, BattleHelper.DamageType.Normal, 0, BattleHelper.ContactLevel.Contact);
                }

                //StartCoroutine(caller.RevertScale(0.1f));
            }
            else
            {
                yield return StartCoroutine(caller.FollowBezierCurve(0.15f * dist, (float a) => MainManager.EasingQuadratic(a, -0.2f), 1.25f, positions));
                caller.InvokeMissEvents(caller.curTarget);
            }
        }

        yield return StartCoroutine(caller.Move(caller.homePos));
    }
}

public class BM_Drillbeak_Hard_DreadStab : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Drillbeak_Hard_DreadStab;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemy);

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        //fly back up        
        yield return StartCoroutine(caller.FlyingFlyBackUp());

        //Debug.Log(caller.id + " jump");
        if (!BattleControl.Instance.EntityValid(caller.curTarget))
        {
            caller.curTarget = null;
        }

        if (caller.curTarget != null)
        {
            Vector3 offset = Vector3.right * 3f + Vector3.up * 2f;
            Vector3 tposA = caller.curTarget.transform.position + offset;
            Vector3 tposend = caller.curTarget.transform.position + ((caller.width / 2) + (caller.curTarget.width / 2)) * Vector3.right + (caller.curTarget.height / 2) * Vector3.up;


            bool backFlag = false;

            if (!BattleControl.Instance.IsFrontmostLow(caller, caller.curTarget))
            {
                tposA = BattleControl.Instance.GetFrontmostLow(caller).transform.position + offset + Vector3.back * 0.5f;
                backFlag = true;
            }

            Vector3 tposmid = (tposA + tposend) / 2 + Vector3.down * 0.5f;

            float dist = tposA.x - tposend.x - 0.25f;

            yield return StartCoroutine(caller.Move(tposA));

            Vector3[] positions = new Vector3[] { tposA, tposmid, tposend };

            if (caller.GetAttackHit(caller.curTarget, 0))
            {
                yield return StartCoroutine(caller.FollowBezierCurve(backFlag ? 0.4f : 0.3f, (float a) => MainManager.EasingQuadratic(a, -0.2f), positions));

                //yield return StartCoroutine(caller.Squish(0.067f, 0.2f));

                caller.DealDamage(caller.curTarget, 8, BattleHelper.DamageType.Dark, 0, BattleHelper.ContactLevel.Contact);
                caller.InflictEffect(caller.curTarget, new Effect(Effect.EffectType.Dread, 1, 2));

                //StartCoroutine(caller.RevertScale(0.1f));
            }
            else
            {
                yield return StartCoroutine(caller.FollowBezierCurve(0.15f * dist, (float a) => MainManager.EasingQuadratic(a, -0.2f), 1.25f, positions));
                caller.InvokeMissEvents(caller.curTarget);
            }
        }

        yield return StartCoroutine(caller.Move(caller.homePos));
    }
}