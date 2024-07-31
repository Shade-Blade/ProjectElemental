using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BE_Slime : BattleEntity
{
    int counterCount;
    public override void Initialize()
    {
        moveset = new List<Move> { gameObject.AddComponent<BM_Slime_Stomp>(), gameObject.AddComponent<BM_Slime_SplashStomp>(), gameObject.AddComponent<BM_Shared_Hard_CounterRecover>() };

        base.Initialize();
    }

    public override void ChooseMoveInternal()
    {
        //also reset here in case something weird happens
        counterCount = 0;

        if (BattleControl.Instance.GetCurseLevel() > 0)
        {
            currMove = moveset[(posId + BattleControl.Instance.turnCount - 1) % 2];
        }
        else
        {
            currMove = moveset[0];
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

public class BM_Slime_Stomp : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Slime_Stomp;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemy);

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        if (!BattleControl.Instance.EntityValid(caller.curTarget))
        {
            caller.curTarget = null;
        }

        if (caller.curTarget != null)
        {
            Vector3 tpos = caller.curTarget.ApplyScaledOffset(caller.curTarget.stompOffset);
            Vector3 spos = transform.position;

            yield return StartCoroutine(caller.JumpHeavy(tpos, 2, 0.5f, -0.25f));

            if (caller.GetAttackHit(caller.curTarget, 0))
            {
                caller.DealDamage(caller.curTarget, 3, BattleHelper.DamageType.Normal, 0, BattleHelper.ContactLevel.Contact);
                yield return StartCoroutine(caller.JumpHeavy(caller.homePos, 2, 0.5f, 0.15f));
            }
            else
            {
                caller.InvokeMissEvents(caller.curTarget);

                //extrapolate the move curve
                yield return StartCoroutine(caller.ExtrapolateJumpHeavy(spos, tpos, 2, 0.5f, -0.25f));
                yield return StartCoroutine(caller.Move(caller.homePos));
            }
        }

        yield return StartCoroutine(caller.Move(caller.homePos));
    }
}

public class BM_Slime_SplashStomp : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Slime_SplashStomp;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemy);

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        if (!BattleControl.Instance.EntityValid(caller.curTarget))
        {
            caller.curTarget = null;
        }

        if (caller.curTarget != null)
        {
            Vector3 tpos = caller.curTarget.ApplyScaledOffset(caller.curTarget.stompOffset);
            Vector3 spos = transform.position;

            yield return StartCoroutine(caller.JumpHeavy(tpos, 5, 0.8f, -0.25f));

            if (caller.GetAttackHit(caller.curTarget, BattleHelper.DamageType.Water))
            {
                caller.DealDamage(caller.curTarget, 4, BattleHelper.DamageType.Water, 0, BattleHelper.ContactLevel.Contact);
                yield return StartCoroutine(caller.JumpHeavy(caller.homePos, 2, 0.5f, 0.15f));
            }
            else
            {
                caller.InvokeMissEvents(caller.curTarget);

                //extrapolate the move curve
                yield return StartCoroutine(caller.ExtrapolateJumpHeavy(spos, tpos, 2, 0.5f, -0.25f));
                yield return StartCoroutine(caller.Move(caller.homePos));
            }
        }

        yield return StartCoroutine(caller.Move(caller.homePos));
    }
}

public class BE_Slimewalker : BattleEntity
{
    public override void Initialize()
    {
        moveset = new List<Move> { gameObject.AddComponent<BM_Shared_Slash>(), gameObject.AddComponent<BM_Slimewalker_WaterCannon>(), gameObject.AddComponent<BM_Slimewalker_SoftSplash>() };

        base.Initialize();
    }

