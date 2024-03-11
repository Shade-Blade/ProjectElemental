using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;


public abstract class LunaMove : PlayerMove
{
    public enum MoveType
    {
        HeavyStomp,
        Brace,
        DashThrough,
        FlipKick,
        FluffHeal,
        SleepStomp,
        MeteorStomp,
        UnderStrike,
        IronStomp,
        ElementalStomp,
        TeamThrow,
        DoubleEgg,
        Smash,
        PowerSmash,
        DazzleSmash,
        HammerThrow,
        BreakerSmash,
        FlameSmash,
        MomentumSmash,
        QuakeSmash,
        LightSmash,
        Illuminate,
        HammerBeat,
        MistWall
    }

    public int CostCalculation(BattleEntity caller, int level = 1, int scale = 2)
    {
        if (GetBaseCost() == 0)
        {
            return 0;
        }

        return CostModification(caller, level, GetBaseCost() + ((level - 1) * scale));
    }

    public int CostModification(BattleEntity caller, int level = 1, int cost = 1)
    {
        if (caller.entityID == BattleHelper.EntityID.Luna)
        {
            return StandardCostModification(caller, level, cost);
        }
        else
        {
            return 2 * StandardCostModification(caller, level, cost);
        }
    }

    public override int GetMaxLevel(BattleEntity caller)
    {
        if (caller is PlayerEntity pcaller)
        {
            return pcaller.GetLunaMoveMaxLevel(GetTextIndex());
        }
        return 1;
    }

    //public abstract int GetTextIndex();
    public string GetNameWithIndex(int index)
    {
        string output = BattleControl.Instance.lunaText[index + 1][1];
        return output;
    }

    public override string GetDescription(int level = 1)
    {
        return GetDescriptionWithIndex(0, level);
    }

    public string GetDescriptionWithIndex(int index, int level = 1)
    {
        string output = BattleControl.Instance.lunaText[index + 1][2];

        if (level != 1)
        {
            output += " <color,#0000ff>(Lv. " + level + ": " + BattleControl.Instance.lunaText[index + 1][1 + level] + ")</color>";
        }

        return output;
    }
} 

public class LM_HeavyStomp : LunaMove
{
    public LM_HeavyStomp()
    {
    }

    public override int GetTextIndex()
    {
        return 0;
    }
    public override string GetName() => GetNameWithIndex(GetTextIndex());
    public override string GetDescription(int level = 1) => GetDescriptionWithIndex(GetTextIndex(), level);
    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemyLowStompable, false);
    //public override float GetBasePower() => 1.0f;
    public override int GetBaseCost() => 0;

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        //Debug.Log(caller.id + " jump");
        if (!BattleControl.Instance.EntityValid(caller.curTarget))
        {
            caller.curTarget = null;
        }

        if (caller.curTarget != null)
        {
            int sd = 2;

            AC_Jump actionCommand = null;
            if (caller is PlayerEntity pcaller) //we have technology
            {
                sd = pcaller.GetStompDamage();
                actionCommand = gameObject.AddComponent<AC_Jump>();
                actionCommand.Init(pcaller);
                actionCommand.Setup(0.5f);
            }

            Vector3 tpos = caller.curTarget.ApplyScaledOffset(caller.curTarget.stompOffset);
            Vector3 spos = transform.position;

            //yield return StartCoroutine(caller.Squish(0.067f, 0.2f));
            //StartCoroutine(caller.RevertScale(0.1f));
            yield return StartCoroutine(caller.JumpHeavy(tpos, 2, 0.5f, -0.25f));

            bool result = actionCommand == null ? true : actionCommand.GetSuccess();
            if (actionCommand != null)
            {
                actionCommand.End();
                Destroy(actionCommand);
            }

            if (GetOutcome(caller, sd))
            {
                if (result)
                {
                    DealDamageSuccess(caller, sd);
                    //yield return StartCoroutine(caller.Squish(0.067f, 0.2f));
                    //StartCoroutine(caller.RevertScale(0.1f));
                    yield return StartCoroutine(caller.JumpHeavy(caller.homePos, 2, 0.5f, 0.15f));
                }
                else
                {
                    DealDamageFailure(caller, sd);
                    Vector3 targetPos = caller.transform.position;
                    float height = caller.transform.position.y;
                    targetPos += Vector3.down * height + Vector3.left * 0.5f * height;
                    //yield return StartCoroutine(caller.Squish(0.067f, 0.2f));
                    //StartCoroutine(caller.RevertScale(0.1f));
                    yield return StartCoroutine(caller.Jump(targetPos, 0.5f, 0.3f));
                    yield return StartCoroutine(caller.Move(caller.homePos));
                }
            }
            else
            {
                //Miss
                caller.InvokeMissEvents(caller.curTarget);

                //extrapolate the move curve
                yield return StartCoroutine(caller.ExtrapolateJumpHeavy(spos, tpos, 2, 0.5f, -0.25f));
                yield return StartCoroutine(caller.Move(caller.homePos));
            }
        }
        else
        {
            yield return StartCoroutine(caller.JumpHeavy(caller.homePos, 2, 0.5f, 0.15f));
        }
    }

    public virtual bool GetOutcome(BattleEntity caller, int sd)
    {
        return caller.GetAttackHit(caller.curTarget, 0);
    }
    public virtual void DealDamageSuccess(BattleEntity caller, int sd)
    {
        ulong propertyBlock = (ulong)BattleHelper.DamageProperties.AC_Success;
        caller.DealDamage(caller.curTarget, sd, BattleHelper.DamageType.Normal, propertyBlock, BattleHelper.ContactLevel.Contact);
    }
    public virtual void DealDamageFailure(BattleEntity caller, int sd)
    {
        caller.DealDamage(caller.curTarget, Mathf.CeilToInt(sd / 2f), BattleHelper.DamageType.Normal, 0, BattleHelper.ContactLevel.Contact);
    }

    public override string GetHighlightText(BattleEntity caller, BattleEntity target, int level = 1)
    {
        if (caller is PlayerEntity pcallerA)
        {
            if (!pcallerA.BadgeEquipped(Badge.BadgeType.PowerSight))
            {
                return "";
            }
        }

        int sd = 2;

        if (caller is PlayerEntity pcaller)
        {
            sd = pcaller.GetStompDamage();
        }

        int val = caller.DealDamageCalculation(target, sd, BattleHelper.DamageType.Normal, 0);

        return val + "";
    }
}

public class LM_Brace : LunaMove
{
    public LM_Brace()
    {
    }

    public override int GetTextIndex()
    {
        return 1;
    }
    public override string GetName() => GetNameWithIndex(GetTextIndex());
    public override string GetDescription(int level = 1) => GetDescriptionWithIndex(GetTextIndex(), level);
    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.Self, false);
    //public override float GetBasePower() => 0.5f;
    public override int GetBaseCost() => 1;

    public override int GetCost(BattleEntity caller, int level = 1)
    {
        switch (level)
        {
            case 1: return CostModification(caller, level, 1);
            case 2: return CostModification(caller, level, 3);
            case 3: return CostModification(caller, level, 9);
        }
        return CostCalculation(caller, level, 3);
    }

    /*
    public override int GetMaxLevel(BattleEntity caller)
    {
        return 3;
    }
    */

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        AC_PressButtonTimed actionCommand = null;
        if (caller is PlayerEntity pcaller) //we have technology
        {
            actionCommand = gameObject.AddComponent<AC_PressButtonTimed>();
            actionCommand.Init(pcaller);
            actionCommand.Setup(0.5f);
        }

        yield return new WaitForSeconds(ActionCommand.FADE_IN_TIME);
        StartCoroutine(caller.Spin(Vector3.up * 360, 0.5f));
        yield return new WaitUntil(() => actionCommand.IsComplete());

        bool result = actionCommand == null ? true : actionCommand.GetSuccess();
        if (actionCommand != null)
        {
            actionCommand.End();
            Destroy(actionCommand);
        }

        if (result)
        {
            BattleControl.Instance.CreateActionCommandEffect(BattleHelper.ActionCommandText.Nice, caller.GetDamageEffectPosition(), caller);
            yield return StartCoroutine(caller.JumpHeavy(caller.homePos, 1.5f, 0.3f, 0.15f));
            switch (level)
            {
                case 1:
                    caller.InflictEffect(caller, new Effect(Effect.EffectType.Absorb, 2, 255));
                    break;
                case 2:
                    caller.InflictEffect(caller, new Effect(Effect.EffectType.Absorb, 3, 255));
                    break;
                case 3:
                    caller.InflictEffect(caller, new Effect(Effect.EffectType.Absorb, 6, 255));
                    break;
            }
        }
        else
        {
            yield return StartCoroutine(caller.JumpHeavy(caller.homePos, 0.5f, 0.15f, 0.15f));
            switch (level)
            {
                case 1:
                    caller.InflictEffect(caller, new Effect(Effect.EffectType.Absorb, 1, 255));
                    break;
                case 2:
                    caller.InflictEffect(caller, new Effect(Effect.EffectType.Absorb, 2, 255));
                    break;
                case 3:
                    caller.InflictEffect(caller, new Effect(Effect.EffectType.Absorb, 3, 255));
                    break;
            }

        }
    }

    public override void PreMove(BattleEntity caller, int level = 1)
    {
    }
    public override void PostMove(BattleEntity caller, int level = 1)
    {
    }
}

public class LM_DashThrough : LunaMove
{
    public LM_DashThrough()
    {
    }

    public override int GetTextIndex()
    {
        return 2;
    }
    public override string GetName() => GetNameWithIndex(GetTextIndex());
    public override string GetDescription(int level = 1) => GetDescriptionWithIndex(GetTextIndex(), level);
    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemyLow, true);
    //public override float GetBasePower() => 1.5f;
    public override int GetBaseCost() => 3;

    public override int GetCost(BattleEntity caller, int level = 1)
    {
        return CostCalculation(caller, level, 3);
    }

    /*
    public override int GetMaxLevel(BattleEntity caller)
    {
        return 2;
    }
    */

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        //get list of entities to pass through
        List<BattleEntity> targets = BattleControl.Instance.GetEntitiesSorted(caller, GetBaseTarget());

        //somewhere offscreen
        Vector3 target = transform.position + Vector3.right * 16f;

        int sd = 2;

        AC_HoldLeft actionCommand = null;
        if (caller is PlayerEntity pcaller)
        {
            sd = pcaller.GetStompDamage();
            actionCommand = gameObject.AddComponent<AC_HoldLeft>();
            actionCommand.Init(pcaller);
            actionCommand.Setup(0.5f);
        }

        yield return new WaitUntil(() => actionCommand.IsStarted());

        yield return new WaitUntil(() => actionCommand.IsComplete());

        //yield return StartCoroutine(caller.Spin(Vector3.up * 360, 0.15f));
        bool result = actionCommand == null ? true : actionCommand.GetSuccess();
        if (actionCommand != null)
        {
            actionCommand.End();
            Destroy(actionCommand);
        }

        //doing multiple things at once

        bool move = true;
        IEnumerator MoveTracked()
        {
            yield return caller.Move(target, caller.entitySpeed * 2f);
            move = false;
        }

        StartCoroutine(MoveTracked());

        int count = 0;
        while (move)
        {
            for (int i = 0; i < targets.Count; i++)
            {
                if (targets[i].homePos.x <= caller.transform.position.x)
                {
                    //Debug.Log(targets[i]);
                    if (caller.GetAttackHit(targets[i], 0))
                    {
                        count++;
                        ulong propertyBlock = 0;
                        if (count == 1)
                        {
                            propertyBlock = (ulong)BattleHelper.DamageProperties.AC_Success;
                        } else
                        {
                            propertyBlock = (ulong)BattleHelper.DamageProperties.AC_SuccessStall;
                        }
                        //Note: this is before the action command is done, so these are independent of action command
                        switch (level)
                        {
                            case 1:
                                caller.DealDamage(targets[i], sd - 1, BattleHelper.DamageType.Normal, propertyBlock, BattleHelper.ContactLevel.Contact);
                                break;
                            case 2:
                                caller.DealDamage(targets[i], sd, BattleHelper.DamageType.Normal, propertyBlock, BattleHelper.ContactLevel.Infinite);
                                break;
                        }
                    } else
                    {
                        caller.InvokeMissEvents(targets[i]);
                    }
                    targets.RemoveAt(i);
                    i--;
                }
            }
            yield return null;
        }
        
        yield return StartCoroutine(caller.Move(caller.homePos));
    }

    public override void PreMove(BattleEntity caller, int level = 1)
    {

    }
    public override void PostMove(BattleEntity caller, int level = 1)
    {
    }

    public override string GetHighlightText(BattleEntity caller, BattleEntity target, int level = 1)
    {
        if (caller is PlayerEntity pcallerA)
        {
            if (!pcallerA.BadgeEquipped(Badge.BadgeType.PowerSight))
            {
                return "";
            }
        }

        int sd = 2;

        if (caller is PlayerEntity pcaller)
        {
            sd = pcaller.GetStompDamage();
        }

        int val = 0;

        switch (level)
        {
            case 1:
                val = caller.DealDamageCalculation(target, sd - 1, 0, 0);
                break;
            case 2:
                val = caller.DealDamageCalculation(target, sd, 0, 0);
                break;
        }


        return val + "?";
    }
}

public class LM_FlipKick : LunaMove
{
    public LM_FlipKick()
    {
    }

