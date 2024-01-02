using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BE_Rootling : BattleEntity
{
    public int ai_state;
    public bool ai_flip;

    public override void Initialize()
    {
        ai_state = 0;
        ai_flip = false;

        moveset = new List<Move>()
        {
            gameObject.AddComponent<BM_Shared_FrontBite>(),
            gameObject.AddComponent<BM_Rootling_FrontSlam>(),
            gameObject.AddComponent<BM_Rootling_DoubleSlam>(),
            gameObject.AddComponent<BM_Rootling_Dig>(),
            gameObject.AddComponent<BM_Rootling_Uproot>()
        };

        base.Initialize();
    }

    public override void ChooseMoveInternal()
    {
        if (GetEntityProperty(BattleHelper.EntityProperties.NoTarget, true))
        {
            ai_state = 3;
        }

        switch (ai_state)
        {
            case 0:
                currMove = moveset[0];
                ai_state = 1;
                break;
            case 1:
                if (!ai_flip)
                {
                    currMove = moveset[2];
                }
                else
                {
                    currMove = moveset[1];
                }
                ai_state = 2;
                break;
            case 2:
                currMove = moveset[3];
                ai_state = 3;
                break;
            case 3:
                if (GetEntityProperty(BattleHelper.EntityProperties.NoTarget, true))
                {
                    currMove = moveset[4];
                }
                else
                {
                    currMove = moveset[3];
                }
                ai_state = 0;
                ai_flip = !ai_flip;
                break;
        }

        BasicTargetChooser();
    }
}

public class BM_Rootling_FrontSlam : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Rootling_FrontSlam;
    //public override string GetName() => "Rootling Front Bite";
    //public override string GetDescription() => "";

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemy);

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        //Debug.Log(caller.id + " jump");
        if (!BattleControl.Instance.EntityValid(caller.curTarget))
        {
            caller.curTarget = null;
        }

        if (caller.curTarget != null)
        {
            Vector3 tpos = caller.curTarget.transform.position + ((caller.width / 2) + caller.curTarget.width) * Vector3.right;

            yield return StartCoroutine(caller.Move(tpos));

            if (caller.GetAttackHit(caller.curTarget, 0))
            {
                yield return StartCoroutine(caller.Squish(0.067f, 0.2f));
                caller.DealDamage(caller.curTarget, 4, BattleHelper.DamageType.Normal, 0, BattleHelper.ContactLevel.Contact);
                StartCoroutine(caller.RevertScale(0.1f));
            }
            else
            {
                yield return StartCoroutine(caller.Squish(0.067f, 0.2f));

                //Miss
                caller.InvokeMissEvents(caller.curTarget);
                StartCoroutine(caller.RevertScale(0.1f));
            }
        }

        yield return StartCoroutine(caller.Move(caller.homePos));
    }
}

public class BM_Rootling_DoubleSlam : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Rootling_DoubleSlam;
    //public override string GetName() => "Rootling Front Bite";
    //public override string GetDescription() => "";

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
                caller.DealDamage(t, 3, BattleHelper.DamageType.Normal, 0, BattleHelper.ContactLevel.Contact);
            } else
            {
                //Miss
                caller.InvokeMissEvents(t);
            }
        }
        StartCoroutine(caller.RevertScale(0.1f));

        yield return StartCoroutine(caller.Move(caller.homePos));
    }
}

public class BM_Rootling_Dig : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Rootling_Dig;
    //public override string GetName() => "Rootling Front Bite";
    //public override string GetDescription() => "";

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.Self);

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        caller.SetEntityProperty(BattleHelper.EntityProperties.NoTarget, true);
        yield return StartCoroutine(caller.Squish(0.067f, 0.2f));

        if (BattleControl.Instance.GetCurseLevel() > 0)
        {
            caller.HealHealth(2);
            caller.InflictEffect(caller, new Effect(Effect.EffectType.Absorb, 2, 255));
        }

        //StartCoroutine(caller.RevertScale(0.1f));        
    }
}

public class BM_Rootling_Uproot : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Rootling_Uproot;
    //public override string GetName() => "Rootling Front Bite";
    //public override string GetDescription() => "";

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemy, true);

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        caller.SetEntityProperty(BattleHelper.EntityProperties.NoTarget, false);
        //yield return StartCoroutine(caller.Squish(0.067f, 0.2f));
        //yield return StartCoroutine(caller.RevertScale(0.1f));
        StartCoroutine(caller.RevertScale(0.1f));

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
                    caller.DealDamage(t, 4, BattleHelper.DamageType.Normal, 0, BattleHelper.ContactLevel.Contact);
                } else
                {
                    //Miss
                    caller.InvokeMissEvents(t);
                }
            }
            StartCoroutine(caller.RevertScale(0.1f));

        yield return StartCoroutine(caller.Move(caller.homePos));
    }
}