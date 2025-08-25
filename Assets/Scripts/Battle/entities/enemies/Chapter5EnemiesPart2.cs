using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BE_Toxiwing : BattleEntity
{
    public override void Initialize()
    {
        moveset = new List<Move> { gameObject.AddComponent<BM_Toxiwing_TripleDrainBite>(), gameObject.AddComponent<BM_Toxiwing_CruelSwoop>(), gameObject.AddComponent<BM_Heatwing_Hard_DreadScreech>() };

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
            if (GetEntityProperty(BattleHelper.EntityProperties.Grounded))
            {
                currMove = moveset[0];
            }
            else
            {
                if (BattleControl.Instance.GetCurseLevel() > 0)
                {
                    currMove = moveset[(posId + BattleControl.Instance.turnCount - 1) % 3];
                }
                else
                {
                    currMove = moveset[(posId + BattleControl.Instance.turnCount - 1) % 2];
                }
            }
        }

        BasicTargetChooser();

        if (currMove == moveset[0])
        {
            SpecialTargetChooserFirst(TargetStrategy.TargetStrategyType.HighHP);
        }
        if (currMove == moveset[1])
        {
            SpecialTargetChooserFirst(TargetStrategy.TargetStrategyType.LowHP);
        }
    }

    public override IEnumerator DoEvent(BattleHelper.Event eventID)
    {
        yield return StartCoroutine(DefaultEventHandler_Flying(eventID));
    }
}

public class BM_Toxiwing_TripleDrainBite : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Toxiwing_TripleDrainBite;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemy);

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
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

            if (caller.GetAttackHit(caller.curTarget, 0))
            {
                yield return StartCoroutine(caller.FollowBezierCurve(backFlag ? 0.4f : 0.3f, (float a) => MainManager.EasingQuadratic(a, -0.2f), positions));

                //yield return StartCoroutine(caller.Squish(0.067f, 0.2f));

                yield return StartCoroutine(caller.Squish(0.067f, 0.2f));
                caller.curTarget.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.ReverseSquish);
                switch (caller.entityID)
                {
                    case BattleHelper.EntityID.Toxiwing:
                        caller.DealDamage(caller.curTarget, 4, BattleHelper.DamageType.Normal, (ulong)(BattleHelper.DamageProperties.HPDrainOneToOne), BattleHelper.ContactLevel.Contact);
                        break;
                    default:
                        caller.DealDamage(caller.curTarget, 2, BattleHelper.DamageType.Normal, 0, BattleHelper.ContactLevel.Contact);
                        break;
                }
                yield return StartCoroutine(caller.RevertScale(0.1f));
                yield return new WaitForSeconds(0.2f);

                for (int i = 0; i < 2; i++)
                {
                    yield return StartCoroutine(caller.Squish(0.067f, 0.2f));
                    switch (caller.entityID)
                    {
                        case BattleHelper.EntityID.Toxiwing:
                            caller.DealDamage(caller.curTarget, 4, BattleHelper.DamageType.Normal, (ulong)(BattleHelper.DamageProperties.HPDrainOneToOne), BattleHelper.ContactLevel.Contact);
                            break;
                        default:
                            caller.DealDamage(caller.curTarget, 2, BattleHelper.DamageType.Normal, 0, BattleHelper.ContactLevel.Contact);
                            break;
                    }
                    if (i == 1 && BattleControl.Instance.GetCurseLevel() > 0)
                    {
                        caller.InflictEffect(caller.curTarget, new Effect(Effect.EffectType.Poison, 1, 3));
                    }
                    yield return StartCoroutine(caller.RevertScale(0.1f));
                    yield return new WaitForSeconds(0.2f);
                }

                //StartCoroutine(caller.RevertScale(0.1f));
            }
            else
            {
                positions[2] = tposend + Vector3.left * 0.5f;
                yield return StartCoroutine(caller.FollowBezierCurve(backFlag ? 0.4f : 0.3f, (float a) => MainManager.EasingQuadratic(a, -0.2f), 1.25f, positions));
                caller.InvokeMissEvents(caller.curTarget);
            }
        }

        yield return StartCoroutine(caller.MoveEasing(caller.homePos, (e) => MainManager.EasingOutIn(e)));
    }
}

