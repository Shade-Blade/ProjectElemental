using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BE_PuffJelly : BattleEntity
{
    public override void Initialize()
    {
        moveset = new List<Move> { gameObject.AddComponent<BM_PuffJelly_Slam>(), gameObject.AddComponent<BM_PuffJelly_Hard_BlinkSlam>() };

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
            } else
            {
                currMove = moveset[0];
            }
        }

        BasicOffsetTargetChooser(2, 3);
    }

    public override IEnumerator DoEvent(BattleHelper.Event eventID)
    {
        yield return StartCoroutine(DefaultEventHandler_Flying(eventID));
    }
}

public class BM_PuffJelly_Slam : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.PuffJelly_Slam;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemy);

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        //fly back up        
        yield return StartCoroutine(caller.FlyingFlyBackUp());

        if (!BattleControl.Instance.EntityValid(caller.curTarget))
        {
            caller.curTarget = null;
        }

        if (caller.curTarget != null)
        {
            Vector3 abovePos = caller.curTarget.transform.position + Vector3.up * 2;
            Vector3 tpos = caller.curTarget.ApplyScaledOffset(caller.curTarget.stompOffset);

            yield return StartCoroutine(caller.MoveEasing(abovePos, (e) => MainManager.EasingOutIn(e)));

            Vector3 spos = transform.position;

            yield return StartCoroutine(caller.JumpHeavy(tpos, 2, 0.5f, -0.25f));

            if (caller.GetAttackHit(caller.curTarget, 0))
            {
                caller.curTarget.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.Squish);
                caller.DealDamage(caller.curTarget, 1, 0, 0, BattleHelper.ContactLevel.Contact);
                yield return StartCoroutine(caller.MoveEasing(caller.homePos, (e) => MainManager.EasingOutIn(e)));
            }
            else
            {
                caller.InvokeMissEvents(caller.curTarget);

                //extrapolate the move curve
                yield return StartCoroutine(caller.ExtrapolateJumpHeavy(spos, tpos, 2, 0.5f, -0.25f));
                yield return StartCoroutine(caller.MoveEasing(caller.homePos, (e) => MainManager.EasingOutIn(e)));
            }
        }
    }
}

public class BM_PuffJelly_Hard_BlinkSlam : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.PuffJelly_Hard_BlinkSlam;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemy);

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        //fly back up        
        yield return StartCoroutine(caller.FlyingFlyBackUp());

        if (!BattleControl.Instance.EntityValid(caller.curTarget))
        {
            caller.curTarget = null;
        }

        if (caller.curTarget != null)
        {
            Vector3 abovePos = caller.curTarget.transform.position + Vector3.up * 2;
            Vector3 tpos = caller.curTarget.ApplyScaledOffset(caller.curTarget.stompOffset);

            yield return StartCoroutine(caller.MoveEasing(abovePos, (e) => MainManager.EasingOutIn(e)));

            Vector3 spos = transform.position;

            yield return StartCoroutine(caller.JumpHeavy(tpos, 3, 0.5f, -0.25f));

            if (caller.GetAttackHit(caller.curTarget, BattleHelper.DamageType.Light))
            {
                caller.curTarget.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.Squish);
                caller.DealDamage(caller.curTarget, 1, BattleHelper.DamageType.Light, 0, BattleHelper.ContactLevel.Contact);
                yield return StartCoroutine(caller.MoveEasing(caller.homePos, (e) => MainManager.EasingOutIn(e)));
            }
            else
            {
                caller.InvokeMissEvents(caller.curTarget);

                //extrapolate the move curve
                yield return StartCoroutine(caller.ExtrapolateJumpHeavy(spos, tpos, 2, 0.5f, -0.25f));
                yield return StartCoroutine(caller.MoveEasing(caller.homePos, (e) => MainManager.EasingOutIn(e)));
            }
        }
    }
}

public class BE_Fluffling : BattleEntity
{
    public override void Initialize()
    {
        moveset = new List<Move> { gameObject.AddComponent<BM_Fluffling_Bash>(), gameObject.AddComponent<BM_Fluffling_Hard_WaterTorpedo>() };

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

        BasicOffsetTargetChooser(2, 3);
    }

    public override IEnumerator DoEvent(BattleHelper.Event eventID)
    {
        yield return StartCoroutine(DefaultEventHandler_Flying(eventID));
    }
}

