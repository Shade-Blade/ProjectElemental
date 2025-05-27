using System.Collections;
using System.Collections.Generic;
using System.Runtime.Versioning;
using UnityEngine;

public class BE_Sycamore : BattleEntity
{
    public int ai_state;
    public bool second_phase;
    public BattleEntity throne;
    public bool onthrone;

    public int counterCount;

    public override void Initialize()
    {
        ai_state = 0;
        onthrone = false;

        moveset = new List<Move>()
        {
            gameObject.AddComponent<BM_Sycamore_ThornToss>(),
            gameObject.AddComponent<BM_Sycamore_Pollenate>(),
            gameObject.AddComponent<BM_Sycamore_FlowerShuriken>(),
            gameObject.AddComponent<BM_Sycamore_Overgrowth>(),
            gameObject.AddComponent<BM_Sycamore_VineStab>(),
            gameObject.AddComponent<BM_Sycamore_FullBloom>(),
            gameObject.AddComponent<BM_Sycamore_VineField>(),
            gameObject.AddComponent<BM_Sycamore_Fall>(),

            gameObject.AddComponent<BM_Sycamore_Hard_RootShake>(),
            gameObject.AddComponent<BM_Sycamore_Hard_RootGrasp>(),
        };

        base.Initialize();
    }

    public override void ChooseMoveInternal()
    {
        counterCount = 0;
        //time to enter phase 2?
        if (!second_phase && hp <= (int)(maxHP * 2.001f/3))
        {
            second_phase = true;
            currMove = moveset[3];
        } else
        {
            if (onthrone && !BattleControl.Instance.EntityValid(throne))
            {
                onthrone = false;
                currMove = moveset[7];
            } else
            {
                if (throne != null)
                {
                    switch (ai_state)
                    {
                        case 0:
                            currMove = moveset[4];
                            ai_state = 1;
                            break;
                        case 1:
                            currMove = moveset[5];
                            ai_state = 2;
                            break;
                        case 2:
                            currMove = moveset[6];
                            if (BattleControl.Instance.GetCurseLevel() > 0)
                            {
                                ai_state = 3;
                            }
                            else
                            {
                                ai_state = 0;
                            }
                            break;
                        case 3:
                            currMove = moveset[9];
                            ai_state = 0;
                            break;
                    }
                }
                else
                {
                    switch (ai_state)
                    {
                        case 0:
                            currMove = moveset[0];
                            ai_state = 1;
                            break;
                        case 1:
                            currMove = moveset[1];
                            ai_state = 2;
                            break;
                        case 2:
                            currMove = moveset[2];
                            if (BattleControl.Instance.GetCurseLevel() > 0)
                            {
                                ai_state = 3;
                            }
                            else
                            {
                                ai_state = 0;
                            }
                            break;
                        case 3:
                            currMove = moveset[8];
                            ai_state = 0;
                            break;
                    }
                }
            }
        }

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
        if (onthrone && (!BattleControl.Instance.EntityValid(throne) || throne.hp == 0))
        {
            onthrone = false;
            counterCount++;
            BattleControl.Instance.AddReactionMoveEvent(this, target.lastAttacker, moveset[7], true);
            return true;
        }

        return false;
    }
}

public class BE_VinePlatform : BattleEntity
{
    int counterCount;
    public override void Initialize()
    {
        moveset = new List<Move>()
        {
            gameObject.AddComponent<BM_Shared_Hard_CounterHarden>()
        };

        base.Initialize();
    }

    public override void ChooseMoveInternal()
    {
        counterCount = 0;
        //Does nothing
        currMove = null;
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
            BattleControl.Instance.AddReactionMoveEvent(this, target.lastAttacker, moveset[0]);
            return true;
        }

        return false;
    }
}

public class BM_VinePlatform_Hard_CounterSoften : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.VinePlatform_Hard_CounterSoften;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.Self);

    public override IEnumerator ExecuteOutOfTurn(BattleEntity caller, BattleEntity target, int level = 1)
    {
        caller.InflictEffectBuffered(caller, new Effect(Effect.EffectType.Soften, 1, 1));
        yield return new WaitForSeconds(0.5f);
    }
}

