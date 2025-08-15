using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class BE_SpireGuard : BattleEntity
{
    public override void Initialize()
    {
        moveset = new List<Move> { gameObject.AddComponent<BM_Shared_Stab>(), gameObject.AddComponent<BM_SpireGuard_PowerRally>() };

        base.Initialize();
    }

    public override void ChooseMoveInternal()
    {
        currMove = moveset[(posId + BattleControl.Instance.turnCount - 1) % 2];

        BasicTargetChooser();
    }
}

public class BM_SpireGuard_PowerRally : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.SpireGuard_PowerRally;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveAlly);

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        yield return StartCoroutine(caller.SpinHeavy(Vector3.up * 360, 0.25f));

        List<BattleEntity> targets = BattleControl.Instance.GetEntitiesSorted(caller, GetBaseTarget());

        foreach (BattleEntity target in targets)
        {
            caller.InflictEffect(target, new Effect(Effect.EffectType.AttackUp, 2, 3));
            if (BattleControl.Instance.GetCurseLevel() > 0)
            {
                caller.InflictEffect(target, new Effect(Effect.EffectType.Focus, 2, Effect.INFINITE_DURATION));
            }
        }
    }
}

public class BE_EliteGuard : BattleEntity
{
    public override void Initialize()
    {
        moveset = new List<Move> { gameObject.AddComponent<BM_Shared_DoubleStab>(), gameObject.AddComponent<BM_EliteGuard_SpearWave>(), gameObject.AddComponent<BM_EliteGuard_GuardRally>() };

        base.Initialize();
    }

    public override void ChooseMoveInternal()
    {
        currMove = moveset[(posId + BattleControl.Instance.turnCount - 1) % 3];

        if (BattleControl.Instance.turnCount < 1)
        {
            currMove = moveset[2];
        }

        BasicTargetChooser();
    }
}

public class BM_EliteGuard_SpearWave : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.EliteGuard_SpearWave;

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
                caller.DealDamage(t, 6, BattleHelper.DamageType.Water, 0, BattleHelper.ContactLevel.Infinite);
            }
            else
            {
                //Miss
                caller.InvokeMissEvents(t);
            }
        }
    }
}

public class BM_EliteGuard_GuardRally : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.EliteGuard_GuardRally;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveAlly);

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        yield return StartCoroutine(caller.SpinHeavy(Vector3.up * 360, 0.25f));

        List<BattleEntity> targets = BattleControl.Instance.GetEntitiesSorted(caller, GetBaseTarget());

        foreach (BattleEntity target in targets)
        {
            caller.InflictEffect(target, new Effect(Effect.EffectType.DefenseUp, 2, 3));
            if (BattleControl.Instance.GetCurseLevel() > 0)
            {
                caller.InflictEffect(target, new Effect(Effect.EffectType.QuantumShield, 1, 3));
            }
        }
    }
}

public class BE_DesertBloom : BattleEntity
{
    int counterCount;
    public override void Initialize()
    {
        moveset = new List<Move> { gameObject.AddComponent<BM_DesertBloom_SwoopHealHustle>(), gameObject.AddComponent<BM_DesertBloom_ShiftyBiteHustle>(), gameObject.AddComponent<BM_Shared_Hard_CounterRush>() };

        base.Initialize();

        if (BattleControl.Instance.GetCurseLevel() > 0)
        {
            SetEntityProperty(BattleHelper.EntityProperties.StateCounter, true);
        }
    }

    public override void ChooseMoveInternal()
    {
        counterCount = 0;

        if (moveset.Count == 0)
        {
            currMove = null;
        }
        else if (moveset.Count == 1)
        {
            currMove = moveset[0];
        }
        else
        {
            if (GetEntityProperty(BattleHelper.EntityProperties.Grounded))
            {
                currMove = moveset[1];
            }
            else
            {
                currMove = moveset[0];
            }
        }

        BasicTargetChooser();
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

public class BM_DesertBloom_ShiftyBiteHustle : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.DesertBloom_ShiftyBiteHustle;

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
                    case BattleHelper.EntityID.DesertBloom:
                        caller.DealDamage(caller.curTarget, 10, BattleHelper.DamageType.Normal, 0, BattleHelper.ContactLevel.Contact);
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

        //fly back up        
        yield return StartCoroutine(caller.FlyingFlyBackUp());

        yield return DoHeal(caller);

        yield return ShiftPositions(caller);
    }

