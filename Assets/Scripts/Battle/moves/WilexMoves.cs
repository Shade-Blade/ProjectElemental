using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;


public abstract class WilexMove : PlayerMove
{
    public enum MoveType
    {
        HighStomp,
        Focus,
        MultiStomp,
        ElectroStomp,
        Taunt,
        ParalyzeStomp,
        FlameStomp,
        DoubleStomp,
        Overstomp,
        SmartStomp,
        TeamQuake,
        EggToss,
        Slash,
        ElectroSlash,
        SlipSlash,
        PoisonSlash,
        PreciseStab,
        SwordBolt,
        SwordDance,
        BoomerangSlash,
        DarkSlash,
        Aetherize,
        FlameBat,
        AstralWall
    }

    public int BaseCostCalculation(int baseCost = 1, int level = 1, int scale = 2, int offset = 0)
    {
        return baseCost + ((level - 1) * scale) + offset;
    }
    public int CostCalculation(BattleEntity caller, int level = 1, int scale = 2, int offset = 0)
    {
        if (GetBaseCost() == 0)
        {
            return 0;
        }

        return CostModification(caller, level, GetBaseCost() + ((level - 1) * scale) + offset);
    }

    public int CostModification(BattleEntity caller, int level = 1, int cost = 1)
    {
        if (caller.entityID == BattleHelper.EntityID.Wilex)
        {
            return StandardCostModification(caller, level, cost);
        } else
        {
            return (int)(1.5f * StandardCostModification(caller, level, cost));
        }
    }

    public override int GetMaxLevel(BattleEntity caller)
    {
        if (caller is PlayerEntity pcaller)
        {
            return pcaller.GetWilexMoveMaxLevel(GetTextIndex());
        }
        return 1;
    }

    public override bool ShowNamePopup()
    {
        return true;
    }


    //public abstract int GetTextIndex();
    public override string GetName() => GetNameWithIndex(GetTextIndex());
    public string GetNameWithIndex(int index)
    {
        string output = BattleControl.Instance.wilexText[index + 1][1];
        return output;
    }

    public override string GetDescription(int level = 1) => GetDescriptionWithIndex(GetTextIndex(), level);

    public string GetDescriptionWithIndex(int index, int level = 1)
    {
        string output = BattleControl.Instance.wilexText[index + 1][2];

        if (level != 1)
        {
            if (level >= 4 || BattleControl.Instance.wilexText[index + 1][1 + level].Length < 2) //if I didn't write a description, use the infinite stacking one
            {
                string[] vars = new string[] { "0", (level).ToString(), (level * 2).ToString(), (level * 3).ToString(), (level * 4).ToString(), (level * 5).ToString(), (level * 6).ToString(), (level * 7).ToString(), (level * 8).ToString() };
                output += " <color,#5000ff>(Lv. " + level + ": " + FormattedString.ParseVars(BattleControl.Instance.wilexText[index + 1][5], vars) + ")</color>";
            }
            else
            {
                output += " <color,#0000ff>(Lv. " + level + ": " + BattleControl.Instance.wilexText[index + 1][1 + level] + ")</color>";
            }
        }

        return output;
    }
}
public class WM_HighStomp : WilexMove
{
    public WM_HighStomp()
    {
    }

    public override int GetTextIndex()
    {
        return 0;
    }

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemyTopmost, false);
    //public override float GetBasePower() => 1.0f;
    public override int GetBaseCost(int level = 1) => 0;

    public override BaseBattleMenu.BaseMenuName GetMoveType() => BaseBattleMenu.BaseMenuName.Jump;

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

            //caller.DebugDrawX(tpos, Color.black);
            //caller.DebugDrawX(caller.curTarget.transform.position, Color.black);

            //yield return StartCoroutine(caller.Squish(0.067f, 0.2f));
            //StartCoroutine(caller.RevertScale(0.1f));
            StartJumpEffects(caller);
            yield return StartCoroutine(caller.JumpHeavy(tpos, 2.5f, 0.5f, -0.25f));

            bool result = actionCommand == null ? true : actionCommand.GetSuccess();
            if (actionCommand != null)
            {
                actionCommand.End();
                Destroy(actionCommand);
            }

            if (GetOutcome(caller))
            {
                caller.SetAnimation("stompsquat");
                yield return new WaitForSeconds(0.05f);
                StompEffects(caller, result);
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
                    yield return StartCoroutine(caller.MoveEasing(caller.homePos, (e) => MainManager.EasingOutIn(e)));
                }
            }
            else
            {
                //Miss
                caller.InvokeMissEvents(caller.curTarget);

                //extrapolate the move curve
                yield return StartCoroutine(caller.ExtrapolateJumpHeavy(spos, tpos, 2, 0.5f, -0.25f));
                yield return StartCoroutine(caller.MoveEasing(caller.homePos, (e) => MainManager.EasingOutIn(e)));
            }
        }
        else
        {
            yield return StartCoroutine(caller.JumpHeavy(caller.homePos, 2, 0.5f, 0.15f));
        }
    }

    public virtual void StartJumpEffects(BattleEntity caller)
    {

    }
    public virtual void StompEffects(BattleEntity caller, bool result)
    {

    }
    public virtual bool GetOutcome(BattleEntity caller)
    {
        return caller.GetAttackHit(caller.curTarget, 0);
    }
    public virtual void DealDamageSuccess(BattleEntity caller, int sd)
    {
        ulong propertyBlock = (ulong)BattleHelper.DamageProperties.AC_Success;
        caller.curTarget.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.Squish);
        caller.DealDamage(caller.curTarget, sd, BattleHelper.DamageType.Normal, propertyBlock, BattleHelper.ContactLevel.Contact);
    }
    public virtual void DealDamageFailure(BattleEntity caller, int sd)
    {
        caller.curTarget.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.Squish);
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

        int val = caller.DealDamageCalculation(target, sd, BattleHelper.DamageType.Normal, (ulong)BattleHelper.DamageProperties.AC_Success);

        return val + "";
    }

    public override string GetActionCommandDesc(int level = 1)
    {
        return AC_Jump.GetACDesc();
    }
}

public class WM_Focus : WilexMove
{
    public WM_Focus()
    {
    }

    public override int GetTextIndex()
    {
        return 1;
    }


    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.Self, false);
    //public override float GetBasePower() => 0.5f;
    public override int GetBaseCost(int level = 1)
    {
        switch (level)
        {
            case 1: return 1;
            case 2: return 4;
            case 3: return 12;
        }
        return BaseCostCalculation(1, level, 3, 5);
    }

    public override BaseBattleMenu.BaseMenuName GetMoveType() => BaseBattleMenu.BaseMenuName.Jump;

    public override int GetCost(BattleEntity caller, int level = 1)
    {
        switch (level)
        {
            case 1: return CostModification(caller, level, 1);
            case 2: return CostModification(caller, level, 4);
            case 3: return CostModification(caller, level, 12);
        }
        return CostCalculation(caller, level, 3, 5);
    }

    /*
    public override int GetMaxLevel(BattleEntity caller)
    {
        return 3;
    }
    */

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        AC_PressATimed actionCommand = null;
        if (caller is PlayerEntity pcaller) //we have technology
        {
            actionCommand = gameObject.AddComponent<AC_PressATimed>();
            actionCommand.Init(pcaller);
            actionCommand.Setup(0.5f);
        }

        yield return new WaitForSeconds(ActionCommand.FADE_IN_TIME);
        StartCoroutine(caller.SpinHeavy(Vector3.up * 360, 0.5f));
        FocusEffect(caller);
        yield return new WaitUntil(() => actionCommand.IsComplete());

        bool result = actionCommand == null ? true : actionCommand.GetSuccess();
        if (actionCommand != null)
        {
            yield return new WaitForSeconds(ActionCommand.END_LAG);
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
                    caller.InflictEffect(caller, new Effect(Effect.EffectType.Focus, 2, Effect.INFINITE_DURATION));
                    break;
                case 2:
                    caller.InflictEffect(caller, new Effect(Effect.EffectType.Focus, 3, Effect.INFINITE_DURATION));
                    break;
                case 3:
                    caller.InflictEffect(caller, new Effect(Effect.EffectType.Focus, 6, Effect.INFINITE_DURATION));
                    break;
                default:
                    caller.InflictEffect(caller, new Effect(Effect.EffectType.Focus, (sbyte)(level * 2), Effect.INFINITE_DURATION));
                    break;
            }
        }
        else
        {
            yield return StartCoroutine(caller.JumpHeavy(caller.homePos, 0.5f, 0.15f, 0.15f));
            switch (level)
            {
                case 1:
                    caller.InflictEffect(caller, new Effect(Effect.EffectType.Focus, 1, Effect.INFINITE_DURATION));
                    break;
                case 2:
                    caller.InflictEffect(caller, new Effect(Effect.EffectType.Focus, 2, Effect.INFINITE_DURATION));
                    break;
                case 3:
                    caller.InflictEffect(caller, new Effect(Effect.EffectType.Focus, 3, Effect.INFINITE_DURATION));
                    break;
                default:
                    caller.InflictEffect(caller, new Effect(Effect.EffectType.Focus, (sbyte)(level), Effect.INFINITE_DURATION));
                    break;
            }
        }
    }

    public void FocusEffect(BattleEntity caller)
    {
        GameObject eo = null;
        eo = Instantiate(Resources.Load<GameObject>("VFX/Battle/Moves/Player/Effect_FocusRadialFlowIn"), BattleControl.Instance.transform);
        eo.transform.position = caller.ApplyScaledOffset(Vector3.up * 0.5f);
        eo.transform.localRotation = Quaternion.identity;
    }

    public override void PreMove(BattleEntity caller, int level = 1)
    {
    }
    public override void PostMove(BattleEntity caller, int level = 1)
    {
    }

    public override string GetActionCommandDesc(int level = 1)
    {
        return AC_PressATimed.GetACDesc();
    }
}

public class WM_MultiStomp : WilexMove
{
    public WM_MultiStomp()
    {
    }

    public override int GetTextIndex()
    {
        return 2;
    }


    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemyTopmost, true);
    //public override float GetBasePower() => 1.0f;
    public override int GetBaseCost(int level = 1) => BaseCostCalculation(4, level);

    public override BaseBattleMenu.BaseMenuName GetMoveType() => BaseBattleMenu.BaseMenuName.Jump;

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        //Debug.Log(caller.id + " jump");
        if (!BattleControl.Instance.EntityValid(caller.curTarget))
        {
            caller.curTarget = null;
        }

        List<BattleEntity> targets = BattleControl.Instance.GetEntitiesSorted(caller, GetBaseTarget());
        if (targets.Count > 0)
        {
            bool fail = false;

            int count = 0;

            while (targets.Count > 0 && !fail)
            {
                count++;
                caller.curTarget = targets[0];
                int sd = 2;

                AC_Jump actionCommand = null;
                if (caller is PlayerEntity pcaller) //we have technology
                {
                    sd = pcaller.GetStompDamage();
                    actionCommand = gameObject.AddComponent<AC_Jump>();
                    actionCommand.Init(pcaller);
                    actionCommand.Setup(0.6f);
                }

                Vector3 tpos = caller.curTarget.ApplyScaledOffset(caller.curTarget.stompOffset);
                Vector3 spos = transform.position;

                //yield return StartCoroutine(caller.Squish(0.067f, 0.2f));
                //StartCoroutine(caller.RevertScale(0.1f));
                yield return StartCoroutine(caller.JumpHeavy(tpos, 2.5f, 0.6f, -0.25f));

                bool result = actionCommand == null ? true : actionCommand.GetSuccess();
                if (actionCommand != null)
                {
                    actionCommand.End();
                    Destroy(actionCommand);
                }

                if (GetOutcome(caller))
                {
                    if (result)
                    {
                        DealDamageSuccess(caller, sd, count);
                        //yield return StartCoroutine(caller.Squish(0.067f, 0.2f));
                        //StartCoroutine(caller.RevertScale(0.1f));
                        //yield return StartCoroutine(caller.JumpHeavy(caller.homePos, 2, 0.5f, 0.15f));
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
                        fail = true;
                    }
                }
                else
                {
                    //Miss
                    caller.InvokeMissEvents(caller.curTarget);

                    //extrapolate the move curve
                    yield return StartCoroutine(caller.ExtrapolateJumpHeavy(spos, tpos, 2, 0.5f, -0.25f));
                    fail = true;
                }

                targets.RemoveAt(0);
            }

            if (!fail)
            {
                yield return StartCoroutine(caller.JumpHeavy(caller.homePos, 2, 0.5f, 0.15f));
            } else
            {
                yield return StartCoroutine(caller.MoveEasing(caller.homePos, (e) => MainManager.EasingOutIn(e)));
            }
        }
        else
        {
            yield return StartCoroutine(caller.JumpHeavy(caller.homePos, 2, 0.5f, 0.15f));
        }
    }

    public virtual bool GetOutcome(BattleEntity caller)
    {
        return caller.GetAttackHit(caller.curTarget, 0);
    }
    public virtual void DealDamageSuccess(BattleEntity caller, int sd, int count)
    {
        ulong properties = 0;
        if (count == 1)
        {
            properties = (ulong)BattleHelper.DamageProperties.AC_Success;
        }
        else
        {
            properties = (ulong)BattleHelper.DamageProperties.AC_SuccessStall;
        }
        caller.curTarget.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.Squish);
        caller.DealDamage(caller.curTarget, sd - 2 + level * 2, BattleHelper.DamageType.Normal, properties, BattleHelper.ContactLevel.Contact);
    }
    public virtual void DealDamageFailure(BattleEntity caller, int sd)
    {
        caller.curTarget.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.Squish);
        caller.DealDamage(caller.curTarget, Mathf.CeilToInt(sd / 2f) - 2 + level * 2, BattleHelper.DamageType.Normal, 0, BattleHelper.ContactLevel.Contact);
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

        int val = caller.DealDamageCalculation(target, sd - 2 + level * 2, 0, (ulong)BattleHelper.DamageProperties.AC_Success);

        return val + "?";
    }

    public override string GetActionCommandDesc(int level = 1)
    {
        return AC_Jump.GetACDesc();
    }
}

public class WM_ElectroStomp : WM_HighStomp
{
    public WM_ElectroStomp()
    {
    }

