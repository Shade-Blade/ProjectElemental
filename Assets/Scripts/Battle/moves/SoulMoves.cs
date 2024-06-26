using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SM_Hasten : SoulMove
{
    public SM_Hasten()
    {
    }

    public override int GetTextIndex()
    {
        return 0;
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
        return StandardCostCalculation(caller, level, 12);
    }

    /*
    public override int GetMaxLevel(BattleEntity caller)
    {
        if (caller is PlayerEntity pcaller)
        {
            return pcaller.GetSoulMoveMaxLevel(GetTextIndex());
        }
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
        CastAnimation(caller, level);
        yield return new WaitUntil(() => actionCommand.IsComplete());

        bool result = actionCommand == null ? true : actionCommand.GetSuccess();
        if (actionCommand != null)
        {
            actionCommand.End();
            Destroy(actionCommand);
        }

        EndAnimation(caller, level, result);
        if (result)
        {
            yield return StartCoroutine(caller.JumpHeavy(caller.homePos, 1.5f, 0.3f, 0.15f));
            if (level == 1)
            {
                caller.InflictEffect(caller.curTarget, new Effect(Effect.EffectType.Hustle, 1, 2));
            }
            else
            {
                List<BattleEntity> targets = BattleControl.Instance.GetEntitiesSorted(caller, GetTargetArea(caller, level));
                foreach (BattleEntity b in targets)
                {
                    caller.InflictEffect(b, new Effect(Effect.EffectType.Hustle, 1, 2));
                }
            }
        }
        else
        {
            yield return StartCoroutine(caller.JumpHeavy(caller.homePos, 0.5f, 0.15f, 0.15f));
            if (level == 1)
            {
                caller.InflictEffect(caller.curTarget, new Effect(Effect.EffectType.Hustle, 1, 2));
            }
            else
            {
                List<BattleEntity> targets = BattleControl.Instance.GetEntitiesSorted(caller, GetTargetArea(caller, level));
                foreach (BattleEntity b in targets)
                {
                    caller.InflictEffect(b, new Effect(Effect.EffectType.Hustle, 1, 2));
                }
            }
        }
    }

    public void CastAnimation(BattleEntity caller, int level)
    {
        GameObject eo = null;
        eo = Instantiate(Resources.Load<GameObject>("VFX/Battle/Moves/Player/SoulMoves/Effect_HastenSoulCast"), BattleControl.Instance.transform);
        eo.transform.position = caller.ApplyScaledOffset(Vector3.zero);
        eo.transform.localRotation = Quaternion.identity;
    }
    public void EndAnimation(BattleEntity caller, int level, bool result)
    {
        if (level == 1)
        {
            GameObject eo = null;
            eo = Instantiate(Resources.Load<GameObject>("VFX/Battle/Moves/Player/SoulMoves/Effect_HastenShockwave"), BattleControl.Instance.transform);
            eo.transform.position = caller.curTarget.ApplyScaledOffset(Vector3.zero);
            eo.transform.localRotation = Quaternion.identity;
        }
        else
        {
            List<BattleEntity> targets = BattleControl.Instance.GetEntitiesSorted(caller, GetTargetArea(caller, level));
            foreach (BattleEntity b in targets)
            {
                GameObject eo = null;
                eo = Instantiate(Resources.Load<GameObject>("VFX/Battle/Moves/Player/SoulMoves/Effect_HastenShockwave"), BattleControl.Instance.transform);
                eo.transform.position = b.ApplyScaledOffset(Vector3.zero);
                eo.transform.localRotation = Quaternion.identity;
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

public class SM_Revitalize : SoulMove
{
    public SM_Revitalize()
    {
    }

    public override int GetTextIndex()
    {
        return 1;
    }
    public override string GetName() => GetNameWithIndex(GetTextIndex());
    public override string GetDescription(int level = 1) => GetDescriptionWithIndex(GetTextIndex(), level);
    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.Ally, false);
    //public override float GetBasePower() => 0.5f;
    public override int GetBaseCost() => 8;

    public override TargetArea GetTargetArea(BattleEntity caller, int level = 1)
    {
        switch (level)
        {
            case 1:
                return new TargetArea(TargetArea.TargetAreaType.Ally, true);
            default:
                return new TargetArea(TargetArea.TargetAreaType.Ally, true);
        }
    }

    public override int GetCost(BattleEntity caller, int level = 1)
    {
        return StandardCostCalculation(caller, level, 12);
    }

    /*
    public override int GetMaxLevel(BattleEntity caller)
    {
        if (caller is PlayerEntity pcaller)
        {
            return pcaller.GetSoulMoveMaxLevel(GetTextIndex());
        }
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
        CastAnimation(caller, level);
        yield return new WaitUntil(() => actionCommand.IsComplete());

        bool result = actionCommand == null ? true : actionCommand.GetSuccess();
        if (actionCommand != null)
        {
            actionCommand.End();
            Destroy(actionCommand);
        }

        EndAnimation(caller, level, result);
        if (result)
        {
            yield return StartCoroutine(caller.JumpHeavy(caller.homePos, 1.5f, 0.3f, 0.15f));
            if (level > 1)
            {
                List<BattleEntity> targets = BattleControl.Instance.GetEntitiesSorted(caller, GetTargetArea(caller, level));
                foreach (BattleEntity b in targets)
                {
                    b.HealHealth(12);
                }
                yield return new WaitForSeconds(0.5f);
                caller.HealEnergy(12);
                yield return new WaitForSeconds(0.5f);
                foreach (BattleEntity b in targets)
                {
                    b.CureCurableEffects();
                }
            }
            else
            {
                List<BattleEntity> targets = BattleControl.Instance.GetEntitiesSorted(caller, GetTargetArea(caller, level));
                foreach (BattleEntity b in targets)
                {
                    b.HealHealth(6);
                }
                yield return new WaitForSeconds(0.5f);
                caller.HealEnergy(6);
                yield return new WaitForSeconds(0.5f);
                foreach (BattleEntity b in targets)
                {
                    b.CureCurableEffects();
                }
            }
        }
        else
        {
            yield return StartCoroutine(caller.JumpHeavy(caller.homePos, 0.5f, 0.15f, 0.15f));
            if (level > 1)
            {
                List<BattleEntity> targets = BattleControl.Instance.GetEntitiesSorted(caller, GetTargetArea(caller, level));
                foreach (BattleEntity b in targets)
                {
                    b.HealHealth(6);
                }
                yield return new WaitForSeconds(0.5f);
                caller.HealEnergy(6);
                yield return new WaitForSeconds(0.5f);
                foreach (BattleEntity b in targets)
                {
                    b.CureCurableEffects();
                }
            }
            else
            {
                List<BattleEntity> targets = BattleControl.Instance.GetEntitiesSorted(caller, GetTargetArea(caller, level));
                foreach (BattleEntity b in targets)
                {
                    b.HealHealth(3);
                }
                yield return new WaitForSeconds(0.5f);
                caller.HealEnergy(3);
                yield return new WaitForSeconds(0.5f);
                foreach (BattleEntity b in targets)
                {
                    b.CureCurableEffects();
                }
            }
        }
    }

    public void CastAnimation(BattleEntity caller, int level)
    {
        GameObject eo = null;
        eo = Instantiate(Resources.Load<GameObject>("VFX/Battle/Moves/Player/SoulMoves/Effect_RevitalizeSoulCast"), BattleControl.Instance.transform);
        eo.transform.position = caller.ApplyScaledOffset(Vector3.zero);
        eo.transform.localRotation = Quaternion.identity;
    }
    public void EndAnimation(BattleEntity caller, int level, bool result)
    {
        List<BattleEntity> targets = BattleControl.Instance.GetEntitiesSorted(caller, GetTargetArea(caller, level));
        foreach (BattleEntity b in targets)
        {
            GameObject eo = null;
            eo = Instantiate(Resources.Load<GameObject>("VFX/Battle/Moves/Player/SoulMoves/Effect_RevitalizeShockwave"), BattleControl.Instance.transform);
            eo.transform.position = b.ApplyScaledOffset(Vector3.zero);
            eo.transform.localRotation = Quaternion.identity;
        }
    }

    public override void PreMove(BattleEntity caller, int level = 1)
    {
    }
    public override void PostMove(BattleEntity caller, int level = 1)
    {
    }
}

public class SM_LeafStorm : SoulMove
{
    public SM_LeafStorm()
    {
    }

    public override int GetTextIndex()
    {
        return 2;
    }
    public override string GetName() => GetNameWithIndex(GetTextIndex());
    public override string GetDescription(int level = 1) => GetDescriptionWithIndex(GetTextIndex(), level);
    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemy, true);
    //public override float GetBasePower() => 0.5f;
    public override int GetBaseCost() => 15;

    public override int GetCost(BattleEntity caller, int level = 1)
    {
        return StandardCostCalculation(caller, level, 15);
    }

    /*
    public override int GetMaxLevel(BattleEntity caller)
    {
        if (caller is PlayerEntity pcaller)
        {
            return pcaller.GetSoulMoveMaxLevel(GetTextIndex());
        }
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
        CastAnimation(caller, level);
        yield return new WaitUntil(() => actionCommand.IsComplete());

        bool result = actionCommand == null ? true : actionCommand.GetSuccess();
        if (actionCommand != null)
        {
            actionCommand.End();
            Destroy(actionCommand);
        }

        List<BattleEntity> targets = BattleControl.Instance.GetEntitiesSorted(caller, GetTargetArea(caller, level));

        EndAnimation(caller, level, result);
        ulong propertyBlock = (ulong)(BattleHelper.DamageProperties.Static);
        if (result)
        {
            yield return StartCoroutine(caller.JumpHeavy(caller.homePos, 1.5f, 0.3f, 0.15f));
            foreach (BattleEntity b in targets)
            {
                if (caller.GetAttackHit(b, BattleHelper.DamageType.Earth))
                {
                    if (level > 1)
                    {
                        caller.DealDamage(b, 8, BattleHelper.DamageType.Earth, propertyBlock, BattleHelper.ContactLevel.Infinite);
                    }
                    else
                    {
                        caller.DealDamage(b, 4, BattleHelper.DamageType.Earth, propertyBlock, BattleHelper.ContactLevel.Infinite);
                    }
                }
                else
                {
                    caller.InvokeMissEvents(b);
                }
            }
        }
        else
        {
            yield return StartCoroutine(caller.JumpHeavy(caller.homePos, 0.5f, 0.15f, 0.15f));
            foreach (BattleEntity b in targets)
            {
                if (caller.GetAttackHit(b, BattleHelper.DamageType.Earth))
                {
                    if (level > 1)
                    {
                        caller.DealDamage(b, 4, BattleHelper.DamageType.Earth, propertyBlock, BattleHelper.ContactLevel.Infinite);
                    }
                    else
                    {
                        caller.DealDamage(b, 2, BattleHelper.DamageType.Earth, propertyBlock, BattleHelper.ContactLevel.Infinite);
                    }
                }
                else
                {
                    caller.InvokeMissEvents(b);
                }
            }
        }
    }

    public void CastAnimation(BattleEntity caller, int level)
    {
        GameObject eo = null;
        eo = Instantiate(Resources.Load<GameObject>("VFX/Battle/Moves/Player/SoulMoves/Effect_EarthFlowIn"), BattleControl.Instance.transform);
        //position is the same as the one used by items
        Vector3 position = caller.transform.position + caller.height * Vector3.up + Vector3.up * 1.5f;
        eo.transform.position = position;
        eo.transform.localRotation = Quaternion.identity;
    }
    public void EndAnimation(BattleEntity caller, int level, bool result)
    {
        GameObject eo = null;
        eo = Instantiate(Resources.Load<GameObject>("VFX/Battle/Moves/Player/SoulMoves/Effect_EarthFlowOut"), BattleControl.Instance.transform);
        //position is the same as the one used by items
        Vector3 position = caller.transform.position + caller.height * Vector3.up + Vector3.up * 1.5f;
        eo.transform.position = position;
        eo.transform.localRotation = Quaternion.identity;
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

        ulong propertyBlock = (ulong)BattleHelper.DamageProperties.Static;

        int val;

        if (level > 1)
        {
            val = caller.DealDamageCalculation(target, 8, BattleHelper.DamageType.Earth, propertyBlock);
        }
        else
        {
            val = caller.DealDamageCalculation(target, 4, BattleHelper.DamageType.Earth, propertyBlock);
        }

        return val + "";
    }
}

public class SM_ElectroDischarge : SoulMove
{
    public SM_ElectroDischarge()
    {
    }

    public override int GetTextIndex()
    {
        return 3;
    }
    public override string GetName() => GetNameWithIndex(GetTextIndex());
    public override string GetDescription(int level = 1) => GetDescriptionWithIndex(GetTextIndex(), level);
    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemy, true);
    //public override float GetBasePower() => 0.5f;
    public override int GetBaseCost() => 15;

    public override int GetCost(BattleEntity caller, int level = 1)
    {
        return StandardCostCalculation(caller, level, 15);
    }

    /*
    public override int GetMaxLevel(BattleEntity caller)
    {
        if (caller is PlayerEntity pcaller)
        {
            return pcaller.GetSoulMoveMaxLevel(GetTextIndex());
        }
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
        CastAnimation(caller, level);
        yield return new WaitUntil(() => actionCommand.IsComplete());

        bool result = actionCommand == null ? true : actionCommand.GetSuccess();
        if (actionCommand != null)
        {
            actionCommand.End();
            Destroy(actionCommand);
        }

        List<BattleEntity> targets = BattleControl.Instance.GetEntitiesSorted(caller, GetTargetArea(caller, level));

        EndAnimation(caller, level, result);
        if (result)
        {
            yield return StartCoroutine(caller.JumpHeavy(caller.homePos, 1.5f, 0.3f, 0.15f));
            foreach (BattleEntity b in targets)
            {
                if (caller.GetAttackHit(b, BattleHelper.DamageType.Air))
                {
                    if (level > 1)
                    {
                        BattleEntity.SpecialInvokeHurtEvents(caller, b, BattleHelper.DamageType.Air, 0);
                        caller.InflictEffect(b, new Effect(Effect.EffectType.AttackDown, 2, 3));
                        caller.InflictEffect(b, new Effect(Effect.EffectType.DefenseDown, 2, 3));
                    }
                    else
                    {
                        BattleEntity.SpecialInvokeHurtEvents(caller, b, BattleHelper.DamageType.Air, 0);
                        caller.InflictEffect(b, new Effect(Effect.EffectType.AttackDown, 2, 3));
                    }
                }
                else
                {
                    caller.InvokeMissEvents(b);
                }
            }
        }
        else
        {
            yield return StartCoroutine(caller.JumpHeavy(caller.homePos, 0.5f, 0.15f, 0.15f));
            foreach (BattleEntity b in targets)
            {
                if (caller.GetAttackHit(b, BattleHelper.DamageType.Air))
                {
                    if (level > 1)
                    {
                        BattleEntity.SpecialInvokeHurtEvents(caller, b, BattleHelper.DamageType.Air, 0);
                        caller.InflictEffect(b, new Effect(Effect.EffectType.AttackDown, 1, 3));
                        caller.InflictEffect(b, new Effect(Effect.EffectType.DefenseDown, 1, 3));
                    }
                    else
                    {
                        BattleEntity.SpecialInvokeHurtEvents(caller, b, BattleHelper.DamageType.Air, 0);
                        caller.InflictEffect(b, new Effect(Effect.EffectType.AttackDown, 1, 3));
                    }
                }
                else
                {
                    caller.InvokeMissEvents(b);
                }
            }
        }
    }

    public void CastAnimation(BattleEntity caller, int level)
    {
        GameObject eo = null;
        eo = Instantiate(Resources.Load<GameObject>("VFX/Battle/Moves/Player/SoulMoves/Effect_SparkFlowIn"), BattleControl.Instance.transform);
        //position is the same as the one used by items
        Vector3 position = caller.transform.position + caller.height * Vector3.up + Vector3.up * 1.5f;
        eo.transform.position = position;
        eo.transform.localRotation = Quaternion.identity;
    }
    public void EndAnimation(BattleEntity caller, int level, bool result)
    {
        GameObject eo = null;
        eo = Instantiate(Resources.Load<GameObject>("VFX/Battle/Moves/Player/SoulMoves/Effect_SparkFlowOut"), BattleControl.Instance.transform);
        //position is the same as the one used by items
        Vector3 position = caller.transform.position + caller.height * Vector3.up + Vector3.up * 1.5f;
        eo.transform.position = position;
        eo.transform.localRotation = Quaternion.identity;
    }

    public override void PreMove(BattleEntity caller, int level = 1)
    {
    }
    public override void PostMove(BattleEntity caller, int level = 1)
    {
    }
}

public class SM_MistWave : SoulMove
{
    public SM_MistWave()
    {
    }

    public override int GetTextIndex()
    {
        return 4;
    }
    public override string GetName() => GetNameWithIndex(GetTextIndex());
    public override string GetDescription(int level = 1) => GetDescriptionWithIndex(GetTextIndex(), level);
    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemy, true);
    //public override float GetBasePower() => 0.5f;
    public override int GetBaseCost() => 20;

    public override int GetCost(BattleEntity caller, int level = 1)
    {
        return StandardCostCalculation(caller, level, 20);
    }

    /*
    public override int GetMaxLevel(BattleEntity caller)
    {
        if (caller is PlayerEntity pcaller)
        {
            return pcaller.GetSoulMoveMaxLevel(GetTextIndex());
        }
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
        CastAnimation(caller, level);
        yield return new WaitUntil(() => actionCommand.IsComplete());

        bool result = actionCommand == null ? true : actionCommand.GetSuccess();
        if (actionCommand != null)
        {
            actionCommand.End();
            Destroy(actionCommand);
        }

        List<BattleEntity> targets = BattleControl.Instance.GetEntitiesSorted(caller, GetTargetArea(caller, level));

        EndAnimation(caller, level, result);
        ulong propertyBlock = (ulong)(BattleHelper.DamageProperties.Static);
        if (result)
        {
            yield return StartCoroutine(caller.JumpHeavy(caller.homePos, 1.5f, 0.3f, 0.15f));
            foreach (BattleEntity b in targets)
            {
                if (caller.GetAttackHit(b, BattleHelper.DamageType.Water))
                {
                    if (level > 1)
                    {
                        caller.DealDamage(b, 12, BattleHelper.DamageType.Water, propertyBlock, BattleHelper.ContactLevel.Infinite);
                    }
                    else
                    {
                        caller.DealDamage(b, 6, BattleHelper.DamageType.Water, propertyBlock, BattleHelper.ContactLevel.Infinite);
                    }
                }
                else
                {
                    caller.InvokeMissEvents(b);
                }
            }
        }
        else
        {
            yield return StartCoroutine(caller.JumpHeavy(caller.homePos, 0.5f, 0.15f, 0.15f));
            foreach (BattleEntity b in targets)
            {
                if (caller.GetAttackHit(b, BattleHelper.DamageType.Water))
                {
                    if (level > 1)
                    {
                        caller.DealDamage(b, 6, BattleHelper.DamageType.Water, propertyBlock, BattleHelper.ContactLevel.Infinite);
                    }
                    else
                    {
                        caller.DealDamage(b, 3, BattleHelper.DamageType.Water, propertyBlock, BattleHelper.ContactLevel.Infinite);
                    }
                }
                else
                {
                    caller.InvokeMissEvents(b);
                }
            }
        }
    }

    public void CastAnimation(BattleEntity caller, int level)
    {
        GameObject eo = null;
        eo = Instantiate(Resources.Load<GameObject>("VFX/Battle/Moves/Player/SoulMoves/Effect_WaterFlowIn"), BattleControl.Instance.transform);
        //position is the same as the one used by items
        Vector3 position = caller.transform.position + caller.height * Vector3.up + Vector3.up * 1.5f;
        eo.transform.position = position;
        eo.transform.localRotation = Quaternion.identity;
    }
    public void EndAnimation(BattleEntity caller, int level, bool result)
    {
        GameObject eo = null;
        eo = Instantiate(Resources.Load<GameObject>("VFX/Battle/Moves/Player/SoulMoves/Effect_WaterFlowOut"), BattleControl.Instance.transform);
        //position is the same as the one used by items
        Vector3 position = caller.transform.position + caller.height * Vector3.up + Vector3.up * 1.5f;
        eo.transform.position = position;
        eo.transform.localRotation = Quaternion.identity;
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

        ulong propertyBlock = (ulong)BattleHelper.DamageProperties.Static;

        int val;
        
        if (level > 1)
        {
            val = caller.DealDamageCalculation(target, 12, BattleHelper.DamageType.Water, propertyBlock);
        } else
        {
            val = caller.DealDamageCalculation(target, 6, BattleHelper.DamageType.Water, propertyBlock);
        }

        return val + "";
    }
}

public class SM_Overheat : SoulMove
{
    public SM_Overheat()
    {
    }

    public override int GetTextIndex()
    {
        return 5;
    }
    public override string GetName() => GetNameWithIndex(GetTextIndex());
    public override string GetDescription(int level = 1) => GetDescriptionWithIndex(GetTextIndex(), level);
    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemy, true);
    //public override float GetBasePower() => 0.5f;
    public override int GetBaseCost() => 12;

    public override int GetCost(BattleEntity caller, int level = 1)
    {
        return StandardCostCalculation(caller, level, 12);
    }

    /*
    public override int GetMaxLevel(BattleEntity caller)
    {
        if (caller is PlayerEntity pcaller)
        {
            return pcaller.GetSoulMoveMaxLevel(GetTextIndex());
        }
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
        CastAnimation(caller, level);
        yield return new WaitUntil(() => actionCommand.IsComplete());

        bool result = actionCommand == null ? true : actionCommand.GetSuccess();
        if (actionCommand != null)
        {
            actionCommand.End();
            Destroy(actionCommand);
        }

        List<BattleEntity> targets = BattleControl.Instance.GetEntitiesSorted(caller, GetTargetArea(caller, level));

        EndAnimation(caller, level, result);
        if (result)
        {
            yield return StartCoroutine(caller.JumpHeavy(caller.homePos, 1.5f, 0.3f, 0.15f));
            foreach (BattleEntity b in targets)
            {
                if (caller.GetAttackHit(b, BattleHelper.DamageType.Fire))
                {
                    if (level > 1)
                    {
                        BattleEntity.SpecialInvokeHurtEvents(caller, b, BattleHelper.DamageType.Fire, 0);
                        caller.InflictEffect(b, new Effect(Effect.EffectType.DefenseDown, 2, 3));
                        caller.InflictEffect(b, new Effect(Effect.EffectType.Sunder, 4, 255));
                    }
                    else
                    {
                        BattleEntity.SpecialInvokeHurtEvents(caller, b, BattleHelper.DamageType.Fire, 0);
                        caller.InflictEffect(b, new Effect(Effect.EffectType.DefenseDown, 2, 3));
                    }
                }
                else
                {
                    caller.InvokeMissEvents(b);
                }
            }
        }
        else
        {
            yield return StartCoroutine(caller.JumpHeavy(caller.homePos, 0.5f, 0.15f, 0.15f));
            foreach (BattleEntity b in targets)
            {
                if (caller.GetAttackHit(b, BattleHelper.DamageType.Fire))
                {
                    if (level > 1)
                    {
                        BattleEntity.SpecialInvokeHurtEvents(caller, b, BattleHelper.DamageType.Fire, 0);
                        caller.InflictEffect(b, new Effect(Effect.EffectType.DefenseDown, 1, 3));
                        caller.InflictEffect(b, new Effect(Effect.EffectType.Sunder, 2, 255));
                    }
                    else
                    {
                        BattleEntity.SpecialInvokeHurtEvents(caller, b, BattleHelper.DamageType.Fire, 0);
                        caller.InflictEffect(b, new Effect(Effect.EffectType.DefenseDown, 1, 3));
                    }
                }
                else
                {
                    caller.InvokeMissEvents(b);
                }
            }
        }
    }

    public void CastAnimation(BattleEntity caller, int level)
    {
        GameObject eo = null;
        eo = Instantiate(Resources.Load<GameObject>("VFX/Battle/Moves/Player/SoulMoves/Effect_FireFlowIn"), BattleControl.Instance.transform);
        //position is the same as the one used by items
        Vector3 position = caller.transform.position + caller.height * Vector3.up + Vector3.up * 1.5f;
        eo.transform.position = position;
        eo.transform.localRotation = Quaternion.identity;
    }
    public void EndAnimation(BattleEntity caller, int level, bool result)
    {
        GameObject eo = null;
        eo = Instantiate(Resources.Load<GameObject>("VFX/Battle/Moves/Player/SoulMoves/Effect_FireFlowOut"), BattleControl.Instance.transform);
        //position is the same as the one used by items
        Vector3 position = caller.transform.position + caller.height * Vector3.up + Vector3.up * 1.5f;
        eo.transform.position = position;
        eo.transform.localRotation = Quaternion.identity;
    }

    public override void PreMove(BattleEntity caller, int level = 1)
    {
    }
    public override void PostMove(BattleEntity caller, int level = 1)
    {
    }
}

public class SM_VoidCrush : SoulMove
{
    public SM_VoidCrush()
    {
    }

    public override int GetTextIndex()
    {
        return 6;
    }
    public override string GetName() => GetNameWithIndex(GetTextIndex());
    public override string GetDescription(int level = 1) => GetDescriptionWithIndex(GetTextIndex(), level);
    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemy, true);
    //public override float GetBasePower() => 0.5f;
    public override int GetBaseCost() => 20;

    public override int GetCost(BattleEntity caller, int level = 1)
    {
        return StandardCostCalculation(caller, level, 10);
    }

    /*
    public override int GetMaxLevel(BattleEntity caller)
    {
        if (caller is PlayerEntity pcaller)
        {
            return pcaller.GetSoulMoveMaxLevel(GetTextIndex());
        }
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
        CastAnimation(caller, level);
        yield return new WaitUntil(() => actionCommand.IsComplete());

        bool result = actionCommand == null ? true : actionCommand.GetSuccess();
        if (actionCommand != null)
        {
            actionCommand.End();
            Destroy(actionCommand);
        }

        List<BattleEntity> targets = BattleControl.Instance.GetEntitiesSorted(caller, GetTargetArea(caller, level));

        EndAnimation(caller, level, result);
        if (result)
        {
            yield return StartCoroutine(caller.JumpHeavy(caller.homePos, 1.5f, 0.3f, 0.15f));
            foreach (BattleEntity b in targets)
            {
                if (caller.GetAttackHit(b, BattleHelper.DamageType.Dark))
                {
                    if (level > 1)
                    {
                        caller.TryVoidCrush(b, 10);
                        caller.InflictEffect(b, new Effect(Effect.EffectType.Defocus, 4, 255));
                    }
                    else
                    {
                        caller.TryVoidCrush(b, 1);
                    }
                }
                else
                {
                    caller.InvokeMissEvents(b);
                }
            }
        }
        else
        {
            yield return StartCoroutine(caller.JumpHeavy(caller.homePos, 0.5f, 0.15f, 0.15f));
            foreach (BattleEntity b in targets)
            {
                if (caller.GetAttackHit(b, BattleHelper.DamageType.Dark))
                {
                    if (level > 1)
                    {
                        caller.TryVoidCrush(b, 1);
                        caller.InflictEffect(b, new Effect(Effect.EffectType.Defocus, 2, 255));
                    }
                    else
                    {
                        caller.TryVoidCrush(b, 0.5f);
                    }
                }
                else
                {
                    caller.InvokeMissEvents(b);
                }
            }
        }
    }

    public void CastAnimation(BattleEntity caller, int level)
    {
        GameObject eo = null;
        eo = Instantiate(Resources.Load<GameObject>("VFX/Battle/Moves/Player/SoulMoves/Effect_DarkFlowIn"), BattleControl.Instance.transform);
        //position is the same as the one used by items
        Vector3 position = caller.transform.position + caller.height * Vector3.up + Vector3.up * 1.5f;
        eo.transform.position = position;
        eo.transform.localRotation = Quaternion.identity;
    }
    public void EndAnimation(BattleEntity caller, int level, bool result)
    {
        GameObject eo = null;
        eo = Instantiate(Resources.Load<GameObject>("VFX/Battle/Moves/Player/SoulMoves/Effect_DarkFlowOut"), BattleControl.Instance.transform);
        //position is the same as the one used by items
        Vector3 position = caller.transform.position + caller.height * Vector3.up + Vector3.up * 1.5f;
        eo.transform.position = position;
        eo.transform.localRotation = Quaternion.identity;
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

        int hp = 0;

        if (level == 2)
        {
            hp = caller.VoidCrushHP(target, 10);
        } else
        {
            hp = caller.VoidCrushHP(target, 1);
        }

        bool doesWork = (target.hp > 0) && (target.hp <= hp);

        if (doesWork)
        {
            return "<highlightyescolor>(" + hp + ")</highlightyescolor>";
        } else
        {
            return "<highlightnocolor>(" + hp + ")</highlightnocolor>";
        }
    }
}

public class SM_FlashFreeze : SoulMove
{
    public SM_FlashFreeze()
    {
    }

    public override int GetTextIndex()
    {
        return 7;
    }
    public override string GetName() => GetNameWithIndex(GetTextIndex());
    public override string GetDescription(int level = 1) => GetDescriptionWithIndex(GetTextIndex(), level);
    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemy, true);
    //public override float GetBasePower() => 0.5f;
    public override int GetBaseCost() => 15;

    public override int GetCost(BattleEntity caller, int level = 1)
    {
        return StandardCostCalculation(caller, level, 15);
    }

    /*
    public override int GetMaxLevel(BattleEntity caller)
    {
        if (caller is PlayerEntity pcaller)
        {
            return pcaller.GetSoulMoveMaxLevel(GetTextIndex());
        }
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
        CastAnimation(caller, level);
        yield return new WaitUntil(() => actionCommand.IsComplete());

        bool result = actionCommand == null ? true : actionCommand.GetSuccess();
        if (actionCommand != null)
        {
            actionCommand.End();
            Destroy(actionCommand);
        }

        List<BattleEntity> targets = BattleControl.Instance.GetEntitiesSorted(caller, GetTargetArea(caller, level));

        EndAnimation(caller, level, result);
        if (result)
        {
            yield return StartCoroutine(caller.JumpHeavy(caller.homePos, 1.5f, 0.3f, 0.15f));
            foreach (BattleEntity b in targets)
            {
                if (caller.GetAttackHit(b, BattleHelper.DamageType.Light))
                {
                    if (level > 1)
                    {
                        BattleEntity.SpecialInvokeHurtEvents(caller, b, BattleHelper.DamageType.Light, 0);
                        caller.InflictEffect(b, new Effect(Effect.EffectType.Freeze, 1, 3));
                        caller.InflictEffect(b, new Effect(Effect.EffectType.Defocus, 2, 255));
                        caller.InflictEffect(b, new Effect(Effect.EffectType.Sunder, 2, 255));
                    }
                    else
                    {
                        BattleEntity.SpecialInvokeHurtEvents(caller, b, BattleHelper.DamageType.Light, 0);
                        caller.InflictEffect(b, new Effect(Effect.EffectType.Freeze, 1, 3));
                    }
                }
                else
                {
                    caller.InvokeMissEvents(b);
                }
            }
        }
        else
        {
            yield return StartCoroutine(caller.JumpHeavy(caller.homePos, 0.5f, 0.15f, 0.15f));
            foreach (BattleEntity b in targets)
            {
                if (caller.GetAttackHit(b, BattleHelper.DamageType.Light))
                {
                    if (level > 1)
                    {
                        BattleEntity.SpecialInvokeHurtEvents(caller, b, BattleHelper.DamageType.Light, 0);
                        caller.InflictEffect(b, new Effect(Effect.EffectType.Freeze, 1, 2));
                        caller.InflictEffect(b, new Effect(Effect.EffectType.Defocus, 1, 255));
                        caller.InflictEffect(b, new Effect(Effect.EffectType.Sunder, 1, 255));
                    }
                    else
                    {
                        BattleEntity.SpecialInvokeHurtEvents(caller, b, BattleHelper.DamageType.Light, 0);
                        caller.InflictEffect(b, new Effect(Effect.EffectType.Freeze, 1, 2));
                    }
                }
                else
                {
                    caller.InvokeMissEvents(b);
                }
            }
        }
    }

    public void CastAnimation(BattleEntity caller, int level)
    {
        GameObject eo = null;
        eo = Instantiate(Resources.Load<GameObject>("VFX/Battle/Moves/Player/SoulMoves/Effect_LightFlowIn"), BattleControl.Instance.transform);
        //position is the same as the one used by items
        Vector3 position = caller.transform.position + caller.height * Vector3.up + Vector3.up * 1.5f;
        eo.transform.position = position;
        eo.transform.localRotation = Quaternion.identity;
    }
    public void EndAnimation(BattleEntity caller, int level, bool result)
    {
        GameObject eo = null;
        eo = Instantiate(Resources.Load<GameObject>("VFX/Battle/Moves/Player/SoulMoves/Effect_LightFlowOut"), BattleControl.Instance.transform);
        //position is the same as the one used by items
        Vector3 position = caller.transform.position + caller.height * Vector3.up + Vector3.up * 1.5f;
        eo.transform.position = position;
        eo.transform.localRotation = Quaternion.identity;
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
            if (!pcallerA.BadgeEquipped(Badge.BadgeType.StatusSight))
            {
                return "";
            }
        }

        float statusBoost = 1;
        if (caller is PlayerEntity pcaller)
        {
            statusBoost = pcaller.CalculateStatusBoost(target);
        }

        int hp = (int)(target.StatusWorkingHP(Effect.EffectType.Freeze) * statusBoost);

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
}

public class SM_Cleanse : SoulMove
{
    public SM_Cleanse()
    {
    }

    public override int GetTextIndex()
    {
        return 8;
    }
    public override string GetName() => GetNameWithIndex(GetTextIndex());
    public override string GetDescription(int level = 1) => GetDescriptionWithIndex(GetTextIndex(), level);
    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemy, true);
    //public override float GetBasePower() => 0.5f;
    public override int GetBaseCost() => 6;

    public override TargetArea GetTargetArea(BattleEntity caller, int level = 1)
    {
        switch (level)
        {
            default:
                return new TargetArea(TargetArea.TargetAreaType.LiveEnemy, true);
        }
    }


    public override int GetCost(BattleEntity caller, int level = 1)
    {
        return StandardCostCalculation(caller, level, 6);
    }

    /*
    public override int GetMaxLevel(BattleEntity caller)
    {
        if (caller is PlayerEntity pcaller)
        {
            return pcaller.GetSoulMoveMaxLevel(GetTextIndex());
        }
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
        CastAnimation(caller, level);
        yield return new WaitUntil(() => actionCommand.IsComplete());

        bool result = actionCommand == null ? true : actionCommand.GetSuccess();
        if (actionCommand != null)
        {
            actionCommand.End();
            Destroy(actionCommand);
        }

        List<BattleEntity> targets = BattleControl.Instance.GetEntitiesSorted(caller, GetTargetArea(caller, level));

        EndAnimation(caller, level, result);
        if (result)
        {
            yield return StartCoroutine(caller.JumpHeavy(caller.homePos, 1.5f, 0.3f, 0.15f));
            foreach (BattleEntity b in targets)
            {
                if (caller.GetAttackHit(b, BattleHelper.DamageType.Spectral))
                {
                    BattleEntity.SpecialInvokeHurtEvents(caller, b, BattleHelper.DamageType.Spectral, 0);
                    b.CureCleanseableEffects(false);
                    if (level > 1)
                    {
                        caller.InflictEffect(b, new Effect(Effect.EffectType.Seal, 1, 2));
                    }
                }
                else
                {
                    caller.InvokeMissEvents(b);
                }
            }
        }
        else
        {
            yield return StartCoroutine(caller.JumpHeavy(caller.homePos, 0.5f, 0.15f, 0.15f));
            foreach (BattleEntity b in targets)
            {
                if (caller.GetAttackHit(b, BattleHelper.DamageType.Spectral))
                {
                    BattleEntity.SpecialInvokeHurtEvents(caller, b, BattleHelper.DamageType.Spectral, 0);
                    b.CureCleanseableEffects(false);
                    if (level > 1)
                    {
                        caller.InflictEffect(b, new Effect(Effect.EffectType.Seal, 1, 1));
                    }
                }
                else
                {
                    caller.InvokeMissEvents(b);
                }
            }
        }
    }

    public void CastAnimation(BattleEntity caller, int level)
    {
        GameObject eo = null;
        eo = Instantiate(Resources.Load<GameObject>("VFX/Battle/Moves/Player/SoulMoves/Effect_CleanseSoulCast"), BattleControl.Instance.transform);
        eo.transform.position = caller.ApplyScaledOffset(Vector3.zero);
        eo.transform.localRotation = Quaternion.identity;
    }
    public void EndAnimation(BattleEntity caller, int level, bool result)
    {
        GameObject eo = null;
        eo = Instantiate(Resources.Load<GameObject>("VFX/Battle/Moves/Player/SoulMoves/Effect_CleanseFlowOut"), BattleControl.Instance.transform);
        eo.transform.position = caller.ApplyScaledOffset(Vector3.up * 0.5f);
        eo.transform.localRotation = Quaternion.identity;
    }

    public override void PreMove(BattleEntity caller, int level = 1)
    {
    }
    public override void PostMove(BattleEntity caller, int level = 1)
    {
    }
}

public class SM_Blight : SoulMove
{
    public SM_Blight()
    {
    }

    public override int GetTextIndex()
    {
        return 9;
    }
    public override string GetName() => GetNameWithIndex(GetTextIndex());
    public override string GetDescription(int level = 1) => GetDescriptionWithIndex(GetTextIndex(), level);
    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemy, false);
    //public override float GetBasePower() => 0.5f;
    public override int GetBaseCost() => 12;

    public override TargetArea GetTargetArea(BattleEntity caller, int level = 1)
    {
        switch (level)
        {
            case 1:
                return new TargetArea(TargetArea.TargetAreaType.LiveEnemy, false);
            default:
                return new TargetArea(TargetArea.TargetAreaType.LiveEnemy, true);
        }
    }


    public override int GetCost(BattleEntity caller, int level = 1)
    {
        return StandardCostCalculation(caller, level, 12);
    }

    /*
    public override int GetMaxLevel(BattleEntity caller)
    {
        if (caller is PlayerEntity pcaller)
        {
            return pcaller.GetSoulMoveMaxLevel(GetTextIndex());
        }
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
        CastAnimation(caller, level);
        yield return new WaitUntil(() => actionCommand.IsComplete());

        bool result = actionCommand == null ? true : actionCommand.GetSuccess();
        if (actionCommand != null)
        {
            actionCommand.End();
            Destroy(actionCommand);
        }

        List<BattleEntity> targets = BattleControl.Instance.GetEntitiesSorted(caller, GetTargetArea(caller, level));

        ulong propertyBlock = (ulong)BattleHelper.DamageProperties.Static;
        propertyBlock += (ulong)BattleHelper.DamageProperties.CountDebuffs2;

        EndAnimation(caller, level, result);
        if (result)
        {
            yield return StartCoroutine(caller.JumpHeavy(caller.homePos, 1.5f, 0.3f, 0.15f));

            if (level > 1)
            {
                foreach (BattleEntity b in targets)
                {
                    if (caller.GetAttackHit(b, BattleHelper.DamageType.Astral))
                    {
                        BattleEntity.SpecialInvokeHurtEvents(caller, b, BattleHelper.DamageType.Astral, 0);
                        caller.InflictEffect(b, new Effect(Effect.EffectType.DrainSprout, 1, 4));
                        caller.InflictEffect(b, new Effect(Effect.EffectType.BoltSprout, 1, 4));
                    }
                    else
                    {
                        caller.InvokeMissEvents(b);
                    }
                }
            }
            else
            {
                BattleEntity b = caller.curTarget;
                if (caller.GetAttackHit(b, BattleHelper.DamageType.Astral))
                {
                    BattleEntity.SpecialInvokeHurtEvents(caller, b, BattleHelper.DamageType.Astral, 0);
                    caller.InflictEffect(b, new Effect(Effect.EffectType.DrainSprout, 1, 3));
                    caller.InflictEffect(b, new Effect(Effect.EffectType.BoltSprout, 1, 3));
                }
                else
                {
                    caller.InvokeMissEvents(b);
                }
            }
        }
        else
        {
            yield return StartCoroutine(caller.JumpHeavy(caller.homePos, 0.5f, 0.15f, 0.15f));

            if (level > 1)
            {
                foreach (BattleEntity b in targets)
                {
                    if (caller.GetAttackHit(b, BattleHelper.DamageType.Astral))
                    {
                        BattleEntity.SpecialInvokeHurtEvents(caller, b, BattleHelper.DamageType.Astral, 0);
                        caller.InflictEffect(b, new Effect(Effect.EffectType.DrainSprout, 1, 3));
                        caller.InflictEffect(b, new Effect(Effect.EffectType.BoltSprout, 1, 3));
                    }
                    else
                    {
                        caller.InvokeMissEvents(b);
                    }
                }
            }
            else
            {
                BattleEntity b = caller.curTarget;
                if (caller.GetAttackHit(b, BattleHelper.DamageType.Astral))
                {
                    BattleEntity.SpecialInvokeHurtEvents(caller, b, BattleHelper.DamageType.Astral, 0);
                    caller.InflictEffect(b, new Effect(Effect.EffectType.DrainSprout, 1, 2));
                    caller.InflictEffect(b, new Effect(Effect.EffectType.BoltSprout, 1, 2));
                }
                else
                {
                    caller.InvokeMissEvents(b);
                }
            }
        }
    }

    public void CastAnimation(BattleEntity caller, int level)
    {
        GameObject eo = null;
        eo = Instantiate(Resources.Load<GameObject>("VFX/Battle/Moves/Player/SoulMoves/Effect_BlightSoulCast"), BattleControl.Instance.transform);
        eo.transform.position = caller.ApplyScaledOffset(Vector3.zero);
        eo.transform.localRotation = Quaternion.identity;
    }
    public void EndAnimation(BattleEntity caller, int level, bool result)
    {
        GameObject eo = null;
        eo = Instantiate(Resources.Load<GameObject>("VFX/Battle/Moves/Player/SoulMoves/Effect_BlightFlowOut"), BattleControl.Instance.transform);
        eo.transform.position = caller.ApplyScaledOffset(Vector3.up * 0.5f);
        eo.transform.localRotation = Quaternion.identity;
    }

    public override void PreMove(BattleEntity caller, int level = 1)
    {
    }
    public override void PostMove(BattleEntity caller, int level = 1)
    {
    }

    /*
    public override string GetHighlightText(BattleEntity caller, BattleEntity target, int level = 1)
    {
        if (caller is PlayerEntity pcallerA)
        {
            if (!pcallerA.BadgeEquipped(Badge.BadgeType.PowerSight))
            {
                return "";
            }
        }

        ulong propertyBlock = (ulong)BattleHelper.DamageProperties.Static;
        propertyBlock += (ulong)BattleHelper.DamageProperties.CountDebuffs2;

        int val = caller.DealDamageCalculation(target, 6, 0, propertyBlock);

        return val + "";
    }
    */
}

