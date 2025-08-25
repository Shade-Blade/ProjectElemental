using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BE_Spikeflake : BattleEntity, ICleaveUser
{
    bool didSplit = false;
    public override void Initialize()
    {
        //This is pretty sus
        moveset = new List<Move> { gameObject.AddComponent<BM_Spikeflake_SnowflakeStatic>(), gameObject.AddComponent<BM_Spikeflake_IceSpikes>(), gameObject.AddComponent<BM_RigidSlime_Cleave>(), gameObject.AddComponent<BM_Spikeflake_Hard_IceWind>() };
        base.Initialize();
    }

    public void SetCleave()
    {
        didSplit = true;
    }

    public override void ChooseMoveInternal()
    {
        if (moveset.Count == 0)
        {
            currMove = null;
        }
        else
        {
            //note: posId >= 0 for enemies
            if (BattleControl.Instance.GetCurseLevel() > 0)
            {
                currMove = moveset[(posId + BattleControl.Instance.turnCount - 1) % moveset.Count];
            }
            else
            {
                currMove = moveset[(posId + BattleControl.Instance.turnCount - 1) % (moveset.Count - 1)];
            }

            if (currMove == moveset[2] && (didSplit || BattleControl.Instance.FindUnoccupiedID(false, true) >= 4 || hp / 2 <= 0))
            {
                currMove = moveset[1];
            }
        }

        BasicTargetChooser();
    }

    public override void TryContactHazard(BattleEntity target, BattleHelper.ContactLevel contact, BattleHelper.DamageType type, int damage)
    {
        EnvironmentalContactHazards(target, contact, type, damage);

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
                DealDamage(target, 3, BattleHelper.DamageType.Light, (ulong)BattleHelper.DamageProperties.StandardContactHazard, BattleHelper.ContactLevel.Contact);
                target.contactImmunityList.Add(posId);
            }
        }
    }
}

public class BM_Spikeflake_SnowflakeStatic : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Spikeflake_SnowflakeStatic;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemy);

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        if (!BattleControl.Instance.EntityValid(caller.curTarget))
        {
            caller.curTarget = null;
        }

        yield return StartCoroutine(caller.SpinHeavy(Vector3.up * 360, 0.5f));

        caller.InflictEffect(caller, new Effect(Effect.EffectType.Supercharge, 1, 3));

        List<BattleEntity> targets = BattleControl.Instance.GetEntitiesSorted(caller, GetBaseTarget());
        foreach (BattleEntity t in targets)
        {
            if (caller.GetAttackHit(t, BattleHelper.DamageType.Dark))
            {
                t.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.ReverseSquish);
                caller.DealDamage(t, 4, BattleHelper.DamageType.Air, 0, BattleHelper.ContactLevel.Infinite);
            }
            else
            {
                //Miss
                caller.InvokeMissEvents(t);
            }
        }
    }
}

public class BM_Spikeflake_IceSpikes : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Spikeflake_IceSpikes;

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
                yield return StartCoroutine(caller.MoveEasing(itpos, (e) => MainManager.EasingOut(e)));
                yield return StartCoroutine(caller.MoveEasing(tpos, (e) => MainManager.EasingIn(e)));
            }
            else
            {
                yield return StartCoroutine(caller.MoveEasing(tpos, (e) => MainManager.EasingOutIn(e)));
            }

            yield return StartCoroutine(caller.SpinHeavy(Vector3.up * 360, 0.25f));
            if (caller.GetAttackHit(caller.curTarget, BattleHelper.DamageType.Light))
            {
                for (int i = 0; i < 3; i++)
                {
                    caller.curTarget.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.ReverseSquish);
                    caller.DealDamage(caller.curTarget, 2, BattleHelper.DamageType.Light, 0, BattleHelper.ContactLevel.Contact);
                    if (i < 2)
                    {
                        yield return StartCoroutine(caller.SpinHeavy(Vector3.up * 360, 0.25f));
                    }
                }
            }
            else
            {
                caller.InvokeMissEvents(caller.curTarget);
            }
        }

        yield return StartCoroutine(caller.MoveEasing(caller.homePos, (e) => MainManager.EasingOutIn(e)));
    }
}

