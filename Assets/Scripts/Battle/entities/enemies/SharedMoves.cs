using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//File for shared moves
//Note: I am following an idea of "One script, one description"
//  (Which means that moves with extra effects that would warrant a different description must use a different script)
//  (This should help keep scripts from getting too complicated or multivalent)
//  Note that exact numbers can change (So an enemy with a 1 damage attack can share a script with an enemy that does the same attack but does 3 damage with it)
//      For this reason descriptions must not contain exact numbers
//  Note that multiple scripts can share a description since some enemies might need complicated animation stuff to do the same thing
//  (But it may be a good idea to just give it a different script anyway)

public class BM_Shared_Bite : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Bite;

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
                    case BattleHelper.EntityID.Leafling:
                    case BattleHelper.EntityID.Flowerling:
                    case BattleHelper.EntityID.Rockling:
                        caller.DealDamage(caller.curTarget, 1, BattleHelper.DamageType.Normal, 0, BattleHelper.ContactLevel.Contact);
                        break;
                    case BattleHelper.EntityID.Rootling:
                    case BattleHelper.EntityID.Shrublet:
                    case BattleHelper.EntityID.Honeybud:
                        caller.DealDamage(caller.curTarget, 2, BattleHelper.DamageType.Normal, 0, BattleHelper.ContactLevel.Contact);
                        break;
                    case BattleHelper.EntityID.Lavaswimmer:
                        caller.DealDamage(caller.curTarget, 3, BattleHelper.DamageType.Normal, 0, BattleHelper.ContactLevel.Contact);
                        break;
                    case BattleHelper.EntityID.Sawcrest:
                        caller.DealDamage(caller.curTarget, 5, BattleHelper.DamageType.Normal, 0, BattleHelper.ContactLevel.Contact);
                        break;
                    case BattleHelper.EntityID.Plaguebud:
                        caller.DealDamage(caller.curTarget, 8, BattleHelper.DamageType.Normal, 0, BattleHelper.ContactLevel.Contact);
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

public class BM_Shared_FrontBite : BM_Shared_Bite
{
    public override MoveIndex GetMoveIndex() => MoveIndex.FrontBite;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemyLowFrontmost);
}

public class BM_Shared_SwoopDown : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Swoop;

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

            yield return StartCoroutine(caller.MoveEasing(tposA, (e) => MainManager.EasingOutIn(e)));

            Vector3[] positions = new Vector3[] { tposA, tposmid, tposend };

            if (caller.GetAttackHit(caller.curTarget, 0))
            {
                yield return StartCoroutine(caller.FollowBezierCurve(backFlag ? 0.4f : 0.3f, (float a) => MainManager.EasingQuadratic(a, -0.2f), positions));

                //yield return StartCoroutine(caller.Squish(0.067f, 0.2f));

                caller.curTarget.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.ReverseSquish);
                switch (caller.entityID)
                {
                    case BattleHelper.EntityID.Flowerling:
                        caller.DealDamage(caller.curTarget, 1, BattleHelper.DamageType.Normal, 0, BattleHelper.ContactLevel.Contact);
                        break;
                    case BattleHelper.EntityID.Sunflower:
                        caller.DealDamage(caller.curTarget, 6, BattleHelper.DamageType.Normal, 0, BattleHelper.ContactLevel.Contact);
                        break;
                    default:
                        caller.DealDamage(caller.curTarget, 2, BattleHelper.DamageType.Normal, 0, BattleHelper.ContactLevel.Contact);
                        break;
                }

                //StartCoroutine(caller.RevertScale(0.1f));
            }
            else
            {
                positions[2] = tposend + Vector3.left * 0.5f;
                yield return StartCoroutine(caller.FollowBezierCurve(backFlag ? 0.4f : 0.3f, (float a) => MainManager.EasingQuadratic(a, -0.2f), 1.25f, positions));
                caller.InvokeMissEvents(caller.curTarget);
            }
        }

        yield return StartCoroutine(caller.MoveEasing(caller.homePos, (e) => MainManager.EasingOutIn(e)));
    }
}