    public override int GetTextIndex()
    {
        return 3;
    }


    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemyTopmost, false);
    //public override float GetBasePower() => 1.0f;
    public override int GetBaseCost(int level = 1)
    {
        switch (level)
        {
            case 1: return 6;
            case 2: return 10;
        }
        return BaseCostCalculation(6, level, 4);
    }
    public override BaseBattleMenu.BaseMenuName GetMoveType() => BaseBattleMenu.BaseMenuName.Jump;

    public override int GetCost(BattleEntity caller, int level = 1)
    {
        switch (level)
        {
            case 1: return CostModification(caller, level, 6);
            case 2: return CostModification(caller, level, 10);
        }
        return CostCalculation(caller, level, 4);
    }

    /*
    public override int GetMaxLevel(BattleEntity caller)
    {
        return 2;
    }
    */

    public override void StartJumpEffects(BattleEntity caller)
    {
        GameObject effect = Instantiate(Resources.Load<GameObject>("VFX/Overworld/Player/Effect_Jump_Spark"), BattleControl.Instance.transform);
        effect.transform.position = caller.transform.position + Vector3.up * 0.425f;
    }
    public override void StompEffects(BattleEntity caller, bool result)
    {
        GameObject effect = Instantiate(Resources.Load<GameObject>("VFX/Overworld/Player/Effect_Jump_Spark"), BattleControl.Instance.transform);
        effect.transform.position = caller.transform.position + Vector3.up * 0.425f;
    }
    public override bool GetOutcome(BattleEntity caller)
    {
        return caller.GetAttackHit(caller.curTarget, BattleHelper.DamageType.Air);
    }
    public override void DealDamageSuccess(BattleEntity caller, int sd)
    {
        ulong propertyBlockB = (ulong)(BattleHelper.DamageProperties.AdvancedElementCalc | BattleHelper.DamageProperties.AC_Success);
        ulong propertyBlock = (ulong)BattleHelper.DamageProperties.AC_Success;

        switch (level)
        {
            case 1:
                caller.curTarget.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.Squish);
                caller.DealDamage(caller.curTarget, sd, BattleHelper.DamageType.Air, propertyBlock, BattleHelper.ContactLevel.Infinite);
                break;
            case 2:
                caller.curTarget.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.Squish);
                caller.DealDamage(caller.curTarget, sd, BattleHelper.DamageType.Air, propertyBlockB, BattleHelper.ContactLevel.Infinite);
                caller.InflictEffect(caller.curTarget, new Effect(Effect.EffectType.DefenseDown, 2, 3), caller.posId);
                break;
            default:
                caller.curTarget.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.Squish);
                caller.DealDamage(caller.curTarget, sd + 2 * level - 4, BattleHelper.DamageType.Air, propertyBlockB, BattleHelper.ContactLevel.Infinite);
                caller.InflictEffect(caller.curTarget, new Effect(Effect.EffectType.DefenseDown, (sbyte)(level * 2 - 2), 3), caller.posId);
                break;
        }
    }
    public override void DealDamageFailure(BattleEntity caller, int sd)
    {
        ulong propertyBlock = (ulong)(BattleHelper.DamageProperties.AdvancedElementCalc);
        switch (level)
        {
            case 1:
                caller.curTarget.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.Squish);
                caller.DealDamage(caller.curTarget, Mathf.CeilToInt(sd / 2f), BattleHelper.DamageType.Air, 0, BattleHelper.ContactLevel.Infinite);
                break;
            case 2:
                caller.curTarget.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.Squish);
                caller.DealDamage(caller.curTarget, Mathf.CeilToInt(sd / 2f), BattleHelper.DamageType.Air, propertyBlock, BattleHelper.ContactLevel.Infinite);
                caller.InflictEffect(caller.curTarget, new Effect(Effect.EffectType.DefenseDown, 1, 3), caller.posId);
                break;
            default:
                caller.curTarget.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.Squish);
                caller.DealDamage(caller.curTarget, Mathf.CeilToInt(sd / 2f) + 2 * level - 4, BattleHelper.DamageType.Air, propertyBlock, BattleHelper.ContactLevel.Infinite);
                caller.InflictEffect(caller.curTarget, new Effect(Effect.EffectType.DefenseDown, (sbyte)(level - 1), 3), caller.posId);
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

        ulong propertyBlock = (ulong)(BattleHelper.DamageProperties.AdvancedElementCalc) + (ulong)BattleHelper.DamageProperties.AC_Success;
        switch (level)
        {
            case 1:
                val = caller.DealDamageCalculation(target, sd, BattleHelper.DamageType.Air, (ulong)BattleHelper.DamageProperties.AC_Success);
                break;
            case 2:
                val = caller.DealDamageCalculation(target, sd, BattleHelper.DamageType.Air, propertyBlock);
                break;
            default:
                val = caller.DealDamageCalculation(target, sd + 2 * level - 4, BattleHelper.DamageType.Air, propertyBlock);
                break;
        }

        return val + "";
    }

    public override string GetActionCommandDesc(int level = 1)
    {
        return AC_Jump.GetACDesc();
    }
}

public class WM_Taunt : WilexMove
{
    public WM_Taunt()
    {
    }

    public override int GetTextIndex()
    {
        return 4;
    }


    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemy, true);
    //public override float GetBasePower() => 0.5f;
    public override int GetBaseCost(int level = 1) => BaseCostCalculation(3, level, 3);
    public override int GetCost(BattleEntity caller, int level = 1)
    {
        return CostCalculation(caller, level, 3);
    }
    public override BaseBattleMenu.BaseMenuName GetMoveType() => BaseBattleMenu.BaseMenuName.Jump;

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        List<BattleEntity> targets = BattleControl.Instance.GetEntitiesSorted(caller, GetBaseTarget());

        AC_PressATimed actionCommand = null;
        if (caller is PlayerEntity pcaller) //we have technology
        {
            actionCommand = gameObject.AddComponent<AC_PressATimed>();
            actionCommand.Init(pcaller);
            actionCommand.Setup(0.5f);
        }

        caller.SetAnimation("taunt");
        yield return new WaitForSeconds(ActionCommand.FADE_IN_TIME);

        yield return new WaitUntil(() => actionCommand.IsComplete());

        bool result = actionCommand == null ? true : actionCommand.GetSuccess();
        if (actionCommand != null)
        {
            yield return new WaitForSeconds(ActionCommand.END_LAG);
            actionCommand.End();
            Destroy(actionCommand);
        }

        if (result)
        {
            BattleControl.Instance.CreateActionCommandEffect(BattleHelper.ActionCommandText.Nice, caller.GetDamageEffectPosition(), caller);

            foreach (BattleEntity b in targets)
            {
                BattleEntity.SpecialInvokeHurtEvents(caller, b, BattleHelper.DamageType.Fire, 0);
                caller.InflictEffect(b, new Effect(Effect.EffectType.Berserk, 1, (sbyte)(1 + level * 2)), caller.posId);
            }
        }
        else
        {

            foreach (BattleEntity b in targets)
            {
                BattleEntity.SpecialInvokeHurtEvents(caller, b, BattleHelper.DamageType.Fire, 0);
                caller.InflictEffect(b, new Effect(Effect.EffectType.Berserk, 1, (sbyte)(1 + level * 1)), caller.posId);
            }
        }
        caller.SetIdleAnimation();
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
            if (!pcallerA.BadgeEquipped(Badge.BadgeType.AilmentSight))
            {
                return "";
            }
        }

        float statusBoost = 1;
        if (caller is PlayerEntity pcaller)
        {
            statusBoost = pcaller.CalculateStatusBoost(target);
        }

        int hp = (int)(target.StatusWorkingHP(Effect.EffectType.Berserk) / statusBoost);

        bool doesWork = (target.hp > 0) && (target.hp <= hp);

        //bool realDoesWork = target.StatusWillWork(Effect.EffectType.Freeze);

        //Debug.Log(doesWork + " " + realDoesWork);

        if (doesWork)
        {
            return "<highlightyescolor>(" + hp + ")</highlightyescolor>";
        }
        else
        {
            return "<highlightnocolor>(" + hp + ")</highlightnocolor>";
        }
    }

    public override string GetActionCommandDesc(int level = 1)
    {
        return AC_PressATimed.GetACDesc();
    }
}

public class WM_ParalyzeStomp : WM_HighStomp
{
    public WM_ParalyzeStomp()
    {
    }

    public override int GetTextIndex()
    {
        return 5;
    }


    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemyTopmost, false);
    //public override float GetBasePower() => 1.0f;
    public override int GetBaseCost(int level=1) => BaseCostCalculation(8, level, 3);
    public override int GetCost(BattleEntity caller, int level = 1)
    {
        return CostCalculation(caller, level, 3);
    }
    public override BaseBattleMenu.BaseMenuName GetMoveType() => BaseBattleMenu.BaseMenuName.Jump;

    public override void StartJumpEffects(BattleEntity caller)
    {
        GameObject effect = Instantiate(Resources.Load<GameObject>("VFX/Overworld/Player/Effect_Jump_Spark"), BattleControl.Instance.transform);
        effect.transform.position = caller.transform.position + Vector3.up * 0.425f;
    }
    public override void StompEffects(BattleEntity caller, bool result)
    {
        GameObject effect = Instantiate(Resources.Load<GameObject>("VFX/Overworld/Player/Effect_Jump_Spark"), BattleControl.Instance.transform);
        effect.transform.position = caller.transform.position + Vector3.up * 0.425f;
    }
    public override bool GetOutcome(BattleEntity caller)
    {
        return caller.GetAttackHit(caller.curTarget, 0);
    }
    public override void DealDamageSuccess(BattleEntity caller, int sd)
    {
        ulong propertyBlock = (ulong)BattleHelper.DamageProperties.AC_Success;
        caller.curTarget.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.Squish);
        caller.DealDamage(caller.curTarget, sd, BattleHelper.DamageType.Air, propertyBlock, BattleHelper.ContactLevel.Infinite);
        caller.InflictEffect(caller.curTarget, new Effect(Effect.EffectType.Paralyze, 1, (sbyte)(1 + level * 2)), caller.posId);
    }
    public override void DealDamageFailure(BattleEntity caller, int sd)
    {
        caller.curTarget.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.Squish);
        caller.DealDamage(caller.curTarget, Mathf.CeilToInt(sd / 2f), BattleHelper.DamageType.Air, 0, BattleHelper.ContactLevel.Infinite);
        caller.InflictEffect(caller.curTarget, new Effect(Effect.EffectType.Paralyze, 1, (sbyte)(1 + level)), caller.posId);
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

            if (pcallerA.BadgeEquipped(Badge.BadgeType.AilmentSight))
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

        int val = caller.DealDamageCalculation(target, sd, 0, (ulong)BattleHelper.DamageProperties.AC_Success);

        int statusHP = (int)(target.StatusWorkingHP(Effect.EffectType.Paralyze) / statusBoost);

        bool doesWork = (target.hp > 0) && (target.hp - val <= statusHP);

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

    public override string GetActionCommandDesc(int level = 1)
    {
        return AC_Jump.GetACDesc();
    }
}

public class WM_FlameStomp : WM_HighStomp
{
    public WM_FlameStomp()
    {
    }

    public override int GetTextIndex()
    {
        return 6;
    }


    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemyTopmost, false);
    //public override float GetBasePower() => 1.0f;
    public override int GetBaseCost(int level = 1) => BaseCostCalculation(10, level, 4);    
    public override BaseBattleMenu.BaseMenuName GetMoveType() => BaseBattleMenu.BaseMenuName.Jump;

    public override int GetCost(BattleEntity caller, int level = 1)
    {
        switch (level)
        {
            case 1: return CostModification(caller, level, 10);
            case 2: return CostModification(caller, level, 14);
        }
        return CostCalculation(caller, level, 4);
    }

    /*
    public override int GetMaxLevel(BattleEntity caller)
    {
        return 2;
    }
    */

    public override void StartJumpEffects(BattleEntity caller)
    {
        GameObject effect = Instantiate(Resources.Load<GameObject>("VFX/Overworld/Player/Effect_Jump_Flame"), BattleControl.Instance.transform);
        effect.transform.position = caller.transform.position + Vector3.down * (0.05f);

        GameObject eo = null;
        eo = Instantiate(Resources.Load<GameObject>("VFX/Overworld/Player/Effect_FireParticleTrail"), caller.transform);
        eo.transform.localPosition = Vector3.zero;
        eo.transform.localRotation = Quaternion.identity;

        GameObject eoB = null;
        eoB = Instantiate(Resources.Load<GameObject>("VFX/Overworld/Player/Effect_FireTrail"), caller.transform);
        eoB.transform.localPosition = MainManager.Instance.Camera.transform.forward * 0.01f + Vector3.up * 0.05f;
        eoB.transform.localRotation = Quaternion.identity;

        IEnumerator DestroyDelayed(GameObject particle)
        {
            float height = caller.transform.position.y;
            float pastHeight = height;

            float delayTime = 5;
            while (true)
            {
                height = caller.transform.position.y;

                if (delayTime <= 0 && (height < pastHeight + Time.deltaTime * 1))
                {
                    particle.GetComponent<ParticleSystem>().Stop();
                    //Destroy(particle);
                    yield break;
                }
                if (delayTime > 0)
                {
                    delayTime--;
                }

                yield return null;
                pastHeight = height;
            }
        }
        StartCoroutine(DestroyDelayed(eo));
    }
    public override void StompEffects(BattleEntity caller, bool result)
    {
        GameObject effect = Instantiate(Resources.Load<GameObject>("VFX/Overworld/Player/Effect_Jump_Flame"), BattleControl.Instance.transform);
        effect.transform.position = caller.transform.position + Vector3.down * (0.05f);
    }
    public override bool GetOutcome(BattleEntity caller)
    {
        return caller.GetAttackHit(caller.curTarget, BattleHelper.DamageType.Fire);
    }
    public override void DealDamageSuccess(BattleEntity caller, int sd)
    {
        ulong propertyBlock = (ulong)BattleHelper.DamageProperties.AC_Success;
        switch (level)
        {
            case 1:
                caller.curTarget.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.Squish);
                caller.DealDamage(caller.curTarget, sd + 3, BattleHelper.DamageType.Fire, propertyBlock, BattleHelper.ContactLevel.Contact);
                break;
            case 2:
                caller.curTarget.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.Squish);
                caller.DealDamage(caller.curTarget, sd + 4, BattleHelper.DamageType.Fire, propertyBlock, BattleHelper.ContactLevel.Contact);
                caller.InflictEffectBuffered(caller.curTarget, new Effect(Effect.EffectType.Sunder, 3, Effect.INFINITE_DURATION), caller.posId);
                break;
            default:
                caller.curTarget.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.Squish);
                caller.DealDamage(caller.curTarget, sd + level * 2, BattleHelper.DamageType.Fire, propertyBlock, BattleHelper.ContactLevel.Contact);
                caller.InflictEffectBuffered(caller.curTarget, new Effect(Effect.EffectType.Sunder, (sbyte)(level + 2), Effect.INFINITE_DURATION), caller.posId);
                break;
        }
    }
    public override void DealDamageFailure(BattleEntity caller, int sd)
    {
        //ulong propertyBlock = (ulong)(BattleHelper.DamageProperties.AdvancedElementCalc);
        switch (level)
        {
            case 1:
                caller.curTarget.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.Squish);
                caller.DealDamage(caller.curTarget, Mathf.CeilToInt(sd / 2f) + 3, BattleHelper.DamageType.Fire, 0, BattleHelper.ContactLevel.Contact);
                break;
            case 2:
                caller.curTarget.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.Squish);
                caller.DealDamage(caller.curTarget, Mathf.CeilToInt(sd / 2f) + 4, BattleHelper.DamageType.Fire, 0, BattleHelper.ContactLevel.Contact);
                caller.InflictEffectBuffered(caller.curTarget, new Effect(Effect.EffectType.Sunder, 1, Effect.INFINITE_DURATION), caller.posId);
                break;
            default:
                caller.curTarget.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.Squish);
                caller.DealDamage(caller.curTarget, Mathf.CeilToInt(sd / 2f) + level * 2, BattleHelper.DamageType.Fire, 0, BattleHelper.ContactLevel.Contact);
                caller.InflictEffectBuffered(caller.curTarget, new Effect(Effect.EffectType.Sunder, (sbyte)(level + 1), Effect.INFINITE_DURATION), caller.posId);
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

        switch (level)
        {
            case 1:
                val = caller.DealDamageCalculation(target, sd + 3, BattleHelper.DamageType.Fire, (ulong)BattleHelper.DamageProperties.AC_Success);
                break;
            case 2:
                val = caller.DealDamageCalculation(target, sd + 4, BattleHelper.DamageType.Fire, (ulong)BattleHelper.DamageProperties.AC_Success);
                break;
            default:
                val = caller.DealDamageCalculation(target, sd + level * 2, BattleHelper.DamageType.Fire, (ulong)BattleHelper.DamageProperties.AC_Success);
                break;
        }


        return val + "";
    }

    public override string GetActionCommandDesc(int level = 1)
    {
        return AC_Jump.GetACDesc();
    }
}

