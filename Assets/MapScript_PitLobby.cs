using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapScript_PitLobby : MapScript
{
    public override void MapInit()
    {
        base.MapInit();
        MainManager.Instance.PitReset();
    }

    public override void OnExit(int exitID)
    {
        MainManager.Instance.SetGlobalVar(MainManager.GlobalVar.GV_PitFloor, (1).ToString());
    }

    public override string GetTattle()
    {
        return "";
    }
}