public class BM_Shared_BiteThenFly : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.BiteThenFly;

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
                    case BattleHelper.EntityID.Honeybud:
                    case BattleHelper.EntityID.Cactupole:
                        caller.DealDamage(caller.curTarget, 2, BattleHelper.DamageType.Normal, 0, BattleHelper.ContactLevel.Contact);
                        break;
                    case BattleHelper.EntityID.Sunflower:
                        caller.DealDamage(caller.curTarget, 4, BattleHelper.DamageType.Normal, 0, BattleHelper.ContactLevel.Contact);
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

        //fly back up        
        yield return StartCoroutine(caller.FlyingFlyBackUp());
    }
}

public class BM_Shared_Slash : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Slash;

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

            yield return StartCoroutine(caller.SpinHeavy(Vector3.up * 360, 0.25f));
            if (caller.GetAttackHit(caller.curTarget, 0))
            {
                caller.curTarget.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.Spin);
                switch (caller.entityID)
                {
                    case BattleHelper.EntityID.Renegade:
                        caller.DealDamage(caller.curTarget, 3, BattleHelper.DamageType.Normal, 0, BattleHelper.ContactLevel.Weapon);
                        break;
                    case BattleHelper.EntityID.Slimewalker:
                        caller.DealDamage(caller.curTarget, 3, BattleHelper.DamageType.Normal, 0, BattleHelper.ContactLevel.Weapon);
                        break;
                    case BattleHelper.EntityID.LumistarSoldier:
                        caller.DealDamage(caller.curTarget, 5, BattleHelper.DamageType.Normal, 0, BattleHelper.ContactLevel.Weapon);
                        break;
                    default:
                        caller.DealDamage(caller.curTarget, 2, BattleHelper.DamageType.Normal, 0, BattleHelper.ContactLevel.Weapon);
                        break;
                }
            }
            else
            {
                caller.InvokeMissEvents(caller.curTarget);
            }
        }

        yield return StartCoroutine(caller.MoveEasing(caller.homePos, (e) => MainManager.EasingOutIn(e)));
    }
}

public class BM_Shared_DualSlash : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.DualSlash;

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

            yield return StartCoroutine(caller.SpinHeavy(Vector3.up * 360, 0.25f));
            if (caller.GetAttackHit(caller.curTarget, 0))
            {
                caller.curTarget.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.Spin);
                switch (caller.entityID)
                {
                    case BattleHelper.EntityID.Renegade:
                        caller.DealDamage(caller.curTarget, 2, BattleHelper.DamageType.Normal, 0, BattleHelper.ContactLevel.Weapon);
                        break;
                    case BattleHelper.EntityID.Embercrest:
                        caller.DealDamage(caller.curTarget, 3, BattleHelper.DamageType.Normal, 0, BattleHelper.ContactLevel.Weapon);
                        break;
                    case BattleHelper.EntityID.LumistarStriker:
                        caller.DealDamage(caller.curTarget, 4, BattleHelper.DamageType.Normal, 0, BattleHelper.ContactLevel.Weapon);
                        break;
                    default:
                        caller.DealDamage(caller.curTarget, 2, BattleHelper.DamageType.Normal, 0, BattleHelper.ContactLevel.Weapon);
                        break;
                }
                yield return StartCoroutine(caller.SpinHeavy(Vector3.up * 360, 0.25f));
                caller.curTarget.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.Spin);
                switch (caller.entityID)
                {
                    case BattleHelper.EntityID.Renegade:
                        caller.DealDamage(caller.curTarget, 2, BattleHelper.DamageType.Normal, 0, BattleHelper.ContactLevel.Weapon);
                        break;
                    case BattleHelper.EntityID.Embercrest:
                        caller.DealDamage(caller.curTarget, 3, BattleHelper.DamageType.Normal, 0, BattleHelper.ContactLevel.Weapon);
                        break;
                    case BattleHelper.EntityID.LumistarStriker:
                        caller.DealDamage(caller.curTarget, 4, BattleHelper.DamageType.Normal, 0, BattleHelper.ContactLevel.Weapon);
                        break;
                    default:
                        caller.DealDamage(caller.curTarget, 2, BattleHelper.DamageType.Normal, 0, BattleHelper.ContactLevel.Weapon);
                        break;
                }
            }
            else
            {
                caller.InvokeMissEvents(caller.curTarget);
            }
        }

        yield return StartCoroutine(caller.MoveEasing(caller.homePos, (e) => MainManager.EasingOutIn(e)));
    }
}

