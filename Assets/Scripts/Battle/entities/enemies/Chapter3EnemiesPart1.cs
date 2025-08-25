using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BE_Slime : BattleEntity
{
    public bool didSplit = false;
    int counterCount;
    public override void Initialize()
    {
        moveset = new List<Move> { gameObject.AddComponent<BM_Slime_Stomp>(), gameObject.AddComponent<BM_Slime_SplashStomp>(), gameObject.AddComponent<BM_Slime_CounterSplit>() };

        base.Initialize();
    }

    public override void ChooseMoveInternal()
    {
        //also reset here in case something weird happens
        counterCount = 0;

        currMove = moveset[(posId + BattleControl.Instance.turnCount - 1) % 2];

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
        if (e == BattleHelper.Event.Hurt && target == this && counterCount <= 0 && (!didSplit || BattleControl.Instance.GetCurseLevel() > 0))
        {
            //Can't because no space
            if (BattleControl.Instance.FindUnoccupiedID(false, true) >= 4)
            {
                return false;
            }

            //I am a split, so no
            if (transform.localScale.x < 1)
            {
                return false;
            }

            counterCount++;
            BattleControl.Instance.AddReactionMoveEvent(this, target.lastAttacker, moveset[2]);
            return true;
        }

        return false;
    }
}

public class BM_Slime_Stomp : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Slime_Stomp;

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
                caller.curTarget.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.Squish);
                caller.DealDamage(caller.curTarget, 3, BattleHelper.DamageType.Normal, 0, BattleHelper.ContactLevel.Contact);
                yield return StartCoroutine(caller.JumpHeavy(caller.homePos, 2, 0.5f, 0.15f));
            }
            else
            {
                caller.InvokeMissEvents(caller.curTarget);

                //extrapolate the move curve
                yield return StartCoroutine(caller.ExtrapolateJumpHeavy(spos, tpos, 2, 0.5f, -0.25f));
                yield return StartCoroutine(caller.MoveEasing(caller.homePos, (e) => MainManager.EasingOutIn(e)));
            }
        }

        yield return StartCoroutine(caller.MoveEasing(caller.homePos, (e) => MainManager.EasingOutIn(e)));
    }
}

public class BM_Slime_SplashStomp : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Slime_SplashStomp;

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

            yield return StartCoroutine(caller.JumpHeavy(tpos, 5, 0.8f, -0.25f));

            if (caller.GetAttackHit(caller.curTarget, BattleHelper.DamageType.Water))
            {
                caller.curTarget.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.Squish);
                caller.DealDamage(caller.curTarget, 3, BattleHelper.DamageType.Water, 0, BattleHelper.ContactLevel.Contact);
                yield return StartCoroutine(caller.JumpHeavy(caller.homePos, 2, 0.5f, 0.15f));
            }
            else
            {
                caller.InvokeMissEvents(caller.curTarget);

                //extrapolate the move curve
                yield return StartCoroutine(caller.ExtrapolateJumpHeavy(spos, tpos, 2, 0.5f, -0.25f));
                yield return StartCoroutine(caller.MoveEasing(caller.homePos, (e) => MainManager.EasingOutIn(e)));
            }
        }

        yield return StartCoroutine(caller.MoveEasing(caller.homePos, (e) => MainManager.EasingOutIn(e)));
    }
}