public class BM_Spikeflake_Hard_IceWind : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Spikeflake_Hard_IceWind;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemy);

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        if (!BattleControl.Instance.EntityValid(caller.curTarget))
        {
            caller.curTarget = null;
        }

        yield return StartCoroutine(caller.SpinHeavy(Vector3.up * 360, 0.5f));

        caller.InflictEffect(caller, new Effect(Effect.EffectType.Supercharge, 1, 3));

        List<BattleEntity> targets = BattleControl.Instance.GetEntitiesSorted(caller, GetBaseTarget());
        for (int i = 0; i < 2; i++)
        {
            foreach (BattleEntity t in targets)
            {
                if (caller.GetAttackHit(t, BattleHelper.DamageType.Dark))
                {
                    t.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.ReverseSquish);
                    caller.DealDamage(t, 3, BattleHelper.DamageType.Light, 0, BattleHelper.ContactLevel.Infinite);
                    if (i == 1)
                    {
                        caller.InflictEffect(t, new Effect(Effect.EffectType.Freeze, 1, 3));
                    }
                }
                else
                {
                    //Miss
                    caller.InvokeMissEvents(t);
                }
            }

            if (i < 1)
            {
                yield return StartCoroutine(caller.SpinHeavy(Vector3.up * 360, 0.25f));
            }
        }
    }
}

public class BE_Mirrorwing : BattleEntity
{
    int counterCount;
    public override void Initialize()
    {
        moveset = new List<Move> { gameObject.AddComponent<BM_Shared_DoubleSwoop>(), gameObject.AddComponent<BM_Mirrorwing_LuminousSong>(), gameObject.AddComponent<BM_Mirrorwing_CounterBlast>() };

        base.Initialize();
    }

    public override void ChooseMoveInternal()
    {
        counterCount = 0;

        if (moveset.Count == 0)
        {
            currMove = null;
        }
        else
        {
            if (GetEntityProperty(BattleHelper.EntityProperties.Grounded))
            {
                currMove = moveset[1];
            }
            else
            {
                currMove = moveset[(posId + BattleControl.Instance.turnCount + 1) % 2];
            }
        }

        BasicTargetChooser(2, 3);
    }

    public override IEnumerator DoEvent(BattleHelper.Event eventID)
    {
        yield return StartCoroutine(DefaultEventHandler_Flying(eventID));
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
        if (!GetEntityProperty(BattleHelper.EntityProperties.Airborne))
        {
            return false;
        }

        if (e == BattleHelper.Event.Hurt && target.posId >= 0 && !target.GetEntityProperty(BattleHelper.EntityProperties.NoTarget) && counterCount <= 0)
        {
            counterCount++;
            //reaction system doesn't have 2 things
            //but I can just get target.lastAttacker like this anyway
            BattleControl.Instance.AddReactionMoveEvent(this, target, moveset[2]);
            return true;
        }

        return false;
    }
}

public class BM_Mirrorwing_LuminousSong : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Mirrorwing_LuminousSong;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveAllyNotSelf);

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        if (!BattleControl.Instance.EntityValid(caller.curTarget))
        {
            caller.curTarget = null;
        }

        yield return StartCoroutine(caller.FlyingFlyBackUp());
        yield return StartCoroutine(caller.SpinHeavy(Vector3.up * 360, 0.5f));

        List<BattleEntity> targets = BattleControl.Instance.GetEntitiesSorted(caller, GetBaseTarget());

        caller.InflictEffect(caller, new Effect(Effect.EffectType.Illuminate, 1, 3));
        if (caller.curTarget != null)
        {
            caller.InflictEffect(caller.curTarget, new Effect(Effect.EffectType.Illuminate, 1, 3));
        }
    }
}

public class BM_Mirrorwing_CounterBlast : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Mirrorwing_CounterBlast;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemy);

    public override IEnumerator ExecuteOutOfTurn(BattleEntity caller, BattleEntity target, int level = 1)
    {
        BattleEntity attackTarget = target.lastAttacker;

        //fizzle
        //possible if knocked down (so it gets added to the counter list but it isn't valid anymore
        if (!caller.GetEntityProperty(BattleHelper.EntityProperties.Airborne))
        {
            yield break;
        }

        //fly back up        
        //yield return StartCoroutine(caller.FlyingFlyBackUp());

        yield return StartCoroutine(caller.SpinHeavy(Vector3.up * 360, 0.4f));
        caller.InflictEffect(target, new Effect(Effect.EffectType.Illuminate, 1, 3));
        if (BattleControl.Instance.GetCurseLevel() > 0)
        {
            caller.InflictEffectCapped(caller, new Effect(Effect.EffectType.AttackBoost, 1, Effect.INFINITE_DURATION), 8);
        }

        yield return StartCoroutine(caller.SpinHeavy(Vector3.up * 360, 0.4f));
        if (caller.GetAttackHit(attackTarget, BattleHelper.DamageType.Light))
        {
            attackTarget.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.Spin);
            caller.DealDamage(attackTarget, 4, BattleHelper.DamageType.Light, 0, BattleHelper.ContactLevel.Infinite);
        }
        else
        {
            //Miss
            caller.InvokeMissEvents(attackTarget);
        }
    }
}

