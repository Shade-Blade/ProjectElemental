using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldEnemyEntityDieOnFlee : WorldEnemyEntity
{
    public override void HandleBattleOutcome(BattleHelper.BattleOutcome outcome)
    {
        if (gameObject == null)
        {
            return;
        }

        frameTimer = -1;
        firstStrikeTimer = -1;
        bsa = new BattleStartArguments();
        DeathDrops();
        DeathFlags();
        Destroy(gameObject);
    }
}