    public IEnumerator DoHeal(BattleEntity caller)
    {
        int singleHealAmount = 0;

        switch (caller.entityID)
        {
            case BattleHelper.EntityID.DesertBloom:
                singleHealAmount = 8;
                break;
        }

        //heal
        int singularHeal = 0;

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
        }


        //Single heal?
        if (singleHealTarget != null)
        {
            //heal the target
            yield return new WaitForSeconds(0.5f);
            singleHealTarget.HealHealth(singleHealAmount);
            ApplyHealEffect(caller, singleHealTarget);
            yield return new WaitForSeconds(0.5f);
        }
    }

    public IEnumerator ShiftPositions(BattleEntity caller)
    {
        //choose an enemy
        caller.BasicTargetChooser(new TargetArea(TargetArea.TargetAreaType.LiveAllyShiftable));
        BattleEntity shiftTarget = caller.curTarget;

        if (shiftTarget == null)
        {
            yield break;
        }

        //Change last digit

        int posA = shiftTarget.posId;
        int posB = caller.posId;

        caller.posId = ((caller.posId / 10) * 10) + posA % 10;
        shiftTarget.posId = ((shiftTarget.posId / 10) * 10) + posB % 10;

        //shift
        shiftTarget.homePos = BattleHelper.GetDefaultPosition(shiftTarget.posId);
        caller.homePos = BattleHelper.GetDefaultPosition(caller.posId);

        bool movedoneA = false;
        bool movedoneB = false;

        IEnumerator MoveA()
        {
            yield return StartCoroutine(caller.Move(caller.homePos));
            movedoneA = true;
        }
        IEnumerator MoveB()
        {
            yield return StartCoroutine(shiftTarget.Move(shiftTarget.homePos));
            movedoneB = true;
        }
        StartCoroutine(MoveA());
        StartCoroutine(MoveB());
        yield return new WaitUntil(() => (movedoneA && movedoneB));
    }

    public virtual void ApplyHealEffect(BattleEntity caller, BattleEntity target)
    {
        if (BattleControl.Instance.GetCurseLevel() > 0)
        {
            caller.InflictEffect(target, new Effect(Effect.EffectType.Hustle, 1, 1));
        }
    }
}