public class SM_ChromaBlast : SoulMove
{
    public SM_ChromaBlast()
    {
    }

    public override int GetTextIndex()
    {
        return 10;
    }
    public override string GetName() => GetNameWithIndex(GetTextIndex());
    public override string GetDescription(int level = 1) => GetDescriptionWithIndex(GetTextIndex(), level);
    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemy, true);
    //public override float GetBasePower() => 0.5f;
    public override int GetBaseCost() => 30;

    public override int GetCost(BattleEntity caller, int level = 1)
    {
        return StandardCostCalculation(caller, level, 30);
    }

    /*
    public override int GetMaxLevel(BattleEntity caller)
    {
        if (caller is PlayerEntity pcaller)
        {
            return pcaller.GetSoulMoveMaxLevel(GetTextIndex());
        }
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

        List<BattleEntity> targets = BattleControl.Instance.GetEntitiesSorted(caller, GetTargetArea(caller, level));

        ulong propertyBlock = (ulong)(BattleHelper.DamageProperties.Static);
        ulong propertyBlockCombo = propertyBlock | (ulong)(BattleHelper.DamageProperties.Combo);
        if (result)
        {
            yield return StartCoroutine(caller.JumpHeavy(caller.homePos, 1.5f, 0.3f, 0.15f));
            if (level > 1)
            {
                foreach (BattleEntity b in targets)
                {
                    if (caller.GetAttackHit(b, BattleHelper.DamageType.Everything))
                    {
                        caller.DealDamage(b, 15, BattleHelper.DamageType.Everything, propertyBlock, BattleHelper.ContactLevel.Infinite);
                    }
                    else
                    {
                        caller.InvokeMissEvents(b);
                    }
                }
            }
            else
            {
                BattleHelper.DamageType[] damageTypes = { BattleHelper.DamageType.Fire, BattleHelper.DamageType.Air, BattleHelper.DamageType.Earth, BattleHelper.DamageType.Light, BattleHelper.DamageType.Water, BattleHelper.DamageType.Dark };
                
                foreach (BattleHelper.DamageType t in damageTypes)
                {
                    foreach (BattleEntity b in targets)
                    {
                        if (caller.GetAttackHit(b, t))
                        {
                            if (t != damageTypes[damageTypes.Length - 1])
                            {
                                caller.DealDamage(b, 3, t, propertyBlockCombo, BattleHelper.ContactLevel.Infinite);
                            }
                            else
                            {
                                caller.DealDamage(b, 3, t, propertyBlock, BattleHelper.ContactLevel.Infinite);
                            }
                        }
                        else
                        {
                            caller.InvokeMissEvents(b);
                        }
                    }
                    if (t != damageTypes[damageTypes.Length - 1])
                    {
                        yield return new WaitForSeconds(0.25f);
                    }
                }
            }
        }
        else
        {
            yield return StartCoroutine(caller.JumpHeavy(caller.homePos, 0.5f, 0.15f, 0.15f));
            if (level > 1)
            {
                foreach (BattleEntity b in targets)
                {
                    if (caller.GetAttackHit(b, BattleHelper.DamageType.Everything))
                    {
                        caller.DealDamage(b, 15, BattleHelper.DamageType.Everything, propertyBlock, BattleHelper.ContactLevel.Infinite);
                    }
                    else
                    {
                        caller.InvokeMissEvents(b);
                    }
                }
            }
            else
            {
                BattleHelper.DamageType[] damageTypes = { BattleHelper.DamageType.Fire, BattleHelper.DamageType.Air, BattleHelper.DamageType.Earth, BattleHelper.DamageType.Light, BattleHelper.DamageType.Water, BattleHelper.DamageType.Dark };

                foreach (BattleHelper.DamageType t in damageTypes)
                {
                    foreach (BattleEntity b in targets)
                    {
                        if (caller.GetAttackHit(b, t))
                        {
                            if (t != damageTypes[damageTypes.Length - 1])
                            {
                                caller.DealDamage(b, 2, t, propertyBlockCombo, BattleHelper.ContactLevel.Infinite);
                            }
                            else
                            {
                                caller.DealDamage(b, 2, t, propertyBlock, BattleHelper.ContactLevel.Infinite);
                            }
                        }
                        else
                        {
                            caller.InvokeMissEvents(b);
                        }
                    }
                    if (t != damageTypes[damageTypes.Length - 1])
                    {
                        yield return new WaitForSeconds(0.25f);
                    }
                }
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

        ulong propertyBlock = (ulong)BattleHelper.DamageProperties.Static;

        string val;

        if (level > 1)
        {
            val = "" + caller.DealDamageCalculation(target, 15, BattleHelper.DamageType.Everything, propertyBlock);
        }
        else
        {
            BattleHelper.DamageType[] damageTypes = { BattleHelper.DamageType.Fire, BattleHelper.DamageType.Air, BattleHelper.DamageType.Earth, BattleHelper.DamageType.Light, BattleHelper.DamageType.Water, BattleHelper.DamageType.Dark };

            val = "";

            int count = 0;
            foreach (BattleHelper.DamageType t in damageTypes)
            { 
                if (count == 0)
                {
                    val = val + caller.DealDamageCalculation(target, 3, t, propertyBlock);
                } else
                {
                    val = val + ", " + caller.DealDamageCalculation(target, 3, t, propertyBlock) + "?";
                }
                count++;
            }            
        }

        return val;
    }
}

public class SM_AbyssalDawn : SoulMove
{
    public SM_AbyssalDawn()
    {
        useCount = 0;
    }

    int useCount = 0;

    public override int GetTextIndex()
    {
        return 11;
    }
    public override string GetName() => GetNameWithIndex(GetTextIndex());
    public override string GetDescription(int level = 1) => GetDescriptionWithIndex(GetTextIndex(), level);
    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveAlly, false);
    //public override float GetBasePower() => 0.5f;
    public override int GetBaseCost() => 30;

    public override bool CanChoose(BattleEntity caller, int level = 1)
    {
        if (useCount >= level)
        {
            return false;
        }
        return base.CanChoose(caller, level);
    }
    public override CantMoveReason GetCantMoveReason(BattleEntity caller, int level = 1)
    {
        if (useCount >= level)
        {
            return CantMoveReason.MoveExpended;
        }
        return base.GetCantMoveReason(caller, level);
    }

    public override TargetArea GetTargetArea(BattleEntity caller, int level = 1)
    {
        return new TargetArea(TargetArea.TargetAreaType.Ally, true);
    }

    public override int GetCost(BattleEntity caller, int level = 1)
    {
        return StandardCostCalculation(caller, level, 10);
    }

    /*
    public override int GetMaxLevel(BattleEntity caller)
    {
        if (caller is PlayerEntity pcaller)
        {
            return pcaller.GetSoulMoveMaxLevel(GetTextIndex());
        }
        return 2;
    }
    */

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        useCount++;
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
            yield return StartCoroutine(caller.JumpHeavy(caller.homePos, 1.5f, 0.3f, 0.15f));
            List<BattleEntity> targets = BattleControl.Instance.GetEntitiesSorted(caller, GetTargetArea(caller, level));
            foreach (BattleEntity b in targets)
            {
                b.HealHealth(b.maxHP);
            }
            yield return new WaitForSeconds(0.5f);
            caller.HealEnergy(BattleControl.Instance.maxEP);
            foreach (BattleEntity b in targets)
            {
                b.CureCurableEffects();
            }
        }
        else
        {
            yield return StartCoroutine(caller.JumpHeavy(caller.homePos, 0.5f, 0.15f, 0.15f));
            List<BattleEntity> targets = BattleControl.Instance.GetEntitiesSorted(caller, GetTargetArea(caller, level));
            foreach (BattleEntity b in targets)
            {
                b.HealHealth(b.maxHP / 2);
            }
            yield return new WaitForSeconds(0.5f);
            caller.HealEnergy(BattleControl.Instance.maxEP / 2);
            foreach (BattleEntity b in targets)
            {
                b.CureCurableEffects();
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