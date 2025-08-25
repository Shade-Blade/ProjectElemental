using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BE_DarkBurrower : BattleEntity
{
    int counterCount = 0;
    public override void Initialize()
    {
        moveset = new List<Move> { gameObject.AddComponent<BM_DarkBurrower_SleepBite>(), gameObject.AddComponent<BM_DarkBurrower_CounterSleepBite>(), gameObject.AddComponent<BM_DarkBurrower_Hard_AstralBloom>() };

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
        if ((e == BattleHelper.Event.Hurt || e == BattleHelper.Event.Death) && target.posId >= 0 && target.lastAttacker != null && target.lastAttacker.posId < 0 && counterCount <= 0)
        {
            counterCount++;
            BattleControl.Instance.AddReactionMoveEvent(this, target.lastAttacker, moveset[1]);
            return true;
        }

        return false;
    }
}

public class BM_DarkBurrower_SleepBite : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.DarkBurrower_SleepBite;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemy);

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        if (!BattleControl.Instance.EntityValid(caller.curTarget))
        {
            caller.curTarget = null;
        }

        Vector3 bpos = caller.homePos + caller.curTarget.height * 2 * Vector3.down;
        yield return StartCoroutine(caller.MoveEasing(bpos, 6, (e) => MainManager.EasingIn(e), "burrow"));

        if (caller.curTarget != null)
        {
            Vector3 spos = caller.curTarget.transform.position - caller.curTarget.transform.position.y * Vector3.up + bpos.y * Vector3.up + caller.curTarget.width * 0.5f * Vector3.right;
            caller.transform.position = spos;
            Vector3 tpos2 = caller.curTarget.transform.position - caller.curTarget.transform.position.y * Vector3.up + caller.curTarget.width * 0.5f * Vector3.right;

            yield return StartCoroutine(caller.MoveEasing(tpos2, 6, (e) => MainManager.EasingIn(e), "unburrow"));

            if (caller.GetAttackHit(caller.curTarget, 0))
            {
                bool hasStatus = caller.curTarget.HasAilment();
                caller.curTarget.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.ReverseSquish);
                caller.DealDamage(caller.curTarget, 3, BattleHelper.DamageType.Normal, 0, BattleHelper.ContactLevel.Contact);
                if (!hasStatus)
                {
                    caller.InflictEffect(caller.curTarget, new Effect(Effect.EffectType.Sleep, 1, 2));
                }
            }
            else
            {
                caller.InvokeMissEvents(caller.curTarget);
            }

            yield return StartCoroutine(caller.MoveEasing(spos, 6, (e) => MainManager.EasingIn(e), "burrow"));
        }

        caller.transform.position = bpos;
        yield return StartCoroutine(caller.MoveEasing(caller.homePos, (e) => MainManager.EasingIn(e), "unburrow"));
    }
}

public class BM_DarkBurrower_CounterSleepBite : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.DarkBurrower_CounterSleepBite;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemy);

    public override IEnumerator ExecuteOutOfTurn(BattleEntity caller, BattleEntity causer, int level = 1)
    {
        caller.curTarget = causer;
        if (!BattleControl.Instance.EntityValid(caller.curTarget))
        {
            caller.curTarget = null;
        }

        Vector3 bpos = caller.homePos + caller.curTarget.height * 2 * Vector3.down;
        yield return StartCoroutine(caller.MoveEasing(bpos, 6, (e) => MainManager.EasingIn(e), "burrow"));

        if (caller.curTarget != null)
        {
            Vector3 spos = caller.curTarget.transform.position - caller.curTarget.transform.position.y * Vector3.up + bpos.y * Vector3.up + caller.curTarget.width * 0.5f * Vector3.right;
            caller.transform.position = spos;
            Vector3 tpos2 = caller.curTarget.transform.position - caller.curTarget.transform.position.y * Vector3.up + caller.curTarget.width * 0.5f * Vector3.right;

            yield return StartCoroutine(caller.MoveEasing(tpos2, 6, (e) => MainManager.EasingIn(e), "unburrow"));

            if (caller.GetAttackHit(caller.curTarget, 0))
            {
                bool hasStatus = caller.curTarget.HasAilment();
                caller.curTarget.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.ReverseSquish);
                caller.DealDamage(caller.curTarget, 3, BattleHelper.DamageType.Normal, 0, BattleHelper.ContactLevel.Contact);
                if (!hasStatus)
                {
                    caller.InflictEffect(caller.curTarget, new Effect(Effect.EffectType.Sleep, 1, 2));
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
            yield return StartCoroutine(caller.MoveEasing(spos, 6, (e) => MainManager.EasingIn(e), "burrow"));
        }

        if (caller.hp == 0)
        {
            caller.SetAnimation("dead");
            yield return StartCoroutine(caller.DefaultDeathEvent());
            yield break;
        }

        caller.transform.position = bpos;
        yield return StartCoroutine(caller.MoveEasing(caller.homePos, (e) => (MainManager.EasingIn(e)), "unburrow"));
    }
}