    public override int GetTextIndex()
    {
        return 3;
    }
    public override string GetName() => GetNameWithIndex(GetTextIndex());
    public override string GetDescription(int level = 1) => GetDescriptionWithIndex(GetTextIndex(), level);
    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemy, false);
    //public override float GetBasePower() => 1.0f;
    public override int GetBaseCost() => 4;

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        //Debug.Log(caller.id + " jump");
        if (!BattleControl.Instance.EntityValid(caller.curTarget))
        {
            caller.curTarget = null;
        }

        if (caller.curTarget != null)
        {
            int sd = 2;

            AC_Jump actionCommand = null;
            if (caller is PlayerEntity pcaller) //we have technology
            {
                sd = pcaller.GetStompDamage();
                actionCommand = gameObject.AddComponent<AC_Jump>();
                actionCommand.Init(pcaller);
                actionCommand.Setup(0.5f);
            }

            Vector3 tpos = caller.curTarget.ApplyScaledOffset(caller.curTarget.kickOffset) + ((caller.width) / 2) * Vector3.left;
            Vector3 spos = transform.position;

            //yield return StartCoroutine(caller.Squish(0.067f, 0.2f));
            //StartCoroutine(caller.RevertScale(0.1f));
            yield return StartCoroutine(caller.JumpHeavy(tpos, 2, 0.5f, -0.25f));

            bool result = actionCommand == null ? true : actionCommand.GetSuccess();
            if (actionCommand != null)
            {
                actionCommand.End();
                Destroy(actionCommand);
            }

            if (GetOutcome(caller, sd))
            {
                if (result)
                {
                    DealDamageSuccess(caller, sd, result);
                    //yield return StartCoroutine(caller.Squish(0.067f, 0.2f));
                    //StartCoroutine(caller.RevertScale(0.1f));
                    yield return StartCoroutine(caller.JumpHeavy(caller.homePos, 2, 0.5f, 0.15f));
                }
                else
                {
                    DealDamageFailure(caller, sd, result);
                    Vector3 targetPos = caller.transform.position;
                    float height = caller.transform.position.y;
                    targetPos += Vector3.down * height + Vector3.left * 0.5f * height;
                    //yield return StartCoroutine(caller.Squish(0.067f, 0.2f));
                    //StartCoroutine(caller.RevertScale(0.1f));
                    yield return StartCoroutine(caller.Jump(targetPos, 0.5f, 0.3f));
                    yield return StartCoroutine(caller.Move(caller.homePos));
                }
            }
            else
            {
                //Miss
                caller.InvokeMissEvents(caller.curTarget);

                //extrapolate the move curve
                yield return StartCoroutine(caller.ExtrapolateJumpHeavy(spos, tpos, 2, 0.5f, -0.25f));
                yield return StartCoroutine(caller.Move(caller.homePos));
            }
        }
        else
        {
            yield return StartCoroutine(caller.JumpHeavy(caller.homePos, 2, 0.5f, 0.15f));
        }
    }

    public virtual bool GetOutcome(BattleEntity caller, int sd)
    {
        return caller.GetAttackHit(caller.curTarget, 0);
    }
    public virtual void DealDamageSuccess(BattleEntity caller, int sd, bool result)
    {
        ulong propertyBlock = (ulong)BattleHelper.DamageProperties.AC_Success;
        caller.DealDamage(caller.curTarget, sd - 1, BattleHelper.DamageType.Normal, propertyBlock, BattleHelper.ContactLevel.Contact);
    }
    public virtual void DealDamageFailure(BattleEntity caller, int sd, bool result)
    {
        caller.DealDamage(caller.curTarget, Mathf.CeilToInt(sd / 2f) - 1, BattleHelper.DamageType.Normal, 0, BattleHelper.ContactLevel.Contact);
    }

    public override string GetHighlightText(BattleEntity caller, BattleEntity target, int level = 1)
    {
        if (caller is PlayerEntity pcallerA)
        {
            if (!pcallerA.BadgeEquipped(Badge.BadgeType.PowerSight))
            {
                return "";
            }
        }

        int sd = 2;

        if (caller is PlayerEntity pcaller)
        {
            sd = pcaller.GetStompDamage();
        }

        int val = caller.DealDamageCalculation(target, sd - 1, 0, 0);

        return val + "";
    }
}

public class LM_FluffHeal : LunaMove
{
    public LM_FluffHeal()
    {
    }

    public override int GetTextIndex()
    {
        return 4;
    }
    public override string GetName() => GetNameWithIndex(GetTextIndex());
    public override string GetDescription(int level = 1) => GetDescriptionWithIndex(GetTextIndex(), level);
    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveAlly, false);
    //public override float GetBasePower() => 0.5f;
    public override int GetBaseCost() => 4;

    public override TargetArea GetTargetArea(BattleEntity caller, int level = 1)
    {
        switch (level)
        {
            case 1:
                return new TargetArea(TargetArea.TargetAreaType.LiveAlly, false);
            default:
                return new TargetArea(TargetArea.TargetAreaType.LiveAlly, true);
        }
    }

    public override int GetCost(BattleEntity caller, int level = 1)
    {
        switch (level)
        {
            case 1: return CostModification(caller, level, 4);
            case 2: return CostModification(caller, level, 8);
        }
        return CostCalculation(caller, level, 4);
    }

    /*
    public override int GetMaxLevel(BattleEntity caller)
    {
        return 2;
    }
    */

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        AC_PressButtonTimed actionCommand = null;
        if (caller is PlayerEntity pcaller) //we have technology
        {
            actionCommand = gameObject.AddComponent<AC_PressButtonTimed>();
            actionCommand.Init(pcaller);
            actionCommand.Setup(0.5f);
        }

        yield return new WaitForSeconds(ActionCommand.FADE_IN_TIME);
        StartCoroutine(caller.Spin(Vector3.up * 360, 0.5f));
        yield return new WaitUntil(() => actionCommand.IsComplete());

        bool result = actionCommand == null ? true : actionCommand.GetSuccess();
        if (actionCommand != null)
        {
            actionCommand.End();
            Destroy(actionCommand);
        }

        if (result)
        {
            BattleControl.Instance.CreateActionCommandEffect(BattleHelper.ActionCommandText.Nice, caller.GetDamageEffectPosition(), caller);
            yield return StartCoroutine(caller.JumpHeavy(caller.homePos, 1.5f, 0.3f, 0.15f));

            switch (level)
            {
                case 1:
                    caller.InflictEffect(caller.curTarget, new Effect(Effect.EffectType.HealthRegen, 4, 3));
                    break;
                case 2:
                    List<BattleEntity> targets = BattleControl.Instance.GetEntitiesSorted(caller, GetTargetArea(caller, level));
                    foreach (BattleEntity b in targets)
                    {
                        caller.InflictEffect(b, new Effect(Effect.EffectType.HealthRegen, 4, 3));
                    }
                    break;
            }
        }
        else
        {
            yield return StartCoroutine(caller.JumpHeavy(caller.homePos, 0.5f, 0.15f, 0.15f));
            switch (level)
            {
                case 1:
                    caller.InflictEffect(caller.curTarget, new Effect(Effect.EffectType.HealthRegen, 2, 3));
                    break;
                case 2:
                    List<BattleEntity> targets = BattleControl.Instance.GetEntitiesSorted(caller, GetTargetArea(caller, level));
                    foreach (BattleEntity b in targets)
                    {
                        caller.InflictEffect(b, new Effect(Effect.EffectType.HealthRegen, 2, 3));
                    }
                    break;
            }
        }
    }

    public override void PreMove(BattleEntity caller, int level = 1)
    {
    }
    public override void PostMove(BattleEntity caller, int level = 1)
    {
    }
}

public class LM_SleepStomp : LM_HeavyStomp
{
    public LM_SleepStomp()
    {
    }

    public override int GetTextIndex()
    {
        return 5;
    }
    public override string GetName() => GetNameWithIndex(GetTextIndex());
    public override string GetDescription(int level = 1) => GetDescriptionWithIndex(GetTextIndex(), level);
    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemyLowStompable, false);
    //public override float GetBasePower() => 1.0f;
    public override int GetBaseCost() => 5;

    public override bool GetOutcome(BattleEntity caller, int sd)
    {
        return caller.GetAttackHit(caller.curTarget, 0);
    }
    public override void DealDamageSuccess(BattleEntity caller, int sd)
    {
        ulong propertyBlock = (ulong)BattleHelper.DamageProperties.AC_Success;
        caller.DealDamage(caller.curTarget, sd, 0, propertyBlock, BattleHelper.ContactLevel.Contact);
        caller.InflictEffect(caller.curTarget, new Effect(Effect.EffectType.Sleep, 1, 2), caller.posId);
    }
    public override void DealDamageFailure(BattleEntity caller, int sd)
    {
        caller.DealDamage(caller.curTarget, Mathf.CeilToInt(sd / 2f), 0, 0, BattleHelper.ContactLevel.Contact);
        caller.InflictEffect(caller.curTarget, new Effect(Effect.EffectType.Sleep, 1, 2), caller.posId);
    }

    public override string GetHighlightText(BattleEntity caller, BattleEntity target, int level = 1)
    {
        bool damageDisplay = false;
        bool statusDisplay = false;
        if (caller is PlayerEntity pcallerA)
        {
            if (pcallerA.BadgeEquipped(Badge.BadgeType.PowerSight))
            {
                damageDisplay = true;
            }

            if (pcallerA.BadgeEquipped(Badge.BadgeType.StatusSight))
            {
                statusDisplay = true;
            }
        }
        if (!damageDisplay && !statusDisplay)
        {
            return "";
        }

        int sd = 2;

        float statusBoost = 1;
        if (caller is PlayerEntity pcaller)
        {
            sd = pcaller.GetStompDamage();
            statusBoost = pcaller.CalculateStatusBoost(target);
        }

        int val = caller.DealDamageCalculation(target, sd, 0, 0);

        int statusHP = (int)(target.StatusWorkingHP(Effect.EffectType.Sleep) * statusBoost);

        bool doesWork = (target.hp > 0) && (target.hp <= statusHP);

        //bool realDoesWork = target.StatusWillWork(Effect.EffectType.Freeze);

        //Debug.Log(doesWork + " " + realDoesWork);

        string outString = "";
        if (damageDisplay)
        {
            outString += val;
        }
        if (statusDisplay)
        {
            if (outString.Length > 0)
            {
                outString += "<line>";
            }
            if (doesWork)
            {
                return outString + "<highlightyescolor>(" + statusHP + ")</highlightyescolor>";
            }
            else
            {
                return outString + "<highlightnocolor>(" + statusHP + ")</highlightnocolor>";
            }
        }

        return outString;
    }
}

public class LM_MeteorStomp : LunaMove
{
    public LM_MeteorStomp()
    {
    }

    public override int GetTextIndex()
    {
        return 6;
    }
    public override string GetName() => GetNameWithIndex(GetTextIndex());
    public override string GetDescription(int level = 1) => GetDescriptionWithIndex(GetTextIndex(), level);
    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemyTopmost, false);
    //public override float GetBasePower() => 1.0f;
    public override int GetBaseCost() => 6;

    public override int GetCost(BattleEntity caller, int level = 1)
    {
        return CostCalculation(caller, level, 4);
    }

    /*
    public override int GetMaxLevel(BattleEntity caller)
    {
        return 2;
    }
    */

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        //Debug.Log(caller.id + " jump");
        if (!BattleControl.Instance.EntityValid(caller.curTarget))
        {
            caller.curTarget = null;
        }

        if (caller.curTarget != null)
        {
            int sd = 2;

            AC_Jump actionCommand = null;
            if (caller is PlayerEntity pcaller) //we have technology
            {
                sd = pcaller.GetStompDamage();
                actionCommand = gameObject.AddComponent<AC_Jump>();
                actionCommand.Init(pcaller);
                actionCommand.Setup(0.65f);
            }

            Vector3 tpos = caller.curTarget.ApplyScaledOffset(caller.curTarget.stompOffset);
            Vector3 spos = transform.position;

            //yield return StartCoroutine(caller.Squish(0.067f, 0.2f));
            //StartCoroutine(caller.RevertScale(0.1f));
            yield return StartCoroutine(caller.JumpHeavy(tpos, 4, 0.65f, -0.25f));

            bool result = actionCommand == null ? true : actionCommand.GetSuccess();
            if (actionCommand != null)
            {
                actionCommand.End();
                Destroy(actionCommand);
            }

            if (GetOutcome(caller, sd))
            {
                if (result)
                {
                    DealDamageSuccess(caller, sd);
                    //yield return StartCoroutine(caller.Squish(0.067f, 0.2f));
                    //StartCoroutine(caller.RevertScale(0.1f));
                    yield return StartCoroutine(caller.JumpHeavy(caller.homePos, 2, 0.5f, 0.15f));
                }
                else
                {
                    DealDamageFailure(caller, sd);
                    Vector3 targetPos = caller.transform.position;
                    float height = caller.transform.position.y;
                    targetPos += Vector3.down * height + Vector3.left * 0.5f * height;
                    //yield return StartCoroutine(caller.Squish(0.067f, 0.2f));
                    //StartCoroutine(caller.RevertScale(0.1f));
                    yield return StartCoroutine(caller.Jump(targetPos, 0.5f, 0.3f));
                    yield return StartCoroutine(caller.Move(caller.homePos));
                }
            }
            else
            {
                //Miss
                caller.InvokeMissEvents(caller.curTarget);

                //extrapolate the move curve
                yield return StartCoroutine(caller.ExtrapolateJumpHeavy(spos, tpos, 2, 0.5f, -0.25f));
                yield return StartCoroutine(caller.Move(caller.homePos));
            }
        }
        else
        {
            yield return StartCoroutine(caller.JumpHeavy(caller.homePos, 2, 0.5f, 0.15f));
        }
    }

    public bool GetOutcome(BattleEntity caller, int sd)
    {
        return caller.GetAttackHit(caller.curTarget, 0);
    }
    public void DealDamageSuccess(BattleEntity caller, int sd)
    {
        ulong propertyBlock = (ulong)BattleHelper.DamageProperties.AC_Success;
        ulong propertyBlockB = (ulong)(BattleHelper.DamageProperties.AdvancedElementCalc | BattleHelper.DamageProperties.AC_Success);
        switch (level)
        {
            case 2:
                caller.DealDamage(caller.curTarget, sd + 2, BattleHelper.DamageType.Fire, propertyBlockB, BattleHelper.ContactLevel.Contact);
                caller.InflictEffect(caller.curTarget, new Effect(Effect.EffectType.AttackDown, 2, 3), caller.posId);
                break;
            case 1:
                caller.DealDamage(caller.curTarget, sd + 2, BattleHelper.DamageType.Fire, propertyBlock, BattleHelper.ContactLevel.Contact);
                break;
        }
    }
    public void DealDamageFailure(BattleEntity caller, int sd)
    {
        ulong propertyBlock = (ulong)(BattleHelper.DamageProperties.AdvancedElementCalc);
        switch (level)
        {
            case 2:
                caller.DealDamage(caller.curTarget, Mathf.CeilToInt(sd / 2f) + 2, BattleHelper.DamageType.Fire, propertyBlock, BattleHelper.ContactLevel.Contact);
                caller.InflictEffect(caller.curTarget, new Effect(Effect.EffectType.AttackDown, 2, 3), caller.posId);
                break;
            case 1:
                caller.DealDamage(caller.curTarget, Mathf.CeilToInt(sd / 2f) + 2, BattleHelper.DamageType.Fire, 0, BattleHelper.ContactLevel.Contact);
                break;
        }
    }

    public override string GetHighlightText(BattleEntity caller, BattleEntity target, int level = 1)
    {
        if (caller is PlayerEntity pcallerA)
        {
            if (!pcallerA.BadgeEquipped(Badge.BadgeType.PowerSight))
            {
                return "";
            }
        }

        int sd = 2;

        if (caller is PlayerEntity pcaller)
        {
            sd = pcaller.GetStompDamage();
        }

        int val = 0;

        ulong propertyBlock = (ulong)(BattleHelper.DamageProperties.AdvancedElementCalc);
        switch (level)
        {
            case 1:
                val = caller.DealDamageCalculation(target, sd + 2, BattleHelper.DamageType.Fire, 0);
                break;
            case 2:
                val = caller.DealDamageCalculation(target, sd + 2, BattleHelper.DamageType.Fire, propertyBlock);
                break;
        }


        return val + "";
    }
}