    public override void ChooseMoveInternal()
    {
        if (BattleControl.Instance.GetCurseLevel() > 0)
        {
            currMove = moveset[(posId + BattleControl.Instance.turnCount - 1) % 3];
        }
        else
        {
            currMove = moveset[(posId + BattleControl.Instance.turnCount - 1) % 2];
        }

        if (currMove == moveset[1])
        {
            //SpecialTargetChooserFirst(TargetStrategy.TargetStrategyType.Slimewalker);
            List<BattleEntity> bl = BattleControl.Instance.GetEntities(this, currMove.GetTargetArea(this));
            TargetStrategy strategy = new TargetStrategy(TargetStrategy.TargetStrategyType.FrontMost);
            bl.Sort((a, b) => strategy.selectorFunction(a, b));
            if (GetBerserkTarget() != null)
            {
                if (bl.Contains(curTarget))
                {
                    curTarget = GetBerserkTarget();
                }
            } else
            {
                //get target by damage check
                int damage = 0;
                curTarget = bl[0];

                foreach (BattleEntity be in bl)
                {
                    int d = DealDamageCalculation(be, 4, BattleHelper.DamageType.Water, 0);
                    if (d > damage)
                    {
                        damage = d;
                        curTarget = be;
                    }
                }
            }
        }
        else
        {
            BasicTargetChooser();
        }
    }
}

public class BM_Slimewalker_WaterCannon : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Slimewalker_WaterCannon;

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
            if (caller.GetAttackHit(caller.curTarget, BattleHelper.DamageType.Water))
            {
                caller.DealDamage(caller.curTarget, 3, BattleHelper.DamageType.Water, 0, BattleHelper.ContactLevel.Infinite);
            }
            else
            {
                caller.InvokeMissEvents(caller.curTarget);
            }
        }
    }
}

public class BM_Slimewalker_SoftSplash : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Slimewalker_Hard_SoftSplash;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemy);

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        if (!BattleControl.Instance.EntityValid(caller.curTarget))
        {
            caller.curTarget = null;
        }

        if (caller.curTarget != null)
        {
            Vector3 tpos = caller.curTarget.ApplyScaledOffset(caller.curTarget.stompOffset);
            Vector3 spos = transform.position;

            yield return StartCoroutine(caller.JumpHeavy(tpos, 2, 0.5f, -0.25f));

            if (caller.GetAttackHit(caller.curTarget, BattleHelper.DamageType.Water))
            {
                caller.DealDamage(caller.curTarget, 1, BattleHelper.DamageType.Water, 0, BattleHelper.ContactLevel.Contact);
                caller.InflictEffect(caller.curTarget, new Effect(Effect.EffectType.DefenseDown, 2, 3));
                yield return StartCoroutine(caller.JumpHeavy(caller.homePos, 2, 0.5f, 0.15f));
            }
            else
            {
                caller.InvokeMissEvents(caller.curTarget);

                //extrapolate the move curve
                yield return StartCoroutine(caller.ExtrapolateJumpHeavy(spos, tpos, 2, 0.5f, -0.25f));
                yield return StartCoroutine(caller.Move(caller.homePos));
            }
        }

        yield return StartCoroutine(caller.Move(caller.homePos));
    }
}

public class BE_Slimeworm : BattleEntity
{
    public override void Initialize()
    {
        moveset = new List<Move> { gameObject.AddComponent<BM_Slimeworm_Charge>(), gameObject.AddComponent<BM_Slimeworm_Mortar>() };

        base.Initialize();
    }

    public override void ChooseMoveInternal()
    {
        if (GetEntityProperty(BattleHelper.EntityProperties.StateCharge))
        {
            currMove = moveset[1];
            SetEntityProperty(BattleHelper.EntityProperties.StateCharge, false);
        }
        else
        {
            currMove = moveset[0];
        }

        BasicTargetChooser();
    }
}

public class BM_Slimeworm_Charge : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Slimeworm_Charge;

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
            caller.SetEntityProperty(BattleHelper.EntityProperties.StateCharge);
            if (BattleControl.Instance.GetCurseLevel() > 0)
            {
                caller.InflictEffect(caller, new Effect(Effect.EffectType.Focus, 3, Effect.INFINITE_DURATION));
                caller.InflictEffect(caller, new Effect(Effect.EffectType.Absorb, 3, Effect.INFINITE_DURATION));
                caller.HealHealth(4);
            }
            else
            {
                caller.InflictEffect(caller, new Effect(Effect.EffectType.Focus, 3, Effect.INFINITE_DURATION));
                caller.InflictEffect(caller, new Effect(Effect.EffectType.Absorb, 3, Effect.INFINITE_DURATION));
            }
        }
    }
}

