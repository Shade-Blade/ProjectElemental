using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BE_Shieldwing : BattleEntity
{
    public override void Initialize()
    {
        moveset = new List<Move> { gameObject.AddComponent<BM_Shieldwing_DualPeck>(), gameObject.AddComponent<BM_Shieldwing_ChillingScreech>(), gameObject.AddComponent<BM_Shieldwing_FeatherWall>() };

        base.Initialize();
    }

    public override void ChooseMoveInternal()
    {
        //also reset here in case something weird happens
        currMove = moveset[(posId + BattleControl.Instance.turnCount - 1) % moveset.Count];

        SpecialTargetChooserFirst(TargetStrategy.TargetStrategyType.HighHP);
    }
}

public class BM_Shieldwing_DualPeck : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Shieldwing_DualPeck;

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
            if (caller.GetAttackHit(caller.curTarget, 0))
            {
                caller.curTarget.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.ReverseSquish);
                caller.DealDamage(caller.curTarget, 2, BattleHelper.DamageType.Normal, 0, BattleHelper.ContactLevel.Contact);
                yield return StartCoroutine(caller.SpinHeavy(Vector3.up * 360, 0.25f));
                caller.curTarget.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.ReverseSquish);
                caller.DealDamage(caller.curTarget, 2, BattleHelper.DamageType.Normal, 0, BattleHelper.ContactLevel.Contact);
            }
            else
            {
                caller.InvokeMissEvents(caller.curTarget);
            }
        }

        yield return StartCoroutine(caller.MoveEasing(caller.homePos, (e) => MainManager.EasingOutIn(e)));
    }
}

public class BM_Shieldwing_ChillingScreech : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Shieldwing_ChillingScreech;

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
                bool hasStatus = t.HasAilment();
                caller.DealDamage(t, 2, BattleHelper.DamageType.Light, 0, BattleHelper.ContactLevel.Infinite);
                if (!hasStatus)
                {
                    caller.InflictEffect(t, new Effect(Effect.EffectType.Freeze, 1, 3));
                }
                if (BattleControl.Instance.GetCurseLevel() > 0)
                {
                    caller.InflictEffect(t, new Effect(Effect.EffectType.Defocus, 2, Effect.INFINITE_DURATION));
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

public class BM_Shieldwing_FeatherWall : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Shieldwing_FeatherWall;

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
            caller.InflictEffect(be, new Effect(Effect.EffectType.DefenseUp, 2, 3));
            if (BattleControl.Instance.GetCurseLevel() > 0)
            {
                caller.InflictEffect(be, new Effect(Effect.EffectType.MistWall, 1, 3));
            }
        }
    }
}

public class BE_Honeywing : BattleEntity
{
    int counterCount = 0;
    public override void Initialize()
    {
        moveset = new List<Move> { gameObject.AddComponent<BM_Honeywing_SpitHeal>(), gameObject.AddComponent<BM_Honeywing_SwoopHealSoften>(), gameObject.AddComponent<BM_Shared_Hard_CounterProtect>() };

        base.Initialize();
    }

    public override void ChooseMoveInternal()
    {
        counterCount = 0;
        //Debug
        //RandomGenerator r = new RandomGenerator();

        //Debug.Log(1 + BattleControl.Instance.turnCount + " " + moveset.Count);
        //debug / default
        //do it in order
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
    public override bool ReactToEvent(BattleEntity target, BattleHelper.Event e, int previousReactions)
    {
        if (BattleControl.Instance.GetCurseLevel() <= 0)
        {
            return false;
        }

        if (e == BattleHelper.Event.Hurt && target == this && counterCount <= 0)
        {
            counterCount++;
            BattleControl.Instance.AddReactionMoveEvent(this, target.lastAttacker, moveset[2]);
            return true;
        }

        return false;
    }
}

public class BM_Honeywing_SpitHeal : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Honeywing_SpitHeal;
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

