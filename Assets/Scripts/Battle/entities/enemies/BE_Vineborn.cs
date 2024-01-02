using System.Collections;
using System.Collections.Generic;
using System.Runtime.Versioning;
using UnityEngine;

public class BE_Vineborn : BattleEntity
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
            gameObject.AddComponent<BM_Vineborn_ThornToss>(),
            gameObject.AddComponent<BM_Vineborn_Pollenate>(),
            gameObject.AddComponent<BM_Vineborn_FlowerShuriken>(),
            gameObject.AddComponent<BM_Vineborn_Overgrowth>(),
            gameObject.AddComponent<BM_Vineborn_VineStab>(),
            gameObject.AddComponent<BM_Vineborn_FullBloom>(),
            gameObject.AddComponent<BM_Vineborn_VineField>(),
            gameObject.AddComponent<BM_Vineborn_Fall>(),
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

public class BM_Vineborn_ThornToss : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Vineborn_ThornToss;

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

public class BM_Vineborn_Pollenate : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Vineborn_Pollenate;

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
                caller.InflictEffect(t, new Effect(Effect.EffectType.Dizzy, 1, 1));
                if (BattleControl.Instance.GetCurseLevel() > 0)
                {
                    caller.InflictEffect(t, new Effect(Effect.EffectType.Defocus, 1, 255));
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

public class BM_Vineborn_FlowerShuriken : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Vineborn_FlowerShuriken;

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
                if (BattleControl.Instance.GetCurseLevel() > 0)
                {
                    caller.InflictEffect(t, new Effect(Effect.EffectType.Defocus, 1, 255));
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

public class BM_Vineborn_Overgrowth : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Vineborn_Overgrowth;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.Self);

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        //Heal for 10, summon burrow trap and sundew and vine platform
        BE_Vineborn vb = (BE_Vineborn)caller;

        if (BattleControl.Instance.GetEntityByID(1) == null)
        {
            BattleEntity a = BattleControl.Instance.SummonEntity(BattleHelper.EntityID.BurrowTrap, 1);
            vb.InflictEffectForce(a, new Effect(Effect.EffectType.Cooldown, 1, 255));
        }
        if (BattleControl.Instance.GetEntityByID(3) == null)
        {
            BattleEntity b = BattleControl.Instance.SummonEntity(BattleHelper.EntityID.Sundew, 3);
            vb.InflictEffectForce(b, new Effect(Effect.EffectType.Cooldown, 1, 255));
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

public class BM_Vineborn_VineStab : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Vineborn_VineStab;

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

public class BM_Vineborn_FullBloom : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Vineborn_FullBloom;

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
                caller.InflictEffect(t, new Effect(Effect.EffectType.Dizzy, 1, 1));
                if (BattleControl.Instance.GetCurseLevel() > 0)
                {
                    caller.InflictEffect(t, new Effect(Effect.EffectType.Defocus, 1, 255));
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

public class BM_Vineborn_VineField : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Vineborn_VineField;

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
                if (BattleControl.Instance.GetCurseLevel() > 0)
                {
                    caller.InflictEffect(t, new Effect(Effect.EffectType.Defocus, 1, 255));
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

public class BM_Vineborn_Fall : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Vineborn_Fall;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.Self);

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        //Heal for 10, summon burrow trap and sundew and vine platform
        BE_Vineborn vb = (BE_Vineborn)caller;

        vb.posId = 2;
        vb.homePos = vb.homePos + vb.homePos.y * Vector3.down;
        yield return StartCoroutine(vb.Move(vb.homePos, 8));

        vb.throne = null;
        vb.InflictEffect(vb, new Effect(Effect.EffectType.DefenseBoost, 1, 255));
        if (BattleControl.Instance.GetCurseLevel() > 0)
        {
            if (BattleControl.Instance.GetEntityByID(1) == null)
            {
                BattleEntity a = BattleControl.Instance.SummonEntity(BattleHelper.EntityID.BurrowTrap, 1);
                //(don't let it attack immediately)
                vb.InflictEffectForce(a, new Effect(Effect.EffectType.Cooldown, 1, 255));
            }
        }
        vb.SetEntityProperty(BattleHelper.EntityProperties.Grounded, true);
    }
    public override IEnumerator ExecuteOutOfTurn(BattleEntity caller, BattleEntity target, int level = 1)
    {
        //Heal for 10, summon burrow trap and sundew and vine platform
        BE_Vineborn vb = (BE_Vineborn)caller;

        vb.posId = 2;
        vb.homePos = vb.homePos + vb.homePos.y * Vector3.down;
        yield return StartCoroutine(vb.Move(vb.homePos, 8));

        vb.throne = null;
        vb.InflictEffect(vb, new Effect(Effect.EffectType.DefenseBoost, 1, 255));
        if (BattleControl.Instance.GetCurseLevel() > 0)
        {
            if (BattleControl.Instance.GetEntityByID(1) == null)
            {
                BattleControl.Instance.SummonEntity(BattleHelper.EntityID.BurrowTrap, 1);
            }
        }
    }
}