public class BM_DarkBurrower_Hard_AstralBloom : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.DarkBurrower_Hard_AstralBloom;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.Self);

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        yield return StartCoroutine(caller.SpinHeavy(Vector3.up * 360, 0.25f));
        caller.InflictEffect(caller, new Effect(Effect.EffectType.AstralWall, (sbyte)Mathf.CeilToInt(caller.maxHP / 2f), 3));
    }
}

public class BE_Shadew : BattleEntity
{
    int counterCount = 0;
    public override void Initialize()
    {
        moveset = new List<Move> { gameObject.AddComponent<BM_Shadew_PoisonSplash>(), gameObject.AddComponent<BM_Shadew_CounterPoisonSplash>(), gameObject.AddComponent<BM_Shadew_Hard_DreadBall>() };

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

        BasicTargetChooser(1, 2);
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

public class BM_Shadew_PoisonSplash : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Shadew_PoisonSplash;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemy);

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        if (!BattleControl.Instance.EntityValid(caller.curTarget))
        {
            caller.curTarget = null;
        }

        yield return StartCoroutine(caller.SpinHeavy(Vector3.up * 360, 1f));

        foreach (BattleEntity target in BattleControl.Instance.GetEntitiesSorted(caller, GetTargetArea(caller)))
        {
            if (caller.GetAttackHit(target, BattleHelper.DamageType.Earth))
            {
                bool hasStatus = target.HasAilment();
                target.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.Spin);
                caller.DealDamage(target, 2, BattleHelper.DamageType.Earth, 0, BattleHelper.ContactLevel.Infinite);
                if (!hasStatus)
                {
                    caller.InflictEffect(target, new Effect(Effect.EffectType.Poison, 1, 1));
                }
            }
            else
            {
                caller.InvokeMissEvents(target);
            }
        }
    }
}

public class BM_Shadew_CounterPoisonSplash : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Shadew_CounterPoisonSplash;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemy);

    public override IEnumerator ExecuteOutOfTurn(BattleEntity caller, BattleEntity target, int level = 1)
    {
        caller.curTarget = target;
        if (!BattleControl.Instance.EntityValid(caller.curTarget))
        {
            caller.curTarget = null;
        }

        yield return StartCoroutine(caller.SpinHeavy(Vector3.up * 360, 1f));

        foreach (BattleEntity t in BattleControl.Instance.GetEntitiesSorted(caller, GetTargetArea(caller)))
        {
            if (caller.GetAttackHit(t, BattleHelper.DamageType.Earth))
            {
                bool hasStatus = t.HasAilment();
                t.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.Spin);
                caller.DealDamage(t, 2, BattleHelper.DamageType.Earth, 0, BattleHelper.ContactLevel.Infinite);
                if (!hasStatus)
                {
                    caller.InflictEffect(t, new Effect(Effect.EffectType.Poison, 1, 1));
                }
            }
            else
            {
                caller.InvokeMissEvents(t);
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

public class BM_Shadew_Hard_DreadBall : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Shadew_Hard_DreadBall;

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
            if (caller.GetAttackHit(caller.curTarget, BattleHelper.DamageType.Dark))
            {
                bool hasStatus = caller.curTarget.HasAilment();
                caller.curTarget.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.Spin);
                caller.DealDamage(caller.curTarget, 3, BattleHelper.DamageType.Dark, 0, BattleHelper.ContactLevel.Infinite);
                if (!hasStatus)
                {
                    caller.InflictEffect(caller.curTarget, new Effect(Effect.EffectType.Dread, 1, 2));
                }
            }
            else
            {
                caller.InvokeMissEvents(caller.curTarget);
            }
        }
    }
}

