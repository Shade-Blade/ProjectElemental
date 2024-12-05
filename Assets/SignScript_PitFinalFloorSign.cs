using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SignScript_PitFinalFloorSign : SignScript, ITextSpeaker
{
    public override void Awake()
    {
        base.Awake();

        if (hasUnread && unreadIndicator != null && (globalFlag != null && globalFlag.Length > 0))
        {
            //attempt to parse
            MainManager.GlobalFlag gf;
            Enum.TryParse(globalFlag, out gf);
            bool flag = MainManager.Instance.GetGlobalFlag(gf);
            if (flag)
            {
                Destroy(unreadIndicator);
            }
        }

        if (!hasUnread && unreadIndicator != null)
        {
            Destroy(unreadIndicator);
        }
    }

    public override IEnumerator SignCutscene()
    {
        string[][] testTextFile = new string[4][];
        testTextFile[0] = new string[1];
        PlayerData pd = MainManager.Instance.playerData;
        PlayerData.PlayerDataEntry wilexPDE = pd.GetPlayerDataEntry(BattleHelper.EntityID.Wilex);
        PlayerData.PlayerDataEntry lunaPDE = pd.GetPlayerDataEntry(BattleHelper.EntityID.Luna);

        testTextFile[0][0] = "<sign>Thanks for playing! You have made it all the way to the final floor.<next>Total Battles: " + pd.totalBattles + "<line>Battles Won: " + pd.battlesWon + "<line>Battles Fled: " + pd.battlesFled + "<line>Battles Lost: " + pd.battlesLost;
        testTextFile[0][0] += "<next>Total Damage Dealt: " + (wilexPDE != null ? "<color,red>" + wilexPDE.cumulativeDamageDealt + "</color>/" : "") + (lunaPDE != null ? "<color,green>" + lunaPDE.cumulativeDamageDealt + "</color>/" : "") + "<color,yellow>" + pd.cumulativeDamageDealt + "</color>";
        testTextFile[0][0] += "<line>Total Damage Taken: " + (wilexPDE != null ? "<color,red>" + wilexPDE.cumulativeDamageTaken + "</color>/" : "") + (lunaPDE != null ? "<color,green>" + lunaPDE.cumulativeDamageTaken + "</color>/" : "") + "<color,yellow>" + pd.cumulativeDamageTaken + "</color>";
        testTextFile[0][0] += "<line>Maximum Damage Per Turn: " + (wilexPDE != null ? "<color,red>" + wilexPDE.maxDamagePerTurn + "</color>/" : "") + (lunaPDE != null ? "<color,green>" + lunaPDE.maxDamagePerTurn + "</color>/" : "") + "<color,yellow>" + pd.maxDamagePerTurn + "</color>";
        testTextFile[0][0] += "<line>Maximum Damage Single Hit: " + (wilexPDE != null ? "<color,red>" + wilexPDE.maxDamageSingleHit + "</color>/" : "") + (lunaPDE != null ? "<color,green>" + lunaPDE.maxDamageSingleHit + "</color>/" : "") + "<color,yellow>" + pd.maxDamageSingleHit + "</color>";
        testTextFile[0][0] += "<next>Total Playtime: " + MainManager.ParseTime(MainManager.Instance.playTime) + "<line>Time in front: " + (wilexPDE != null ? "<color,red>" + MainManager.ParseTime(wilexPDE.timeInFront) + "</color>" + (lunaPDE != null ? "/" : "") : "") + (lunaPDE != null ? "<color,green>" + MainManager.ParseTime(lunaPDE.timeInFront) + "</color>" : "");
        testTextFile[0][0] += "<line>Turns in front: " + (wilexPDE != null ? "<color,red>" + wilexPDE.turnsInFront + "</color>" + (lunaPDE != null ? "/" : "") : "") + (lunaPDE != null ? "<color,green>" + lunaPDE.turnsInFront + "</color> " : " ");
        testTextFile[0][0] += "<next>Badges Obtained: " + pd.badgeInventory.Count + "<line>Ribbons Obtained: " + pd.ribbonInventory.Count;
        testTextFile[0][0] += "<line>Items Obtained: " + pd.itemCounter;
        testTextFile[0][0] += "<line>Items Used: " + (wilexPDE != null ? "<color,red>" + wilexPDE.itemsUsed + "</color>/" : "") + (lunaPDE != null ? "<color,green>" + lunaPDE.itemsUsed + "</color>/" : "") + "<color,yellow>" + pd.itemsUsed + "</color>";

        if ((globalFlag != null && globalFlag.Length > 0))
        {
            //attempt to parse
            MainManager.GlobalFlag gf;
            Enum.TryParse(globalFlag, out gf);
            MainManager.Instance.SetGlobalFlag(gf, true);
        }

        if (unreadIndicator != null)
        {
            Destroy(unreadIndicator);
        }

        yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 0, this));
    }
}