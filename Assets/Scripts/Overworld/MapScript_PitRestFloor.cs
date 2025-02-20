using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapScript_PitRestFloor : MapScript
{
    int floor;
    public TextDisplayer floorNumberText;

    public MeshRenderer mr;
    public MeshRenderer mr_block;

    public List<Material> materials;


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

        if (floor == 50)
        {
            MainManager.Instance.AwardAchievement(MainManager.Achievement.ACH_Halfway);
        }

        switch ((floor - 1) / 10)
        {
            case 0:
                worldLocation = MainManager.WorldLocation.SolarGrove.ToString();
                skyboxID = MainManager.SkyboxID.SolarGrove.ToString();
                mr.material = materials[0];
                mr_block.material = materials[0];
                break;
            case 1:
                worldLocation = MainManager.WorldLocation.VerdantForest.ToString();
                skyboxID = MainManager.SkyboxID.VerdantForest.ToString();
                mr.material = materials[1];
                mr_block.material = materials[1];
                break;
            case 2:
                worldLocation = MainManager.WorldLocation.TempestDesert.ToString();
                skyboxID = MainManager.SkyboxID.TempestDesert.ToString();
                mr.material = materials[2];
                mr_block.material = materials[2];
                break;
            case 3:
                worldLocation = MainManager.WorldLocation.GemstoneIslands.ToString();
                skyboxID = MainManager.SkyboxID.GemstoneIslands.ToString();
                mr.material = materials[3];
                mr_block.material = materials[3];
                break;
            case 4:
                worldLocation = MainManager.WorldLocation.InfernalCaldera.ToString();
                skyboxID = MainManager.SkyboxID.InfernalCaldera.ToString();
                mr.material = materials[4];
                mr_block.material = materials[4];
                break;
            case 5:
                worldLocation = MainManager.WorldLocation.ShroudedValley.ToString();
                skyboxID = MainManager.SkyboxID.ShroudedValley.ToString();
                mr.material = materials[5];
                mr_block.material = materials[5];
                break;
            case 6:
                worldLocation = MainManager.WorldLocation.RadiantPlateau.ToString();
                skyboxID = MainManager.SkyboxID.RadiantPlateau.ToString();
                mr.material = materials[6];
                mr_block.material = materials[6];
                break;
            case 7:
                worldLocation = MainManager.WorldLocation.AetherTrench.ToString();
                skyboxID = MainManager.SkyboxID.AetherTrench.ToString();
                mr.material = materials[7];
                mr_block.material = materials[7];
                break;
            case 8:
                worldLocation = MainManager.WorldLocation.CrystalHills.ToString();
                skyboxID = MainManager.SkyboxID.CrystalHills.ToString();
                mr.material = materials[8];
                mr_block.material = materials[8];
                break;
            case 9:
                worldLocation = MainManager.WorldLocation.ForsakenMountains.ToString();
                skyboxID = MainManager.SkyboxID.ForsakenMountains.ToString();
                mr.material = materials[9];
                mr_block.material = materials[9];
                break;
        }

        MapExitHoleScript mehs = FindObjectOfType<MapExitHoleScript>();
        if (mehs != null)
        {
            if (floor % 10 == 9)
            {
                mehs.nextMap = MainManager.MapID.RabbitHole_RestFloor.ToString();
            }
            if (floor == 99)
            {
                mehs.nextMap = MainManager.MapID.RabbitHole_FinalFloor.ToString();
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
