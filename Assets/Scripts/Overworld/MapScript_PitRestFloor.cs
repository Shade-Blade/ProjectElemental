using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapScript_PitRestFloor : MapScript
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
        if (floor == 1)
        {
            MainManager.Instance.PitReset();
        }

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

        MapExitHoleScript mehs = FindObjectOfType<MapExitHoleScript>();
        if (mehs != null)
        {
            if (floor % 10 == 9)
            {
                mehs.nextMap = "Test_PitRestFloor";
            }
            if (floor == 99)
            {
                mehs.nextMap = "Test_PitFinalFloor";
            }
        }

        base.MapInit();
    }

    public override void OnEnter(int exitID, Vector3 offset, float yawOffset)
    {
        if (exitID == -1)
        {
            //No NPCs
            PitNPCSpawner pns = FindObjectOfType<PitNPCSpawner>();
            Destroy(pns.gameObject);
        }

        base.OnEnter(exitID, offset, yawOffset);
    }

    public override void OnExit(int exitID)
    {
        MainManager.Instance.SetGlobalVar(MainManager.GlobalVar.GV_PitFloor, (floor + 1).ToString());
    }

    public override string GetTattle()
    {
        return "<tail,k>This looks like a safe floor. You can save here, or you can interact with the altar in the back to open the path forward.<next><tail,l>There's people here too. We can probably stock up on everything they're selling.<next><tail,w>Some of their prices are pretty expensive though. We probably shouldn't spend everything on stuff we can just get later on the normal floors.";
    }
}