public class BM_Fluffling_Bash : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Fluffling_Bash;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemy);

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        if (!BattleControl.Instance.EntityValid(caller.curTarget))
        {
            caller.curTarget = null;
        }

        if (caller.curTarget != null)
        {
            Vector3 tpos = caller.curTarget.ApplyScaledOffset(caller.curTarget.kickOffset);
            Vector3 spos = transform.position;

            float delta = Random.Range(0f, 1f);
            yield return StartCoroutine(caller.JumpHeavy(tpos, 1f + delta, 0.5f + delta/2, -0.25f));

            if (caller.GetAttackHit(caller.curTarget, 0))
            {
                caller.curTarget.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.ReverseSquish);
                caller.DealDamage(caller.curTarget, 1, 0, 0, BattleHelper.ContactLevel.Contact);
                yield return StartCoroutine(caller.MoveEasing(caller.homePos, (e) => MainManager.EasingOutIn(e)));
            }
            else
            {
                caller.InvokeMissEvents(caller.curTarget);

                //extrapolate the move curve
                yield return StartCoroutine(caller.ExtrapolateJumpHeavy(spos, tpos, 2, 0.5f, -0.25f));
                yield return StartCoroutine(caller.MoveEasing(caller.homePos, (e) => MainManager.EasingOutIn(e)));
            }
        }
    }
}

public class BM_Fluffling_Hard_WaterTorpedo : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Fluffling_Hard_WaterTorpedo;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemy);

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        if (!BattleControl.Instance.EntityValid(caller.curTarget))
        {
            caller.curTarget = null;
        }

        if (caller.curTarget != null)
        {
            Vector3 tpos = caller.curTarget.ApplyScaledOffset(caller.curTarget.kickOffset);
            Vector3 spos = transform.position;

            float delta = Random.Range(0f, 1f);
            yield return StartCoroutine(caller.JumpHeavy(tpos, 1f + delta, 0.5f + delta / 2, -0.25f));

            if (caller.GetAttackHit(caller.curTarget, BattleHelper.DamageType.Water))
            {
                caller.curTarget.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.ReverseSquish);
                caller.DealDamage(caller.curTarget, 1, BattleHelper.DamageType.Water, 0, BattleHelper.ContactLevel.Contact);
                yield return StartCoroutine(caller.MoveEasing(caller.homePos, (e) => MainManager.EasingOutIn(e)));
            }
            else
            {
                caller.InvokeMissEvents(caller.curTarget);

                //extrapolate the move curve
                yield return StartCoroutine(caller.ExtrapolateJumpHeavy(spos, tpos, 2, 0.5f, -0.25f));
                yield return StartCoroutine(caller.MoveEasing(caller.homePos, (e) => MainManager.EasingOutIn(e)));
            }
        }
    }
}

public class BE_CloudJelly : BattleEntity
{
    public enum CloudJellyForm
    {
        Ice,
        Water,
        Cloud
    }
    public CloudJellyForm form;

    int counterCount;

    public override void Initialize()
    {
        moveset = new List<Move> { gameObject.AddComponent<BM_CloudJelly_IceSwing>(), gameObject.AddComponent<BM_CloudJelly_FrostFortify>(), gameObject.AddComponent<BM_CloudJelly_BubbleToss>(), gameObject.AddComponent<BM_CloudJelly_BubbleBlast>(), gameObject.AddComponent<BM_CloudJelly_PowerBolt>(), gameObject.AddComponent<BM_CloudJelly_PowerCharge>(), gameObject.AddComponent<BM_CloudJelly_CounterFormChange>() };

        base.Initialize();
    }
    public void SetForm(CloudJellyForm cjf)
    {
        //delete old one
        AnimationController oac = GetComponentInChildren<AnimationController>();
        Destroy(oac.gameObject);

        GameObject ac = null;
        form = cjf;
        switch (form)
        {
            case CloudJellyForm.Ice:
                //s.color = new Color(0.4f, 1, 1);
                ac = AnimationControllerSetup(this, subObject, MainManager.SpriteID.C8_IceJelly);
                ResetDefenseTable();
                SetDefense(BattleHelper.DamageType.Default, 6);
                SetDefense(BattleHelper.DamageType.Light, DefenseTableEntry.IMMUNITY_CONSTANT);
                SetDefense(BattleHelper.DamageType.Dark, -2);
                break;
            case CloudJellyForm.Water:
                //s.color = new Color(0.2f, 0.5f, 0.7f);
                ac = AnimationControllerSetup(this, subObject, MainManager.SpriteID.C8_WaterJelly);
                ResetDefenseTable();
                SetDefense(BattleHelper.DamageType.Default, 3);
                SetDefense(BattleHelper.DamageType.Water, DefenseTableEntry.IMMUNITY_CONSTANT);
                SetDefense(BattleHelper.DamageType.Fire, -4);
                break;
            case CloudJellyForm.Cloud:
                //s.color = new Color(0.8f, 0.9f, 1);
                ac = AnimationControllerSetup(this, subObject, MainManager.SpriteID.C8_CloudJelly);
                ResetDefenseTable();
                SetDefense(BattleHelper.DamageType.Air, DefenseTableEntry.IMMUNITY_CONSTANT);
                SetDefense(BattleHelper.DamageType.Earth, -6);
                break;
        }
        ac.transform.localPosition = offset;// + Vector3.up * (height / 2);
        this.ac = ac.GetComponent<AnimationController>();
        EffectSpriteUpdate();
        if (flipDefault && this.ac != null)
        {
            this.ac.SendAnimationData("xflip");
        }
    }

