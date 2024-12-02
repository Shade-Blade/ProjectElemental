using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static EnemyMove;

public class BE_Sunflower : BattleEntity
{
    public override void Initialize()
    {
        moveset = new List<Move> { gameObject.AddComponent<BM_Shared_SwoopDown>(), gameObject.AddComponent<BM_Shared_BiteThenFly>(), gameObject.AddComponent<BM_Sunflower_Hard_SolarBite>() };

        base.Initialize();
    }

    public override void ChooseMoveInternal()
    {
        if (moveset.Count == 0)
        {
            currMove = null;
        }
        else if (moveset.Count == 1)
        {
            if (BattleControl.Instance.GetCurseLevel() > 0)
            {
                currMove = moveset[2];
            }
            else
            {
                currMove = moveset[0];
            }
        }
        else
        {
            if (GetEntityProperty(BattleHelper.EntityProperties.Grounded))
            {
                currMove = moveset[1];
            }
            else
            {
                if (BattleControl.Instance.GetCurseLevel() > 0)
                {
                    currMove = moveset[2];
                }
                else
                {
                    currMove = moveset[0];
                }
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
                DealDamage(target, 3, BattleHelper.DamageType.Fire, (ulong)BattleHelper.DamageProperties.StandardContactHazard, BattleHelper.ContactLevel.Contact);
                target.contactImmunityList.Add(posId);
            }
        }
    }

    public override IEnumerator DoEvent(BattleHelper.Event eventID)
    {
        yield return StartCoroutine(DefaultEventHandler_Flying(eventID));
    }
}