public class LM_UnderStrike : LunaMove
{
    public LM_UnderStrike()
    {
    }

    public override int GetTextIndex()
    {
        return 7;
    }
    public override string GetName() => GetNameWithIndex(GetTextIndex());
    public override string GetDescription(int level = 1) => GetDescriptionWithIndex(GetTextIndex(), level);
    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemyLow, false);
    //public override float GetBasePower() => 1.0f;
    public override int GetBaseCost() => 7;

    public override int GetCost(BattleEntity caller, int level = 1)
    {
        return CostCalculation(caller, level, 4);
    }

    /*
    public override int GetMaxLevel(BattleEntity caller)
    {
        return 2;
    }
    */

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        //Debug.Log(caller.id + " jump");
        if (!BattleControl.Instance.EntityValid(caller.curTarget))
        {
            caller.curTarget = null;
        }

        if (caller.curTarget != null)
        {
            int sd = 2;

            Vector3 bpos = caller.homePos + caller.curTarget.height * 2 * Vector3.down;
            yield return StartCoroutine(caller.Move(bpos, 16));

            AC_PressButtonTimed actionCommand = null;
            if (caller is PlayerEntity pcaller) //we have technology
            {
                sd = pcaller.GetStompDamage();
                actionCommand = gameObject.AddComponent<AC_PressButtonTimed>();
                actionCommand.Init(pcaller);
                actionCommand.Setup(0.5f);
            }



            Vector3 tpos = caller.curTarget.transform.position - caller.curTarget.transform.position.y * Vector3.up + caller.transform.position.y * Vector3.up;
            caller.transform.position = tpos;
            Vector3 spos = caller.transform.position + caller.curTarget.height * Vector3.up * 2;
            Vector3 tpos2 = caller.curTarget.transform.position + caller.curTarget.height * Vector3.up;

            //yield return StartCoroutine(caller.Squish(0.067f, 0.2f));
            //StartCoroutine(caller.RevertScale(0.1f));
            //yield return StartCoroutine(caller.JumpHeavy(tpos, 4, 0.65f, -0.25f));

            yield return new WaitUntil(() => actionCommand.IsComplete());

            bool result = actionCommand == null ? true : actionCommand.GetSuccess();
            if (actionCommand != null)
            {
                actionCommand.End();
                Destroy(actionCommand);
            }

            if (GetOutcome(caller, sd))
            {
                if (result)
                {
                    if (level > 1)
                    {
                        DealDamageSuccessA(caller, sd);
                        AC_Jump actionCommand2 = null;
                        if (caller is PlayerEntity pcaller2) //we have technology
                        {
                            actionCommand2 = gameObject.AddComponent<AC_Jump>();
                            actionCommand2.Init(pcaller2);
                            actionCommand2.Setup(0.65f);
                        }
                        yield return StartCoroutine(caller.JumpHeavy(tpos2, 3f, 0.65f, 0.15f));

                        bool result2 = actionCommand2 == null ? true : actionCommand2.GetSuccess();
                        if (actionCommand2 != null)
                        {
                            actionCommand2.End();
                            Destroy(actionCommand2);
                        }

                        if (result2)
                        {
                            DealDamageSuccessB(caller, sd);
                            //yield return StartCoroutine(caller.Squish(0.067f, 0.2f));
                            //StartCoroutine(caller.RevertScale(0.1f));
                            yield return StartCoroutine(caller.JumpHeavy(caller.homePos, 2, 0.5f, 0.15f));
                        }
                        else
                        {
                            DealDamageFailureB(caller, sd);
                            Vector3 targetPos = caller.transform.position;
                            float height = caller.transform.position.y;
                            targetPos += Vector3.down * height + Vector3.left * 0.5f * height;
                            //yield return StartCoroutine(caller.Squish(0.067f, 0.2f));
                            //StartCoroutine(caller.RevertScale(0.1f));
                            yield return StartCoroutine(caller.Jump(targetPos, 0.5f, 0.3f));
                            yield return StartCoroutine(caller.Move(caller.homePos));
                        }
                    }
                    else
                    {
                        DealDamageSuccess(caller, sd);
                        yield return StartCoroutine(caller.JumpHeavy(spos, 2, 0.5f, 0.15f));
                    }
                    yield return StartCoroutine(caller.Move(caller.homePos));
                }
                else
                {
                    DealDamageFailure(caller, sd);
                    yield return StartCoroutine(caller.JumpHeavy(spos, 0.5f, 0.5f, 0.15f));
                    yield return StartCoroutine(caller.Move(caller.homePos));
                }
            }
            else
            {
                //Miss
                caller.InvokeMissEvents(caller.curTarget);

                //extrapolate the move curve
                //yield return StartCoroutine(caller.ExtrapolateJumpHeavy(spos, tpos, 2, 0.5f, -0.25f));
                yield return StartCoroutine(caller.JumpHeavy(spos, 2, 0.5f, 0.15f));
                yield return StartCoroutine(caller.Move(caller.homePos));
            }
        }
        else
        {
            yield return StartCoroutine(caller.JumpHeavy(caller.homePos, 2, 0.5f, 0.15f));
        }
    }

    public bool GetOutcome(BattleEntity caller, int sd)
    {
        return caller.GetAttackHit(caller.curTarget, 0);
    }
    public void DealDamageSuccess(BattleEntity caller, int sd)
    {
        ulong propertyBlock = (ulong)BattleHelper.DamageProperties.AC_Success;
        caller.DealDamage(caller.curTarget, sd + 3, BattleHelper.DamageType.Dark, propertyBlock, BattleHelper.ContactLevel.Contact);
    }
    public void DealDamageFailure(BattleEntity caller, int sd)
    {
        switch (level)
        {
            case 2:
                caller.DealDamage(caller.curTarget, Mathf.CeilToInt(sd / 2f) + 2, BattleHelper.DamageType.Dark, 0, BattleHelper.ContactLevel.Contact);
                caller.InflictEffect(caller.curTarget, new Effect(Effect.EffectType.Defocus, 3, 255), caller.posId);
                break;
            case 1:
                caller.DealDamage(caller.curTarget, Mathf.CeilToInt(sd / 2f) + 2, BattleHelper.DamageType.Dark, 0, BattleHelper.ContactLevel.Contact);
                break;
        }
    }
    public void DealDamageSuccessA(BattleEntity caller, int sd)
    {
        ulong propertyBlock = (ulong)(BattleHelper.DamageProperties.Combo | BattleHelper.DamageProperties.AC_Success);
        switch (level)
        {
            case 2:
                caller.DealDamage(caller.curTarget, sd + 2, BattleHelper.DamageType.Dark, propertyBlock, BattleHelper.ContactLevel.Contact);
                break;
            case 1:
                caller.DealDamage(caller.curTarget, sd + 2, BattleHelper.DamageType.Dark, propertyBlock, BattleHelper.ContactLevel.Contact);
                break;
        }
    }
    public void DealDamageSuccessB(BattleEntity caller, int sd)
    {
        ulong propertyBlock = (ulong)BattleHelper.DamageProperties.AC_Success;
        switch (level)
        {
            case 2:
                caller.DealDamage(caller.curTarget, sd, 0, propertyBlock, BattleHelper.ContactLevel.Contact);
                caller.InflictEffect(caller.curTarget, new Effect(Effect.EffectType.Defocus, 6, 255), caller.posId);
                break;
            case 1:
                caller.DealDamage(caller.curTarget, sd, 0, propertyBlock, BattleHelper.ContactLevel.Contact);
                break;
        }
    }
    public void DealDamageFailureB(BattleEntity caller, int sd)
    {
        switch (level)
        {
            case 2:
                caller.DealDamage(caller.curTarget, Mathf.CeilToInt(sd / 2f), 0, 0, BattleHelper.ContactLevel.Contact);
                caller.InflictEffect(caller.curTarget, new Effect(Effect.EffectType.Defocus, 6, 255), caller.posId);
                break;
            case 1:
                caller.DealDamage(caller.curTarget, Mathf.CeilToInt(sd / 2f), 0, 0, BattleHelper.ContactLevel.Contact);
                break;
        }
    }


    public override string GetHighlightText(BattleEntity caller, BattleEntity target, int level = 1)
    {
        if (caller is PlayerEntity pcallerA)
        {
            if (!pcallerA.BadgeEquipped(Badge.BadgeType.PowerSight))
            {
                return "";
            }
        }

        int sd = 2;

        if (caller is PlayerEntity pcaller)
        {
            sd = pcaller.GetStompDamage();
        }

        string outstring = "";

        int hits = 2 + (level - 1);

        int val = 0;

        if (level > 1)
        {
            val = caller.DealDamageCalculation(target, sd + 2, BattleHelper.DamageType.Dark, 0);
            outstring = val + ", ";
            val = caller.DealDamageCalculation(target, Mathf.CeilToInt(sd / 2f), 0, 0);
            outstring += val + "?";
        } else
        {
            val = caller.DealDamageCalculation(target, sd + 2, BattleHelper.DamageType.Dark, 0);
            outstring = val + "";
        }

        return outstring;
    }
}

public class LM_IronStomp : LunaMove
{
    public LM_IronStomp()
    {
    }

    public override int GetTextIndex()
    {
        return 8;
    }
    public override string GetName() => GetNameWithIndex(GetTextIndex());
    public override string GetDescription(int level = 1) => GetDescriptionWithIndex(GetTextIndex(), level);
    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemyTopmost, false);
    //public override float GetBasePower() => 1.0f;
    public override int GetBaseCost() => 8;

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        //Debug.Log(caller.id + " jump");
        if (!BattleControl.Instance.EntityValid(caller.curTarget))
        {
            caller.curTarget = null;
        }

        if (caller.curTarget != null)
        {
            int sd = 2;

            AC_Jump actionCommand = null;
            if (caller is PlayerEntity pcaller) //we have technology
            {
                sd = pcaller.GetStompDamage();
                actionCommand = gameObject.AddComponent<AC_Jump>();
                actionCommand.Init(pcaller);
                actionCommand.Setup(0.65f);
            }

            Vector3 tpos = caller.curTarget.ApplyScaledOffset(caller.curTarget.stompOffset);
            Vector3 spos = transform.position;

            //yield return StartCoroutine(caller.Squish(0.067f, 0.2f));
            //StartCoroutine(caller.RevertScale(0.1f));
            yield return StartCoroutine(caller.JumpHeavy(tpos, 4, 0.65f, -0.25f));

            bool result = actionCommand == null ? true : actionCommand.GetSuccess();
            if (actionCommand != null)
            {
                actionCommand.End();
                Destroy(actionCommand);
            }

            if (GetOutcome(caller, sd))
            {
                if (result)
                {
                    DealDamageSuccess(caller, sd);
                    //yield return StartCoroutine(caller.Squish(0.067f, 0.2f));
                    //StartCoroutine(caller.RevertScale(0.1f));
                    yield return StartCoroutine(caller.JumpHeavy(caller.homePos, 2, 0.5f, 0.15f));
                }
                else
                {
                    DealDamageFailure(caller, sd);
                    Vector3 targetPos = caller.transform.position;
                    float height = caller.transform.position.y;
                    targetPos += Vector3.down * height + Vector3.left * 0.5f * height;
                    //yield return StartCoroutine(caller.Squish(0.067f, 0.2f));
                    //StartCoroutine(caller.RevertScale(0.1f));
                    yield return StartCoroutine(caller.Jump(targetPos, 0.5f, 0.3f));
                    yield return StartCoroutine(caller.Move(caller.homePos));
                }
            }
            else
            {
                //Miss
                caller.InvokeMissEvents(caller.curTarget);

                //extrapolate the move curve
                yield return StartCoroutine(caller.ExtrapolateJumpHeavy(spos, tpos, 2, 0.5f, -0.25f));
                yield return StartCoroutine(caller.Move(caller.homePos));
            }
        }
        else
        {
            yield return StartCoroutine(caller.JumpHeavy(caller.homePos, 2, 0.5f, 0.15f));
        }
    }

    public bool GetOutcome(BattleEntity caller, int sd)
    {
        return caller.GetAttackHit(caller.curTarget, 0);
    }
    public void DealDamageSuccess(BattleEntity caller, int sd)
    {
        ulong propertyBlock = (ulong)BattleHelper.DamageProperties.AC_Success;
        int defBonus = caller.GetEffectDefenseBonus() + caller.GetBadgeDefenseBonus();
        Debug.Log(defBonus);
        caller.DealDamage(caller.curTarget, sd + 4 + defBonus, 0, propertyBlock, BattleHelper.ContactLevel.Contact);
    }
    public void DealDamageFailure(BattleEntity caller, int sd)
    {
        int defBonus = caller.GetEffectDefenseBonus() + caller.GetBadgeDefenseBonus();
        caller.DealDamage(caller.curTarget, Mathf.CeilToInt(sd / 2f) + 4 + defBonus, 0, 0, BattleHelper.ContactLevel.Contact);
    }

    public override string GetHighlightText(BattleEntity caller, BattleEntity target, int level = 1)
    {
        if (caller is PlayerEntity pcallerA)
        {
            if (!pcallerA.BadgeEquipped(Badge.BadgeType.PowerSight))
            {
                return "";
            }
        }

        int sd = 2;

        if (caller is PlayerEntity pcaller)
        {
            sd = pcaller.GetStompDamage();
        }

        int val = 0;
        int defBonus = caller.GetEffectDefenseBonus() + caller.GetBadgeDefenseBonus();
        val = caller.DealDamageCalculation(target, sd + 4 + defBonus, 0, 0);


        return val + "";
    }
}

public class LM_ElementalStomp : LunaMove
{
    public LM_ElementalStomp()
    {
    }