    public static GameObject AnimationControllerSetup(BattleEntity be, GameObject sub, MainManager.SpriteID sid)
    {
        AnimationController ac = MainManager.CreateAnimationController(sid, sub);
        be.ac = ac;
        ac.gameObject.transform.parent = sub.transform;
        return ac.gameObject;
    }



    public override void SetEncounterVariables(string variable)
    {
        if (variable != null && variable.Contains("cloud"))
        {
            form = CloudJellyForm.Cloud;
        }
        if (variable != null && variable.Contains("water"))
        {
            form = CloudJellyForm.Water;
        }
        if (variable != null && variable.Contains("ice"))
        {
            form = CloudJellyForm.Ice;
        }
        SetForm(form);
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
            switch (form)
            {
                case CloudJellyForm.Ice:
                    currMove = moveset[(posId + BattleControl.Instance.turnCount - 1) % 2];
                    break;
                case CloudJellyForm.Water:
                    currMove = moveset[2 + ((posId + BattleControl.Instance.turnCount - 1) % 2)];
                    break;
                case CloudJellyForm.Cloud:
                    currMove = moveset[4 + ((posId + BattleControl.Instance.turnCount - 1) % 2)];
                    break;
            }
        }

        BasicOffsetTargetChooser(2, 3);
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
        /*
        if (BattleControl.Instance.GetCurseLevel() <= 0)
        {
            return false;
        }
        */

        if (e == BattleHelper.Event.Hurt && target == this && counterCount <= 0)
        {
            bool formChange = true;
            if ((((lastDamageType & BattleHelper.DamageType.Light) != 0) || ((lastDamageType & BattleHelper.DamageType.Water) != 0) || ((lastDamageType & BattleHelper.DamageType.Air) != 0)))
            {
                if (form == CloudJellyForm.Ice)
                {
                    formChange = false;
                }
            } else
            {
                if (form == CloudJellyForm.Cloud)
                {
                    formChange = false;
                }
            }

            if (!formChange)
            {
                return false;
            }

            counterCount++;
            BattleControl.Instance.AddReactionMoveEvent(this, target.lastAttacker, moveset[6]);
            return true;
        }

        return false;
    }
}

public class BM_CloudJelly_IceSwing : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.CloudJelly_IceSwing;

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
            if (caller.GetAttackHit(caller.curTarget, BattleHelper.DamageType.Light))
            {
                caller.curTarget.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.Spin);
                caller.DealDamage(caller.curTarget, 3, BattleHelper.DamageType.Light, 0, BattleHelper.ContactLevel.Contact);
            }
            else
            {
                caller.InvokeMissEvents(caller.curTarget);
            }
        }

        yield return StartCoroutine(caller.MoveEasing(caller.homePos, (e) => MainManager.EasingOutIn(e)));
    }
}

