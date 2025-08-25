using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BE_Pyrenfish : BattleEntity
{
    int counterCount;
    public override void Initialize()
    {
        moveset = new List<Move> { gameObject.AddComponent<BM_Pyrenfish_SplotchBubble>(), gameObject.AddComponent<BM_Pyrenfish_DoubleFireBreath>(), gameObject.AddComponent<BM_Pyrenfish_FlareSong>(), gameObject.AddComponent<BM_Pyrenfish_Hard_FinalFlareBurst>() };

        base.Initialize();
    }

    public override void ChooseMoveInternal()
    {
        counterCount = 0;
        if (GetEntityProperty(BattleHelper.EntityProperties.Grounded))
        {
            currMove = moveset[(posId + BattleControl.Instance.turnCount - 1) % 2];
        }
        else
        {
            currMove = moveset[(posId + BattleControl.Instance.turnCount - 1) % 3];
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
            BattleControl.Instance.AddReactionMoveEvent(this, target.lastAttacker, moveset[3]);
            return true;
        }

        return false;
    }
}

public class BM_Pyrenfish_SplotchBubble : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Pyrenfish_SplotchBubble;

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

        List<BattleEntity> targets = BattleControl.Instance.GetEntitiesSorted(caller, GetBaseTarget());
        foreach (BattleEntity t in targets)
        {
            if (caller.GetAttackHit(t, BattleHelper.DamageType.Dark))
            {
                t.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.Squish);
                caller.DealDamage(t, 5, BattleHelper.DamageType.Dark, 0, BattleHelper.ContactLevel.Infinite);
                caller.InflictEffect(t, new Effect(Effect.EffectType.Splotch, 1, 3));
            }
            else
            {
                //Miss
                caller.InvokeMissEvents(t);
            }
        }
    }
}

public class BM_Pyrenfish_DoubleFireBreath : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Pyrenfish_DoubleFireBreath;

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

        List<BattleEntity> targets = BattleControl.Instance.GetEntitiesSorted(caller, GetBaseTarget());
        for (int i = 0; i < 2; i++)
        {
            foreach (BattleEntity t in targets)
            {
                if (caller.GetAttackHit(t, BattleHelper.DamageType.Fire))
                {
                    t.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.Spin);
                    caller.DealDamage(t, 4, BattleHelper.DamageType.Fire, 0, BattleHelper.ContactLevel.Infinite);
                }
                else
                {
                    //Miss
                    caller.InvokeMissEvents(t);
                }
            }
            if (i < 1)
            {
                yield return StartCoroutine(caller.SpinHeavy(Vector3.up * 360, 0.5f));
            }
        }
    }
}

public class BM_Pyrenfish_FlareSong : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Pyrenfish_FlareSong;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveAlly);

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        if (!BattleControl.Instance.EntityValid(caller.curTarget))
        {
            caller.curTarget = null;
        }

        yield return StartCoroutine(caller.SpinHeavy(Vector3.up * 360, 0.5f));

        List<BattleEntity> targets = BattleControl.Instance.GetEntitiesSorted(caller, GetBaseTarget());

        caller.InflictEffect(caller, new Effect(Effect.EffectType.CounterFlare, 1, 3));
        foreach (BattleEntity be in targets)
        {
            caller.InflictEffect(be, new Effect(Effect.EffectType.Focus, 2, Effect.INFINITE_DURATION));
        }
    }
}

public class BM_Pyrenfish_Hard_FinalFlareBurst : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Pyrenfish_Hard_FinalFlareBurst;

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
            caller.InflictEffect(be, new Effect(Effect.EffectType.CounterFlare, 1, 3));
        }

        if (caller.hp == 0)
        {
            caller.SetAnimation("dead");
            yield return StartCoroutine(caller.DefaultDeathEvent());
            yield break;
        }
    }
}