public class BM_Slimeworm_Mortar : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Slimeworm_Mortar;

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
                caller.DealDamage(t, 1, BattleHelper.DamageType.Water, 0, BattleHelper.ContactLevel.Infinite);
            }
            else
            {
                //Miss
                caller.InvokeMissEvents(t);
            }
        }
    }
}

public class BE_Slimebloom : BattleEntity
{
    public override void Initialize()
    {
        moveset = new List<Move> { gameObject.AddComponent<BM_Slimebloom_Zap>(), gameObject.AddComponent<BM_Slimebloom_Lob>() };

        base.Initialize();
    }

    public override void ChooseMoveInternal()
    {
        currMove = moveset[(posId + BattleControl.Instance.turnCount - 1) % 2];
        BasicTargetChooser();
    }
}

public class BM_Slimebloom_Zap : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Slimebloom_Zap;

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
                bool hasStatus = caller.curTarget.HasStatus();
                caller.DealDamage(caller.curTarget, 3, BattleHelper.DamageType.Air, 0, BattleHelper.ContactLevel.Infinite);
                if (!hasStatus)
                {
                    caller.InflictEffect(caller.curTarget, new Effect(Effect.EffectType.Paralyze, 1, 1));
                }
                if (BattleControl.Instance.GetCurseLevel() > 0)
                {
                    caller.InflictEffectBuffered(caller.curTarget, new Effect(Effect.EffectType.Sunder, 1, Effect.INFINITE_DURATION));
                }
            }
            else
            {
                caller.InvokeMissEvents(caller.curTarget);
            }
        }
    }
}

public class BM_Slimebloom_Lob : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Slimebloom_Lob;

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
                bool hasStatus = caller.curTarget.HasStatus();
                caller.DealDamage(caller.curTarget, 2, BattleHelper.DamageType.Water, 0, BattleHelper.ContactLevel.Infinite);
                if (!hasStatus)
                {
                    caller.InflictEffect(caller.curTarget, new Effect(Effect.EffectType.Sleep, 1, 1));
                }
                if (BattleControl.Instance.GetCurseLevel() > 0)
                {
                    caller.InflictEffectBuffered(caller.curTarget, new Effect(Effect.EffectType.Defocus, 1, Effect.INFINITE_DURATION));
                }
            }
            else
            {
                caller.InvokeMissEvents(caller.curTarget);
            }
        }
    }
}

public class BE_Sirenfish : BattleEntity
{
    public override void Initialize()
    {
        moveset = new List<Move> { gameObject.AddComponent<BM_Sirenfish_BubbleSong>(), gameObject.AddComponent<BM_Sirenfish_PowerSong>(), gameObject.AddComponent<BM_Sirenfish_NightmareBite>() };

        base.Initialize();
    }

    public override void ChooseMoveInternal()
    {
        if (GetEntityProperty(BattleHelper.EntityProperties.Grounded))
        {
            currMove = moveset[0];
        }
        else
        {
            currMove = moveset[(posId + BattleControl.Instance.turnCount - 1) % 2];
        }

        bool nightmare = false;        
        if (BattleControl.Instance.GetCurseLevel() > 0)
        {
            List<BattleEntity> bl = BattleControl.Instance.GetEntities(this, new TargetArea(TargetArea.TargetAreaType.LiveEnemy));
            TargetStrategy strategy = new TargetStrategy(TargetStrategy.TargetStrategyType.FrontMost);
            bl.Sort((a, b) => strategy.selectorFunction(a, b));

            for (int i = 0; i < bl.Count; i++)
            {
                if (bl[i].HasEffect(Effect.EffectType.Sleep))
                {
                    currMove = moveset[2];
                    curTarget = bl[i];
                    nightmare = true;
                    break;
                }
            }
        }

        if (!nightmare)
        {
            BasicTargetChooser();
        }
    }