public class BM_CloudJelly_FrostFortify : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.CloudJelly_FrostFortify;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.Self);

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        yield return StartCoroutine(caller.SpinHeavy(Vector3.up * 360, 0.25f));
        caller.InflictEffect(caller, new Effect(Effect.EffectType.Focus, 3, Effect.INFINITE_DURATION));
        caller.InflictEffect(caller, new Effect(Effect.EffectType.Absorb, 3, Effect.INFINITE_DURATION));
        if (BattleControl.Instance.GetCurseLevel() > 0)
        {
            caller.InflictEffect(caller, new Effect(Effect.EffectType.AttackUp, 1, Effect.INFINITE_DURATION), caller.posId, Effect.EffectStackMode.KeepDurAddPot);
        }
    }
}

public class BM_CloudJelly_BubbleToss : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.CloudJelly_BubbleToss;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemy);

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        if (!BattleControl.Instance.EntityValid(caller.curTarget))
        {
            caller.curTarget = null;
        }

        yield return StartCoroutine(caller.SpinHeavy(Vector3.up * 360, 1f));

        if (caller.GetAttackHit(caller.curTarget, BattleHelper.DamageType.Water))
        {
            caller.curTarget.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.Spin);
            caller.DealDamage(caller.curTarget, 5, BattleHelper.DamageType.Water, 0, BattleHelper.ContactLevel.Infinite);
        }
        else
        {
            //Miss
            caller.InvokeMissEvents(caller.curTarget);
        }
    }
}

public class BM_CloudJelly_BubbleBlast : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.CloudJelly_BubbleBlast;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemy);

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        if (!BattleControl.Instance.EntityValid(caller.curTarget))
        {
            caller.curTarget = null;
        }

        yield return StartCoroutine(caller.SpinHeavy(Vector3.up * 360, 1f));

        List<BattleEntity> targets = BattleControl.Instance.GetEntitiesSorted(caller, GetBaseTarget());

        foreach (BattleEntity t in targets)
        {
            if (caller.GetAttackHit(t, BattleHelper.DamageType.Water))
            {
                t.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.Squish);
                caller.DealDamage(t, 3, BattleHelper.DamageType.Water, 0, BattleHelper.ContactLevel.Infinite);
            }
            else
            {
                //Miss
                caller.InvokeMissEvents(t);
            }
        }
        if (BattleControl.Instance.GetCurseLevel() > 0)
        {
            caller.InflictEffect(caller, new Effect(Effect.EffectType.AttackUp, 1, Effect.INFINITE_DURATION), caller.posId, Effect.EffectStackMode.KeepDurAddPot);
        }
    }
}

public class BM_CloudJelly_PowerBolt : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.CloudJelly_PowerBolt;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemy);

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        //fly back up        
        yield return StartCoroutine(caller.FlyingFlyBackUp());

        if (!BattleControl.Instance.EntityValid(caller.curTarget))
        {
            caller.curTarget = null;
        }

        yield return StartCoroutine(caller.SpinHeavy(Vector3.up * 360, 1f));

        if (caller.GetAttackHit(caller.curTarget, BattleHelper.DamageType.Air))
        {
            caller.curTarget.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.Squish);
            caller.DealDamage(caller.curTarget, 8, BattleHelper.DamageType.Air, 0, BattleHelper.ContactLevel.Infinite);
        }
        else
        {
            //Miss
            caller.InvokeMissEvents(caller.curTarget);
        }
    }
}

public class BM_CloudJelly_PowerCharge : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.CloudJelly_PowerCharge;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.Self);

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        //fly back up        
        yield return StartCoroutine(caller.FlyingFlyBackUp());

        yield return StartCoroutine(caller.SpinHeavy(Vector3.up * 360, 0.25f));
        caller.HealHealth(4);
        caller.InflictEffect(caller, new Effect(Effect.EffectType.Focus, 4, Effect.INFINITE_DURATION));
        if (BattleControl.Instance.GetCurseLevel() > 0)
        {
            caller.InflictEffect(caller, new Effect(Effect.EffectType.AttackUp, 3, Effect.INFINITE_DURATION), caller.posId, Effect.EffectStackMode.KeepDurAddPot);
        }
        else
        {
            caller.InflictEffect(caller, new Effect(Effect.EffectType.AttackUp, 2, Effect.INFINITE_DURATION), caller.posId, Effect.EffectStackMode.KeepDurAddPot);
        }
    }
}