public class BM_Slime_CounterSplit : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Slime_CounterSplit;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.Self);

    public override IEnumerator ExecuteOutOfTurn(BattleEntity caller, BattleEntity target, int level = 1)
    {
        //fizzle
        if (BattleControl.Instance.FindUnoccupiedID(false, true) >= 4)
        {
            yield return new WaitForSeconds(0.5f);
            yield break;
        }

        if (BattleControl.Instance.GetCurseLevel() > 0)
        {
            caller.InflictEffect(caller, new Effect(Effect.EffectType.MistWall, 1, 2));
        }

        BE_Slime sl = (BE_Slime)caller;

        BE_Slime ns = (BE_Slime)(BattleControl.Instance.SummonEntity(BattleHelper.EntityID.Slime));

        if (sl != null)
        {
            sl.didSplit = true;
            ns.didSplit = true;
        }
        if (ns != null)
        {
            ns.transform.localScale = Vector3.one * 0.75f;
            ns.didSplit = true;
            ns.hp = caller.hp / 2;
            ns.statMultiplier = sl.statMultiplier;
            //ns.maxHP = caller.maxHP / 2;
            if (ns.hp == 0)
            {
                ns.hp = 1;
            }
            foreach (Effect e in caller.effects)
            {
                ns.ReceiveEffectForce(e);
            }

            ns.transform.position = caller.transform.position + Vector3.forward * -0.05f;
            yield return StartCoroutine(ns.JumpHeavy(ns.homePos, 0.6f, 0.2f, 1));
            yield return new WaitForSeconds(0.1f);
            ns.SetIdleAnimation(true);
            ns.StartIdle();
        }

        yield return new WaitForSeconds(0.5f);
    }
}

public class BE_Slimewalker : BattleEntity
{
    public override void Initialize()
    {
        moveset = new List<Move> { gameObject.AddComponent<BM_Shared_Slash>(), gameObject.AddComponent<BM_Slimewalker_WaterCannon>(), gameObject.AddComponent<BM_Slimewalker_Hard_SoftSplash>() };

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

        if (currMove == moveset[1])
        {
            //SpecialTargetChooserFirst(TargetStrategy.TargetStrategyType.Slimewalker);
            List<BattleEntity> bl = BattleControl.Instance.GetEntities(this, currMove.GetTargetArea(this));
            TargetStrategy strategy = new TargetStrategy(TargetStrategy.TargetStrategyType.FrontMost);
            bl.Sort((a, b) => strategy.selectorFunction(a, b));
            if (GetBerserkTarget() != null)
            {
                if (bl.Contains(curTarget))
                {
                    curTarget = GetBerserkTarget();
                }
            } else
            {
                //get target by damage check
                int damage = 0;
                curTarget = bl[0];

                foreach (BattleEntity be in bl)
                {
                    int d = DealDamageCalculation(be, 4, BattleHelper.DamageType.Water, 0);
                    if (d > damage)
                    {
                        damage = d;
                        curTarget = be;
                    }
                }
            }
        }
        else
        {
            BasicTargetChooser();
        }
    }
}

public class BM_Slimewalker_WaterCannon : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Slimewalker_WaterCannon;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemy);

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        if (!BattleControl.Instance.EntityValid(caller.curTarget))
        {
            caller.curTarget = null;
        }

        yield return StartCoroutine(caller.SpinHeavy(Vector3.up * 360, 1f));

        if (caller.curTarget != null)
        {
            if (caller.GetAttackHit(caller.curTarget, BattleHelper.DamageType.Water))
            {
                caller.curTarget.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.Spin);
                caller.DealDamage(caller.curTarget, 3, BattleHelper.DamageType.Water, 0, BattleHelper.ContactLevel.Infinite);
            }
            else
            {
                caller.InvokeMissEvents(caller.curTarget);
            }
        }
    }
}

public class BM_Slimewalker_Hard_SoftSplash : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Slimewalker_Hard_SoftSplash;

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

            if (caller.GetAttackHit(caller.curTarget, BattleHelper.DamageType.Water))
            {
                caller.curTarget.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.Squish);
                caller.DealDamage(caller.curTarget, 1, BattleHelper.DamageType.Water, 0, BattleHelper.ContactLevel.Contact);
                caller.InflictEffect(caller.curTarget, new Effect(Effect.EffectType.DefenseDown, 2, 3));
                yield return StartCoroutine(caller.JumpHeavy(caller.homePos, 2, 0.5f, 0.15f));
            }
            else
            {
                caller.InvokeMissEvents(caller.curTarget);

                //extrapolate the move curve
                yield return StartCoroutine(caller.ExtrapolateJumpHeavy(spos, tpos, 2, 0.5f, -0.25f));
                yield return StartCoroutine(caller.MoveEasing(caller.homePos, (e) => MainManager.EasingOutIn(e)));
            }
        }

        yield return StartCoroutine(caller.MoveEasing(caller.homePos, (e) => MainManager.EasingOutIn(e)));
    }
}

