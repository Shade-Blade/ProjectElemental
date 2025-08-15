using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BE_Leafswimmer : BattleEntity
{
    public override void Initialize()
    {
        moveset = new List<Move> { gameObject.AddComponent<BM_Starfish_LeafStorm>(), gameObject.AddComponent<BM_Leafswimmer_LightBlast>(), gameObject.AddComponent<BM_Leafswimmer_Abide>() };

        base.Initialize();
    }

    public override void ChooseMoveInternal()
    {
        currMove = moveset[(posId + BattleControl.Instance.turnCount - 1) % (moveset.Count)];

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

public class BM_Leafswimmer_LightBlast : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Leafswimmer_LightBlast;
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
            for (int i = 0; i < 2; i++)
            {
                yield return StartCoroutine(caller.SpinHeavy(Vector3.up * 360, 0.175f));

                if (caller.GetAttackHit(caller.curTarget, BattleHelper.DamageType.Light))
                {
                    caller.curTarget.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.ReverseSquish);
                    caller.DealDamage(caller.curTarget, 4, BattleHelper.DamageType.Light, 0, BattleHelper.ContactLevel.Contact);
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

public class BM_Leafswimmer_Abide : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Leafswimmer_Abide;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.Self);

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        //fly back up        
        yield return StartCoroutine(caller.FlyingFlyBackUp());

        yield return StartCoroutine(caller.SpinHeavy(Vector3.up * 360, 0.25f));
        caller.InflictEffect(caller, new Effect(Effect.EffectType.AstralWall, (sbyte)Mathf.CeilToInt(caller.maxHP / 2f), 3));
        if (BattleControl.Instance.GetCurseLevel() > 0)
        {
            caller.InflictEffectCapped(caller, new Effect(Effect.EffectType.AttackBoost, 1, Effect.INFINITE_DURATION), 8, caller.posId, Effect.EffectStackMode.KeepDurAddPot);
            caller.InflictEffectCapped(caller, new Effect(Effect.EffectType.DefenseBoost, 1, Effect.INFINITE_DURATION), 8, caller.posId, Effect.EffectStackMode.KeepDurAddPot);
        }
    }
}

public class BE_Brambleling : BattleEntity
{
    public override void Initialize()
    {
        moveset = new List<Move> { gameObject.AddComponent<BM_Shared_DoubleBite>(), gameObject.AddComponent<BM_Brambleling_ProtectiveRoar>() };

        base.Initialize();
    }

    public override void ChooseMoveInternal()
    {
        //also reset here in case something weird happens
        currMove = moveset[(posId + BattleControl.Instance.turnCount - 1) % (moveset.Count)];

        BasicTargetChooser();
    }
}

public class BM_Brambleling_ProtectiveRoar : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Brambleling_ProtectiveRoar;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveAlly);

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        yield return StartCoroutine(caller.SpinHeavy(Vector3.up * 360, 0.25f));

        caller.InflictEffect(caller, new Effect(Effect.EffectType.Soften, 1, 3));
        List<BattleEntity> battleEntityList = BattleControl.Instance.GetEntitiesSorted(caller, GetBaseTarget());
        foreach (BattleEntity entity in battleEntityList)
        {
            caller.InflictEffect(entity, new Effect(Effect.EffectType.DefenseUp, 2, 3));
        }
        if (BattleControl.Instance.GetCurseLevel() > 0)
        {
            caller.InflictEffectCapped(caller, new Effect(Effect.EffectType.DefenseBoost, 2, Effect.INFINITE_DURATION), 8, caller.posId, Effect.EffectStackMode.KeepDurAddPot);
        }
    }
}

public class BE_Solardew : BattleEntity
{
    int counterCount = 0;
    public override void Initialize()
    {
        moveset = new List<Move> { gameObject.AddComponent<BM_Solardew_SunToss>(), gameObject.AddComponent<BM_Solardew_LuminousHeal>(), gameObject.AddComponent<BM_Solardew_CounterSunFlash>(), gameObject.AddComponent<BM_HoarderFly_Hard_FinalHeal>() };

        base.Initialize();
    }

    public override void ChooseMoveInternal()
    {
        counterCount = 0;
        currMove = moveset[(posId + BattleControl.Instance.turnCount - 1) % 3 == 0 ? 1 : 0];

        //Heal logic
        int healPossible = 0;
        List<BattleEntity> healTargets = BattleControl.Instance.GetEntitiesSorted(this, new TargetArea(TargetArea.TargetAreaType.LiveAlly));
        for (int i = 0; i < healTargets.Count; i++)
        {
            healPossible += healTargets[i].maxHP - healTargets[i].hp;
        }
        if (healPossible == 0)
        {
            currMove = moveset[0];
        }

        if (currMove == moveset[1])
        {
            SpecialTargetChooserFirst(TargetStrategy.TargetStrategyType.LowHP);
        } else
        {
            BasicTargetChooser(1, 2);
        }
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

        if (BattleControl.Instance.GetCurseLevel() <= 0)
        {
            if (e == BattleHelper.Event.Death && target == this && counterCount <= 0)
            {
                counterCount++;
                BattleControl.Instance.AddReactionMoveEvent(this, target.lastAttacker, moveset[3]);
                return true;
            }
        }

        //Counter attack
        //Can be overshadowed by the above thing with final heal
        if ((e == BattleHelper.Event.Hurt || e == BattleHelper.Event.Death) && target == this && counterCount <= 0)
        {
            counterCount++;
            BattleControl.Instance.AddReactionMoveEvent(this, target.lastAttacker, moveset[2]);
            return true;
        }

        return false;
    }
}

public class BM_Solardew_SunToss : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Solardew_SunToss;

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
            if (caller.GetAttackHit(caller.curTarget, BattleHelper.DamageType.Light))
            {
                caller.curTarget.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.Spin);
                caller.DealDamage(caller.curTarget, 6, BattleHelper.DamageType.Light, 0, BattleHelper.ContactLevel.Infinite);
            }
            else
            {
                caller.InvokeMissEvents(caller.curTarget);
            }
        }
    }
}

public class BM_Solardew_LuminousHeal : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Solardew_LuminousHeal;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveAlly);

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        if (!BattleControl.Instance.EntityValid(caller.curTarget))
        {
            caller.curTarget = null;
        }

        yield return StartCoroutine(caller.SpinHeavy(Vector3.up * 360, 1f));

        if (caller.curTarget != null)
        {
            caller.curTarget.HealHealth(7);
            caller.InflictEffect(caller.curTarget, new Effect(Effect.EffectType.Illuminate, 1, 3));
        }
    }
}

public class BM_Solardew_CounterSunFlash : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Solardew_CounterSunFlash;

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
                caller.curTarget.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.Spin);
                caller.DealDamage(caller.curTarget, 3, BattleHelper.DamageType.Earth, 0, BattleHelper.ContactLevel.Infinite);
                yield return StartCoroutine(caller.SpinHeavy(Vector3.up * 360, 0.25f));
                caller.DealDamage(caller.curTarget, 5, BattleHelper.DamageType.Light, 0, BattleHelper.ContactLevel.Infinite);
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