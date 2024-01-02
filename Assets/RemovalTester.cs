using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemovalTester : InteractTrigger
{
    public BattleHelper.EntityID entityID;
    public override void Interact()
    {
        Debug.Log("Interact");
        PlayerData.PlayerDataEntry luna = MainManager.Instance.playerData.GetPlayerDataEntry(entityID);

        if (luna == null)
        {
            Debug.Log("Unhide");
            MainManager.Instance.playerData.TransferFromHiddenParty(entityID);
        }
        else
        {
            Debug.Log("Hide");
            MainManager.Instance.playerData.TransferToHiddenParty(entityID);
        }
    }
}
