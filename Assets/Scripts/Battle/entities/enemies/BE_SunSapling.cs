using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BE_SunSapling : BattleEntity
{
    public int ai_state;
    public bool ai_flip;

    public BattleEntity summonLeft;
    public BattleEntity summonRight;

    public override void Initialize()
    {
        summonLeft = null;
        summonRight = null;

        ai_state = 0;
        ai_flip = false;

        moveset = new List<Move>()
        {
            gameObject.AddComponent<BM_SunSapling_FrontSlam>(),
            gameObject.AddComponent<BM_SunSapling_BigSlam>(),
            gameObject.AddComponent<BM_SunSapling_GrowCharge>(),
            gameObject.AddComponent<BM_SunSapling_PowerRoar>()
        };

        InflictEffect(this, new Effect(Effect.EffectType.HealthRegen, 6, Effect.INFINITE_DURATION));

        base.Initialize();
    }

    public override void ChooseMoveInternal()
    {
        switch (ai_state)
        {
            case 0:
                currMove = moveset[0];
                ai_state = 1;
                break;
            case 1:
                if (ai_flip)
                {
                    currMove = moveset[0];
                }
                else
                {
                    currMove = moveset[1];
                }
                ai_state = 2;
                break;
            case 2:
                currMove = moveset[2];
                ai_state = 3;
                break;
            case 3:
                currMove = moveset[3];
                ai_state = 0;
                ai_flip = !ai_flip;
                break;
        }

        if (!BattleControl.Instance.EntityValid(summonLeft))
        {
            summonLeft = null;
        }
        if (!BattleControl.Instance.EntityValid(summonRight))
        {
            summonRight = null;
        }

        BasicTargetChooser();
    }

    public override IEnumerator PostMove()
    {
        InflictEffect(this, new Effect(Effect.EffectType.HealthRegen, 6, Effect.INFINITE_DURATION));

        yield return StartCoroutine(base.PostMove());
    }
}

public class BM_SunSapling_FrontSlam : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.SunSapling_FrontSlam;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemy);

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        if (!BattleControl.Instance.EntityValid(caller.curTarget))
        {
            caller.curTarget = null;
        }

        if (caller.curTarget != null)
        {
            Vector3 tpos = caller.curTarget.transform.position + ((caller.width / 2) + caller.curTarget.width / 2) * Vector3.right;

            yield return StartCoroutine(caller.MoveEasing(tpos, (e) => MainManager.EasingOutIn(e)));

            if (caller.GetAttackHit(caller.curTarget, 0))
            {
                yield return StartCoroutine(caller.Squish(0.067f, 0.2f));
                caller.DealDamage(caller.curTarget, 12, BattleHelper.DamageType.Normal, 0, BattleHelper.ContactLevel.Contact);
                StartCoroutine(caller.RevertScale(0.1f));
            }
            else
            {
                yield return StartCoroutine(caller.Squish(0.067f, 0.2f));

                //Miss
                caller.InvokeMissEvents(caller.curTarget);
            }
        }

        yield return StartCoroutine(caller.MoveEasing(caller.homePos, (e) => MainManager.EasingOutIn(e)));

        //try summon
        yield return StartCoroutine(TrySummonOrFocus(caller));
    }

    public IEnumerator TrySummonOrFocus(BattleEntity caller)
    {
        BE_SunSapling ss = (BE_SunSapling)caller;

        bool summon = false;
        //summon
        if (ss.summonLeft == null)
        {
            ss.summonLeft = BattleControl.Instance.SummonEntity(BattleHelper.EntityID.Sunnybud, 20);
            ss.InflictEffectForce(ss.summonLeft, new Effect(Effect.EffectType.Cooldown, 1, Effect.INFINITE_DURATION));
            summon = true;
        }

        if (ss.summonRight == null)
        {
            ss.summonRight = BattleControl.Instance.SummonEntity(BattleHelper.EntityID.Sunflower, 23);
            ss.InflictEffectForce(ss.summonRight, new Effect(Effect.EffectType.Cooldown, 1, Effect.INFINITE_DURATION));
            summon = true;
        }

        if (!summon)
        {
            List<BattleEntity> focusTargets = BattleControl.Instance.GetEntitiesSorted(caller, new TargetArea(TargetArea.TargetAreaType.AllyNotSelf, true));
            foreach (BattleEntity target in focusTargets)
            {
                //Debug.Log(focusTargets[i]);
                if (BattleControl.Instance.GetCurseLevel() > 0)
                {
                    caller.InflictEffect(target, new Effect(Effect.EffectType.Focus, 3, Effect.INFINITE_DURATION));
                    caller.InflictEffect(target, new Effect(Effect.EffectType.Illuminate, 1, 3));
                }
                else
                {
                    caller.InflictEffect(target, new Effect(Effect.EffectType.Focus, 3, Effect.INFINITE_DURATION));
                }
            }
        }
        yield return null;
    }
}

