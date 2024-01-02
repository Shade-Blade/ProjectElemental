using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BE_Blazecrest : BattleEntity
{
    int counterCount;
    public bool lastHitWeakness;
    public override void Initialize()
    {
        moveset = new List<Move> { gameObject.AddComponent<BM_Blazecrest_FlameBreath>(), gameObject.AddComponent<BM_Blazecrest_Roar>(), gameObject.AddComponent<BM_Blazecrest_Hard_CounterFlameFocus>() };

        base.Initialize();
    }

    public override void ChooseMoveInternal()
    {
        //also reset here in case something weird happens
        counterCount = 0;
        lastHitWeakness = false;
        currMove = moveset[(posId + BattleControl.Instance.turnCount - 1) % 2];

        BasicTargetChooser();
    }


    public override IEnumerator PostMove()
    {
        //also reset here in case something weird happens
        counterCount = 0;
        lastHitWeakness = false;
        yield return StartCoroutine(base.PostMove());
    }

    public override IEnumerator PreReact(Move move, BattleEntity target)
    {
        counterCount = 0;
        lastHitWeakness = false;

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
            if ((target.lastDamageType & (BattleHelper.DamageType.Light | BattleHelper.DamageType.Water)) != 0)
            {
                lastHitWeakness = true;
            }
            BattleControl.Instance.AddReactionMoveEvent(this, target.lastAttacker, moveset[2]);
            return true;
        }

        return false;
    }
}

public class BM_Blazecrest_FlameBreath : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Blazecrest_FlameBreath;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemy);

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        if (!BattleControl.Instance.EntityValid(caller.curTarget))
        {
            caller.curTarget = null;
        }

        yield return StartCoroutine(caller.Spin(Vector3.up * 360, 1f));

        List<BattleEntity> targets = BattleControl.Instance.GetEntitiesSorted(caller, GetBaseTarget());

        foreach (BattleEntity t in targets)
        {
            if (caller.GetAttackHit(t, BattleHelper.DamageType.Fire))
            {
                caller.DealDamage(t, 3, BattleHelper.DamageType.Fire, 0, BattleHelper.ContactLevel.Infinite);
            }
            else
            {
                //Miss
                caller.InvokeMissEvents(t);
            }
        }
    }
}

public class BM_Blazecrest_Roar : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Blazecrest_Roar;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemy);

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        if (!BattleControl.Instance.EntityValid(caller.curTarget))
        {
            caller.curTarget = null;
        }

        yield return StartCoroutine(caller.Spin(Vector3.up * 360, 1f));

        List<BattleEntity> targets = BattleControl.Instance.GetEntitiesSorted(caller, GetBaseTarget());

        foreach (BattleEntity t in targets)
        {
            if (caller.GetAttackHit(t, 0))
            {
                bool hasStatus = t.HasStatus();
                caller.DealDamage(t, 2, 0, 0, BattleHelper.ContactLevel.Infinite);
                if (!hasStatus)
                {
                    caller.InflictEffect(t, new Effect(Effect.EffectType.Berserk, 1, 2), caller.posId);
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

public class BM_Blazecrest_Hard_CounterFlameFocus : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Blazecrest_Hard_CounterFlameFocus;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.Self);

    public override IEnumerator ExecuteOutOfTurn(BattleEntity caller, BattleEntity target, int level = 1)
    {
        if (caller is BE_Blazecrest bl)
        {
            if (bl.lastHitWeakness)
            {
                caller.InflictEffect(caller, new Effect(Effect.EffectType.Focus, 2, 255));
            }
            else
            {
                caller.InflictEffect(caller, new Effect(Effect.EffectType.Focus, 1, 255));
            }
        }
        yield return new WaitForSeconds(0.5f);
    }
}

public class BE_Embercrest : BattleEntity
{
    public override void Initialize()
    {
        moveset = new List<Move> { gameObject.AddComponent<BM_Shared_DualSlash>(), gameObject.AddComponent<BM_Embercrest_Fireball>() };

        base.Initialize();
    }

    public override void ChooseMoveInternal()
    {
        currMove = moveset[(posId + BattleControl.Instance.turnCount - 1) % 2];

        BasicTargetChooser();
    }
}

public class BM_Embercrest_Fireball : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Embercrest_Fireball;

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
            if (caller.GetAttackHit(caller.curTarget, BattleHelper.DamageType.Fire))
            {
                switch (caller.entityID)
                {
                    case BattleHelper.EntityID.Lavaswimmer:
                        caller.DealDamage(caller.curTarget, 3, BattleHelper.DamageType.Fire, 0, BattleHelper.ContactLevel.Infinite);
                        break;
                    case BattleHelper.EntityID.Embercrest:
                        caller.DealDamage(caller.curTarget, 4, BattleHelper.DamageType.Fire, 0, BattleHelper.ContactLevel.Infinite);
                        break;
                    default:
                        caller.DealDamage(caller.curTarget, 4, BattleHelper.DamageType.Fire, 0, BattleHelper.ContactLevel.Infinite);
                        break;
                }
                if (BattleControl.Instance.GetCurseLevel() > 0)
                {
                    caller.InflictEffectBuffered(caller, new Effect(Effect.EffectType.Focus, 1, 255));
                    caller.InflictEffectBuffered(caller, new Effect(Effect.EffectType.Sunder, 1, 255));
                }
            }
            else
            {
                caller.InvokeMissEvents(caller.curTarget);
            }
        }
    }
}