public class BM_Toxiwing_CruelSwoop : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Toxiwing_CruelSwoop;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemy);

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
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

            if (caller.GetAttackHit(caller.curTarget, 0))
            {
                yield return StartCoroutine(caller.FollowBezierCurve(backFlag ? 0.4f : 0.3f, (float a) => MainManager.EasingQuadratic(a, -0.2f), positions));

                //yield return StartCoroutine(caller.Squish(0.067f, 0.2f));

                caller.curTarget.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.ReverseSquish);
                switch (caller.entityID)
                {
                    case BattleHelper.EntityID.Toxiwing:
                        caller.DealDamage(caller.curTarget, 6, BattleHelper.DamageType.Fire, 0, BattleHelper.ContactLevel.Contact);
                        caller.InflictEffect(caller, new Effect(Effect.EffectType.CounterFlare, 1, 3));
                        break;
                    default:
                        caller.DealDamage(caller.curTarget, 2, BattleHelper.DamageType.Normal, 0, BattleHelper.ContactLevel.Contact);
                        break;
                }

                //StartCoroutine(caller.RevertScale(0.1f));
            }
            else
            {
                positions[2] = tposend + Vector3.left * 0.5f;
                yield return StartCoroutine(caller.FollowBezierCurve(backFlag ? 0.4f : 0.3f, (float a) => MainManager.EasingQuadratic(a, -0.2f), 1.25f, positions));
                caller.InvokeMissEvents(caller.curTarget);
            }
        }

        yield return StartCoroutine(caller.MoveEasing(caller.homePos, (e) => MainManager.EasingOutIn(e)));
    }
}

public class BE_GoldFly : BattleEntity
{
    int counterCount;

    public override void Initialize()
    {
        moveset = new List<Move> { gameObject.AddComponent<BM_GoldFly_SunderingBite>(), gameObject.AddComponent<BM_GoldFly_GoldWind>(), gameObject.AddComponent<BM_GoldFly_MorbidGoldBoost>(), gameObject.AddComponent<BM_GoldFly_Hard_FinalMassObscure>() };

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

        BasicTargetChooser();
    }

    public override IEnumerator DoEvent(BattleHelper.Event eventID)
    {
        yield return StartCoroutine(DefaultEventHandler_Flying(eventID));
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
        if (BattleControl.Instance.GetCurseLevel() > 0 && (e == BattleHelper.Event.Death && target == this && counterCount <= 0))
        {
            counterCount++;
            BattleControl.Instance.AddReactionMoveEvent(this, target.lastAttacker, moveset[3]);
            return true;
        }

        //morbid effect
        //takes less priority than Mass Obscure
        if ((e == BattleHelper.Event.Death && target != this && target.posId >= 0 && counterCount <= 0))
        {
            counterCount++;
            BattleControl.Instance.AddReactionMoveEvent(this, target.lastAttacker, moveset[2]);
            return true;
        }

        return false;
    }
}

public class BM_GoldFly_SunderingBite : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.GoldFly_SunderingBite;

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

            if (caller.GetAttackHit(caller.curTarget, 0))
            {
                yield return StartCoroutine(caller.FollowBezierCurve(backFlag ? 0.4f : 0.3f, (float a) => MainManager.EasingQuadratic(a, -0.2f), positions));

                caller.curTarget.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.ReverseSquish);
                caller.DealDamage(caller.curTarget, 6, BattleHelper.DamageType.Normal, 0, BattleHelper.ContactLevel.Contact);
                caller.InflictEffect(caller.curTarget, new Effect(Effect.EffectType.Defocus, 2, Effect.INFINITE_DURATION));
                caller.InflictEffectBuffered(caller.curTarget, new Effect(Effect.EffectType.Sunder, 2, Effect.INFINITE_DURATION));
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