public class BM_DesertBloom_SwoopHealHustle : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.DesertBloom_SwoopHealHustle;
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
                yield return StartCoroutine(caller.FollowBezierCurve(backFlag ? 0.3f : 0.2f, (float a) => MainManager.EasingQuadratic(a, -0.2f), positions));

                //yield return StartCoroutine(caller.Squish(0.067f, 0.2f));

                caller.curTarget.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.ReverseSquish);
                switch (caller.entityID)
                {
                    case BattleHelper.EntityID.DesertBloom:
                        caller.DealDamage(caller.curTarget, 9, BattleHelper.DamageType.Normal, 0, BattleHelper.ContactLevel.Contact);
                        break;
                    default:
                        caller.DealDamage(caller.curTarget, 2, BattleHelper.DamageType.Normal, 0, BattleHelper.ContactLevel.Contact);
                        break;
                }

                //StartCoroutine(caller.RevertScale(0.1f));
            }
            else
            {
                yield return StartCoroutine(caller.FollowBezierCurve(0.15f * dist, (float a) => MainManager.EasingQuadratic(a, -0.2f), 1.25f, positions));
                caller.InvokeMissEvents(caller.curTarget);
            }
        }

        yield return StartCoroutine(caller.MoveEasing(caller.homePos, (e) => MainManager.EasingOutIn(e)));

        yield return DoHeal(caller);

        yield return ShiftPositions(caller);
    }

    public IEnumerator DoHeal(BattleEntity caller)
    {
        int singleHealAmount = 0;
        int multiHealAmount = 0;

        switch (caller.entityID)
        {
            case BattleHelper.EntityID.DesertBloom:
                singleHealAmount = 8;
                multiHealAmount = 4;
                break;
        }

        //heal
        int singularHeal = 0;
        int spreadHeal = 0;

        //change: hardcoded to heal self if no other target exists :)
        BattleEntity singleHealTarget = caller; //note: if all targets have 0 missing hp then this will stay null (and so no heal)

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
            ApplyHealEffect(caller, caller);
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
                ApplyHealEffect(caller, singleHealTarget);
                yield return new WaitForSeconds(0.5f);
            }
        }

        if (caller.entityID == BattleHelper.EntityID.Honeybud)
        {
            if (caller.homePos.y > 0)
            {
                //Fall down
                float dist = caller.homePos.y;
                caller.homePos.y = 0;
                yield return StartCoroutine(caller.Jump(caller.homePos, 0, dist / 8));
                caller.SetEntityProperty(BattleHelper.EntityProperties.Grounded, true);
            }
        }
    }

    public IEnumerator ShiftPositions(BattleEntity caller)
    {
        //choose an enemy
        caller.BasicTargetChooser(new TargetArea(TargetArea.TargetAreaType.LiveAllyShiftable));
        BattleEntity shiftTarget = caller.curTarget;

        if (shiftTarget == null)
        {
            yield break;
        }

        //Change last digit

        int posA = shiftTarget.posId;
        int posB = caller.posId;

        caller.posId = ((caller.posId / 10) * 10) + posA % 10;
        shiftTarget.posId = ((shiftTarget.posId / 10) * 10) + posB % 10;

        //shift
        shiftTarget.homePos = BattleHelper.GetDefaultPosition(shiftTarget.posId);
        caller.homePos = BattleHelper.GetDefaultPosition(caller.posId);

        bool movedoneA = false;
        bool movedoneB = false;

        IEnumerator MoveA()
        {
            yield return StartCoroutine(caller.Move(caller.homePos));
            movedoneA = true;
        }
        IEnumerator MoveB()
        {
            yield return StartCoroutine(shiftTarget.Move(shiftTarget.homePos));
            movedoneB = true;
        }
        StartCoroutine(MoveA());
        StartCoroutine(MoveB());
        yield return new WaitUntil(() => (movedoneA && movedoneB));
    }

    public virtual void ApplyHealEffect(BattleEntity caller, BattleEntity target)
    {
        caller.InflictEffect(caller, new Effect(Effect.EffectType.Hustle, 1, 1));
        if (BattleControl.Instance.GetCurseLevel() > 0)
        {
            caller.InflictEffect(target, new Effect(Effect.EffectType.Hustle, 1, 1));
        }

    }
}

public class BE_Goldpole : BattleEntity
{
    //public bool grounded = false;
    public int counterCount = 0;

    public override void Initialize()
    {
        moveset = new List<Move> { gameObject.AddComponent<BM_Goldpole_QuickZap>(), gameObject.AddComponent<BM_Goldpole_GatherCharge>() };

        base.Initialize();
    }

    public override void ChooseMoveInternal()
    {
        counterCount = 0;
        currMove = moveset[1];
        BasicTargetChooser();
    }

    public override IEnumerator PostMove()
    {
        counterCount = 0;
        yield return StartCoroutine(base.PostMove());
    }

    public override void TryContactHazard(BattleEntity target, BattleHelper.ContactLevel contact, BattleHelper.DamageType type, int damage)
    {
        EnvironmentalContactHazards(target, contact, type, damage);

        //only airborne
        if (!GetEntityProperty(BattleHelper.EntityProperties.Airborne))
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
                DealDamage(target, 2, BattleHelper.DamageType.Normal, (ulong)BattleHelper.DamageProperties.StandardContactHazard, BattleHelper.ContactLevel.Contact);
                target.contactImmunityList.Add(posId);
            }
        }
    }

    public override IEnumerator DoEvent(BattleHelper.Event eventID)
    {
        yield return StartCoroutine(DefaultEventHandler_Flying(eventID));
    }

    public override IEnumerator PreReact(Move move, BattleEntity target)
    {
        counterCount = 0;

        Effect_ReactionCounter();

        yield return new WaitForSeconds(0.5f);
    }
    public override bool ReactToEvent(BattleEntity target, BattleHelper.Event e, int previousReactions)
    {
        if ((e == BattleHelper.Event.PostAction) && BattleControl.IsPlayerControlled(target, false) && counterCount <= 0)
        {
            counterCount++;
            BattleControl.Instance.AddReactionMoveEvent(this, target.lastAttacker, moveset[0]);
            return true;
        }

        return false;
    }
}