    public override int GetTextIndex()
    {
        return 9;
    }
    public override string GetName() => GetNameWithIndex(GetTextIndex());
    public override string GetDescription(int level = 1) => GetDescriptionWithIndex(GetTextIndex(), level);
    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemyTopmost, false);
    //public override float GetBasePower() => 1.0f;
    public override int GetBaseCost() => 8;

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        //Debug.Log(caller.id + " jump");
        if (!BattleControl.Instance.EntityValid(caller.curTarget))
        {
            caller.curTarget = null;
        }

        if (caller.curTarget != null)
        {
            int sd = 2;

            AC_Jump actionCommand = null;
            if (caller is PlayerEntity pcaller) //we have technology
            {
                sd = pcaller.GetStompDamage();
                actionCommand = gameObject.AddComponent<AC_Jump>();
                actionCommand.Init(pcaller);
                actionCommand.Setup(0.65f);
            }

            Vector3 tpos = caller.curTarget.ApplyScaledOffset(caller.curTarget.stompOffset);
            Vector3 spos = transform.position;

            //yield return StartCoroutine(caller.Squish(0.067f, 0.2f));
            //StartCoroutine(caller.RevertScale(0.1f));
            yield return StartCoroutine(caller.JumpHeavy(tpos, 4, 0.65f, -0.25f));

            bool result = actionCommand == null ? true : actionCommand.GetSuccess();
            if (actionCommand != null)
            {
                actionCommand.End();
                Destroy(actionCommand);
            }

            if (GetOutcome(caller, sd))
            {
                if (result)
                {
                    DealDamageSuccess(caller, sd);
                    //yield return StartCoroutine(caller.Squish(0.067f, 0.2f));
                    //StartCoroutine(caller.RevertScale(0.1f));
                    yield return StartCoroutine(caller.JumpHeavy(caller.homePos, 2, 0.5f, 0.15f));
                }
                else
                {
                    DealDamageFailure(caller, sd);
                    Vector3 targetPos = caller.transform.position;
                    float height = caller.transform.position.y;
                    targetPos += Vector3.down * height + Vector3.left * 0.5f * height;
                    //yield return StartCoroutine(caller.Squish(0.067f, 0.2f));
                    //StartCoroutine(caller.RevertScale(0.1f));
                    yield return StartCoroutine(caller.Jump(targetPos, 0.5f, 0.3f));
                    yield return StartCoroutine(caller.Move(caller.homePos));
                }
            }
            else
            {
                //Miss
                caller.InvokeMissEvents(caller.curTarget);

                //extrapolate the move curve
                yield return StartCoroutine(caller.ExtrapolateJumpHeavy(spos, tpos, 2, 0.5f, -0.25f));
                yield return StartCoroutine(caller.Move(caller.homePos));
            }
        }
        else
        {
            yield return StartCoroutine(caller.JumpHeavy(caller.homePos, 2, 0.5f, 0.15f));
        }
    }

    public bool GetOutcome(BattleEntity caller, int sd)
    {
        return caller.GetAttackHit(caller.curTarget, 0);
    }
    public void DealDamageSuccess(BattleEntity caller, int sd)
    {
        BattleHelper.DamageType best = BattleHelper.DamageType.Normal;
        int bestDamage = 0;

        //remove normal from this list?
        //Elemental stomp is forced to only use elemental damage (normal is "nonelemental")
        BattleHelper.DamageType[] damageList = { BattleHelper.DamageType.Light, BattleHelper.DamageType.Dark, BattleHelper.DamageType.Fire, BattleHelper.DamageType.Earth, BattleHelper.DamageType.Water, BattleHelper.DamageType.Air };

        foreach (BattleHelper.DamageType type in damageList)
        {
            int temp = caller.DealDamageCalculation(caller.curTarget, sd + 2, type, 0);

            //one evaluated later wins
            //(note that this is roughly in order of which types are most likely to be highest)
            if (temp >= bestDamage)
            {
                bestDamage = temp;
                best = type;
            }
        }

        ulong propertyBlock = (ulong)BattleHelper.DamageProperties.AC_Success;
        caller.DealDamage(caller.curTarget, sd + 2, best, propertyBlock, BattleHelper.ContactLevel.Contact);
    }
    public void DealDamageFailure(BattleEntity caller, int sd)
    {
        BattleHelper.DamageType best = BattleHelper.DamageType.Normal;
        int bestDamage = 0;

        //remove normal from this list?
        //Elemental stomp is forced to only use elemental damage (normal is "nonelemental")
        BattleHelper.DamageType[] damageList = { BattleHelper.DamageType.Light, BattleHelper.DamageType.Dark, BattleHelper.DamageType.Fire, BattleHelper.DamageType.Earth, BattleHelper.DamageType.Water, BattleHelper.DamageType.Air };

        foreach (BattleHelper.DamageType type in damageList)
        {
            int temp = caller.DealDamageCalculation(caller.curTarget, sd + 2, type, 0);

            if (temp >= bestDamage)
            {
                bestDamage = temp;
                best = type;
            }
        }

        caller.DealDamage(caller.curTarget, Mathf.CeilToInt(sd / 2f) + 2, best, 0, BattleHelper.ContactLevel.Contact);
    }

    public override string GetHighlightText(BattleEntity caller, BattleEntity target, int level = 1)
    {
        if (caller is PlayerEntity pcallerA)
        {
            if (!pcallerA.BadgeEquipped(Badge.BadgeType.PowerSight))
            {
                return "";
            }
        }

        int sd = 2;

        if (caller is PlayerEntity pcaller)
        {
            sd = pcaller.GetStompDamage();
        }

        BattleHelper.DamageType best = BattleHelper.DamageType.Normal;
        int bestDamage = 0;

        //remove normal from this list?
        BattleHelper.DamageType[] damageList = { BattleHelper.DamageType.Light, BattleHelper.DamageType.Dark, BattleHelper.DamageType.Fire, BattleHelper.DamageType.Earth, BattleHelper.DamageType.Water, BattleHelper.DamageType.Air, BattleHelper.DamageType.Normal };

        foreach (BattleHelper.DamageType type in damageList)
        {
            int temp = caller.DealDamageCalculation(target, sd + 2, type, 0);

            if (temp >= bestDamage)
            {
                bestDamage = temp;
                best = type;
            }
        }

        int val = caller.DealDamageCalculation(target, sd + 2, best, 0);

        string outstring = best == BattleHelper.DamageType.Normal ? "0" : best + ": " + val;

        return outstring;
    }
}
public class LM_TeamThrow : LunaMove
{
    public LM_TeamThrow()
    {
    }

    public override int GetTextIndex()
    {
        return 10;
    }
    public override string GetName() => GetNameWithIndex(GetTextIndex());
    public override string GetDescription(int level = 1) => GetDescriptionWithIndex(GetTextIndex(), level);
    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemy, false);
    //public override float GetBasePower() => 1.0f;
    public override int GetBaseCost() => 12;

    public override int GetCost(BattleEntity caller, int level = 1)
    {
        return CostCalculation(caller, level, 4);
    }

    /*
    public override int GetMaxLevel(BattleEntity caller)
    {
        return 2;
    }
    */

    public override bool CanChoose(BattleEntity caller, int level = 1)
    {
        List<PlayerEntity> players = BattleControl.Instance.GetPlayerEntities();
        if (players.Count <= 1)
        {
            return false;
        }
        for (int i = 0; i < players.Count; i++)
        {
            if (!players[i].CanBlock())
            {
                return false;
            }
        }

        return base.CanChoose(caller, level);
    }

    public override CantMoveReason GetCantMoveReason(BattleEntity caller, int level = 1)
    {
        List<PlayerEntity> players = BattleControl.Instance.GetPlayerEntities();
        if (players.Count <= 1)
        {
            return CantMoveReason.TeamMoveNoTeammate;
        }
        for (int i = 0; i < players.Count; i++)
        {
            if (!players[i].CanBlock())
            {
                return CantMoveReason.TeamMoveUnavailableTeammate;
            }
        }

        return base.GetCantMoveReason(caller);
    }

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        //Debug.Log(caller.id + " jump");
        if (!BattleControl.Instance.EntityValid(caller.curTarget))
        {
            caller.curTarget = null;
        }

        if (caller.curTarget != null)
        {
            //Grab the other player
            PlayerEntity other = null;
            List<PlayerEntity> players = BattleControl.Instance.GetPlayerEntities();
            for (int i = 0; i < players.Count; i++)
            {
                if (players[i] != caller)
                {
                    other = players[i];
                    break;
                }
            }

            int sd = 2;

            Vector3 grabPos = caller.transform.position + Vector3.up * 0.5f * caller.height + Vector3.right * (caller.width * 0.5f + other.width);
            yield return StartCoroutine(other.JumpHeavy(grabPos, 2, 0.5f, -0.25f));

            AC_MashLeft actionCommand = null;
            if (caller is PlayerEntity pcaller)
            {
                sd = pcaller.GetStompDamage();
                actionCommand = gameObject.AddComponent<AC_MashLeft>();
                actionCommand.Init(pcaller);
                actionCommand.Setup(1.5f, 8);
            }

            yield return new WaitUntil(() => actionCommand.IsStarted());

            //spinny (but also busy wait for actionCommand.IsComplete())

            float radialVel = 0;
            float radialMax = 25;
            float radialPos = 0;
            while (!actionCommand.IsComplete())
            {
                radialVel = radialMax * actionCommand.GetCompletion();
                radialPos += radialVel;
                if (radialPos > 360)
                {
                    radialPos -= 360;
                }

                caller.SetRotation(Vector3.up * -radialPos);
                other.SetRotation(Vector3.up * (-radialPos + 180));

                float xoffset = (caller.width * 0.5f + other.width);
                Vector3 wPos = Vector3.up * 0.5f * caller.height + Vector3.right * xoffset * Mathf.Cos(radialPos * (Mathf.PI / 180)) + Vector3.forward * xoffset * Mathf.Sin(radialPos * (Mathf.PI / 180));
                other.Warp(caller.transform.position + wPos);

                yield return null;
            }

            //Fix orientation and position (i.e. keep rotating until you are correct)

            while (true)
            {
                if (radialVel < radialMax * 0.05f)
                {
                    radialVel = radialMax * 0.05f;
                }
                //radialVel = radialMax * actionCommand.GetCompletion();
                radialPos += radialVel;
                if (radialPos > 360)
                {
                    radialPos = 0;
                }

                caller.SetRotation(Vector3.up * -radialPos);
                other.SetRotation(Vector3.up * (-radialPos + 180));

                float xoffset2 = (caller.width * 0.5f + other.width);
                Vector3 wPos2 = Vector3.up * 0.5f * caller.height + Vector3.right * xoffset2 * Mathf.Cos(radialPos * (Mathf.PI / 180)) + Vector3.forward * xoffset2 * Mathf.Sin(radialPos * (Mathf.PI / 180));
                other.Warp(caller.transform.position + wPos2);

                yield return null;

                if (radialPos == 0)
                {
                    break;
                }
            }

            caller.SetRotation(Vector3.zero);
            other.SetRotation(Vector3.zero);
            float xoffset3 = (caller.width * 0.5f + other.width);
            Vector3 wPos3 = Vector3.up * 0.5f * caller.height + Vector3.right * xoffset3;
            other.Warp(caller.transform.position + wPos3);

            //yield return new WaitUntil(() => actionCommand.IsComplete());

            Vector3 tpos = caller.curTarget.ApplyScaledOffset(caller.curTarget.kickOffset) + (other.width / 2) * Vector3.left;
            Vector3 tpos2 = grabPos + 1.5f * (tpos - grabPos);
            //Vector3 spos = transform.position;

            //yield return StartCoroutine(caller.Squish(0.067f, 0.2f));
            //StartCoroutine(caller.RevertScale(0.1f));

            bool result = actionCommand == null ? true : actionCommand.GetSuccess();
            float completion = actionCommand == null ? 0 : actionCommand.GetCompletion();
            tpos2 = grabPos + 1.5f * (tpos - grabPos) * completion;

            if (actionCommand != null)
            {
                actionCommand.End();
                Destroy(actionCommand);
            }

            if (GetOutcome(caller, other, sd))
            {

                if (result)
                {
                    //Second AC
                    AC_Jump actionCommand2 = null;
                    if (other is PlayerEntity pcaller2) //we have technology
                    {
                        sd = pcaller2.GetStompDamage();
                        actionCommand2 = gameObject.AddComponent<AC_Jump>();
                        actionCommand2.Init(pcaller2);
                        actionCommand2.Setup(other.GetMoveTime(tpos, 15));
                    }

                    yield return StartCoroutine(other.Move(tpos, 20));

                    bool result2 = actionCommand2 == null ? true : actionCommand2.GetSuccess();
                    if (actionCommand2 != null)
                    {
                        actionCommand2.End();
                        Destroy(actionCommand2);
                    }

                    if (result2)
                    {
                        DealDamageSuccess(caller, other, sd, result);
                        //yield return StartCoroutine(caller.Squish(0.067f, 0.2f));
                        //StartCoroutine(caller.RevertScale(0.1f));
                        yield return StartCoroutine(other.JumpHeavy(other.homePos, 2, 0.5f, 0.15f));
                    }
                    else
                    {
                        DealDamageFailure(caller, other, sd, result);
                        Vector3 targetPos = other.transform.position;
                        float height = other.transform.position.y;
                        targetPos += Vector3.down * height + Vector3.left * 0.5f * height;
                        //yield return StartCoroutine(caller.Squish(0.067f, 0.2f));
                        //StartCoroutine(caller.RevertScale(0.1f));
                        yield return StartCoroutine(other.Jump(targetPos, 0.5f, 0.3f));
                        yield return StartCoroutine(other.Move(other.homePos));
                    }

                }
                else
                {
                    //Fly off into space
                    //extrapolate the move curve
                    //yield return StartCoroutine(other.Move(tpos2, 15));

                    //other.transform.position = other.transform.position - other.transform.position.y * Vector3.up;
                    //fall down
                    float failHeight = (tpos2 - grabPos).y * 2.5f;
                    float failWidth = (tpos2 - grabPos).x * 1;

                    //Vector3 tpos3 = Vector3.right * (failWidth + other.transform.position.x) + (other.transform.position.y + failHeight) * Vector3.up;
                    Vector3 tpos4 = Vector3.right * (failWidth * 2 + tpos2.x);

                    if (failWidth > 3)
                    {
                        yield return StartCoroutine(other.Jump(tpos4, failHeight, 1f * 1.5f));
                    }
                    else
                    {
                        if (failWidth < 0.5f)
                        {
                            yield return StartCoroutine(other.Jump(tpos4, failHeight, 1f * (0.5f / 2)));
                        }
                        else
                        {
                            yield return StartCoroutine(other.Jump(tpos4, failHeight, 1f * (failWidth / 2)));
                        }
                    }

                    yield return StartCoroutine(other.Move(other.homePos));
                }
            }
            else
            {
                //Fly off into space
                //extrapolate the move curve
                //yield return StartCoroutine(other.Move(tpos2, 15));

                //other.transform.position = other.transform.position - other.transform.position.y * Vector3.up;
                //fall down
                float failHeight = (tpos2 - grabPos).y * 2.5f;
                float failWidth = (tpos2 - grabPos).x * 1;

                //Vector3 tpos3 = Vector3.right * (failWidth + other.transform.position.x) + (other.transform.position.y + failHeight) * Vector3.up;
                Vector3 tpos4 = Vector3.right * (failWidth * 2 + tpos2.x);

                bool move = true;
                IEnumerator MoveTracked()
                {
                    if (failWidth > 3)
                    {
                        yield return StartCoroutine(other.Jump(tpos4, failHeight, 1f * 1.5f));
                    }
                    else
                    {
                        if (failWidth < 0.5f)
                        {
                            yield return StartCoroutine(other.Jump(tpos4, failHeight, 1f * (0.5f / 2)));
                        }
                        else
                        {
                            yield return StartCoroutine(other.Jump(tpos4, failHeight, 1f * (failWidth / 2)));
                        }
                    }
                    move = false;
                }

                StartCoroutine(MoveTracked());

                bool miss = true;

                while (move)
                {
                    if (miss && caller.curTarget.homePos.x <= other.transform.position.x)
                    {
                        miss = false;
                        other.InvokeMissEvents(caller.curTarget);
                    }
                    
                    yield return null;
                }

                yield return StartCoroutine(other.Move(other.homePos));
            }

            caller.InflictEffectForce(other, new Effect(Effect.EffectType.Cooldown, 1, 255));
            other.actionCounter++;
        }
        else
        {
            yield return StartCoroutine(caller.JumpHeavy(caller.homePos, 2, 0.5f, 0.15f));
        }
    }

    public virtual bool GetOutcome(BattleEntity caller, BattleEntity other, int sd)
    {
        return caller.GetAttackHit(caller.curTarget, 0);
    }
    public virtual void DealDamageSuccess(BattleEntity caller, BattleEntity other, int sd, bool result)
    {
        //calc this from scratch
        int a = 0; // ((PlayerEntity)other).GetStompDamage() + 8;
        int b = 0; // ((PlayerEntity)caller).GetStompDamage() + 8;
        switch (level)
        {
            case 1:
                a = ((PlayerEntity)other).GetStompDamage() + 8;
                b = ((PlayerEntity)caller).GetStompDamage() + 8;
                break;
            case 2:
                a = ((PlayerEntity)other).GetStompDamage() + 11;
                b = ((PlayerEntity)caller).GetStompDamage() + 11;
                break;
        }
        ulong propertyBlock = (ulong)BattleHelper.DamageProperties.AC_Success;
        other.DealDamagePooled(caller, caller.curTarget, a, b, BattleHelper.DamageType.Normal, propertyBlock, BattleHelper.ContactLevel.Contact);
    }
    public virtual void DealDamageFailure(BattleEntity caller, BattleEntity other, int sd, bool result)
    {
        int a = 0; // Mathf.CeilToInt(((PlayerEntity)other).GetStompDamage() / 2f) + 8;
        int b = 0; // ((PlayerEntity)caller).GetStompDamage() + 8;
        switch (level)
        {
            case 1:
                a = Mathf.CeilToInt(((PlayerEntity)other).GetStompDamage() / 2f) + 8;
                b = ((PlayerEntity)caller).GetStompDamage() + 8;
                break;
            case 2:
                a = Mathf.CeilToInt(((PlayerEntity)other).GetStompDamage() / 2f) + 11;
                b = ((PlayerEntity)caller).GetStompDamage() + 11;
                break;
        }
        other.DealDamagePooled(caller, caller.curTarget, a, b, BattleHelper.DamageType.Normal, 0, BattleHelper.ContactLevel.Contact);
    }

    public override string GetHighlightText(BattleEntity caller, BattleEntity target, int level = 1)
    {
        if (caller is PlayerEntity pcallerA)
        {
            if (!pcallerA.BadgeEquipped(Badge.BadgeType.PowerSight))
            {
                return "";
            }
        }

        //Grab the other player
        PlayerEntity other = null;
        List<PlayerEntity> players = BattleControl.Instance.GetPlayerEntities();
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i] != caller)
            {
                other = players[i];
                break;
            }
        }

        int sd = 2;

        if (caller is PlayerEntity pcaller)
        {
            sd = pcaller.GetStompDamage();
        }

        int sdo = 2;
        if (other is PlayerEntity pother)
        {
            sdo = pother.GetStompDamage();
        }

        string outstring = "";

        int val = 0;

        //calc this from scratch
        int a = 0;
        int b = 0;
        switch (level)
        {
            case 1:
                a = ((PlayerEntity)other).GetStompDamage() + 8;
                b = ((PlayerEntity)caller).GetStompDamage() + 8;
                break;
            case 2:
                a = ((PlayerEntity)other).GetStompDamage() + 11;
                b = ((PlayerEntity)caller).GetStompDamage() + 11;
                break;
        }

        val = other.DealDamagePooledCalculation(caller, target, a, b, BattleHelper.DamageType.Normal, 0);

        outstring = val + "";

        return outstring;
    }
}