public class BE_Slimeworm : BattleEntity
{
    public override void Initialize()
    {
        moveset = new List<Move> { gameObject.AddComponent<BM_Slimeworm_Charge>(), gameObject.AddComponent<BM_Slimeworm_Mortar>(), gameObject.AddComponent<BM_Slimeworm_DeepMortar>() };

        base.Initialize();
    }

    public override void ChooseMoveInternal()
    {
        if (GetEntityProperty(BattleHelper.EntityProperties.StateCharge))
        {
            if (BattleControl.Instance.GetCurseLevel() > 0)
            {
                currMove = moveset[(BattleControl.Instance.turnCount + posId) % 4 > 1 ? 2 : 1];
            }
            else
            {
                currMove = moveset[1];
            }
            SetEntityProperty(BattleHelper.EntityProperties.StateCharge, false);
        }
        else
        {
            currMove = moveset[0];
        }

        BasicTargetChooser();
    }
}

public class BM_Slimeworm_Charge : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Slimeworm_Charge;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.Self);

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        if (!BattleControl.Instance.EntityValid(caller.curTarget))
        {
            caller.curTarget = null;
        }

        yield return StartCoroutine(caller.SpinHeavy(Vector3.up * 360, 0.5f));

        if (caller.curTarget != null)
        {
            caller.SetEntityProperty(BattleHelper.EntityProperties.StateCharge);
            if (BattleControl.Instance.GetCurseLevel() > 0)
            {
                caller.InflictEffect(caller, new Effect(Effect.EffectType.Focus, 2, Effect.INFINITE_DURATION));
                caller.InflictEffect(caller, new Effect(Effect.EffectType.Absorb, 2, Effect.INFINITE_DURATION));
                caller.HealHealth(2);
            }
            else
            {
                caller.InflictEffect(caller, new Effect(Effect.EffectType.Focus, 2, Effect.INFINITE_DURATION));
                caller.InflictEffect(caller, new Effect(Effect.EffectType.Absorb, 2, Effect.INFINITE_DURATION));
            }
        }
    }
}

public class BM_Slimeworm_Mortar : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Slimeworm_Mortar;

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
            if (caller.GetAttackHit(t, BattleHelper.DamageType.Water))
            {
                t.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.Squish);
                //Note that the focus last turn makes this pretty dangerous
                caller.DealDamage(t, 1, BattleHelper.DamageType.Water, 0, BattleHelper.ContactLevel.Infinite);
            }
            else
            {
                //Miss
                caller.InvokeMissEvents(t);
            }
        }
    }
}

public class BM_Slimeworm_DeepMortar : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Slimeworm_Hard_DeepMortar;

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
            if (caller.GetAttackHit(t, BattleHelper.DamageType.Fire))
            {
                t.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.Squish);
                //Note that the focus last turn makes this pretty dangerous
                caller.DealDamage(t, 1, BattleHelper.DamageType.Fire, 0, BattleHelper.ContactLevel.Infinite);
                caller.InflictEffect(t, new Effect(Effect.EffectType.Splotch, 1, 2));
            }
            else
            {
                //Miss
                caller.InvokeMissEvents(t);
            }
        }
    }
}

public class BE_Slimebloom : BattleEntity
{
    public override void Initialize()
    {
        moveset = new List<Move> { gameObject.AddComponent<BM_Slimebloom_Zap>(), gameObject.AddComponent<BM_Slimebloom_Lob>(), gameObject.AddComponent<BM_Slimebloom_Hard_Flash>() };

        base.Initialize();
    }