public class BM_SunSapling_BigSlam : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.SunSapling_BigSlam;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemy, true);

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        //iterate through ground targets
        //To do: get a better way to do this?
        List<BattleEntity> targets = BattleControl.Instance.GetEntitiesSorted(caller, GetBaseTarget());

        float accumulatedX = 0;
        foreach (BattleEntity t in targets)
        {
            accumulatedX += t.transform.position.x;
        }

        accumulatedX /= targets.Count;

        Vector3 tpos = accumulatedX * Vector3.right;

        yield return StartCoroutine(caller.JumpHeavy(tpos, 2, 0.5f, -0.25f));

        yield return StartCoroutine(caller.Squish(0.067f, 0.2f));
        foreach (BattleEntity t in targets)
        {
            if (caller.GetAttackHit(t, 0))
            {
                caller.DealDamage(t, 9, BattleHelper.DamageType.Normal, 0, BattleHelper.ContactLevel.Contact);
            } else
            {
                //Miss
                caller.InvokeMissEvents(t);
            }
        }
        StartCoroutine(caller.RevertScale(0.1f));

        yield return StartCoroutine(caller.MoveEasing(caller.homePos, (e) => MainManager.EasingOutIn(e)));
    }
}

public class BM_SunSapling_GrowCharge : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.SunSapling_GrowCharge;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.Self);

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        yield return StartCoroutine(caller.JumpHeavy(caller.homePos, 2, 0.5f, -0.25f));

        //try summon
        yield return StartCoroutine(TrySummonOrFocus(caller));

        //self focus
        caller.InflictEffect(caller, new Effect(Effect.EffectType.Focus, 6, Effect.INFINITE_DURATION));
    }

    public IEnumerator TrySummonOrFocus(BattleEntity caller)
    {
        BE_SunSapling ss = (BE_SunSapling)caller;

        bool summon = false;
        //summon
        if (ss.summonLeft == null)
        {
            ss.summonLeft = BattleControl.Instance.SummonEntity(BattleHelper.EntityID.Sunnybud, 20);
            ss.InflictEffectForce(ss.summonLeft, new Effect(Effect.EffectType.Cooldown, 1, Effect.INFINITE_DURATION));
            summon = true;
        }

        if (ss.summonRight == null)
        {
            ss.summonRight = BattleControl.Instance.SummonEntity(BattleHelper.EntityID.Sunflower, 23);
            ss.InflictEffectForce(ss.summonRight, new Effect(Effect.EffectType.Cooldown, 1, Effect.INFINITE_DURATION));
            summon = true;
        }

        if (!summon)
        {
            List<BattleEntity> focusTargets = BattleControl.Instance.GetEntitiesSorted(caller, new TargetArea(TargetArea.TargetAreaType.AllyNotSelf, true));
            foreach (BattleEntity target in focusTargets)
            {
                //Debug.Log(focusTargets[i]);
                if (BattleControl.Instance.GetCurseLevel() > 0)
                {
                    caller.InflictEffect(target, new Effect(Effect.EffectType.Focus, 3, Effect.INFINITE_DURATION));
                    caller.InflictEffect(target, new Effect(Effect.EffectType.Illuminate, 1, 3));
                }
                else
                {
                    caller.InflictEffect(target, new Effect(Effect.EffectType.Focus, 3, Effect.INFINITE_DURATION));
                }
            }
        }
        yield return null;
    }
}

public class BM_SunSapling_PowerRoar : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.SunSapling_PowerRoar;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemy, true);

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        //iterate through ground targets
        //To do: get a better way to do this?
        List<BattleEntity> targets = BattleControl.Instance.GetEntitiesSorted(caller, GetBaseTarget());

        yield return StartCoroutine(caller.JumpHeavy(caller.homePos, 2, 0.5f, -0.25f));

        List<BattleEntity> focusTargets = BattleControl.Instance.GetEntitiesSorted(caller, new TargetArea(TargetArea.TargetAreaType.AllyNotSelf, true));
        for (int i = 0; i < focusTargets.Count; i++)
        {
            caller.InflictEffect(focusTargets[i], new Effect(Effect.EffectType.Focus, 3, Effect.INFINITE_DURATION));
        }

        if (BattleControl.Instance.GetCurseLevel() > 0)
        {
            caller.HealHealth(3);
            caller.InflictEffect(caller, new Effect(Effect.EffectType.Absorb, 3, Effect.INFINITE_DURATION));
        }

        yield return StartCoroutine(caller.Squish(0.067f, 0.2f));
        foreach (BattleEntity t in targets)
        {
            if (caller.GetAttackHit(t, 0))
            {
                caller.DealDamage(t, 4, BattleHelper.DamageType.Fire, 0, BattleHelper.ContactLevel.Contact);
                if (BattleControl.Instance.GetCurseLevel() > 0)
                {
                    caller.InflictEffect(t, new Effect(Effect.EffectType.Sunflame, 1, 3));
                }
            } else
            {
                //Miss
                caller.InvokeMissEvents(t);
            }
        }
        yield return StartCoroutine(caller.RevertScale(0.1f));
    }
}