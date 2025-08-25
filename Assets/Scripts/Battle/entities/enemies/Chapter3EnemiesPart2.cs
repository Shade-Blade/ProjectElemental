using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class BE_Urchiling : BattleEntity
{
    public int chargeState = 0;
    public override void Initialize()
    {
        moveset = new List<Move> { gameObject.AddComponent<BM_Urchiling_Charge>(), gameObject.AddComponent<BM_Urchiling_WaterBlast>(), gameObject.AddComponent<BM_Urchiling_Hard_DeepCharge>(), gameObject.AddComponent<BM_Urchiling_Hard_SpikeStorm>() };

        base.Initialize();
    }

    public override void ChooseMoveInternal()
    {
        switch (chargeState)
        {
            case 2:
                currMove = moveset[1];
                break;
            case 1:
                if (BattleControl.Instance.GetCurseLevel() > 0)
                {
                    if (hp <= (maxHP / 2))
                    {
                        currMove = moveset[2];
                    } else
                    {
                        currMove = moveset[1];
                    }
                } else
                {
                    currMove = moveset[1];
                }
                break;
            default:
                if (BattleControl.Instance.GetCurseLevel() > 0)
                {
                    if ((posId + BattleControl.Instance.turnCount - 1) % 5 < 3)
                    {
                        currMove = moveset[3];
                    } else
                    {
                        currMove = moveset[0];
                    }
                } else
                {
                    currMove = moveset[0];
                }
                break;
        }

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

            //Weapon range contact hazard :)
            if (contact <= BattleHelper.ContactLevel.Weapon)
            {
                DealDamage(target, 4, BattleHelper.DamageType.Water, (ulong)BattleHelper.DamageProperties.StandardContactHazard, BattleHelper.ContactLevel.Weapon);
                target.contactImmunityList.Add(posId);
            }
        }
    }
}

public class BM_Urchiling_Charge : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Urchiling_Charge;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.Self);

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        if (caller is BE_Urchiling u)
        {
            u.chargeState = 1;
        }

        yield return StartCoroutine(caller.SpinHeavy(Vector3.up * 360, 0.25f));
        caller.SetEntityProperty(BattleHelper.EntityProperties.StateCharge);
        caller.InflictEffect(caller, new Effect(Effect.EffectType.AstralWall, (sbyte)Mathf.CeilToInt(caller.maxHP / 2f), 3));
        caller.InflictEffect(caller, new Effect(Effect.EffectType.Focus, 2, Effect.INFINITE_DURATION));
    }
}

public class BM_Urchiling_WaterBlast : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Urchiling_WaterBlast;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemy);

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        caller.SetEntityProperty(BattleHelper.EntityProperties.StateCharge, false);
        if (caller is BE_Urchiling u)
        {
            u.chargeState = 0;
        }

        if (!BattleControl.Instance.EntityValid(caller.curTarget))
        {
            caller.curTarget = null;
        }

        yield return StartCoroutine(caller.SpinHeavy(Vector3.up * 360, 0.5f));

        if (caller.curTarget != null)
        {
            List<BattleEntity> targets = BattleControl.Instance.GetEntitiesSorted(caller, GetBaseTarget());
            foreach (BattleEntity t in targets)
            {
                if (caller.GetAttackHit(t, BattleHelper.DamageType.Water))
                {
                    t.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.Spin);
                    caller.DealDamage(t, 7, BattleHelper.DamageType.Water, 0, BattleHelper.ContactLevel.Infinite);
                }
                else
                {
                    //Miss
                    caller.InvokeMissEvents(t);
                }
            }
        }
    }
}

public class BM_Urchiling_Hard_DeepCharge : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Urchiling_Hard_DeepCharge;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.Self);

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        if (caller is BE_Urchiling u)
        {
            u.chargeState = 2;
        }

        yield return StartCoroutine(caller.SpinHeavy(Vector3.up * 360, 0.25f));
        caller.SetEntityProperty(BattleHelper.EntityProperties.StateCharge);
        caller.InflictEffect(caller, new Effect(Effect.EffectType.Focus, 4, Effect.INFINITE_DURATION));
        caller.InflictEffectCapped(caller, new Effect(Effect.EffectType.AttackBoost, 2, Effect.INFINITE_DURATION), 8);
        caller.InflictEffectCapped(caller, new Effect(Effect.EffectType.DefenseBoost, 2, Effect.INFINITE_DURATION), 8);
    }
}