public class WM_DoubleStomp : WilexMove
{
    public WM_DoubleStomp()
    {
    }

    public override int GetTextIndex()
    {
        return 7;
    }


    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemyTopmost, false);
    //public override float GetBasePower() => 1.0f;
    public override int GetBaseCost(int level = 1) => BaseCostCalculation(11, level, 3);
    public override BaseBattleMenu.BaseMenuName GetMoveType() => BaseBattleMenu.BaseMenuName.Jump;

    public override int GetCost(BattleEntity caller, int level = 1)
    {
        switch (level)
        {
            case 1: return CostModification(caller, level, 11);
            case 2: return CostModification(caller, level, 14);
        }
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

            bool result;
            /*
            bool result = actionCommand == null ? true : actionCommand.GetSuccess();
            if (actionCommand != null)
            {
                actionCommand.End();
                Destroy(actionCommand);
            }
            */

            int hits = 2 + (level - 1);
            for (int i = 0; i < hits; i++)
            {
                result = actionCommand == null ? true : actionCommand.GetSuccess();
                if (actionCommand != null)
                {
                    actionCommand.End();
                    Destroy(actionCommand);
                }
                if (GetOutcome(caller))
                {
                    if (result)
                    {
                        DealDamageSuccess(caller, sd, i, i == hits - 1);
                        //yield return StartCoroutine(caller.Squish(0.067f, 0.2f));
                        //StartCoroutine(caller.RevertScale(0.1f));                                            
                    }
                    else
                    {
                        DealDamageFailure(caller, sd, i);
                        Vector3 targetPos = caller.transform.position;
                        float height = caller.transform.position.y;
                        targetPos += Vector3.down * height + Vector3.left * 0.5f * height;
                        //yield return StartCoroutine(caller.Squish(0.067f, 0.2f));
                        //StartCoroutine(caller.RevertScale(0.1f));
                        yield return StartCoroutine(caller.Jump(targetPos, 0.5f, 0.3f));
                        yield return StartCoroutine(caller.MoveEasing(caller.homePos, (e) => MainManager.EasingOutIn(e)));
                        break;
                    }
                }
                else
                {
                    //Miss
                    caller.InvokeMissEvents(caller.curTarget);

                    //extrapolate the move curve
                    yield return StartCoroutine(caller.ExtrapolateJumpHeavy(spos, tpos, 2, 0.5f, -0.25f));
                    yield return StartCoroutine(caller.MoveEasing(caller.homePos, (e) => MainManager.EasingOutIn(e)));
                    break;
                }

                if (i == hits - 1)
                {
                    yield return StartCoroutine(caller.JumpHeavy(caller.homePos, 2, 0.5f, -0.25f));
                } else
                {
                    if (caller is PlayerEntity pcaller2) //we have technology
                    {
                        sd = pcaller2.GetStompDamage();
                        actionCommand = gameObject.AddComponent<AC_Jump>();
                        actionCommand.Init(pcaller2);
                        actionCommand.Setup(0.5f);
                    }

                    yield return StartCoroutine(caller.JumpHeavy(tpos, 2, 0.5f, -0.25f));
                }
            }            
        }
        else
        {
            yield return StartCoroutine(caller.JumpHeavy(caller.homePos, 2, 0.5f, 0.15f));
        }
    }

    public virtual bool GetOutcome(BattleEntity caller)
    {
        return caller.GetAttackHit(caller.curTarget, 0);
    }
    public virtual void DealDamageSuccess(BattleEntity caller, int sd, int h, bool last)
    {
        ulong propertyBlock = (ulong)BattleHelper.DamageProperties.AC_Success;
        if (!last)
        {
            propertyBlock |= (ulong)BattleHelper.DamageProperties.Combo;
        }
        caller.curTarget.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.Squish);
        caller.DealDamageMultihit(caller.curTarget, sd + 1, BattleHelper.DamageType.Normal, propertyBlock, BattleHelper.ContactLevel.Contact, h, BattleHelper.MultihitReductionFormula.ReduceThreeFourths);
    }
    public virtual void DealDamageFailure(BattleEntity caller, int sd, int h)
    {
        caller.curTarget.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.Squish);
        caller.DealDamageMultihit(caller.curTarget, Mathf.CeilToInt(sd / 2f) + 1, BattleHelper.DamageType.Normal, 0, BattleHelper.ContactLevel.Contact, h, BattleHelper.MultihitReductionFormula.ReduceThreeFourths);
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

        int val = 0; // caller.DealDamageMultihitCalculation(target, sd, 0, 0, 0, BattleHelper.MultihitReductionFormula.ReduceThreeFourths);

        for (int i = 0; i < hits; i++)
        {
            val = caller.DealDamageMultihitCalculation(target, sd + 1, 0, 0, i, BattleHelper.MultihitReductionFormula.ReduceThreeFourths);
            outstring += val;
            if (i > 0)
            {
                outstring += "?";
            }
            if (i < hits - 1)
            {
                outstring += ", ";
            }
        }

        return outstring;
    }

    public override string GetActionCommandDesc(int level = 1)
    {
        return AC_Jump.GetACDesc();
    }
}

public class WM_Overstomp : WilexMove
{
    public WM_Overstomp()
    {
    }

    public override int GetTextIndex()
    {
        return 8;
    }


    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemyTopmost, false);
    //public override float GetBasePower() => 1.0f;
    public override int GetBaseCost(int level = 1) => BaseCostCalculation(12, level, 3);
    public override int GetCost(BattleEntity caller, int level = 1)
    {
        return CostCalculation(caller, level, 3);
    }
    public override BaseBattleMenu.BaseMenuName GetMoveType() => BaseBattleMenu.BaseMenuName.Jump;

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
                actionCommand.Setup(0.75f);
            }

            Vector3 tpos = caller.curTarget.ApplyScaledOffset(caller.curTarget.stompOffset);
            Vector3 spos = transform.position;

            //yield return StartCoroutine(caller.Squish(0.067f, 0.2f));
            //StartCoroutine(caller.RevertScale(0.1f));
            StartJumpEffects(caller);
            yield return StartCoroutine(caller.JumpHeavy(tpos, 5, 0.75f, -0.25f));

            bool result = actionCommand == null ? true : actionCommand.GetSuccess();
            if (actionCommand != null)
            {
                actionCommand.End();
                Destroy(actionCommand);
            }

            if (GetOutcome(caller))
            {
                StompEffects(caller, result);
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
                    yield return StartCoroutine(caller.MoveEasing(caller.homePos, (e) => MainManager.EasingOutIn(e)));
                }
            }
            else
            {
                //Miss
                caller.InvokeMissEvents(caller.curTarget);

                //extrapolate the move curve
                yield return StartCoroutine(caller.ExtrapolateJumpHeavy(spos, tpos, 2, 0.5f, -0.25f));
                yield return StartCoroutine(caller.MoveEasing(caller.homePos, (e) => MainManager.EasingOutIn(e)));
            }
        }
        else
        {
            yield return StartCoroutine(caller.JumpHeavy(caller.homePos, 2, 0.5f, 0.15f));
        }
    }

    public virtual void StartJumpEffects(BattleEntity caller)
    {

    }
    public virtual void StompEffects(BattleEntity caller, bool result)
    {
        GameObject effect = Instantiate(Resources.Load<GameObject>("VFX/Battle/Moves/Player/Effect_OverstompShockwave"), BattleControl.Instance.transform);
        effect.transform.position = caller.transform.position + Vector3.down * (0.05f);
    }
    public bool GetOutcome(BattleEntity caller)
    {
        return caller.GetAttackHit(caller.curTarget, BattleHelper.DamageType.Fire);
    }
    public void DealDamageSuccess(BattleEntity caller, int sd)
    {
        ulong propertyBlockB = (ulong)BattleHelper.DamageProperties.ContactHazard;
        ulong propertyBlock = (ulong)BattleHelper.DamageProperties.AC_Success;
        caller.curTarget.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.Squish);
        caller.DealDamage(caller.curTarget, sd + 12 + 5 * level - 5, 0, propertyBlock, BattleHelper.ContactLevel.Contact);
        caller.DealDamage(caller, sd + 3 * level - 3, 0, propertyBlockB, BattleHelper.ContactLevel.Contact);
    }
    public void DealDamageFailure(BattleEntity caller, int sd)
    {
        ulong propertyBlock = (ulong)BattleHelper.DamageProperties.ContactHazard;
        caller.curTarget.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.Squish);
        caller.DealDamage(caller.curTarget, Mathf.CeilToInt(sd / 2f) + 12 + 5 * level - 5, 0, 0, BattleHelper.ContactLevel.Contact);
        caller.DealDamage(caller, Mathf.CeilToInt(sd / 2f) + 3 * level - 3, 0, propertyBlock, BattleHelper.ContactLevel.Contact);
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

        int val = caller.DealDamageCalculation(target, sd + 12 + 5 * level - 5, 0, (ulong)BattleHelper.DamageProperties.AC_Success);

        int reval = caller.DealDamageCalculation(target, sd + 3 * level - 3, 0, (ulong)BattleHelper.DamageProperties.ContactHazard + (ulong)BattleHelper.DamageProperties.AC_Success);

        return val + "<line><highlightdangercolor>Recoil: " + reval + "</highlightdangercolor>";
    }

    public override string GetActionCommandDesc(int level = 1)
    {
        return AC_Jump.GetACDesc();
    }
}

public class WM_SmartStomp : WM_HighStomp
{
    public WM_SmartStomp()
    {
    }