public class BM_CloudJelly_CounterFormChange : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.CloudJelly_CounterFormChange;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.Self);

    public override IEnumerator ExecuteOutOfTurn(BattleEntity caller, BattleEntity target, int level = 1)
    {
        yield return StartCoroutine(caller.SpinHeavy(Vector3.up * 360, 0.5f));
        if (caller is BE_CloudJelly cj)
        {
            if (((caller.lastDamageType & BattleHelper.DamageType.Light) != 0) || ((caller.lastDamageType & BattleHelper.DamageType.Water) != 0) || ((caller.lastDamageType & BattleHelper.DamageType.Air) != 0))
            {
                switch (cj.form)
                {
                    case BE_CloudJelly.CloudJellyForm.Ice:
                        cj.SetForm(BE_CloudJelly.CloudJellyForm.Ice);
                        break;
                    case BE_CloudJelly.CloudJellyForm.Water:
                        cj.SetForm(BE_CloudJelly.CloudJellyForm.Ice);
                        break;
                    case BE_CloudJelly.CloudJellyForm.Cloud:
                        cj.SetForm(BE_CloudJelly.CloudJellyForm.Water);
                        break;
                }
            } else
            {
                switch (cj.form)
                {
                    case BE_CloudJelly.CloudJellyForm.Ice:
                        cj.SetForm(BE_CloudJelly.CloudJellyForm.Water);
                        break;
                    case BE_CloudJelly.CloudJellyForm.Water:
                        cj.SetForm(BE_CloudJelly.CloudJellyForm.Cloud);
                        break;
                    case BE_CloudJelly.CloudJellyForm.Cloud:
                        cj.SetForm(BE_CloudJelly.CloudJellyForm.Cloud);
                        break;
                }
            }
        }
    }
}

public class BE_CrystalCrab : BattleEntity
{
    int counterCount;
    public override void Initialize()
    {
        moveset = new List<Move> { gameObject.AddComponent<BM_CrystalCrab_TripleClaw>(), gameObject.AddComponent<BM_CrystalCrab_DarkClaw>(), gameObject.AddComponent<BM_CrystalCrab_Hard_CounterClearClaw>() };

        base.Initialize();

        if (BattleControl.Instance.GetCurseLevel() > 0)
        {
            SetEntityProperty(BattleHelper.EntityProperties.StateCounter, true);
        }
    }

    public override void ChooseMoveInternal()
    {
        //also reset here in case something weird happens
        counterCount = 0;

        currMove = moveset[(posId + BattleControl.Instance.turnCount - 1) % 2];
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

        Effect_ReactionCounter();

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

public class BM_CrystalCrab_TripleClaw : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.CrystalCrab_TripleClaw;

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
                caller.DealDamage(caller.curTarget, 2, BattleHelper.DamageType.Normal, 0, BattleHelper.ContactLevel.Contact);
                yield return StartCoroutine(caller.SpinHeavy(Vector3.up * 360, 0.25f));
                caller.DealDamage(caller.curTarget, 3, BattleHelper.DamageType.Normal, 0, BattleHelper.ContactLevel.Contact);
                yield return StartCoroutine(caller.SpinHeavy(Vector3.up * 360, 0.25f));
                caller.DealDamage(caller.curTarget, 3, BattleHelper.DamageType.Normal, 0, BattleHelper.ContactLevel.Contact);
            }
            else
            {
                caller.InvokeMissEvents(caller.curTarget);
            }
        }

        yield return StartCoroutine(caller.MoveEasing(caller.homePos, (e) => MainManager.EasingOutIn(e)));
    }
}

public class BM_CrystalCrab_DarkClaw : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.CrystalCrab_DarkClaw;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemy);

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        if (!BattleControl.Instance.EntityValid(caller.curTarget))
        {
            caller.curTarget = null;
        }

        yield return StartCoroutine(caller.SpinHeavy(Vector3.up * 360, 1f));

        List<BattleEntity> targets = BattleControl.Instance.GetEntitiesSorted(caller, GetBaseTarget());

        foreach (BattleEntity t in targets)
        {
            if (caller.GetAttackHit(t, BattleHelper.DamageType.Dark))
            {
                bool hasStatus = t.HasStatus();
                caller.DealDamage(t, 3, BattleHelper.DamageType.Dark, 0, BattleHelper.ContactLevel.Contact);
                if (!hasStatus)
                {
                    caller.InflictEffect(t, new Effect(Effect.EffectType.Sleep, 1, 3));
                }
                caller.InflictEffect(t, new Effect(Effect.EffectType.AttackDown, 1, 3));
            }
            else
            {
                //Miss
                caller.InvokeMissEvents(t);
            }
        }
    }
}

