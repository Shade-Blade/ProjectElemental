using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BE_Plaguebud : BattleEntity
{
    public override void Initialize()
    {
        moveset = new List<Move> { gameObject.AddComponent<BM_Shared_Bite>(), gameObject.AddComponent<BM_Plaguebud_HeadSprout>(), gameObject.AddComponent<BM_Plaguebud_TailSprout>() };

        base.Initialize();
    }

    public override void ChooseMoveInternal()
    {
        currMove = moveset[(Mathf.Abs(posId + BattleControl.Instance.turnCount - 1)) % 3];
        BasicTargetChooser();
    }
}

public class BM_Plaguebud_HeadSprout : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Plaguebud_HeadSprout;

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
            if (caller.GetAttackHit(caller.curTarget, 0))
            {
                caller.DealDamage(caller.curTarget, 4, 0, 0, BattleHelper.ContactLevel.Infinite);
                caller.InflictEffect(caller.curTarget, new Effect(Effect.EffectType.DrainSprout, 1, 3));
                if (BattleControl.Instance.GetCurseLevel() > 0)
                {
                    caller.HealHealth(4);
                }
            }
            else
            {
                caller.InvokeMissEvents(caller.curTarget);
            }
        }
    }
}

public class BM_Plaguebud_TailSprout : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Plaguebud_TailSprout;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemy);

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        if (!BattleControl.Instance.EntityValid(caller.curTarget))
        {
            caller.curTarget = null;
        }

        yield return StartCoroutine(caller.Spin(Vector3.up * 360, 1f));

        if (caller.curTarget != null)
        {
            if (caller.GetAttackHit(caller.curTarget, BattleHelper.DamageType.Air))
            {
                caller.DealDamage(caller.curTarget, 10, BattleHelper.DamageType.Air, 0, BattleHelper.ContactLevel.Infinite);
                caller.InflictEffect(caller.curTarget, new Effect(Effect.EffectType.BoltSprout, 1, 3));
                caller.InflictEffect(caller.curTarget, new Effect(Effect.EffectType.Poison, 1, 3));
                if (BattleControl.Instance.GetCurseLevel() > 0)
                {
                    caller.HealHealth(4);
                }
            }
            else
            {
                caller.InvokeMissEvents(caller.curTarget);
            }
        }
    }
}

public class BE_Starfish : BattleEntity
{
    int counterCount;
    public override void Initialize()
    {
        moveset = new List<Move> { gameObject.AddComponent<BM_Starfish_FeebleWave>(), gameObject.AddComponent<BM_Starfish_FatigueFog>(), gameObject.AddComponent<BM_Starfish_LeafStorm>(), gameObject.AddComponent<BM_Shared_Hard_CounterReinforce>()};

        base.Initialize();
    }

    public override void ChooseMoveInternal()
    {
        counterCount = 0;

        currMove = moveset[(posId + BattleControl.Instance.turnCount - 1) % 3];
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
            BattleControl.Instance.AddReactionMoveEvent(this, target.lastAttacker, moveset[3]);
            return true;
        }

        return false;
    }
}

public class BM_Starfish_FeebleWave : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Starfish_FeebleWave;

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
            if (caller.GetAttackHit(t, BattleHelper.DamageType.Water))
            {
                //Note that the focus last turn makes this pretty dangerous
                caller.DealDamage(t, 3, BattleHelper.DamageType.Water, 0, BattleHelper.ContactLevel.Infinite);
                caller.InflictEffect(t, new Effect(Effect.EffectType.AttackDown, 2, 3));
            }
            else
            {
                //Miss
                caller.InvokeMissEvents(t);
            }
        }
    }
}

public class BM_Starfish_FatigueFog : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Starfish_FatigueFog;

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
            if (caller.GetAttackHit(t, BattleHelper.DamageType.Dark))
            {
                //Note that the focus last turn makes this pretty dangerous
                caller.DealDamage(t, 3, BattleHelper.DamageType.Dark, 0, BattleHelper.ContactLevel.Infinite);
                caller.InflictEffect(t, new Effect(Effect.EffectType.EnduranceDown, 2, 3));
            }
            else
            {
                //Miss
                caller.InvokeMissEvents(t);
            }
        }
    }
}

