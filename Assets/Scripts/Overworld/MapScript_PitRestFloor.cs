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

        int i = ((floor - 1) / 10);
        mr.material = materials[i];
        mr_block.material = materials[i];
        MainManager.WorldLocation wl = MainManager.WorldLocation.None;

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