public class BM_Goldpole_QuickZap : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Goldpole_QuickZap;

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

            if (caller.GetAttackHit(caller.curTarget, BattleHelper.DamageType.Air))
            {
                yield return StartCoroutine(caller.FollowBezierCurve(backFlag ? 0.4f : 0.3f, (float a) => MainManager.EasingQuadratic(a, -0.2f), positions));

                if (caller.GetEntityProperty(BattleHelper.EntityProperties.Airborne))
                {
                    caller.DealDamage(caller.curTarget, 4, BattleHelper.DamageType.Air, 0, BattleHelper.ContactLevel.Contact);
                }
                else
                {
                    caller.DealDamage(caller.curTarget, 2, BattleHelper.DamageType.Air, 0, BattleHelper.ContactLevel.Contact);
                }
            }
            else
            {
                yield return StartCoroutine(caller.FollowBezierCurve(0.15f * dist, (float a) => MainManager.EasingQuadratic(a, -0.2f), 1.25f, positions));
                caller.InvokeMissEvents(caller.curTarget);
            }
        }

        yield return StartCoroutine(caller.MoveEasing(caller.homePos, (e) => MainManager.EasingOutIn(e)));
    }

    public override IEnumerator ExecuteOutOfTurn(BattleEntity caller, BattleEntity target, int level = 1)
    {
        caller.BasicTargetChooser(GetTargetArea(caller), 7, 5 + caller.actionCounter * 2);
        yield return StartCoroutine(Execute(caller, level));
    }
}

public class BM_Goldpole_GatherCharge : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Goldpole_GatherCharge;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.Self);

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        yield return StartCoroutine(caller.FlyingFlyBackUp());
        yield return StartCoroutine(caller.SpinHeavy(Vector3.up * 360, 0.25f));
        caller.InflictEffect(caller, new Effect(Effect.EffectType.Supercharge, 1, 3));
        caller.InflictEffect(caller, new Effect(Effect.EffectType.Focus, 2, Effect.INFINITE_DURATION));
        if (BattleControl.Instance.GetCurseLevel() > 0)
        {
            caller.InflictEffectCapped(caller, new Effect(Effect.EffectType.AttackBoost, 1, Effect.INFINITE_DURATION), 8);
            caller.InflictEffectCapped(caller, new Effect(Effect.EffectType.DefenseBoost, 1, Effect.INFINITE_DURATION), 8);
        }
    }
}

public class BE_Shockworm : BattleEntity
{
    public override void Initialize()
    {
        moveset = new List<Move> { gameObject.AddComponent<BM_Shockworm_ArcBite>(), gameObject.AddComponent<BM_Goldpole_GatherCharge>(), gameObject.AddComponent<BM_Shockworm_MatchFocus>() };

        base.Initialize();
    }

    public override void ChooseMoveInternal()
    {
        currMove = moveset[(posId + BattleControl.Instance.turnCount - 1) % 3];

        if (currMove == moveset[2])
        {
            //Can't copy anything (note: can't copy from self)
            if (CountPartyCumulativeBuffPotency() - CountBuffPotency() == 0)
            {
                currMove = moveset[0];
            }
        }

        BasicTargetChooser();
    }

    public override IEnumerator DoEvent(BattleHelper.Event eventID)
    {
        yield return StartCoroutine(DefaultEventHandler_Flying(eventID));
    }
}

