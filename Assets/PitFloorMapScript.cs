using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PitFloorMapScript : MapScript
{
    int floor;
    public TextDisplayer floorNumberText;

    public PitObstacleScript[] pitObstacles;

    public override void MapInit()
    {
        for (int i = 0; i < pitObstacles.Length; i++)
        {
            pitObstacles[i].gameObject.SetActive(true);
        }

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

        if (floor % 10 == 3 || floor % 10 == 5 || floor % 10 == 8)
        {
            PitObstacleScript pos = pitObstacles[RandomGenerator.GetIntRange(0, 4)];

            List<IRandomTableEntry<MainManager.MiscSprite>> randomTableEntries = new List<IRandomTableEntry<MainManager.MiscSprite>>();

            PlayerData.PlayerDataEntry pde = MainManager.Instance.playerData.GetPlayerDataEntry(BattleHelper.EntityID.Wilex);
            if (pde != null)
            {
                if (pde.weaponLevel < 0)
                {
                    randomTableEntries.Add(new RandomTableEntry<MainManager.MiscSprite>(MainManager.MiscSprite.AbilitySlash, 1));
                }
                if (pde.weaponLevel < 1 && floor > 30)
                {
                    randomTableEntries.Add(new RandomTableEntry<MainManager.MiscSprite>(MainManager.MiscSprite.AbilitySlash2, 1));
                }
                if (pde.weaponLevel < 2 && floor > 60)
                {
                    randomTableEntries.Add(new RandomTableEntry<MainManager.MiscSprite>(MainManager.MiscSprite.AbilitySlash3, 1));
                }
                if (pde.jumpLevel < 1 && floor > 20)
                {
                    randomTableEntries.Add(new RandomTableEntry<MainManager.MiscSprite>(MainManager.MiscSprite.AbilityDoubleJump, 1));
                }
                if (pde.jumpLevel < 2 && floor > 40)
                {
                    randomTableEntries.Add(new RandomTableEntry<MainManager.MiscSprite>(MainManager.MiscSprite.AbilitySuperJump, 1));
                }
            }
            pde = MainManager.Instance.playerData.GetPlayerDataEntry(BattleHelper.EntityID.Luna);
            if (pde != null)
            {
                if (pde.weaponLevel < 0)
                {
                    randomTableEntries.Add(new RandomTableEntry<MainManager.MiscSprite>(MainManager.MiscSprite.AbilitySmash, 1));
                }
                if (pde.weaponLevel < 1 && floor > 30)
                {
                    randomTableEntries.Add(new RandomTableEntry<MainManager.MiscSprite>(MainManager.MiscSprite.AbilitySmash2, 1));
                }
                if (pde.weaponLevel < 2 && floor > 60)
                {
                    randomTableEntries.Add(new RandomTableEntry<MainManager.MiscSprite>(MainManager.MiscSprite.AbilitySmash3, 1));
                }
                if (pde.jumpLevel < 1 && floor > 10)
                {
                    randomTableEntries.Add(new RandomTableEntry<MainManager.MiscSprite>(MainManager.MiscSprite.AbilityDashHop, 1));
                }
                if (pde.jumpLevel < 2 && floor > 50)
                {
                    randomTableEntries.Add(new RandomTableEntry<MainManager.MiscSprite>(MainManager.MiscSprite.AbilityDig, 1));
                }
            }
            RandomTable<MainManager.MiscSprite> randomTable = new RandomTable<MainManager.MiscSprite>(randomTableEntries);
            Debug.Log(randomTableEntries.Count);
            if (randomTableEntries.Count > 0)
            {
                pos.pu.type = PickupUnion.PickupType.Misc;
                pos.pu.misc = randomTable.Output();
                pos.cost = PickupUnion.GetBaseCost(pos.pu);
                pos.ChooseRandomType();
            }
        }

        switch ((floor - 1)/ 10)
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

    public override void OnExit(int exitID)
    {
        MainManager.Instance.SetGlobalVar(MainManager.GlobalVar.GV_PitFloor, (floor + 1).ToString());
    }
}