public class BM_Urchiling_Hard_SpikeStorm : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.Urchiling_Hard_SpikeStorm;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemy);

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        if (!BattleControl.Instance.EntityValid(caller.curTarget))
        {
            caller.curTarget = null;
        }

        yield return StartCoroutine(caller.SpinHeavy(Vector3.up * 360, 0.5f));

        if (caller.curTarget != null)
        {
            List<BattleEntity> targets = BattleControl.Instance.GetEntitiesSorted(caller, GetBaseTarget());
            for (int i = 0; i < 3; i++)
            {
                foreach (BattleEntity t in targets)
                {
                    if (caller.GetAttackHit(t, BattleHelper.DamageType.Normal))
                    {
                        t.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.Spin);
                        caller.DealDamage(t, 3, BattleHelper.DamageType.Normal, 0, BattleHelper.ContactLevel.Infinite);
                    }
                    else
                    {
                        //Miss
                        caller.InvokeMissEvents(t);
                    }
                }
                if (i < 2)
                {
                    yield return StartCoroutine(caller.SpinHeavy(Vector3.up * 360, 0.3f));
                }
            }
        }
    }
}

public class BE_GoldenSlime : BattleEntity
{
    BM_GoldenSlime_CopyStomp copyStomp;
    public override void Initialize()
    {
        //Keep copy stomp separate because moveset[0] will be overwritten by the copy stomp move
        //Copy stomp will come back if moveset[0] is unusable
        copyStomp = gameObject.AddComponent<BM_GoldenSlime_CopyStomp>();
        moveset = new List<Move> { copyStomp, gameObject.AddComponent<BM_GoldenSlime_InversionSplash>(), gameObject.AddComponent<BM_GoldenSlime_GoldenWave>(), gameObject.AddComponent<BM_GoldenSlime_CopyForm>() };

        base.Initialize();
    }

    public override void ChooseMoveInternal()
    {
        currMove = moveset[(posId + BattleControl.Instance.turnCount - 1) % 4];
        BasicTargetChooser();

        //Invalidate the move
        if (moveset[0] != copyStomp && !moveset[0].CanChoose(this))
        {
            moveset[0] = copyStomp;
            currMove = moveset[0];
            BasicTargetChooser();
        }

        if (currMove == moveset[3])
        {
            //double check
            if (curTarget == null)
            {
                currMove = moveset[0];
                BasicTargetChooser();
            }
        }

        if (currMove == moveset[1])
        {
            //double check
            BattleEntity p = BattleControl.Instance.GetEntitiesSorted(this, new TargetArea(TargetArea.TargetAreaType.LiveEnemy))[0];

            int power = p.CountBuffPotency() - p.CountDebuffPotency();

            if (power <= 0)
            {
                currMove = moveset[0];
                BasicTargetChooser();
            }
        }

        if (currMove == copyStomp)
        {
            //Prioritize targets
            //priority 1: grounded allies
            BasicTargetChooser(new TargetArea(TargetArea.TargetAreaType.LiveAllyLowNotSameType));

            //priority 2: airborne allies (lower priority because they use air based moves and golden slime does not have the proper logic
            if (curTarget == null)
            {
                BasicTargetChooser(new TargetArea(TargetArea.TargetAreaType.LiveAllyNotSameType));
            }

            //priority 3: enemies
            if (curTarget == null)
            {
                BasicTargetChooser(new TargetArea(TargetArea.TargetAreaType.LiveEnemy));
            }
        }
    }

    public override IEnumerator PostAction()
    {
        //Something to make using airborne moves not look completely stupid
        if (GetEntityProperty(BattleHelper.EntityProperties.Airborne))
        {
            yield return StartCoroutine(FlyingFallDown());
        }
    }

    public override IEnumerator PostMove()
    {
        //Something to make using airborne moves not look completely stupid
        if (GetEntityProperty(BattleHelper.EntityProperties.Airborne))
        {
            yield return StartCoroutine(FlyingFallDown());
        }

        yield return StartCoroutine(base.PostMove());
    }
}