public class BE_Beaconwing : BattleEntity
{
    int counterCount = 0;
    public override void Initialize()
    {
        moveset = new List<Move> { gameObject.AddComponent<BM_Beaconwing_SoftCharge>(), gameObject.AddComponent<BM_Beaconwing_LightBlast>(), gameObject.AddComponent<BM_Beaconwing_CounterProtect>() };

        base.Initialize();
    }

    public override void ChooseMoveInternal()
    {
        counterCount = 0;

        if (GetEntityProperty(BattleHelper.EntityProperties.StateCharge))
        {
            SetEntityProperty(BattleHelper.EntityProperties.StateCharge, false);
            currMove = moveset[1];
        } else
        {
            currMove = moveset[0];
        }

        SpecialTargetChooserFirst(TargetStrategy.TargetStrategyType.HighHP);
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
        if (e == BattleHelper.Event.Hurt && target.posId >= 0 && !target.GetEntityProperty(BattleHelper.EntityProperties.NoTarget) && counterCount <= 0)
        {
            counterCount++;
            BattleControl.Instance.AddReactionMoveEvent(this, target, moveset[2]);
            return true;
        }

        return false;
    }
}

public class BM_Beaconwing_CounterProtect : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Beaconwing_CounterProtect;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveAlly);

    public override IEnumerator ExecuteOutOfTurn(BattleEntity caller, BattleEntity target, int level = 1)
    {
        BattleEntity attackTarget = target.lastAttacker;

        //fly back up        
        //yield return StartCoroutine(caller.FlyingFlyBackUp());

        yield return StartCoroutine(caller.SpinHeavy(Vector3.up * 360, 0.4f));
        caller.InflictEffect(target, new Effect(Effect.EffectType.DefenseUp, 2, 3));
        if (BattleControl.Instance.GetCurseLevel() > 0)
        {
            caller.InflictEffectCapped(target, new Effect(Effect.EffectType.DefenseBoost, 1, Effect.INFINITE_DURATION), 8);
        }
    }
}

public class BM_Beaconwing_SoftCharge : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Beaconwing_SoftCharge;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.Self);

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        yield return StartCoroutine(caller.SpinHeavy(Vector3.up * 360, 0.25f));
        caller.SetEntityProperty(BattleHelper.EntityProperties.StateCharge);
        caller.InflictEffect(caller, new Effect(Effect.EffectType.Soften, 1, 2));
        caller.InflictEffect(caller, new Effect(Effect.EffectType.Focus, 3, Effect.INFINITE_DURATION));
        if (BattleControl.Instance.GetCurseLevel() > 0)
        {
            caller.InflictEffect(caller, new Effect(Effect.EffectType.DefenseBoost, 1, Effect.INFINITE_DURATION));
        }
    }
}

public class BM_Beaconwing_LightBlast : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Beaconwing_LightBlast;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemy);

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        if (!BattleControl.Instance.EntityValid(caller.curTarget))
        {
            caller.curTarget = null;
        }

        yield return StartCoroutine(caller.SpinHeavy(Vector3.up * 360, 1f));

        List<BattleEntity> targets = BattleControl.Instance.GetEntitiesSorted(caller, GetBaseTarget());

        foreach (BattleEntity t in targets)
        {
            if (caller.GetAttackHit(t, BattleHelper.DamageType.Light))
            {
                caller.DealDamage(t, 6, BattleHelper.DamageType.Light, 0, BattleHelper.ContactLevel.Infinite);
            }
            else
            {
                //Miss
                caller.InvokeMissEvents(t);
            }
        }
    }
}


public class BE_Harmonywing : BattleEntity
{
    int counterCount = 0;
    public override void Initialize()
    {
        moveset = new List<Move> { gameObject.AddComponent<BM_Honeywing_SpitHeal>(), gameObject.AddComponent<BM_Harmonywing_HealingSong>(), gameObject.AddComponent<BM_Harmonywing_Encore>(), gameObject.AddComponent<BM_Harmonywing_Hard_FinalMistRain>() };

        base.Initialize();
    }

