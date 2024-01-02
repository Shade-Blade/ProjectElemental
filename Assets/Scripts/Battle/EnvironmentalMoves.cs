using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        return "Ionized Sand bolt";
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
        return "Trial of Haste bolt";
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
