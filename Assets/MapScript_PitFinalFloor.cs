using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MainManager;

public class MapScript_PitFinalFloor : MapScript
{
    int floor;
    public TextDisplayer floorNumberText;

    public override void MapInit()
    {
        string floorNo = MainManager.Instance.GetGlobalVar(MainManager.GlobalVar.GV_PitFloor);
        if (floorNo == null)
        {
            MainManager.Instance.SetGlobalVar(MainManager.GlobalVar.GV_PitFloor, 1.ToString());
            floorNo = 1.ToString();
        }
        floor = int.Parse(floorNo);
        floorNumberText.SetText(floorNo, true);

        MainManager.Instance.AwardAchievement(MainManager.Achievement.ACH_Complete);

        MainManager.WorldLocation wl = MainManager.WorldLocation.None;

        int i = ((floor - 1) / 10);
        switch (i)
        {
            case 0:
                wl = MainManager.WorldLocation.SolarGrove;
                break;
            case 1:
                wl = MainManager.WorldLocation.VerdantForest;
                break;
            case 2:
                wl = MainManager.WorldLocation.TempestDesert;
                break;
            case 3:
                wl = MainManager.WorldLocation.GemstoneIslands;
                break;
            case 4:
                wl = MainManager.WorldLocation.InfernalCaldera;
                break;
            case 5:
                wl = MainManager.WorldLocation.ShroudedValley;
                break;
            case 6:
                wl = MainManager.WorldLocation.RadiantPlateau;
                break;
            case 7:
                wl = MainManager.WorldLocation.AetherTrench;
                break;
            case 8:
                wl = MainManager.WorldLocation.CrystalHills;
                break;
            case 9:
                wl = MainManager.WorldLocation.ForsakenMountains;
                break;
        }
        worldLocation = wl.ToString();
        skyboxID = wl.ToString();
        this.ambientLight = MainManager.GetDefaultAmbientColor(wl);

        Light l = GetComponentInChildren<Light>();
        if (l != null)
        {
            l.color = MainManager.GetDefaultLightColor(wl);
        }
        MainManager.CreateDefaultParticles(wl, transform, false);

        base.MapInit();
    }

    public override string GetTattle()
    {
        return "<tail,w>This is pretty disappointing. We went all the way down here for a sign?<next><tail,k>I don't think the developer has a boss fight for this place right now.<next><tail,l>At least Aster's down here with us.";
    }
}