public class BM_Starfish_LeafStorm : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Starfish_LeafStorm;

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
            if (caller.GetAttackHit(t, BattleHelper.DamageType.Earth))
            {
                //Note that the focus last turn makes this pretty dangerous
                caller.DealDamage(t, 3, BattleHelper.DamageType.Earth, 0, BattleHelper.ContactLevel.Infinite);
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

public class BE_CursedEye : BattleEntity
{
    //public bool grounded = false;
    public int counterCount = 0;
    public override void Initialize()
    {
        moveset = new List<Move> { gameObject.AddComponent<BM_CursedEye_UnnervingStare>(), gameObject.AddComponent<BM_CursedEye_MaliciousStare>(), gameObject.AddComponent<BM_CursedEye_CounterSpitefulStare>(), gameObject.AddComponent<BM_CursedEye_Hard_InvertedStare>() };

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
                currMove = moveset[(posId + BattleControl.Instance.turnCount - 1) % 3];
                if (currMove == moveset[2])
                {
                    currMove = moveset[3];
                }
            }
            else
            {
                currMove = moveset[(posId + BattleControl.Instance.turnCount - 1) % 2];
            }
        }

        SpecialTargetChooserFirst(TargetStrategy.TargetStrategyType.LowHP);

        //don't miss the attack
        if (curTarget.HasEffect(Effect.EffectType.Ethereal) && currMove == moveset[1])
        {
            currMove = moveset[0];
        }
    }

    //Not groundable
    /*
    public override IEnumerator DoEvent(BattleHelper.Event eventID)
    {
        yield return StartCoroutine(DefaultEventHandler_Flying(eventID));
    }
    */

    public override IEnumerator PreReact(Move move, BattleEntity target)
    {
        counterCount = 0;

        Effect_ReactionCounter();

        yield return new WaitForSeconds(0.5f);
    }
    public override bool ReactToEvent(BattleEntity target, BattleHelper.Event e, int previousReactions)
    {
        if ((e == BattleHelper.Event.Hurt) && target == this && counterCount <= 0)
        {
            counterCount++;
            BattleControl.Instance.AddReactionMoveEvent(this, target.lastAttacker, moveset[2]);
            return true;
        }

        return false;
    }
}

public class BM_CursedEye_UnnervingStare : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.CursedEye_UnnervingStare;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemy);

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        if (!BattleControl.Instance.EntityValid(caller.curTarget))
        {
            caller.curTarget = null;
        }

        yield return StartCoroutine(caller.Spin(Vector3.up * 360, 0.5f));

        List<BattleEntity> targets = BattleControl.Instance.GetEntitiesSorted(caller, GetBaseTarget());
        foreach (BattleEntity t in targets)
        {
            if (caller.GetAttackHit(t, BattleHelper.DamageType.Light))
            {
                caller.DealDamage(t, 2, BattleHelper.DamageType.Light, 0, BattleHelper.ContactLevel.Infinite);                
            }
            else
            {
                caller.InvokeMissEvents(t);
            }
        }
    }
}

public class BM_CursedEye_MaliciousStare : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.CursedEye_MaliciousStare;

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
                caller.DealDamage(caller.curTarget, 5, BattleHelper.DamageType.Dark, 0, BattleHelper.ContactLevel.Infinite);
                if (BattleControl.Instance.GetCurseLevel() > 0)
                {
                    caller.InflictEffect(caller.curTarget, new Effect(Effect.EffectType.Seal, 1, 3));
                }
            }
            else
            {
                caller.InvokeMissEvents(caller.curTarget);
            }
        }
    }
}

public class BM_CursedEye_CounterSpitefulStare : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.CursedEye_CounterSpitefulStare;

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
                caller.DealDamage(caller.curTarget, 3, BattleHelper.DamageType.Dark, 0, BattleHelper.ContactLevel.Infinite);
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

