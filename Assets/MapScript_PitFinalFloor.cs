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

        switch ((floor - 1) / 10)
        {
            case 0:
                worldLocation = MainManager.WorldLocation.SolarGrove.ToString();
                skyboxID = MainManager.SkyboxID.SolarGrove.ToString();
                break;
            case 1:
                worldLocation = MainManager.WorldLocation.VerdantForest.ToString();
                skyboxID = MainManager.SkyboxID.VerdantForest.ToString();
                break;
            case 2:
                worldLocation = MainManager.WorldLocation.TempestDesert.ToString();
                skyboxID = MainManager.SkyboxID.TempestDesert.ToString();
                break;
            case 3:
                worldLocation = MainManager.WorldLocation.GemstoneIslands.ToString();
                skyboxID = MainManager.SkyboxID.GemstoneIslands.ToString();
                break;
            case 4:
                worldLocation = MainManager.WorldLocation.InfernalCaldera.ToString();
                skyboxID = MainManager.SkyboxID.InfernalCaldera.ToString();
                break;
            case 5:
                worldLocation = MainManager.WorldLocation.ShroudedValley.ToString();
                skyboxID = MainManager.SkyboxID.ShroudedValley.ToString();
                break;
            case 6:
                worldLocation = MainManager.WorldLocation.RadiantPlateau.ToString();
                skyboxID = MainManager.SkyboxID.RadiantPlateau.ToString();
                break;
            case 7:
                worldLocation = MainManager.WorldLocation.AetherTrench.ToString();
                skyboxID = MainManager.SkyboxID.AetherTrench.ToString();
                break;
            case 8:
                worldLocation = MainManager.WorldLocation.CrystalHills.ToString();
                skyboxID = MainManager.SkyboxID.CrystalHills.ToString();
                break;
            case 9:
                worldLocation = MainManager.WorldLocation.ForsakenMountains.ToString();
                skyboxID = MainManager.SkyboxID.ForsakenMountains.ToString();
                break;
        }

        base.MapInit();
    }

    public override string GetTattle()
    {
        return "<tail,w>This is pretty disappointing. We went all the way down here for a sign?<next><tail,k>I don't think the developer has a boss fight for this place right now.<next><tail,l>At least Aster's down here with us.";
    }
}
