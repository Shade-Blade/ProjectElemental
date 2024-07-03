using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//move executed by Counter Flare entities (Targets all enemies to deal damage and heal)
public class CounterFlare : Move
{
    public override TargetArea GetBaseTarget()
    {
        return new TargetArea(TargetArea.TargetAreaType.LiveEnemy, true);
    }

    public override string GetDescription()
    {
        return "Counter damage and healing from the Counter Flare buff.";
    }

    public override string GetName()
    {
        return "(Counter Flare) Vengeful Flames";
    }

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        List<BattleEntity> targets = BattleControl.Instance.GetEntities(caller, GetBaseTarget());

        caller.HealHealth(caller.counterFlareDamage);

        //Impact effect
        GameObject eoShockwave;
        eoShockwave = Instantiate(Resources.Load<GameObject>("VFX/Battle/Moves/Special/Effect_CounterFlareShockwave"), BattleControl.Instance.transform);
        eoShockwave.transform.position = caller.transform.position;

        for (int i = 0; i < targets.Count; i++)
        {
            caller.DealDamage(targets[i], caller.counterFlareDamage, BattleHelper.DamageType.Fire, (ulong)(BattleHelper.DamageProperties.Static | BattleHelper.DamageProperties.IgnoreElementCalculation), BattleHelper.ContactLevel.Infinite);
        }

        yield return new WaitForSeconds(0.5f);
    }
}

public class ArcDischarge : Move
{
    public override TargetArea GetBaseTarget()
    {
        return new TargetArea(TargetArea.TargetAreaType.LiveAlly, true);
    }

    public override string GetDescription()
    {
        return "Damage from the Arc Discharge ailment.";
    }

    public override string GetName()
    {
        return "(Arc Discharge) Shockwave";
    }

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        List<BattleEntity> targets = BattleControl.Instance.GetEntities(caller, GetBaseTarget());

        //Impact effect
        GameObject eoShockwave;
        eoShockwave = Instantiate(Resources.Load<GameObject>("VFX/Battle/Moves/Special/Effect_ArcDischargeShockwave"), BattleControl.Instance.transform);
        eoShockwave.transform.position = caller.transform.position;

        yield return new WaitForSeconds(0.3f);

        for (int i = 0; i < targets.Count; i++)
        {
            caller.DealDamage(targets[i], caller.arcDischargeDamage, BattleHelper.DamageType.Air, (ulong)(BattleHelper.DamageProperties.Static | BattleHelper.DamageProperties.IgnoreElementCalculation), BattleHelper.ContactLevel.Infinite);
        }

        yield return new WaitForSeconds(0.5f);
    }
}

public class Splotch : Move
{
    public override TargetArea GetBaseTarget()
    {
        return new TargetArea(TargetArea.TargetAreaType.LiveEnemy, true);
    }

    public override string GetDescription()
    {
        return "Damage taken due to Splotch";
    }

    public override string GetName()
    {
        return "(Splotch) Burn";
    }

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        List<BattleEntity> targets = BattleControl.Instance.GetEntities(caller, GetBaseTarget());

        caller.lastAttacker = null;
        caller.TakeDamage(caller.splotchDamage, BattleHelper.DamageType.Fire, (ulong)(BattleHelper.DamageProperties.Static | BattleHelper.DamageProperties.IgnoreElementCalculation));

        yield return new WaitForSeconds(0.5f);
    }
}

public class ScaldingMagma : Move
{
    public override TargetArea GetBaseTarget()
    {
        return new TargetArea(TargetArea.TargetAreaType.LiveEnemy, true);
    }

    public override string GetDescription()
    {
        return "Damage taken due to Scalding Magma or Trial of Zeal";
    }

    public override string GetName()
    {
        return "(Environment) Magma Burn";
    }

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        List<BattleEntity> targets = BattleControl.Instance.GetEntities(caller, GetBaseTarget());

        caller.lastAttacker = null;
        caller.TakeDamage(caller.magmaDamage, BattleHelper.DamageType.Fire, (ulong)(BattleHelper.DamageProperties.Static | BattleHelper.DamageProperties.IgnoreElementCalculation));

        yield return new WaitForSeconds(0.5f);
    }
}

public class IonizedSandBolt : Move
{
    //note: executed from the perspective of one of the enemies
    public override TargetArea GetBaseTarget()
    {
        return new TargetArea(TargetArea.TargetAreaType.LiveEnemy, true);
    }

    public override string GetDescription()
    {
        return "[This should never be visible] Ionized Sand's lightning bolt every 3 turns thing";
    }

    public override string GetName()
    {
        return "(Environment) Lightning Bolt";
    }

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        BattleEntity newCaller = BattleControl.Instance.GetEntities((e) => e.posId >= 0)[0];

        List<BattleEntity> targets = BattleControl.Instance.GetEntities(newCaller, GetBaseTarget());

        for (int i = 0; i < targets.Count; i++)
        {
            BattleEntity.DealDamageSourceless(targets[i], 8, BattleHelper.DamageType.Air, (ulong)(BattleHelper.DamageProperties.Unblockable));
        }

        yield return null;
    }
}

public class TrialOfHasteBolt : Move
{
    //note: executed from the perspective of one of the enemies
    public override TargetArea GetBaseTarget()
    {
        return new TargetArea(TargetArea.TargetAreaType.LiveEnemy, true);
    }

    public override string GetDescription()
    {
        return "[This should never be visible] Trial of Haste's lightning bolt every 3 turns thing";
    }

    public override string GetName()
    {
        return "(Environment) Mega Bolt";
    }

    public override IEnumerator Execute(BattleEntity caller, int level = 1)
    {
        BattleEntity newCaller = BattleControl.Instance.GetEntities((e) => e.posId >= 0)[0];

        List<BattleEntity> targets = BattleControl.Instance.GetEntities(newCaller, GetBaseTarget());

        for (int i = 0; i < targets.Count; i++)
        {
            BattleEntity.DealDamageSourceless(targets[i], 75, BattleHelper.DamageType.Air, (ulong)(BattleHelper.DamageProperties.Unblockable));
        }

        yield return null;
    }
}