public class BM_CursedEye_Hard_InvertedStare : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.CursedEye_Hard_InvertedStare;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemy);

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        if (!BattleControl.Instance.EntityValid(caller.curTarget))
        {
            caller.curTarget = null;
        }

        yield return StartCoroutine(caller.Spin(Vector3.up * 360, 0.5f));

        List<BattleEntity> targets = BattleControl.Instance.GetEntitiesSorted(caller, GetBaseTarget());
        foreach (BattleEntity t in targets)
        {
            if (caller.GetAttackHit(t, BattleHelper.DamageType.Water))
            {
                caller.DealDamage(t, 2, BattleHelper.DamageType.Water, 0, BattleHelper.ContactLevel.Infinite);
                caller.InflictEffect(t, new Effect(Effect.EffectType.Inverted, 1, 3));
            }
            else
            {
                caller.InvokeMissEvents(t);
            }
        }
    }
}

public class BE_StrangeTendril : BattleEntity
{
    public override void Initialize()
    {
        moveset = new List<Move> { gameObject.AddComponent<BM_StrangeTendril_StrangeCoil>(), gameObject.AddComponent<BM_StrangeTendril_Slam>() };

        base.Initialize();
    }

    public override void ChooseMoveInternal()
    {
        currMove = moveset[(posId + BattleControl.Instance.turnCount - 1) % 2];
        BasicTargetChooser();
    }

    public override IEnumerator DefaultHurtEvent()
    {
        List<BattleEntity> otherTendrils = BattleControl.Instance.GetEntities((e) => (e.entityID == BattleHelper.EntityID.StrangeTendril && e.posId != posId));
        foreach (BattleEntity b in otherTendrils)
        {
            //Don't infinitely recurse
            if (!b.immediateInEvent)
            {
                b.InvokeHurtEvents(lastDamageType, 0);
            }
        }
        yield return StartCoroutine(base.DefaultHurtEvent());
    }
}

public class BM_StrangeTendril_StrangeCoil : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.StrangeTendril_StrangeCoil;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.Self);

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        if (!BattleControl.Instance.EntityValid(caller.curTarget))
        {
            caller.curTarget = null;
        }

        yield return StartCoroutine(caller.Spin(Vector3.up * 360, 0.5f));

        if (caller.curTarget != null)
        {
            //allow it to buff multiple :)
            if (!caller.HasEffect(Effect.EffectType.DefenseUp) || (caller.GetEffectEntry(Effect.EffectType.DefenseUp).potency < 3))
            {
                caller.InflictEffect(caller, new Effect(Effect.EffectType.DefenseUp, 1, Effect.INFINITE_DURATION), caller.posId, Effect.EffectStackMode.KeepDurAddPot);
            }
            if (BattleControl.Instance.GetCurseLevel() > 0)
            {
                if (!caller.HasEffect(Effect.EffectType.AstralWall))
                {
                    caller.InflictEffect(caller, new Effect(Effect.EffectType.AstralWall, (sbyte)(caller.maxHP / 2), 3));
                }
            }
            caller.InflictEffect(caller, new Effect(Effect.EffectType.Absorb, 3, Effect.INFINITE_DURATION));

            //find the target
            List<BattleEntity> bl = BattleControl.Instance.GetEntitiesSorted(caller, new TargetArea(TargetArea.TargetAreaType.LiveAllyNotSelf));
            List<BattleEntity> blB = bl.FindAll((e) => (e.posId != caller.posId && !e.HasEffect(Effect.EffectType.DefenseUp)));

            BattleEntity boostTarget = null;

            bool nodefense = false;

            if (blB.Count == 0)
            {
                nodefense = true;
                //try bl
                if (bl.Count == 0)
                {
                    //buff yourself I guess?
                    boostTarget = caller;
                } else
                {
                    boostTarget = bl[BattleControl.Instance.GetPsuedoRandom(bl.Count, caller.posId + 2)];
                }
            }
            else
            {
                boostTarget = blB[BattleControl.Instance.GetPsuedoRandom(blB.Count, caller.posId + 2)];
            }

            //Don't stack defense
            if (!nodefense)
            {
                caller.InflictEffect(boostTarget, new Effect(Effect.EffectType.DefenseUp, 1, Effect.INFINITE_DURATION), caller.posId, Effect.EffectStackMode.KeepDurAddPot);
            }
            if (BattleControl.Instance.GetCurseLevel() > 0)
            {
                if (!caller.HasEffect(Effect.EffectType.AstralWall))
                {
                    caller.InflictEffect(boostTarget, new Effect(Effect.EffectType.AstralWall, (sbyte)(boostTarget.maxHP / 2), 3));
                }
            }
            caller.InflictEffect(boostTarget, new Effect(Effect.EffectType.Absorb, 3, Effect.INFINITE_DURATION));
        }
    }
}