public class BM_GoldenSlime_CopyStomp : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.GoldenSlime_CopyStomp;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveAnyone);

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


            {
                //Try to copy a move
                if (caller is BE_GoldenSlime gs)
                {
                    if (caller.curTarget.currMove == null || !caller.curTarget.currMove.CanChoose(gs))
                    {
                        gs.moveset[0] = caller.curTarget.moveset[0];
                    }
                    else
                    {
                        gs.moveset[0] = caller.curTarget.currMove;
                    }
                    if (caller.curTarget is PlayerEntity pe)
                    {
                        //enforce copying jump moves only
                        bool forceJump = false;

                        if (gs.moveset[0] is PlayerMove pm)
                        {
                            if (!pe.jumpMoves.Contains(pm))
                            {
                                forceJump = true;
                            }
                        }
                        else
                        {
                            forceJump = true;
                        }

                        if (forceJump)
                        {
                            gs.moveset[0] = pe.jumpMoves[BattleControl.Instance.GetPsuedoRandom(pe.jumpMoves.Count - 1, 5)];
                        }

                        if (gs.moveset[0] is LM_TeamThrow || gs.moveset[0] is WM_TeamQuake)
                        {
                            //something random I guess
                            gs.moveset[0] = pe.jumpMoves[4];
                        }
                    }
                    BattleControl.Instance.AddBattlePopup(new BattlePopup(caller, "This Golden Slime can now use " + gs.moveset[0].GetName()));
                }

                //caller.curTarget.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.Squish);
                //caller.DealDamage(caller.curTarget, 1, BattleHelper.DamageType.Normal, 0, BattleHelper.ContactLevel.Contact);
                yield return StartCoroutine(caller.JumpHeavy(caller.homePos, 2, 0.5f, 0.15f));
            }
        }

        yield return StartCoroutine(caller.MoveEasing(caller.homePos, (e) => MainManager.EasingOutIn(e)));
    }
}

public class BM_GoldenSlime_InversionSplash : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.GoldenSlime_InversionSplash;
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
                t.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.Launch);
                caller.DealDamage(t, 4, BattleHelper.DamageType.Air, 0, BattleHelper.ContactLevel.Infinite);
                t.InvertEffects();
            }
            else
            {
                //Miss
                caller.InvokeMissEvents(t);
            }
        }
        StartCoroutine(caller.RevertScale(0.1f));

        yield return StartCoroutine(caller.MoveEasing(caller.homePos, (e) => MainManager.EasingOutIn(e)));
    }
}

public class BM_GoldenSlime_GoldenWave : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.GoldenSlime_GoldenWave;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveAlly);

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        if (!BattleControl.Instance.EntityValid(caller.curTarget))
        {
            caller.curTarget = null;
        }

        if (caller.curTarget != null)
        {
            yield return StartCoroutine(caller.JumpHeavy(caller.homePos, 2, 0.5f, -0.25f));

            List<BattleEntity> list = BattleControl.Instance.GetEntitiesSorted(caller, GetTargetArea(caller));

            foreach (BattleEntity target in list)
            {
                caller.InflictEffect(target, new Effect(Effect.EffectType.HealthRegen, 3, 3));

                int bonus = 1;
                for (int i = 0; i < target.effects.Count; i++)
                {
                    //don't boost to infinity
                    if (target.effects[i].duration < Effect.MAX_NORMAL_DURATION)
                    {
                        if (target.effects[i].duration > Effect.MAX_NORMAL_DURATION - bonus)
                        {
                            target.effects[i].duration = Effect.MAX_NORMAL_DURATION;
                        }
                        else
                        {
                            target.effects[i].duration = (sbyte)(target.effects[i].duration + bonus);
                        }
                    }
                }
            }
        }

        yield return StartCoroutine(caller.MoveEasing(caller.homePos, (e) => MainManager.EasingOutIn(e)));
    }
}

