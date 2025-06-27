using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BE_Rockling : BattleEntity
{
    int counterCount;
    public override void Initialize()
    {
        moveset = new List<Move> { gameObject.AddComponent<BM_Shared_Bite>(), gameObject.AddComponent<BM_Shared_Hard_CounterHarden>() };

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
            BattleControl.Instance.AddReactionMoveEvent(this, target.lastAttacker, moveset[1]);
            return true;
        }

        return false;
    }
}

public class BE_Honeybud : BattleEntity
{
    int counterCount = 0;
    public override void Initialize()
    {
        moveset = new List<Move> { gameObject.AddComponent<BM_Honeybud_SwoopHeal>(), gameObject.AddComponent<BM_Shared_BiteThenFly>(), gameObject.AddComponent<BM_Shared_Hard_CounterRush>() };

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

        BasicOffsetTargetChooser(2,3);
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

public class BE_BurrowTrap : BattleEntity
{
    int counterCount = 0;
    public override void Initialize()
    {
        moveset = new List<Move> { gameObject.AddComponent<BM_BurrowTrap_PollenBite>(), gameObject.AddComponent<BM_BurrowTrap_CounterPollenBite>(), gameObject.AddComponent<BM_BurrowTrap_Hard_SunBloom>() };

        base.Initialize();
    }

    public override void ChooseMoveInternal()
    {
        counterCount = 0;

        if (BattleControl.Instance.GetCurseLevel() > 0)
        {
            currMove = moveset[(posId + BattleControl.Instance.turnCount - 1) % 3 == 0 ? 2 : 0];
        } else
        {
            currMove = moveset[0];
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

        Effect_ReactionCounter();

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
        if ((e == BattleHelper.Event.Hurt || e == BattleHelper.Event.Death) && target == this && counterCount <= 0)
        {
            counterCount++;
            BattleControl.Instance.AddReactionMoveEvent(this, target.lastAttacker, moveset[1]);
            return true;
        }

        return false;
    }
}

public class BM_BurrowTrap_PollenBite : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.BurrowTrap_PollenBite;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemy);

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        if (!BattleControl.Instance.EntityValid(caller.curTarget))
        {
            caller.curTarget = null;
        }

        Vector3 bpos = caller.homePos + caller.curTarget.height * 2 * Vector3.down;
        yield return StartCoroutine(caller.MoveEasing(bpos, 6, (e) => MainManager.EasingIn(e)));

        if (caller.curTarget != null)
        {
            Vector3 spos = caller.curTarget.transform.position - caller.curTarget.transform.position.y * Vector3.up + bpos.y * Vector3.up + caller.curTarget.width * 0.5f * Vector3.right;
            caller.transform.position = spos;
            Vector3 tpos2 = caller.curTarget.transform.position - caller.curTarget.transform.position.y * Vector3.up + caller.curTarget.width * 0.5f * Vector3.right;

            yield return StartCoroutine(caller.MoveEasing(tpos2, 6, (e) => MainManager.EasingIn(e)));

            if (caller.GetAttackHit(caller.curTarget, 0))
            {
                bool hasStatus = caller.curTarget.HasStatus();
                caller.curTarget.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.ReverseSquish);
                caller.DealDamage(caller.curTarget, 2, BattleHelper.DamageType.Normal, 0, BattleHelper.ContactLevel.Contact);
                if (!hasStatus)
                {
                    caller.InflictEffect(caller.curTarget, new Effect(Effect.EffectType.Dizzy, 1, 2));
                }
            }
            else
            {
                caller.InvokeMissEvents(caller.curTarget);
            }

            yield return StartCoroutine(caller.MoveEasing(spos, 6, (e) => MainManager.EasingIn(e)));
        }

        caller.transform.position = bpos;
        yield return StartCoroutine(caller.MoveEasing(caller.homePos, (e) => MainManager.EasingIn(e)));
    }
}