    public override int GetTextIndex()
    {
        return 9;
    }


    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemyTopmost, false);
    //public override float GetBasePower() => 1.0f;
    public override int GetBaseCost(int level = 1) => BaseCostCalculation(11, level);
    public override BaseBattleMenu.BaseMenuName GetMoveType() => BaseBattleMenu.BaseMenuName.Jump;

    public override void StompEffects(BattleEntity caller, bool result)
    {
        GameObject effect = Instantiate(Resources.Load<GameObject>("VFX/Battle/Moves/Player/Effect_SmartStompShockwave"), BattleControl.Instance.transform);
        effect.transform.position = caller.transform.position + Vector3.down * (0.05f);
    }
    public override bool GetOutcome(BattleEntity caller)
    {
        return caller.GetAttackHit(caller.curTarget, 0);
    }
    public override void DealDamageSuccess(BattleEntity caller, int sd)
    {
        ulong propertyBlock = (ulong)BattleHelper.DamageProperties.AC_Success;
        caller.curTarget.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.Squish);
        caller.DealDamage(caller.curTarget, sd, 0, propertyBlock, BattleHelper.ContactLevel.Contact);

        //Determining best status
        //Step 1: figure out which ones will actually work
        //Step 2: find one with highest turn count
        //Step 3: break ties with priority

        //Order of worst to best
        //berserk, poison, paralyze, sleep, dizzy, freeze

        Effect.EffectType[] statuses = { Effect.EffectType.Berserk, Effect.EffectType.Poison, Effect.EffectType.Paralyze, Effect.EffectType.Sleep, Effect.EffectType.Dizzy, Effect.EffectType.Freeze };
        //bool[] statusBools = { false, false, false, false, false, false };
        //int[] statusTurns = { 0, 0, 0, 0, 0, 0 };

        Effect.EffectType best = Effect.EffectType.Freeze;
        float bestMod = 0;

        float statusBoost = 1;
        if (caller is PlayerEntity pcaller)
        {
            statusBoost = pcaller.CalculateStatusBoost(caller.curTarget);
        }

        foreach (Effect.EffectType status in statuses)
        {
            if (caller.curTarget.StatusWillWork(status, statusBoost))
            {
                if (caller.curTarget.GetStatusTurnModifier(status) > bestMod)
                {
                    bestMod = caller.curTarget.GetStatusTurnModifier(status);
                    best = status;
                }
            }
        }

        caller.InflictEffect(caller.curTarget, new Effect(best, 1, (sbyte)(1 + 2 * level)), caller.posId);
    }
    public override void DealDamageFailure(BattleEntity caller, int sd)
    {
        caller.curTarget.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.Squish);
        caller.DealDamage(caller.curTarget, Mathf.CeilToInt(sd / 2f), 0, 0, BattleHelper.ContactLevel.Contact);

        //Determining best status
        //Step 1: figure out which ones will actually work
        //Step 2: find one with highest turn count
        //Step 3: break ties with priority

        //Order of worst to best
        //berserk, poison, paralyze, sleep, dizzy, freeze

        Effect.EffectType[] statuses = { Effect.EffectType.Berserk, Effect.EffectType.Poison, Effect.EffectType.Paralyze, Effect.EffectType.Sleep, Effect.EffectType.Dizzy, Effect.EffectType.Freeze };
        //bool[] statusBools = { false, false, false, false, false, false };
        //int[] statusTurns = { 0, 0, 0, 0, 0, 0 };

        Effect.EffectType best = Effect.EffectType.Freeze;
        float bestMod = 0;

        float statusBoost = 1;
        if (caller is PlayerEntity pcaller)
        {
            statusBoost = pcaller.CalculateStatusBoost(caller.curTarget);
        }

        foreach (Effect.EffectType status in statuses)
        {
            if (caller.curTarget.StatusWillWork(status, statusBoost))
            {
                //ties go to the one checked later (the better one)
                if (caller.curTarget.GetStatusTurnModifier(status) >= bestMod)
                {
                    bestMod = caller.curTarget.GetStatusTurnModifier(status);
                    best = status;
                }
            }
        }

        caller.InflictEffect(caller.curTarget, new Effect(best, 1, (sbyte)(1 + level)), caller.posId);
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

            if (pcallerA.BadgeEquipped(Badge.BadgeType.AilmentSight))
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

        int val = caller.DealDamageCalculation(target, sd, 0, (ulong)BattleHelper.DamageProperties.AC_Success);

        Effect.EffectType[] statuses = { Effect.EffectType.Berserk, Effect.EffectType.Poison, Effect.EffectType.Paralyze, Effect.EffectType.Sleep, Effect.EffectType.Dizzy, Effect.EffectType.Freeze };
        //bool[] statusBools = { false, false, false, false, false, false };
        //int[] statusTurns = { 0, 0, 0, 0, 0, 0 };

        Effect.EffectType best = Effect.EffectType.Default;
        float bestMod = 0;

        foreach (Effect.EffectType status in statuses)
        {
            if (target.StatusWillWork(status, statusBoost, val))
            {
                if (target.GetStatusTurnModifier(status) > bestMod)
                {
                    bestMod = target.GetStatusTurnModifier(status);
                    best = status;
                }
            }
            else
            {
                if (best == Effect.EffectType.Default || !target.StatusWillWork(best))
                {
                    //report the highest
                    if (target.GetStatusTableEntry(best).susceptibility < target.GetStatusTableEntry(status).susceptibility)
                    {
                        best = status;
                    }
                }
            }
        }

        //this is the most accurate number (the hp for the status that it will inflict)
        //though this isn't necessarily the status with the highest working HP (so the number may seem inconsistent)

        int statusHP = (int)(target.StatusWorkingHP(best) / statusBoost);

        bool doesWork = (target.hp > 0) && (target.hp - val <= statusHP);

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
                return outString + "<highlightyescolor>" + best + ": (" + statusHP + ")</highlightyescolor>";
            }
            else
            {
                if (best == Effect.EffectType.Default)
                {
                    return outString + "<highlightnocolor>(" + statusHP + ")</highlightnocolor>";
                }
                else
                {
                    return outString + "<highlightnocolor>" + best + ": (" + statusHP + ")</highlightnocolor>";
                }
            }
        }

        return outString;
    }

    public override string GetActionCommandDesc(int level = 1)
    {
        return AC_Jump.GetACDesc();
    }
}

public class WM_TeamQuake : WilexMove
{
    public WM_TeamQuake()
    {
    }

    public override int GetTextIndex()
    {
        return 10;
    }


    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemyGrounded, true);
    //public override float GetBasePower() => 0.5f;
    public override int GetBaseCost(int level = 1) => BaseCostCalculation(18, level, 5);
    public override BaseBattleMenu.BaseMenuName GetMoveType() => BaseBattleMenu.BaseMenuName.Jump;

    public override int GetCost(BattleEntity caller, int level = 1)
    {
        return CostCalculation(caller, level, 5);
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

        List<BattleEntity> targets = BattleControl.Instance.GetEntitiesSorted(caller, GetBaseTarget());

        int hits = 3 + (level - 1) * 2;
        //Debug.Log(hits);

        for (int h = 0; h < hits; h++)
        {
            //Debug.Log(h);
            int sd = 2;
            AC_PressATimed actionCommand = null;
            if (caller is PlayerEntity pcaller) //we have technology
            {
                sd = pcaller.GetStompDamage();
                actionCommand = gameObject.AddComponent<AC_PressATimed>();
                actionCommand.Init(pcaller);
                actionCommand.Setup(0.5f);
            }

            int sdo = 2;
            if (other is PlayerEntity pother)
            {
                sdo = pother.GetStompDamage();
            }

            yield return new WaitForSeconds(ActionCommand.FADE_IN_TIME);

            Vector3 targetPos = caller.transform.position + Vector3.up * caller.height;
            StartCoroutine(other.JumpHeavy(targetPos, 2, 0.5f, -0.25f));
            //StartCoroutine(caller.SpinHeavy(Vector3.up * 360, 0.5f));
            yield return new WaitUntil(() => actionCommand.IsComplete());

            bool result = actionCommand == null ? true : actionCommand.GetSuccess();
            if (actionCommand != null)
            {
                yield return new WaitForSeconds(ActionCommand.END_LAG);
                actionCommand.End();
                Destroy(actionCommand);
            }

            ulong propertyBlock = (ulong)BattleHelper.DamageProperties.Combo + (ulong)BattleHelper.DamageProperties.HitsWhileDizzy;
            ulong propertyBlockLast = (ulong)BattleHelper.DamageProperties.HitsWhileDizzy;

            ulong block = propertyBlock;

            if (h == hits - 1)
            {
                block = propertyBlockLast;
            } else
            {
                if (!result)
                {
                    block = propertyBlockLast;
                }
            }

            ulong blockA = block;
            ulong blockB = block;

            if (result)
            {
                blockA |= (ulong)BattleHelper.DamageProperties.AC_Success;
                blockB |= (ulong)BattleHelper.DamageProperties.AC_SuccessStall;
            }

            MainManager.Instance.StartCoroutine(MainManager.Instance.CameraShake1D(Vector3.up, 0.3f, 0.5f));
            Shockwave(caller, 1);
            yield return new WaitForSeconds(0.1f);

            int c = 0;
            foreach (BattleEntity target in targets)
            {
                c++;
                if (other.GetAttackHit(target, 0, block))
                {
                    if (c == 1)
                    {
                        target.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.Launch);
                        other.DealDamagePooledMultihit(caller, target, Mathf.CeilToInt(sdo / 2f) + 1, Mathf.CeilToInt(sd / 2f) + 1, BattleHelper.DamageType.Normal, blockA, BattleHelper.ContactLevel.Infinite, h, BattleHelper.MultihitReductionFormula.ReduceThreeFourths);
                    }
                    else
                    {
                        target.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.Launch);
                        other.DealDamagePooledMultihit(caller, target, Mathf.CeilToInt(sdo / 2f) + 1, Mathf.CeilToInt(sd / 2f) + 1, BattleHelper.DamageType.Normal, blockB, BattleHelper.ContactLevel.Infinite, h, BattleHelper.MultihitReductionFormula.ReduceThreeFourths);
                    }
                } else
                {
                    other.InvokeMissEvents(target);
                }
            }
            if (!result)
            {
                break;
            }
        }      
        
        yield return StartCoroutine(other.JumpHeavy(other.homePos, 2, 0.5f, -0.25f));

        caller.InflictEffectForce(other, new Effect(Effect.EffectType.Cooldown, 1, Effect.INFINITE_DURATION));
        other.actionCounter++;

    }

    public virtual void Shockwave(BattleEntity caller, int level = 1)
    {
        //Impact effect
        GameObject eoShockwave;
        eoShockwave = Instantiate(Resources.Load<GameObject>("VFX/Battle/Moves/Player/Effect_TeamQuakeShockwave"), BattleControl.Instance.transform);
        eoShockwave.transform.position = transform.position + Vector3.down * 0.2f;  //(so that it fades away smoother at the end)
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

        int hits = 3 + (level - 1) * 2;

        ulong block = (ulong)BattleHelper.DamageProperties.Combo + (ulong)BattleHelper.DamageProperties.HitsWhileDizzy;

        int val = 0;

        for (int i = 0; i < hits; i++)
        {
            val = other.DealDamagePooledMultihitCalculation(caller, target, Mathf.CeilToInt(sdo / 2f) + 1, Mathf.CeilToInt(sd / 2f) + 1, BattleHelper.DamageType.Normal, block, i, BattleHelper.MultihitReductionFormula.ReduceThreeFourths);
            outstring += val;
            if (i > 0)
            {
                outstring += "?";
            }
            if (i < hits - 1)
            {
                outstring += ", ";
            }
        }

        return outstring;
    }

    public override string GetActionCommandDesc(int level = 1)
    {
        return AC_PressATimed.GetACDesc();
    }
}

public class WM_EggToss : WilexMove
{
    public WM_EggToss()
    {
    }

    public override int GetTextIndex()
    {
        return 11;
    }


    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemy, false);
    //public override float GetBasePower() => 0.5f;
    public override int GetBaseCost(int level = 1) => BaseCostCalculation(1, level, 2);
    public override BaseBattleMenu.BaseMenuName GetMoveType() => BaseBattleMenu.BaseMenuName.Jump;

    public override int GetCost(BattleEntity caller, int level = 1)
    {
        return CostCalculation(caller, level, 2);
    }

    /*
    public override int GetMaxLevel(BattleEntity caller)
    {
        return 3;
    }
    */

    public GameObject MakeEggSprite(BattleEntity caller, Item.ItemType eggGenerated)
    {
        Sprite isp = GlobalItemScript.GetItemSprite(eggGenerated);

        Vector2 startPosition = caller.ApplyScaledOffset(Vector3.left * 0.25f + Vector3.up * 0.4f);
        GameObject so = new GameObject("Egg Sprite");
        so.transform.parent = BattleControl.Instance.transform;
        SpriteRenderer s = so.AddComponent<SpriteRenderer>();
        s.sprite = isp;
        so.transform.position = startPosition;

        return so;
    }
    public IEnumerator EggAnimation(BattleEntity caller, Item.ItemType eggGenerated, float offset, GameObject go)
    {
        Vector2 startPosition = caller.ApplyScaledOffset(Vector3.left * 0.25f + Vector3.up * 0.4f) + Vector3.back * 0.05f * offset;
        Vector3 endPosition = caller.transform.position + Vector3.left * offset + Vector3.up * 0.25f + Vector3.back * 0.05f * offset;

        GameObject so = go;

        IEnumerator PosLerp(GameObject o, float duration, Vector3 posA, Vector3 posB)
        {
            Vector3 mpos = ((posA + posB) / 2) + Vector3.up * ((posA.y - posB.y) / 4);

            float time = 0;

            while (time < 1)
            {
                o.transform.localScale = Vector3.one * time;
                time += Time.deltaTime / duration;
                o.transform.position = MainManager.BezierCurve(time, new Vector3[] { posA, mpos, posB });
                yield return null;
            }

            o.transform.localScale = Vector3.one;
            o.transform.position = posB;
        }

        yield return StartCoroutine(PosLerp(so, 0.3f, startPosition, endPosition));
        //yield return new WaitForSeconds(1f);

        yield return null;
    }

    public Item.ItemType GetEggType(BattleEntity caller, int turnOffset, int offset, bool fail = false)
    {
        //do psuedo random things
        int radix = caller.hp * 3 + turnOffset * 7 + offset + caller.actionCounter;

        //random extra step using more prime numbers
        if (radix % 5 == 0 || radix % 11 == 0)
        {
            radix *= 17;
        }

        radix %= 12;

        if (fail)
        {
            radix %= 6;
        }

        return (Item.ItemType)(radix + Item.ItemType.SpicyEgg);
    }

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        AC_PressATimed actionCommand = null;
        if (caller is PlayerEntity pcaller) //we have technology
        {
            actionCommand = gameObject.AddComponent<AC_PressATimed>();
            actionCommand.Init(pcaller);
            actionCommand.Setup(0.5f);
        }

        yield return new WaitForSeconds(ActionCommand.FADE_IN_TIME);
        caller.SetAnimation("eggsquat");
        yield return new WaitUntil(() => actionCommand.IsComplete());

        bool result = actionCommand == null ? true : actionCommand.GetSuccess();
        if (actionCommand != null)
        {
            yield return new WaitForSeconds(ActionCommand.END_LAG);
            actionCommand.End();
            Destroy(actionCommand);
        }

        Item.ItemType[] eggTypes = new Item.ItemType[level];
        for (int i = 0; i < eggTypes.Length; i++)
        {
            eggTypes[i] = Item.ItemType.None;
        }

        if (result)
        {
            BattleControl.Instance.CreateActionCommandEffect(BattleHelper.ActionCommandText.Nice, caller.GetDamageEffectPosition(), caller);

            for (int i = 0; i < eggTypes.Length; i++)
            {
                eggTypes[i] = GetEggType(caller, BattleControl.Instance.turnCount, i * 17, false);
            }
        }
        else
        {
            for (int i = 0; i < eggTypes.Length; i++)
            {
                eggTypes[i] = GetEggType(caller, BattleControl.Instance.turnCount, i * 17, true);
            }
        }

        GameObject[] eggObjects = new GameObject[eggTypes.Length];
        for (int i = 0; i < eggTypes.Length; i++)
        {
            caller.SetAnimation("egglay", true);
            eggObjects[i] = MakeEggSprite(caller, eggTypes[i]);
            eggObjects[i].transform.localScale = Vector3.zero;
            yield return new WaitForSeconds(0.2f);
            StartCoroutine(EggAnimation(caller, eggTypes[i], 0.25f + 0.25f * i * (0.2f + 1f / eggTypes.Length), eggObjects[i]));
            yield return new WaitForSeconds(0.3f);
        }

        BattleEntity oldTarget = caller.curTarget;

        for (int i = 0; i < eggTypes.Length; i++)
        {
            yield return new WaitForSeconds(0.3f);
            Destroy(eggObjects[i]);
            ItemMove im = Item.GetItemMoveScript(new Item(eggTypes[i]));

            if (im.GetTargetArea(caller, 1).GetCheckerResult(caller, caller))
            {
                caller.curTarget = caller;
            } else
            {
                caller.curTarget = oldTarget;
            }

            if (!BattleControl.Instance.EntityValid(caller.curTarget))
            {
                List<BattleEntity> list = BattleControl.Instance.GetEntitiesSorted(caller, im.GetTargetArea(caller, 1), false);

                if (list.Count == 0)
                {
                    yield return new WaitForSeconds(0.3f);
                    for (int j = 0; j < eggObjects.Length; j++)
                    {
                        Destroy(eggObjects[j]);
                        if (eggObjects[j] == null)
                        {
                            continue;
                        }
                    }
                    caller.SetIdleAnimation();
                    yield break;
                }

                caller.curTarget = list[0];
            }

            //Bypasses ChooseMove
            yield return StartCoroutine(im.Execute(caller));

            BattleControl.Instance.playerData.GetPlayerDataEntry(caller.entityID).itemsUsed++;
            BattleControl.Instance.playerData.itemsUsed++;
        }

        yield return new WaitForSeconds(0.3f);
        for (int j = 0; j < eggObjects.Length; j++)
        {
            Destroy(eggObjects[j]);
            if (eggObjects[j] == null)
            {
                continue;
            }
        }
        caller.SetIdleAnimation();
    }

    public override void PreMove(BattleEntity caller, int level = 1)
    {
    }
    public override void PostMove(BattleEntity caller, int level = 1)
    {
    }

    public override string GetActionCommandDesc(int level = 1)
    {
        return AC_PressATimed.GetACDesc();
    }
}



