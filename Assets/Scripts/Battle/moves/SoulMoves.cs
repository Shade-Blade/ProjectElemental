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

    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveAlly, false);
    //public override float GetBasePower() => 0.5f;
    public override int GetBaseCost(int level = 1) => BaseCostCalculation(8, level, 12);

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
        AC_PressATimed actionCommand = null;
        if (caller is PlayerEntity pcaller) //we have technology
        {
            actionCommand = gameObject.AddComponent<AC_PressATimed>();
            actionCommand.Init(pcaller);
            actionCommand.Setup(0.5f);
        }

        yield return new WaitForSeconds(ActionCommand.FADE_IN_TIME);
        caller.SetAnimation("soulcaststart");
        CastAnimation(caller, level);
        yield return new WaitUntil(() => actionCommand.IsComplete());

        bool result = actionCommand == null ? true : actionCommand.GetSuccess();
        if (actionCommand != null)
        {
            yield return new WaitForSeconds(ActionCommand.END_LAG);
            actionCommand.End();
            Destroy(actionCommand);
        }

        EndAnimation(caller, level, result);
        if (result)
        {
            if (level == 1)
            {
                caller.InflictEffect(caller.curTarget, new Effect(Effect.EffectType.Hustle, 1, 1));
            }
            else
            {
                List<BattleEntity> targets = BattleControl.Instance.GetEntitiesSorted(caller, GetTargetArea(caller, level));
                foreach (BattleEntity b in targets)
                {
                    caller.InflictEffect(b, new Effect(Effect.EffectType.Hustle, (sbyte)(level - 1), 1));
                }
            }
        }
        else
        {
            if (level == 1)
            {
                caller.InflictEffect(caller.curTarget, new Effect(Effect.EffectType.Hustle, 1, 1));
            }
            else
            {
                List<BattleEntity> targets = BattleControl.Instance.GetEntitiesSorted(caller, GetTargetArea(caller, level));
                foreach (BattleEntity b in targets)
                {
                    caller.InflictEffect(b, new Effect(Effect.EffectType.Hustle, (sbyte)(level - 1), 1));
                }
            }
        }
        yield return new WaitForSeconds(0.5f);
        caller.SetIdleAnimation();
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
        caller.SetAnimation("soulcastend");
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


    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.Ally, false);
    //public override float GetBasePower() => 0.5f;
    public override int GetBaseCost(int level = 1) => BaseCostCalculation(8, level, 8);

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
        return StandardCostCalculation(caller, level, 8);
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
        AC_PressATimed actionCommand = null;
        if (caller is PlayerEntity pcaller) //we have technology
        {
            actionCommand = gameObject.AddComponent<AC_PressATimed>();
            actionCommand.Init(pcaller);
            actionCommand.Setup(0.5f);
        }

        yield return new WaitForSeconds(ActionCommand.FADE_IN_TIME);
        caller.SetAnimation("soulcaststart");        
        CastAnimation(caller, level);
        yield return new WaitUntil(() => actionCommand.IsComplete());

        bool result = actionCommand == null ? true : actionCommand.GetSuccess();
        if (actionCommand != null)
        {
            yield return new WaitForSeconds(ActionCommand.END_LAG);
            actionCommand.End();
            Destroy(actionCommand);
        }

        EndAnimation(caller, level, result);
        if (result)
        {
            //yield return StartCoroutine(caller.JumpHeavy(caller.homePos, 1.5f, 0.3f, 0.15f));
            if (level > 1)
            {
                List<BattleEntity> targets = BattleControl.Instance.GetEntitiesSorted(caller, GetTargetArea(caller, level));
                foreach (BattleEntity b in targets)
                {
                    b.HealHealth(level * 6);
                }
                yield return new WaitForSeconds(0.5f);
                caller.HealEnergy(level * 6);
                yield return new WaitForSeconds(0.5f);
                foreach (BattleEntity b in targets)
                {
                    b.CureEffects(false);
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
                    b.CureEffects(false);
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
                    b.HealHealth(level * 3);
                }
                yield return new WaitForSeconds(0.5f);
                caller.HealEnergy(level * 3);
                yield return new WaitForSeconds(0.5f);
                foreach (BattleEntity b in targets)
                {
                    b.CureEffects(false);
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
                    b.CureEffects(false);
                }
            }
        }
        caller.SetIdleAnimation();
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
        caller.SetAnimation("soulcastend");
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


    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemy, true);
    //public override float GetBasePower() => 0.5f;
    public override int GetBaseCost(int level = 1) => BaseCostCalculation(16, level, 8);

    public override int GetCost(BattleEntity caller, int level = 1)
    {
        return StandardCostCalculation(caller, level, 8);
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
        AC_PressATimed actionCommand = null;
        if (caller is PlayerEntity pcaller) //we have technology
        {
            actionCommand = gameObject.AddComponent<AC_PressATimed>();
            actionCommand.Init(pcaller);
            actionCommand.Setup(0.5f);
        }

        yield return new WaitForSeconds(ActionCommand.FADE_IN_TIME);
        StartCoroutine(caller.SpinHeavy(Vector3.up * 360, 0.5f));
        CastAnimation(caller, level);
        yield return new WaitUntil(() => actionCommand.IsComplete());

        bool result = actionCommand == null ? true : actionCommand.GetSuccess();
        if (actionCommand != null)
        {
            yield return new WaitForSeconds(ActionCommand.END_LAG);
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
                        caller.DealDamage(b, level * 4, BattleHelper.DamageType.Earth, propertyBlock, BattleHelper.ContactLevel.Infinite);
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
                        caller.DealDamage(b, level * 2, BattleHelper.DamageType.Earth, propertyBlock, BattleHelper.ContactLevel.Infinite);
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
        Vector3 position = caller.transform.position + caller.height * Vector3.up + Vector3.up * 1f;
        eo.transform.position = position;
        eo.transform.localRotation = Quaternion.identity;
    }
    public void EndAnimation(BattleEntity caller, int level, bool result)
    {
        GameObject eo = null;
        eo = Instantiate(Resources.Load<GameObject>("VFX/Battle/Moves/Player/SoulMoves/Effect_EarthFlowOut"), BattleControl.Instance.transform);
        //position is the same as the one used by items
        Vector3 position = caller.transform.position + caller.height * Vector3.up + Vector3.up * 1f;
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


    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemy, true);
    //public override float GetBasePower() => 0.5f;
    public override int GetBaseCost(int level = 1) => BaseCostCalculation(12, level, 6);

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
        AC_PressATimed actionCommand = null;
        if (caller is PlayerEntity pcaller) //we have technology
        {
            actionCommand = gameObject.AddComponent<AC_PressATimed>();
            actionCommand.Init(pcaller);
            actionCommand.Setup(0.5f);
        }

        yield return new WaitForSeconds(ActionCommand.FADE_IN_TIME);
        StartCoroutine(caller.SpinHeavy(Vector3.up * 360, 0.5f));
        CastAnimation(caller, level);
        yield return new WaitUntil(() => actionCommand.IsComplete());

        bool result = actionCommand == null ? true : actionCommand.GetSuccess();
        if (actionCommand != null)
        {
            yield return new WaitForSeconds(ActionCommand.END_LAG);
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
                        caller.InflictEffect(b, new Effect(Effect.EffectType.AttackDown, 2, (sbyte)(level + 1)));
                        caller.InflictEffect(b, new Effect(Effect.EffectType.DefenseDown, 2, (sbyte)(level + 1)));
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
                        caller.InflictEffect(b, new Effect(Effect.EffectType.AttackDown, 1, (sbyte)(level + 1)));
                        caller.InflictEffect(b, new Effect(Effect.EffectType.DefenseDown, 1, (sbyte)(level + 1)));
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
        Vector3 position = caller.transform.position + caller.height * Vector3.up + Vector3.up * 1f;
        eo.transform.position = position;
        eo.transform.localRotation = Quaternion.identity;
    }
    public void EndAnimation(BattleEntity caller, int level, bool result)
    {
        GameObject eo = null;
        eo = Instantiate(Resources.Load<GameObject>("VFX/Battle/Moves/Player/SoulMoves/Effect_SparkFlowOut"), BattleControl.Instance.transform);
        //position is the same as the one used by items
        Vector3 position = caller.transform.position + caller.height * Vector3.up + Vector3.up * 1f;
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


    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemy, true);
    //public override float GetBasePower() => 0.5f;
    public override int GetBaseCost(int level = 1) => BaseCostCalculation(20, level, 10);

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
        AC_PressATimed actionCommand = null;
        if (caller is PlayerEntity pcaller) //we have technology
        {
            actionCommand = gameObject.AddComponent<AC_PressATimed>();
            actionCommand.Init(pcaller);
            actionCommand.Setup(0.5f);
        }

        yield return new WaitForSeconds(ActionCommand.FADE_IN_TIME);
        StartCoroutine(caller.SpinHeavy(Vector3.up * 360, 0.5f));
        CastAnimation(caller, level);
        yield return new WaitUntil(() => actionCommand.IsComplete());

        bool result = actionCommand == null ? true : actionCommand.GetSuccess();
        if (actionCommand != null)
        {
            yield return new WaitForSeconds(ActionCommand.END_LAG);
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
                        caller.DealDamage(b, 5 * level, BattleHelper.DamageType.Water, propertyBlock, BattleHelper.ContactLevel.Infinite);
                    }
                    else
                    {
                        caller.DealDamage(b, 5, BattleHelper.DamageType.Water, propertyBlock, BattleHelper.ContactLevel.Infinite);
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
                        caller.DealDamage(b, 3 * level, BattleHelper.DamageType.Water, propertyBlock, BattleHelper.ContactLevel.Infinite);
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
        Vector3 position = caller.transform.position + caller.height * Vector3.up + Vector3.up * 1f;
        eo.transform.position = position;
        eo.transform.localRotation = Quaternion.identity;
    }
    public void EndAnimation(BattleEntity caller, int level, bool result)
    {
        GameObject eo = null;
        eo = Instantiate(Resources.Load<GameObject>("VFX/Battle/Moves/Player/SoulMoves/Effect_WaterFlowOut"), BattleControl.Instance.transform);
        //position is the same as the one used by items
        Vector3 position = caller.transform.position + caller.height * Vector3.up + Vector3.up * 1f;
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
            val = caller.DealDamageCalculation(target, 10, BattleHelper.DamageType.Water, propertyBlock);
        } else
        {
            val = caller.DealDamageCalculation(target, 5, BattleHelper.DamageType.Water, propertyBlock);
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


    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemy, true);
    //public override float GetBasePower() => 0.5f;
    public override int GetBaseCost(int level = 1) => BaseCostCalculation(12, level, 12);

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
        AC_PressATimed actionCommand = null;
        if (caller is PlayerEntity pcaller) //we have technology
        {
            actionCommand = gameObject.AddComponent<AC_PressATimed>();
            actionCommand.Init(pcaller);
            actionCommand.Setup(0.5f);
        }

        yield return new WaitForSeconds(ActionCommand.FADE_IN_TIME);
        StartCoroutine(caller.SpinHeavy(Vector3.up * 360, 0.5f));
        CastAnimation(caller, level);
        yield return new WaitUntil(() => actionCommand.IsComplete());

        bool result = actionCommand == null ? true : actionCommand.GetSuccess();
        if (actionCommand != null)
        {
            yield return new WaitForSeconds(ActionCommand.END_LAG);
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
                        caller.InflictEffect(b, new Effect(Effect.EffectType.DefenseDown, (sbyte)(level), 3));
                        caller.InflictEffect(b, new Effect(Effect.EffectType.Sunder, (sbyte)(level * 2), Effect.INFINITE_DURATION));
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
                        caller.InflictEffect(b, new Effect(Effect.EffectType.DefenseDown, (sbyte)(level - 1), 3));
                        caller.InflictEffect(b, new Effect(Effect.EffectType.Sunder, (sbyte)(level), Effect.INFINITE_DURATION));
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
        Vector3 position = caller.transform.position + caller.height * Vector3.up + Vector3.up * 1f;
        eo.transform.position = position;
        eo.transform.localRotation = Quaternion.identity;
    }
    public void EndAnimation(BattleEntity caller, int level, bool result)
    {
        GameObject eo = null;
        eo = Instantiate(Resources.Load<GameObject>("VFX/Battle/Moves/Player/SoulMoves/Effect_FireFlowOut"), BattleControl.Instance.transform);
        //position is the same as the one used by items
        Vector3 position = caller.transform.position + caller.height * Vector3.up + Vector3.up * 1f;
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


    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemy, true);
    //public override float GetBasePower() => 0.5f;
    public override int GetBaseCost(int level = 1) => BaseCostCalculation(20, level, 10);

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
        AC_PressATimed actionCommand = null;
        if (caller is PlayerEntity pcaller) //we have technology
        {
            actionCommand = gameObject.AddComponent<AC_PressATimed>();
            actionCommand.Init(pcaller);
            actionCommand.Setup(0.5f);
        }

        yield return new WaitForSeconds(ActionCommand.FADE_IN_TIME);
        StartCoroutine(caller.SpinHeavy(Vector3.up * 360, 0.5f));
        CastAnimation(caller, level);
        yield return new WaitUntil(() => actionCommand.IsComplete());

        bool result = actionCommand == null ? true : actionCommand.GetSuccess();
        if (actionCommand != null)
        {
            yield return new WaitForSeconds(ActionCommand.END_LAG);
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
                        caller.InflictEffect(b, new Effect(Effect.EffectType.Defocus, (sbyte)(level * 6), Effect.INFINITE_DURATION));
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
                        caller.InflictEffect(b, new Effect(Effect.EffectType.Defocus, (sbyte)(level * 3), Effect.INFINITE_DURATION));
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
        Vector3 position = caller.transform.position + caller.height * Vector3.up + Vector3.up * 1f;
        eo.transform.position = position;
        eo.transform.localRotation = Quaternion.identity;
    }
    public void EndAnimation(BattleEntity caller, int level, bool result)
    {
        GameObject eo = null;
        eo = Instantiate(Resources.Load<GameObject>("VFX/Battle/Moves/Player/SoulMoves/Effect_DarkFlowOut"), BattleControl.Instance.transform);
        //position is the same as the one used by items
        Vector3 position = caller.transform.position + caller.height * Vector3.up + Vector3.up * 1f;
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


    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemy, true);
    //public override float GetBasePower() => 0.5f;
    public override int GetBaseCost(int level = 1) => BaseCostCalculation(15, level, 10);

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
        AC_PressATimed actionCommand = null;
        if (caller is PlayerEntity pcaller) //we have technology
        {
            actionCommand = gameObject.AddComponent<AC_PressATimed>();
            actionCommand.Init(pcaller);
            actionCommand.Setup(0.5f);
        }

        yield return new WaitForSeconds(ActionCommand.FADE_IN_TIME);
        StartCoroutine(caller.SpinHeavy(Vector3.up * 360, 0.5f));
        CastAnimation(caller, level);
        yield return new WaitUntil(() => actionCommand.IsComplete());

        bool result = actionCommand == null ? true : actionCommand.GetSuccess();
        if (actionCommand != null)
        {
            yield return new WaitForSeconds(ActionCommand.END_LAG);
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
                        caller.InflictEffect(b, new Effect(Effect.EffectType.Freeze, 1, (sbyte)(level * 2 + 1)));
                        caller.InflictEffect(b, new Effect(Effect.EffectType.Defocus, (sbyte)(level), Effect.INFINITE_DURATION));
                        caller.InflictEffect(b, new Effect(Effect.EffectType.Sunder, (sbyte)(level), Effect.INFINITE_DURATION));
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
                        caller.InflictEffect(b, new Effect(Effect.EffectType.Freeze, 1, (sbyte)(level)));
                        caller.InflictEffect(b, new Effect(Effect.EffectType.Defocus, (sbyte)(level - 1), Effect.INFINITE_DURATION));
                        caller.InflictEffect(b, new Effect(Effect.EffectType.Sunder, (sbyte)(level - 1), Effect.INFINITE_DURATION));
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
        Vector3 position = caller.transform.position + caller.height * Vector3.up + Vector3.up * 1f;
        eo.transform.position = position;
        eo.transform.localRotation = Quaternion.identity;
    }
    public void EndAnimation(BattleEntity caller, int level, bool result)
    {
        GameObject eo = null;
        eo = Instantiate(Resources.Load<GameObject>("VFX/Battle/Moves/Player/SoulMoves/Effect_LightFlowOut"), BattleControl.Instance.transform);
        //position is the same as the one used by items
        Vector3 position = caller.transform.position + caller.height * Vector3.up + Vector3.up * 1f;
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


    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemy, true);
    //public override float GetBasePower() => 0.5f;
    public override int GetBaseCost(int level = 1) => BaseCostCalculation(6, level, 6);

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
        AC_PressATimed actionCommand = null;
        if (caller is PlayerEntity pcaller) //we have technology
        {
            actionCommand = gameObject.AddComponent<AC_PressATimed>();
            actionCommand.Init(pcaller);
            actionCommand.Setup(0.5f);
        }

        yield return new WaitForSeconds(ActionCommand.FADE_IN_TIME);
        StartCoroutine(caller.SpinHeavy(Vector3.up * 360, 0.5f));
        CastAnimation(caller, level);
        yield return new WaitUntil(() => actionCommand.IsComplete());

        bool result = actionCommand == null ? true : actionCommand.GetSuccess();
        if (actionCommand != null)
        {
            yield return new WaitForSeconds(ActionCommand.END_LAG);
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
                    b.CleanseEffects(false);
                    if (level > 1)
                    {
                        caller.InflictEffect(b, new Effect(Effect.EffectType.Seal, 1, (sbyte)(level)));
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
                    b.CleanseEffects(false);
                    if (level > 1)
                    {
                        caller.InflictEffect(b, new Effect(Effect.EffectType.Seal, 1, (sbyte)(level - 1)));
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


    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemy, false);
    //public override float GetBasePower() => 0.5f;
    public override int GetBaseCost(int level = 1) => BaseCostCalculation(12, level, 12);

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
        AC_PressATimed actionCommand = null;
        if (caller is PlayerEntity pcaller) //we have technology
        {
            actionCommand = gameObject.AddComponent<AC_PressATimed>();
            actionCommand.Init(pcaller);
            actionCommand.Setup(0.5f);
        }

        yield return new WaitForSeconds(ActionCommand.FADE_IN_TIME);
        StartCoroutine(caller.SpinHeavy(Vector3.up * 360, 0.5f));
        CastAnimation(caller, level);
        yield return new WaitUntil(() => actionCommand.IsComplete());

        bool result = actionCommand == null ? true : actionCommand.GetSuccess();
        if (actionCommand != null)
        {
            yield return new WaitForSeconds(ActionCommand.END_LAG);
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
                        caller.InflictEffect(b, new Effect(Effect.EffectType.DrainSprout, 1, (sbyte)(level * 2)));
                        caller.InflictEffect(b, new Effect(Effect.EffectType.BoltSprout, 1, (sbyte)(level * 2)));
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
                        caller.InflictEffect(b, new Effect(Effect.EffectType.DrainSprout, 1, (sbyte)(level + 1)));
                        caller.InflictEffect(b, new Effect(Effect.EffectType.BoltSprout, 1, (sbyte)(level + 1)));
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

public class SM_ElementalConflux : SoulMove
{
    public SM_ElementalConflux()
    {
    }

    public override int GetTextIndex()
    {
        return 10;
    }


    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveAlly, false);
    //public override float GetBasePower() => 0.5f;
    public override int GetBaseCost(int level = 1) => BaseCostCalculation(20, level, 20);

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
        AC_PressATimedMultiple actionCommand = null;
        if (caller is PlayerEntity pcaller) //we have technology
        {
            actionCommand = gameObject.AddComponent<AC_PressATimedMultiple>();
            actionCommand.Init(pcaller);
            actionCommand.Setup(2.0f, 3);
        }

        yield return new WaitForSeconds(ActionCommand.FADE_IN_TIME);
        StartCoroutine(caller.SpinHeavy(Vector3.up * 360, 0.5f));
        StartCoroutine(CastAnimation(caller, level));
        yield return new WaitUntil(() => actionCommand.IsComplete());
        caller.SetIdleAnimation();

        bool result = actionCommand == null ? true : actionCommand.GetSuccess();
        if (actionCommand != null)
        {
            yield return new WaitForSeconds(ActionCommand.END_LAG);
            actionCommand.End();
            Destroy(actionCommand);
        }

        //EndAnimation(caller, level, result);
        if (result)
        {
            if (level == 1)
            {
                caller.CureEffects(false);
                yield return new WaitForSeconds(0.1f);
                caller.InflictEffect(caller.curTarget, new Effect(Effect.EffectType.AttackUp, 2, 3));
                yield return new WaitForSeconds(0.1f);
                caller.InflictEffect(caller.curTarget, new Effect(Effect.EffectType.Burst, 2, Effect.INFINITE_DURATION));
                yield return new WaitForSeconds(0.1f);
                caller.InflictEffect(caller.curTarget, new Effect(Effect.EffectType.EnduranceUp, 2, 3));
                yield return new WaitForSeconds(0.1f);
                caller.InflictEffect(caller.curTarget, new Effect(Effect.EffectType.AgilityUp, 2, 3));
                yield return new WaitForSeconds(0.1f);
                caller.InflictEffect(caller.curTarget, new Effect(Effect.EffectType.Absorb, 2, Effect.INFINITE_DURATION));
                yield return new WaitForSeconds(0.1f);
                caller.InflictEffect(caller.curTarget, new Effect(Effect.EffectType.DefenseUp, 2, 3));
                yield return new WaitForSeconds(0.1f);
                caller.InflictEffect(caller.curTarget, new Effect(Effect.EffectType.Focus, 2, Effect.INFINITE_DURATION));
            }
            else
            {
                List<BattleEntity> targets = BattleControl.Instance.GetEntitiesSorted(caller, GetTargetArea(caller, level));
                foreach (BattleEntity b in targets)
                {
                    b.CureEffects(false);
                }
                yield return new WaitForSeconds(0.1f);
                foreach (BattleEntity b in targets)
                {
                    caller.InflictEffect(b, new Effect(Effect.EffectType.AttackUp, (sbyte)(level + 1), 3));
                }
                yield return new WaitForSeconds(0.1f);
                foreach (BattleEntity b in targets)
                {
                    caller.InflictEffect(b, new Effect(Effect.EffectType.Burst, (sbyte)(level + 1), Effect.INFINITE_DURATION));
                }
                yield return new WaitForSeconds(0.1f);
                foreach (BattleEntity b in targets)
                {
                    caller.InflictEffect(b, new Effect(Effect.EffectType.EnduranceUp, (sbyte)(level + 1), 3));
                }
                yield return new WaitForSeconds(0.1f);
                foreach (BattleEntity b in targets)
                {
                    caller.InflictEffect(b, new Effect(Effect.EffectType.AgilityUp, (sbyte)(level + 1), 3));
                }
                yield return new WaitForSeconds(0.1f);
                foreach (BattleEntity b in targets)
                {
                    caller.InflictEffect(b, new Effect(Effect.EffectType.Absorb, (sbyte)(level + 1), Effect.INFINITE_DURATION));
                }
                yield return new WaitForSeconds(0.1f);
                foreach (BattleEntity b in targets)
                {
                    caller.InflictEffect(b, new Effect(Effect.EffectType.DefenseUp, (sbyte)(level + 1), 3));
                }
                yield return new WaitForSeconds(0.1f);
                foreach (BattleEntity b in targets)
                {
                    caller.InflictEffect(b, new Effect(Effect.EffectType.Focus, (sbyte)(level + 1), Effect.INFINITE_DURATION));
                }
            }
        }
        else
        {
            if (level == 1)
            {
                caller.CureEffects(false);
                yield return new WaitForSeconds(0.1f);
                caller.InflictEffect(caller.curTarget, new Effect(Effect.EffectType.AttackUp, 1, 3));
                yield return new WaitForSeconds(0.1f);
                caller.InflictEffect(caller.curTarget, new Effect(Effect.EffectType.Burst, 1, Effect.INFINITE_DURATION));
                yield return new WaitForSeconds(0.1f);
                caller.InflictEffect(caller.curTarget, new Effect(Effect.EffectType.EnduranceUp, 1, 3));
                yield return new WaitForSeconds(0.1f);
                caller.InflictEffect(caller.curTarget, new Effect(Effect.EffectType.AgilityUp, 1, 3));
                yield return new WaitForSeconds(0.1f);
                caller.InflictEffect(caller.curTarget, new Effect(Effect.EffectType.Absorb, 1, Effect.INFINITE_DURATION));
                yield return new WaitForSeconds(0.1f);
                caller.InflictEffect(caller.curTarget, new Effect(Effect.EffectType.DefenseUp, 1, 3));
                yield return new WaitForSeconds(0.1f);
                caller.InflictEffect(caller.curTarget, new Effect(Effect.EffectType.Focus, 1, Effect.INFINITE_DURATION));
            }
            else
            {
                List<BattleEntity> targets = BattleControl.Instance.GetEntitiesSorted(caller, GetTargetArea(caller, level));
                foreach (BattleEntity b in targets)
                {
                    b.CureEffects(false);
                }
                yield return new WaitForSeconds(0.1f);
                foreach (BattleEntity b in targets)
                {
                    caller.InflictEffect(b, new Effect(Effect.EffectType.AttackUp, (sbyte)(level), 3));
                }
                yield return new WaitForSeconds(0.1f);
                foreach (BattleEntity b in targets)
                {
                    caller.InflictEffect(b, new Effect(Effect.EffectType.Burst, (sbyte)(level), Effect.INFINITE_DURATION));
                }
                yield return new WaitForSeconds(0.1f);
                foreach (BattleEntity b in targets)
                {
                    caller.InflictEffect(b, new Effect(Effect.EffectType.EnduranceUp, (sbyte)(level), 3));
                }
                yield return new WaitForSeconds(0.1f);
                foreach (BattleEntity b in targets)
                {
                    caller.InflictEffect(b, new Effect(Effect.EffectType.AgilityUp, (sbyte)(level), 3));
                }
                yield return new WaitForSeconds(0.1f);
                foreach (BattleEntity b in targets)
                {
                    caller.InflictEffect(b, new Effect(Effect.EffectType.Absorb, (sbyte)(level), Effect.INFINITE_DURATION));
                }
                yield return new WaitForSeconds(0.1f);
                foreach (BattleEntity b in targets)
                {
                    caller.InflictEffect(b, new Effect(Effect.EffectType.DefenseUp, (sbyte)(level), 3));
                }
                yield return new WaitForSeconds(0.1f);
                foreach (BattleEntity b in targets)
                {
                    caller.InflictEffect(b, new Effect(Effect.EffectType.Focus, (sbyte)(level), Effect.INFINITE_DURATION));
                }
            }
        }
    }

    public IEnumerator CastAnimation(BattleEntity caller, int level)
    {
        yield return new WaitForSeconds(1.0f);
        caller.SetAnimation("itemuse");
        GameObject eo = null;
        eo = Instantiate(Resources.Load<GameObject>("VFX/Battle/Moves/Player/SoulMoves/Effect_FireFlowIn"), BattleControl.Instance.transform);
        //position is the same as the one used by items
        Vector3 position = caller.transform.position + caller.height * Vector3.up + Vector3.up * 1f;
        eo.transform.position = position;
        eo.transform.localRotation = Quaternion.identity;
        yield return new WaitForSeconds(0.1f);

        eo = Instantiate(Resources.Load<GameObject>("VFX/Battle/Moves/Player/SoulMoves/Effect_SparkFlowIn"), BattleControl.Instance.transform);
        eo.transform.position = position;
        eo.transform.localRotation = Quaternion.identity;
        yield return new WaitForSeconds(0.1f);

        eo = Instantiate(Resources.Load<GameObject>("VFX/Battle/Moves/Player/SoulMoves/Effect_EarthFlowIn"), BattleControl.Instance.transform);
        eo.transform.position = position;
        eo.transform.localRotation = Quaternion.identity;
        yield return new WaitForSeconds(0.1f);

        eo = Instantiate(Resources.Load<GameObject>("VFX/Battle/Moves/Player/SoulMoves/Effect_WaterFlowIn"), BattleControl.Instance.transform);
        eo.transform.position = position;
        eo.transform.localRotation = Quaternion.identity;
        yield return new WaitForSeconds(0.1f);

        eo = Instantiate(Resources.Load<GameObject>("VFX/Battle/Moves/Player/SoulMoves/Effect_LightFlowIn"), BattleControl.Instance.transform);
        eo.transform.position = position;
        eo.transform.localRotation = Quaternion.identity;
        yield return new WaitForSeconds(0.1f);

        eo = Instantiate(Resources.Load<GameObject>("VFX/Battle/Moves/Player/SoulMoves/Effect_DarkFlowIn"), BattleControl.Instance.transform);
        eo.transform.position = position;
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
        return AC_PressATimedMultiple.GetACDesc();
    }
}

public class SM_PrismaticBlast : SoulMove
{
    public override int GetTextIndex()
    {
        return 11;
    }


    public override TargetArea GetBaseTarget() => new TargetArea(TargetArea.TargetAreaType.LiveEnemy, true);
    //public override float GetBasePower() => 0.5f;
    public override int GetBaseCost(int level = 1) => BaseCostCalculation(30, level, 30);

    public override int GetCost(BattleEntity caller, int level = 1)
    {
        return StandardCostCalculation(caller, level, 30);
    }

    public override TargetArea GetTargetArea(BattleEntity caller, int level = 1)
    {
        return new TargetArea(TargetArea.TargetAreaType.LiveEnemy, true);
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
        AC_PressATimedMultiple actionCommand = null;
        if (caller is PlayerEntity pcaller) //we have technology
        {
            actionCommand = gameObject.AddComponent<AC_PressATimedMultiple>();
            actionCommand.Init(pcaller);
            actionCommand.Setup(2.0f, 3);
        }

        caller.SetAnimation("itemuse");
        yield return new WaitForSeconds(ActionCommand.FADE_IN_TIME);

        yield return new WaitUntil(() => actionCommand.IsComplete());

        bool result = actionCommand == null ? true : actionCommand.GetSuccess();
        if (actionCommand != null)
        {
            yield return new WaitForSeconds(ActionCommand.END_LAG);
            actionCommand.End();
            Destroy(actionCommand);
        }
        caller.SetIdleAnimation();


        List<BattleEntity> targets = BattleControl.Instance.GetEntitiesSorted(caller, GetTargetArea(caller, level));

        ulong propertyBlock = (ulong)(BattleHelper.DamageProperties.Static);
        ulong propertyBlockCombo = propertyBlock | (ulong)(BattleHelper.DamageProperties.Combo);
        if (result)
        {
            if (level > 1)
            {
                foreach (BattleEntity b in targets)
                {
                    if (caller.GetAttackHit(b, BattleHelper.DamageType.Prismatic))
                    {
                        caller.DealDamage(b, 15 * level, BattleHelper.DamageType.Prismatic, propertyBlock, BattleHelper.ContactLevel.Infinite);
                        caller.InflictEffect(b, new Effect(Effect.EffectType.AttackDown, (sbyte)(level), 3));
                        caller.InflictEffect(b, new Effect(Effect.EffectType.DefenseDown, (sbyte)(level), 3));
                        caller.InflictEffect(b, new Effect(Effect.EffectType.Defocus, (sbyte)(level), Effect.INFINITE_DURATION));
                        caller.InflictEffect(b, new Effect(Effect.EffectType.Sunder, (sbyte)(level), Effect.INFINITE_DURATION));
                    }
                    else
                    {
                        caller.InvokeMissEvents(b);
                    }
                }
            }
            else
            {
                foreach (BattleEntity b in targets)
                {
                    if (caller.GetAttackHit(b, BattleHelper.DamageType.Prismatic))
                    {
                        caller.DealDamage(b, 15, BattleHelper.DamageType.Prismatic, propertyBlock, BattleHelper.ContactLevel.Infinite);
                    }
                    else
                    {
                        caller.InvokeMissEvents(b);
                    }
                }
            }
        }
        else
        {
            if (level > 1)
            {
                foreach (BattleEntity b in targets)
                {
                    if (caller.GetAttackHit(b, BattleHelper.DamageType.Prismatic))
                    {
                        caller.DealDamage(b, 10 * level, BattleHelper.DamageType.Prismatic, propertyBlock, BattleHelper.ContactLevel.Infinite);
                        caller.InflictEffect(b, new Effect(Effect.EffectType.AttackDown, (sbyte)(level - 1), 3));
                        caller.InflictEffect(b, new Effect(Effect.EffectType.DefenseDown, (sbyte)(level - 1), 3));
                        caller.InflictEffect(b, new Effect(Effect.EffectType.Defocus, (sbyte)(level - 1), Effect.INFINITE_DURATION));
                        caller.InflictEffect(b, new Effect(Effect.EffectType.Sunder, (sbyte)(level - 1), Effect.INFINITE_DURATION));
                    }
                    else
                    {
                        caller.InvokeMissEvents(b);
                    }
                }
            }
            else
            {
                foreach (BattleEntity b in targets)
                {
                    if (caller.GetAttackHit(b, BattleHelper.DamageType.Prismatic))
                    {
                        caller.DealDamage(b, 10, BattleHelper.DamageType.Prismatic, propertyBlock, BattleHelper.ContactLevel.Infinite);
                    }
                    else
                    {
                        caller.InvokeMissEvents(b);
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
            val = "" + caller.DealDamageCalculation(target, 30, BattleHelper.DamageType.Prismatic, propertyBlock);
        }
        else
        {
            val = "" + caller.DealDamageCalculation(target, 15, BattleHelper.DamageType.Prismatic, propertyBlock);
        }

        return val;
    }

    public override string GetActionCommandDesc(int level = 1)
    {
        return AC_PressATimedMultiple.GetACDesc();
    }
}