public class BM_GoldenSlime_CopyForm : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.GoldenSlime_CopyForm;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveAllyWeak);

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

            //copy
            BattleEntity newEntity = (BattleControl.Instance.SummonEntity(caller.curTarget.entityID, caller.posId));
            newEntity.transform.position = caller.transform.position;
            Destroy(caller.ac.gameObject);
            yield return StartCoroutine(newEntity.JumpHeavy(caller.homePos, 2, 0.5f, 0.15f));
            newEntity.ReceiveEffectForce(new Effect(Effect.EffectType.Cooldown, 1, Effect.INFINITE_DURATION));
            foreach (Effect e in caller.curTarget.effects)
            {
                newEntity.ReceiveEffectForce(e);
            }

            newEntity.subObject.GetComponentInChildren<SpriteRenderer>().color = new Color(1, 1, 0.75f, 1);

            BattleControl.Instance.RemoveEntity(caller);
        }
    }
}

public class BE_ElementalSlime : BattleEntity
{
    public override void Initialize()
    {
        moveset = new List<Move> { gameObject.AddComponent<BM_ElementalSlime_FlameSplash>(), gameObject.AddComponent<BM_ElementalSlime_AirSplash>(), gameObject.AddComponent<BM_ElementalSlime_LightSplash>(), gameObject.AddComponent<BM_ElementalSlime_Infusion>() };

        base.Initialize();
    }

    public override void ChooseMoveInternal()
    {
        currMove = moveset[(posId + BattleControl.Instance.turnCount - 1) % 3];
        BasicTargetChooser();

        if (hp <= maxHP / 2 && currMove == moveset[1])
        {
            currMove = moveset[3];
            BasicTargetChooser();

            if (curTarget == null)
            {
                currMove = moveset[1];
                BasicTargetChooser();
            }
        }
    }
}

public class BM_ElementalSlime_FlameSplash : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.ElementalSlime_FlameSplash;
    //public override string GetName() => "Rootling Front Bite";
    //public override string GetDescription() => "";

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemy, true);

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        //iterate through ground targets
        //To do: get a better way to do this?
        List<BattleEntity> targets = BattleControl.Instance.GetEntitiesSorted(caller, GetBaseTarget());
        List<BattleEntity> allies = BattleControl.Instance.GetEntitiesSorted(caller, new TargetArea(TargetArea.TargetAreaType.LiveAlly));

        StartCoroutine(caller.SpinHeavy(Vector3.forward * 360, 0.5f));
        yield return StartCoroutine(caller.JumpHeavy(caller.homePos, 2, 0.5f, -0.25f));

        yield return StartCoroutine(caller.Squish(0.067f, 0.2f));
        foreach (BattleEntity t in targets)
        {
            if (caller.GetAttackHit(t, BattleHelper.DamageType.Fire))
            {
                t.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.Launch);
                caller.DealDamage(t, 3, BattleHelper.DamageType.Fire, 0, BattleHelper.ContactLevel.Infinite);
            }
            else
            {
                //Miss
                caller.InvokeMissEvents(t);
            }
        }
        foreach (BattleEntity t in allies)
        {
            caller.InflictEffectBuffered(t, new Effect(Effect.EffectType.Focus, 2, Effect.INFINITE_DURATION));
            if (BattleControl.Instance.GetCurseLevel() > 0)
            {
                caller.InflictEffectBuffered(t, new Effect(Effect.EffectType.AttackUp, 2, 3));
            }
        }
        StartCoroutine(caller.RevertScale(0.1f));

        BattleEntity target = caller.curTarget;
        yield return StartCoroutine(caller.JumpHeavy(caller.curTarget.ApplyScaledOffset(target.stompOffset), 2, 0.5f, -0.25f));
        if (caller.GetAttackHit(target, BattleHelper.DamageType.Water))
        {
            target.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.Launch);
            caller.DealDamage(target, 4, BattleHelper.DamageType.Water, 0, BattleHelper.ContactLevel.Infinite);
        }
        else
        {
            //Miss
            caller.InvokeMissEvents(target);
        }

        yield return StartCoroutine(caller.MoveEasing(caller.homePos, (e) => MainManager.EasingOutIn(e)));
    }
}