    public override IEnumerator DoEvent(BattleHelper.Event eventID)
    {
        yield return StartCoroutine(DefaultEventHandler_Flying(eventID));
    }
}

public class BM_Sirenfish_BubbleSong : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Sirenfish_BubbleSong;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemy);

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        //fly back up        
        yield return StartCoroutine(caller.FlyingFlyBackUp());

        if (!BattleControl.Instance.EntityValid(caller.curTarget))
        {
            caller.curTarget = null;
        }

        yield return StartCoroutine(caller.Spin(Vector3.up * 360, 0.5f));

        if (caller.curTarget != null)
        {
            if (caller.GetAttackHit(caller.curTarget, 0))
            {
                bool hasStatus = caller.curTarget.HasStatus();
                caller.DealDamage(caller.curTarget, 5, BattleHelper.DamageType.Normal, 0, BattleHelper.ContactLevel.Infinite);
                if (!hasStatus)
                {
                    if (BattleControl.Instance.GetCurseLevel() > 0)
                    {
                        caller.InflictEffect(caller.curTarget, new Effect(Effect.EffectType.Sleep, 1, 3));
                    }
                    else
                    {
                        caller.InflictEffect(caller.curTarget, new Effect(Effect.EffectType.Sleep, 1, 2));
                    }
                }
            }
            else
            {
                caller.InvokeMissEvents(caller.curTarget);
            }
        }
    }
}

public class BM_Sirenfish_PowerSong : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Sirenfish_PowerSong;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveAlly);

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        if (!BattleControl.Instance.EntityValid(caller.curTarget))
        {
            caller.curTarget = null;
        }

        yield return StartCoroutine(caller.Spin(Vector3.up * 360, 0.5f));

        List<BattleEntity> targets = BattleControl.Instance.GetEntitiesSorted(caller, GetBaseTarget());

        foreach (BattleEntity be in targets)
        {
            caller.InflictEffect(be, new Effect(Effect.EffectType.Focus, 1, Effect.INFINITE_DURATION));
        }
    }
}

public class BM_Sirenfish_NightmareBite : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Sirenfish_Hard_NightmareBite;

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
            Vector3 offset = Vector3.right * 3f + Vector3.up * 2f;
            Vector3 tposA = caller.curTarget.transform.position + offset;
            Vector3 tposend = caller.curTarget.transform.position + ((caller.width / 2) + (caller.curTarget.width / 2)) * Vector3.right + (caller.curTarget.height / 2) * Vector3.up;


            bool backFlag = false;

            if (!BattleControl.Instance.IsFrontmostLow(caller, caller.curTarget))
            {
                tposA = BattleControl.Instance.GetFrontmostLow(caller).transform.position + offset + Vector3.back * 0.5f;
                backFlag = true;
            }

            Vector3 tposmid = (tposA + tposend) / 2 + Vector3.down * 0.5f;

            float dist = tposA.x - tposend.x - 0.25f;

            yield return StartCoroutine(caller.Move(tposA));

            Vector3[] positions = new Vector3[] { tposA, tposmid, tposend };

            if (caller.GetAttackHit(caller.curTarget, BattleHelper.DamageType.Dark))
            {
                yield return StartCoroutine(caller.FollowBezierCurve(backFlag ? 0.4f : 0.3f, (float a) => MainManager.EasingQuadratic(a, -0.2f), positions));

                caller.DealDamage(caller.curTarget, 5, BattleHelper.DamageType.Dark, 0, BattleHelper.ContactLevel.Contact);
            }
            else
            {
                yield return StartCoroutine(caller.FollowBezierCurve(0.15f * dist, (float a) => MainManager.EasingQuadratic(a, -0.2f), 1.25f, positions));
                caller.InvokeMissEvents(caller.curTarget);
            }
        }

        yield return StartCoroutine(caller.Move(caller.homePos));
    }
}