//using this as a template for the others to condense the code
public class WM_Slash : WilexMove
{
    public WM_Slash()
    {
    }

    public override int GetTextIndex()
    {
        return 12;
    }


    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemyLowFrontmost, false);
    //public override float GetBasePower() => 1.5f;
    public override int GetBaseCost(int level = 1) => 0;
    public override BaseBattleMenu.BaseMenuName GetMoveType() => BaseBattleMenu.BaseMenuName.Weapon;

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        //Debug.Log(caller.id + " fly");
        if (!BattleControl.Instance.EntityValid(caller.curTarget))
        {
            caller.curTarget = null;
        }

        Vector3 target = caller.curTarget.ApplyScaledOffset(caller.curTarget.hammerOffset);
        target += Vector3.left * 0.5f * caller.width + 0.5f * Vector3.left;
        target.y = caller.homePos.y;
        caller.ac.MultiplyAnimationSpeed(1/0.65f);
        yield return StartCoroutine(caller.MoveEasing(target, (e) => MainManager.EasingOutIn(e), "weaponholdwalk"));
        caller.ac.MultiplyAnimationSpeed(0.65f);

        if (caller.curTarget != null)
        {
            caller.SetAnimation("idleweapon");
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
            PreparationAnimation(caller, sl, level);
            yield return new WaitUntil(() => actionCommand.IsComplete());

            bool result = actionCommand == null ? true : actionCommand.GetSuccess();
            if (actionCommand != null)
            {
                yield return new WaitForSeconds(ActionCommand.END_LAG);
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
        yield return StartCoroutine(caller.MoveEasing(caller.homePos, (e) => MainManager.EasingOutIn(e)));
    }

    public virtual float ActionCommandTime()
    {
        return 0.5f;
    }

    public virtual void PreparationAnimation(BattleEntity caller, int sl, int level = 1)
    {
        caller.SetAnimation("slash_prepare");
    }

    public virtual IEnumerator SwingAnimations(BattleEntity caller, int sl, int level = 1)
    {
        caller.SetAnimation("slash_e");
        GameObject eoS = null;
        switch (sl)
        {
            default:
            case 0:
                eoS = Instantiate(Resources.Load<GameObject>("VFX/Overworld/Player/Effect_SwordSlash0"), BattleControl.Instance.transform);
                break;
            case 1:
                eoS = Instantiate(Resources.Load<GameObject>("VFX/Overworld/Player/Effect_SwordSlash1"), BattleControl.Instance.transform);
                break;
            case 2:
                eoS = Instantiate(Resources.Load<GameObject>("VFX/Overworld/Player/Effect_SwordSlash2"), BattleControl.Instance.transform);
                break;
        }
        eoS.transform.position = transform.position + Vector3.up * 0.325f;
        MainManager.Instance.PlaySound(gameObject, MainManager.Sound.SFX_Overworld_Slash);

        //slightly before the end (So that damage is dealt before the effect completes)
        yield return new WaitForSeconds(0.1f);
    }
    public virtual bool GetOutcome(BattleEntity caller)
    {
        return caller.GetAttackHit(caller.curTarget, 0);
    }
    public virtual void DealDamage(BattleEntity caller, int sd, bool result)
    {
        //Debug.Log(result);
        ulong propertyBlock = (ulong)BattleHelper.DamageProperties.PierceOne;
        if (result)
        {
            propertyBlock |= (ulong)BattleHelper.DamageProperties.AC_Success;
            caller.curTarget.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.Spin);
            caller.DealDamage(caller.curTarget, sd, BattleHelper.DamageType.Normal, propertyBlock, BattleHelper.ContactLevel.Weapon);
        }
        else
        {
            caller.curTarget.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.Spin);
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

        ulong propertyBlock = (ulong)BattleHelper.DamageProperties.PierceOne + (ulong)BattleHelper.DamageProperties.AC_Success;
        int val = caller.DealDamageCalculation(target, sd, 0, propertyBlock);

        return val + "";
    }

    public override string GetActionCommandDesc(int level = 1)
    {
        return AC_HoldLeft.GetACDesc();
    }
}

public class WM_ElectroSlash : WM_Slash
{
    public WM_ElectroSlash()
    {
    }

    public override int GetTextIndex()
    {
        return 13;
    }


    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemyLowFrontmost, false);
    //public override float GetBasePower() => 1.5f;
    public override int GetBaseCost(int level = 1) => BaseCostCalculation(4, level, 3);
    public override BaseBattleMenu.BaseMenuName GetMoveType() => BaseBattleMenu.BaseMenuName.Weapon;

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

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        //Debug.Log(caller.id + " fly");
        if (!BattleControl.Instance.EntityValid(caller.curTarget))
        {
            caller.curTarget = null;
        }

        int hits = level;
        float delay = 0.5f / hits;
        if (delay < 0.15f)
        {
            delay = 0.15f;
        }

        Vector3 target = caller.curTarget.ApplyScaledOffset(caller.curTarget.hammerOffset);
        target += Vector3.left * 0.5f * caller.width + 0.5f * Vector3.left;


        caller.ac.MultiplyAnimationSpeed(1 / 0.65f);
        yield return StartCoroutine(caller.MoveEasing(target, (e) => MainManager.EasingOutIn(e), "weaponholdwalk"));
        caller.ac.MultiplyAnimationSpeed(0.65f);

        if (caller.curTarget != null)
        {
            caller.SetAnimation("idleweapon");
            int sd = 2;
            int sl = 0;

            AC_MashLeft actionCommand = null;
            if (caller is PlayerEntity pcaller)
            {
                sd = pcaller.GetWeaponDamage();
                sl = pcaller.GetWeaponLevel();
                actionCommand = gameObject.AddComponent<AC_MashLeft>();
                actionCommand.Init(pcaller);
                actionCommand.Setup(1f, 5);
            }

            yield return new WaitUntil(() => actionCommand.IsStarted());
            caller.SetAnimation("slash_prepare");
            yield return new WaitUntil(() => actionCommand.IsComplete());

            bool result = actionCommand == null ? true : actionCommand.GetSuccess();
            if (actionCommand != null)
            {
                yield return new WaitForSeconds(ActionCommand.END_LAG);
                actionCommand.End();
                Destroy(actionCommand);
            }

            if (!result)
            {
                hits = 1;
            }
            StartCoroutine(SwingAnimations(caller, sl, hits));
            yield return new WaitForSeconds(0.1f);
            if (GetOutcome(caller))
            {
                for (int i = 0; i < hits; i++)
                {
                    if (i < hits - 1)
                    {
                        DealDamage(caller, sd, i, result, false);
                        yield return new WaitForSeconds(delay);
                    }
                    else
                    {
                        DealDamage(caller, sd, i, result, true);
                    }
                }
                yield return new WaitForSeconds(0.1f);
            }
            else
            {
                //Miss
                caller.InvokeMissEvents(caller.curTarget);
                //yield return StartCoroutine(caller.SpinHeavy(Vector3.up * 360, 0.15f));
            }
        }
        yield return StartCoroutine(caller.MoveEasing(caller.homePos, (e) => MainManager.EasingOutIn(e)));
    }

    public override float ActionCommandTime()
    {
        return 0.5f;
    }
    public override IEnumerator SwingAnimations(BattleEntity caller, int sl, int hits = 1)
    {
        caller.SetAnimation("slash_e");
        if (!GetOutcome(caller))
        {
            hits = 1;
        }

        float delay = 0.5f / hits;
        if (delay < 0.15f)
        {
            delay = 0.15f;
        }

        bool flip = false;

        GameObject eoS;
        
        for (int i = 0; i < hits; i++)
        {
            eoS = null;
            eoS = Instantiate(Resources.Load<GameObject>("VFX/Battle/Moves/Player/Effect_ElectroSlash"), BattleControl.Instance.transform);
            /*
            switch (sl)
            {
                default:
                case 0:
                    eoS = Instantiate(Resources.Load<GameObject>("VFX/Overworld/Player/Effect_SwordSlash0"), BattleControl.Instance.transform);
                    break;
                case 1:
                    eoS = Instantiate(Resources.Load<GameObject>("VFX/Overworld/Player/Effect_SwordSlash1"), BattleControl.Instance.transform);
                    break;
                case 2:
                    eoS = Instantiate(Resources.Load<GameObject>("VFX/Overworld/Player/Effect_SwordSlash2"), BattleControl.Instance.transform);
                    break;
            }
            */
            eoS.transform.position = transform.position + Vector3.up * 0.325f;
            MainManager.Instance.PlaySound(gameObject, MainManager.Sound.SFX_Overworld_Slash);

            //2 hits: 30, 30
            //4 hits: 30, 30, 15, 15
            //6 hits: 30, 30, 15, 15, -30, -30

            int k = i / 2;
            float angle = 30 * (1 - k);
            if (hits > 6)
            {
                angle = 30 * (((hits / 2f - 0.5f) - k) / (hits / 2f - 0.5f));
            }

            if (angle <= 10 && angle >= 0)
            {
                angle = 15;
            }
            if (angle < 0 && angle >= -10)
            {
                angle = -15;
            }

            eoS.transform.localEulerAngles = Vector3.right * angle * (flip ? -1 : 1);

            if (flip)
            {
                eoS.transform.localScale = new Vector3(0.65f, 0.8f, -0.65f);
            }

            //maybe I'll make the animations faster later too (but right now the slash fx doesn't speed up so it might not look right)
            //caller.ac.animator.speed = 1;

            if (flip)
            {
                if (angle > 15)
                {
                    caller.SetAnimation("slash_diagrightreverse", true);
                }
                else if (angle < -15)
                {
                    caller.SetAnimation("slash_diagleftreverse", true);
                }
                else
                {
                    caller.SetAnimation("slash_ereverse", true);
                }
            }
            else
            {
                if (angle > 15)
                {
                    caller.SetAnimation("slash_diagleft", true);
                }
                else if (angle < -15)
                {
                    caller.SetAnimation("slash_diagright", true);
                }
                else
                {
                    caller.SetAnimation("slash_e", true);
                }
            }
            flip = !flip;

            yield return new WaitForSeconds(delay);
        }
        caller.ac.ResetAnimationSpeed();

        //slightly before the end (So that damage is dealt before the effect completes)
        //yield return new WaitForSeconds(0.1f);
    }
    public override bool GetOutcome(BattleEntity caller)
    {
        return caller.GetAttackHit(caller.curTarget, 0);
    }
    public void DealDamage(BattleEntity caller, int sd, int index, bool result, bool end)
    {
        ulong propertyBlock = 0;
        if (!end)
        {
            propertyBlock = (ulong)BattleHelper.DamageProperties.Combo;
        }

        if (result)
        {
            propertyBlock |= (ulong)BattleHelper.DamageProperties.AC_Success;
        }

        if (result)
        {
            caller.curTarget.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.SpinFast);
            caller.DealDamageMultihit(caller.curTarget, sd, BattleHelper.DamageType.Air, propertyBlock, BattleHelper.ContactLevel.Weapon, index, BattleHelper.MultihitReductionFormula.ReduceHalf);
        }
        else
        {
            caller.curTarget.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.SpinFast);
            caller.DealDamageMultihit(caller.curTarget, Mathf.CeilToInt((sd / 2f)), BattleHelper.DamageType.Air, propertyBlock, BattleHelper.ContactLevel.Weapon, index, BattleHelper.MultihitReductionFormula.ReduceHalf);
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

        string outstring = "";

        int hits = level;

        int val = 0; // caller.DealDamageMultihitCalculation(target, sd + 1, 0, 0, 0, BattleHelper.MultihitReductionFormula.ReduceHalf);

        for (int i = 0; i < hits; i++)
        {
            val = caller.DealDamageMultihitCalculation(target, sd, BattleHelper.DamageType.Air, 0, i, BattleHelper.MultihitReductionFormula.ReduceHalf);
            outstring += val;
            if (i > 0)
            {
                outstring += "?";
            }
            if (i < hits - 1)
            {
                outstring += ", ";
            }
        }

        return outstring;
    }

    public override string GetActionCommandDesc(int level = 1)
    {
        return AC_MashLeft.GetACDesc();
    }
}

public class WM_SlipSlash : WilexMove
{
    public WM_SlipSlash()
    {
    }