public class BM_Shared_DoubleSwoop : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.DoubleSwoop;

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

            yield return StartCoroutine(caller.MoveEasing(tposA, (e) => MainManager.EasingOutIn(e)));

            Vector3[] positions = new Vector3[] { tposA, tposmid, tposend };
            Vector3[] invpositions = new Vector3[] { tposend, tposmid, tposA };

            if (caller.GetAttackHit(caller.curTarget, 0))
            {
                for (int k = 0; k < 2; k++)
                {
                    yield return StartCoroutine(caller.FollowBezierCurve(backFlag ? 0.4f : 0.3f, (float a) => MainManager.EasingQuadratic(a, -0.2f), positions));

                    //yield return StartCoroutine(caller.Squish(0.067f, 0.2f));

                    caller.curTarget.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.ReverseSquish);
                    switch (caller.entityID)
                    {
                        case BattleHelper.EntityID.Heatwing:
                            caller.DealDamage(caller.curTarget, 3, BattleHelper.DamageType.Normal, 0, BattleHelper.ContactLevel.Contact);
                            break;
                        default:
                            caller.DealDamage(caller.curTarget, 2, BattleHelper.DamageType.Normal, 0, BattleHelper.ContactLevel.Contact);
                            break;
                    }

                    yield return StartCoroutine(caller.FollowBezierCurve(backFlag ? 0.25f : 0.15f, (float a) => MainManager.EasingQuadratic(a, 0.2f), invpositions));
                    yield return null;
                }
            }
            else
            {
                positions[2] = tposend + Vector3.left * 0.5f;
                yield return StartCoroutine(caller.FollowBezierCurve(backFlag ? 0.4f : 0.3f, (float a) => MainManager.EasingQuadratic(a, -0.2f), 1.25f, positions));
                caller.InvokeMissEvents(caller.curTarget);
            }
        }

        yield return StartCoroutine(caller.MoveEasing(caller.homePos, (e) => MainManager.EasingOutIn(e)));
    }
}

public class BM_Shared_TripleSwoop : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.TripleSwoop;

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

            yield return StartCoroutine(caller.MoveEasing(tposA, (e) => MainManager.EasingOutIn(e)));

            Vector3[] positions = new Vector3[] { tposA, tposmid, tposend };
            Vector3[] invpositions = new Vector3[] { tposend, tposmid, tposA };

            if (caller.GetAttackHit(caller.curTarget, 0))
            {
                for (int k = 0; k < 3; k++)
                {
                    yield return StartCoroutine(caller.FollowBezierCurve(backFlag ? 0.4f : 0.3f, (float a) => MainManager.EasingQuadratic(a, -0.2f), positions));

                    //yield return StartCoroutine(caller.Squish(0.067f, 0.2f));

                    caller.curTarget.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.ReverseSquish);
                    switch (caller.entityID)
                    {
                        case BattleHelper.EntityID.HoarderFly:
                            caller.DealDamage(caller.curTarget, 4, BattleHelper.DamageType.Normal, 0, BattleHelper.ContactLevel.Contact);
                            break;
                        default:
                            caller.DealDamage(caller.curTarget, 2, BattleHelper.DamageType.Normal, 0, BattleHelper.ContactLevel.Contact);
                            break;
                    }

                    yield return StartCoroutine(caller.FollowBezierCurve(backFlag ? 0.4f : 0.3f, (float a) => MainManager.EasingQuadratic(a, 0.2f), invpositions));
                }
            }
            else
            {
                positions[2] = tposend + Vector3.left * 0.5f;
                yield return StartCoroutine(caller.FollowBezierCurve(backFlag ? 0.4f : 0.3f, (float a) => MainManager.EasingQuadratic(a, -0.2f), 1.25f, positions));
                caller.InvokeMissEvents(caller.curTarget);
            }
        }

        yield return StartCoroutine(caller.MoveEasing(caller.homePos, (e) => MainManager.EasingOutIn(e)));
    }
}