public class BM_CrystalCrab_Hard_CounterClearClaw : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.CrystalCrab_Hard_CounterClearClaw;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemy);

    public override IEnumerator ExecuteOutOfTurn(BattleEntity caller, BattleEntity causer, int level = 1)
    {
        caller.curTarget = causer;
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
                caller.DealDamage(caller.curTarget, 5, BattleHelper.DamageType.Normal, 0, BattleHelper.ContactLevel.Contact);
                caller.curTarget.CleanseEffects(false);
                caller.InflictEffect(caller.curTarget, new Effect(Effect.EffectType.Inverted, 1, 3));
            }
            else
            {
                caller.InvokeMissEvents(caller.curTarget);
            }
        }

        yield return StartCoroutine(caller.MoveEasing(caller.homePos, (e) => MainManager.EasingOutIn(e)));
    }
}

public class BE_CrystalSlug : BattleEntity
{
    int counterCount;
    public override void Initialize()
    {
        moveset = new List<Move> { gameObject.AddComponent<BM_CrystalSlug_Slap>(), gameObject.AddComponent<BM_CrystalSlug_ChillingStare>(), gameObject.AddComponent<BM_Shared_Hard_CounterShield>() };

        base.Initialize();
    }

    public override void ChooseMoveInternal()
    {
        //also reset here in case something weird happens
        counterCount = 0;

        currMove = moveset[(posId + BattleControl.Instance.turnCount - 1) % 2];
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

        Effect_ReactionCounter();

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

public class BM_CrystalSlug_Slap : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.CrystalSlug_Slap;

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
                caller.DealDamage(caller.curTarget, 5, BattleHelper.DamageType.Normal, 0, BattleHelper.ContactLevel.Contact);
                bool hasStatus = caller.curTarget.HasStatus();
                if (!hasStatus && BattleControl.Instance.GetCurseLevel() > 0)
                {
                    caller.InflictEffect(caller.curTarget, new Effect(Effect.EffectType.Brittle, 1, 3));
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

public class BM_CrystalSlug_ChillingStare : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.CrystalSlug_ChillingStare;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemy);

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        if (!BattleControl.Instance.EntityValid(caller.curTarget))
        {
            caller.curTarget = null;
        }

        yield return StartCoroutine(caller.SpinHeavy(Vector3.up * 360, 1f));

        List<BattleEntity> targets = BattleControl.Instance.GetEntitiesSorted(caller, GetBaseTarget());

        foreach (BattleEntity t in targets)
        {
            if (caller.GetAttackHit(t, BattleHelper.DamageType.Light))
            {
                bool hasStatus = t.HasStatus();
                caller.DealDamage(t, 3, BattleHelper.DamageType.Light, 0, BattleHelper.ContactLevel.Infinite);
                if (!hasStatus)
                {
                    caller.InflictEffect(t, new Effect(Effect.EffectType.Freeze, 1, 2));
                }
                caller.InflictEffect(t, new Effect(Effect.EffectType.DefenseDown, 1, 3));
            }
            else
            {
                //Miss
                caller.InvokeMissEvents(t);
            }
        }
    }
}

public class BE_CrystalClam : BattleEntity
{
    public override void Initialize()
    {
        moveset = new List<Move> { gameObject.AddComponent<BM_CrystalClam_HealingBreath>(), gameObject.AddComponent<BM_CrystalClam_CleansingBreath>(), gameObject.AddComponent<BM_CrystalClam_Explode>() };

        base.Initialize();
    }

    public override void ChooseMoveInternal()
    {
        //ai
        //heal if possible, or use cleanse
        //(Hard mode: use cleanse sometimes anyway)
        //  Cleanse is bad so I want to throw more curveballs to hard mode players >:)

        //If alone or all crystal clams, use explode
        //(Hard mode: explode on turn 5)

        if (BattleControl.Instance.GetCurseLevel() > 0)
        {
            currMove = moveset[(posId + BattleControl.Instance.turnCount - 1) % 3 == 0 ? 1 : 0];
        }
        else
        {
            currMove = moveset[0];
        }

        int healPossible = 0;

        List<BattleEntity> healTargets = BattleControl.Instance.GetEntitiesSorted(this, new TargetArea(TargetArea.TargetAreaType.LiveAlly));

        for (int i = 0; i < healTargets.Count; i++)
        {
            healPossible += healTargets[i].maxHP - healTargets[i].hp;
        }

        if (healPossible == 0)
        {
            currMove = moveset[1];
        }

        //Explode logic
        bool doExplode = true;
        for (int i = 0; i < healTargets.Count; i++)
        {
            if (healTargets[i].entityID != BattleHelper.EntityID.CrystalClam)
            {
                doExplode = false;
            }
        }
        if (doExplode || (BattleControl.Instance.GetCurseLevel() > 0 && BattleControl.Instance.turnCount > 3))
        {
            currMove = moveset[2];
        }

        BasicTargetChooser();
    }
}

