using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class MapScript_PitFloor : MapScript
{
    int floor;
    public TextDisplayer floorNumberText;

    public PitObstacleScript[] pitObstacles;

    public MeshRenderer mr;
    public MeshRenderer mr_block;
    public MeshRenderer mr_blockB;

    public List<Material> materials;
    public List<Material> materialsBlockTransparent;

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
            //Debug.Log(randomTableEntries.Count);
            if (randomTableEntries.Count > 0)
            {
                pos.pu.type = PickupUnion.PickupType.Misc;
                pos.pu.misc = randomTable.Output();
                pos.cost = PickupUnion.GetBaseCost(pos.pu);
                pos.ChooseRandomType();
            }
        }

        Color ambientColor = new Color(0.5f, 0.5f, 0.5f, 1f);
        Color lightColor = new Color(1f, 1f, 1f, 1f);

        int f = ((floor - 1) / 10);
        mr.material = materials[f];
        mr_block.material = materials[f];
        mr_blockB.material = materialsBlockTransparent[f];
        MainManager.WorldLocation wl = MainManager.WorldLocation.None;

        switch (f)
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

    public override void OnExit(int exitID)
    {
        MainManager.Instance.SetGlobalVar(MainManager.GlobalVar.GV_PitFloor, (floor + 1).ToString());
    }

    public override string GetTattle()
    {
        switch ((floor - 1) / 10)
        {
            case 0:
                return "<tail,k>It looks like you have to beat the enemy on the floor to open the path ahead. You can also open the obstacles in the back to get useful items.";
            case 1:
                return "<tail,w>Wow, the background's completely different now. Where even is this place?";
            case 2:
                return "<tail,w>This desert is way too hot. Wherever this place leads next better cooler than this.<tail,k>You're... not exactly well dressed for the desert.";
            case 3:
                return "<tail,w>Hmm... If the hole in the middle is what leads to the next floor, maybe we can jump off the edge of the platform to get down there another way. Let's take a look...<next><tail,l><anim,l,talkweak>Eep! It's way too deep! I don't wanna fall in!<next><tail,k>Hmm... It seems that the hole to the next floor might be a portal of some kind, so you wouldn't get anywhere useful by jumping off.";
            case 4:
                return "<tail,w>Huff...Here I was thinking the desert was too hot.<next><tail,l>Don't push yourself too hard Wilex, I'll be here to help you!";
            case 5:
                return "<tail,k>I get the feeling that you're halfway to the bottom of this place. Make sure you be careful with your coins and items, so the enemies don't catch you unprepared.<next><tail,l>I hope it isn't going to keep getting darker... The enemies around here are getting really creepy too.";
            case 6:
                return "<tail,l><anim,l,talkweak>Its so c-cold!<next><tail,w>Keep moving Luna, It'll keep you warmer.";
            case 7:
                return "<tail,l>Now it's getting dark again. At least that's better than being frozen.<tail,k>This looks like the Aether Trench. I think these backgrounds are from different places around the world";
            case 8:
                return "<tail,k>I think you're getting close to the bottom. All the backgrounds seem to be different parts of the world, and you've been through most of those regions already.<next><tail,w>Yeah, that makes sense. We've seen all the biomes except for the Forsaken Mountains now, so I don't think there's much more to get through.";
            case 9:
                return "<tail,w>Looks like the background is like back home now. I think that means we're really close to the bottom.<next><tail,l>Yeah, it's good to see that again. But I think it was pretty dangerous outside our home village.<next><tail,k>Yes... You should be very careful. You're very close to the end, it would be unfortunate to lose this deep into this pit.";
        }
        return "";
    }
}