public class BM_Shared_Hard_CounterEnrage : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Hard_CounterEnrage;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.Self);

    public override IEnumerator ExecuteOutOfTurn(BattleEntity caller, BattleEntity target, int level = 1)
    {
        caller.InflictEffect(caller, new Effect(Effect.EffectType.Focus, 1, Effect.INFINITE_DURATION));
        yield return new WaitForSeconds(0.5f);
    }
}

public class BM_Shared_Hard_CounterRush : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Hard_CounterRush;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemy);

    public override IEnumerator ExecuteOutOfTurn(BattleEntity caller, BattleEntity target, int level = 1)
    {
        caller.curTarget = target;
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
                    case BattleHelper.EntityID.Honeybud:
                        caller.DealDamage(caller.curTarget, 1, BattleHelper.DamageType.Normal, 0, BattleHelper.ContactLevel.Contact);
                        break;
                    case BattleHelper.EntityID.Cactupole:
                        caller.DealDamage(caller.curTarget, 2, BattleHelper.DamageType.Normal, 0, BattleHelper.ContactLevel.Contact);
                        break;
                    case BattleHelper.EntityID.Sunnybud:
                        caller.DealDamage(caller.curTarget, 4, BattleHelper.DamageType.Normal, 0, BattleHelper.ContactLevel.Contact);
                        break;
                    case BattleHelper.EntityID.MiracleBloom:
                        caller.DealDamage(caller.curTarget, 8, BattleHelper.DamageType.Normal, 0, BattleHelper.ContactLevel.Contact);
                        break;
                    default:
                        caller.DealDamage(caller.curTarget, 3, BattleHelper.DamageType.Normal, 0, BattleHelper.ContactLevel.Contact);
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

public class BM_Shared_Hard_CounterHarden : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Hard_CounterHarden;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.Self);

    public override IEnumerator ExecuteOutOfTurn(BattleEntity caller, BattleEntity target, int level = 1)
    {
        switch (caller.entityID)
        {
            case BattleHelper.EntityID.VineThrone:
                caller.InflictEffectBuffered(caller, new Effect(Effect.EffectType.Absorb, 2, Effect.INFINITE_DURATION));
                break;
            default:
                caller.InflictEffectBuffered(caller, new Effect(Effect.EffectType.Absorb, 1, Effect.INFINITE_DURATION));
                break;
        }
        yield return new WaitForSeconds(0.5f);
    }
}

public class BM_Shared_Hard_CounterRecover : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Hard_CounterRecover;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.Self);

    public override IEnumerator ExecuteOutOfTurn(BattleEntity caller, BattleEntity target, int level = 1)
    {
        caller.HealHealth(2);
        yield return new WaitForSeconds(0.5f);
    }
}

public class BM_Shared_Hard_CounterReinforce : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Hard_CounterReinforce;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.Self);

    public override IEnumerator ExecuteOutOfTurn(BattleEntity caller, BattleEntity target, int level = 1)
    {
        caller.InflictEffect(caller, new Effect(Effect.EffectType.Focus, 2, Effect.INFINITE_DURATION));
        caller.InflictEffectBuffered(caller, new Effect(Effect.EffectType.Absorb, 2, Effect.INFINITE_DURATION));
        yield return new WaitForSeconds(0.5f);
    }
}

public class BM_Shared_Hard_CounterRally : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Hard_CounterRally;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveAlly);

    public override IEnumerator ExecuteOutOfTurn(BattleEntity caller, BattleEntity target, int level = 1)
    {
        List<BattleEntity> entityList = BattleControl.Instance.GetEntitiesSorted(caller, GetBaseTarget());
        foreach (BattleEntity entity in entityList)
        {
            caller.InflictEffect(entity, new Effect(Effect.EffectType.Focus, 1, Effect.INFINITE_DURATION));
        }
        yield return new WaitForSeconds(0.5f);
    }
}