public class BE_GoldScreecher : BattleEntity
{
    int counterCount;
    public override void Initialize()
    {
        moveset = new List<Move> { gameObject.AddComponent<BM_Shared_QuintupleSwoop>(), gameObject.AddComponent<BM_GoldScreecher_TripleFlameWind>(), gameObject.AddComponent<BM_GoldScreecher_Hard_FinalChargeWave>() };

        base.Initialize();
    }

    public override void ChooseMoveInternal()
    {
        currMove = moveset[(posId + BattleControl.Instance.turnCount - 1) % 2];
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

public class BM_GoldScreecher_TripleFlameWind : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.GoldScreecher_TripleFlameWind;

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

        for (int i = 0; i < 3; i++)
        {
            foreach (BattleEntity t in targets)
            {
                if (caller.GetAttackHit(t, BattleHelper.DamageType.Fire))
                {
                    t.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.Spin);
                    caller.DealDamage(t, 4, BattleHelper.DamageType.Fire, 0, BattleHelper.ContactLevel.Infinite);
                }
                else
                {
                    //Miss
                    caller.InvokeMissEvents(t);
                }
            }
            if (i < 2)
            {
                yield return StartCoroutine(caller.SpinHeavy(Vector3.up * 360, 0.5f));
            }
        }
    }
}

public class BM_GoldScreecher_Hard_FinalChargeWave : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.GoldScreecher_Hard_FinalChargeWave;

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
            caller.InflictEffect(be, new Effect(Effect.EffectType.Supercharge, 1, 3));
            caller.InflictEffect(be, new Effect(Effect.EffectType.Focus, 3, Effect.INFINITE_DURATION));
        }

        if (caller.hp == 0)
        {
            caller.SetAnimation("dead");
            yield return StartCoroutine(caller.DefaultDeathEvent());
            yield break;
        }
    }
}

public class BE_Infernoling : BattleEntity
{
    public override void Initialize()
    {
        moveset = new List<Move> { gameObject.AddComponent<BM_Shared_DoubleBite>(), gameObject.AddComponent<BM_Infernoling_TripleFlameBreath>(), gameObject.AddComponent<BM_Infernoling_LuminousRoar>() };

        base.Initialize();
    }

    public override void ChooseMoveInternal()
    {
        int turnoffset = BattleControl.Instance.turnCount - 2;
        int offset = posId + turnoffset;
        if (offset < 0)
        {
            offset = 0;
        }
        currMove = moveset[(offset) % (moveset.Count)];

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

            if (contact <= BattleHelper.ContactLevel.Weapon)
            {
                DealDamage(target, 3, BattleHelper.DamageType.Fire, (ulong)BattleHelper.DamageProperties.StandardContactHazard, BattleHelper.ContactLevel.Weapon);
                target.contactImmunityList.Add(posId);
            }
        }
    }
}

public class BM_Infernoling_TripleFlameBreath : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Infernoling_TripleFlameBreath;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemy);

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        if (!BattleControl.Instance.EntityValid(caller.curTarget))
        {
            caller.curTarget = null;
        }

        yield return StartCoroutine(caller.SpinHeavy(Vector3.up * 360, 1f));

        List<BattleEntity> targets = BattleControl.Instance.GetEntitiesSorted(caller, GetBaseTarget());

        for (int i = 0; i < 3; i++)
        {
            foreach (BattleEntity t in targets)
            {
                if (caller.GetAttackHit(t, BattleHelper.DamageType.Fire))
                {
                    caller.DealDamage(t, 4, BattleHelper.DamageType.Fire, 0, BattleHelper.ContactLevel.Infinite);
                }
                else
                {
                    //Miss
                    caller.InvokeMissEvents(t);
                }
            }
            if (i < 2)
            {
                yield return StartCoroutine(caller.SpinHeavy(Vector3.up * 360, 0.5f));
            }
        }
    }
}