            if (caller.GetAttackHit(caller.curTarget, BattleHelper.DamageType.Water))
            {
                yield return StartCoroutine(caller.FollowBezierCurve(backFlag ? 0.4f : 0.3f, (float a) => MainManager.EasingQuadratic(a, -0.2f), positions));
                caller.curTarget.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.ReverseSquish);
                caller.DealDamage(caller.curTarget, 3, BattleHelper.DamageType.Water, 0, BattleHelper.ContactLevel.Contact);
            }
            else
            {
                yield return StartCoroutine(caller.FollowBezierCurve(0.15f * dist, (float a) => MainManager.EasingQuadratic(a, -0.2f), 1.25f, positions));
                caller.InvokeMissEvents(caller.curTarget);
            }
        }

        yield return StartCoroutine(caller.MoveEasing(caller.homePos, (e) => MainManager.EasingOutIn(e)));


        yield return DoHeal(caller);
    }

    public IEnumerator DoHeal(BattleEntity caller)
    {
        int singleHealAmount = 0;
        int multiHealAmount = 0;

        switch (caller.entityID)
        {
            case BattleHelper.EntityID.Honeywing:
                singleHealAmount = 3;
                multiHealAmount = 1;
                break;
        }

        //heal
        int singularHeal = 0;
        int spreadHeal = 0;

        BattleEntity singleHealTarget = null; //note: if all targets have 0 missing hp then this will stay null (and so no heal)
        List<BattleEntity> healTargets = BattleControl.Instance.GetEntitiesSorted(caller, new TargetArea(TargetArea.TargetAreaType.LiveAlly));

        int tempHeal = 0;

        for (int i = 0; i < healTargets.Count; i++)
        {
            tempHeal = healTargets[i].maxHP - healTargets[i].hp;

            if (tempHeal > singularHeal)
            {
                singularHeal = tempHeal;
                singleHealTarget = healTargets[i];
            }

            if (tempHeal > multiHealAmount)
            {
                spreadHeal += multiHealAmount;
            }
            else
            {
                spreadHeal += tempHeal;
            }
        }

        if (spreadHeal > Mathf.Min(singularHeal, singleHealAmount))
        {
            //Spread heal
            yield return new WaitForSeconds(0.5f);
            for (int i = 0; i < healTargets.Count; i++)
            {
                healTargets[i].HealHealth(multiHealAmount);
            }
            yield return new WaitForSeconds(0.5f);
        }
        else
        {
            //Single heal?
            if (singleHealTarget != null)
            {
                //heal the target
                yield return new WaitForSeconds(0.5f);
                singleHealTarget.HealHealth(singleHealAmount);
                yield return new WaitForSeconds(0.5f);
            }
        }
    }
}

public class BM_Honeywing_SwoopHealSoften : BM_Honeybud_SwoopHeal
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Honeywing_SwoopHealSoften;

    public override void ApplyHealEffect(BattleEntity caller, BattleEntity target)
    {
        caller.InflictEffect(caller, new Effect(Effect.EffectType.Soften, 1, 3));
        if (BattleControl.Instance.GetCurseLevel() > 0 && caller != target)
        {
            caller.InflictEffect(target, new Effect(Effect.EffectType.Soften, 1, 3));
        }
    }
}

public class BE_Shimmerwing : BattleEntity
{
    public override void Initialize()
    {
        moveset = new List<Move> { gameObject.AddComponent<BM_Shimmerwing_DazzlingScreech>(), gameObject.AddComponent<BM_Shimmerwing_RallySong>(), gameObject.AddComponent<BM_Shimmerwing_Hard_StaticFlurry>() };

        base.Initialize();
    }

    public override void ChooseMoveInternal()
    {
        //Debug
        //RandomGenerator r = new RandomGenerator();

        //Debug.Log(1 + BattleControl.Instance.turnCount + " " + moveset.Count);
        //debug / default
        //do it in order
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

        BasicTargetChooser(2, 3);
    }

    public override IEnumerator DoEvent(BattleHelper.Event eventID)
    {
        yield return StartCoroutine(DefaultEventHandler_Flying(eventID));
    }
}

public class BM_Shimmerwing_DazzlingScreech : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Shimmerwing_DazzlingScreech;

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
            if (caller.GetAttackHit(t, BattleHelper.DamageType.Light))
            {
                bool hasStatus = t.HasAilment();
                t.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.Spin);
                caller.DealDamage(t, 2, BattleHelper.DamageType.Light, 0, BattleHelper.ContactLevel.Infinite);
                if (!hasStatus)
                {
                    caller.InflictEffect(t, new Effect(Effect.EffectType.Dizzy, 1, 2));
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

public class BM_Shimmerwing_RallySong : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Shimmerwing_RallySong;

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
            if (BattleControl.Instance.GetCurseLevel() > 0)
            {
                caller.InflictEffect(be, new Effect(Effect.EffectType.ParryAura, 1, 3));
            }
        }
    }
}