public class LM_DoubleEgg : LunaMove
{
    public LM_DoubleEgg()
    {
    }

    public override int GetTextIndex()
    {
        return 11;
    }
    public override string GetName() => GetNameWithIndex(GetTextIndex());
    public override string GetDescription(int level = 1) => GetDescriptionWithIndex(GetTextIndex(), level);
    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.Self, false);
    //public override float GetBasePower() => 0.5f;
    public override int GetBaseCost() => 1;

    public override int GetCost(BattleEntity caller, int level = 1)
    {
        return CostCalculation(caller, level, 3);
    }

    public override bool CanChoose(BattleEntity caller, int level = 1)
    {
        if (BattleControl.Instance.playerData.itemInventory.Count >= BattleControl.Instance.playerData.GetMaxInventorySize())
        {
            return false;
        }

        return base.CanChoose(caller, level);
    }

    public override CantMoveReason GetCantMoveReason(BattleEntity caller, int level = 1)
    {
        if (BattleControl.Instance.playerData.itemInventory.Count >= BattleControl.Instance.playerData.GetMaxInventorySize())
        {
            return CantMoveReason.FullItems;
        }

        return base.GetCantMoveReason(caller);
    }

    /*
    public override int GetMaxLevel(BattleEntity caller)
    {
        return 3;
    }
    */

    public IEnumerator EggAnimation(BattleEntity caller, Item.ItemType eggGenerated)
    {
        Debug.Log("Egg anim " + eggGenerated);
        //spawn a sprite
        Sprite isp = GlobalItemScript.GetItemSprite(eggGenerated);


        Vector2 startPosition = caller.transform.position + caller.height * Vector3.up;
        Vector3 endPosition = caller.transform.position + caller.height * Vector3.up + Vector3.up * 0.75f;

        GameObject so = new GameObject("Egg Sprite");
        so.transform.parent = BattleControl.Instance.transform;
        SpriteRenderer s = so.AddComponent<SpriteRenderer>();
        s.sprite = isp;
        so.transform.position = startPosition;

        IEnumerator PosLerp(GameObject o, float duration, Vector3 posA, Vector3 posB)
        {
            float time = 0;

            while (time < 1)
            {
                time += Time.deltaTime / duration;
                o.transform.position = posA + (posB - posA) * (1 - (1 - time) * (1 - time));
                yield return null;
            }

            o.transform.position = posB;
        }

        yield return StartCoroutine(PosLerp(so, 0.4f, startPosition, endPosition));
        //yield return new WaitForSeconds(1f);

        Destroy(so);

        yield return null;
    }


    public Item.ItemType GetEggType(BattleEntity caller, int turnOffset, int offset, bool fail = false)
    {
        //do psuedo random things
        int radix = caller.hp * 3 + turnOffset * 7 + offset + caller.actionCounter;
        radix %= 12;

        if (fail)
        {
            radix %= 6;
        }

        return (Item.ItemType)(radix + Item.ItemType.SpicyEgg);
    }

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        AC_PressButtonTimed actionCommand = null;
        if (caller is PlayerEntity pcaller) //we have technology
        {
            actionCommand = gameObject.AddComponent<AC_PressButtonTimed>();
            actionCommand.Init(pcaller);
            actionCommand.Setup(0.5f);
        }

        yield return new WaitForSeconds(ActionCommand.FADE_IN_TIME);
        StartCoroutine(caller.Spin(Vector3.up * 360, 0.5f));
        yield return new WaitUntil(() => actionCommand.IsComplete());

        bool result = actionCommand == null ? true : actionCommand.GetSuccess();
        if (actionCommand != null)
        {
            actionCommand.End();
            Destroy(actionCommand);
        }

        Item.ItemType eggType = Item.ItemType.None;
        Item.ItemType eggTypeB = Item.ItemType.None;
        if (result)
        {
            BattleControl.Instance.CreateActionCommandEffect(BattleHelper.ActionCommandText.Nice, caller.GetDamageEffectPosition(), caller);
            yield return StartCoroutine(caller.JumpHeavy(caller.homePos, 1.5f, 0.3f, 0.15f));

            eggType = GetEggType(caller, BattleControl.Instance.turnCount, 0, false);
            eggTypeB = GetEggType(caller, BattleControl.Instance.turnCount, 2, false);
        }
        else
        {
            yield return StartCoroutine(caller.JumpHeavy(caller.homePos, 0.5f, 0.15f, 0.15f));
            eggType = GetEggType(caller, BattleControl.Instance.turnCount, 0, true);
            eggTypeB = GetEggType(caller, BattleControl.Instance.turnCount, 2, true);
        }

        yield return StartCoroutine(EggAnimation(caller, eggType));
        BattleControl.Instance.playerData.AddItem(new Item(eggType, Item.ItemModifier.None, Item.ItemOrigin.Egg, 0, 0));

        if (BattleControl.Instance.playerData.itemInventory.Count >= BattleControl.Instance.playerData.GetMaxInventorySize())
        {
            yield break;
        }

        yield return StartCoroutine(EggAnimation(caller, eggTypeB));
        BattleControl.Instance.playerData.AddItem(new Item(eggTypeB, Item.ItemModifier.None, Item.ItemOrigin.Egg, 0, 0));
    }

    public override void PreMove(BattleEntity caller, int level = 1)
    {
    }
    public override void PostMove(BattleEntity caller, int level = 1)
    {
    }
}




public class LM_Smash : LunaMove
{
    public LM_Smash()
    {
    }

    public override int GetTextIndex()
    {
        return 12;
    }
    public override string GetName() => GetNameWithIndex(GetTextIndex());
    public override string GetDescription(int level = 1) => GetDescriptionWithIndex(GetTextIndex(), level);
    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemyLowFrontmost, false);
    //public override float GetBasePower() => 1.5f;
    public override int GetBaseCost() => 0;

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        //Debug.Log(caller.id + " fly");
        if (!BattleControl.Instance.EntityValid(caller.curTarget))
        {
            caller.curTarget = null;
        }

        if (caller.curTarget == null)
        {
            yield break;
        }

        Vector3 target = caller.curTarget.ApplyScaledOffset(caller.curTarget.hammerOffset);
        target += Vector3.left * 0.5f * caller.width + 0.5f * Vector3.left;
        yield return StartCoroutine(caller.Move(target));

        if (caller.curTarget != null)
        {
            int sd = 2;
            int sl = 0;
            AC_HoldLeft actionCommand = null;
            if (caller is PlayerEntity pcaller)
            {
                sd = pcaller.GetWeaponDamage();
                sl = pcaller.GetWeaponLevel();
                actionCommand = gameObject.AddComponent<AC_HoldLeft>();
                actionCommand.Init(pcaller);
                actionCommand.Setup(ActionCommandTime());
            }

            yield return new WaitUntil(() => actionCommand.IsStarted());
            yield return new WaitUntil(() => actionCommand.IsComplete());

            bool result = actionCommand == null ? true : actionCommand.GetSuccess();
            if (actionCommand != null)
            {
                actionCommand.End();
                Destroy(actionCommand);
            }

            yield return StartCoroutine(SwingAnimations(caller, sl, level));
            if (GetOutcome(caller))
            {
                DealDamage(caller, sd, result);
            }
            else
            {
                //Miss
                caller.InvokeMissEvents(caller.curTarget);
            }
            yield return new WaitForSeconds(0.1f);
        }
        yield return StartCoroutine(caller.Move(caller.homePos));
    }

    public virtual float ActionCommandTime()
    {
        return 0.5f;
    }
    public virtual IEnumerator SwingAnimations(BattleEntity caller, int sl, int level = 1)
    {
        GameObject eoS = null;
        switch (sl)
        {
            default:
            case 0:
                eoS = Instantiate(Resources.Load<GameObject>("VFX/Overworld/Player/Effect_HammerSwoosh0"), BattleControl.Instance.transform);
                break;
            case 1:
                eoS = Instantiate(Resources.Load<GameObject>("VFX/Overworld/Player/Effect_HammerSwoosh1"), BattleControl.Instance.transform);
                break;
            case 2:
                eoS = Instantiate(Resources.Load<GameObject>("VFX/Overworld/Player/Effect_HammerSwoosh2"), BattleControl.Instance.transform);
                break;
        }
        eoS.transform.position = transform.position + Vector3.up * 0.325f;

        yield return new WaitForSeconds(0.2f);
        //Impact effect
        GameObject eoShockwave;
        switch (sl)
        {
            default:
            case 0:
                eoShockwave = Instantiate(Resources.Load<GameObject>("VFX/Overworld/Player/Effect_HammerShockwave0"), BattleControl.Instance.transform);
                break;
            case 1:
                eoShockwave = Instantiate(Resources.Load<GameObject>("VFX/Overworld/Player/Effect_HammerShockwave1"), BattleControl.Instance.transform);
                break;
            case 2:
                eoShockwave = Instantiate(Resources.Load<GameObject>("VFX/Overworld/Player/Effect_HammerShockwave2"), BattleControl.Instance.transform);
                break;
        }
        eoShockwave.transform.position = transform.position + Vector3.right * 0.7f;
    }
    public virtual bool GetOutcome(BattleEntity caller)
    {
        return caller.GetAttackHit(caller.curTarget, 0);
    }
    public virtual void DealDamage(BattleEntity caller, int sd, bool result)
    {
        ulong propertyBlock = (ulong)BattleHelper.DamageProperties.PlusOneOnBuff;
        if (result)
        {
            propertyBlock |= (ulong)BattleHelper.DamageProperties.AC_Success;
            caller.DealDamage(caller.curTarget, sd, BattleHelper.DamageType.Normal, propertyBlock, BattleHelper.ContactLevel.Weapon);
        }
        else
        {
            caller.DealDamage(caller.curTarget, Mathf.CeilToInt(sd / 2f), BattleHelper.DamageType.Normal, propertyBlock, BattleHelper.ContactLevel.Weapon);
        }
    }

    public override void PreMove(BattleEntity caller, int level = 1)
    {

    }
    public override void PostMove(BattleEntity caller, int level = 1)
    {
    }

    public override string GetHighlightText(BattleEntity caller, BattleEntity target, int level = 1)
    {
        if (caller is PlayerEntity pcallerA)
        {
            if (!pcallerA.BadgeEquipped(Badge.BadgeType.PowerSight))
            {
                return "";
            }
        }

        int sd = 2;

        if (caller is PlayerEntity pcaller)
        {
            sd = pcaller.GetWeaponDamage();
        }

        ulong propertyBlock = (ulong)BattleHelper.DamageProperties.PlusOneOnBuff;

        int val = caller.DealDamageCalculation(target, sd, 0, propertyBlock);

        return val + "";
    }
}