public class BM_Sycamore_ThornToss : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Sycamore_ThornToss;

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
                caller.DealDamage(caller.curTarget, 4, BattleHelper.DamageType.Normal, 0, BattleHelper.ContactLevel.Infinite);
            }
            else
            {
                caller.InvokeMissEvents(caller.curTarget);
            }
        }
    }
}

public class BM_Sycamore_Pollenate : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Sycamore_Pollenate;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemy, true);

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        List<BattleEntity> targets = BattleControl.Instance.GetEntitiesSorted(caller, GetBaseTarget());

        yield return StartCoroutine(caller.Spin(Vector3.up * 360, 0.5f));

        foreach (BattleEntity t in targets)
        {
            if (caller.GetAttackHit(t, 0))
            {
                caller.DealDamage(t, 2, BattleHelper.DamageType.Earth, 0, BattleHelper.ContactLevel.Infinite);
                caller.InflictEffect(t, new Effect(Effect.EffectType.Dizzy, 1, 2));
            }
            else
            {
                //Miss
                caller.InvokeMissEvents(t);
            }
        }
    }
}

public class BM_Sycamore_FlowerShuriken : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Sycamore_FlowerShuriken;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemy, true);

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        List<BattleEntity> targets = BattleControl.Instance.GetEntitiesSorted(caller, GetBaseTarget());

        yield return StartCoroutine(caller.Spin(Vector3.up * 360, 0.5f));

        foreach (BattleEntity t in targets)
        {
            if (caller.GetAttackHit(t, 0))
            {
                caller.DealDamage(t, 3, BattleHelper.DamageType.Earth, 0, BattleHelper.ContactLevel.Infinite);
            }
            else
            {
                //Miss
                caller.InvokeMissEvents(t);
            }
        }
    }
}

public class BM_Sycamore_Overgrowth : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Sycamore_Overgrowth;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.Self);

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        //Heal for 10, summon burrow trap and sundew and vine platform
        BE_Sycamore vb = (BE_Sycamore)caller;

        if (BattleControl.Instance.GetEntityByID(1) == null)
        {
            BattleEntity a = BattleControl.Instance.SummonEntity(BattleHelper.EntityID.BurrowTrap, 1);
            vb.InflictEffectForce(a, new Effect(Effect.EffectType.Cooldown, 1, Effect.INFINITE_DURATION));
        }
        if (BattleControl.Instance.GetEntityByID(3) == null)
        {
            BattleEntity b = BattleControl.Instance.SummonEntity(BattleHelper.EntityID.Sundew, 3);
            vb.InflictEffectForce(b, new Effect(Effect.EffectType.Cooldown, 1, Effect.INFINITE_DURATION));
        }

        vb.HealHealth(10);
        vb.posId = 7;
        BattleEntity platform = BattleControl.Instance.SummonEntity(BattleHelper.EntityID.VinePlatform, 2);
        vb.throne = platform;
        vb.onthrone = true;
        vb.homePos = platform.transform.position + platform.height * Vector3.up;
        vb.SetEntityProperty(BattleHelper.EntityProperties.Grounded, false);
        yield return StartCoroutine(vb.Move(vb.homePos, 8));
    }
}

public class BM_Sycamore_VineStab : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Sycamore_VineStab;

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
                caller.DealDamage(caller.curTarget, 7, BattleHelper.DamageType.Normal, 0, BattleHelper.ContactLevel.Infinite);
            }
            else
            {
                caller.InvokeMissEvents(caller.curTarget);
            }
        }
    }
}

public class BM_Sycamore_FullBloom : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Sycamore_FullBloom;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemy, true);

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        List<BattleEntity> targets = BattleControl.Instance.GetEntitiesSorted(caller, GetBaseTarget());

        yield return StartCoroutine(caller.Spin(Vector3.up * 360, 0.5f));

        foreach (BattleEntity t in targets)
        {
            if (caller.GetAttackHit(t, 0))
            {
                caller.DealDamage(t, 4, BattleHelper.DamageType.Earth, 0, BattleHelper.ContactLevel.Infinite);
                caller.InflictEffect(t, new Effect(Effect.EffectType.Dizzy, 1, 2));
            }
            else
            {
                //Miss
                caller.InvokeMissEvents(t);
            }
        }
    }
}