public class BM_Shockworm_ArcBite : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Shockworm_ArcBite;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemy);

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        yield return StartCoroutine(caller.FlyingFlyBackUp());

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
                    case BattleHelper.EntityID.Shockworm:
                        caller.DealDamage(caller.curTarget, 5, BattleHelper.DamageType.Air, 0, BattleHelper.ContactLevel.Contact);
                        break;
                    default:
                        caller.DealDamage(caller.curTarget, 2, BattleHelper.DamageType.Normal, 0, BattleHelper.ContactLevel.Contact);
                        break;
                }

                if (BattleControl.Instance.GetCurseLevel() > 0)
                {
                    if (!caller.curTarget.HasAilment())
                    {
                        caller.InflictEffect(caller.curTarget, new Effect(Effect.EffectType.ArcDischarge, 1, 3));
                    }
                }

                yield return new WaitForSeconds(0.3f);
                foreach (BattleEntity target in BattleControl.Instance.GetEntitiesSorted(caller, GetTargetArea(caller)))
                {
                    if (target != caller.curTarget)
                    {
                        caller.DealDamage(target, 3, BattleHelper.DamageType.Air, 0, BattleHelper.ContactLevel.Contact);
                    }
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

public class BM_Shockworm_MatchFocus : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Shockworm_MatchFocus;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.Self);

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        yield return StartCoroutine(caller.SpinHeavy(Vector3.up * 360, 0.25f));

        caller.InflictEffectCapped(caller, new Effect(Effect.EffectType.Focus, (sbyte)(BattleControl.Instance.GetPartyCumulativeEffectPotency(caller.posId, Effect.EffectType.Focus) - caller.GetEffectPotency(Effect.EffectType.Focus)), Effect.INFINITE_DURATION), 10);

        //Apply everything >:)
        //(but not permanents)
        if (BattleControl.Instance.GetCurseLevel() > 0)
        {
            for (int i = 0; i < (int)Effect.EffectType.EndOfTable; i++)
            {
                int cap = 0;
                sbyte duration = 3;
                if (!Effect.IsCleanseable((Effect.EffectType)i))
                {
                    continue;
                }

                Effect.EffectType e = (Effect.EffectType)i;

                if (e == Effect.EffectType.Focus) {
                    continue;
                }

                if (Effect.GetEffectClass(e) == Effect.EffectClass.Token)
                {
                    duration = Effect.INFINITE_DURATION;
                }

                if (BattleControl.Instance.GetPartyCumulativeEffectPotency(caller.posId, e) - caller.GetEffectPotency(e) == 0)
                {
                    continue;
                }

                if (cap == 0)
                {
                    caller.InflictEffect(caller, new Effect(e, (sbyte)(BattleControl.Instance.GetPartyCumulativeEffectPotency(caller.posId, e) - caller.GetEffectPotency(e)), duration));
                }
                else
                {
                    caller.InflictEffectCapped(caller, new Effect(e, (sbyte)(BattleControl.Instance.GetPartyCumulativeEffectPotency(caller.posId, e) - caller.GetEffectPotency(e)), duration), cap);
                }
            }
        }
    }
}

public class BE_Brightpole : BattleEntity
{
    public override void Initialize()
    {
        moveset = new List<Move> { gameObject.AddComponent<BM_Brightpole_FlashDarkness>(), gameObject.AddComponent<BM_Brightpole_ElectroWave>() };

        base.Initialize();
    }

    public override void ChooseMoveInternal()
    {
        currMove = moveset[((posId) + BattleControl.Instance.turnCount - 1) % 3 == 0 ? 1 : 0];
        BasicTargetChooser();
    }

    public override void TryContactHazard(BattleEntity target, BattleHelper.ContactLevel contact, BattleHelper.DamageType type, int damage)
    {
        EnvironmentalContactHazards(target, contact, type, damage);

        //only airborne
        if (!GetEntityProperty(BattleHelper.EntityProperties.Airborne))
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
                DealDamage(target, 2, BattleHelper.DamageType.Normal, (ulong)BattleHelper.DamageProperties.StandardContactHazard, BattleHelper.ContactLevel.Contact);
                target.contactImmunityList.Add(posId);
            }
        }
    }

    public override IEnumerator DoEvent(BattleHelper.Event eventID)
    {
        yield return StartCoroutine(DefaultEventHandler_Flying(eventID));
    }
}

