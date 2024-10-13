using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//builds enemies
//player characters use separate script
public class EnemyBuilder : MonoBehaviour
{
    public static GameObject AnimationControllerSetup(BattleEntity be, GameObject sub, MainManager.SpriteID sid)
    {
        AnimationController ac = MainManager.CreateAnimationController(sid, sub);
        be.ac = ac;
        ac.gameObject.transform.parent = sub.transform;
        return ac.gameObject;
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

                //replace debug stuff with actual stuff
                Destroy(so);
                so = AnimationControllerSetup(b, h, MainManager.SpriteID.Wilex);

                break;
            case BattleHelper.EntityID.Luna:
                b = g.AddComponent<PlayerEntity>();
                debugSprite = false;

                //replace debug stuff with actual stuff
                Destroy(so);
                so = AnimationControllerSetup(b, h, MainManager.SpriteID.Luna);

                break;
            case BattleHelper.EntityID.DebugEntity:
                b = g.AddComponent<BattleEntity>();
                s.color = new Color(0.9f, 1, 1f);
                break;
            case BattleHelper.EntityID.Leafling:
                b = g.AddComponent<BE_Leafling>();

                //replace debug stuff with actual stuff
                debugSprite = false;
                Destroy(so);
                so = AnimationControllerSetup(b, h, MainManager.SpriteID.Leafling);

                //s.color = new Color(0.5f, 1, 0.5f);
                break;
            case BattleHelper.EntityID.Flowerling:
                b = g.AddComponent<BE_Flowerling>();

                //replace debug stuff with actual stuff
                debugSprite = false;
                Destroy(so);
                so = AnimationControllerSetup(b, h, MainManager.SpriteID.Flowerling);

                //s.color = new Color(0.8f, 1, 0.6f);
                break;
            case BattleHelper.EntityID.Shrublet:
                b = g.AddComponent<BE_Shrublet>();

                //replace debug stuff with actual stuff
                debugSprite = false;
                Destroy(so);
                so = AnimationControllerSetup(b, h, MainManager.SpriteID.Shrublet);

                //s.color = new Color(0.6f, 0.5f, 0.4f);
                break;
            case BattleHelper.EntityID.Rootling:
                b = g.AddComponent<BE_Rootling>();
                s.color = new Color(0.5f, 1, 0.25f);
                break;
            case BattleHelper.EntityID.Sunflower:
                b = g.AddComponent<BE_Sunflower>();
                s.color = new Color(1, 0.75f, 0f);
                break;
            case BattleHelper.EntityID.Sunnybud:
                b = g.AddComponent<BE_Sunnybud>();
                s.color = new Color(1, 0.75f, 0.4f);
                break;
            case BattleHelper.EntityID.MiracleBloom:
                b = g.AddComponent<BE_MiracleBloom>();
                s.color = new Color(1, 0.4f, 0.75f);
                break;
            case BattleHelper.EntityID.SunSapling:
                b = g.AddComponent<BE_SunSapling>();
                s.color = new Color(1, 0.8f, 0.8f);
                break;
            case BattleHelper.EntityID.Rockling:
                b = g.AddComponent<BE_Rockling>();
                s.color = new Color(0.4f, 0.4f, 0.4f);
                break;
            case BattleHelper.EntityID.Honeybud:
                b = g.AddComponent<BE_Honeybud>();
                s.color = new Color(1f, 0.85f, 0.6f);
                break;
            case BattleHelper.EntityID.BurrowTrap:
                b = g.AddComponent<BE_BurrowTrap>();
                s.color = new Color(0.7f, 0.5f, 0.2f);
                break;
            case BattleHelper.EntityID.Sundew:
                b = g.AddComponent<BE_Sundew>();
                s.color = new Color(0.7f, 0.7f, 0.2f);
                break;
            case BattleHelper.EntityID.VinePlatform:
                b = g.AddComponent<BE_VinePlatform>();
                s.color = new Color(0.25f, 0.6f, 0.25f);
                break;
            case BattleHelper.EntityID.Sycamore:
                b = g.AddComponent<BE_Sycamore>();
                //s.color = new Color(0.65f, 1, 0.65f);
                debugSprite = false;
                Destroy(so);
                so = AnimationControllerSetup(b, h, MainManager.SpriteID.C1_Rabbit_Sycamore);
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
                Destroy(so);
                so = AnimationControllerSetup(b, h, MainManager.SpriteID.C2_FrogNormal_MaleBandit);
                break;
            case BattleHelper.EntityID.Renegade:
                b = g.AddComponent<BE_Renegade>();
                //s.color = new Color(0.9f, 0.9f, 0.65f);
                debugSprite = false;
                Destroy(so);
                so = AnimationControllerSetup(b, h, MainManager.SpriteID.C2_FrogSpiky_MaleBandit);
                break;
            case BattleHelper.EntityID.Sentry:
                b = g.AddComponent<BE_Sentry>();
                //s.color = new Color(1, 1f, 0.75f);
                debugSprite = false;
                Destroy(so);
                so = AnimationControllerSetup(b, h, MainManager.SpriteID.C2_FrogPuffer_MaleBandit);
                break;
            case BattleHelper.EntityID.Cactupole:
                b = g.AddComponent<BE_Cactupole>();
                s.color = new Color(0.9f, 1f, 0.75f);
                break;
            case BattleHelper.EntityID.Sandswimmer:
                b = g.AddComponent<BE_Sandswimmer>();
                s.color = new Color(0.7f, 0.7f, 0.45f);
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
                Destroy(so);
                so = AnimationControllerSetup(b, h, MainManager.SpriteID.C2_FrogCrested_Male);
                break;
            case BattleHelper.EntityID.TournamentBishopA:
                debugSprite = false;
                Destroy(so);
                so = AnimationControllerSetup(b, h, MainManager.SpriteID.C2_FrogPuffer_Ruby);
                break;
            case BattleHelper.EntityID.TournamentBishopB:
                debugSprite = false;
                Destroy(so);
                so = AnimationControllerSetup(b, h, MainManager.SpriteID.C2_FrogPuffer_Aqua);
                break;
            case BattleHelper.EntityID.TournamentRook:
                debugSprite = false;
                Destroy(so);
                so = AnimationControllerSetup(b, h, MainManager.SpriteID.C2_FrogSpiky_MaleCitizen);
                break;
            case BattleHelper.EntityID.TournamentQueenA:
                debugSprite = false;
                Destroy(so);
                so = AnimationControllerSetup(b, h, MainManager.SpriteID.C2_FrogPuffer_Amethyst);
                break;
            case BattleHelper.EntityID.TournamentQueenB:
                debugSprite = false;
                Destroy(so);
                so = AnimationControllerSetup(b, h, MainManager.SpriteID.C2_FrogSpiky_Glaive);
                break;
            case BattleHelper.EntityID.TournamentKing:
            case BattleHelper.EntityID.TournamentChampion:
                s.color = new Color(1, 0.9f, 0.25f);
                break;
            case BattleHelper.EntityID.Slime:
                b = g.AddComponent<BE_Slime>();
                s.color = new Color(0.6f, 0.6f, 1f);
                break;
            case BattleHelper.EntityID.Slimewalker:
                b = g.AddComponent<BE_Slimewalker>();
                s.color = new Color(0.5f, 0.5f, 1f);
                break;
            case BattleHelper.EntityID.Slimeworm:
                b = g.AddComponent<BE_Slimeworm>();
                s.color = new Color(0.3f, 0.3f, 1f);
                break;
            case BattleHelper.EntityID.Slimebloom:
                b = g.AddComponent<BE_Slimebloom>();
                s.color = new Color(0.4f, 0.4f, 1f);
                break;
            case BattleHelper.EntityID.SirenFish:
                b = g.AddComponent<BE_Sirenfish>();
                s.color = new Color(0.7f, 0.8f, 1f);
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
                Destroy(so);
                so = AnimationControllerSetup(b, h, MainManager.SpriteID.C4_Flamecrest_BlazecrestMale);
                break;
            case BattleHelper.EntityID.Embercrest:
                b = g.AddComponent<BE_Embercrest>();
                //s.color = new Color(0.8f, 0.4f, 0.4f);
                debugSprite = false;
                Destroy(so);
                so = AnimationControllerSetup(b, h, MainManager.SpriteID.C4_Flamecrest_EmbercrestMale);
                break;
            case BattleHelper.EntityID.Ashcrest:
                b = g.AddComponent<BE_Ashcrest>();
                //s.color = new Color(0.7f, 0.4f, 0.4f);
                debugSprite = false;
                Destroy(so);
                so = AnimationControllerSetup(b, h, MainManager.SpriteID.C4_Flamecrest_AshcrestMale);
                break;
            case BattleHelper.EntityID.Flametongue:
                b = g.AddComponent<BE_Flametongue>();
                //s.color = new Color(0.5f, 0.2f, 0.2f);
                debugSprite = false;
                Destroy(so);
                so = AnimationControllerSetup(b, h, MainManager.SpriteID.C4_Flametongue_Ashcrest);
                break;
            case BattleHelper.EntityID.Heatwing:
                b = g.AddComponent<BE_Heatwing>();
                s.color = new Color(1, 0.6f, 0.6f);
                break;
            case BattleHelper.EntityID.Lavaswimmer:
                b = g.AddComponent<BE_Lavaswimmer>();
                s.color = new Color(0.4f, 0.2f, 0.2f);
                break;
            case BattleHelper.EntityID.LavaWyvern:
            case BattleHelper.EntityID.MetalWyvern:
                s.color = new Color(1, 0.5f, 0.5f);
                break;
            case BattleHelper.EntityID.EyeSpore:
                b = g.AddComponent<BE_EyeSpore>();
                s.color = new Color(0.3f, 0.3f, 0.3f);
                break;
            case BattleHelper.EntityID.SpikeShroom:
                b = g.AddComponent<BE_SpikeShroom>();
                s.color = new Color(0.2f, 0.2f, 0.2f);
                break;
            case BattleHelper.EntityID.Shrouder:
                b = g.AddComponent<BE_Shrouder>();
                s.color = new Color(0.4f, 0.4f, 0.4f);
                break;
            case BattleHelper.EntityID.HoarderFly:
                b = g.AddComponent<BE_HoarderFly>();
                s.color = new Color(0.5f, 0.5f, 0.5f);
                break;
            case BattleHelper.EntityID.Mosquito:
                b = g.AddComponent<BE_Mosquito>();
                //s.color = new Color(0.1f, 0.1f, 0.1f);
                debugSprite = false;
                Destroy(so);
                so = AnimationControllerSetup(b, h, MainManager.SpriteID.C5_Mosquito_Male);
                break;
            case BattleHelper.EntityID.SporeSpider:
            case BattleHelper.EntityID.MoonSpider:
                s.color = new Color(0.4f, 0.4f, 0.4f);
                break;
            case BattleHelper.EntityID.Shieldwing:
                b = g.AddComponent<BE_Shieldwing>();
                s.color = new Color(0.8f, 0.8f, 1);
                break;
            case BattleHelper.EntityID.Honeywing:
                b = g.AddComponent<BE_Honeywing>();
                s.color = new Color(0.8f, 1, 0.8f);
                break;
            case BattleHelper.EntityID.Shimmerwing:
                b = g.AddComponent<BE_Shimmerwing>();
                s.color = new Color(0.9f, 1, 1);
                break;
            case BattleHelper.EntityID.LumistarVanguard:
                b = g.AddComponent<BE_LumistarVanguard>();
                //s.color = new Color(0.8f, 0.9f, 0.9f);
                debugSprite = false;
                Destroy(so);
                so = AnimationControllerSetup(b, h, MainManager.SpriteID.C6_Sparrow_MaleVanguard);
                break;
            case BattleHelper.EntityID.LumistarSoldier:
                b = g.AddComponent<BE_LumistarSoldier>();
                //s.color = new Color(0.6f, 0.7f, 0.7f);
                debugSprite = false;
                Destroy(so);
                so = AnimationControllerSetup(b, h, MainManager.SpriteID.C6_Hawk_FemaleSoldier);
                break;
            case BattleHelper.EntityID.LumistarStriker:
                b = g.AddComponent<BE_LumistarStriker>();
                //s.color = new Color(0.7f, 0.8f, 0.8f);
                debugSprite = false;
                Destroy(so);
                so = AnimationControllerSetup(b, h, MainManager.SpriteID.C6_Hawk_MaleStriker);
                break;
            case BattleHelper.EntityID.KingIlum:
            case BattleHelper.EntityID.TyrantBlade:
                //s.color = new Color(0.8f, 0.9f, 0.9f);
                debugSprite = false;
                Destroy(so);
                so = AnimationControllerSetup(b, h, MainManager.SpriteID.C6_Hawk_Ilum);
                break;
            case BattleHelper.EntityID.Plateshell:
                b = g.AddComponent<BE_Plateshell>();
                //s.color = new Color(0.5f, 0.4f, 0.3f);
                debugSprite = false;
                Destroy(so);
                so = AnimationControllerSetup(b, h, MainManager.SpriteID.C7_Plateshell_Male);
                break;
            case BattleHelper.EntityID.Speartongue:
                b = g.AddComponent<BE_Speartongue>();
                //s.color = new Color(0.4f, 0.3f, 0.2f);
                debugSprite = false;
                Destroy(so);
                so = AnimationControllerSetup(b, h, MainManager.SpriteID.C7_Speartongue_Male);
                break;
            case BattleHelper.EntityID.Chaintail:
                b = g.AddComponent<BE_Chaintail>();
                //s.color = new Color(1, 0.9f, 0.2f);
                debugSprite = false;
                Destroy(so);
                so = AnimationControllerSetup(b, h, MainManager.SpriteID.C7_Chaintail_Male);
                break;
            case BattleHelper.EntityID.Sawcrest:
                b = g.AddComponent<BE_Sawcrest>();
                s.color = new Color(0.6f, 0.5f, 0.2f);
                break;
            case BattleHelper.EntityID.Coiler:
                b = g.AddComponent<BE_Coiler>();
                s.color = new Color(1, 1, 0.2f);
                break;
            case BattleHelper.EntityID.Drillbeak:
                b = g.AddComponent<BE_Drillbeak>();
                s.color = new Color(1, 0.85f, 0.7f);
                break;
            case BattleHelper.EntityID.AetherBoss:
            case BattleHelper.EntityID.AetherSuperboss:
                //s.color = new Color(1, 0.85f, 0.7f);
                debugSprite = false;
                Destroy(so);
                so = AnimationControllerSetup(b, h, MainManager.SpriteID.C7_Plateshell_Cutle);
                break;
            case BattleHelper.EntityID.PuffJelly:
                b = g.AddComponent<BE_PuffJelly>();
                s.color = new Color(0.7f, 1, 1);
                break;
            case BattleHelper.EntityID.Fluffling:
                b = g.AddComponent<BE_Fluffling>();
                s.color = new Color(0.5f, 1, 1);
                break;
            case BattleHelper.EntityID.CloudJelly:
                b = g.AddComponent<BE_CloudJelly>();
                s.color = new Color(0.4f, 1, 1);
                break;
            case BattleHelper.EntityID.CrystalCrab:
                b = g.AddComponent<BE_CrystalCrab>();
                s.color = new Color(0.2f, 0.6f, 0.6f);
                break;
            case BattleHelper.EntityID.CrystalSlug:
                b = g.AddComponent<BE_CrystalSlug>();
                s.color = new Color(0.5f, 0.8f, 0.8f);
                break;
            case BattleHelper.EntityID.CrystalClam:
                b = g.AddComponent<BE_CrystalClam>();
                s.color = new Color(0.5f, 0.8f, 0.9f);
                break;
            case BattleHelper.EntityID.AuroraWing:
                b = g.AddComponent<BE_AuroraWing>();
                s.color = new Color(0.6f, 1, 1);
                break;
            case BattleHelper.EntityID.FinalBoss:
            case BattleHelper.EntityID.FinalSuperboss:
                //s.color = new Color(0.5f, 1, 1);
                debugSprite = false;
                Destroy(so);
                so = AnimationControllerSetup(b, h, MainManager.SpriteID.C8_Hydromander_Cyclus);
                break;
            case BattleHelper.EntityID.Plaguebud:
                b = g.AddComponent<BE_Plaguebud>();
                debugSprite = false;
                Destroy(so);
                so = AnimationControllerSetup(b, h, MainManager.SpriteID.E_Plaguebud_Male);
                //s.color = new Color(0.75f, 0.5f, 1f);
                break;
            case BattleHelper.EntityID.Starfish:
                b = g.AddComponent<BE_Starfish>();
                s.color = new Color(0.35f, 0.25f, 0.45f);
                break;
            case BattleHelper.EntityID.CursedEye:
                b = g.AddComponent<BE_CursedEye>();
                s.color = new Color(0.25f, 0.15f, 0.35f);
                break;
            case BattleHelper.EntityID.StrangeTendril:
                b = g.AddComponent<BE_StrangeTendril>();
                s.color = new Color(0.15f, 0.05f, 0.25f);
                break;
            case BattleHelper.EntityID.DrainBud:
                b = g.AddComponent<BE_DrainBud>();
                s.color = new Color(0.9f, 0.7f, 1f);
                break;
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
        so.transform.localPosition = b.offset;
        return g;
    }

    /*
    public static BattleEntity GetScript(BattleHelper.EntityID id) {
        //determine what script to return    
    }
     */
}