public class BM_Sycamore_VineField : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Sycamore_VineField;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemy, true);

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        List<BattleEntity> targets = BattleControl.Instance.GetEntitiesSorted(caller, GetBaseTarget());

        yield return StartCoroutine(caller.Spin(Vector3.up * 360, 0.5f));

        foreach (BattleEntity t in targets)
        {
            if (caller.GetAttackHit(t, 0))
            {
                caller.DealDamage(t, 5, BattleHelper.DamageType.Earth, 0, BattleHelper.ContactLevel.Infinite);
            }
            else
            {
                //Miss
                caller.InvokeMissEvents(t);
            }
        }
    }
}

public class BM_Sycamore_Fall : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Sycamore_Fall;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.Self);

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        //Heal for 10, summon burrow trap and sundew and vine platform
        BE_Sycamore vb = (BE_Sycamore)caller;

        vb.posId = 2;
        vb.homePos = vb.homePos + vb.homePos.y * Vector3.down;
        yield return StartCoroutine(vb.Move(vb.homePos, 8));

        vb.throne = null;
        vb.InflictEffect(vb, new Effect(Effect.EffectType.DefenseBoost, 1, Effect.INFINITE_DURATION));
        if (BattleControl.Instance.GetCurseLevel() > 0)
        {
            if (BattleControl.Instance.GetEntityByID(1) == null)
            {
                BattleEntity a = BattleControl.Instance.SummonEntity(BattleHelper.EntityID.BurrowTrap, 1);
                //(don't let it attack immediately)
                vb.InflictEffectForce(a, new Effect(Effect.EffectType.Cooldown, 1, Effect.INFINITE_DURATION));
            }
        }
        vb.SetEntityProperty(BattleHelper.EntityProperties.Grounded, true);
    }
    public override IEnumerator ExecuteOutOfTurn(BattleEntity caller, BattleEntity target, int level = 1)
    {
        //Heal for 10, summon burrow trap and sundew and vine platform
        BE_Sycamore vb = (BE_Sycamore)caller;

        vb.posId = 2;
        vb.homePos = vb.homePos + vb.homePos.y * Vector3.down;
        yield return StartCoroutine(vb.Move(vb.homePos, 8));

        vb.throne = null;
        vb.InflictEffect(vb, new Effect(Effect.EffectType.DefenseBoost, 1, Effect.INFINITE_DURATION));
        if (BattleControl.Instance.GetCurseLevel() > 0)
        {
            if (BattleControl.Instance.GetEntityByID(1) == null)
            {
                BattleControl.Instance.SummonEntity(BattleHelper.EntityID.BurrowTrap, 1);
            }
        }
    }
}

public class BM_Sycamore_Hard_RootShake : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Sycamore_Hard_RootShake;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemy, true);

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        List<BattleEntity> targets = BattleControl.Instance.GetEntitiesSorted(caller, GetBaseTarget());

        yield return StartCoroutine(caller.Spin(Vector3.up * 360, 0.5f));

        foreach (BattleEntity t in targets)
        {
            if (caller.GetAttackHit(t, 0))
            {
                caller.DealDamage(t, 2, BattleHelper.DamageType.Earth, 0, BattleHelper.ContactLevel.Infinite);
                caller.InflictEffect(t, new Effect(Effect.EffectType.Defocus, 2, Effect.INFINITE_DURATION));
            }
            else
            {
                //Miss
                caller.InvokeMissEvents(t);
            }
        }
    }
}

public class BM_Sycamore_Hard_RootGrasp : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Sycamore_Hard_RootGrasp;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemy, true);

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        List<BattleEntity> targets = BattleControl.Instance.GetEntitiesSorted(caller, GetBaseTarget());

        yield return StartCoroutine(caller.Spin(Vector3.up * 360, 0.5f));

        foreach (BattleEntity t in targets)
        {
            if (caller.GetAttackHit(t, 0))
            {
                caller.DealDamage(t, 3, BattleHelper.DamageType.Earth, 0, BattleHelper.ContactLevel.Infinite);
                caller.InflictEffect(t, new Effect(Effect.EffectType.Exhausted, 1, 2));
            }
            else
            {
                //Miss
                caller.InvokeMissEvents(t);
            }
        }
    }
}