public class BE_Ashcrest : BattleEntity
{
    public override void Initialize()
    {
        moveset = new List<Move> { gameObject.AddComponent<BM_Ashcrest_ThornBomb>(), gameObject.AddComponent<BM_Ashcrest_SplashBomb>() };

        base.Initialize();
    }

    public override void ChooseMoveInternal()
    {
        currMove = moveset[(posId + BattleControl.Instance.turnCount - 1) % 2];

        BasicTargetChooser();
    }
}

public class BM_Ashcrest_ThornBomb : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Ashcrest_ThornBomb;

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
            if (caller.GetAttackHit(caller.curTarget, BattleHelper.DamageType.Air))
            {
                caller.DealDamage(caller.curTarget, 4, BattleHelper.DamageType.Air, 0, BattleHelper.ContactLevel.Infinite);
                if (BattleControl.Instance.GetCurseLevel() > 0)
                {
                    caller.InflictEffect(caller.curTarget, new Effect(Effect.EffectType.DefenseDown, 1, 3));
                }
            }
            else
            {
                caller.InvokeMissEvents(caller.curTarget);
            }
        }
    }
}

public class BM_Ashcrest_SplashBomb : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Ashcrest_SplashBomb;

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
            if (caller.GetAttackHit(caller.curTarget, BattleHelper.DamageType.Water))
            {
                caller.DealDamage(caller.curTarget, 4, BattleHelper.DamageType.Water, 0, BattleHelper.ContactLevel.Infinite);
                caller.InflictEffect(caller.curTarget, new Effect(Effect.EffectType.Defocus, 1, 255));
                if (BattleControl.Instance.GetCurseLevel() > 0)
                {
                    caller.InflictEffect(caller.curTarget, new Effect(Effect.EffectType.AttackDown, 1, 3));
                }
            }
            else
            {
                caller.InvokeMissEvents(caller.curTarget);
            }
        }
    }
}

public class BE_Flametongue : BattleEntity
{
    int counterCount;
    public override void Initialize()
    {
        moveset = new List<Move> { gameObject.AddComponent<BM_Flametongue_FlameLick>(), gameObject.AddComponent<BM_Shared_Hard_CounterHarden>() };

        base.Initialize();
    }

    public override void ChooseMoveInternal()
    {
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

public class BM_Flametongue_FlameLick : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Flametongue_FlameLick;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemy);

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        if (!BattleControl.Instance.EntityValid(caller.curTarget))
        {
            caller.curTarget = null;
        }

        yield return StartCoroutine(caller.Spin(Vector3.up * 360, 1f));

        List<BattleEntity> targets = BattleControl.Instance.GetEntitiesSorted(caller, GetBaseTarget());

        foreach (BattleEntity t in targets)
        {
            if (caller.GetAttackHit(t, BattleHelper.DamageType.Fire))
            {
                caller.DealDamage(t, 2, BattleHelper.DamageType.Fire, 0, BattleHelper.ContactLevel.Contact);
            }
            else
            {
                //Miss
                caller.InvokeMissEvents(t);
            }
        }
    }
}

public class BE_Heatwing : BattleEntity
{
    int counterCount;

    public override void Initialize()
    {
        moveset = new List<Move> { gameObject.AddComponent<BM_Heatwing_FlameSpread>(), gameObject.AddComponent<BM_Shared_DoubleSwoop>(), gameObject.AddComponent<BM_Shared_Hard_CounterRally>() };

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

public class BM_Heatwing_FlameSpread : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Heatwing_FlameSpread;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemy);

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        //fly back up        
        yield return StartCoroutine(caller.FlyingFlyBackUp());

        if (!BattleControl.Instance.EntityValid(caller.curTarget))
        {
            caller.curTarget = null;
        }

        yield return StartCoroutine(caller.Spin(Vector3.up * 360, 1f));

        List<BattleEntity> targets = BattleControl.Instance.GetEntitiesSorted(caller, GetBaseTarget());

        foreach (BattleEntity t in targets)
        {
            if (caller.GetAttackHit(t, BattleHelper.DamageType.Fire))
            {
                caller.DealDamage(t, 1, BattleHelper.DamageType.Fire, 0, BattleHelper.ContactLevel.Infinite);
            }
            else
            {
                //Miss
                caller.InvokeMissEvents(t);
            }
        }
    }
}

public class BE_Lavaswimmer : BattleEntity
{
    int counterCount;

    public override void Initialize()
    {
        moveset = new List<Move> { gameObject.AddComponent<BM_Shared_Bite>(), gameObject.AddComponent<BM_Embercrest_Fireball>(), gameObject.AddComponent<BM_Shared_Hard_CounterRoar>() };

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
            if (BattleControl.Instance.GetCurseLevel() > 0)
            {
                int turnoffset = BattleControl.Instance.turnCount - 2;
                int offset = posId + turnoffset;
                if (offset < 0)
                {
                    offset = 0;
                }
                currMove = moveset[(offset) % (moveset.Count - 1)];
            }
            else
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
                DealDamage(target, 2, BattleHelper.DamageType.Fire, (ulong)BattleHelper.DamageProperties.StandardContactHazard, BattleHelper.ContactLevel.Contact);
                target.contactImmunityList.Add(posId);
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