public class BM_Infernoling_LuminousRoar : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Infernoling_LuminousRoar;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemy);

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        if (!BattleControl.Instance.EntityValid(caller.curTarget))
        {
            caller.curTarget = null;
        }

        yield return StartCoroutine(caller.SpinHeavy(Vector3.up * 360, 1f));

        List<BattleEntity> targets = BattleControl.Instance.GetEntitiesSorted(caller, GetBaseTarget());

        caller.InflictEffect(caller, new Effect(Effect.EffectType.Illuminate, 1, 3));
        foreach (BattleEntity t in targets)
        {
            if (caller.GetAttackHit(t, BattleHelper.DamageType.Normal))
            {
                caller.DealDamage(t, 5, BattleHelper.DamageType.Normal, 0, BattleHelper.ContactLevel.Infinite);
                caller.InflictEffect(t, new Effect(Effect.EffectType.DefenseDown, 2, 3));
                if (BattleControl.Instance.GetCurseLevel() > 0)
                {
                    caller.InflictEffect(t, new Effect(Effect.EffectType.Dread, 1, 3));
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

public class BE_Magmaswimmer : BattleEntity
{
    public override void Initialize()
    {
        moveset = new List<Move> { gameObject.AddComponent<BM_Magmaswimmer_Charge>(), gameObject.AddComponent<BM_Magmaswimmer_Fireball>(), gameObject.AddComponent<BM_Magmaswimmer_Hard_RushBite>() };

        base.Initialize();
    }

    public override void ChooseMoveInternal()
    {
        if (GetEntityProperty(BattleHelper.EntityProperties.StateCharge))
        {
            SetEntityProperty(BattleHelper.EntityProperties.StateCharge, false);
            currMove = moveset[1];
        } else
        {
            int turnoffset = BattleControl.Instance.turnCount - 2;
            int offset = posId + turnoffset;
            if (offset < 0)
            {
                offset = 0;
            }

            if (BattleControl.Instance.GetCurseLevel() > 0)
            {
                currMove = moveset[(offset) % 3];
            }
            else
            {
                currMove = moveset[(offset) % 2];
            }
            if (currMove == moveset[1])
            {
                currMove = moveset[0];
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
                DealDamage(target, 2, BattleHelper.DamageType.Fire, (ulong)BattleHelper.DamageProperties.StandardContactHazard, BattleHelper.ContactLevel.Weapon);
                target.contactImmunityList.Add(posId);
            }
        }
    }
}

public class BM_Magmaswimmer_Charge : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Magmaswimmer_Charge;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.Self);

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        yield return StartCoroutine(caller.SpinHeavy(Vector3.up * 360, 0.25f));
        caller.SetEntityProperty(BattleHelper.EntityProperties.StateCharge);
        caller.InflictEffect(caller, new Effect(Effect.EffectType.Ethereal, 1, 2));
        caller.InflictEffect(caller, new Effect(Effect.EffectType.Focus, 3, Effect.INFINITE_DURATION));
    }
}

public class BM_Magmaswimmer_Fireball : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Magmaswimmer_Fireball;

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
            if (caller.GetAttackHit(caller.curTarget, BattleHelper.DamageType.Fire))
            {
                caller.curTarget.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.Spin);
                switch (caller.entityID)
                {
                    case BattleHelper.EntityID.Magmaswimmer:
                        caller.DealDamage(caller.curTarget, 8, BattleHelper.DamageType.Fire, 0, BattleHelper.ContactLevel.Infinite);
                        break;
                    default:
                        caller.DealDamage(caller.curTarget, 3, BattleHelper.DamageType.Fire, 0, BattleHelper.ContactLevel.Infinite);
                        break;
                }
            }
            else
            {
                caller.InvokeMissEvents(caller.curTarget);
            }
        }
    }
}

public class BM_Magmaswimmer_Hard_RushBite : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Magmaswimmer_Hard_RushBite;

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
                    case BattleHelper.EntityID.Magmaswimmer:
                        caller.DealDamage(caller.curTarget, 7, BattleHelper.DamageType.Normal, 0, BattleHelper.ContactLevel.Contact);
                        caller.InflictEffect(caller.curTarget, new Effect(Effect.EffectType.Soulbleed, 1, 3));
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