public class BM_ElementalSlime_AirSplash : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.ElementalSlime_AirSplash;
    //public override string GetName() => "Rootling Front Bite";
    //public override string GetDescription() => "";

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemy, true);

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        //iterate through ground targets
        //To do: get a better way to do this?
        List<BattleEntity> targets = BattleControl.Instance.GetEntitiesSorted(caller, GetBaseTarget());
        List<BattleEntity> allies = BattleControl.Instance.GetEntitiesSorted(caller, new TargetArea(TargetArea.TargetAreaType.LiveAlly));

        StartCoroutine(caller.SpinHeavy(Vector3.back * 360, 0.5f));
        yield return StartCoroutine(caller.JumpHeavy(caller.homePos, 2, 0.5f, -0.25f));

        yield return StartCoroutine(caller.Squish(0.067f, 0.2f));
        foreach (BattleEntity t in targets)
        {
            if (caller.GetAttackHit(t, BattleHelper.DamageType.Air))
            {
                t.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.Launch);
                caller.DealDamage(t, 3, BattleHelper.DamageType.Air, 0, BattleHelper.ContactLevel.Infinite);
            }
            else
            {
                //Miss
                caller.InvokeMissEvents(t);
            }
        }
        foreach (BattleEntity t in allies)
        {
            caller.InflictEffectBuffered(t, new Effect(Effect.EffectType.AstralWall, (sbyte)Mathf.CeilToInt(t.maxHP / 2f), 3));

        }
        StartCoroutine(caller.RevertScale(0.1f));

        BattleEntity target = caller.curTarget;
        yield return StartCoroutine(caller.JumpHeavy(caller.curTarget.ApplyScaledOffset(target.stompOffset), 2, 0.5f, -0.25f));
        if (caller.GetAttackHit(target, BattleHelper.DamageType.Earth))
        {
            target.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.Launch);
            caller.DealDamage(target, 4, BattleHelper.DamageType.Earth, 0, BattleHelper.ContactLevel.Infinite);
        }
        else
        {
            //Miss
            caller.InvokeMissEvents(target);
        }

        yield return StartCoroutine(caller.MoveEasing(caller.homePos, (e) => MainManager.EasingOutIn(e)));
    }
}

public class BM_ElementalSlime_LightSplash : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.ElementalSlime_LightSplash;
    //public override string GetName() => "Rootling Front Bite";
    //public override string GetDescription() => "";

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemy, true);

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        //iterate through ground targets
        //To do: get a better way to do this?
        List<BattleEntity> targets = BattleControl.Instance.GetEntitiesSorted(caller, GetBaseTarget());
        List<BattleEntity> allies = BattleControl.Instance.GetEntitiesSorted(caller, new TargetArea(TargetArea.TargetAreaType.LiveAlly));

        yield return StartCoroutine(caller.JumpHeavy(caller.homePos, 2, 0.5f, -0.25f));

        yield return StartCoroutine(caller.Squish(0.067f, 0.2f));
        foreach (BattleEntity t in targets)
        {
            if (caller.GetAttackHit(t, BattleHelper.DamageType.Light))
            {
                t.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.Launch);
                caller.DealDamage(t, 3, BattleHelper.DamageType.Light, 0, BattleHelper.ContactLevel.Infinite);
            }
            else
            {
                //Miss
                caller.InvokeMissEvents(t);
            }
        }
        foreach (BattleEntity t in allies)
        {
            caller.InflictEffectBuffered(t, new Effect(Effect.EffectType.Absorb, 2, Effect.INFINITE_DURATION));
            if (BattleControl.Instance.GetCurseLevel() > 0)
            {
                caller.InflictEffectBuffered(t, new Effect(Effect.EffectType.DefenseUp, 2, 3));
            }
        }
        StartCoroutine(caller.RevertScale(0.1f));

        BattleEntity target = caller.curTarget;
        yield return StartCoroutine(caller.JumpHeavy(caller.curTarget.ApplyScaledOffset(target.stompOffset), 2, 0.5f, -0.25f));
        if (caller.GetAttackHit(target, BattleHelper.DamageType.Dark))
        {
            target.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.Launch);
            caller.DealDamage(target, 4, BattleHelper.DamageType.Dark, 0, BattleHelper.ContactLevel.Infinite);
        }
        else
        {
            //Miss
            caller.InvokeMissEvents(target);
        }

        yield return StartCoroutine(caller.MoveEasing(caller.homePos, (e) => MainManager.EasingOutIn(e)));
    }
}