    public override int GetTextIndex()
    {
        return 14;
    }


    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemyLow, false);
    //public override float GetBasePower() => 1.5f;
    public override int GetBaseCost(int level = 1) => BaseCostCalculation(5, level, 3);
    public override BaseBattleMenu.BaseMenuName GetMoveType() => BaseBattleMenu.BaseMenuName.Weapon;

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


        if (caller.curTarget != null)
        {
            //get list of entities to pass through
            List<BattleEntity> targets = BattleControl.Instance.GetEntitiesSorted(caller, GetBaseTarget());

            //Debug.Log(targets);

            for (int i = 0; i < targets.Count; i++)
            {
                //Debug.Log(targets[i]);
                if (targets[i].homePos.x > caller.curTarget.homePos.x)
                {
                    //Debug.Log(targets[i].homePos.x + " " + caller.curTarget.homePos.x);
                    targets.RemoveAt(i);
                    i--;
                }
            }

            Vector3 midpoint = BattleControl.Instance.GetFrontmostLow(caller).transform.position + Vector3.left * 1.5f + Vector3.forward * 1.5f;

            Vector3 target = caller.curTarget.ApplyScaledOffset(caller.curTarget.hammerOffset) + (caller.width / 2) * Vector3.left + Vector3.left * 0.3f;

            int sd = 2;
            int sl = 0;

            AC_HoldLeft actionCommand = null;
            if (caller is PlayerEntity pcaller)
            {
                sd = pcaller.GetWeaponDamage();
                sl = pcaller.GetWeaponLevel();
                actionCommand = gameObject.AddComponent<AC_HoldLeft>();
                actionCommand.Init(pcaller);
                actionCommand.Setup(caller.GetMoveTime(target, caller.entitySpeed * 1.5f) + 0.25f + ActionCommand.TIMING_WINDOW);
            }

            yield return new WaitUntil(() => actionCommand.IsStarted());

            yield return StartCoroutine(caller.MoveEasing(midpoint, caller.entitySpeed * 1.5f, (e) => MainManager.EasingOut(e)));
            //doing multiple things at once

            bool move = true;
            IEnumerator MoveTracked()
            {
                yield return StartCoroutine(caller.MoveEasing(target, caller.entitySpeed * 1.5f, (e) => (MainManager.EasingIn(e)), "slipslashwalk"));
                move = false;
            }

            StartCoroutine(MoveTracked());

            while (move)
            {
                for (int i = 0; i < targets.Count; i++)
                {
                    if (targets[i].homePos.x <= caller.transform.position.x)
                    {
                        //Debug.Log(targets[i]);
                        if (caller.GetAttackHit(targets[i], 0))
                        {
                            //Note: this is before the action command is done, so these are independent of action command
                            ulong propertyBlockS = (ulong)BattleHelper.DamageProperties.AC_Premature;
                            switch (level)
                            {
                                case 1:
                                    caller.curTarget.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.Spin);
                                    caller.DealDamage(targets[i], sd - 2, BattleHelper.DamageType.Normal, propertyBlockS, BattleHelper.ContactLevel.Weapon);
                                    break;
                                case 2:
                                    caller.curTarget.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.Spin);
                                    caller.DealDamage(targets[i], sd - 1, BattleHelper.DamageType.Normal, propertyBlockS, BattleHelper.ContactLevel.Weapon);
                                    break;
                                default:
                                    caller.curTarget.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.Spin);
                                    caller.DealDamage(targets[i], sd + level - 3, BattleHelper.DamageType.Normal, propertyBlockS, BattleHelper.ContactLevel.Weapon);
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

            //failsafe
            for (int i = 0; i < targets.Count; i++)
            {
                if (targets[i] == null || targets[i].homePos.x >= caller.curTarget.homePos.x)
                {
                    break;
                }

                //Debug.Log(targets[i]);
                if (caller.GetAttackHit(targets[i], 0))
                {
                    //Note: this is before the action command is done, so these are independent of action command
                    ulong propertyBlockS = (ulong)BattleHelper.DamageProperties.AC_Premature;
                    switch (level)
                    {
                        case 1:
                            caller.curTarget.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.Spin);
                            caller.DealDamage(targets[i], sd - 2, BattleHelper.DamageType.Normal, propertyBlockS, BattleHelper.ContactLevel.Weapon);
                            break;
                        case 2:
                            caller.curTarget.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.Spin);
                            caller.DealDamage(targets[i], sd - 1, BattleHelper.DamageType.Normal, propertyBlockS, BattleHelper.ContactLevel.Weapon);
                            break;
                        default:
                            caller.curTarget.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.Spin);
                            caller.DealDamage(targets[i], sd + level - 3, BattleHelper.DamageType.Normal, propertyBlockS, BattleHelper.ContactLevel.Weapon);
                            break;
                    }
                }
                else
                {
                    caller.InvokeMissEvents(targets[i]);
                }
                targets.RemoveAt(i);
                i--;
            }

            caller.SetAnimation("slash_prepare");
            yield return new WaitUntil(() => actionCommand.IsComplete());

            caller.SetAnimation("slash_e");
            yield return StartCoroutine(SwingAnimations(caller, sl, level));
            bool result = actionCommand == null ? true : actionCommand.GetSuccess();
            if (actionCommand != null)
            {
                yield return new WaitForSeconds(ActionCommand.END_LAG);
                actionCommand.End();
                Destroy(actionCommand);
            }
            ulong propertyBlock = (ulong)BattleHelper.DamageProperties.AC_Success;
            if (caller.GetAttackHit(caller.curTarget, 0))
            {
                if (result)
                {
                    switch (level)
                    {
                        case 1:
                            caller.curTarget.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.Spin);
                            caller.DealDamage(caller.curTarget, sd + 1, BattleHelper.DamageType.Normal, propertyBlock, BattleHelper.ContactLevel.Weapon);
                            break;
                        case 2:
                            caller.curTarget.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.Spin);
                            caller.DealDamage(caller.curTarget, sd + 3, BattleHelper.DamageType.Normal, propertyBlock, BattleHelper.ContactLevel.Weapon);
                            break;
                        default:
                            caller.curTarget.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.Spin);
                            caller.DealDamage(caller.curTarget, sd + level - 1, BattleHelper.DamageType.Normal, propertyBlock, BattleHelper.ContactLevel.Weapon);
                            break;
                    }
                }
                else
                {
                    switch (level)
                    {
                        case 1:
                            caller.curTarget.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.Spin);
                            caller.DealDamage(caller.curTarget, Mathf.CeilToInt((sd / 2f)) + 1, BattleHelper.DamageType.Normal, 0, BattleHelper.ContactLevel.Weapon);
                            break;
                        case 2:
                            caller.curTarget.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.Spin);
                            caller.DealDamage(caller.curTarget, Mathf.CeilToInt((sd / 2f)) + 3, BattleHelper.DamageType.Normal, 0, BattleHelper.ContactLevel.Weapon);
                            break;
                        default:
                            caller.curTarget.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.Spin);
                            caller.DealDamage(caller.curTarget, Mathf.CeilToInt((sd / 2f)) + level - 1, BattleHelper.DamageType.Normal, 0, BattleHelper.ContactLevel.Weapon);
                            break;
                    }
                }
            }
            else
            {
                //Miss
                caller.InvokeMissEvents(caller.curTarget);
                yield return StartCoroutine(caller.SpinHeavy(Vector3.up * 360, 0.15f));
            }
        }
        yield return StartCoroutine(caller.MoveEasing(caller.homePos, (e) => MainManager.EasingOutIn(e)));
    }

    public virtual IEnumerator SwingAnimations(BattleEntity caller, int sl, int level = 1)
    {
        GameObject eoS = null;
        switch (sl)
        {
            default:
            case 0:
                eoS = Instantiate(Resources.Load<GameObject>("VFX/Overworld/Player/Effect_SwordSlash0"), BattleControl.Instance.transform);
                break;
            case 1:
                eoS = Instantiate(Resources.Load<GameObject>("VFX/Overworld/Player/Effect_SwordSlash1"), BattleControl.Instance.transform);
                break;
            case 2:
                eoS = Instantiate(Resources.Load<GameObject>("VFX/Overworld/Player/Effect_SwordSlash2"), BattleControl.Instance.transform);
                break;
        }
        eoS.transform.position = transform.position + Vector3.up * 0.325f;
        MainManager.Instance.PlaySound(gameObject, MainManager.Sound.SFX_Overworld_Slash);

        //slightly before the end (So that damage is dealt before the effect completes)
        yield return new WaitForSeconds(0.1f);
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

        int val = 0;
        int slipVal = 0;
        
        switch (level)
        {
            case 1:
                val = caller.DealDamageCalculation(target, sd + 1, 0, (ulong)BattleHelper.DamageProperties.AC_Success);
                slipVal = caller.DealDamageCalculation(target, sd - 2, 0, (ulong)BattleHelper.DamageProperties.AC_Premature);
                break;
            case 2:
                val = caller.DealDamageCalculation(target, sd + 3, 0, (ulong)BattleHelper.DamageProperties.AC_Success);
                slipVal = caller.DealDamageCalculation(target, sd - 1, 0, (ulong)BattleHelper.DamageProperties.AC_Premature);
                break;
            default:
                val = caller.DealDamageCalculation(target, sd + level - 1, 0, (ulong)BattleHelper.DamageProperties.AC_Success);
                slipVal = caller.DealDamageCalculation(target, sd + level - 3, 0, (ulong)BattleHelper.DamageProperties.AC_Premature);
                break;
        }


        return val + "?<line>Slip: " + slipVal + "?";
    }

    public override string GetActionCommandDesc(int level = 1)
    {
        return AC_HoldLeft.GetACDesc();
    }
}

public class WM_PoisonSlash : WM_Slash
{
    public WM_PoisonSlash()
    {
    }

    public override int GetTextIndex()
    {
        return 15;
    }


    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemyLowFrontmost, false);
    //public override float GetBasePower() => 1.5f;
    public override int GetBaseCost(int level = 1) => BaseCostCalculation(7, level, 2);
    public override int GetCost(BattleEntity caller, int level = 1)
    {
        return CostCalculation(caller, level, 2);
    }
    public override BaseBattleMenu.BaseMenuName GetMoveType() => BaseBattleMenu.BaseMenuName.Weapon;

    public override void DealDamage(BattleEntity caller, int sd, bool result)
    {
        if (result)
        {
            ulong propertyBlock = (ulong)BattleHelper.DamageProperties.AC_Success;
            caller.curTarget.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.Spin);
            caller.DealDamage(caller.curTarget, sd, BattleHelper.DamageType.Normal, propertyBlock, BattleHelper.ContactLevel.Weapon);
            caller.InflictEffect(caller.curTarget, new Effect(Effect.EffectType.Poison, 1, (sbyte)(1 + level * 2)), caller.posId);
        }
        else
        {
            caller.curTarget.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.Spin);
            caller.DealDamage(caller.curTarget, Mathf.CeilToInt((sd / 2f)), BattleHelper.DamageType.Normal, 0, BattleHelper.ContactLevel.Weapon);
            caller.InflictEffect(caller.curTarget, new Effect(Effect.EffectType.Poison, 1, (sbyte)(1 + level)), caller.posId);
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

            if (pcallerA.BadgeEquipped(Badge.BadgeType.AilmentSight))
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

        int val = caller.DealDamageCalculation(target, sd, 0, (ulong)BattleHelper.DamageProperties.AC_Success);

        int statusHP = (int)(target.StatusWorkingHP(Effect.EffectType.Poison) / statusBoost);

        bool doesWork = (target.hp > 0) && (target.hp - val <= statusHP);

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

    public override string GetActionCommandDesc(int level = 1)
    {
        return AC_HoldLeft.GetACDesc();
    }
}

public class WM_PreciseStab : WM_Slash
{
    public WM_PreciseStab()
    {
    }

    public override int GetTextIndex()
    {
        return 16;
    }


    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemyLowFrontmost, false);
    //public override float GetBasePower() => 1.5f;
    public override int GetBaseCost(int level = 1) => BaseCostCalculation(10, level, 3);
    public override BaseBattleMenu.BaseMenuName GetMoveType() => BaseBattleMenu.BaseMenuName.Weapon;

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

    public override void PreparationAnimation(BattleEntity caller, int sl, int level = 1)
    {
        caller.SetAnimation("stab_prepare");
    }

    public override IEnumerator SwingAnimations(BattleEntity caller, int sl, int level = 1)
    {
        caller.SetAnimation("stab");
        //Particles
        GameObject particle = Instantiate(Resources.Load<GameObject>("VFX/Battle/Moves/Player/Effect_PreciseStab"), BattleControl.Instance.transform);
        particle.transform.position = caller.ApplyScaledOffset(Vector3.up * 0.5f) + Vector3.right * 0.3f;

        //slightly before the end (So that damage is dealt before the effect completes)
        yield return new WaitForSeconds(0.1f);
    }
    public override bool GetOutcome(BattleEntity caller)
    {
        return caller.GetAttackHit(caller.curTarget, BattleHelper.DamageType.Water);
    }
    public override void DealDamage(BattleEntity caller, int sd, bool result)
    {
        ulong propertyBlock = (ulong)(BattleHelper.DamageProperties.AdvancedElementCalc);
        if (result)
        {
            ulong propertyBlockB = (ulong)BattleHelper.DamageProperties.AC_Success;
            propertyBlock |= (ulong)BattleHelper.DamageProperties.AC_Success;
            switch (level)
            {
                case 1:
                    caller.curTarget.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.ReverseSquish);
                    caller.DealDamage(caller.curTarget, sd + 3, BattleHelper.DamageType.Water, propertyBlockB, BattleHelper.ContactLevel.Weapon);
                    break;
                case 2:
                    caller.curTarget.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.ReverseSquish);
                    caller.DealDamage(caller.curTarget, sd + 6, BattleHelper.DamageType.Water, propertyBlock, BattleHelper.ContactLevel.Weapon);
                    break;
                default:
                    caller.curTarget.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.ReverseSquish);
                    caller.DealDamage(caller.curTarget, sd + 2 + level * 2, BattleHelper.DamageType.Water, propertyBlock, BattleHelper.ContactLevel.Weapon);
                    break;
            }
        }
        else
        {
            switch (level)
            {
                case 1:
                    caller.curTarget.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.ReverseSquish);
                    caller.DealDamage(caller.curTarget, Mathf.CeilToInt((sd / 2f)) + 3, BattleHelper.DamageType.Water, 0, BattleHelper.ContactLevel.Weapon);
                    break;
                case 2:
                    caller.curTarget.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.ReverseSquish);
                    caller.DealDamage(caller.curTarget, Mathf.CeilToInt((sd / 2f)) + 6, BattleHelper.DamageType.Water, propertyBlock, BattleHelper.ContactLevel.Weapon);
                    break;
                default:
                    caller.curTarget.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.ReverseSquish);
                    caller.DealDamage(caller.curTarget, Mathf.CeilToInt((sd / 2f)) + 2 + level * 2, BattleHelper.DamageType.Water, propertyBlock, BattleHelper.ContactLevel.Weapon);
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

        int val = 0;

        ulong propertyBlock = (ulong)(BattleHelper.DamageProperties.AdvancedElementCalc) + (ulong)BattleHelper.DamageProperties.AC_Success;
        switch (level)
        {
            case 1:
                val = caller.DealDamageCalculation(target, sd + 3, BattleHelper.DamageType.Water, (ulong)BattleHelper.DamageProperties.AC_Success);
                break;
            case 2:
                val = caller.DealDamageCalculation(target, sd + 6, BattleHelper.DamageType.Water, propertyBlock);
                break;
            default:
                val = caller.DealDamageCalculation(target, sd + 2 + level * 2, BattleHelper.DamageType.Water, propertyBlock);
                break;
        }

        return val + "";
    }

    public override string GetActionCommandDesc(int level = 1)
    {
        return AC_HoldLeft.GetACDesc();
    }
}

public class WM_SwordBolt : WilexMove
{
    public WM_SwordBolt()
    {
    }