public class BM_BurrowTrap_CounterPollenBite : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.BurrowTrap_CounterPollenBite;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemy);

    public override IEnumerator ExecuteOutOfTurn(BattleEntity caller, BattleEntity causer, int level = 1)
    {
        caller.curTarget = causer;
        if (!BattleControl.Instance.EntityValid(caller.curTarget))
        {
            caller.curTarget = null;
        }

        Vector3 bpos = caller.homePos + caller.curTarget.height * 2 * Vector3.down;
        yield return StartCoroutine(caller.MoveEasing(bpos, 6, (e) => MainManager.EasingIn(e)));

        if (caller.curTarget != null)
        {
            Vector3 spos = caller.curTarget.transform.position - caller.curTarget.transform.position.y * Vector3.up + bpos.y * Vector3.up + caller.curTarget.width * 0.5f * Vector3.right;
            caller.transform.position = spos;
            Vector3 tpos2 = caller.curTarget.transform.position - caller.curTarget.transform.position.y * Vector3.up + caller.curTarget.width * 0.5f * Vector3.right;

            yield return StartCoroutine(caller.MoveEasing(tpos2, 6, (e) => MainManager.EasingIn(e)));

            if (caller.GetAttackHit(caller.curTarget, 0))
            {
                bool hasStatus = caller.curTarget.HasStatus();
                caller.curTarget.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.ReverseSquish);
                caller.DealDamage(caller.curTarget, 1, BattleHelper.DamageType.Normal, 0, BattleHelper.ContactLevel.Contact);
                if (!hasStatus)
                {
                    caller.InflictEffect(caller.curTarget, new Effect(Effect.EffectType.Dizzy, 1, 2));
                }
            }
            else
            {
                caller.InvokeMissEvents(caller.curTarget);
            }

            if (caller.hp == 0)
            {
                caller.SetAnimation("dead");
                yield return StartCoroutine(caller.DefaultDeathEvent());
                yield break;
            }
            yield return StartCoroutine(caller.MoveEasing(spos, 6, (e) => MainManager.EasingIn(e)));
        }

        if (caller.hp == 0)
        {
            caller.SetAnimation("dead");
            yield return StartCoroutine(caller.DefaultDeathEvent());
            yield break;
        }

        caller.transform.position = bpos;
        yield return StartCoroutine(caller.MoveEasing(caller.homePos, (e) => (MainManager.EasingIn(e))));
    }
}

public class BM_BurrowTrap_Hard_SunBloom : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.BurrowTrap_Hard_SunBloom;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.Self);

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        yield return StartCoroutine(caller.SpinHeavy(Vector3.up * 360, 0.25f));
        caller.InflictEffect(caller, new Effect(Effect.EffectType.Illuminate, 1, 3));
    }
}

public class BE_Sundew : BattleEntity
{
    int counterCount = 0;
    public override void Initialize()
    {
        moveset = new List<Move> { gameObject.AddComponent<BM_Sundew_PoisonToss>(), gameObject.AddComponent<BM_Sundew_CounterPoisonToss>(), gameObject.AddComponent<BM_Sundew_Hard_ExhaustBall>() };

        base.Initialize();
    }

    public override void ChooseMoveInternal()
    {
        counterCount = 0;
        if (BattleControl.Instance.GetCurseLevel() > 0)
        {
            currMove = moveset[(posId + BattleControl.Instance.turnCount - 1) % 3 == 0 ? 2 : 0];
        }
        else
        {
            currMove = moveset[0];
        }

        BasicOffsetTargetChooser(1,2);
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

        Effect_ReactionCounter();

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
        if ((e == BattleHelper.Event.Hurt || e == BattleHelper.Event.Death) && target == this && counterCount <= 0)
        {
            counterCount++;
            BattleControl.Instance.AddReactionMoveEvent(this, target.lastAttacker, moveset[1]);
            return true;
        }

        return false;
    }
}

public class BM_Sundew_PoisonToss : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Sundew_PoisonToss;

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
            if (caller.GetAttackHit(caller.curTarget, BattleHelper.DamageType.Earth))
            {
                bool hasStatus = caller.curTarget.HasStatus();
                caller.curTarget.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.Spin);
                caller.DealDamage(caller.curTarget, 1, BattleHelper.DamageType.Earth, 0, BattleHelper.ContactLevel.Infinite);
                if (!hasStatus)
                {
                    caller.InflictEffect(caller.curTarget, new Effect(Effect.EffectType.Poison, 1, 1));
                }
            }
            else
            {
                caller.InvokeMissEvents(caller.curTarget);
            }
        }
    }
}

public class BM_Sundew_CounterPoisonToss : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Sundew_CounterPoisonToss;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemy);

    public override IEnumerator ExecuteOutOfTurn(BattleEntity caller, BattleEntity target, int level = 1)
    {
        caller.curTarget = target;
        if (!BattleControl.Instance.EntityValid(caller.curTarget))
        {
            caller.curTarget = null;
        }

        yield return StartCoroutine(caller.SpinHeavy(Vector3.up * 360, 1f));

        if (caller.curTarget != null)
        {
            if (caller.GetAttackHit(caller.curTarget, 0))
            {
                bool hasStatus = caller.curTarget.HasStatus();
                caller.curTarget.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.Spin);
                caller.DealDamage(caller.curTarget, 1, BattleHelper.DamageType.Earth, 0, BattleHelper.ContactLevel.Infinite);
                if (!hasStatus)
                {
                    caller.InflictEffect(caller.curTarget, new Effect(Effect.EffectType.Poison, 1, 1));
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

public class BM_Sundew_Hard_ExhaustBall : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Sundew_Hard_ExhaustBall;

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
                bool hasStatus = caller.curTarget.HasStatus();
                caller.curTarget.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.Spin);
                caller.DealDamage(caller.curTarget, 1, BattleHelper.DamageType.Water, 0, BattleHelper.ContactLevel.Infinite);
                if (!hasStatus)
                {
                    caller.InflictEffect(caller.curTarget, new Effect(Effect.EffectType.Exhausted, 1, 2));
                }
            }
            else
            {
                caller.InvokeMissEvents(caller.curTarget);
            }
        }
    }
}
