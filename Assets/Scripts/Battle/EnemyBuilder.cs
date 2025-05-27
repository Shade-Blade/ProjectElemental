using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//builds enemies
//player characters use separate script
public class EnemyBuilder// : MonoBehaviour
{
    public static GameObject AnimationControllerSetup(BattleEntity be, GameObject sub, MainManager.SpriteID sid)
    {
        AnimationController ac = MainManager.CreateAnimationController(sid, sub);
        be.ac = ac;
        ac.gameObject.transform.parent = sub.transform;
        return ac.gameObject;
    }

    public static MainManager.SpriteID EntityIDToSpriteID(BattleHelper.EntityID id)
    {
        switch (id)
        {
            case BattleHelper.EntityID.Wilex:
                return MainManager.SpriteID.Wilex;
            case BattleHelper.EntityID.Luna:
                return MainManager.SpriteID.Luna;
            case BattleHelper.EntityID.Leafling:
                return MainManager.SpriteID.P_Leafling;
            case BattleHelper.EntityID.Flowerling:
                return MainManager.SpriteID.P_Flowerling;
            case BattleHelper.EntityID.Shrublet:
                return MainManager.SpriteID.P_Shrublet;
            case BattleHelper.EntityID.Sunflower:
                return MainManager.SpriteID.P_Sunflower;
            case BattleHelper.EntityID.Sunnybud:
                return MainManager.SpriteID.P_Sunnybud;
            case BattleHelper.EntityID.MiracleBloom:
                return MainManager.SpriteID.P_MiracleBloom;
            case BattleHelper.EntityID.Rockling:
                return MainManager.SpriteID.C1_Rockling;
            case BattleHelper.EntityID.Honeybud:
                return MainManager.SpriteID.C1_Honeybud;
            case BattleHelper.EntityID.BurrowTrap:
                return MainManager.SpriteID.C1_BurrowTrap;
            case BattleHelper.EntityID.Sundew:
                return MainManager.SpriteID.C1_Sundew;
            case BattleHelper.EntityID.Sycamore:
                return MainManager.SpriteID.C1_Rabbit_Sycamore;
            case BattleHelper.EntityID.Bandit:
                return MainManager.SpriteID.C2_FrogNormal_MaleBandit;
            case BattleHelper.EntityID.Renegade:
                return MainManager.SpriteID.C2_FrogSpiky_MaleBandit;
            case BattleHelper.EntityID.Sentry:
                return MainManager.SpriteID.C2_FrogPuffer_MaleBandit;
            case BattleHelper.EntityID.Cactupole:
                return MainManager.SpriteID.C2_Cactupole;
            case BattleHelper.EntityID.Sandswimmer:
                return MainManager.SpriteID.C2_Sandswimmer;
            case BattleHelper.EntityID.TournamentPawn:
                return MainManager.SpriteID.C2_FrogNormal_MaleFoxBandit;
            case BattleHelper.EntityID.TournamentKnight:
                return MainManager.SpriteID.C2_FrogCrested_Male;
            case BattleHelper.EntityID.TournamentBishopA:
                return MainManager.SpriteID.C2_FrogPuffer_Ruby;
            case BattleHelper.EntityID.TournamentBishopB:
                return MainManager.SpriteID.C2_FrogPuffer_Aqua;
            case BattleHelper.EntityID.TournamentRook:
                return MainManager.SpriteID.C2_FrogSpiky_MaleCitizen;
            case BattleHelper.EntityID.TournamentQueenA:
                return MainManager.SpriteID.C2_FrogPuffer_Amethyst;
            case BattleHelper.EntityID.TournamentQueenB:
                return MainManager.SpriteID.C2_FrogSpiky_Glaive;
            case BattleHelper.EntityID.Slime:
                return MainManager.SpriteID.C3_Slime;
            case BattleHelper.EntityID.Slimewalker:
                return MainManager.SpriteID.C3_Slimewalker;
            case BattleHelper.EntityID.Slimeworm:
                return MainManager.SpriteID.C3_Slimeworm;
            case BattleHelper.EntityID.Slimebloom:
                return MainManager.SpriteID.C3_Slimebloom;
            case BattleHelper.EntityID.SirenFish:
                return MainManager.SpriteID.C3_Sirenfish;
            case BattleHelper.EntityID.Blazecrest:
                return MainManager.SpriteID.C4_Flamecrest_BlazecrestMale;
            case BattleHelper.EntityID.Embercrest:
                return MainManager.SpriteID.C4_Flamecrest_EmbercrestMale;
            case BattleHelper.EntityID.Ashcrest:
                return MainManager.SpriteID.C4_Flamecrest_AshcrestMale;
            case BattleHelper.EntityID.Flametongue:
                return MainManager.SpriteID.C4_Flametongue_Ashcrest;
            case BattleHelper.EntityID.Heatwing:
                return MainManager.SpriteID.C4_Heatwing;
            case BattleHelper.EntityID.Lavaswimmer:
                return MainManager.SpriteID.C4_Lavaswimmer;
            case BattleHelper.EntityID.EyeSpore:
                return MainManager.SpriteID.C5_EyeSpore;
            case BattleHelper.EntityID.SpikeShroom:
                return MainManager.SpriteID.C5_SpikeShroom;
            case BattleHelper.EntityID.Shrouder:
                return MainManager.SpriteID.C5_Shrouder;
            case BattleHelper.EntityID.HoarderFly:
                return MainManager.SpriteID.C5_HoarderFly;
            case BattleHelper.EntityID.Mosquito:
                return MainManager.SpriteID.C5_Mosquito_Female;
            case BattleHelper.EntityID.Shieldwing:
                return MainManager.SpriteID.C6_Shieldwing;
            case BattleHelper.EntityID.Honeywing:
                return MainManager.SpriteID.C6_Honeywing;
            case BattleHelper.EntityID.Shimmerwing:
                return MainManager.SpriteID.C6_Shimmerwing;
            case BattleHelper.EntityID.LumistarVanguard:
                return MainManager.SpriteID.C6_Sparrow_MaleVanguard;
            case BattleHelper.EntityID.LumistarSoldier:
                return MainManager.SpriteID.C6_Hawk_FemaleSoldier;
            case BattleHelper.EntityID.LumistarStriker:
                return MainManager.SpriteID.C6_Hawk_MaleStriker;
            case BattleHelper.EntityID.KingIlum:
            case BattleHelper.EntityID.TyrantBlade:
                return MainManager.SpriteID.C6_Hawk_Ilum;
            case BattleHelper.EntityID.Plateshell:
                return MainManager.SpriteID.C7_Plateshell_Male;
            case BattleHelper.EntityID.Speartongue:
                return MainManager.SpriteID.C7_Speartongue_Male;
            case BattleHelper.EntityID.Chaintail:
                return MainManager.SpriteID.C7_Chaintail_Male;
            case BattleHelper.EntityID.Sawcrest:
                return MainManager.SpriteID.C7_Sawcrest;
            case BattleHelper.EntityID.Coiler:
                return MainManager.SpriteID.C7_Coiler;
            case BattleHelper.EntityID.Drillbeak:
                return MainManager.SpriteID.C7_Drillbeak;
            case BattleHelper.EntityID.AetherBoss:
            case BattleHelper.EntityID.AetherSuperboss:
                return MainManager.SpriteID.C7_Plateshell_Cutle;
            case BattleHelper.EntityID.PuffJelly:
                return MainManager.SpriteID.C8_PuffJelly;
            case BattleHelper.EntityID.Fluffling:
                return MainManager.SpriteID.C8_Fluffling;
            case BattleHelper.EntityID.CloudJelly:
                return MainManager.SpriteID.C8_CloudJelly;
            case BattleHelper.EntityID.CrystalCrab:
                return MainManager.SpriteID.C8_CrystalCrab;
            case BattleHelper.EntityID.CrystalSlug:
                return MainManager.SpriteID.C8_CrystalSlug;
            case BattleHelper.EntityID.CrystalClam:
                return MainManager.SpriteID.C8_CrystalClam;
            case BattleHelper.EntityID.AuroraWing:
                return MainManager.SpriteID.C8_AuroraWing;
            case BattleHelper.EntityID.FinalBoss:
            case BattleHelper.EntityID.FinalSuperboss:
                return MainManager.SpriteID.C8_Hydromander_Cyclus;
            case BattleHelper.EntityID.Plaguebud:
                return MainManager.SpriteID.E_Plaguebud_Male;
            case BattleHelper.EntityID.Starfish:
                return MainManager.SpriteID.E_Starfish;
            case BattleHelper.EntityID.CursedEye:
                return MainManager.SpriteID.E_CursedEye;
            case BattleHelper.EntityID.StrangeTendril:
                return MainManager.SpriteID.E_StrangeTendril;
            case BattleHelper.EntityID.DrainBud:
                return MainManager.SpriteID.E_DrainBud;
        }

        return MainManager.SpriteID.None;
    }