public class BM_ElementalSlime_Infusion : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.ElementalSlime_Infusion;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveAllyNotBossNotWeird);

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

            BattleEntity target = caller.curTarget;
            foreach (Effect e in caller.effects)
            {
                target.ReceiveEffectForce(e);
            }
            target.transform.localScale = Vector3.one * 1.1f;
            target.SpriteBoundUpdate();

            sbyte healAmount = (sbyte)Mathf.CeilToInt(caller.hp / 2f);
            caller.InflictEffect(target, new Effect(Effect.EffectType.MaxHPBoost, healAmount, Effect.INFINITE_DURATION));
            target.HealHealth(healAmount);
            caller.InflictEffect(target, new Effect(Effect.EffectType.AttackBoost, 3, Effect.INFINITE_DURATION));
            if (BattleControl.Instance.GetCurseLevel() > 0)
            {
                caller.InflictEffect(target, new Effect(Effect.EffectType.AstralWall, (sbyte)Mathf.CeilToInt(target.maxHP / 3f), Effect.INFINITE_DURATION));
                caller.InflictEffect(target, new Effect(Effect.EffectType.DefenseBoost, 3, Effect.INFINITE_DURATION));
            }

            BattleControl.Instance.RemoveEntity(caller);
        }
    }
}

public class BE_NormalSlime : BattleEntity
{
    public override void Initialize()
    {
        moveset = new List<Move> { gameObject.AddComponent<BM_NormalSlime_NormalStomp>(), gameObject.AddComponent<BM_NormalSlime_NeutralizingGas>() };

        base.Initialize();
    }

    public override void ChooseMoveInternal()
    {
        currMove = moveset[(posId + BattleControl.Instance.turnCount - 1) % 2];
        BasicTargetChooser();
    }
}

public class BM_NormalSlime_NormalStomp : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.NormalSlime_NormalStomp;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemyLowFrontmost);

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

            yield return StartCoroutine(caller.JumpHeavy(tpos, 1, 0.5f, -0.25f));

            if (caller.GetAttackHit(caller.curTarget, 0))
            {
                caller.curTarget.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.Squish);
                caller.DealDamage(caller.curTarget, 7, BattleHelper.DamageType.Normal, 0, BattleHelper.ContactLevel.Contact);
                caller.curTarget.CleanseEffects(false);
                yield return StartCoroutine(caller.JumpHeavy(caller.homePos, 2, 0.5f, 0.15f));
            }
            else
            {
                caller.InvokeMissEvents(caller.curTarget);

                //extrapolate the move curve
                yield return StartCoroutine(caller.ExtrapolateJumpHeavy(spos, tpos, 2, 0.5f, -0.25f));
                yield return StartCoroutine(caller.MoveEasing(caller.homePos, (e) => MainManager.EasingOutIn(e)));
            }
        }

        yield return StartCoroutine(caller.MoveEasing(caller.homePos, (e) => MainManager.EasingOutIn(e)));
    }
}

public class BM_NormalSlime_NeutralizingGas : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.NormalSlime_NeutralizingGas;
    //public override string GetName() => "Rootling Front Bite";
    //public override string GetDescription() => "";

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemy, true);

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        //iterate through ground targets
        //To do: get a better way to do this?
        List<BattleEntity> targets = BattleControl.Instance.GetEntitiesSorted(caller, GetBaseTarget());

        yield return StartCoroutine(caller.SpinHeavy(Vector3.up * 360, 0.5f));

        foreach (BattleEntity t in targets)
        {
            if (caller.GetAttackHit(t, BattleHelper.DamageType.Normal))
            {
                t.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.Launch);
                caller.DealDamage(t, 5, BattleHelper.DamageType.Normal, 0, BattleHelper.ContactLevel.Infinite);
                t.CleanseEffects(false);
            }
            else
            {
                //Miss
                caller.InvokeMissEvents(t);
            }
        }

        yield return StartCoroutine(caller.MoveEasing(caller.homePos, (e) => MainManager.EasingOutIn(e)));
    }
}

public class BE_SoftSlime : BattleEntity
{
    public override void Initialize()
    {
        moveset = new List<Move> { gameObject.AddComponent<BM_SoftSlime_SoftSplash>(), gameObject.AddComponent<BM_SoftSlime_HealingStomp>(), gameObject.AddComponent<BM_SoftSlime_SoftStomp>() };

        base.Initialize();
    }