public class BM_CrystalClam_HealingBreath : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.CrystalClam_HealingBreath;
    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveAlly);

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        yield return DoHeal(caller);
    }

    public IEnumerator DoHeal(BattleEntity caller)
    {
        int multiHealAmount = 3;

        List<BattleEntity> healTargets = BattleControl.Instance.GetEntitiesSorted(caller, new TargetArea(TargetArea.TargetAreaType.LiveAlly));

        caller.SetAnimation("mouthopen");
        yield return new WaitForSeconds(0.25f);
        for (int i = 0; i < healTargets.Count; i++)
        {
            healTargets[i].CureEffects(false);
            healTargets[i].HealHealth(multiHealAmount);
        }
        caller.SetAnimation("mouthclose");
        yield return new WaitForSeconds(0.25f);
    }
}

public class BM_CrystalClam_CleansingBreath : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.CrystalClam_CleansingBreath;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemy, true);

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        if (!BattleControl.Instance.EntityValid(caller.curTarget))
        {
            caller.curTarget = null;
        }

        caller.SetAnimation("mouthopen");
        yield return new WaitForSeconds(0.25f);
        yield return StartCoroutine(caller.SpinHeavy(Vector3.up * 360, 1f));

        List<BattleEntity> targets = BattleControl.Instance.GetEntitiesSorted(caller, GetBaseTarget());

        foreach (BattleEntity t in targets)
        {
            if (caller.GetAttackHit(t, BattleHelper.DamageType.Light))
            {
                t.CleanseEffects(false);
                caller.DealDamage(t, 0, BattleHelper.DamageType.Normal, 0, BattleHelper.ContactLevel.Infinite);
                caller.InflictEffect(t, new Effect(Effect.EffectType.Defocus, 1, Effect.INFINITE_DURATION));
            }
            else
            {
                //Miss
                caller.InvokeMissEvents(t);
            }
        }
        caller.SetAnimation("mouthclose");
        yield return new WaitForSeconds(0.25f);
    }
}

public class BM_CrystalClam_Explode : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.CrystalClam_Explode;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemy, true);

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        if (!BattleControl.Instance.EntityValid(caller.curTarget))
        {
            caller.curTarget = null;
        }

        caller.SetAnimation("mouthopen");
        yield return new WaitForSeconds(0.25f);
        yield return StartCoroutine(caller.SpinHeavy(Vector3.up * 360, 1f));

        List<BattleEntity> targets = BattleControl.Instance.GetEntitiesSorted(caller, GetBaseTarget());

        foreach (BattleEntity t in targets)
        {
            if (caller.GetAttackHit(t, BattleHelper.DamageType.Normal))
            {
                caller.DealDamage(t, 12, BattleHelper.DamageType.Normal, 0, BattleHelper.ContactLevel.Infinite);
            }
            else
            {
                //Miss
                caller.InvokeMissEvents(t);
            }
        }

        //die
        caller.SetAnimation("dead");
        yield return StartCoroutine(caller.DefaultDeathEvent());
        yield break;
    }
}

public class BE_AuroraWing : BattleEntity
{
    public override void Initialize()
    {
        moveset = new List<Move> { gameObject.AddComponent<BM_Aurorawing_RubyDust>(), gameObject.AddComponent<BM_Aurorawing_SapphireDust>(), gameObject.AddComponent<BM_Aurorawing_EmeraldDust>() };

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
            currMove = moveset[(posId + BattleControl.Instance.turnCount - 1) % 3];
        }

        BasicTargetChooser();
    }

    public override IEnumerator DoEvent(BattleHelper.Event eventID)
    {
        yield return StartCoroutine(DefaultEventHandler_Flying(eventID));
    }
}