public class BE_Vilebloom : BattleEntity
{
    int counterCount = 0;
    public override void Initialize()
    {
        moveset = new List<Move> { gameObject.AddComponent<BM_Vilebloom_DustWind>(), gameObject.AddComponent<BM_Vilebloom_DoubleLob>(), gameObject.AddComponent<BM_Vilebloom_MorbidRecovery>(), gameObject.AddComponent<BM_Vilebloom_Hard_DarkLob>() };

        base.Initialize();
    }

    public override void ChooseMoveInternal()
    {
        counterCount = 0;
        if (BattleControl.Instance.GetCurseLevel() > 0)
        {
            currMove = moveset[((posId % 2) + BattleControl.Instance.turnCount - 1) % 3];
        }
        else
        {
            currMove = moveset[(posId + BattleControl.Instance.turnCount - 1) % 2];
        }
        if (currMove == moveset[2])
        {
            currMove = moveset[3];
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
                DealDamage(target, 2, BattleHelper.DamageType.Dark, (ulong)BattleHelper.DamageProperties.StandardContactHazard, BattleHelper.ContactLevel.Contact);
                target.contactImmunityList.Add(posId);
            }
        }
    }

    public override IEnumerator PreReact(Move move, BattleEntity target)
    {
        counterCount = 0;

        Effect_ReactionCounter();

        yield return new WaitForSeconds(0.5f);
    }
    public override bool ReactToEvent(BattleEntity target, BattleHelper.Event e, int previousReactions)
    {
        if ((e == BattleHelper.Event.Death) && target != this && (target.posId >= 0) && counterCount <= 0)
        {
            counterCount++;
            BattleControl.Instance.AddReactionMoveEvent(this, this, moveset[2]);
            return true;
        }

        return false;
    }
}

public class BM_Vilebloom_DustWind : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Vilebloom_DustWind;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemy);

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        if (!BattleControl.Instance.EntityValid(caller.curTarget))
        {
            caller.curTarget = null;
        }

        yield return StartCoroutine(caller.SpinHeavy(Vector3.up * 360, 0.5f));

        foreach (BattleEntity t in BattleControl.Instance.GetEntitiesSorted(caller, GetTargetArea(caller)))
        {
            if (caller.GetAttackHit(t, BattleHelper.DamageType.Earth))
            {
                caller.DealDamage(t, 2, BattleHelper.DamageType.Earth, 0, BattleHelper.ContactLevel.Infinite);
            }
            else
            {
                caller.InvokeMissEvents(t);
            }
        }
    }
}

public class BM_Vilebloom_DoubleLob : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Vilebloom_DoubleLob;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemyLowBackmost);

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        if (!BattleControl.Instance.EntityValid(caller.curTarget))
        {
            caller.curTarget = null;
        }

        yield return StartCoroutine(caller.SpinHeavy(Vector3.up * 360, 0.5f));

        if (caller.curTarget != null)
        {
            for (int i = 0; i < 2; i++)
            {
                if (caller.GetAttackHit(caller.curTarget, BattleHelper.DamageType.Water))
                {
                    bool hasStatus = caller.curTarget.HasAilment();
                    caller.curTarget.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.Spin);
                    caller.DealDamage(caller.curTarget, 2, BattleHelper.DamageType.Water, 0, BattleHelper.ContactLevel.Infinite);
                    if (!hasStatus && i == 1)
                    {
                        caller.InflictEffect(caller.curTarget, new Effect(Effect.EffectType.Sleep, 1, 1));
                    }
                }
                else
                {
                    caller.InvokeMissEvents(caller.curTarget);
                }

                if (i < 1)
                {
                    yield return StartCoroutine(caller.SpinHeavy(Vector3.up * 360, 0.5f));
                }
            }
        }
    }
}