public class BE_Wyverlet : BattleEntity
{
    int counterCount;
    public override void Initialize()
    {
        moveset = new List<Move> { gameObject.AddComponent<BM_Wyverlet_DoubleFlameWind>(), gameObject.AddComponent<BM_Wyverlet_WrathScreech>(), gameObject.AddComponent<BM_Wyverlet_Counter_PowerScreech>() };

        base.Initialize();
    }

    public override void ChooseMoveInternal()
    {
        counterCount = 0;
        currMove = moveset[(posId + BattleControl.Instance.turnCount - 1) % 2];
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

        Effect_ReactionAttack();

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

public class BM_Wyverlet_DoubleFlameWind : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Wyverlet_DoubleFlameWind;

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

        for (int i = 0; i < 2; i++)
        {
            foreach (BattleEntity t in targets)
            {
                if (caller.GetAttackHit(t, BattleHelper.DamageType.Fire))
                {
                    t.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.Spin);
                    caller.DealDamage(t, 4, BattleHelper.DamageType.Fire, 0, BattleHelper.ContactLevel.Infinite);
                }
                else
                {
                    //Miss
                    caller.InvokeMissEvents(t);
                }
            }
            if (i < 1)
            {
                yield return StartCoroutine(caller.SpinHeavy(Vector3.up * 360, 0.5f));
            }
        }
    }
}

public class BM_Wyverlet_WrathScreech : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Wyverlet_WrathScreech;

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

        caller.InflictEffect(caller, new Effect(Effect.EffectType.CounterFlare, 1, 3));
        foreach (BattleEntity t in targets)
        {
            if (caller.GetAttackHit(t, BattleHelper.DamageType.Normal))
            {
                t.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.Spin);
                caller.DealDamage(t, 4, BattleHelper.DamageType.Normal, 0, BattleHelper.ContactLevel.Infinite);
                caller.InflictEffect(t, new Effect(Effect.EffectType.AttackDown, 2, 3));
                caller.InflictEffect(t, new Effect(Effect.EffectType.DefenseDown, 2, 3));
            }
            else
            {
                //Miss
                caller.InvokeMissEvents(t);
            }
        }
    }
}

public class BM_Wyverlet_Counter_PowerScreech : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Wyverlet_CounterPowerScreech;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemy);

    public override IEnumerator ExecuteOutOfTurn(BattleEntity caller, BattleEntity target, int level = 1)
    {
        //fly back up        
        //yield return StartCoroutine(caller.FlyingFlyBackUp());

        if (!BattleControl.Instance.EntityValid(caller.curTarget))
        {
            caller.curTarget = null;
        }

        yield return StartCoroutine(caller.SpinHeavy(Vector3.up * 360, 1f));

        List<BattleEntity> targets = BattleControl.Instance.GetEntitiesSorted(caller, GetBaseTarget());

        caller.InflictEffect(caller, new Effect(Effect.EffectType.CounterFlare, 1, 3));
        if (BattleControl.Instance.GetCurseLevel() > 0)
        {
            caller.InflictEffectCapped(caller, new Effect(Effect.EffectType.AttackBoost, 2, Effect.INFINITE_DURATION), 8);
        }
        else
        {
            caller.InflictEffect(caller, new Effect(Effect.EffectType.AttackUp, 2, 3));
        }
        foreach (BattleEntity t in targets)
        {
            if (caller.GetAttackHit(t, BattleHelper.DamageType.Dark))
            {
                t.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.Spin);
                caller.DealDamage(t, 3, BattleHelper.DamageType.Dark, 0, BattleHelper.ContactLevel.Infinite);
            }
            else
            {
                //Miss
                caller.InvokeMissEvents(t);
            }
        }
    }
}