public class BM_Shared_Hard_CounterRoar : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Hard_CounterRoar;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemy, true);

    public override IEnumerator ExecuteOutOfTurn(BattleEntity caller, BattleEntity target, int level = 1)
    {
        if (!BattleControl.Instance.EntityValid(caller.curTarget))
        {
            caller.curTarget = null;
        }

        List<BattleEntity> battleEntityList = BattleControl.Instance.GetEntitiesSorted(caller, GetBaseTarget());

        yield return new WaitForSeconds(0.5f);
        foreach (BattleEntity entity in battleEntityList)
        {
            if (caller.GetAttackHit(entity, BattleHelper.DamageType.Normal, 0))
            {
                caller.DealDamage(entity, 0, BattleHelper.DamageType.Normal, 0, BattleHelper.ContactLevel.Infinite);
                caller.InflictEffectBuffered(entity, new Effect(Effect.EffectType.Sunder, 1, Effect.INFINITE_DURATION));
            } else
            {
                caller.InvokeMissEvents(entity);
            }
        }
    }
}

public class BM_Shared_Hard_CounterHide : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Hard_CounterHide;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.Self);

    public override IEnumerator ExecuteOutOfTurn(BattleEntity caller, BattleEntity target, int level = 1)
    {
        //note: It is possible to take damage while ethereal, which will extend the effect which is bad
        //Possible if you use light element attacks, sticky spore
        if (!caller.HasEffect(Effect.EffectType.Ethereal))
        {
            switch (caller.entityID)
            {
                case BattleHelper.EntityID.Shrouder:
                    caller.InflictEffect(caller, new Effect(Effect.EffectType.Ethereal, 1, 2));
                    break;
                case BattleHelper.EntityID.Speartongue:
                    caller.InflictEffect(caller, new Effect(Effect.EffectType.Ethereal, 1, 2));
                    break;
                default:
                    caller.InflictEffect(caller, new Effect(Effect.EffectType.Ethereal, 1, 2));
                    break;
            }
        }
        yield return new WaitForSeconds(0.5f);
    }
}

public class BM_Shared_Hard_CounterProtect : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Hard_CounterProtect;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveAlly);

    public override IEnumerator ExecuteOutOfTurn(BattleEntity caller, BattleEntity target, int level = 1)
    {
        List<BattleEntity> entityList = BattleControl.Instance.GetEntitiesSorted(caller, GetBaseTarget());
        foreach (BattleEntity entity in entityList)
        {
            caller.InflictEffect(entity, new Effect(Effect.EffectType.Absorb, 1, Effect.INFINITE_DURATION));
        }
        yield return new WaitForSeconds(0.5f);
    }
}

public class BM_Shared_Hard_CounterShield : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Hard_CounterShield;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveAlly);

    public override IEnumerator ExecuteOutOfTurn(BattleEntity caller, BattleEntity target, int level = 1)
    {
        List<BattleEntity> entityList = BattleControl.Instance.GetEntitiesSorted(caller, GetBaseTarget());
        foreach (BattleEntity entity in entityList)
        {
            caller.InflictEffect(entity, new Effect(Effect.EffectType.DefenseUp, 1, 2));
        }
        yield return new WaitForSeconds(0.5f);
    }
}

public class BM_Shared_Hard_CounterMarshall : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Hard_CounterMarshall;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveAlly);

    public override IEnumerator ExecuteOutOfTurn(BattleEntity caller, BattleEntity target, int level = 1)
    {
        List<BattleEntity> entityList = BattleControl.Instance.GetEntitiesSorted(caller, GetBaseTarget());
        foreach (BattleEntity entity in entityList)
        {
            caller.InflictEffect(entity, new Effect(Effect.EffectType.AttackUp, 1, 2));
        }
        yield return new WaitForSeconds(0.5f);
    }
}