    public override void ChooseMoveInternal()
    {
        currMove = moveset[(posId + BattleControl.Instance.turnCount - 1) % 3];

        BasicTargetChooser();

        if (currMove == moveset[1])
        {
            SpecialTargetChooser(TargetStrategy.TargetStrategyType.MissingHP);
            if (curTarget == null || curTarget.hp == curTarget.maxHP)
            {
                currMove = moveset[0];
                BasicTargetChooser();
            }
        }

        if (curTarget == null)
        {
            currMove = moveset[0];
            BasicTargetChooser();
        }
    }
}

public class BM_SoftSlime_SoftSplash : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.SoftSlime_SoftSplash;
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
                t.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.Launch);
                caller.DealDamage(t, 3, BattleHelper.DamageType.Water, 0, BattleHelper.ContactLevel.Infinite);
                caller.InflictEffect(t, new Effect(Effect.EffectType.Defocus, 2, Effect.INFINITE_DURATION));
            }
            else
            {
                //Miss
                caller.InvokeMissEvents(t);
            }
        }
        StartCoroutine(caller.RevertScale(0.1f));

        yield return StartCoroutine(caller.MoveEasing(caller.homePos, (e) => MainManager.EasingOutIn(e)));
    }
}

public class BM_SoftSlime_HealingStomp : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.SoftSlime_HealingStomp;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveAllyNotSameType);

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

            caller.curTarget.HealHealth(5);
            caller.curTarget.CureEffects(false);

            yield return StartCoroutine(caller.JumpHeavy(caller.homePos, 2, 0.5f, -0.25f));
        }
    }
}

public class BM_SoftSlime_SoftStomp : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.SoftSlime_SoftStomp;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveAllyNotSameType);

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

            caller.InflictEffect(caller.curTarget, new Effect(Effect.EffectType.Soften, 1, 2));
            caller.InflictEffect(caller.curTarget, new Effect(Effect.EffectType.Absorb, 3, Effect.INFINITE_DURATION));

            yield return StartCoroutine(caller.JumpHeavy(caller.homePos, 2, 0.5f, -0.25f));
        }
    }
}

public interface ICleaveUser
{
    public void SetCleave();
}

public class BE_RigidSlime : BattleEntity, ICleaveUser
{
    public bool didSplit = false;
    public override void Initialize()
    {
        moveset = new List<Move> { gameObject.AddComponent<BM_RigidSlime_DoubleStomp>(), gameObject.AddComponent<BM_RigidSlime_Charge>(), gameObject.AddComponent<BM_RigidSlime_SuperStomp>(), gameObject.AddComponent<BM_RigidSlime_Cleave>() };

        base.Initialize();
    }

    public void SetCleave()
    {
        didSplit = true;
    }

    public override void ChooseMoveInternal()
    {
        if (GetEntityProperty(BattleHelper.EntityProperties.StateCharge))
        {
            SetEntityProperty(BattleHelper.EntityProperties.StateCharge, false);
            currMove = moveset[2];
        } else
        {
            currMove = moveset[(posId + BattleControl.Instance.turnCount - 1) % 4];
            if (currMove == moveset[2])
            {
                if (BattleControl.Instance.FindUnoccupiedID(false, true) >= 4 || didSplit)
                {
                    currMove = moveset[0];
                } else
                {
                    currMove = moveset[3];
                }
            }

            if (BattleControl.Instance.FindUnoccupiedID(false, true) >= 4 || hp / 2 <= 0 && currMove == moveset[3])
            {
                currMove = moveset[0];
            }
        }

        BasicTargetChooser();
    }
}