public class BM_Brightpole_FlashDarkness : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Brightpole_FlashDarkness;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemy);

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        //Debug.Log(caller.id + " jump");
        if (!BattleControl.Instance.EntityValid(caller.curTarget))
        {
            caller.curTarget = null;
        }

        //fly back up        
        yield return StartCoroutine(caller.FlyingFlyBackUp());

        //light flash
        if (!BattleControl.Instance.EntityValid(caller.curTarget))
        {
            caller.curTarget = null;
        }

        yield return StartCoroutine(caller.SpinHeavy(Vector3.up * 360, 0.5f));

        List<BattleEntity> targets = BattleControl.Instance.GetEntitiesSorted(caller, GetBaseTarget());

        foreach (BattleEntity t in targets)
        {
            if (caller.GetAttackHit(t, BattleHelper.DamageType.Light))
            {
                t.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.Spin);
                caller.DealDamage(t, 4, BattleHelper.DamageType.Light, 0, BattleHelper.ContactLevel.Infinite);
            }
            else
            {
                //Miss
                caller.InvokeMissEvents(t);
            }
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
                yield return StartCoroutine(caller.FollowBezierCurve(backFlag ? 0.2f : 0.15f, (float a) => MainManager.EasingQuadratic(a, -0.2f), positions));
                caller.DealDamage(caller.curTarget, 5, BattleHelper.DamageType.Dark, 0, BattleHelper.ContactLevel.Contact);
                caller.InflictEffect(caller, new Effect(Effect.EffectType.QuantumShield, 1, 3));
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

public class BM_Brightpole_ElectroWave : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Brightpole_ElectroWave;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemy);

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        //Debug.Log(caller.id + " jump");
        if (!BattleControl.Instance.EntityValid(caller.curTarget))
        {
            caller.curTarget = null;
        }

        //fly back up        
        yield return StartCoroutine(caller.FlyingFlyBackUp());

        //light flash
        yield return StartCoroutine(caller.SpinHeavy(Vector3.up * 360, 0.5f));

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

        yield return StartCoroutine(caller.MoveEasing(caller.homePos, (e) => MainManager.EasingOutIn(e)));
    }
}

public class BE_Stormswimmer : BattleEntity
{
    public override void Initialize()
    {
        moveset = new List<Move> { gameObject.AddComponent<BM_Stormswimmer_TripleBolt>(), gameObject.AddComponent<BM_Stormswimmer_MistWave>(), gameObject.AddComponent<BM_Stormswimmer_ShiftySwoop>(), gameObject.AddComponent<BM_Stormswimmer_Hard_MistRain>() };

        base.Initialize();
    }

    public override void ChooseMoveInternal()
    {
        if (BattleControl.Instance.GetCurseLevel() > 0)
        {
            currMove = moveset[(posId + BattleControl.Instance.turnCount - 1) % (moveset.Count)];
        }
        else
        {
            currMove = moveset[(posId + BattleControl.Instance.turnCount - 1) % (moveset.Count - 1)];
        }

        BasicTargetChooser();
    }

    public override IEnumerator DoEvent(BattleHelper.Event eventID)
    {
        yield return StartCoroutine(DefaultEventHandler_Flying(eventID));
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
                DealDamage(target, 2, BattleHelper.DamageType.Earth, (ulong)BattleHelper.DamageProperties.StandardContactHazard, BattleHelper.ContactLevel.Contact);
                target.contactImmunityList.Add(posId);
            }
        }
    }
}

public class BM_Stormswimmer_TripleBolt : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Stormswimmer_TripleBolt;
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
            for (int i = 0; i < 3; i++)
            {
                yield return StartCoroutine(caller.SpinHeavy(Vector3.up * 360, 0.175f));

                if (caller.GetAttackHit(caller.curTarget, BattleHelper.DamageType.Air))
                {
                    caller.curTarget.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.ReverseSquish);
                    caller.DealDamage(caller.curTarget, 4, BattleHelper.DamageType.Air, 0, BattleHelper.ContactLevel.Contact);
                }
                else
                {
                    caller.InvokeMissEvents(caller.curTarget);
                }
            }
        }

        yield return StartCoroutine(caller.MoveEasing(caller.homePos, (e) => MainManager.EasingOutIn(e)));
    }
}