public class BM_StrangeTendril_Slam : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.StrangeTendril_Slam;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemy);

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        if (!BattleControl.Instance.EntityValid(caller.curTarget))
        {
            caller.curTarget = null;
        }

        yield return StartCoroutine(caller.Spin(Vector3.up * 360, 1f));

        List<BattleEntity> targets = BattleControl.Instance.GetEntitiesSorted(caller, GetBaseTarget());

        if (caller.GetAttackHit(caller.curTarget, 0))
        {
            caller.DealDamage(caller.curTarget, 12, 0, 0, BattleHelper.ContactLevel.Contact);
            if (BattleControl.Instance.GetCurseLevel() > 0)
            {
                caller.InflictEffect(caller.curTarget, new Effect(Effect.EffectType.Sticky, 1, 3));
            }
        }
        else
        {
            //Miss
            caller.InvokeMissEvents(caller.curTarget);
        }
    }
}

public class BE_DrainBud : BattleEntity
{
    public override void Initialize()
    {
        moveset = new List<Move> { gameObject.AddComponent<BM_DrainBud_PowerDrain>(), gameObject.AddComponent<BM_DrainBud_Hard_DrainBloom>() };

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
            if (BattleControl.Instance.GetCurseLevel() > 0)
            {
                currMove = moveset[(posId + BattleControl.Instance.turnCount - 1) % 2];
            }
            else
            {
                currMove = moveset[0];
            }
        }

        BasicTargetChooser();
    }
}

public class BM_DrainBud_PowerDrain : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.DrainBud_PowerDrain;

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
            if (caller.GetAttackHit(caller.curTarget, 0))
            {
                caller.DealDamage(caller.curTarget, 2, 0, (ulong)BattleHelper.DamageProperties.HPDrainOneToOne, BattleHelper.ContactLevel.Contact);
                yield return new WaitForSeconds(0.25f);
                caller.DealDamage(caller.curTarget, 2, 0, (ulong)BattleHelper.DamageProperties.HPDrainOneToOne, BattleHelper.ContactLevel.Contact);
                yield return new WaitForSeconds(0.25f);
                caller.DealDamage(caller.curTarget, 2, 0, (ulong)BattleHelper.DamageProperties.HPDrainOneToOne, BattleHelper.ContactLevel.Contact);
            }
            else
            {
                caller.InvokeMissEvents(caller.curTarget);
            }
        }
    }
}

public class BM_DrainBud_Hard_DrainBloom : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.DrainBud_Hard_DrainBloom;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemy);

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        if (!BattleControl.Instance.EntityValid(caller.curTarget))
        {
            caller.curTarget = null;
        }

        yield return StartCoroutine(caller.Spin(Vector3.up * 360, 0.5f));

        List<BattleEntity> targets = BattleControl.Instance.GetEntitiesSorted(caller, GetBaseTarget());
        foreach (BattleEntity t in targets)
        {
            if (caller.GetAttackHit(t, 0))
            {
                caller.DealDamage(t, 2, 0, (ulong)(BattleHelper.DamageProperties.HPDrainOneToOne | BattleHelper.DamageProperties.EPLossOneToOne), BattleHelper.ContactLevel.Infinite);
                caller.InflictEffect(t, new Effect(Effect.EffectType.DrainSprout, 1, 3));
            }
            else
            {
                caller.InvokeMissEvents(t);
            }
        }
    }
}