public class BM_GoldFly_GoldWind : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.GoldFly_GoldWind;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemy);

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        //fly back up        
        yield return StartCoroutine(caller.FlyingFlyBackUp());

        if (!BattleControl.Instance.EntityValid(caller.curTarget))
        {
            caller.curTarget = null;
        }

        yield return StartCoroutine(caller.SpinHeavy(Vector3.up * 360, 1f));

        List<BattleEntity> targets = BattleControl.Instance.GetEntitiesSorted(caller, GetBaseTarget());

        foreach (BattleEntity t in targets)
        {
            if (caller.GetAttackHit(t, BattleHelper.DamageType.Air))
            {
                t.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.Spin);
                caller.DealDamage(t, 5, BattleHelper.DamageType.Air, 0, BattleHelper.ContactLevel.Infinite);
            }
            else
            {
                //Miss
                caller.InvokeMissEvents(t);
            }
        }
    }
}

public class BM_GoldFly_MorbidGoldBoost : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.GoldFly_MorbidGoldBoost;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.Self);

    public override IEnumerator ExecuteOutOfTurn(BattleEntity caller, BattleEntity target, int level = 1)
    {
        yield return StartCoroutine(caller.SpinHeavy(Vector3.up * 360, 0.5f));

        caller.InflictEffect(caller, new Effect(Effect.EffectType.AttackUp, 2, 3));
        caller.InflictEffect(caller, new Effect(Effect.EffectType.Focus, 2, Effect.INFINITE_DURATION));
    }
}

public class BM_GoldFly_Hard_FinalMassObscure : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.GoldFly_Hard_FinalMassObscure;

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
                caller.InflictEffect(be, new Effect(Effect.EffectType.Ethereal, 1, 2));
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

public class BE_MawSpore : BattleEntity
{
    int counterCount;
    public override void Initialize()
    {
        moveset = new List<Move> { gameObject.AddComponent<BM_MawSpore_EnviousBite>(), gameObject.AddComponent<BM_MawSpore_EnervatingLick>(), gameObject.AddComponent<BM_MawSpore_Devour>(), gameObject.AddComponent<BM_MawSpore_CounterSporeBlast>(), gameObject.AddComponent<BM_MawSpore_Hard_SwiftDevour>() };

        base.Initialize();

        SetEntityProperty(BattleHelper.EntityProperties.StateCounter, true);
    }

    public override void ChooseMoveInternal()
    {
        //also reset here in case something weird happens
        counterCount = 0;
        currMove = moveset[(posId + BattleControl.Instance.turnCount - 1) % 3];

        BasicTargetChooser();

        if (currMove == moveset[0])
        {
            SpecialTargetChooserFirst(TargetStrategy.TargetStrategyType.MostBuffs);
        }

        if (currMove == moveset[2])
        {
            //Is anything edible?
            //if not then go back to biting
            //note: even in hard mode things can still be eaten by devour instead of swift devour
            //because you might use a move that produces multiple devour eligible targets at the same time (or trigger the counter attack as it is higher priority)
            if (curTarget == null)
            {
                currMove = moveset[0];
                BasicTargetChooser();
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

        if (move == moveset[4])
        {
            Effect_ReactionDefend();
        }

        if (move == moveset[3])
        {
            Effect_ReactionCounter();
        }

        yield return new WaitForSeconds(0.5f);
    }
    public override bool ReactToEvent(BattleEntity target, BattleHelper.Event e, int previousReactions)
    {
        //counter attack
        //higher priority than reaction devour
        if (e == BattleHelper.Event.Hurt && target == this && counterCount <= 0)
        {
            counterCount++;
            BattleControl.Instance.AddReactionMoveEvent(this, target.lastAttacker, moveset[3]);
            return true;
        }

        //Eat stuff?
        List<BattleEntity> devourTargets = BattleControl.Instance.GetEntitiesSorted(this, new TargetArea(TargetArea.TargetAreaType.LiveAllyWeakLowNotSameType), new TargetStrategy(TargetStrategy.TargetStrategyType.LowHP));
        if (BattleControl.Instance.GetCurseLevel() > 0 && devourTargets.Count > 0 && counterCount <= 0)
        {
            counterCount++;
            BattleControl.Instance.AddReactionMoveEvent(this, devourTargets[0], moveset[4]);
            return true;
        }

        return false;
    }
}

public class BM_MawSpore_EnviousBite : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.MawSpore_EnviousBite;

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

            if (caller.GetAttackHit(caller.curTarget, 0))
            {
                yield return StartCoroutine(caller.Squish(0.067f, 0.2f));

                caller.curTarget.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.ReverseSquish);
                switch (caller.entityID)
                {
                    case BattleHelper.EntityID.MawSpore:
                        caller.DealDamage(caller.curTarget, 7, BattleHelper.DamageType.Normal, 0, BattleHelper.ContactLevel.Contact);
                        List<Effect> stolenEffects = caller.curTarget.effects.FindAll((e) => (Effect.IsCleanseable(e.effect)));
                        caller.curTarget.CleanseEffects();
                        foreach (Effect e in stolenEffects)
                        {
                            caller.InflictEffect(caller, e);
                        }
                        break;
                    default:
                        caller.DealDamage(caller.curTarget, 2, BattleHelper.DamageType.Normal, 0, BattleHelper.ContactLevel.Contact);
                        break;
                }
                StartCoroutine(caller.RevertScale(0.1f));
            }
            else
            {
                caller.InvokeMissEvents(caller.curTarget);
            }
        }

        yield return StartCoroutine(caller.MoveEasing(caller.homePos, (e) => MainManager.EasingOutIn(e)));
    }
}

