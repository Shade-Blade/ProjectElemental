using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BE_EyeSpore : BattleEntity
{
    int counterCount;
    public override void Initialize()
    {
        moveset = new List<Move> { gameObject.AddComponent<BM_EyeSpore_SporeBeam>(), gameObject.AddComponent<BM_EyeSpore_Hard_CounterSpiteBeam>() };

        base.Initialize();
    }

    public override void ChooseMoveInternal()
    {
        //also reset here in case something weird happens
        counterCount = 0;
        currMove = moveset[0];

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
            BattleControl.Instance.AddReactionMoveEvent(this, target.lastAttacker, moveset[1]);
            return true;
        }

        return false;
    }
}

public class BM_EyeSpore_SporeBeam : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.EyeSpore_SporeBeam;

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
            if (caller.GetAttackHit(caller.curTarget, BattleHelper.DamageType.Dark))
            {
                caller.DealDamage(caller.curTarget, 2, BattleHelper.DamageType.Dark, 0, BattleHelper.ContactLevel.Infinite);
            }
            else
            {
                caller.InvokeMissEvents(caller.curTarget);
            }
        }
    }
}

public class BM_EyeSpore_Hard_CounterSpiteBeam : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.EyeSpore_Hard_CounterSpiteBeam;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemy);

    public override IEnumerator ExecuteOutOfTurn(BattleEntity caller, BattleEntity target, int level = 1)
    {
        caller.curTarget = target;
        if (!BattleControl.Instance.EntityValid(caller.curTarget))
        {
            caller.curTarget = null;
        }

        yield return StartCoroutine(caller.Spin(Vector3.up * 360, 0.5f));


        if (caller.curTarget != null)
        {
            if (caller.GetAttackHit(caller.curTarget, BattleHelper.DamageType.Dark))
            {
                caller.DealDamage(caller.curTarget, 1, BattleHelper.DamageType.Dark, 0, BattleHelper.ContactLevel.Infinite);
                if (BattleControl.Instance.GetCurseLevel() > 0)
                {
                    caller.InflictEffect(caller.curTarget, new Effect(Effect.EffectType.Defocus, 1, 255));
                }
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

public class BE_SpikeShroom : BattleEntity
{
    public override void Initialize()
    {
        //This is pretty sus
        moveset = new List<Move> { gameObject.AddComponent<BM_SpikeSpore_PoisonSpikes>(), gameObject.AddComponent<BM_SpikeSpore_Hard_SpikeBomb>() };
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
            //note: posId >= 0 for enemies
            if (BattleControl.Instance.GetCurseLevel() > 0)
            {
                currMove = moveset[(posId  + BattleControl.Instance.turnCount - 1) % moveset.Count];
            }
            else
            {
                currMove = moveset[(posId + BattleControl.Instance.turnCount - 1) % (moveset.Count - 1)];
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
                DealDamage(target, 3, BattleHelper.DamageType.Normal, (ulong)BattleHelper.DamageProperties.StandardContactHazard, BattleHelper.ContactLevel.Contact);
                target.contactImmunityList.Add(posId);
            }
        }
    }
}

public class BM_SpikeSpore_PoisonSpikes : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.SpikeSpore_PoisonSpikes;

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
                bool hasStatus = caller.curTarget.HasStatus();
                caller.DealDamage(caller.curTarget, 5, BattleHelper.DamageType.Normal, 0, BattleHelper.ContactLevel.Contact);
                if (!hasStatus)
                {
                    caller.InflictEffect(caller.curTarget, new Effect(Effect.EffectType.Poison, 1, 3));
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

public class BM_SpikeSpore_Hard_SpikeBomb : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.SpikeSpore_Hard_SpikeBomb;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemy);

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        if (!BattleControl.Instance.EntityValid(caller.curTarget))
        {
            caller.curTarget = null;
        }

        yield return StartCoroutine(caller.Spin(Vector3.up * 360, 0.5f));

        List<BattleEntity> targets = BattleControl.Instance.GetEntitiesSorted(caller, GetBaseTarget());
        caller.InflictEffectBuffered(caller, new Effect(Effect.EffectType.Defocus, 1, 255));
        caller.InflictEffectBuffered(caller, new Effect(Effect.EffectType.Sunder, 1, 255));
        foreach (BattleEntity t in targets)
        {
            if (caller.GetAttackHit(t, BattleHelper.DamageType.Dark))
            {
                caller.DealDamage(t, 2, BattleHelper.DamageType.Dark, 0, BattleHelper.ContactLevel.Infinite);
                caller.InflictEffect(t, new Effect(Effect.EffectType.Defocus, 1, 255));
                caller.InflictEffect(t, new Effect(Effect.EffectType.Sunder, 1, 255));
            }
            else
            {
                //Miss
                caller.InvokeMissEvents(t);
            }
        }
    }
}