public class BM_RigidSlime_DoubleStomp : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.RigidSlime_DoubleStomp;

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
                caller.curTarget.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.Squish);
                caller.DealDamage(caller.curTarget, 5, BattleHelper.DamageType.Normal, 0, BattleHelper.ContactLevel.Contact);
                yield return StartCoroutine(caller.JumpHeavy(tpos, 2, 0.5f, -0.25f));

                if (caller.GetAttackHit(caller.curTarget, 0))
                {
                    caller.DealDamage(caller.curTarget, 5, BattleHelper.DamageType.Normal, 0, BattleHelper.ContactLevel.Contact);
                } else
                {
                    caller.InvokeMissEvents(caller.curTarget);

                    //extrapolate the move curve
                    yield return StartCoroutine(caller.ExtrapolateJumpHeavy(spos, tpos, 2, 0.5f, -0.25f));
                }

                yield return StartCoroutine(caller.JumpHeavy(caller.homePos, 2, 0.5f, 0.15f));
            }
            else
            {
                caller.InvokeMissEvents(caller.curTarget);

                //extrapolate the move curve
                yield return StartCoroutine(caller.ExtrapolateJumpHeavy(spos, tpos, 2, 0.5f, -0.25f));
                yield return StartCoroutine(caller.MoveEasing(caller.homePos, (e) => MainManager.EasingOutIn(e)));
            }
        }

        yield return StartCoroutine(caller.MoveEasing(caller.homePos, (e) => MainManager.EasingOutIn(e)));
    }
}

public class BM_RigidSlime_Charge : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.RigidSlime_Charge;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.Self);

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        yield return StartCoroutine(caller.SpinHeavy(Vector3.up * 360, 0.25f));
        caller.SetEntityProperty(BattleHelper.EntityProperties.StateCharge);
        caller.InflictEffect(caller, new Effect(Effect.EffectType.QuantumShield, 1, 2));
        caller.InflictEffect(caller, new Effect(Effect.EffectType.Focus, 3, Effect.INFINITE_DURATION));
    }
}

public class BM_RigidSlime_SuperStomp : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.RigidSlime_SuperStomp;

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

            yield return StartCoroutine(caller.JumpHeavy(tpos, 10, 1f, -0.25f));

            if (caller.GetAttackHit(caller.curTarget, 0))
            {
                caller.curTarget.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.Squish);
                caller.DealDamage(caller.curTarget, 9, BattleHelper.DamageType.Normal, 0, BattleHelper.ContactLevel.Contact);
                yield return StartCoroutine(caller.JumpHeavy(caller.homePos, 2, 0.5f, 0.15f));
            }
            else
            {
                caller.InvokeMissEvents(caller.curTarget);

                //extrapolate the move curve
                yield return StartCoroutine(caller.ExtrapolateJumpHeavy(spos, tpos, 2, 0.5f, -0.25f));
                yield return StartCoroutine(caller.MoveEasing(caller.homePos, (e) => MainManager.EasingOutIn(e)));
            }
        }

        yield return StartCoroutine(caller.MoveEasing(caller.homePos, (e) => MainManager.EasingOutIn(e)));
    }
}

public class BM_RigidSlime_Cleave : EnemyMove
{
    public override MoveIndex GetMoveIndex() => MoveIndex.RigidSlime_Cleave;

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.Self);

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        yield return StartCoroutine(caller.SpinHeavy(Vector3.up * 360, 0.25f));

        //fizzle
        if (BattleControl.Instance.FindUnoccupiedID(false, true) >= 4 || caller.hp / 2 <= 0)
        {
            yield return new WaitForSeconds(0.5f);
            yield break;
        }

        BattleEntity newSlime = (BattleControl.Instance.SummonEntity(caller.entityID));

        if (caller is ICleaveUser icu)
        {
            icu.SetCleave();
        }
        if (newSlime != null)
        {
            caller.hp = caller.hp / 2;

            if (newSlime is ICleaveUser icu2)
            {
                icu2.SetCleave();
            }

            newSlime.hp = caller.hp;
            newSlime.statMultiplier = caller.statMultiplier;
            foreach (Effect e in caller.effects)
            {
                newSlime.ReceiveEffectForce(e);
            }
            newSlime.ReceiveEffectForce(new Effect(Effect.EffectType.Cooldown, 1, Effect.INFINITE_DURATION));

            newSlime.transform.position = caller.transform.position + Vector3.forward * -0.05f;
            yield return StartCoroutine(newSlime.JumpHeavy(newSlime.homePos, 0.6f, 0.2f, 1));
            yield return new WaitForSeconds(0.1f);
            newSlime.SetIdleAnimation(true);
            newSlime.StartIdle();
        }

        yield return new WaitForSeconds(0.5f);
    }
}