    public override void ChooseMoveInternal()
    {
        if (BattleControl.Instance.GetCurseLevel() > 0)
        {
            currMove = moveset[((posId % 2) + BattleControl.Instance.turnCount - 1) % 3];
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
                DealDamage(target, 2, BattleHelper.DamageType.Air, (ulong)BattleHelper.DamageProperties.StandardContactHazard, BattleHelper.ContactLevel.Contact);
                target.contactImmunityList.Add(posId);
            }
        }
    }
}

public class BM_Slimebloom_Zap : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Slimebloom_Zap;

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
            if (caller.GetAttackHit(caller.curTarget, BattleHelper.DamageType.Air))
            {
                bool hasStatus = caller.curTarget.HasAilment();
                caller.DealDamage(caller.curTarget, 3, BattleHelper.DamageType.Air, 0, BattleHelper.ContactLevel.Infinite);
                if (!hasStatus)
                {
                    caller.InflictEffect(caller.curTarget, new Effect(Effect.EffectType.Paralyze, 1, 1));
                }
                if (BattleControl.Instance.GetCurseLevel() > 0)
                {
                    caller.InflictEffectBuffered(caller.curTarget, new Effect(Effect.EffectType.Sunder, 1, Effect.INFINITE_DURATION));
                }
            }
            else
            {
                caller.InvokeMissEvents(caller.curTarget);
            }
        }
    }
}

public class BM_Slimebloom_Lob : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Slimebloom_Lob;

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
            if (caller.GetAttackHit(caller.curTarget, BattleHelper.DamageType.Water))
            {
                bool hasStatus = caller.curTarget.HasAilment();
                caller.curTarget.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.Spin);
                caller.DealDamage(caller.curTarget, 2, BattleHelper.DamageType.Water, 0, BattleHelper.ContactLevel.Infinite);
                if (!hasStatus)
                {
                    caller.InflictEffect(caller.curTarget, new Effect(Effect.EffectType.Sleep, 1, 1));
                }
                if (BattleControl.Instance.GetCurseLevel() > 0)
                {
                    caller.InflictEffectBuffered(caller.curTarget, new Effect(Effect.EffectType.Defocus, 1, Effect.INFINITE_DURATION));
                }
            }
            else
            {
                caller.InvokeMissEvents(caller.curTarget);
            }
        }
    }
}

public class BM_Slimebloom_Hard_Flash : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Slimebloom_Hard_Flash;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemyLowFrontmost);

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        if (!BattleControl.Instance.EntityValid(caller.curTarget))
        {
            caller.curTarget = null;
        }

        yield return StartCoroutine(caller.SpinHeavy(Vector3.up * 360, 0.5f));

        if (caller.curTarget != null)
        {
            if (caller.GetAttackHit(caller.curTarget, BattleHelper.DamageType.Light))
            {
                bool hasStatus = caller.curTarget.HasAilment();
                caller.DealDamage(caller.curTarget, 2, BattleHelper.DamageType.Light, 0, BattleHelper.ContactLevel.Infinite);
                if (!hasStatus)
                {
                    caller.InflictEffect(caller.curTarget, new Effect(Effect.EffectType.TimeStop, 1, 2));
                }
            }
            else
            {
                caller.InvokeMissEvents(caller.curTarget);
            }
        }
    }
}

public class BE_Sirenfish : BattleEntity
{
    public override void Initialize()
    {
        moveset = new List<Move> { gameObject.AddComponent<BM_Sirenfish_BubbleSong>(), gameObject.AddComponent<BM_Sirenfish_PowerSong>(), gameObject.AddComponent<BM_Sirenfish_NightmareBite>() };

        base.Initialize();
    }