public class BE_Shrouder : BattleEntity
{
    int counterCount;

    public override void Initialize()
    {
        moveset = new List<Move> { gameObject.AddComponent<BM_Shrouder_SporeCloud>(), gameObject.AddComponent<BM_Shrouder_SporeCloak>(), gameObject.AddComponent<BM_Shared_Hard_CounterHide>() };

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
            } else
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

public class BM_Shrouder_SporeCloud : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Shrouder_SporeCloud;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemy);

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        //fly back up        
        yield return StartCoroutine(caller.FlyingFlyBackUp());

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
            List<BattleEntity> targets = BattleControl.Instance.GetEntitiesSorted(caller, GetBaseTarget());
            foreach (BattleEntity t in targets)
            {
                if (caller.GetAttackHit(t, 0))
                {
                    caller.DealDamage(t, 4, 0, 0, BattleHelper.ContactLevel.Infinite);
                    caller.InflictEffect(t, new Effect(Effect.EffectType.EnduranceDown, 2, 3));
                }
                else
                {
                    //Miss
                    caller.InvokeMissEvents(t);
                }
            }
        }

        yield return StartCoroutine(caller.Move(caller.homePos));
    }
}

public class BM_Shrouder_SporeCloak : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Shrouder_SporeCloak;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveAlly);

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        if (!BattleControl.Instance.EntityValid(caller.curTarget))
        {
            caller.curTarget = null;
        }

        yield return StartCoroutine(caller.Spin(Vector3.up * 360, 0.5f));

        caller.InflictEffect(caller.curTarget, new Effect(Effect.EffectType.Ethereal, 1, 2));
    }
}

public class BE_HoarderFly : BattleEntity
{
    int counterCount;

    public override void Initialize()
    {
        moveset = new List<Move> { gameObject.AddComponent<BM_HoarderFly_PoisonHeal>(), gameObject.AddComponent<BM_Shared_TripleSwoop>(), gameObject.AddComponent<BM_HoarderFly_Hard_FinalHeal>() };

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
        if (BattleControl.Instance.GetCurseLevel() <= 0)
        {
            return false;
        }

        if (e == BattleHelper.Event.Death && target == this && counterCount <= 0)
        {
            counterCount++;
            BattleControl.Instance.AddReactionMoveEvent(this, target.lastAttacker, moveset[2]);
            return true;
        }

        return false;
    }
}

public class BM_HoarderFly_PoisonHeal : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.HoarderFly_PoisonHeal;

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

                //this order is so that it doesn't trigger the revive code thing
                caller.curTarget.HealHealth(3);
                bool hasStatus = caller.curTarget.HasStatus();
                caller.DealDamage(caller.curTarget, 0, BattleHelper.DamageType.Normal, 0, BattleHelper.ContactLevel.Contact);
                if (!hasStatus)
                {
                    caller.InflictEffect(caller.curTarget, new Effect(Effect.EffectType.Poison, 1, 3));
                }
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