    public override void ChooseMoveInternal()
    {
        counterCount = 0;
        if (moveset.Count == 0)
        {
            currMove = null;
        }
        else
        {
            if (GetEntityProperty(BattleHelper.EntityProperties.Grounded))
            {
                currMove = moveset[0];
            }
            else
            {
                currMove = moveset[(posId + BattleControl.Instance.turnCount - 1) % 2];
            }
        }

        BasicTargetChooser(2, 3);

        if (currMove = moveset[1])
        {
            SpecialTargetChooserFirst(TargetStrategy.TargetStrategyType.LowHP);
        }
    }

    public override IEnumerator DoEvent(BattleHelper.Event eventID)
    {
        yield return StartCoroutine(DefaultEventHandler_Flying(eventID));
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
        /*
        if (HasStatus(Status.StatusEffect.Slow))
        {
            return BattleControl.Instance.turnCount % (GetStatusEntry(Status.StatusEffect.Slow).potency + 1) == 1; //slow enemies can move on turn 1
        }
        */
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
    public override IEnumerator DefaultDeathEvent()
    {
        if (counterCount > 0)
        {
            yield break;
        }

        yield return StartCoroutine(base.DefaultDeathEvent());
    }

    public override bool ReactToEvent(BattleEntity target, BattleHelper.Event e, int previousReactions)
    {
        if (BattleControl.Instance.GetCurseLevel() <= 0)
        {
            return false;
        }

        //final effect
        if (BattleControl.Instance.GetCurseLevel() > 0 && (e == BattleHelper.Event.Death && target == this && counterCount <= 0))
        {
            counterCount++;
            BattleControl.Instance.AddReactionMoveEvent(this, target.lastAttacker, moveset[3]);
            return true;
        }


        if (GetEntityProperty(BattleHelper.EntityProperties.Airborne) && target != this && e == BattleHelper.Event.PostAction && target.posId >= 0 && counterCount <= 0)
        {
            //can't just do GetName().Contains if I ever add localization
            //but the internal index will not change in that case
            //Note 2: I can't make any enemy have "Song" in their real name (or for the move indices) or else things go off the rails
            if (target.currMove is EnemyMove em)
            {
                if (em.GetMoveIndex().ToString().Contains("Song"))
                {
                    counterCount++;
                    BattleControl.Instance.AddReactionMoveEvent(this, target.lastAttacker, moveset[2]);
                    return true;
                }
            }
        }

        return false;
    }
}

public class BM_Harmonywing_HealingSong : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Harmonywing_HealingSong;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveAllyNotSelf);

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        if (!BattleControl.Instance.EntityValid(caller.curTarget))
        {
            caller.curTarget = null;
        }

        yield return StartCoroutine(caller.FlyingFlyBackUp());
        yield return StartCoroutine(caller.SpinHeavy(Vector3.up * 360, 0.5f));

        List<BattleEntity> targets = BattleControl.Instance.GetEntitiesSorted(caller, GetBaseTarget());

        if (caller.curTarget != null)
        {
            caller.InflictEffect(caller.curTarget, new Effect(Effect.EffectType.MistWall, 1, 3));
            caller.curTarget.HealHealth(6);
        }
    }
}

public class BM_Harmonywing_Encore : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Harmonywing_Encore;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveAlly);

    public override IEnumerator ExecuteOutOfTurn(BattleEntity caller, BattleEntity target, int level = 1)
    {
        caller.curTarget = target;
        if (!BattleControl.Instance.EntityValid(caller.curTarget))
        {
            caller.curTarget = null;
        }

        yield return StartCoroutine(caller.SpinHeavy(Vector3.up * 360, 0.25f));

        List<BattleEntity> targets = BattleControl.Instance.GetEntitiesSorted(caller, GetBaseTarget());

        foreach (BattleEntity be in targets)
        {
            be.HealHealth(2);
        }

        if (caller.hp == 0)
        {
            caller.SetAnimation("dead");
            yield return StartCoroutine(caller.DefaultDeathEvent());
            yield break;
        }
    }
}

public class BM_Harmonywing_Hard_FinalMistRain : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Harmonywing_Hard_FinalMistRain;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveAlly);

    public override IEnumerator ExecuteOutOfTurn(BattleEntity caller, BattleEntity target, int level = 1)
    {
        caller.curTarget = target;
        if (!BattleControl.Instance.EntityValid(caller.curTarget))
        {
            caller.curTarget = null;
        }

        yield return StartCoroutine(caller.SpinHeavy(Vector3.up * 360, 0.5f));

        List<BattleEntity> targets = BattleControl.Instance.GetEntitiesSorted(caller, GetBaseTarget());

        foreach (BattleEntity be in targets)
        {
            if (be != caller)
            {
                caller.InflictEffect(be, new Effect(Effect.EffectType.MistWall, 1, 3));
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