    public override void ChooseMoveInternal()
    {
        if (GetEntityProperty(BattleHelper.EntityProperties.Grounded))
        {
            currMove = moveset[0];
        }
        else
        {
            currMove = moveset[(posId + BattleControl.Instance.turnCount - 1) % 2];
        }

        bool nightmare = false;        
        if (BattleControl.Instance.GetCurseLevel() > 0)
        {
            List<BattleEntity> bl = BattleControl.Instance.GetEntitiesSorted(this, new TargetArea(TargetArea.TargetAreaType.LiveEnemy));

            for (int i = 0; i < bl.Count; i++)
            {
                if (bl[i].HasEffect(Effect.EffectType.Sleep))
                {
                    currMove = moveset[2];
                    curTarget = bl[i];
                    nightmare = true;
                    break;
                }
            }
        }

        if (!nightmare)
        {
            BasicTargetChooser();
        }
    }

    public override IEnumerator DoEvent(BattleHelper.Event eventID)
    {
        yield return StartCoroutine(DefaultEventHandler_Flying(eventID));
    }
}

public class BM_Sirenfish_BubbleSong : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Sirenfish_BubbleSong;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemy);

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        //fly back up        
        yield return StartCoroutine(caller.FlyingFlyBackUp());

        if (!BattleControl.Instance.EntityValid(caller.curTarget))
        {
            caller.curTarget = null;
        }

        yield return StartCoroutine(caller.SpinHeavy(Vector3.up * 360, 0.5f));

        if (caller.curTarget != null)
        {
            if (caller.GetAttackHit(caller.curTarget, 0))
            {
                bool hasStatus = caller.curTarget.HasAilment();
                caller.curTarget.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.Spin);
                caller.DealDamage(caller.curTarget, 3, BattleHelper.DamageType.Normal, 0, BattleHelper.ContactLevel.Infinite);
                if (!hasStatus)
                {
                    if (BattleControl.Instance.GetCurseLevel() > 0)
                    {
                        caller.InflictEffect(caller.curTarget, new Effect(Effect.EffectType.Sleep, 1, 3));
                    }
                    else
                    {
                        caller.InflictEffect(caller.curTarget, new Effect(Effect.EffectType.Sleep, 1, 2));
                    }
                }
            }
            else
            {
                caller.InvokeMissEvents(caller.curTarget);
            }
        }
    }
}

public class BM_Sirenfish_PowerSong : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Sirenfish_PowerSong;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveAlly);

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        if (!BattleControl.Instance.EntityValid(caller.curTarget))
        {
            caller.curTarget = null;
        }

        yield return StartCoroutine(caller.SpinHeavy(Vector3.up * 360, 0.5f));

        List<BattleEntity> targets = BattleControl.Instance.GetEntitiesSorted(caller, GetBaseTarget());

        foreach (BattleEntity be in targets)
        {
            caller.InflictEffect(be, new Effect(Effect.EffectType.Focus, 1, Effect.INFINITE_DURATION));
        }
    }
}

public class BM_Sirenfish_NightmareBite : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Sirenfish_Hard_NightmareBite;

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

            yield return StartCoroutine(caller.MoveEasing(tposA, (e) => MainManager.EasingOutIn(e)));

            Vector3[] positions = new Vector3[] { tposA, tposmid, tposend };

            if (caller.GetAttackHit(caller.curTarget, BattleHelper.DamageType.Dark))
            {
                yield return StartCoroutine(caller.FollowBezierCurve(backFlag ? 0.4f : 0.3f, (float a) => MainManager.EasingQuadratic(a, -0.2f), positions));
                caller.curTarget.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.Squish);
                caller.DealDamage(caller.curTarget, 5, BattleHelper.DamageType.Dark, 0, BattleHelper.ContactLevel.Contact);
            }
            else
            {
                yield return StartCoroutine(caller.FollowBezierCurve(0.15f * dist, (float a) => MainManager.EasingQuadratic(a, -0.2f), 1.25f, positions));
                caller.InvokeMissEvents(caller.curTarget);
            }
        }

        yield return StartCoroutine(caller.MoveEasing(caller.homePos, (e) => MainManager.EasingOutIn(e)));
    }
}