public class BM_MawSpore_EnervatingLick : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.MawSpore_EnervatingLick;

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

            if (caller.GetAttackHit(caller.curTarget, 0))
            {
                yield return StartCoroutine(caller.Squish(0.067f, 0.2f));

                caller.curTarget.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.ReverseSquish);
                switch (caller.entityID)
                {
                    case BattleHelper.EntityID.MawSpore:
                        caller.DealDamage(caller.curTarget, 4, BattleHelper.DamageType.Water, (ulong)(BattleHelper.DamageProperties.EPLossOneToOne), BattleHelper.ContactLevel.Contact);
                        break;
                    default:
                        caller.DealDamage(caller.curTarget, 2, BattleHelper.DamageType.Normal, 0, BattleHelper.ContactLevel.Contact);
                        break;
                }
                StartCoroutine(caller.RevertScale(0.1f));
            }
            else
            {
                caller.InvokeMissEvents(caller.curTarget);
            }
        }

        yield return StartCoroutine(caller.MoveEasing(caller.homePos, (e) => MainManager.EasingOutIn(e)));
    }
}

public class BM_MawSpore_Devour : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.MawSpore_Devour;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveAllyWeakLowNotSameType);

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
            //not completely correct but it shouldn't look wrong mostly
            if (Mathf.Abs(caller.posId - caller.curTarget.posId) > 1)
            {
                backflag = true;
            }

            //Debug.Log(itpos);

            Vector3 tpos = caller.curTarget.transform.position + ((caller.width / 2) + (caller.curTarget.width / 2)) * (Vector3.right * (caller.curTarget.transform.position.x > caller.transform.position.x ? -1 : 1));

            if (backflag)
            {
                yield return StartCoroutine(caller.MoveEasing(itpos, (e) => MainManager.EasingOut(e)));
                yield return StartCoroutine(caller.MoveEasing(tpos, (e) => MainManager.EasingIn(e)));
            }
            else
            {
                yield return StartCoroutine(caller.MoveEasing(tpos, (e) => MainManager.EasingOutIn(e)));
            }

            if (caller.GetAttackHit(caller.curTarget, 0))
            {
                yield return StartCoroutine(caller.Squish(0.067f, 0.2f));

                caller.curTarget.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.ReverseSquish);

                //note that enemies might have some final thing that activates because of this since I decided not to delete the enemy immediately
                //maw spore healing
                caller.HealHealth(caller.curTarget.hp);
                caller.InflictEffectCapped(caller, new Effect(Effect.EffectType.AttackBoost, 2, Effect.INFINITE_DURATION), 8);
                caller.DealDamage(caller.curTarget, caller.curTarget.hp, BattleHelper.DamageType.Normal, (ulong)(BattleHelper.DamageProperties.Hardcode), BattleHelper.ContactLevel.Contact);
                //maw spore inherits effects
                foreach (Effect e in caller.curTarget.effects)
                {
                    caller.InflictEffect(caller, e);
                }

                StartCoroutine(caller.RevertScale(0.1f));
            }
            else
            {
                caller.InvokeMissEvents(caller.curTarget);
            }
        }

        yield return StartCoroutine(caller.MoveEasing(caller.homePos, (e) => MainManager.EasingOutIn(e)));
    }
}