    public override int GetTextIndex()
    {
        return 17;
    }


    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemy, false);
    //public override float GetBasePower() => 1.5f;
    public override int GetBaseCost(int level = 1) => BaseCostCalculation(11, level, 4);
    public override int GetCost(BattleEntity caller, int level = 1)
    {
        return CostCalculation(caller, level, 4);
    }
    public override BaseBattleMenu.BaseMenuName GetMoveType() => BaseBattleMenu.BaseMenuName.Weapon;

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        //Debug.Log(caller.id + " fly");
        if (!BattleControl.Instance.EntityValid(caller.curTarget))
        {
            caller.curTarget = null;
        }

        //Vector3 target = caller.curTarget.transform.position - caller.curTarget.width * Vector3.right;
        //target += Vector3.left * 0.5f;
        //yield return StartCoroutine(caller.MoveEasing(target));

        if (caller.curTarget != null)
        {
            int sd = 2;

            AC_MashLeftRight actionCommand = null;
            if (caller is PlayerEntity pcaller)
            {
                sd = pcaller.GetWeaponDamage();
                actionCommand = gameObject.AddComponent<AC_MashLeftRight>();
                actionCommand.Init(pcaller);
                actionCommand.Setup(1.5f, 8);
            }

            yield return new WaitUntil(() => actionCommand.IsStarted());
            yield return new WaitUntil(() => actionCommand.IsComplete());

            yield return StartCoroutine(caller.SpinHeavy(Vector3.up * 360, 0.15f));
            bool result = actionCommand == null ? true : actionCommand.GetSuccess();
            if (actionCommand != null)
            {
                yield return new WaitForSeconds(ActionCommand.END_LAG);
                actionCommand.End();
                Destroy(actionCommand);
            }
            if (caller.GetAttackHit(caller.curTarget, BattleHelper.DamageType.Air))
            {
                //ulong propertyBlock = (ulong)BattleHelper.DamageProperties.PierceOne;
                if (result)
                {
                    ulong propertyBlock = (ulong)BattleHelper.DamageProperties.AC_Success;
                    caller.DealDamage(caller.curTarget, sd - 1 + level * 4, BattleHelper.DamageType.Air, propertyBlock, BattleHelper.ContactLevel.Infinite);
                }
                else
                {
                    caller.DealDamage(caller.curTarget, Mathf.CeilToInt((sd / 2f)) - 1 + level * 4, BattleHelper.DamageType.Air, 0, BattleHelper.ContactLevel.Infinite);
                }
            }
            else
            {
                //Miss
                caller.InvokeMissEvents(caller.curTarget);
                yield return StartCoroutine(caller.SpinHeavy(Vector3.up * 360, 0.15f));
            }
        }
        //yield return StartCoroutine(caller.MoveEasing(caller.homePos));
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

        int val = caller.DealDamageCalculation(target, sd - 1 + level * 4, BattleHelper.DamageType.Air, (ulong)BattleHelper.DamageProperties.AC_Success);

        return val + "";
    }

    public override string GetActionCommandDesc(int level = 1)
    {
        return AC_MashLeftRight.GetACDesc();
    }
}

public class WM_SwordDance : WilexMove
{
    public WM_SwordDance()
    {
    }

    public override int GetTextIndex()
    {
        return 18;
    }


    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemyLow, false);
    //public override float GetBasePower() => 1.5f;
    public override int GetBaseCost(int level = 1) => BaseCostCalculation(12, level, 4);
    public override int GetCost(BattleEntity caller, int level = 1)
    {
        return CostCalculation(caller, level, 4);
    }
    public override BaseBattleMenu.BaseMenuName GetMoveType() => BaseBattleMenu.BaseMenuName.Weapon;

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        //Debug.Log(caller.id + " fly");
        if (!BattleControl.Instance.EntityValid(caller.curTarget))
        {
            caller.curTarget = null;
        }


        if (caller.curTarget != null)
        {
            Vector3 midpoint = BattleControl.Instance.GetFrontmostLow(caller).transform.position + Vector3.left * 1.5f + Vector3.forward * 1.5f;

            Vector3 target = caller.curTarget.ApplyScaledOffset(caller.curTarget.hammerOffset) + (caller.width / 2) * Vector3.left;

            int sd = 2;
            int sl = 0;

            AC_HoldLeft actionCommand = null;
            if (caller is PlayerEntity pcaller)
            {
                sd = pcaller.GetWeaponDamage();
                sl = pcaller.GetWeaponLevel();
                actionCommand = gameObject.AddComponent<AC_HoldLeft>();
                actionCommand.Init(pcaller);
                actionCommand.Setup(caller.GetMoveTime(target, caller.entitySpeed * 1.5f) + 0.25f + ActionCommand.TIMING_WINDOW);
            }

            yield return new WaitUntil(() => actionCommand.IsStarted());


            caller.ac.MultiplyAnimationSpeed(1 / 0.65f);
            yield return StartCoroutine(caller.MoveEasing(midpoint, caller.entitySpeed * 1.5f, (e) => MainManager.EasingOut(e), "weaponholdwalk"));

            yield return StartCoroutine(caller.MoveEasing(target, caller.entitySpeed * 1.5f, (e) => MainManager.EasingIn(e), "weaponholdwalk"));
            caller.ac.MultiplyAnimationSpeed(0.65f);

            caller.SetAnimation("slash_prepare");
            yield return new WaitUntil(() => actionCommand.IsComplete());

            caller.SetAnimation("slash_e");
            yield return StartCoroutine(SwingAnimations(caller, sl, level));
            bool result = actionCommand == null ? true : actionCommand.GetSuccess();
            if (actionCommand != null)
            {
                yield return new WaitForSeconds(ActionCommand.END_LAG);
                actionCommand.End();
                Destroy(actionCommand);
            }
            if (caller.GetAttackHit(caller.curTarget, 0))
            {
                ulong properties = (ulong)BattleHelper.DamageProperties.PreserveFocus;
                if (result)
                {
                    properties |= (ulong)BattleHelper.DamageProperties.AC_Success;
                    caller.curTarget.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.Spin);
                    caller.DealDamage(caller.curTarget, sd + 3 - 3 * level, BattleHelper.DamageType.Normal, properties, BattleHelper.ContactLevel.Weapon);
                    caller.InflictEffect(caller, new Effect(Effect.EffectType.Focus, (sbyte)(level * 3), Effect.INFINITE_DURATION));
                }
                else
                {
                    caller.curTarget.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.Spin);
                    caller.DealDamage(caller.curTarget, Mathf.CeilToInt((sd / 2f) + 3 - 3 * level), BattleHelper.DamageType.Normal, properties, BattleHelper.ContactLevel.Weapon);
                    caller.InflictEffect(caller, new Effect(Effect.EffectType.Focus, (sbyte)(level), Effect.INFINITE_DURATION));
                }
            }
            else
            {
                //Miss
                caller.InvokeMissEvents(caller.curTarget);
                yield return StartCoroutine(caller.SpinHeavy(Vector3.up * 360, 0.15f));
            }
        }

        yield return StartCoroutine(caller.MoveEasing(caller.homePos, (e) => MainManager.EasingOutIn(e)));
    }

    public virtual IEnumerator SwingAnimations(BattleEntity caller, int sl, int level = 1)
    {
        GameObject eoS = null;
        switch (sl)
        {
            default:
            case 0:
                eoS = Instantiate(Resources.Load<GameObject>("VFX/Overworld/Player/Effect_SwordSlash0"), BattleControl.Instance.transform);
                break;
            case 1:
                eoS = Instantiate(Resources.Load<GameObject>("VFX/Overworld/Player/Effect_SwordSlash1"), BattleControl.Instance.transform);
                break;
            case 2:
                eoS = Instantiate(Resources.Load<GameObject>("VFX/Overworld/Player/Effect_SwordSlash2"), BattleControl.Instance.transform);
                break;
        }
        eoS.transform.position = transform.position + Vector3.up * 0.325f;
        MainManager.Instance.PlaySound(gameObject, MainManager.Sound.SFX_Overworld_Slash);

        //slightly before the end (So that damage is dealt before the effect completes)
        yield return new WaitForSeconds(0.1f);
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

        int val = 0;

        ulong properties = (ulong)BattleHelper.DamageProperties.PreserveFocus + (ulong)BattleHelper.DamageProperties.AC_Success;
        val = caller.DealDamageCalculation(target, sd + 3 - 3 * level, 0, properties);

        return val + "";
    }

    public override string GetActionCommandDesc(int level = 1)
    {
        return AC_HoldLeft.GetACDesc();
    }
}

public class WM_BoomerangSlash : WilexMove
{
    public WM_BoomerangSlash()
    {
    }

    public override int GetTextIndex()
    {
        return 19;
    }


    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemy, false);
    //public override float GetBasePower() => 1.5f;
    public override int GetBaseCost(int level = 1) => BaseCostCalculation(13, level, 4);
    public override int GetCost(BattleEntity caller, int level = 1)
    {
        return CostCalculation(caller, level, 4);
    }
    public override BaseBattleMenu.BaseMenuName GetMoveType() => BaseBattleMenu.BaseMenuName.Weapon;

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        //Debug.Log(caller.id + " fly");
        if (!BattleControl.Instance.EntityValid(caller.curTarget))
        {
            caller.curTarget = null;
        }

        //Vector3 target = caller.curTarget.transform.position - caller.curTarget.width * Vector3.right;
        //target += Vector3.left * 0.5f;
        //yield return StartCoroutine(caller.MoveEasing(target));

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

            yield return StartCoroutine(caller.SpinHeavy(Vector3.up * 360, 0.15f));
            bool result = actionCommand == null ? true : actionCommand.GetSuccess();
            if (actionCommand != null)
            {
                yield return new WaitForSeconds(ActionCommand.END_LAG);
                actionCommand.End();
                Destroy(actionCommand);
            }
            if (caller.GetAttackHit(caller.curTarget, 0))
            {
                ulong propertyBlock = (ulong)BattleHelper.DamageProperties.Combo;
                ulong propertyBlockB = (ulong)BattleHelper.DamageProperties.AC_SuccessStall;
                if (result)
                {
                    propertyBlock |= (ulong)BattleHelper.DamageProperties.AC_Success;
                    caller.curTarget.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.Spin);
                    caller.DealDamage(caller.curTarget, sd - 2 + 2 * level, 0, propertyBlock, BattleHelper.ContactLevel.Infinite);
                }
                else
                {
                    caller.curTarget.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.Spin);
                    caller.DealDamage(caller.curTarget, Mathf.CeilToInt((sd / 2f)) - 2 + 2 * level, 0, propertyBlock, BattleHelper.ContactLevel.Infinite);
                }

                yield return new WaitForSeconds(0.5f);
                if (result)
                {
                    caller.curTarget.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.Spin);
                    caller.DealDamage(caller.curTarget, sd - 1 + 2 * level, 0, propertyBlockB, BattleHelper.ContactLevel.Infinite);
                }
                else
                {
                    caller.curTarget.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.Spin);
                    caller.DealDamage(caller.curTarget, Mathf.CeilToInt((sd / 2f)) - 1 + 2 * level, 0, 0, BattleHelper.ContactLevel.Infinite);
                }
            }
            else
            {
                //Miss
                caller.InvokeMissEvents(caller.curTarget);
                yield return StartCoroutine(caller.SpinHeavy(Vector3.up * 360, 0.15f));
            }
        }
        //yield return StartCoroutine(caller.MoveEasing(caller.homePos));
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

        int val = caller.DealDamageCalculation(target, sd - 2 + 2 * level, 0, (ulong)BattleHelper.DamageProperties.AC_Success);

        int hits = 2;

        string outstring = "";

        for (int i = 0; i < hits; i++)
        {
            outstring += val;
            if (i > 0)
            {
                outstring += "?";
            }
            if (i < hits - 1)
            {
                outstring += ", ";
                val = caller.DealDamageCalculation(target, sd - 1 + 2 * level, 0, (ulong)BattleHelper.DamageProperties.AC_Success);
            }
        }

        return outstring;
    }

    public override string GetActionCommandDesc(int level = 1)
    {
        return AC_HoldLeft.GetACDesc();
    }
}

public class WM_DarkSlash : WM_Slash
{
    public WM_DarkSlash()
    {
    }

    public override int GetTextIndex()
    {
        return 20;
    }


    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemyLowFrontmost, false);
    //public override float GetBasePower() => 1.5f;
    public override int GetBaseCost(int level = 1)
    {
        switch (level)
        {
            case 1: return 15;
            case 2: return 18;
        }
        return BaseCostCalculation(15, level, 6, -3);
    }
    public override BaseBattleMenu.BaseMenuName GetMoveType() => BaseBattleMenu.BaseMenuName.Weapon;

    public override int GetCost(BattleEntity caller, int level = 1)
    {
        switch (level)
        {
            case 1: return CostModification(caller, level, 15);
            case 2: return CostModification(caller, level, 18);
        }
        return CostCalculation(caller, level, 6,-3);
    }
    /*
    public override int GetMaxLevel(BattleEntity caller)
    {
        return 2;
    }
    */

    public override bool GetOutcome(BattleEntity caller)
    {
        return caller.GetAttackHit(caller.curTarget, BattleHelper.DamageType.Dark);
    }
    public override void DealDamage(BattleEntity caller, int sd, bool result)
    {
        ulong propertyBlock = (ulong)(BattleHelper.DamageProperties.AdvancedElementCalc);
        if (result)
        {
            ulong propertyBlockB = (ulong)BattleHelper.DamageProperties.AC_Success;
            propertyBlock |= (ulong)BattleHelper.DamageProperties.AC_Success;
            switch (level)
            {
                case 1:
                    caller.curTarget.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.Spin);
                    caller.DealDamage(caller.curTarget, sd + 7, BattleHelper.DamageType.Dark, propertyBlockB, BattleHelper.ContactLevel.Weapon);
                    break;
                case 2:
                    caller.curTarget.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.Spin);
                    caller.DealDamage(caller.curTarget, sd + 10, BattleHelper.DamageType.Dark, propertyBlock, BattleHelper.ContactLevel.Weapon);
                    break;
                default:
                    caller.curTarget.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.Spin);
                    caller.DealDamage(caller.curTarget, sd + 4 + 3 * level, BattleHelper.DamageType.Dark, propertyBlock, BattleHelper.ContactLevel.Weapon);
                    break;
            }
        }
        else
        {
            switch (level)
            {
                case 1:
                    caller.curTarget.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.Spin);
                    caller.DealDamage(caller.curTarget, Mathf.CeilToInt((sd / 2f)) + 7, BattleHelper.DamageType.Dark, 0, BattleHelper.ContactLevel.Weapon);
                    break;
                case 2:
                    caller.curTarget.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.Spin);
                    caller.DealDamage(caller.curTarget, Mathf.CeilToInt((sd / 2f)) + 10, BattleHelper.DamageType.Dark, propertyBlock, BattleHelper.ContactLevel.Weapon);
                    break;
                default:
                    caller.curTarget.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.Spin);
                    caller.DealDamage(caller.curTarget, Mathf.CeilToInt((sd / 2f)) + 4 + 3 * level, BattleHelper.DamageType.Dark, propertyBlock, BattleHelper.ContactLevel.Weapon);
                    break;
            }
        }
    }

    public override IEnumerator SwingAnimations(BattleEntity caller, int sl, int level = 1)
    {
        caller.SetAnimation("slash_e");
        GameObject eoS = null;
        eoS = Instantiate(Resources.Load<GameObject>("VFX/Battle/Moves/Player/Effect_DarkSlash"), BattleControl.Instance.transform);
        eoS.transform.position = transform.position + Vector3.up * 0.325f;

        //slightly before the end (So that damage is dealt before the effect completes)
        yield return new WaitForSeconds(0.1f);
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

        int val = 0;

        ulong propertyBlock = (ulong)(BattleHelper.DamageProperties.AdvancedElementCalc) + (ulong)BattleHelper.DamageProperties.AC_Success;
        switch (level)
        {
            case 1:
                val = caller.DealDamageCalculation(target, sd + 7, BattleHelper.DamageType.Dark, (ulong)BattleHelper.DamageProperties.AC_Success);
                break;
            case 2:
                val = caller.DealDamageCalculation(target, sd + 10, BattleHelper.DamageType.Dark, propertyBlock);
                break;
            default:
                val = caller.DealDamageCalculation(target, sd + 4 + 3 * level, BattleHelper.DamageType.Dark, propertyBlock);
                break;
        }

        return val + "";
    }

    public override string GetActionCommandDesc(int level = 1)
    {
        return AC_HoldLeft.GetACDesc();
    }
}