public class BM_Vilebloom_MorbidRecovery : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Vilebloom_MorbidRecovery;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.Self);

    public override IEnumerator ExecuteOutOfTurn(BattleEntity caller, BattleEntity target, int level = 1)
    {
        yield return StartCoroutine(caller.SpinHeavy(Vector3.up * 360, 0.5f));
        caller.HealHealth(4);
    }
}

public class BM_Vilebloom_Hard_DarkLob : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Vilebloom_Hard_DarkLob;

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
            if (caller.GetAttackHit(caller.curTarget, BattleHelper.DamageType.Dark))
            {
                bool hasStatus = caller.curTarget.HasAilment();
                caller.DealDamage(caller.curTarget, 3, BattleHelper.DamageType.Dark, 0, BattleHelper.ContactLevel.Infinite);
                if (!hasStatus)
                {
                    caller.InflictEffect(caller.curTarget, new Effect(Effect.EffectType.Inverted, 1, 3));
                }
            }
            else
            {
                caller.InvokeMissEvents(caller.curTarget);
            }
        }
    }
}

public class BE_Thornweed : BattleEntity
{
    public override void Initialize()
    {
        moveset = new List<Move> { gameObject.AddComponent<BM_Thornweed_ThornShots>(), gameObject.AddComponent<BM_Thornweed_ThornSpin>() };

        base.Initialize();
    }

    public override void ChooseMoveInternal()
    {
        currMove = moveset[(posId + BattleControl.Instance.turnCount - 1) % 2];
        BasicTargetChooser();
    }
}

public class BM_Thornweed_ThornShots : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Thornweed_ThornShots;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemy);

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        if (!BattleControl.Instance.EntityValid(caller.curTarget))
        {
            caller.curTarget = null;
        }

        yield return StartCoroutine(caller.SpinHeavy(Vector3.up * 360, 0.5f));

        foreach (BattleEntity t in BattleControl.Instance.GetEntitiesSorted(caller, GetTargetArea(caller)))
        {
            if (caller.GetAttackHit(t, BattleHelper.DamageType.Normal))
            {
                caller.DealDamage(t, 2, BattleHelper.DamageType.Normal, 0, BattleHelper.ContactLevel.Infinite);
                caller.InflictEffectBuffered(t, new Effect(Effect.EffectType.Sunder, 2, Effect.INFINITE_DURATION));
            }
            else
            {
                caller.InvokeMissEvents(t);
            }
        }
    }
}

public class BM_Thornweed_ThornSpin : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Thornweed_ThornSpin;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemyLowFrontmost);

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        if (!BattleControl.Instance.EntityValid(caller.curTarget))
        {
            caller.curTarget = null;
        }
        BattleEntity target = caller.curTarget;

        if (target == null)
        {
            yield break;
        }

        yield return StartCoroutine(caller.SpinHeavy(Vector3.up * 360, 0.5f));
        for (int i = 0; i < 2; i++)
        {
            if (caller.GetAttackHit(target, BattleHelper.DamageType.Normal))
            {
                caller.DealDamage(target, 2, BattleHelper.DamageType.Normal, 0, BattleHelper.ContactLevel.Infinite);
            }
            else
            {
                caller.InvokeMissEvents(target);
            }
            if (i < 1)
            {
                yield return StartCoroutine(caller.SpinHeavy(Vector3.up * 360, 0.5f));
            }
        }
        if (BattleControl.Instance.GetCurseLevel() > 0)
        {
            yield return StartCoroutine(caller.SpinHeavy(Vector3.up * 360, 0.5f));
            if (caller.GetAttackHit(target, BattleHelper.DamageType.Normal))
            {
                caller.DealDamage(target, 2, BattleHelper.DamageType.Normal, 0, BattleHelper.ContactLevel.Infinite);
            }
            else
            {
                caller.InvokeMissEvents(target);
            }
        }
    }
}