public class LM_PowerSmash : LM_Smash
{
    public LM_PowerSmash()
    {
    }

    public override int GetTextIndex()
    {
        return 13;
    }
    public override string GetName() => GetNameWithIndex(GetTextIndex());
    public override string GetDescription(int level = 1) => GetDescriptionWithIndex(GetTextIndex(), level);
    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemyLowFrontmost, false);
    //public override float GetBasePower() => 1.5f;
    public override int GetBaseCost() => 3;

    public override int GetCost(BattleEntity caller, int level = 1)
    {
        return CostCalculation(caller, level, 3);
    }

    /*
    public override int GetMaxLevel(BattleEntity caller)
    {
        return 3;
    }
    */

    public override float ActionCommandTime()
    {
        return 0.5f;
    }
    public override bool GetOutcome(BattleEntity caller)
    {
        return caller.GetAttackHit(caller.curTarget, 0);
    }
    public override void DealDamage(BattleEntity caller, int sd, bool result)
    {
        if (result)
        {
            ulong propertyBlock = (ulong)BattleHelper.DamageProperties.AC_Success;
            switch (level)
            {
                case 1:
                    caller.DealDamage(caller.curTarget, sd + 2, BattleHelper.DamageType.Normal, propertyBlock, BattleHelper.ContactLevel.Weapon);
                    break;
                case 2:
                    caller.DealDamage(caller.curTarget, sd + 6, BattleHelper.DamageType.Normal, propertyBlock, BattleHelper.ContactLevel.Weapon);
                    break;
                case 3:
                    caller.DealDamage(caller.curTarget, sd + 10, BattleHelper.DamageType.Normal, propertyBlock, BattleHelper.ContactLevel.Weapon);
                    break;
            }
        }
        else
        {
            switch (level)
            {
                case 1:
                    caller.DealDamage(caller.curTarget, Mathf.CeilToInt(sd / 2f) + 2, BattleHelper.DamageType.Normal, 0, BattleHelper.ContactLevel.Weapon);
                    break;
                case 2:
                    caller.DealDamage(caller.curTarget, Mathf.CeilToInt(sd / 2f) + 6, BattleHelper.DamageType.Normal, 0, BattleHelper.ContactLevel.Weapon);
                    break;
                case 3:
                    caller.DealDamage(caller.curTarget, Mathf.CeilToInt(sd / 2f) + 10, BattleHelper.DamageType.Normal, 0, BattleHelper.ContactLevel.Weapon);
                    break;
            }
        }
    }

    public override void PreMove(BattleEntity caller, int level = 1)
    {

    }
    public override void PostMove(BattleEntity caller, int level = 1)
    {
    }

    public override string GetHighlightText(BattleEntity caller, BattleEntity target, int level = 1)
    {
        if (caller is PlayerEntity pcallerA)
        {
            if (!pcallerA.BadgeEquipped(Badge.BadgeType.PowerSight))
            {
                return "";
            }
        }

        int sd = 2;

        if (caller is PlayerEntity pcaller)
        {
            sd = pcaller.GetWeaponDamage();
        }

        //ulong propertyBlock = (ulong)BattleHelper.DamageProperties.PlusOneOnBuff;

        int val = 0; // caller.DealDamageCalculation(target, sd, 0, propertyBlock);

        switch (level)
        {
            case 1:
                val = caller.DealDamageCalculation(target, sd + 2, BattleHelper.DamageType.Normal, 0);
                break;
            case 2:
                val = caller.DealDamageCalculation(target, sd + 6, BattleHelper.DamageType.Normal, 0);
                break;
            case 3:
                val = caller.DealDamageCalculation(target, sd + 10, BattleHelper.DamageType.Normal, 0);
                break;
        }

        return val + "";
    }
}

public class LM_DazzleSmash : LM_Smash
{
    public LM_DazzleSmash()
    {
    }

    public override int GetTextIndex()
    {
        return 14;
    }
    public override string GetName() => GetNameWithIndex(GetTextIndex());
    public override string GetDescription(int level = 1) => GetDescriptionWithIndex(GetTextIndex(), level);
    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemyLowFrontmost, false);
    //public override float GetBasePower() => 1.5f;
    public override int GetBaseCost() => 4;

    public override float ActionCommandTime()
    {
        return 0.5f;
    }
    public override bool GetOutcome(BattleEntity caller)
    {
        return caller.GetAttackHit(caller.curTarget, 0);
    }
    public override void DealDamage(BattleEntity caller, int sd, bool result)
    {
        if (result)
        {
            ulong propertyBlock = (ulong)BattleHelper.DamageProperties.AC_Success;
            caller.DealDamage(caller.curTarget, sd, BattleHelper.DamageType.Normal, propertyBlock, BattleHelper.ContactLevel.Weapon);
            caller.InflictEffect(caller.curTarget, new Effect(Effect.EffectType.Dizzy, 1, 2), caller.posId);
        }
        else
        {
            caller.DealDamage(caller.curTarget, Mathf.CeilToInt(sd / 2f), BattleHelper.DamageType.Normal, 0, BattleHelper.ContactLevel.Weapon);
            caller.InflictEffect(caller.curTarget, new Effect(Effect.EffectType.Dizzy, 1, 1), caller.posId);
        }
    }

    public override void PreMove(BattleEntity caller, int level = 1)
    {

    }
    public override void PostMove(BattleEntity caller, int level = 1)
    {
    }

    public override string GetHighlightText(BattleEntity caller, BattleEntity target, int level = 1)
    {
        bool damageDisplay = false;
        bool statusDisplay = false;
        if (caller is PlayerEntity pcallerA)
        {
            if (pcallerA.BadgeEquipped(Badge.BadgeType.PowerSight))
            {
                damageDisplay = true;
            }

            if (pcallerA.BadgeEquipped(Badge.BadgeType.StatusSight))
            {
                statusDisplay = true;
            }
        }
        if (!damageDisplay && !statusDisplay)
        {
            return "";
        }

        int sd = 2;

        float statusBoost = 1;
        if (caller is PlayerEntity pcaller)
        {
            sd = pcaller.GetWeaponDamage();
            statusBoost = pcaller.CalculateStatusBoost(target);
        }

        int val = caller.DealDamageCalculation(target, sd, 0, 0);

        int statusHP = (int)(target.StatusWorkingHP(Effect.EffectType.Dizzy) * statusBoost);

        bool doesWork = (target.hp > 0) && (target.hp <= statusHP);


        string outString = "";
        if (damageDisplay)
        {
            outString += val;
        }
        if (statusDisplay)
        {
            if (outString.Length > 0)
            {
                outString += "<line>";
            }
            if (doesWork)
            {
                return outString + "<highlightyescolor>(" + statusHP + ")</highlightyescolor>";
            }
            else
            {
                return outString + "<highlightnocolor>(" + statusHP + ")</highlightnocolor>";
            }
        }

        return outString;
    }
}

public class LM_HammerThrow : LunaMove
{
    public LM_HammerThrow()
    {
    }

    public override int GetTextIndex()
    {
        return 15;
    }
    public override string GetName() => GetNameWithIndex(GetTextIndex());
    public override string GetDescription(int level = 1) => GetDescriptionWithIndex(GetTextIndex(), level);
    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemy, false);
    //public override float GetBasePower() => 1.5f;
    public override int GetBaseCost() => 5;

    public override int GetCost(BattleEntity caller, int level = 1)
    {
        return CostCalculation(caller, level, 3);
    }

    /*
    public override int GetMaxLevel(BattleEntity caller)
    {
        return 2;
    }
    */

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        //Debug.Log(caller.id + " fly");
        if (!BattleControl.Instance.EntityValid(caller.curTarget))
        {
            caller.curTarget = null;
        }

        //Vector3 target = caller.curTarget.transform.position - caller.curTarget.width * Vector3.right;
        //target += Vector3.left * 0.5f;
        //yield return StartCoroutine(caller.Move(target));

        if (caller.curTarget != null)
        {
            int sd = 2;

            AC_HoldLeft actionCommand = null;
            if (caller is PlayerEntity pcaller)
            {
                sd = pcaller.GetWeaponDamage();
                actionCommand = gameObject.AddComponent<AC_HoldLeft>();
                actionCommand.Init(pcaller);
                actionCommand.Setup(0.5f);
            }

            yield return new WaitUntil(() => actionCommand.IsStarted());
            yield return new WaitUntil(() => actionCommand.IsComplete());

            yield return StartCoroutine(caller.Spin(Vector3.up * 360, 0.15f));
            bool result = actionCommand == null ? true : actionCommand.GetSuccess();
            if (actionCommand != null)
            {
                actionCommand.End();
                Destroy(actionCommand);
            }
            if (caller.GetAttackHit(caller.curTarget, 0))
            {
                if (result)
                {
                    ulong propertyBlock = (ulong)BattleHelper.DamageProperties.AC_Success;
                    switch (level)
                    {
                        case 2:
                            caller.DealDamage(caller.curTarget, sd + 7, 0, propertyBlock, BattleHelper.ContactLevel.Infinite);
                            break;
                        case 1:
                            caller.DealDamage(caller.curTarget, sd + 2, 0, propertyBlock, BattleHelper.ContactLevel.Infinite);
                            break;
                    }
                }
                else
                {
                    switch (level)
                    {
                        case 2:
                            caller.DealDamage(caller.curTarget, Mathf.CeilToInt(sd / 2f) + 7, 0, 0, BattleHelper.ContactLevel.Infinite);
                            break;
                        case 1:
                            caller.DealDamage(caller.curTarget, Mathf.CeilToInt(sd / 2f) + 2, 0, 0, BattleHelper.ContactLevel.Infinite);
                            break;
                    }
                }
            }
            else
            {
                //Miss
                caller.InvokeMissEvents(caller.curTarget);
                yield return StartCoroutine(caller.Spin(Vector3.up * 360, 0.15f));
            }
        }
        //yield return StartCoroutine(caller.Move(caller.homePos));
    }

    public override void PreMove(BattleEntity caller, int level = 1)
    {

    }
    public override void PostMove(BattleEntity caller, int level = 1)
    {
    }

    public override string GetHighlightText(BattleEntity caller, BattleEntity target, int level = 1)
    {
        if (caller is PlayerEntity pcallerA)
        {
            if (!pcallerA.BadgeEquipped(Badge.BadgeType.PowerSight))
            {
                return "";
            }
        }

        int sd = 2;

        if (caller is PlayerEntity pcaller)
        {
            sd = pcaller.GetWeaponDamage();
        }

        //ulong propertyBlock = (ulong)BattleHelper.DamageProperties.PlusOneOnBuff;

        int val = 0; // caller.DealDamageCalculation(target, sd, 0, propertyBlock);

        switch (level)
        {
            case 1:
                val = caller.DealDamageCalculation(target, sd + 2, BattleHelper.DamageType.Normal, 0);
                break;
            case 2:
                val = caller.DealDamageCalculation(target, sd + 7, BattleHelper.DamageType.Normal, 0);
                break;
        }

        return val + "";
    }
}

public class LM_BreakerSmash : LM_Smash
{
    public LM_BreakerSmash()
    {
    }

    public override int GetTextIndex()
    {
        return 16;
    }
    public override string GetName() => GetNameWithIndex(GetTextIndex());
    public override string GetDescription(int level = 1) => GetDescriptionWithIndex(GetTextIndex(), level);
    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemyLowFrontmost, false);
    //public override float GetBasePower() => 1.5f;
    public override int GetBaseCost() => 6;

    public override int GetCost(BattleEntity caller, int level = 1)
    {
        return CostCalculation(caller, level, 3);
    }

    /*
    public override int GetMaxLevel(BattleEntity caller)
    {
        return 2;
    }
    */

    public override float ActionCommandTime()
    {
        return 0.5f;
    }
    public override bool GetOutcome(BattleEntity caller)
    {
        return caller.GetAttackHit(caller.curTarget, BattleHelper.DamageType.Earth);
    }
    public override void DealDamage(BattleEntity caller, int sd, bool result)
    {
        ulong propertyBlockB = (ulong)(BattleHelper.DamageProperties.AdvancedElementCalc);
        if (result)
        {
            ulong propertyBlock = (ulong)BattleHelper.DamageProperties.AC_Success;
            propertyBlockB |= (ulong)BattleHelper.DamageProperties.AC_Success;
            switch (level)
            {
                case 1:
                    caller.DealDamage(caller.curTarget, sd + 4, BattleHelper.DamageType.Earth, propertyBlock, BattleHelper.ContactLevel.Weapon);
                    break;
                case 2:
                    caller.DealDamage(caller.curTarget, sd + 5, BattleHelper.DamageType.Earth, propertyBlockB, BattleHelper.ContactLevel.Weapon);
                    break;
            }
        }
        else
        {
            switch (level)
            {
                case 1:
                    caller.DealDamage(caller.curTarget, Mathf.CeilToInt(sd / 2f) + 4, BattleHelper.DamageType.Earth, 0, BattleHelper.ContactLevel.Weapon);
                    break;
                case 2:
                    caller.DealDamage(caller.curTarget, Mathf.CeilToInt(sd / 2f) + 5, BattleHelper.DamageType.Earth, propertyBlockB, BattleHelper.ContactLevel.Weapon);
                    break;
            }
        }
    }

    public override IEnumerator SwingAnimations(BattleEntity caller, int sl, int level = 1)
    {
        GameObject eoS = null;
        eoS = Instantiate(Resources.Load<GameObject>("VFX/Battle/Moves/Player/Effect_BreakerSmash"), BattleControl.Instance.transform);
        eoS.transform.position = transform.position + Vector3.up * 0.325f;

        yield return new WaitForSeconds(0.2f);
        //Impact effect
        GameObject eoShockwave;
        eoShockwave = Instantiate(Resources.Load<GameObject>("VFX/Battle/Moves/Player/Effect_BreakerShockwave"), BattleControl.Instance.transform);
        eoShockwave.transform.position = transform.position + Vector3.right * 0.7f;
    }

    public override void PreMove(BattleEntity caller, int level = 1)
    {

    }
    public override void PostMove(BattleEntity caller, int level = 1)
    {
    }

    public override string GetHighlightText(BattleEntity caller, BattleEntity target, int level = 1)
    {
        if (caller is PlayerEntity pcallerA)
        {
            if (!pcallerA.BadgeEquipped(Badge.BadgeType.PowerSight))
            {
                return "";
            }
        }

        int sd = 2;

        if (caller is PlayerEntity pcaller)
        {
            sd = pcaller.GetWeaponDamage();
        }

        //ulong propertyBlock = (ulong)BattleHelper.DamageProperties.PlusOneOnBuff;

        int val = 0; // caller.DealDamageCalculation(target, sd, 0, propertyBlock);

        ulong propertyBlock = (ulong)(BattleHelper.DamageProperties.AdvancedElementCalc);
        switch (level)
        {
            case 1:
                val = caller.DealDamageCalculation(target, sd + 4, BattleHelper.DamageType.Earth, 0);
                break;
            case 2:
                val = caller.DealDamageCalculation(target, sd + 5, BattleHelper.DamageType.Earth, propertyBlock);
                break;
        }

        return val + "";
    }
}