public class BM_Aurorawing_RubyDust : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.AuroraWing_RubyDust;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemy);

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        //fly back up        
        yield return StartCoroutine(caller.FlyingFlyBackUp());

        if (!BattleControl.Instance.EntityValid(caller.curTarget))
        {
            caller.curTarget = null;
        }

        yield return StartCoroutine(caller.SpinHeavy(Vector3.up * 360, 1f));

        List<BattleEntity> targets = BattleControl.Instance.GetEntitiesSorted(caller, GetBaseTarget());

        foreach (BattleEntity t in targets)
        {
            if (caller.GetAttackHit(t, BattleHelper.DamageType.Fire))
            {
                t.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.Spin);
                caller.DealDamage(t, 3, BattleHelper.DamageType.Fire, 0, BattleHelper.ContactLevel.Infinite);
            }
            else
            {
                //Miss
                caller.InvokeMissEvents(t);
            }
        }

        //ally boost effect
        List<BattleEntity> allyTargets = BattleControl.Instance.GetEntitiesSorted(caller, new TargetArea(TargetArea.TargetAreaType.LiveAlly));
        foreach (BattleEntity a in allyTargets)
        {
            caller.InflictEffect(a, new Effect(Effect.EffectType.AttackUp, 2, 3));
            if (BattleControl.Instance.GetCurseLevel() > 0)
            {
                caller.InflictEffect(a, new Effect(Effect.EffectType.Focus, 2, Effect.INFINITE_DURATION));
            }
        }
    }
}

public class BM_Aurorawing_SapphireDust : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.AuroraWing_SapphireDust;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemy);

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        //fly back up        
        yield return StartCoroutine(caller.FlyingFlyBackUp());

        if (!BattleControl.Instance.EntityValid(caller.curTarget))
        {
            caller.curTarget = null;
        }

        yield return StartCoroutine(caller.SpinHeavy(Vector3.up * 360, 1f));

        List<BattleEntity> targets = BattleControl.Instance.GetEntitiesSorted(caller, GetBaseTarget());

        foreach (BattleEntity t in targets)
        {
            if (caller.GetAttackHit(t, BattleHelper.DamageType.Water))
            {
                t.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.Spin);
                caller.DealDamage(t, 2, BattleHelper.DamageType.Water, 0, BattleHelper.ContactLevel.Infinite);
            }
            else
            {
                //Miss
                caller.InvokeMissEvents(t);
            }
        }

        //ally boost effect
        List<BattleEntity> allyTargets = BattleControl.Instance.GetEntitiesSorted(caller, new TargetArea(TargetArea.TargetAreaType.LiveAlly));
        foreach (BattleEntity a in allyTargets)
        {
            caller.InflictEffect(a, new Effect(Effect.EffectType.DefenseUp, 2, 3));
            if (BattleControl.Instance.GetCurseLevel() > 0)
            {
                caller.InflictEffect(a, new Effect(Effect.EffectType.Absorb, 2, Effect.INFINITE_DURATION));
            }
        }
    }
}

public class BM_Aurorawing_EmeraldDust : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.AuroraWing_EmeraldDust;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemy);

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        //fly back up        
        yield return StartCoroutine(caller.FlyingFlyBackUp());

        if (!BattleControl.Instance.EntityValid(caller.curTarget))
        {
            caller.curTarget = null;
        }

        yield return StartCoroutine(caller.SpinHeavy(Vector3.up * 360, 1f));

        List<BattleEntity> targets = BattleControl.Instance.GetEntitiesSorted(caller, GetBaseTarget());

        foreach (BattleEntity t in targets)
        {
            if (caller.GetAttackHit(t, BattleHelper.DamageType.Earth))
            {
                t.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.Spin);
                caller.DealDamage(t, 2, BattleHelper.DamageType.Earth, 0, BattleHelper.ContactLevel.Infinite);
            }
            else
            {
                //Miss
                caller.InvokeMissEvents(t);
            }
        }

        //ally boost effect
        List<BattleEntity> allyTargets = BattleControl.Instance.GetEntitiesSorted(caller, new TargetArea(TargetArea.TargetAreaType.LiveAlly));
        foreach (BattleEntity a in allyTargets)
        {
            a.HealHealth(5);
            if (BattleControl.Instance.GetCurseLevel() > 0)
            {
                caller.InflictEffect(a, new Effect(Effect.EffectType.Immunity, 1, 3));
            }
        }
    }
}