public class BM_Stormswimmer_MistWave : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Stormswimmer_MistWave;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemy);

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        yield return StartCoroutine(caller.FlyingFlyBackUp());

        if (!BattleControl.Instance.EntityValid(caller.curTarget))
        {
            caller.curTarget = null;
        }

        yield return StartCoroutine(caller.SpinHeavy(Vector3.up * 360, 1f));

        List<BattleEntity> targets = BattleControl.Instance.GetEntitiesSorted(caller, GetBaseTarget());

        caller.InflictEffect(caller, new Effect(Effect.EffectType.MistWall, 1, 3));
        foreach (BattleEntity t in targets)
        {
            if (caller.GetAttackHit(t, BattleHelper.DamageType.Water))
            {
                t.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.Spin);
                caller.DealDamage(t, 3, BattleHelper.DamageType.Water, 0, BattleHelper.ContactLevel.Infinite);
            }
            else
            {
                //Miss
                caller.InvokeMissEvents(t);
            }
        }
    }
}

public class BM_Stormswimmer_ShiftySwoop : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Stormswimmer_ShiftySwoop;
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
                yield return StartCoroutine(caller.FollowBezierCurve(backFlag ? 0.3f : 0.2f, (float a) => MainManager.EasingQuadratic(a, -0.2f), positions));

                //yield return StartCoroutine(caller.Squish(0.067f, 0.2f));

                caller.curTarget.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.ReverseSquish);
                switch (caller.entityID)
                {
                    case BattleHelper.EntityID.DesertBloom:
                        caller.DealDamage(caller.curTarget, 9, BattleHelper.DamageType.Normal, 0, BattleHelper.ContactLevel.Contact);
                        break;
                    default:
                        caller.DealDamage(caller.curTarget, 2, BattleHelper.DamageType.Normal, 0, BattleHelper.ContactLevel.Contact);
                        break;
                }

                //StartCoroutine(caller.RevertScale(0.1f));
            }
            else
            {
                yield return StartCoroutine(caller.FollowBezierCurve(0.15f * dist, (float a) => MainManager.EasingQuadratic(a, -0.2f), 1.25f, positions));
                caller.InvokeMissEvents(caller.curTarget);
            }
        }

        yield return StartCoroutine(caller.MoveEasing(caller.homePos, (e) => MainManager.EasingOutIn(e)));

        yield return ShiftPositions(caller);
    }

    public IEnumerator ShiftPositions(BattleEntity caller)
    {
        //choose an enemy
        caller.BasicTargetChooser(new TargetArea(TargetArea.TargetAreaType.LiveAllyShiftable));
        BattleEntity shiftTarget = caller.curTarget;

        if (shiftTarget == null)
        {
            yield break;
        }

        //Change last digit

        int posA = shiftTarget.posId;
        int posB = caller.posId;

        caller.posId = ((caller.posId / 10) * 10) + posA % 10;
        shiftTarget.posId = ((shiftTarget.posId / 10) * 10) + posB % 10;

        Debug.Log("caller " + caller.posId + " <- " + posB);
        Debug.Log("shifter " + shiftTarget.posId + " <- " + posA);

        //shift
        shiftTarget.homePos = BattleHelper.GetDefaultPosition(shiftTarget.posId);
        caller.homePos = BattleHelper.GetDefaultPosition(caller.posId);

        bool movedoneA = false;
        bool movedoneB = false;

        IEnumerator MoveA()
        {
            yield return StartCoroutine(caller.Move(caller.homePos));
            movedoneA = true;
        }
        IEnumerator MoveB()
        {
            yield return StartCoroutine(shiftTarget.Move(shiftTarget.homePos));
            movedoneB = true;
        }
        StartCoroutine(MoveA());
        StartCoroutine(MoveB());
        yield return new WaitUntil(() => (movedoneA && movedoneB));
    }
}

public class BM_Stormswimmer_Hard_MistRain : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Stormswimmer_Hard_MistRain;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveAlly);

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        yield return StartCoroutine(caller.FlyingFlyBackUp());

        if (!BattleControl.Instance.EntityValid(caller.curTarget))
        {
            caller.curTarget = null;
        }

        yield return StartCoroutine(caller.SpinHeavy(Vector3.up * 360, 1f));

        List<BattleEntity> targets = BattleControl.Instance.GetEntitiesSorted(caller, GetBaseTarget());

        foreach (BattleEntity t in targets)
        {
            caller.InflictEffect(t, new Effect(Effect.EffectType.MistWall, 1, 3));
        }
    }
}