public class LM_FlameSmash : LM_Smash
{
    public LM_FlameSmash()
    {
    }

    public override int GetTextIndex()
    {
        return 17;
    }
    public override string GetName() => GetNameWithIndex(GetTextIndex());
    public override string GetDescription(int level = 1) => GetDescriptionWithIndex(GetTextIndex(), level);
    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemyLowFrontmost, false);
    //public override float GetBasePower() => 1.5f;
    public override int GetBaseCost() => 7;

    public override float ActionCommandTime()
    {
        return 0.5f;
    }
    public override bool GetOutcome(BattleEntity caller)
    {
        return caller.GetAttackHit(caller.curTarget, BattleHelper.DamageType.Fire);
    }
    public override void DealDamage(BattleEntity caller, int sd, bool result)
    {
        if (result)
        {
            ulong propertyBlock = (ulong)BattleHelper.DamageProperties.AC_Success;
            caller.DealDamage(caller.curTarget, sd + 5, BattleHelper.DamageType.Fire, propertyBlock, BattleHelper.ContactLevel.Weapon);
        }
        else
        {
            caller.DealDamage(caller.curTarget, Mathf.CeilToInt(sd / 2f) + 5, BattleHelper.DamageType.Fire, 0, BattleHelper.ContactLevel.Weapon);
        }
    }

    public override IEnumerator SwingAnimations(BattleEntity caller, int sl, int level = 1)
    {
        GameObject eoS = null;
        eoS = Instantiate(Resources.Load<GameObject>("VFX/Battle/Moves/Player/Effect_FlameSmash"), BattleControl.Instance.transform);
        eoS.transform.position = transform.position + Vector3.up * 0.325f;

        yield return new WaitForSeconds(0.2f);
        //Impact effect
        GameObject eoShockwave;
        eoShockwave = Instantiate(Resources.Load<GameObject>("VFX/Battle/Moves/Player/Effect_FlameShockwave"), BattleControl.Instance.transform);
        eoShockwave.transform.position = transform.position + Vector3.right * 0.7f;
    }
    public override void PreMove(BattleEntity caller, int level = 1)
    {

    }
    public override void PostMove(BattleEntity caller, int level = 1)
    {
    }

    public override string GetHighlightText(BattleEntity caller, BattleEntity target, int level = 1)
    {
        if (caller is PlayerEntity pcallerA)
        {
            if (!pcallerA.BadgeEquipped(Badge.BadgeType.PowerSight))
            {
                return "";
            }
        }

        int sd = 2;

        if (caller is PlayerEntity pcaller)
        {
            sd = pcaller.GetWeaponDamage();
        }

        //ulong propertyBlock = (ulong)BattleHelper.DamageProperties.PlusOneOnBuff;

        int val = 0; // caller.DealDamageCalculation(target, sd, 0, propertyBlock);

        val = caller.DealDamageCalculation(target, sd + 5, BattleHelper.DamageType.Fire, 0);

        return val + "";
    }
}

public class LM_MomentumSmash : LM_Smash
{
    public LM_MomentumSmash()
    {
    }

    public override int GetTextIndex()
    {
        return 18;
    }
    public override string GetName() => GetNameWithIndex(GetTextIndex());
    public override string GetDescription(int level = 1) => GetDescriptionWithIndex(GetTextIndex(), level);
    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemyLowFrontmost, false);
    //public override float GetBasePower() => 1.5f;
    public override int GetBaseCost() => 7;

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        //Debug.Log(caller.id + " fly");
        if (!BattleControl.Instance.EntityValid(caller.curTarget))
        {
            caller.curTarget = null;
        }

        Vector3 target = caller.curTarget.ApplyScaledOffset(caller.curTarget.hammerOffset);
        target += Vector3.left * 0.5f * caller.width;
        yield return StartCoroutine(caller.Move(target));

        bool miss = true;
        if (caller.curTarget != null)
        {
            int sd = 2;
            AC_HoldLeft actionCommand = null;
            if (caller is PlayerEntity pcaller)
            {
                sd = pcaller.GetWeaponDamage();
                actionCommand = gameObject.AddComponent<AC_HoldLeft>();
                actionCommand.Init(pcaller);
                actionCommand.Setup(ActionCommandTime());
            }

            yield return new WaitUntil(() => actionCommand.IsStarted());
            yield return new WaitUntil(() => actionCommand.IsComplete());

            bool result = actionCommand == null ? true : actionCommand.GetSuccess();
            if (actionCommand != null)
            {
                actionCommand.End();
                Destroy(actionCommand);
            }
            if (GetOutcome(caller))
            {
                yield return StartCoroutine(caller.Spin(Vector3.up * 360, 0.15f));

                DealDamage(caller, sd, result);
                caller.InflictEffect(caller, new Effect(Effect.EffectType.Absorb, 2, 255));
                miss = false;
            }
            else
            {
                //Miss
                caller.InvokeMissEvents(caller.curTarget);
                yield return StartCoroutine(caller.Spin(Vector3.up * 360, 0.15f));
            }
        }

        if (miss)
        {
            yield return StartCoroutine(caller.Move(caller.homePos));
        }
        else
        {
            List<BattleEntity> playerParty = BattleControl.Instance.GetEntities((e) => e.posId < 0);

            //Store the info for the first entity on the list (for later)
            Vector3 tempHPos = playerParty[0].homePos;
            Vector3 tempPos = playerParty[0].transform.position;
            //Not swapping posIds makes things still work
            int tempID = playerParty[0].posId;

            BattleControl.Instance.SwapEffectCasters(playerParty);

            for (int i = 0; i < playerParty.Count; i++)
            {
                //Debug.Log("[" + i + "] " + playerParty[i].homePos + " " + playerParty[i].posId);
                if (i == playerParty.Count - 1)
                {
                    playerParty[i].homePos = tempHPos;
                    //playerParty[i].transform.position = tempPos;
                    playerParty[i].posId = tempID;
                }
                else
                {
                    playerParty[i].homePos = playerParty[i + 1].homePos;
                    //playerParty[i].transform.position = playerParty[i + 1].transform.position;
                    playerParty[i].posId = playerParty[i + 1].posId;
                }
                //Debug.Log("[" + i + "] " + playerParty[i].homePos + " " + playerParty[i].posId);
                if (playerParty[i] != caller)
                {
                    StartCoroutine(playerParty[i].Move(playerParty[i].homePos, 15));
                }
            }
            yield return StartCoroutine(caller.JumpHeavy(caller.homePos, 2, 0.5f, 0.15f));
        }
    }


    public override float ActionCommandTime()
    {
        return 0.5f;
    }
    public override bool GetOutcome(BattleEntity caller)
    {
        return caller.GetAttackHit(caller.curTarget, 0);
    }
    public override void DealDamage(BattleEntity caller, int sd, bool result)
    {
        ulong propertyBlock = (ulong)BattleHelper.DamageProperties.MetaKnockback;
        if (result)
        {
            propertyBlock |= (ulong)BattleHelper.DamageProperties.AC_Success;
            caller.DealDamage(caller.curTarget, sd + 2, BattleHelper.DamageType.Normal, propertyBlock, BattleHelper.ContactLevel.Weapon);
        }
        else
        {
            caller.DealDamage(caller.curTarget, Mathf.CeilToInt(sd / 2f) + 2, BattleHelper.DamageType.Normal, propertyBlock, BattleHelper.ContactLevel.Weapon);
        }
    }

    public override void PreMove(BattleEntity caller, int level = 1)
    {

    }
    public override void PostMove(BattleEntity caller, int level = 1)
    {
    }

    public override string GetHighlightText(BattleEntity caller, BattleEntity target, int level = 1)
    {
        if (caller is PlayerEntity pcallerA)
        {
            if (!pcallerA.BadgeEquipped(Badge.BadgeType.PowerSight))
            {
                return "";
            }
        }

        int sd = 2;

        if (caller is PlayerEntity pcaller)
        {
            sd = pcaller.GetWeaponDamage();
        }


        ulong properties = (ulong)BattleHelper.DamageProperties.MetaKnockback;

        int val = caller.DealDamageCalculation(target, sd + 2, 0, properties);


        return val + "";
    }
}
public class LM_QuakeSmash : LunaMove
{
    public LM_QuakeSmash()
    {
    }

    public override int GetTextIndex()
    {
        return 19;
    }
    public override string GetName() => GetNameWithIndex(GetTextIndex());
    public override string GetDescription(int level = 1) => GetDescriptionWithIndex(GetTextIndex(), level);
    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemyGrounded, true);
    //public override float GetBasePower() => 1.5f;
    public override int GetBaseCost() => 8;

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        //Debug.Log(caller.id + " fly");

        //get list of entities to hit
        List<BattleEntity> targets = BattleControl.Instance.GetEntitiesSorted(caller, GetBaseTarget());

        int sd = 2;
        int sl = 0;
        AC_HoldLeft actionCommand = null;
        if (caller is PlayerEntity pcaller)
        {
            sd = pcaller.GetWeaponDamage();
            sl = pcaller.GetWeaponLevel();
            actionCommand = gameObject.AddComponent<AC_HoldLeft>();
            actionCommand.Init(pcaller);
            actionCommand.Setup(0.8f);
        }

        yield return new WaitUntil(() => actionCommand.IsStarted());
        yield return new WaitUntil(() => actionCommand.IsComplete());

        yield return StartCoroutine(SwingAnimations(caller, sl, level));
        bool result = actionCommand == null ? true : actionCommand.GetSuccess();
        if (actionCommand != null)
        {
            actionCommand.End();
            Destroy(actionCommand);
        }
        MainManager.Instance.StartCoroutine(MainManager.Instance.CameraShake1D(Vector3.up, 0.3f, 0.5f));
        int count = 0;
        foreach (BattleEntity target in targets)
        {
            if (caller.GetAttackHit(target, 0))
            {
                count++;
                ulong propertyBlock = 0;
                if (count == 1)
                {
                    propertyBlock = (ulong)BattleHelper.DamageProperties.AC_Success;
                } else
                {
                    propertyBlock = (ulong)BattleHelper.DamageProperties.AC_SuccessStall;
                }
                if (result)
                {
                    caller.DealDamage(target, sd - 1, BattleHelper.DamageType.Earth, propertyBlock, BattleHelper.ContactLevel.Infinite);
                }
                else
                {
                    caller.DealDamage(target, Mathf.CeilToInt((sd - 1) / 2f), BattleHelper.DamageType.Earth, 0, BattleHelper.ContactLevel.Infinite);
                }
            } else
            {
                caller.InvokeMissEvents(target);
            }
        }


        yield return StartCoroutine(caller.Move(caller.homePos));
    }

    public virtual IEnumerator SwingAnimations(BattleEntity caller, int sl, int level = 1)
    {
        GameObject eoS = null;
        switch (sl)
        {
            default:
            case 0:
                eoS = Instantiate(Resources.Load<GameObject>("VFX/Overworld/Player/Effect_HammerSwoosh0"), BattleControl.Instance.transform);
                break;
            case 1:
                eoS = Instantiate(Resources.Load<GameObject>("VFX/Overworld/Player/Effect_HammerSwoosh1"), BattleControl.Instance.transform);
                break;
            case 2:
                eoS = Instantiate(Resources.Load<GameObject>("VFX/Overworld/Player/Effect_HammerSwoosh2"), BattleControl.Instance.transform);
                break;
        }
        eoS.transform.position = transform.position + Vector3.up * 0.325f;

        yield return new WaitForSeconds(0.2f);
        //Impact effect
        GameObject eoShockwave;
        eoShockwave = Instantiate(Resources.Load<GameObject>("VFX/Battle/Moves/Player/Effect_QuakeShockwave"), BattleControl.Instance.transform);
        eoShockwave.transform.position = transform.position + Vector3.right * 0.7f + Vector3.down * 0.2f;
    }

    public override void PreMove(BattleEntity caller, int level = 1)
    {

    }
    public override void PostMove(BattleEntity caller, int level = 1)
    {
    }

    public override string GetHighlightText(BattleEntity caller, BattleEntity target, int level = 1)
    {
        if (caller is PlayerEntity pcallerA)
        {
            if (!pcallerA.BadgeEquipped(Badge.BadgeType.PowerSight))
            {
                return "";
            }
        }

        int sd = 2;

        if (caller is PlayerEntity pcaller)
        {
            sd = pcaller.GetWeaponDamage();
        }

        //ulong propertyBlock = (ulong)BattleHelper.DamageProperties.PlusOneOnBuff;

        int val = 0; // caller.DealDamageCalculation(target, sd, 0, propertyBlock);

        val = caller.DealDamageCalculation(target, sd - 1, BattleHelper.DamageType.Earth, 0);

        return val + "?";
    }
}

public class LM_LightSmash : LM_Smash
{
    public LM_LightSmash()
    {
    }