public class BM_Shimmerwing_Hard_StaticFlurry : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Shimmerwing_Hard_StaticFlurry;

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
                bool hasStatus = t.HasAilment();
                t.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.Spin);
                caller.DealDamage(t, 3, BattleHelper.DamageType.Air, 0, BattleHelper.ContactLevel.Infinite);
                if (!hasStatus)
                {
                    caller.InflictEffect(t, new Effect(Effect.EffectType.Brittle, 1, 2));
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

public class BE_LumistarVanguard : BattleEntity
{
    public override void Initialize()
    {
        moveset = new List<Move> { gameObject.AddComponent<BM_LumistarVanguard_HornBlast>(), gameObject.AddComponent<BM_LumistarVanguard_LanternRally>(), gameObject.AddComponent<BM_LumistarVanguard_Hard_SoftGlow>() };

        base.Initialize();
    }

    public override void ChooseMoveInternal()
    {
        if (BattleControl.Instance.GetCurseLevel() > 0)
        {
            currMove = moveset[(posId + BattleControl.Instance.turnCount - 1) % moveset.Count];
        }
        else
        {
            currMove = moveset[(posId + BattleControl.Instance.turnCount - 1) % (moveset.Count - 1)];
        }

        BasicTargetChooser();

        if (currMove == moveset[2])
        {
            SpecialTargetChooserFirst(TargetStrategy.TargetStrategyType.LowHP);
        }
    }
}

public class BM_LumistarVanguard_HornBlast : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.LumistarVanguard_HornBlast;

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
                caller.DealDamage(t, 3, BattleHelper.DamageType.Light, 0, BattleHelper.ContactLevel.Infinite);
            }
            else
            {
                //Miss
                caller.InvokeMissEvents(t);
            }
        }
    }
}

public class BM_LumistarVanguard_LanternRally : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.LumistarVanguard_LanternRally;

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
                caller.DealDamage(t, 2, BattleHelper.DamageType.Light, 0, BattleHelper.ContactLevel.Infinite);
            }
            else
            {
                //Miss
                caller.InvokeMissEvents(t);
            }
        }

        //ally boost effect
        List<BattleEntity> allyTargets = BattleControl.Instance.GetEntitiesSorted(caller, new TargetArea(TargetArea.TargetAreaType.LiveAlly));
        foreach (BattleEntity a in allyTargets)
        {
            caller.InflictEffect(a, new Effect(Effect.EffectType.DefenseUp, 2, 3));
        }
    }
}

public class BM_LumistarVanguard_Hard_SoftGlow : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.LumistarVanguard_Hard_SoftGlow;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveAlly);

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        if (!BattleControl.Instance.EntityValid(caller.curTarget))
        {
            caller.curTarget = null;
        }

        yield return StartCoroutine(caller.SpinHeavy(Vector3.up * 360, 1f));

        caller.InflictEffect(caller.curTarget, new Effect(Effect.EffectType.Soften, 1, 3));
    }
}

public class BE_LumistarSoldier : BattleEntity
{
    int counterCount;
    public override void Initialize()
    {
        moveset = new List<Move> { gameObject.AddComponent<BM_Shared_Slash>(), gameObject.AddComponent<BM_LumistarSoldier_Charge>(), gameObject.AddComponent<BM_LumistarSoldier_ChargeSlash>(), gameObject.AddComponent<BM_Shared_Hard_CounterMarshall>() };

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
            currMove = moveset[(posId + BattleControl.Instance.turnCount - 1) % 3 > 1 ? 1 : 0];
        }

        if (currMove == moveset[2])
        {
            //hardcode: so you don't have to win the 66% bet or decipher the targetting logic
            SpecialTargetChooserFirst(TargetStrategy.TargetStrategyType.FrontMost);
        }
        else
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

        Effect_ReactionCounter();

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

public class BM_LumistarSoldier_Charge : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.LumistarSoldier_Charge;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.Self);

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        yield return StartCoroutine(caller.SpinHeavy(Vector3.up * 360, 0.25f));
        caller.SetEntityProperty(BattleHelper.EntityProperties.StateCharge);
        if (BattleControl.Instance.GetCurseLevel() > 0)
        {
            //ally boost effect
            List<BattleEntity> allyTargets = BattleControl.Instance.GetEntitiesSorted(caller, new TargetArea(TargetArea.TargetAreaType.LiveAlly));
            foreach (BattleEntity a in allyTargets)
            {
                caller.InflictEffect(a, new Effect(Effect.EffectType.Focus, 1, Effect.INFINITE_DURATION));
            }
        }
    }
}