public class BM_HoarderFly_Hard_FinalHeal : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.HoarderFly_Hard_FinalHeal;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveAlly);

    public override IEnumerator ExecuteOutOfTurn(BattleEntity caller, BattleEntity target, int level = 1)
    {
        caller.curTarget = target;
        if (!BattleControl.Instance.EntityValid(caller.curTarget))
        {
            caller.curTarget = null;
        }

        yield return StartCoroutine(caller.Spin(Vector3.up * 360, 0.5f));

        List<BattleEntity> targets = BattleControl.Instance.GetEntitiesSorted(caller, GetBaseTarget());

        foreach (BattleEntity be in targets)
        {
            if (be != caller)
            {
                be.HealHealth(4);
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

public class BE_Mosquito : BattleEntity
{
    public override void Initialize()
    {
        moveset = new List<Move> { gameObject.AddComponent<BM_Mosquito_ShockNeedle>(), gameObject.AddComponent<BM_Mosquito_DrainBite>(), gameObject.AddComponent<BM_Mosquito_Hard_Shroud>() };

        base.Initialize();
    }

    public override void ChooseMoveInternal()
    {
        if (BattleControl.Instance.GetCurseLevel() > 0)
        {
            //logic
            //note: % 3 + 1 prevents shroud being used turn 1
            //also prevents them from being too far from the shroud turn (3 turns ish may make it possible to avoid the shroud but that may not be a problem)
            int offset = ((posId % 3) + 1 + BattleControl.Instance.turnCount - 1) % 5;

            if (offset == 4)
            {
                currMove = moveset[2];
            }
            else
            {
                currMove = moveset[offset % 2];
            }
        }
        else
        {
            currMove = moveset[(posId + BattleControl.Instance.turnCount - 1) % 2];
        }

        if (currMove == moveset[1])
        {
            SpecialTargetChooserFirst(TargetStrategy.TargetStrategyType.HighHP);
        } else
        {
            BasicTargetChooser();
        }
    }
}

public class BM_Mosquito_ShockNeedle : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Mosquito_ShockNeedle;

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

            if (Random.Range(0, 1) > 0.5f)
            {
                yield return StartCoroutine(caller.Spin(Vector3.up * 360, 0.75f));
            }
            else
            {
                yield return StartCoroutine(caller.Spin(Vector3.up * 360, 0.25f));
            }

            if (caller.GetAttackHit(caller.curTarget, 0))
            {
                bool hasStatus = caller.curTarget.HasStatus();
                caller.DealDamage(caller.curTarget, 4, BattleHelper.DamageType.Normal, 0, BattleHelper.ContactLevel.Contact);
                if (!hasStatus)
                {
                    caller.InflictEffect(caller.curTarget, new Effect(Effect.EffectType.Paralyze, 1, 2));
                }
                caller.InflictEffect(caller.curTarget, new Effect(Effect.EffectType.Sunder, 2, 255));
            }
            else
            {
                caller.InvokeMissEvents(caller.curTarget);
            }
        }

        yield return StartCoroutine(caller.Move(caller.homePos));
    }
}

public class BM_Mosquito_DrainBite : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Mosquito_DrainBite;

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

            if (Random.Range(0, 1) > 0.5f)
            {
                yield return StartCoroutine(caller.Spin(Vector3.up * 360, 0.75f));
            }
            else
            {
                yield return StartCoroutine(caller.Spin(Vector3.up * 360, 0.25f));
            }

            if (caller.GetAttackHit(caller.curTarget, 0))
            {
                caller.DealDamage(caller.curTarget, 5, BattleHelper.DamageType.Normal, (uint)BattleHelper.DamageProperties.HPDrainOneToOne, BattleHelper.ContactLevel.Contact);
            }
            else
            {
                caller.InvokeMissEvents(caller.curTarget);
            }
        }

        yield return StartCoroutine(caller.Move(caller.homePos));
    }
}

public class BM_Mosquito_Hard_Shroud : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Mosquito_Hard_Shroud;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.Self);

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        yield return StartCoroutine(caller.Spin(Vector3.up * 360, 0.25f));
        caller.InflictEffect(caller, new Effect(Effect.EffectType.Ethereal, 1, 2));
    }
}