public class BM_MawSpore_CounterSporeBlast : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.MawSpore_CounterSporeBlast;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemy);

    public override IEnumerator ExecuteOutOfTurn(BattleEntity caller, BattleEntity target, int level = 1)
    {
        caller.curTarget = target;
        if (!BattleControl.Instance.EntityValid(caller.curTarget))
        {
            caller.curTarget = null;
        }

        yield return StartCoroutine(caller.SpinHeavy(Vector3.up * 360, 0.5f));


        if (caller.curTarget != null)
        {
            if (caller.GetAttackHit(caller.curTarget, BattleHelper.DamageType.Dark))
            {
                caller.curTarget.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.Spin);
                caller.DealDamage(caller.curTarget, 4, BattleHelper.DamageType.Dark, 0, BattleHelper.ContactLevel.Infinite);
                caller.InflictEffect(caller.curTarget, new Effect(Effect.EffectType.Defocus, 3, Effect.INFINITE_DURATION));
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

public class BM_MawSpore_Hard_SwiftDevour : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.MawSpore_Hard_SwiftDevour;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveAllyWeakLowNotSameType);

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
            //not completely correct but it shouldn't look wrong mostly
            if (Mathf.Abs(caller.posId - caller.curTarget.posId) > 1)
            {
                backflag = true;
            }

            //Debug.Log(itpos);

            Vector3 tpos = caller.curTarget.transform.position + ((caller.width / 2) + (caller.curTarget.width / 2)) * (Vector3.right * (caller.curTarget.transform.position.x > caller.transform.position.x ? -1 : 1));

            if (backflag)
            {
                itpos = ((transform.position + tpos) / 2) + Vector3.forward * 1;
                yield return StartCoroutine(caller.MoveEasing(itpos, (e) => MainManager.EasingOut(e)));
                yield return StartCoroutine(caller.MoveEasing(tpos, (e) => MainManager.EasingIn(e)));
            }
            else
            {
                yield return StartCoroutine(caller.MoveEasing(tpos, (e) => MainManager.EasingOutIn(e)));
            }

            if (caller.GetAttackHit(caller.curTarget, 0))
            {
                yield return StartCoroutine(caller.Squish(0.067f, 0.2f));

                caller.curTarget.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.ReverseSquish);

                //note that enemies might have some final thing that activates because of this since I decided not to delete the enemy immediately
                //maw spore healing
                caller.HealHealth(caller.curTarget.hp);
                caller.InflictEffectCapped(caller, new Effect(Effect.EffectType.AttackBoost, 2, Effect.INFINITE_DURATION), 8);
                caller.DealDamage(caller.curTarget, caller.curTarget.hp, BattleHelper.DamageType.Normal, (ulong)(BattleHelper.DamageProperties.Hardcode), BattleHelper.ContactLevel.Contact);
                //maw spore inherits effects
                foreach (Effect e in caller.curTarget.effects)
                {
                    caller.InflictEffect(caller, e);
                }

                StartCoroutine(caller.RevertScale(0.1f));
            }
            else
            {
                caller.InvokeMissEvents(caller.curTarget);
            }
        }

        yield return StartCoroutine(caller.MoveEasing(caller.homePos, (e) => MainManager.EasingOutIn(e)));
    }
}