public class BM_LumistarSoldier_ChargeSlash : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.LumistarSoldier_ChargeSlash;

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
            if (caller.GetAttackHit(caller.curTarget, 0))
            {
                caller.curTarget.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.Spin);
                caller.DealDamage(caller.curTarget, 11, BattleHelper.DamageType.Normal, 0, BattleHelper.ContactLevel.Weapon);
            }
            else
            {
                caller.InvokeMissEvents(caller.curTarget);
            }
        }

        yield return StartCoroutine(caller.MoveEasing(caller.homePos, (e) => MainManager.EasingOutIn(e)));
    }
}

public class BE_LumistarStriker : BattleEntity
{
    public override void Initialize()
    {
        moveset = new List<Move> { gameObject.AddComponent<BM_Shared_DualSlash>(), gameObject.AddComponent<BM_LumistarStriker_Charge>(), gameObject.AddComponent<BM_LumistarStriker_QuadSlash>() };

        base.Initialize();
    }

    public override void ChooseMoveInternal()
    {
        if (GetEntityProperty(BattleHelper.EntityProperties.StateCharge))
        {
            SetEntityProperty(BattleHelper.EntityProperties.StateCharge, false);
            currMove = moveset[2];
        }
        else
        {
            //Note: Just putting %2 here could make the Striker just cycle charge -> quad slash infinitely which is not what I want
            //So you get this wacky series of symbols
            currMove = moveset[(posId + BattleControl.Instance.turnCount - 1) % 3 > 1 ? 1 : 0];
        }

        if (currMove == moveset[2])
        {
            //hardcode: so you don't have to win the 66% bet or decipher the targetting logic
            SpecialTargetChooserFirst(TargetStrategy.TargetStrategyType.FrontMost);
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
}

public class BM_LumistarStriker_DualSlash : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.LumistarStriker_DualSlash;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemy);

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        if (!BattleControl.Instance.EntityValid(caller.curTarget))
        {
            caller.curTarget = null;
        }

        yield return StartCoroutine(caller.FlyingFallDown());

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
            if (caller.GetAttackHit(caller.curTarget, 0))
            {
                caller.curTarget.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.Spin);
                caller.DealDamage(caller.curTarget, 4, BattleHelper.DamageType.Normal, 0, BattleHelper.ContactLevel.Weapon);
                yield return StartCoroutine(caller.SpinHeavy(Vector3.up * 360, 0.25f));
                caller.curTarget.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.Spin);
                caller.DealDamage(caller.curTarget, 4, BattleHelper.DamageType.Normal, 0, BattleHelper.ContactLevel.Weapon);
            }
            else
            {
                caller.InvokeMissEvents(caller.curTarget);
            }
        }

        yield return StartCoroutine(caller.MoveEasing(caller.homePos, (e) => MainManager.EasingOutIn(e)));
        yield return StartCoroutine(caller.FlyingFlyBackUp());
    }
}

public class BM_LumistarStriker_Charge : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.LumistarStriker_Charge;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.Self);

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        yield return StartCoroutine(caller.FlyingFallDown());

        yield return StartCoroutine(caller.SpinHeavy(Vector3.up * 360, 0.25f));
        caller.SetEntityProperty(BattleHelper.EntityProperties.StateCharge);
        if (BattleControl.Instance.GetCurseLevel() > 0)
        {
            //ally boost effect
            List<BattleEntity> allyTargets = BattleControl.Instance.GetEntitiesSorted(caller, new TargetArea(TargetArea.TargetAreaType.LiveAlly));
            foreach (BattleEntity a in allyTargets)
            {
                caller.InflictEffect(a, new Effect(Effect.EffectType.Focus, 1, Effect.INFINITE_DURATION));
            }
        }
    }
}

public class BM_LumistarStriker_QuadSlash : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.LumistarStriker_QuadSlash;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemy);

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        if (!BattleControl.Instance.EntityValid(caller.curTarget))
        {
            caller.curTarget = null;
        }

        yield return StartCoroutine(caller.FlyingFallDown());

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
            if (caller.GetAttackHit(caller.curTarget, 0))
            {
                caller.DealDamage(caller.curTarget, 5, BattleHelper.DamageType.Normal, 0, BattleHelper.ContactLevel.Weapon);
                for (int i = 0; i < 3; i++)
                {
                    yield return StartCoroutine(caller.SpinHeavy(Vector3.up * 360, 0.25f));
                    caller.curTarget.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.Spin);
                    caller.DealDamage(caller.curTarget, 5, BattleHelper.DamageType.Normal, 0, BattleHelper.ContactLevel.Weapon);
                }
                if (BattleControl.Instance.GetCurseLevel() > 0)
                {
                    caller.InflictEffect(caller.curTarget, new Effect(Effect.EffectType.Brittle, 1, 3));
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