    //initializes posid to -1
    public static GameObject BuildEnemy(BattleHelper.EntityID id, Vector3 pos)
    {
        //create base game object
        GameObject g = new GameObject(id.ToString());
        g.transform.SetParent(BattleControl.Instance.transform);
        g.transform.position = pos;
        GameObject h = new GameObject("subobject");
        h.transform.parent = g.transform;
        h.transform.localPosition = Vector3.zero;

        //note: I want the sprite to be "bottom anchored" to the subobject
        GameObject so = new GameObject("sprite");           
        so.transform.parent = h.transform;
        SpriteRenderer s = so.AddComponent<SpriteRenderer>();
        s.sprite = MainManager.Instance.defaultSprite;
        s.material = MainManager.Instance.defaultSpriteMaterial;
        BattleEntity b = null;
        bool debugSprite = true;
        switch (id)
        {
            case BattleHelper.EntityID.Wilex:
                b = g.AddComponent<PlayerEntity>();
                debugSprite = false;
                break;
            case BattleHelper.EntityID.Luna:
                b = g.AddComponent<PlayerEntity>();
                debugSprite = false;
                break;
            case BattleHelper.EntityID.DebugEntity:
                b = g.AddComponent<BattleEntity>();
                s.color = new Color(0.9f, 1, 1f);
                break;
            case BattleHelper.EntityID.Leafling:
                b = g.AddComponent<BE_Leafling>();
                //replace debug stuff with actual stuff
                debugSprite = false;
                //s.color = new Color(0.5f, 1, 0.5f);
                break;
            case BattleHelper.EntityID.Flowerling:
                b = g.AddComponent<BE_Flowerling>();
                //replace debug stuff with actual stuff
                debugSprite = false;
                //s.color = new Color(0.8f, 1, 0.6f);
                break;
            case BattleHelper.EntityID.Shrublet:
                b = g.AddComponent<BE_Shrublet>();
                //replace debug stuff with actual stuff
                debugSprite = false;
                //s.color = new Color(0.6f, 0.5f, 0.4f);
                break;
            case BattleHelper.EntityID.Rootling:
                b = g.AddComponent<BE_Rootling>();
                s.color = new Color(0.5f, 1, 0.25f);
                break;
            case BattleHelper.EntityID.Sunflower:
                b = g.AddComponent<BE_Sunflower>();
                //s.color = new Color(1, 0.75f, 0f);
                debugSprite = false;
                break;
            case BattleHelper.EntityID.Sunnybud:
                b = g.AddComponent<BE_Sunnybud>();
                //s.color = new Color(1, 0.75f, 0.4f);
                debugSprite = false;
                break;
            case BattleHelper.EntityID.MiracleBloom:
                b = g.AddComponent<BE_MiracleBloom>();
                //s.color = new Color(1, 0.4f, 0.75f);
                debugSprite = false;
                break;
            case BattleHelper.EntityID.SunSapling:
                b = g.AddComponent<BE_SunSapling>();
                s.color = new Color(1, 0.8f, 0.8f);
                break;
            case BattleHelper.EntityID.Rockling:
                b = g.AddComponent<BE_Rockling>();
                //s.color = new Color(0.4f, 0.4f, 0.4f);
                debugSprite = false;
                break;
            case BattleHelper.EntityID.Honeybud:
                b = g.AddComponent<BE_Honeybud>();
                //s.color = new Color(1f, 0.85f, 0.6f);
                debugSprite = false;
                break;
            case BattleHelper.EntityID.BurrowTrap:
                b = g.AddComponent<BE_BurrowTrap>();
                //s.color = new Color(0.7f, 0.5f, 0.2f);
                debugSprite = false;
                break;
            case BattleHelper.EntityID.Sundew:
                b = g.AddComponent<BE_Sundew>();
                //s.color = new Color(0.7f, 0.7f, 0.2f);
                debugSprite = false;
                break;
            case BattleHelper.EntityID.VinePlatform:
                b = g.AddComponent<BE_VinePlatform>();
                s.color = new Color(0.25f, 0.6f, 0.25f);
                break;
            case BattleHelper.EntityID.Sycamore:
                b = g.AddComponent<BE_Sycamore>();
                //s.color = new Color(0.65f, 1, 0.65f);
                debugSprite = false;
                break;
            case BattleHelper.EntityID.GiantVine:
                b = g.AddComponent<BE_GiantVine>();
                s.color = new Color(1, 0.9f, 0.8f);
                break;
            case BattleHelper.EntityID.VineThrone:
                b = g.AddComponent<BE_VineThrone>();
                s.color = new Color(0.8f, 0.7f, 0.6f);
                break;
            case BattleHelper.EntityID.MasterOfAutumn:
                b = g.AddComponent<BE_MasterOfAutumn>();
                s.color = new Color(1, 0.9f, 0.8f);
                break;
            case BattleHelper.EntityID.Bandit:
                b = g.AddComponent<BE_Bandit>();
                //s.color = new Color(0.8f, 0.8f, 0.55f);
                debugSprite = false;
                break;
            case BattleHelper.EntityID.Renegade:
                b = g.AddComponent<BE_Renegade>();
                //s.color = new Color(0.9f, 0.9f, 0.65f);
                debugSprite = false;
                break;
            case BattleHelper.EntityID.Sentry:
                b = g.AddComponent<BE_Sentry>();
                //s.color = new Color(1, 1f, 0.75f);
                debugSprite = false;
                break;
            case BattleHelper.EntityID.Cactupole:
                b = g.AddComponent<BE_Cactupole>();
                //s.color = new Color(0.9f, 1f, 0.75f);
                debugSprite = false;
                break;
            case BattleHelper.EntityID.Sandswimmer:
                b = g.AddComponent<BE_Sandswimmer>();
                //s.color = new Color(0.7f, 0.7f, 0.45f);
                debugSprite = false;
                break;
            case BattleHelper.EntityID.DesertMinibossA:
            case BattleHelper.EntityID.DesertMinibossB:
            case BattleHelper.EntityID.DesertMinibossC:
            case BattleHelper.EntityID.DesertBossGuy:
            case BattleHelper.EntityID.StormCannon:
            case BattleHelper.EntityID.Stormtamer:
            case BattleHelper.EntityID.Stormkiller:
                s.color = new Color(1, 1f, 0.25f);
                break;
            case BattleHelper.EntityID.TrainingDummy:
            case BattleHelper.EntityID.TrainingDummyBoss:
                s.color = new Color(0.8f, 0.8f, 0.65f);
                break;
            case BattleHelper.EntityID.TournamentPawn:
                s.color = new Color(1, 0.9f, 0.25f);
                break;
            case BattleHelper.EntityID.TournamentKnight:
                debugSprite = false;
                break;
            case BattleHelper.EntityID.TournamentBishopA:
                debugSprite = false;
                break;
            case BattleHelper.EntityID.TournamentBishopB:
                debugSprite = false;
                break;
            case BattleHelper.EntityID.TournamentRook:
                debugSprite = false;
                break;
            case BattleHelper.EntityID.TournamentQueenA:
                debugSprite = false;
                break;
            case BattleHelper.EntityID.TournamentQueenB:
                debugSprite = false;
                break;
            case BattleHelper.EntityID.TournamentKing:
            case BattleHelper.EntityID.TournamentChampion:
                s.color = new Color(1, 0.9f, 0.25f);
                break;
            case BattleHelper.EntityID.Slime:
                b = g.AddComponent<BE_Slime>();
                //s.color = new Color(0.6f, 0.6f, 1f);
                debugSprite = false;
                break;
            case BattleHelper.EntityID.Slimewalker:
                b = g.AddComponent<BE_Slimewalker>();
                //s.color = new Color(0.5f, 0.5f, 1f);
                debugSprite = false;
                break;
            case BattleHelper.EntityID.Slimeworm:
                b = g.AddComponent<BE_Slimeworm>();
                //s.color = new Color(0.3f, 0.3f, 1f);
                debugSprite = false;
                break;
            case BattleHelper.EntityID.Slimebloom:
                b = g.AddComponent<BE_Slimebloom>();
                //s.color = new Color(0.4f, 0.4f, 1f);
                debugSprite = false;
                break;
            case BattleHelper.EntityID.SirenFish:
                b = g.AddComponent<BE_Sirenfish>();
                //s.color = new Color(0.7f, 0.8f, 1f);
                debugSprite = false;
                break;
            case BattleHelper.EntityID.FalseDragon:
            case BattleHelper.EntityID.FalseDragonArm:
            case BattleHelper.EntityID.AmalgamLeftArm:
            case BattleHelper.EntityID.AmalgamRightArm:
            case BattleHelper.EntityID.DiscordantAmalgam:
                s.color = new Color(0.6f, 0.6f, 1f);
                break;
            case BattleHelper.EntityID.Blazecrest:
                b = g.AddComponent<BE_Blazecrest>();
                //s.color = new Color(1, 0.5f, 0.5f);
                debugSprite = false;
                break;
            case BattleHelper.EntityID.Embercrest:
                b = g.AddComponent<BE_Embercrest>();
                //s.color = new Color(0.8f, 0.4f, 0.4f);
                debugSprite = false;
                break;
            case BattleHelper.EntityID.Ashcrest:
                b = g.AddComponent<BE_Ashcrest>();
                //s.color = new Color(0.7f, 0.4f, 0.4f);
                debugSprite = false;
                break;
            case BattleHelper.EntityID.Flametongue:
                b = g.AddComponent<BE_Flametongue>();
                //s.color = new Color(0.5f, 0.2f, 0.2f);
                debugSprite = false;
                break;
            case BattleHelper.EntityID.Heatwing:
                b = g.AddComponent<BE_Heatwing>();
                //s.color = new Color(1, 0.6f, 0.6f);
                debugSprite = false;
                break;
            case BattleHelper.EntityID.Lavaswimmer:
                b = g.AddComponent<BE_Lavaswimmer>();
                //s.color = new Color(0.4f, 0.2f, 0.2f);
                debugSprite = false;
                break;
            case BattleHelper.EntityID.LavaWyvern:
            case BattleHelper.EntityID.MetalWyvern:
                s.color = new Color(1, 0.5f, 0.5f);
                break;
            case BattleHelper.EntityID.EyeSpore:
                b = g.AddComponent<BE_EyeSpore>();
                //s.color = new Color(0.3f, 0.3f, 0.3f);
                debugSprite = false;
                break;
            case BattleHelper.EntityID.SpikeShroom:
                b = g.AddComponent<BE_SpikeShroom>();
                //s.color = new Color(0.2f, 0.2f, 0.2f);
                debugSprite = false;
                break;
            case BattleHelper.EntityID.Shrouder:
                b = g.AddComponent<BE_Shrouder>();
                //s.color = new Color(0.4f, 0.4f, 0.4f);
                debugSprite = false;
                break;
            case BattleHelper.EntityID.HoarderFly:
                b = g.AddComponent<BE_HoarderFly>();
                //s.color = new Color(0.5f, 0.5f, 0.5f);
                debugSprite = false;
                break;
            case BattleHelper.EntityID.Mosquito:
                b = g.AddComponent<BE_Mosquito>();
                //s.color = new Color(0.1f, 0.1f, 0.1f);
                debugSprite = false;
                break;
            case BattleHelper.EntityID.SporeSpider:
            case BattleHelper.EntityID.MoonSpider:
                s.color = new Color(0.4f, 0.4f, 0.4f);
                break;
            case BattleHelper.EntityID.Shieldwing:
                b = g.AddComponent<BE_Shieldwing>();
                //s.color = new Color(0.8f, 0.8f, 1);
                debugSprite = false;
                break;
            case BattleHelper.EntityID.Honeywing:
                b = g.AddComponent<BE_Honeywing>();
                //s.color = new Color(0.8f, 1, 0.8f);
                debugSprite = false;
                break;
            case BattleHelper.EntityID.Shimmerwing:
                b = g.AddComponent<BE_Shimmerwing>();
                //s.color = new Color(0.9f, 1, 1);
                debugSprite = false;
                break;
            case BattleHelper.EntityID.LumistarVanguard:
                b = g.AddComponent<BE_LumistarVanguard>();
                //s.color = new Color(0.8f, 0.9f, 0.9f);
                debugSprite = false;
                break;
            case BattleHelper.EntityID.LumistarSoldier:
                b = g.AddComponent<BE_LumistarSoldier>();
                //s.color = new Color(0.6f, 0.7f, 0.7f);
                debugSprite = false;
                break;
            case BattleHelper.EntityID.LumistarStriker:
                b = g.AddComponent<BE_LumistarStriker>();
                //s.color = new Color(0.7f, 0.8f, 0.8f);
                debugSprite = false;
                break;
            case BattleHelper.EntityID.KingIlum:
            case BattleHelper.EntityID.TyrantBlade:
                //s.color = new Color(0.8f, 0.9f, 0.9f);
                debugSprite = false;
                break;
            case BattleHelper.EntityID.Plateshell:
                b = g.AddComponent<BE_Plateshell>();
                //s.color = new Color(0.5f, 0.4f, 0.3f);
                debugSprite = false;
                break;
            case BattleHelper.EntityID.Speartongue:
                b = g.AddComponent<BE_Speartongue>();
                //s.color = new Color(0.4f, 0.3f, 0.2f);
                debugSprite = false;
                break;
            case BattleHelper.EntityID.Chaintail:
                b = g.AddComponent<BE_Chaintail>();
                //s.color = new Color(1, 0.9f, 0.2f);
                debugSprite = false;
                break;
            case BattleHelper.EntityID.Sawcrest:
                b = g.AddComponent<BE_Sawcrest>();
                //s.color = new Color(0.6f, 0.5f, 0.2f);
                debugSprite = false;
                break;
            case BattleHelper.EntityID.Coiler:
                b = g.AddComponent<BE_Coiler>();
                //s.color = new Color(1, 1, 0.2f);
                debugSprite = false;
                break;
            case BattleHelper.EntityID.Drillbeak:
                b = g.AddComponent<BE_Drillbeak>();
                //s.color = new Color(1, 0.85f, 0.7f);
                debugSprite = false;
                break;
            case BattleHelper.EntityID.AetherBoss:
            case BattleHelper.EntityID.AetherSuperboss:
                //s.color = new Color(1, 0.85f, 0.7f);
                debugSprite = false;
                break;
            case BattleHelper.EntityID.PuffJelly:
                b = g.AddComponent<BE_PuffJelly>();
                //s.color = new Color(0.7f, 1, 1);
                debugSprite = false;
                break;
            case BattleHelper.EntityID.Fluffling:
                b = g.AddComponent<BE_Fluffling>();
                //s.color = new Color(0.5f, 1, 1);
                debugSprite = false;
                break;
            case BattleHelper.EntityID.CloudJelly:
                b = g.AddComponent<BE_CloudJelly>();
                //s.color = new Color(0.4f, 1, 1);
                debugSprite = false;
                break;
            case BattleHelper.EntityID.CrystalCrab:
                b = g.AddComponent<BE_CrystalCrab>();
                //s.color = new Color(0.2f, 0.6f, 0.6f);
                debugSprite = false;
                break;
            case BattleHelper.EntityID.CrystalSlug:
                b = g.AddComponent<BE_CrystalSlug>();
                //s.color = new Color(0.5f, 0.8f, 0.8f);
                debugSprite = false;
                break;
            case BattleHelper.EntityID.CrystalClam:
                b = g.AddComponent<BE_CrystalClam>();
                //s.color = new Color(0.5f, 0.8f, 0.9f);
                debugSprite = false;
                break;
            case BattleHelper.EntityID.AuroraWing:
                b = g.AddComponent<BE_AuroraWing>();
                //s.color = new Color(0.6f, 1, 1);
                debugSprite = false;
                break;
            case BattleHelper.EntityID.FinalBoss:
            case BattleHelper.EntityID.FinalSuperboss:
                //s.color = new Color(0.5f, 1, 1);
                debugSprite = false;
                break;
            case BattleHelper.EntityID.Plaguebud:
                b = g.AddComponent<BE_Plaguebud>();
                debugSprite = false;
                //s.color = new Color(0.75f, 0.5f, 1f);
                break;
            case BattleHelper.EntityID.Starfish:
                b = g.AddComponent<BE_Starfish>();
                //s.color = new Color(0.35f, 0.25f, 0.45f);
                debugSprite = false;
                break;
            case BattleHelper.EntityID.CursedEye:
                b = g.AddComponent<BE_CursedEye>();
                //s.color = new Color(0.25f, 0.15f, 0.35f);
                debugSprite = false;
                break;
            case BattleHelper.EntityID.StrangeTendril:
                b = g.AddComponent<BE_StrangeTendril>();
                //s.color = new Color(0.15f, 0.05f, 0.25f);
                debugSprite = false;
                break;
            case BattleHelper.EntityID.DrainBud:
                b = g.AddComponent<BE_DrainBud>();
                //s.color = new Color(0.9f, 0.7f, 1f);
                debugSprite = false;
                break;
        }

        if (!debugSprite)
        {
            GameObject.Destroy(so);
            so = AnimationControllerSetup(b, h, EntityIDToSpriteID(id));
        }

        // = GetScript(id)

        b.entityID = id;
        b.subObject = h;
        b.flipDefault = ((int)id >= 0); //note: makes all enemies flipped visually
        b.Initialize();
        if (debugSprite)
        {
            s.transform.localScale = new Vector3(b.width, b.height, 1);
        }
        so.transform.localPosition = b.offset + Vector3.up * (b.height / 2);
        return g;
    }

    /*
    public static BattleEntity GetScript(BattleHelper.EntityID id) {
        //determine what script to return    
    }
     */
}