public class BE_Sunnybud : BattleEntity
{
    int counterCount;
    public override void Initialize()
    {
        moveset = new List<Move> { gameObject.AddComponent<BM_Honeybud_SwoopHealIlluminate>(), gameObject.AddComponent<BM_Honeybud_FrontBiteIlluminate>(), gameObject.AddComponent<BM_Shared_Hard_CounterRush>() };

        base.Initialize();

        if (BattleControl.Instance.GetCurseLevel() > 0)
        {
            SetEntityProperty(BattleHelper.EntityProperties.StateCounter, true);
        }
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

public class BE_MiracleBloom : BattleEntity
{
    int counterCount;
    public override void Initialize()
    {
        moveset = new List<Move> { gameObject.AddComponent<BM_Honeybud_SwoopHealMiracle>(), gameObject.AddComponent<BM_Honeybud_FrontBiteMiracle>(), gameObject.AddComponent<BM_Shared_Hard_CounterRush>() };

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
public class BM_Honeybud_FrontBite : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Sunnybud_BiteFlyHeal;

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

                switch (caller.entityID)
                {
                    case BattleHelper.EntityID.Sunnybud:
                        caller.DealDamage(caller.curTarget, 6, BattleHelper.DamageType.Normal, 0, BattleHelper.ContactLevel.Contact);
                        break;
                    case BattleHelper.EntityID.MiracleBloom:
                        caller.DealDamage(caller.curTarget, 9, BattleHelper.DamageType.Normal, 0, BattleHelper.ContactLevel.Contact);
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

        yield return StartCoroutine(caller.Move(caller.homePos));

        //fly back up        
        yield return StartCoroutine(caller.FlyingFlyBackUp());

        yield return DoHeal(caller);
    }

    public IEnumerator DoHeal(BattleEntity caller)
    {
        int singleHealAmount = 0;

        switch (caller.entityID)
        {
            case BattleHelper.EntityID.Honeybud:
                singleHealAmount = 2;
                break;
            case BattleHelper.EntityID.Sunnybud:
                singleHealAmount = 4;
                break;
            case BattleHelper.EntityID.MiracleBloom:
                singleHealAmount = 16;
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

    public virtual void ApplyHealEffect(BattleEntity caller, BattleEntity target)
    {

    }
}

public class BM_Honeybud_FrontBiteIlluminate : BM_Honeybud_FrontBite
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Sunnybud_BiteFlyHealIlluminate;
    public override void ApplyHealEffect(BattleEntity caller, BattleEntity target)
    {
        if (BattleControl.Instance.GetCurseLevel() > 0)
        {
            caller.InflictEffect(target, new Effect(Effect.EffectType.Illuminate, 1, 3));
        }
    }
}

public class BM_Honeybud_FrontBiteMiracle : BM_Honeybud_FrontBite
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Sunnybud_BiteFlyHealMiracle;
    public override void ApplyHealEffect(BattleEntity caller, BattleEntity target)
    {
        if (BattleControl.Instance.GetCurseLevel() > 0)
        {
            caller.InflictEffect(target, new Effect(Effect.EffectType.Miracle, 1, Effect.INFINITE_DURATION));
        }
    }
}

public class BM_Honeybud_SwoopHeal : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Honeybud_SwoopHeal;
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

            yield return StartCoroutine(caller.Move(tposA));

            Vector3[] positions = new Vector3[] { tposA, tposmid, tposend };

            if (caller.GetAttackHit(caller.curTarget, 0))
            {
                yield return StartCoroutine(caller.FollowBezierCurve(backFlag ? 0.3f : 0.2f, (float a) => MainManager.EasingQuadratic(a, -0.2f), positions));

                //yield return StartCoroutine(caller.Squish(0.067f, 0.2f));

                switch (caller.entityID)
                {
                    case BattleHelper.EntityID.Honeybud:
                        caller.DealDamage(caller.curTarget, 2, BattleHelper.DamageType.Normal, 0, BattleHelper.ContactLevel.Contact);
                        break;
                    case BattleHelper.EntityID.Sunnybud:
                        caller.DealDamage(caller.curTarget, 3, BattleHelper.DamageType.Normal, 0, BattleHelper.ContactLevel.Contact);
                        break;
                    case BattleHelper.EntityID.Honeywing:
                        caller.DealDamage(caller.curTarget, 4, BattleHelper.DamageType.Normal, 0, BattleHelper.ContactLevel.Contact);
                        break;
                    case BattleHelper.EntityID.MiracleBloom:
                        caller.DealDamage(caller.curTarget, 8, BattleHelper.DamageType.Normal, 0, BattleHelper.ContactLevel.Contact);
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

        yield return StartCoroutine(caller.Move(caller.homePos));


        yield return DoHeal(caller);
    }

    public IEnumerator DoHeal(BattleEntity caller)
    {
        int singleHealAmount = 0;
        int multiHealAmount = 0;

        switch (caller.entityID)
        {
            case BattleHelper.EntityID.Honeybud:
                singleHealAmount = 2;
                multiHealAmount = 1;
                break;
            case BattleHelper.EntityID.Sunnybud:
            case BattleHelper.EntityID.Honeywing:
                singleHealAmount = 4;
                multiHealAmount = 2;
                break;
            case BattleHelper.EntityID.MiracleBloom:
                singleHealAmount = 16;
                multiHealAmount = 8;
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

    public virtual void ApplyHealEffect(BattleEntity caller, BattleEntity target)
    {

    }
}

public class BM_Honeybud_SwoopHealIlluminate : BM_Honeybud_SwoopHeal
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Honeybud_SwoopHealIlluminate;

    public override void ApplyHealEffect(BattleEntity caller, BattleEntity target)
    {
        if (BattleControl.Instance.GetCurseLevel() > 0)
        {
            caller.InflictEffect(target, new Effect(Effect.EffectType.Illuminate, 1, 3));
        }
    }
}

public class BM_Honeybud_SwoopHealMiracle : BM_Honeybud_SwoopHeal
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Honeybud_SwoopHealMiracle;

    public override void ApplyHealEffect(BattleEntity caller, BattleEntity target)
    {
        if (BattleControl.Instance.GetCurseLevel() > 0)
        {
            caller.InflictEffect(target, new Effect(Effect.EffectType.Miracle, 1, Effect.INFINITE_DURATION));
        }
    }
}