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
        return "<tail,w>Aster went in by himself...<next><tail,l>Yeah, I'm worried about him. Maybe we can catch up to him and help him out!<next><tail,w>He'll probably get to the bottom before we can.";
    }
}