public class WM_Aetherize : WilexMove
{
    public WM_Aetherize()
    {
    }

    public override int GetTextIndex()
    {
        return 21;
    }


    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveAlly, false);
    //public override float GetBasePower() => 0.5f;
    public override int GetBaseCost(int level = 1)
    {
        switch (level)
        {
            case 1: return 4;
            case 2: return 8;
            case 3: return 16;
        }
        return BaseCostCalculation(4, level, 6, 0);
    }
    public override BaseBattleMenu.BaseMenuName GetMoveType() => BaseBattleMenu.BaseMenuName.Weapon;

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
            case 3: return CostModification(caller, level, 16);
        }
        return CostCalculation(caller, level, 6,0);
    }

    /*
    public override int GetMaxLevel(BattleEntity caller)
    {
        return 3;
    }
    */

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        AC_PressATimed actionCommand = null;
        if (caller is PlayerEntity pcaller) //we have technology
        {
            actionCommand = gameObject.AddComponent<AC_PressATimed>();
            actionCommand.Init(pcaller);
            actionCommand.Setup(0.5f);
        }

        yield return new WaitForSeconds(ActionCommand.FADE_IN_TIME);
        caller.SetAnimation("weaponholdidle");
        yield return new WaitUntil(() => actionCommand.IsComplete());

        bool result = actionCommand == null ? true : actionCommand.GetSuccess();
        if (actionCommand != null)
        {
            yield return new WaitForSeconds(ActionCommand.END_LAG);
            actionCommand.End();
            Destroy(actionCommand);
        }

        if (result)
        {
            BattleControl.Instance.CreateActionCommandEffect(BattleHelper.ActionCommandText.Nice, caller.GetDamageEffectPosition(), caller);

            switch (level)
            {
                case 1:
                    caller.InflictEffect(caller.curTarget, new Effect(Effect.EffectType.Ethereal, 1, 1));
                    caller.InflictEffectForce(caller.curTarget, new Effect(Effect.EffectType.Cooldown, 1, Effect.INFINITE_DURATION));
                    break;
                case 2:
                    List<BattleEntity> targets = BattleControl.Instance.GetEntitiesSorted(caller, GetTargetArea(caller, level));
                    foreach (BattleEntity b in targets)
                    {
                        caller.InflictEffect(b, new Effect(Effect.EffectType.Ethereal, 1, 1));
                        caller.InflictEffectForce(b, new Effect(Effect.EffectType.Cooldown, 1, Effect.INFINITE_DURATION));
                    }
                    break;
                case 3:
                    List<BattleEntity> targetsB = BattleControl.Instance.GetEntitiesSorted(caller, GetTargetArea(caller, level));
                    foreach (BattleEntity b in targetsB)
                    {
                        caller.InflictEffect(b, new Effect(Effect.EffectType.Ethereal, 1, 1));
                    }
                    break;
                default:
                    List<BattleEntity> targetsC = BattleControl.Instance.GetEntitiesSorted(caller, GetTargetArea(caller, level));
                    foreach (BattleEntity b in targetsC)
                    {
                        caller.InflictEffect(b, new Effect(Effect.EffectType.Ethereal, 1, (sbyte)(level - 3)));
                    }
                    break;
            }
        }
        else
        {
            switch (level)
            {
                case 1:
                    caller.InflictEffect(caller.curTarget, new Effect(Effect.EffectType.Ethereal, 1, 1));
                    caller.InflictEffectForce(caller.curTarget, new Effect(Effect.EffectType.Cooldown, 1, Effect.INFINITE_DURATION));
                    break;
                case 2:
                    List<BattleEntity> targets = BattleControl.Instance.GetEntitiesSorted(caller, GetTargetArea(caller, level));
                    foreach (BattleEntity b in targets)
                    {
                        caller.InflictEffect(b, new Effect(Effect.EffectType.Ethereal, 1, 1));
                        caller.InflictEffectForce(b, new Effect(Effect.EffectType.Cooldown, 1, Effect.INFINITE_DURATION));
                    }
                    break;
                case 3:
                    List<BattleEntity> targetsB = BattleControl.Instance.GetEntitiesSorted(caller, GetTargetArea(caller, level));
                    foreach (BattleEntity b in targetsB)
                    {
                        caller.InflictEffect(b, new Effect(Effect.EffectType.Ethereal, 1, 1));
                    }
                    break;
                case 4:
                    List<BattleEntity> targetsC = BattleControl.Instance.GetEntitiesSorted(caller, GetTargetArea(caller, level));
                    foreach (BattleEntity b in targetsC)
                    {
                        caller.InflictEffect(b, new Effect(Effect.EffectType.Ethereal, 1, (sbyte)(level - 2)));
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

    public override string GetActionCommandDesc(int level = 1)
    {
        return AC_PressATimed.GetACDesc();
    }
}

public class WM_FlameBat : WilexMove
{
    public WM_FlameBat()
    {
    }

    public override int GetTextIndex()
    {
        return 22;
    }


    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemy, false);
    //public override float GetBasePower() => 1.5f;
    public override int GetBaseCost(int level = 1) => BaseCostCalculation(8, level, 3);
    public override int GetCost(BattleEntity caller, int level = 1)
    {
        return CostCalculation(caller, level, 3);
    }
    public override BaseBattleMenu.BaseMenuName GetMoveType() => BaseBattleMenu.BaseMenuName.Weapon;

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        //Debug.Log(caller.id + " fly");
        if (!BattleControl.Instance.EntityValid(caller.curTarget))
        {
            caller.curTarget = null;
        }

        //Vector3 target = caller.curTarget.transform.position - caller.curTarget.width * Vector3.right;
        //target += Vector3.left * 0.5f;
        //yield return StartCoroutine(caller.MoveEasing(target));

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

            yield return StartCoroutine(caller.SpinHeavy(Vector3.up * 360, 0.15f));
            bool result = actionCommand == null ? true : actionCommand.GetSuccess();
            if (actionCommand != null)
            {
                yield return new WaitForSeconds(ActionCommand.END_LAG);
                actionCommand.End();
                Destroy(actionCommand);
            }
            if (caller.GetAttackHit(caller.curTarget, BattleHelper.DamageType.Fire))
            {
                if (result)
                {
                    ulong propertyBlock = (ulong)BattleHelper.DamageProperties.AC_Success;
                    caller.curTarget.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.ReverseSquish);
                    caller.DealDamage(caller.curTarget, Mathf.CeilToInt((sd / 2f) - 2 + level * 2), BattleHelper.DamageType.Fire, propertyBlock, BattleHelper.ContactLevel.Infinite);
                    caller.InflictEffectForce(caller, new Effect(Effect.EffectType.BonusTurns, 1, Effect.INFINITE_DURATION));
                }
                else
                {
                    caller.curTarget.SetSpecialHurtAnim(BattleHelper.SpecialHitAnim.ReverseSquish);
                    caller.DealDamage(caller.curTarget, Mathf.CeilToInt(sd / 4f) - 1 + level, BattleHelper.DamageType.Fire, 0, BattleHelper.ContactLevel.Infinite);
                }
            }
            else
            {
                //Miss
                caller.InvokeMissEvents(caller.curTarget);
                yield return StartCoroutine(caller.SpinHeavy(Vector3.up * 360, 0.15f));
            }
        }
        //yield return StartCoroutine(caller.MoveEasing(caller.homePos));
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

        int val = caller.DealDamageCalculation(target, Mathf.CeilToInt((sd / 2f) - 2 + level * 2), BattleHelper.DamageType.Fire, (ulong)BattleHelper.DamageProperties.AC_Success);

        return val + "";
    }

    public override string GetActionCommandDesc(int level = 1)
    {
        return AC_HoldLeft.GetACDesc();
    }
}

public class WM_AstralWall : WilexMove
{
    public WM_AstralWall()
    {
    }

    public override int GetTextIndex()
    {
        return 23;
    }


    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveAlly, false);
    //public override float GetBasePower() => 0.5f;
    public override int GetBaseCost(int level = 1)
    {
        switch (level)
        {
            case 1: return 8;
            case 2: return 12;
            case 3: return 16;
        }
        return BaseCostCalculation(8, level, 2, 4);
    }
    public override BaseBattleMenu.BaseMenuName GetMoveType() => BaseBattleMenu.BaseMenuName.Weapon;

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
        return CostCalculation(caller, level, 2, 4);
    }

    /*
    public override int GetMaxLevel(BattleEntity caller)
    {
        return 3;
    }
    */

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        AC_PressATimed actionCommand = null;
        if (caller is PlayerEntity pcaller) //we have technology
        {
            actionCommand = gameObject.AddComponent<AC_PressATimed>();
            actionCommand.Init(pcaller);
            actionCommand.Setup(0.5f);
        }

        yield return new WaitForSeconds(ActionCommand.FADE_IN_TIME);
        caller.SetAnimation("weaponholdidle");
        yield return new WaitUntil(() => actionCommand.IsComplete());

        bool result = actionCommand == null ? true : actionCommand.GetSuccess();
        if (actionCommand != null)
        {
            yield return new WaitForSeconds(ActionCommand.END_LAG);
            actionCommand.End();
            Destroy(actionCommand);
        }

        if (result)
        {
            BattleControl.Instance.CreateActionCommandEffect(BattleHelper.ActionCommandText.Nice, caller.GetDamageEffectPosition(), caller);

            switch (level)
            {
                case 1:
                    caller.InflictEffect(caller.curTarget, new Effect(Effect.EffectType.AstralWall, (sbyte)Mathf.CeilToInt(caller.maxHP / 4f), 3));
                    break;
                case 2:
                    List<BattleEntity> targets = BattleControl.Instance.GetEntitiesSorted(caller, GetTargetArea(caller, level));
                    foreach (BattleEntity b in targets)
                    {
                        caller.InflictEffect(b, new Effect(Effect.EffectType.AstralWall, (sbyte)Mathf.CeilToInt(b.maxHP / 4f), 3));
                    }
                    break;
                case 3:
                    List<BattleEntity> targetsB = BattleControl.Instance.GetEntitiesSorted(caller, GetTargetArea(caller, level));
                    foreach (BattleEntity b in targetsB)
                    {
                        caller.InflictEffect(b, new Effect(Effect.EffectType.AstralWall, (sbyte)Mathf.CeilToInt(b.maxHP / 4f), 3));
                        caller.InflictEffect(b, new Effect(Effect.EffectType.AstralRecovery, 1, 3));
                    }
                    break;
                default:
                    List<BattleEntity> targetsC = BattleControl.Instance.GetEntitiesSorted(caller, GetTargetArea(caller, level));
                    foreach (BattleEntity b in targetsC)
                    {
                        caller.InflictEffect(b, new Effect(Effect.EffectType.AstralWall, (sbyte)Mathf.CeilToInt(b.maxHP / (1.0001f + level)), (sbyte)(level)));
                        caller.InflictEffect(b, new Effect(Effect.EffectType.AstralRecovery, 1, (sbyte)(level)));
                    }
                    break;
            }
        }
        else
        {
            switch (level)
            {
                case 1:
                    caller.InflictEffect(caller, new Effect(Effect.EffectType.AstralWall, (sbyte)Mathf.CeilToInt(caller.maxHP / 2f), 3));
                    break;
                case 2:
                    List<BattleEntity> targets = BattleControl.Instance.GetEntitiesSorted(caller, GetTargetArea(caller, level));
                    foreach (BattleEntity b in targets)
                    {
                        caller.InflictEffect(b, new Effect(Effect.EffectType.AstralWall, (sbyte)Mathf.CeilToInt(b.maxHP / 2f), 3));
                    }
                    break;
                case 3:
                    List<BattleEntity> targetsB = BattleControl.Instance.GetEntitiesSorted(caller, GetTargetArea(caller, level));
                    foreach (BattleEntity b in targetsB)
                    {
                        caller.InflictEffect(b, new Effect(Effect.EffectType.AstralWall, (sbyte)Mathf.CeilToInt(b.maxHP / 2f), 3));
                        caller.InflictEffect(b, new Effect(Effect.EffectType.AstralRecovery, 1, 3));
                    }
                    break;
                default:
                    List<BattleEntity> targetsC = BattleControl.Instance.GetEntitiesSorted(caller, GetTargetArea(caller, level));
                    foreach (BattleEntity b in targetsC)
                    {
                        caller.InflictEffect(b, new Effect(Effect.EffectType.AstralWall, (sbyte)Mathf.CeilToInt((b.maxHP * 2) / (1.0001f + level)), (sbyte)(level)));
                        caller.InflictEffect(b, new Effect(Effect.EffectType.AstralRecovery, 1, (sbyte)(level)));
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

    public override string GetActionCommandDesc(int level = 1)
    {
        return AC_PressATimed.GetACDesc();
    }
}