public class BE_CaveSpider : BattleEntity
{
    public bool didSummon = false;
    public override void Initialize()
    {
        moveset = new List<Move> { gameObject.AddComponent<BM_Shared_Bite>(), gameObject.AddComponent<BM_CaveSpider_SporeBall>(), gameObject.AddComponent<BM_CaveSpider_ShieldWeave>(), gameObject.AddComponent<BM_CaveSpider_WeaveCopy>() };

        base.Initialize();
    }

    public override void ChooseMoveInternal()
    {
        currMove = moveset[((posId + BattleControl.Instance.turnCount - 1) % 3) + 1];
        if (currMove == moveset[0] || currMove == moveset[1])
        {
            if (GetEntityProperty(BattleHelper.EntityProperties.Airborne))
            {
                currMove = moveset[1];
            }
            else
            {
                currMove = moveset[0];
            }
        }
        BasicTargetChooser();

        if (currMove == moveset[3] && curTarget == null)
        {
            if (GetEntityProperty(BattleHelper.EntityProperties.Airborne))
            {
                currMove = moveset[1];
            } else
            {
                currMove = moveset[0];
            }
            currMove = moveset[0];
            BasicTargetChooser();
        }

        //no open space
        //or already did summon something
        if (currMove == moveset[3] && (didSummon || BattleControl.Instance.FindUnoccupiedID(false, true) >= 4))
        {
            if (GetEntityProperty(BattleHelper.EntityProperties.Airborne))
            {
                currMove = moveset[1];
            }
            else
            {
                currMove = moveset[0];
            }
            BasicTargetChooser();
        }
    }

    public override IEnumerator DoEvent(BattleHelper.Event eventID)
    {
        yield return StartCoroutine(DefaultEventHandler_Flying(eventID));
    }
}

public class BM_CaveSpider_SporeBall : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.CaveSpider_SporeBall;

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
            if (caller.GetAttackHit(caller.curTarget, BattleHelper.DamageType.Dark))
            {
                caller.curTarget.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.Spin);
                caller.DealDamage(caller.curTarget, 5, BattleHelper.DamageType.Dark, 0, BattleHelper.ContactLevel.Infinite);
            }
            else
            {
                caller.InvokeMissEvents(caller.curTarget);
            }
        }
    }
}

public class BM_CaveSpider_ShieldWeave : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.CaveSpider_ShieldWeave;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.Self);

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        yield return StartCoroutine(caller.SpinHeavy(Vector3.up * 360, 0.5f));
        caller.InflictEffect(caller, new Effect(Effect.EffectType.QuantumShield, 1, 3));
        caller.InflictEffect(caller, new Effect(Effect.EffectType.DefenseUp, 2, 3));
    }
}

public class BM_CaveSpider_WeaveCopy : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.CaveSpider_WeaveCopy;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveAllyNotSameType);

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        yield return StartCoroutine(caller.SpinHeavy(Vector3.up * 360, 0.25f));

        BE_CaveSpider cs = (BE_CaveSpider)caller;

        //fizzle
        int newPosId = BattleControl.Instance.FindUnoccupiedID(false, true);
        if (newPosId >= 4)
        {
            yield return new WaitForSeconds(0.5f);
            yield break;
        }

        //airborne -> airbone copy
        newPosId += (caller.curTarget.posId / 10) * 10;

        BattleEntity newEntity = BattleControl.Instance.SummonEntity(caller.curTarget.entityID, newPosId);
        newEntity.hp = newEntity.maxHP / 2;
        if (newEntity.hp == 0)
        {
            newEntity.hp = 1;
        }
        foreach (Effect e in caller.curTarget.effects)
        {
            caller.InflictEffect(newEntity, e);
        }
        caller.InflictEffectCapped(newEntity, new Effect(Effect.EffectType.AttackBoost, 2, Effect.INFINITE_DURATION), 8);
        if (BattleControl.Instance.GetCurseLevel() > 0)
        {
            caller.InflictEffectCapped(newEntity, new Effect(Effect.EffectType.DefenseBoost, 2, Effect.INFINITE_DURATION), 8);
            caller.InflictEffect(newEntity, new Effect(Effect.EffectType.QuantumShield, 1, 3));
        }
        newEntity.ReceiveEffectForce(new Effect(Effect.EffectType.Cooldown, 1, Effect.INFINITE_DURATION));

        if (cs != null)
        {
            cs.didSummon = true;
        }
        if (newEntity != null)
        {
            newEntity.transform.position = caller.transform.position + Vector3.forward * -0.05f;
            newEntity.subObject.GetComponentInChildren<SpriteRenderer>().color = new Color(1, 0.75f, 1, 1);
            yield return StartCoroutine(newEntity.JumpHeavy(newEntity.homePos, 0.6f, 0.2f, 1));
            yield return new WaitForSeconds(0.1f);
            newEntity.SetIdleAnimation(true);
            newEntity.StartIdle();
        }

        yield return new WaitForSeconds(0.5f);
    }
}

