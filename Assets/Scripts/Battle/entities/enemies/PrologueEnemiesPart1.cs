using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class BE_Leafling : BattleEntity
{
    public override void Initialize()
    {
        //This is pretty sus
        moveset = new List<Move> { gameObject.AddComponent<BM_Shared_Bite>(), gameObject.AddComponent<BM_Leafling_TailWhip>() };
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
            currMove = moveset[(posId + BattleControl.Instance.turnCount - 1) % moveset.Count];
        }

        BasicTargetChooser();
    }
}

public class BM_Leafling_TailWhip : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Leafling_TailWhip;

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

            if (BattleControl.Instance.GetCurseLevel() > 0)
            {
                yield return StartCoroutine(caller.SpinHeavy(Vector3.up * 360, 0.25f));
                if (caller.GetAttackHit(caller.curTarget, BattleHelper.DamageType.Earth))
                {
                    caller.curTarget.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.Spin);
                    caller.DealDamage(caller.curTarget, 1, BattleHelper.DamageType.Earth, 0, BattleHelper.ContactLevel.Contact);
                }
                else
                {
                    caller.InvokeMissEvents(caller.curTarget);
                }
                yield return StartCoroutine(caller.SpinHeavy(Vector3.up * 360, 0.25f));
                if (caller.GetAttackHit(caller.curTarget, BattleHelper.DamageType.Earth))
                {
                    caller.curTarget.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.Spin);
                    caller.DealDamage(caller.curTarget, 1, BattleHelper.DamageType.Earth, 0, BattleHelper.ContactLevel.Contact);
                }
                else
                {
                    caller.InvokeMissEvents(caller.curTarget);
                }
            }
            else
            {
                yield return StartCoroutine(caller.SpinHeavy(Vector3.up * 360, 0.25f));
                if (caller.GetAttackHit(caller.curTarget, BattleHelper.DamageType.Earth))
                {
                    caller.curTarget.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.Spin);
                    caller.DealDamage(caller.curTarget, 1, BattleHelper.DamageType.Earth, 0, BattleHelper.ContactLevel.Contact);
                }
                else
                {
                    caller.InvokeMissEvents(caller.curTarget);
                }
            }
        }

        yield return StartCoroutine(caller.MoveEasing(caller.homePos, (e) => MainManager.EasingOutIn(e)));
    }
}

public class BE_Flowerling : BattleEntity
{
    //public bool grounded = false;

    public override void Initialize()
    {
        moveset = new List<Move> { gameObject.AddComponent<BM_Shared_SwoopDown>(), gameObject.AddComponent<BM_Shared_Bite>(), gameObject.AddComponent<BM_Flowerling_Hard_SwoopBloom>() };

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
            if (GetEntityProperty(BattleHelper.EntityProperties.Grounded))
            {
                currMove = moveset[1];
            } else
            {
                if (BattleControl.Instance.GetCurseLevel() > 0)
                {
                    currMove = moveset[2];
                }
                else
                {
                    currMove = moveset[0];
                }
            }
        }

        BasicTargetChooser();
    }

    public override IEnumerator DoEvent(BattleHelper.Event eventID)
    {
        yield return StartCoroutine(DefaultEventHandler_Flying(eventID));
    }
}

public class BM_Flowerling_Hard_SwoopBloom : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Flowerling_Hard_SwoopBloom;

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
                yield return StartCoroutine(caller.FollowBezierCurve(backFlag ? 0.25f : 0.15f, (float a) => MainManager.EasingQuadratic(a, -0.2f), positions));

                //yield return StartCoroutine(caller.Squish(0.067f, 0.2f));

                caller.curTarget.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.ReverseSquish);
                switch (caller.entityID)
                {
                    case BattleHelper.EntityID.Flowerling:
                        caller.DealDamage(caller.curTarget, 1, BattleHelper.DamageType.Normal, 0, BattleHelper.ContactLevel.Contact);
                        break;
                    case BattleHelper.EntityID.Sunflower:
                        caller.DealDamage(caller.curTarget, 7, BattleHelper.DamageType.Normal, 0, BattleHelper.ContactLevel.Contact);
                        break;
                }

                //StartCoroutine(caller.RevertScale(0.1f));
            }
            else
            {
                positions[2] = tposend + Vector3.left * 0.5f;
                yield return StartCoroutine(caller.FollowBezierCurve(backFlag ? 0.3f : 0.2f, (float a) => MainManager.EasingQuadratic(a, -0.2f), 1.25f, positions));
                caller.InvokeMissEvents(caller.curTarget);
            }
        }

        yield return StartCoroutine(caller.MoveEasing(caller.homePos, (e) => MainManager.EasingOutIn(e)));

        switch (caller.entityID)
        {
            case BattleHelper.EntityID.Flowerling:
                caller.InflictEffectBuffered(caller, new Effect(Effect.EffectType.Focus, 1, Effect.INFINITE_DURATION));
                break;
            case BattleHelper.EntityID.Sunflower:
                caller.InflictEffectBuffered(caller, new Effect(Effect.EffectType.Focus, 2, Effect.INFINITE_DURATION));
                break;
        }
    }
}

public class BM_Sunflower_Hard_SolarBite : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Sunflower_Hard_SolarBite;

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
                yield return StartCoroutine(caller.FollowBezierCurve(backFlag ? 0.25f : 0.15f, (float a) => MainManager.EasingQuadratic(a, -0.2f), positions));

                //yield return StartCoroutine(caller.Squish(0.067f, 0.2f));

                switch (caller.entityID)
                {
                    case BattleHelper.EntityID.Sunflower:
                        bool hasStatus = caller.curTarget.HasAilment();
                        caller.curTarget.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.ReverseSquish);
                        caller.DealDamage(caller.curTarget, 6, BattleHelper.DamageType.Normal, 0, BattleHelper.ContactLevel.Contact);
                        if (!hasStatus)
                        {
                            caller.InflictEffect(caller.curTarget, new Effect(Effect.EffectType.Sunflame, 1, 3));
                        }
                        break;
                }

                //StartCoroutine(caller.RevertScale(0.1f));
            }
            else
            {
                positions[2] = tposend + Vector3.left * 0.5f;
                yield return StartCoroutine(caller.FollowBezierCurve(backFlag ? 0.3f : 0.2f, (float a) => MainManager.EasingQuadratic(a, -0.2f), 1.25f, positions));
                caller.InvokeMissEvents(caller.curTarget);
            }
        }

        yield return StartCoroutine(caller.MoveEasing(caller.homePos, (e) => MainManager.EasingOutIn(e)));
    }
}

public class BE_Shrublet : BattleEntity
{
    int counterCount = 0;

    public override void Initialize()
    {
        moveset = new List<Move> { gameObject.AddComponent<BM_Shared_Bite>(), gameObject.AddComponent<BM_Shared_Hard_CounterEnrage>() };

        base.Initialize();
    }
    public override void ChooseMoveInternal()
    {
        //also reset here in case something weird happens
        counterCount = 0;
        currMove = moveset[0];

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
                DealDamage(target, 2, BattleHelper.DamageType.Normal, (ulong)BattleHelper.DamageProperties.StandardContactHazard, BattleHelper.ContactLevel.Contact);
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
            BattleControl.Instance.AddReactionMoveEvent(this, target.lastAttacker, moveset[1]);
            return true;
        }

        return false;
    }
}