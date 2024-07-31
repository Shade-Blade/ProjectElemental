using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

//file for debug moves

public class DebugJump : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Unknown;
    public DebugJump()
    {
    }

    //public override string GetName() => "Debug Jump";
    //public override string GetDescription() => "Jump on an enemy.";
    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemy, false);
    //public override float GetBasePower() => 1.0f;
    //public override int GetBaseCost() => 0;

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        //Debug.Log(caller.id + " jump");
        if (!BattleControl.Instance.EntityValid(caller.curTarget))
        {
            caller.curTarget = null;
        }

        if (caller.curTarget != null)
        {
            Vector3 tpos = caller.curTarget.transform.position + caller.curTarget.height * Vector3.up;
            Vector3 spos = transform.position;

            //yield return StartCoroutine(caller.Squish(0.067f, 0.2f));
            //StartCoroutine(caller.RevertScale(0.1f));
            yield return StartCoroutine(caller.JumpHeavy(tpos, 2, 0.5f, -0.25f));

            if (caller.GetAttackHit(caller.curTarget, 0))
            {
                caller.DealDamage(caller.curTarget, 2, BattleHelper.DamageType.Normal, 0, BattleHelper.ContactLevel.Contact);
                //yield return StartCoroutine(caller.Squish(0.067f, 0.2f));
                //StartCoroutine(caller.RevertScale(0.1f));
                yield return StartCoroutine(caller.JumpHeavy(caller.homePos, 2, 0.5f, 0.15f));
            }
            else
            {
                //Miss

                //extrapolate the move curve
                yield return StartCoroutine(caller.ExtrapolateJumpHeavy(spos, tpos, 2, 0.5f, -0.25f));
                yield return StartCoroutine(caller.Move(caller.homePos));
            }
        } else
        {
            yield return StartCoroutine(caller.JumpHeavy(caller.homePos, 2, 0.5f, 0.15f));
        }
    }
}


public class DebugFly : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Unknown;

    public DebugFly()
    {
    }

    //public override string GetName() => "Debug Fly";
    //public override string GetDescription() => "Fly and drop onto an enemy.";
    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemy, false);
    //public override float GetBasePower() => 1.5f;
    //public override int GetBaseCost() => 4;

    public override int GetMaxLevel(BattleEntity caller)
    {
        return 2;
    }

    public override TargetArea GetTargetArea(BattleEntity caller, int level = 1)
    {
        if (level > 1)
        {
            return new TargetArea(TargetArea.TargetAreaType.LiveEnemyLowFrontmost, false);
        } else
        {
            return GetBaseTarget();
        }
    }

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        //Debug.Log(caller.id + " fly");
        if (!BattleControl.Instance.EntityValid(caller.curTarget))
        {
            caller.curTarget = null;
        }

        yield return StartCoroutine(caller.JumpHeavy(caller.transform.position + Vector3.up * 10, 2, 0.5f, 0.25f));

        if (caller.curTarget != null)
        {
            if (caller.GetAttackHit(caller.curTarget, 0))
            {
                Vector3 tpos = caller.curTarget.transform.position + caller.curTarget.height * Vector3.up;
                caller.Warp(tpos + Vector3.up * 10);
                yield return StartCoroutine(caller.JumpHeavy(tpos, 2, 0.5f, -0.25f));
                caller.DealDamage(caller.curTarget, 3 + (level - 1) * 2, BattleHelper.DamageType.Normal);
            }
            else
            {
                Vector3 tpos = caller.curTarget.transform.position - caller.curTarget.transform.position.y * Vector3.up + Vector3.forward * 0.05f;
                caller.Warp(tpos + Vector3.up * 10);
                yield return StartCoroutine(caller.JumpHeavy(tpos, 2, 0.5f, -0.25f));
            }
        }
        /*
        if (interrupted)
        {
            yield return new WaitUntil(() => caller.eventQueue.Count == 0 && !caller.inEvent); //wait for counterhurt and stuff to execute
            yield return StartCoroutine(caller.Jump(caller.homePos, 2, 1));
            yield break;
        }
        */
        yield return StartCoroutine(caller.JumpHeavy(caller.homePos, 2, 0.5f, 0.25f));
    }

    public override void PreMove(BattleEntity caller, int level = 1)
    {

    }
    public override void PostMove(BattleEntity caller, int level = 1)
    {
    }
}

public class DebugCounter : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Unknown;

    public DebugCounter()
    {
        //isCounterReact = false;
    }

    //public override string GetName() => "Debug Counter";
    //public override string GetDescription() => "Counter certain attacks.";
    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.Self, false);
    //public override float GetBasePower() => 0.5f;
    //public override int GetBaseCost() => 2;

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        yield return StartCoroutine(caller.Spin(Vector3.up * 360, 0.25f));
        caller.InflictEffect(caller, new Effect(Effect.EffectType.Focus, 3, Effect.INFINITE_DURATION));
    }

    public override IEnumerator ExecuteOutOfTurn(BattleEntity caller, BattleEntity causer, int level = 1)
    {
        caller.DealDamage(causer, 1, BattleHelper.DamageType.Normal, 0); // (uint)BattleHelper.DamageProperties.Counter);
        //Debug.Log("start");
        StartCoroutine(caller.Jump(caller.homePos, 2, 0.15f));
        yield return StartCoroutine(caller.Spin(Vector3.up * 360, 0.15f));

        //so can only counter once per turn
        //isCounterReact = false;
    }

    public override void PreMove(BattleEntity caller, int level = 1)
    {
        //isCounterReact = true;
    }
    public override void PostMove(BattleEntity caller, int level = 1)
    {
    }
}

public class DebugRush : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Unknown;

    public DebugRush()
    {
    }

    //public override string GetName() => "Debug Rush";
    //public override string GetDescription() => "Rush through all low enemies";
    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemyLow, true);
    //public override float GetBasePower() => 1.0f;
    //public override int GetBaseCost() => 3;

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        //iterate through ground targets
        //To do: get a better way to do this?
        List<BattleEntity> targets = BattleControl.Instance.GetEntitiesSorted(caller, GetBaseTarget());
        for (int i = 0; i < targets.Count; i++)
        {
            yield return caller.Move(targets[i].gameObject.transform.position, 10f);
            caller.DealDamage(targets[i], 2, BattleHelper.DamageType.Normal, (ulong)BattleHelper.DamageProperties.RemoveMaxHP);
        }

        yield return StartCoroutine(caller.JumpHeavy(caller.homePos, 2, 0.5f, 0.15f));
    }
}