public class BE_Obscurer : BattleEntity
{
    int counterCount;

    public override void Initialize()
    {
        moveset = new List<Move> { gameObject.AddComponent<BM_Obscurer_SinisterFog>(), gameObject.AddComponent<BM_Shrouder_SporeCloak>(), gameObject.AddComponent<BM_GoldFly_Hard_FinalMassObscure>() };

        base.Initialize();
    }

    public override void ChooseMoveInternal()
    {
        counterCount = 0;
        if (GetEntityProperty(BattleHelper.EntityProperties.Grounded))
        {
            currMove = moveset[0];
        }
        else
        {
            currMove = moveset[(posId + BattleControl.Instance.turnCount - 1) % 2];
        }

        if (currMove == moveset[1])
        {
            //Special case targetting: Make sure you don't target an ethereal enemy
            //If it is impossible not to target an ethereal enemy, give up and choose move 0
            List<BattleEntity> bl = BattleControl.Instance.GetEntitiesSorted(this, currMove.GetTargetArea(this));
            bl = bl.FindAll((e) => (e.posId != posId && !e.HasEffect(Effect.EffectType.Ethereal)));

            if (bl.Count == 0)
            {
                currMove = moveset[0];
                BasicTargetChooser();
            }
            else
            {
                curTarget = bl[BattleControl.Instance.GetPsuedoRandom(bl.Count, posId + 6)];
            }
        }
        else
        {
            BasicTargetChooser();
        }
    }

    public override IEnumerator DoEvent(BattleHelper.Event eventID)
    {
        yield return StartCoroutine(DefaultEventHandler_Flying(eventID));
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
        if (BattleControl.Instance.GetCurseLevel() > 0 && (e == BattleHelper.Event.Death && target == this && counterCount <= 0))
        {
            counterCount++;
            BattleControl.Instance.AddReactionMoveEvent(this, target.lastAttacker, moveset[2]);
            return true;
        }

        return false;
    }
}

public class BM_Obscurer_SinisterFog : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Obscurer_SinisterFog;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemy);

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        //fly back up        
        yield return StartCoroutine(caller.FlyingFlyBackUp());

        if (!BattleControl.Instance.EntityValid(caller.curTarget))
        {
            caller.curTarget = null;
        }

        yield return StartCoroutine(caller.SpinHeavy(Vector3.up * 360, 1f));

        List<BattleEntity> targets = BattleControl.Instance.GetEntitiesSorted(caller, GetBaseTarget());

        caller.InflictEffectCapped(caller, new Effect(Effect.EffectType.AttackBoost, 1, Effect.INFINITE_DURATION), 8);
        foreach (BattleEntity t in targets)
        {
            if (caller.GetAttackHit(t, 0))
            {
                t.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.Spin);
                caller.DealDamage(t, 5, BattleHelper.DamageType.Dark, 0, BattleHelper.ContactLevel.Infinite);
                caller.InflictEffect(t, new Effect(Effect.EffectType.EnduranceDown, 2, 3));
            }
            else
            {
                //Miss
                caller.InvokeMissEvents(t);
            }
        }

        yield return StartCoroutine(caller.MoveEasing(caller.homePos, (e) => MainManager.EasingOutIn(e)));
    }
}