    public override int GetTextIndex()
    {
        return 20;
    }
    public override string GetName() => GetNameWithIndex(GetTextIndex());
    public override string GetDescription(int level = 1) => GetDescriptionWithIndex(GetTextIndex(), level);
    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemyLowFrontmost, false);
    //public override float GetBasePower() => 1.5f;
    public override int GetBaseCost() => 9;

    public override int GetCost(BattleEntity caller, int level = 1)
    {
        return CostCalculation(caller, level, 6);
    }

    /*
    public override int GetMaxLevel(BattleEntity caller)
    {
        return 2;
    }
    */

    public override float ActionCommandTime()
    {
        return 0.5f;
    }
    public override bool GetOutcome(BattleEntity caller)
    {
        return caller.GetAttackHit(caller.curTarget, BattleHelper.DamageType.Light);
    }
    public override void DealDamage(BattleEntity caller, int sd, bool result)
    {
        ulong propertyBlockB = (ulong)(BattleHelper.DamageProperties.AdvancedElementCalc);
        if (result)
        {
            ulong propertyBlock = (ulong)BattleHelper.DamageProperties.AC_Success;
            propertyBlockB |= (ulong)BattleHelper.DamageProperties.AC_Success;
            switch (level)
            {
                case 1:
                    caller.DealDamage(caller.curTarget, sd + 5, BattleHelper.DamageType.Light, propertyBlock, BattleHelper.ContactLevel.Weapon);
                    break;
                case 2:
                    caller.DealDamage(caller.curTarget, sd + 7, BattleHelper.DamageType.Light, propertyBlockB, BattleHelper.ContactLevel.Weapon);
                    break;
            }
        }
        else
        {
            switch (level)
            {
                case 1:
                    caller.DealDamage(caller.curTarget, Mathf.CeilToInt(sd / 2f) + 5, BattleHelper.DamageType.Light, 0, BattleHelper.ContactLevel.Weapon);
                    break;
                case 2:
                    caller.DealDamage(caller.curTarget, Mathf.CeilToInt(sd / 2f) + 7, BattleHelper.DamageType.Light, propertyBlockB, BattleHelper.ContactLevel.Weapon);
                    break;
            }
        }
    }

    public override IEnumerator SwingAnimations(BattleEntity caller, int sl, int level = 1)
    {
        GameObject eoS = null;
        eoS = Instantiate(Resources.Load<GameObject>("VFX/Battle/Moves/Player/Effect_LightSmash"), BattleControl.Instance.transform);
        eoS.transform.position = transform.position + Vector3.up * 0.325f;

        yield return new WaitForSeconds(0.2f);
        //Impact effect
        GameObject eoShockwave;
        eoShockwave = Instantiate(Resources.Load<GameObject>("VFX/Battle/Moves/Player/Effect_LightShockwave"), BattleControl.Instance.transform);
        eoShockwave.transform.position = transform.position + Vector3.right * 0.7f;
    }

    public override void PreMove(BattleEntity caller, int level = 1)
    {

    }
    public override void PostMove(BattleEntity caller, int level = 1)
    {
    }

    public override string GetHighlightText(BattleEntity caller, BattleEntity target, int level = 1)
    {
        if (caller is PlayerEntity pcallerA)
        {
            if (!pcallerA.BadgeEquipped(Badge.BadgeType.PowerSight))
            {
                return "";
            }
        }

        int sd = 2;

        if (caller is PlayerEntity pcaller)
        {
            sd = pcaller.GetWeaponDamage();
        }

        //ulong propertyBlock = (ulong)BattleHelper.DamageProperties.PlusOneOnBuff;

        int val = 0; // caller.DealDamageCalculation(target, sd, 0, propertyBlock);

        ulong propertyBlock = (ulong)(BattleHelper.DamageProperties.AdvancedElementCalc);
        switch (level)
        {
            case 1:
                val = caller.DealDamageCalculation(target, sd + 5, BattleHelper.DamageType.Light, 0);
                break;
            case 2:
                val = caller.DealDamageCalculation(target, sd + 7, BattleHelper.DamageType.Light, propertyBlock);
                break;
        }

        return val + "";
    }
}

public class LM_Illuminate : LunaMove
{
    public LM_Illuminate()
    {
    }

    public override int GetTextIndex()
    {
        return 21;
    }
    public override string GetName() => GetNameWithIndex(GetTextIndex());
    public override string GetDescription(int level = 1) => GetDescriptionWithIndex(GetTextIndex(), level);
    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveAlly, false);
    //public override float GetBasePower() => 0.5f;
    public override int GetBaseCost() => 3;

    public override TargetArea GetTargetArea(BattleEntity caller, int level = 1)
    {
        switch (level)
        {
            case 1:
                return new TargetArea(TargetArea.TargetAreaType.LiveAlly, false);
            default:
                return new TargetArea(TargetArea.TargetAreaType.LiveAlly, true);
        }
    }

    public override int GetCost(BattleEntity caller, int level = 1)
    {
        switch (level)
        {
            case 1: return CostModification(caller, level, 3);
            case 2: return CostModification(caller, level, 6);
            case 3: return CostModification(caller, level, 9);
        }
        return CostCalculation(caller, level, 3);
    }

    /*
    public override int GetMaxLevel(BattleEntity caller)
    {
        return 3;
    }
    */

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        AC_PressButtonTimed actionCommand = null;
        if (caller is PlayerEntity pcaller) //we have technology
        {
            actionCommand = gameObject.AddComponent<AC_PressButtonTimed>();
            actionCommand.Init(pcaller);
            actionCommand.Setup(0.5f);
        }

        yield return new WaitForSeconds(ActionCommand.FADE_IN_TIME);
        StartCoroutine(caller.Spin(Vector3.up * 360, 0.5f));
        yield return new WaitUntil(() => actionCommand.IsComplete());

        bool result = actionCommand == null ? true : actionCommand.GetSuccess();
        if (actionCommand != null)
        {
            actionCommand.End();
            Destroy(actionCommand);
        }

        if (result)
        {
            BattleControl.Instance.CreateActionCommandEffect(BattleHelper.ActionCommandText.Nice, caller.GetDamageEffectPosition(), caller);
            yield return StartCoroutine(caller.JumpHeavy(caller.homePos, 1.5f, 0.3f, 0.15f));

            switch (level)
            {
                case 1:
                    caller.InflictEffect(caller.curTarget, new Effect(Effect.EffectType.AttackUp, 2, 3));
                    break;
                case 2:
                    List<BattleEntity> targets = BattleControl.Instance.GetEntitiesSorted(caller, GetTargetArea(caller, level));
                    foreach (BattleEntity b in targets)
                    {
                        caller.InflictEffect(b, new Effect(Effect.EffectType.AttackUp, 2, 3));
                    }
                    break;
                case 3:
                    List<BattleEntity> targetsB = BattleControl.Instance.GetEntitiesSorted(caller, GetTargetArea(caller, level));
                    foreach (BattleEntity b in targetsB)
                    {
                        caller.InflictEffect(b, new Effect(Effect.EffectType.AttackUp, 2, 3));
                        caller.InflictEffect(b, new Effect(Effect.EffectType.ParryAura, 1, 3));
                    }
                    break;
            }
        }
        else
        {
            yield return StartCoroutine(caller.JumpHeavy(caller.homePos, 0.5f, 0.15f, 0.15f));
            switch (level)
            {
                case 1:
                    caller.InflictEffect(caller.curTarget, new Effect(Effect.EffectType.AttackUp, 1, 3));
                    break;
                case 2:
                    List<BattleEntity> targets = BattleControl.Instance.GetEntitiesSorted(caller, GetTargetArea(caller, level));
                    foreach (BattleEntity b in targets)
                    {
                        caller.InflictEffect(b, new Effect(Effect.EffectType.AttackUp, 1, 3));
                    }
                    break;
                case 3:
                    List<BattleEntity> targetsB = BattleControl.Instance.GetEntitiesSorted(caller, GetTargetArea(caller, level));
                    foreach (BattleEntity b in targetsB)
                    {
                        caller.InflictEffect(b, new Effect(Effect.EffectType.AttackUp, 1, 3));
                        caller.InflictEffect(b, new Effect(Effect.EffectType.ParryAura, 1, 3));
                    }
                    break;
            }
        }
    }

    public override void PreMove(BattleEntity caller, int level = 1)
    {
    }
    public override void PostMove(BattleEntity caller, int level = 1)
    {
    }
}

public class LM_HammerBeat : LunaMove
{
    public LM_HammerBeat()
    {
    }

    public override int GetTextIndex()
    {
        return 22;
    }
    public override string GetName() => GetNameWithIndex(GetTextIndex());
    public override string GetDescription(int level = 1) => GetDescriptionWithIndex(GetTextIndex(), level);
    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveAlly, true);
    //public override float GetBasePower() => 1.5f;
    public override int GetBaseCost() => 5;

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        //Debug.Log(caller.id + " fly");
        if (!BattleControl.Instance.EntityValid(caller.curTarget))
        {
            caller.curTarget = null;
        }

        //Vector3 target = caller.curTarget.transform.position - caller.curTarget.width * Vector3.right;
        //target += Vector3.left * 0.5f;
        //yield return StartCoroutine(caller.Move(target));

        if (caller.curTarget != null)
        {
            int sd = 2;

            AC_HoldLeft actionCommand = null;
            if (caller is PlayerEntity pcaller)
            {
                sd = pcaller.GetWeaponDamage();
                actionCommand = gameObject.AddComponent<AC_HoldLeft>();
                actionCommand.Init(pcaller);

                float window = ActionCommand.TIMING_WINDOW;

                window /= caller.actionCounter;
                actionCommand.Setup(0.5f / caller.actionCounter);
            }

            yield return new WaitUntil(() => actionCommand.IsStarted());
            yield return new WaitUntil(() => actionCommand.IsComplete());

            yield return StartCoroutine(caller.Spin(Vector3.up * 360, 0.15f));
            bool result = actionCommand == null ? true : actionCommand.GetSuccess();
            if (actionCommand != null)
            {
                actionCommand.End();
                Destroy(actionCommand);
            }
            if (result)
            {
                BattleControl.Instance.CreateActionCommandEffect(BattleHelper.ActionCommandText.Nice, caller.GetDamageEffectPosition(), caller);
                List<BattleEntity> targets = BattleControl.Instance.GetEntitiesSorted(caller, GetTargetArea(caller, level));
                foreach (BattleEntity b in targets)
                {
                    b.HealHealth(5);
                    b.InflictEffectForce(caller, new Effect(Effect.EffectType.Focus, 1, 255));
                }
                caller.InflictEffectForce(caller, new Effect(Effect.EffectType.BonusTurns, 1, 255));
            }
            else
            {
                List<BattleEntity> targets = BattleControl.Instance.GetEntitiesSorted(caller, GetTargetArea(caller, level));
                foreach (BattleEntity b in targets)
                {
                    b.HealHealth(3);
                    b.InflictEffectForce(caller, new Effect(Effect.EffectType.Focus, 1, 255));
                }
            }
        }
        //yield return StartCoroutine(caller.Move(caller.homePos));
    }

    public override void PreMove(BattleEntity caller, int level = 1)
    {

    }
    public override void PostMove(BattleEntity caller, int level = 1)
    {
    }
}

public class LM_MistWall : LunaMove
{
    public LM_MistWall()
    {
    }

    public override int GetTextIndex()
    {
        return 23;
    }
    public override string GetName() => GetNameWithIndex(GetTextIndex());
    public override string GetDescription(int level = 1) => GetDescriptionWithIndex(GetTextIndex(), level);
    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveAlly, false);
    //public override float GetBasePower() => 0.5f;
    public override int GetBaseCost() => 8;

    public override TargetArea GetTargetArea(BattleEntity caller, int level = 1)
    {
        switch (level)
        {
            case 1:
                return new TargetArea(TargetArea.TargetAreaType.LiveAlly, false);
            default:
                return new TargetArea(TargetArea.TargetAreaType.LiveAlly, true);
        }
    }

    public override int GetCost(BattleEntity caller, int level = 1)
    {
        switch (level)
        {
            case 1: return CostModification(caller, level, 8);
            case 2: return CostModification(caller, level, 12);
            case 3: return CostModification(caller, level, 16);
        }
        return CostCalculation(caller, level, 4);
    }

    /*
    public override int GetMaxLevel(BattleEntity caller)
    {
        return 3;
    }
    */

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        AC_PressButtonTimed actionCommand = null;
        if (caller is PlayerEntity pcaller) //we have technology
        {
            actionCommand = gameObject.AddComponent<AC_PressButtonTimed>();
            actionCommand.Init(pcaller);
            actionCommand.Setup(0.5f);
        }

        yield return new WaitForSeconds(ActionCommand.FADE_IN_TIME);
        StartCoroutine(caller.Spin(Vector3.up * 360, 0.5f));
        yield return new WaitUntil(() => actionCommand.IsComplete());

        bool result = actionCommand == null ? true : actionCommand.GetSuccess();
        if (actionCommand != null)
        {
            actionCommand.End();
            Destroy(actionCommand);
        }

        if (result)
        {
            BattleControl.Instance.CreateActionCommandEffect(BattleHelper.ActionCommandText.Nice, caller.GetDamageEffectPosition(), caller);
            yield return StartCoroutine(caller.JumpHeavy(caller.homePos, 1.5f, 0.3f, 0.15f));

            switch (level)
            {
                case 1:
                    caller.CureCurableEffects(false);
                    caller.InflictEffect(caller, new Effect(Effect.EffectType.MistWall, 1, 3));
                    caller.InflictEffect(caller, new Effect(Effect.EffectType.Immunity, 1, 3));
                    break;
                case 2:
                    List<BattleEntity> targets = BattleControl.Instance.GetEntitiesSorted(caller, GetTargetArea(caller, level));
                    foreach (BattleEntity b in targets)
                    {
                        b.CureCurableEffects(false);
                        caller.InflictEffect(b, new Effect(Effect.EffectType.MistWall, 1, 3));
                        caller.InflictEffect(b, new Effect(Effect.EffectType.Immunity, 1, 3));
                    }
                    break;
                case 3:
                    List<BattleEntity> targetsB = BattleControl.Instance.GetEntitiesSorted(caller, GetTargetArea(caller, level));
                    foreach (BattleEntity b in targetsB)
                    {
                        b.CureCurableEffects(false);
                        caller.InflictEffect(b, new Effect(Effect.EffectType.MistWall, 1, 3));
                        caller.InflictEffect(b, new Effect(Effect.EffectType.Immunity, 1, 3));
                        caller.InflictEffect(b, new Effect(Effect.EffectType.BolsterAura, 1, 3));
                    }
                    break;
            }
        }
        else
        {
            yield return StartCoroutine(caller.JumpHeavy(caller.homePos, 0.5f, 0.15f, 0.15f));
            switch (level)
            {
                case 1:
                    caller.InflictEffect(caller, new Effect(Effect.EffectType.MistWall, 1, 2));
                    break;
                case 2:
                    List<BattleEntity> targets = BattleControl.Instance.GetEntitiesSorted(caller, GetTargetArea(caller, level));
                    foreach (BattleEntity b in targets)
                    {
                        caller.InflictEffect(b, new Effect(Effect.EffectType.MistWall, 1, 2));
                    }
                    break;
                case 3:
                    List<BattleEntity> targetsB = BattleControl.Instance.GetEntitiesSorted(caller, GetTargetArea(caller, level));
                    foreach (BattleEntity b in targetsB)
                    {
                        caller.InflictEffect(b, new Effect(Effect.EffectType.MistWall, 1, 2));
                        caller.InflictEffect(b, new Effect(Effect.EffectType.BolsterAura, 1, 2));
                    }
                    break;
            }
        }
    }

    public override void PreMove(BattleEntity caller, int level = 1)
    {
    }
    public override void PostMove(BattleEntity caller, int level = 1)
    {
    }
}