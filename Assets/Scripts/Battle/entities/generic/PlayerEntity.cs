using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static BattleHelper;
using static Item;
using static MainManager;

public class PlayerEntity : BattleEntity
{
    public float blockFrames = -1;
    public float sblockFrames = -1;
    public List<BattleAction> tactics;
    public bool dead;

    //new setup for the menus
    public List<PlayerMove> jumpMoves;
    public List<PlayerMove> weaponMoves;
    public List<SoulMove> soulMoves;

    public int actionCommandSuccesses;
    public int blockSuccesses;

    //statistics (there might be badges that use these)
    public int damageDealt;
    public int hitsDealt;
    public int nonItemHitsDealt;
    public int damageTaken;
    public int lastTrueDamageTaken;
    public int hitsTaken;
    public int energyUsed;
    public int movesUsed;

    public bool lastHitWasBlocked;

    public int perTurnDamageDealt;
    public int perTurnDamageTaken;

    public int QuickSupply;   //quick bite sets this to 2, postmove decrements (so using the item -> leaves you with 1, then you do something else and it becomes 0)
    public int itemSaver;
    public int lastChance;
    public int undyingRage;
    public int lastRestTurn;
    public bool protectiveRushPerTurn;
    public int agilityRush;
    public bool revivalFlameHPCheck;
    public int revivalFlame;

    public string scanTable;

    //Block agility regen next turn
    public bool staminaBlock;


    public override void Initialize()
    {
        jumpMoves = new List<PlayerMove>();
        weaponMoves = new List<PlayerMove>();
        soulMoves = new List<SoulMove>();

        tactics = new List<BattleAction>();

        itemSaver = 0;
        QuickSupply = 0;

        alive = true;
        dead = false;

        AddTactics();

        AddMoves();

        AddSoulMoves();

        //consistency with overworld sprite (but the camera needs to be the same distance away from you in the overworld and in battle, though further camera in the overworld is not a problem)
        width = 0.35f;
        height = 0.75f;
        offset = Vector3.zero;
        selectionOffset = Vector3.zero;
        //offset = Vector3.up * (height / 2);
        //statusOffset = Vector3.up * (height) + Vector3.right * ((width / 2) + 0.4f);
        //selectionOffset = Vector3.up * (height + 0.5f);    
        statusOffset = Vector3.right * (0.1f);

        //Dropshadow is parallel to subobject (because it should not inherit subobject's rotation)
        if (!noShadow && dropShadow == null)
        {
            dropShadow = Instantiate(Resources.Load<GameObject>("Overworld/Other/DropShadow"), transform);
        }
    }

    public void AddMoves()
    {
        switch (entityID)
        {
            case EntityID.Wilex:
                AddWilexMoves();
                if (BadgeEquipped(Badge.BadgeType.SkillSwap) || MainManager.Instance.Cheat_SkillSwap)
                {
                    AddLunaMoves();
                }
                break;
            case EntityID.Luna:
                AddLunaMoves();
                if (BadgeEquipped(Badge.BadgeType.SkillSwap) || MainManager.Instance.Cheat_SkillSwap)
                {
                    AddWilexMoves();
                }
                break;
        }
    }

    public void AddWilexMoves()
    {
        if (GetWilexMoveMaxLevel((int)WilexMove.MoveType.HighStomp) > 0)
        {
            WM_HighStomp hs = gameObject.AddComponent<WM_HighStomp>();
            moveset.Add(hs);
            jumpMoves.Add(hs);
        }
        if (GetWilexMoveMaxLevel((int)WilexMove.MoveType.Focus) > 0)
        {
            WM_Focus f = gameObject.AddComponent<WM_Focus>();
            moveset.Add(f);
            jumpMoves.Add(f);
        }
        if (GetWilexMoveMaxLevel((int)WilexMove.MoveType.MultiStomp) > 0)
        {
            WM_MultiStomp ms = gameObject.AddComponent<WM_MultiStomp>();
            moveset.Add(ms);
            jumpMoves.Add(ms);
        }
        if (GetWilexMoveMaxLevel((int)WilexMove.MoveType.ElectroStomp) > 0)
        {
            WM_ElectroStomp es = gameObject.AddComponent<WM_ElectroStomp>();
            moveset.Add(es);
            jumpMoves.Add(es);
        }
        if (GetWilexMoveMaxLevel((int)WilexMove.MoveType.Taunt) > 0)
        {
            WM_Taunt t = gameObject.AddComponent<WM_Taunt>();
            moveset.Add(t);
            jumpMoves.Add(t);
        }
        if (GetWilexMoveMaxLevel((int)WilexMove.MoveType.ParalyzeStomp) > 0)
        {
            WM_ParalyzeStomp pas = gameObject.AddComponent<WM_ParalyzeStomp>();
            moveset.Add(pas);
            jumpMoves.Add(pas);
        }
        if (GetWilexMoveMaxLevel((int)WilexMove.MoveType.FlameStomp) > 0)
        {
            WM_FlameStomp fs = gameObject.AddComponent<WM_FlameStomp>();
            moveset.Add(fs);
            jumpMoves.Add(fs);
        }
        if (GetWilexMoveMaxLevel((int)WilexMove.MoveType.DoubleStomp) > 0)
        {
            WM_DoubleStomp dos = gameObject.AddComponent<WM_DoubleStomp>();
            moveset.Add(dos);
            jumpMoves.Add(dos);
        }
        if (GetWilexMoveMaxLevel((int)WilexMove.MoveType.Overstomp) > 0)
        {
            WM_Overstomp os = gameObject.AddComponent<WM_Overstomp>();
            moveset.Add(os);
            jumpMoves.Add(os);
        }
        if (GetWilexMoveMaxLevel((int)WilexMove.MoveType.SmartStomp) > 0)
        {
            WM_SmartStomp sms = gameObject.AddComponent<WM_SmartStomp>();
            moveset.Add(sms);
            jumpMoves.Add(sms);
        }
        if (GetWilexMoveMaxLevel((int)WilexMove.MoveType.TeamQuake) > 0)
        {
            WM_TeamQuake tq = gameObject.AddComponent<WM_TeamQuake>();
            moveset.Add(tq);
            jumpMoves.Add(tq);
        }
        if (GetWilexMoveMaxLevel((int)WilexMove.MoveType.EggToss) > 0)
        {
            WM_EggToss els = gameObject.AddComponent<WM_EggToss>();
            moveset.Add(els);
            jumpMoves.Add(els);
        }

        if (GetWilexMoveMaxLevel((int)WilexMove.MoveType.Slash) > 0)
        {
            WM_Slash s = gameObject.AddComponent<WM_Slash>();
            moveset.Add(s);
            weaponMoves.Add(s);
        }
        if (GetWilexMoveMaxLevel((int)WilexMove.MoveType.MultiSlash) > 0)
        {
            WM_MultiSlash ps = gameObject.AddComponent<WM_MultiSlash>();
            moveset.Add(ps);
            weaponMoves.Add(ps);
        }
        if (GetWilexMoveMaxLevel((int)WilexMove.MoveType.SlipSlash) > 0)
        {
            WM_SlipSlash ss = gameObject.AddComponent<WM_SlipSlash>();
            moveset.Add(ss);
            weaponMoves.Add(ss);
        }
        if (GetWilexMoveMaxLevel((int)WilexMove.MoveType.PoisonSlash) > 0)
        {
            WM_PoisonSlash pos = gameObject.AddComponent<WM_PoisonSlash>();
            moveset.Add(pos);
            weaponMoves.Add(pos);
        }
        if (GetWilexMoveMaxLevel((int)WilexMove.MoveType.PreciseStab) > 0)
        {
            WM_PreciseStab prs = gameObject.AddComponent<WM_PreciseStab>();
            moveset.Add(prs);
            weaponMoves.Add(prs);
        }
        if (GetWilexMoveMaxLevel((int)WilexMove.MoveType.SwordDischarge) > 0)
        {
            WM_SwordDischarge sd = gameObject.AddComponent<WM_SwordDischarge>();
            moveset.Add(sd);
            weaponMoves.Add(sd);
        }
        if (GetWilexMoveMaxLevel((int)WilexMove.MoveType.SwordDance) > 0)
        {
            WM_SwordDance swd = gameObject.AddComponent<WM_SwordDance>();
            moveset.Add(swd);
            weaponMoves.Add(swd);
        }
        if (GetWilexMoveMaxLevel((int)WilexMove.MoveType.BoomerangSlash) > 0)
        {
            WM_BoomerangSlash bs = gameObject.AddComponent<WM_BoomerangSlash>();
            moveset.Add(bs);
            weaponMoves.Add(bs);
        }
        if (GetWilexMoveMaxLevel((int)WilexMove.MoveType.DarkSlash) > 0)
        {
            WM_DarkSlash ds = gameObject.AddComponent<WM_DarkSlash>();
            moveset.Add(ds);
            weaponMoves.Add(ds);
        }
        if (GetWilexMoveMaxLevel((int)WilexMove.MoveType.Aetherize) > 0)
        {
            WM_Aetherize ae = gameObject.AddComponent<WM_Aetherize>();
            moveset.Add(ae);
            weaponMoves.Add(ae);
        }
        if (GetWilexMoveMaxLevel((int)WilexMove.MoveType.FlameBat) > 0)
        {
            WM_FlameBat fb = gameObject.AddComponent<WM_FlameBat>();
            moveset.Add(fb);
            weaponMoves.Add(fb);
        }
        if (GetWilexMoveMaxLevel((int)WilexMove.MoveType.AstralWall) > 0)
        {
            WM_AstralWall aw = gameObject.AddComponent<WM_AstralWall>();
            moveset.Add(aw);
            weaponMoves.Add(aw);
        }
    }
    public void AddLunaMoves()
    {
        if (GetLunaMoveMaxLevel((int)LunaMove.MoveType.HeavyStomp) > 0)
        {
            LM_HeavyStomp hs2 = gameObject.AddComponent<LM_HeavyStomp>(); //my current system requires adding all possible move scripts to an enemy
            moveset.Add(hs2);
            jumpMoves.Add(hs2);
        }
        if (GetLunaMoveMaxLevel((int)LunaMove.MoveType.Brace) > 0)
        {
            LM_Brace b2 = gameObject.AddComponent<LM_Brace>();
            moveset.Add(b2);
            jumpMoves.Add(b2);
        }
        if (GetLunaMoveMaxLevel((int)LunaMove.MoveType.DashThrough) > 0)
        {
            LM_DashThrough dt = gameObject.AddComponent<LM_DashThrough>();
            moveset.Add(dt);
            jumpMoves.Add(dt);
        }
        if (GetLunaMoveMaxLevel((int)LunaMove.MoveType.FlipKick) > 0)
        {
            LM_FlipKick fk = gameObject.AddComponent<LM_FlipKick>();
            moveset.Add(fk);
            jumpMoves.Add(fk);
        }
        if (GetLunaMoveMaxLevel((int)LunaMove.MoveType.FluffHeal) > 0)
        {
            LM_FluffHeal fh = gameObject.AddComponent<LM_FluffHeal>();
            moveset.Add(fh);
            jumpMoves.Add(fh);
        }
        if (GetLunaMoveMaxLevel((int)LunaMove.MoveType.SleepStomp) > 0)
        {
            LM_SleepStomp sls = gameObject.AddComponent<LM_SleepStomp>();
            moveset.Add(sls);
            jumpMoves.Add(sls);
        }
        if (GetLunaMoveMaxLevel((int)LunaMove.MoveType.MeteorStomp) > 0)
        {
            LM_MeteorStomp mes = gameObject.AddComponent<LM_MeteorStomp>();
            moveset.Add(mes);
            jumpMoves.Add(mes);
        }
        if (GetLunaMoveMaxLevel((int)LunaMove.MoveType.UnderStrike) > 0)
        {
            LM_UnderStrike us = gameObject.AddComponent<LM_UnderStrike>();
            moveset.Add(us);
            jumpMoves.Add(us);
        }
        if (GetLunaMoveMaxLevel((int)LunaMove.MoveType.IronStomp) > 0)
        {
            LM_IronStomp irs = gameObject.AddComponent<LM_IronStomp>();
            moveset.Add(irs);
            jumpMoves.Add(irs);
        }
        if (GetLunaMoveMaxLevel((int)LunaMove.MoveType.ElementalStomp) > 0)
        {
            LM_ElementalStomp els = gameObject.AddComponent<LM_ElementalStomp>();
            moveset.Add(els);
            jumpMoves.Add(els);
        }
        if (GetLunaMoveMaxLevel((int)LunaMove.MoveType.TeamThrow) > 0)
        {
            LM_TeamThrow tt = gameObject.AddComponent<LM_TeamThrow>();
            moveset.Add(tt);
            jumpMoves.Add(tt);
        }
        if (GetLunaMoveMaxLevel((int)LunaMove.MoveType.DoubleEgg) > 0)
        {
            LM_DoubleEgg els = gameObject.AddComponent<LM_DoubleEgg>();
            moveset.Add(els);
            jumpMoves.Add(els);
        }

        if (GetLunaMoveMaxLevel((int)LunaMove.MoveType.Smash) > 0)
        {
            LM_Smash s2 = gameObject.AddComponent<LM_Smash>();
            moveset.Add(s2);
            weaponMoves.Add(s2);
        }
        if (GetLunaMoveMaxLevel((int)LunaMove.MoveType.PowerSmash) > 0)
        {
            LM_PowerSmash ps2 = gameObject.AddComponent<LM_PowerSmash>();
            moveset.Add(ps2);
            weaponMoves.Add(ps2);
        }
        if (GetLunaMoveMaxLevel((int)LunaMove.MoveType.DazzleSmash) > 0)
        {
            LM_DazzleSmash das = gameObject.AddComponent<LM_DazzleSmash>();
            moveset.Add(das);
            weaponMoves.Add(das);
        }
        if (GetLunaMoveMaxLevel((int)LunaMove.MoveType.HammerThrow) > 0)
        {
            LM_HammerThrow ht = gameObject.AddComponent<LM_HammerThrow>();
            moveset.Add(ht);
            weaponMoves.Add(ht);
        }
        if (GetLunaMoveMaxLevel((int)LunaMove.MoveType.BreakerSmash) > 0)
        {
            LM_BreakerSmash brs = gameObject.AddComponent<LM_BreakerSmash>();
            moveset.Add(brs);
            weaponMoves.Add(brs);
        }
        if (GetLunaMoveMaxLevel((int)LunaMove.MoveType.FlameSmash) > 0)
        {
            LM_FlameSmash fls = gameObject.AddComponent<LM_FlameSmash>();
            moveset.Add(fls);
            weaponMoves.Add(fls);
        }
        if (GetLunaMoveMaxLevel((int)LunaMove.MoveType.MomentumSmash) > 0)
        {
            LM_MomentumSmash ms = gameObject.AddComponent<LM_MomentumSmash>();
            moveset.Add(ms);
            weaponMoves.Add(ms);
        }
        if (GetLunaMoveMaxLevel((int)LunaMove.MoveType.QuakeSmash) > 0)
        {
            LM_QuakeSmash qs = gameObject.AddComponent<LM_QuakeSmash>();
            moveset.Add(qs);
            weaponMoves.Add(qs);
        }
        if (GetLunaMoveMaxLevel((int)LunaMove.MoveType.LightSmash) > 0)
        {
            LM_LightSmash ls = gameObject.AddComponent<LM_LightSmash>();
            moveset.Add(ls);
            weaponMoves.Add(ls);
        }
        if (GetLunaMoveMaxLevel((int)LunaMove.MoveType.Illuminate) > 0)
        {
            LM_Illuminate pa = gameObject.AddComponent<LM_Illuminate>();
            moveset.Add(pa);
            weaponMoves.Add(pa);
        }
        if (GetLunaMoveMaxLevel((int)LunaMove.MoveType.HammerBeat) > 0)
        {
            LM_HammerBeat hb = gameObject.AddComponent<LM_HammerBeat>();
            moveset.Add(hb);
            weaponMoves.Add(hb);
        }
        if (GetLunaMoveMaxLevel((int)LunaMove.MoveType.MistWall) > 0)
        {
            LM_MistWall mw = gameObject.AddComponent<LM_MistWall>();
            moveset.Add(mw);
            weaponMoves.Add(mw);
        }
    }
    public void AddSoulMoves()
    {
        PlayerTurnController ptc = PlayerTurnController.Instance;

        string pitaltar = MainManager.Instance.GetGlobalVar(MainManager.GlobalVar.GV_PitAltar);
        if (pitaltar == null)
        {
            MainManager.Instance.SetGlobalVar(MainManager.GlobalVar.GV_PitAltar, 0.ToString());
            pitaltar = MainManager.Instance.GetGlobalVar(MainManager.GlobalVar.GV_PitAltar);
        }
        int altar = int.Parse(pitaltar);

        if (altar >= 1)
        {
            SM_Revitalize r = ptc.GetOrAddComponent<SM_Revitalize>();
            soulMoves.Add(r);
            SM_Hasten sp = ptc.GetOrAddComponent<SM_Hasten>();
            soulMoves.Add(sp);
        }
        if (altar >= 2)
        {
            SM_LeafStorm les = ptc.GetOrAddComponent<SM_LeafStorm>();
            soulMoves.Add(les);
        }
        if (altar >= 3)
        {
            SM_ElectroDischarge ed = ptc.GetOrAddComponent<SM_ElectroDischarge>();
            soulMoves.Add(ed);
        }
        if (altar >= 4)
        {
            SM_MistWave miw = ptc.GetOrAddComponent<SM_MistWave>();
            soulMoves.Add(miw);
        }
        if (altar >= 5)
        {
            SM_Overheat oh = ptc.GetOrAddComponent<SM_Overheat>();
            soulMoves.Add(oh);
        }
        if (altar >= 6)
        {
            SM_VoidCrush vc = ptc.GetOrAddComponent<SM_VoidCrush>();
            soulMoves.Add(vc);
        }
        if (altar >= 7)
        {
            SM_FlashFreeze ff = ptc.GetOrAddComponent<SM_FlashFreeze>();
            soulMoves.Add(ff);
        }
        if (altar >= 8)
        {
            SM_Cleanse c = ptc.GetOrAddComponent<SM_Cleanse>();
            soulMoves.Add(c);
            SM_Blight b = ptc.GetOrAddComponent<SM_Blight>();
            soulMoves.Add(b);
        }
        if (altar >= 9)
        {
            SM_ElementalConflux cb = ptc.GetOrAddComponent<SM_ElementalConflux>();
            soulMoves.Add(cb);
            SM_PrismaticBlast ad = ptc.GetOrAddComponent<SM_PrismaticBlast>();
            soulMoves.Add(ad);
        }
    }
    public void AddTactics()
    {
        PlayerTurnController ptc = PlayerTurnController.Instance;
        tactics.Add(ptc.GetOrAddComponent<BA_Check>());
        tactics.Add(ptc.GetOrAddComponent<BA_Scan>());
        tactics.Add(ptc.GetOrAddComponent<BA_SwapEntities>());
        tactics.Add(ptc.GetOrAddComponent<BA_Rest>());
        tactics.Add(ptc.GetOrAddComponent<BA_Flee>());

        if (BadgeEquipped(Badge.BadgeType.DodgeStep))
        {
            tactics.Add(ptc.GetOrAddComponent<BA_EasyFlee>());
        }

        //tactics.Add(ptc.GetOrAddComponent<BA_TurnRelay>());

        if (BadgeEquipped(Badge.BadgeType.EffectRelay))
        {
            tactics.Add(ptc.GetOrAddComponent<BA_EffectRelay>());
        }

        if (MainManager.Instance.Cheat_BattleRandomActions)
        {
            tactics.Add(ptc.GetOrAddComponent<BA_Cheat_RandomMove>());
            tactics.Add(ptc.GetOrAddComponent<BA_Cheat_RandomItem>());
        }
        if (MainManager.Instance.Cheat_BattleCheatActions)
        {
            tactics.Add(ptc.GetOrAddComponent<BA_Cheat_BonusTurn>());
            tactics.Add(ptc.GetOrAddComponent<BA_Cheat_Kill>());
            tactics.Add(ptc.GetOrAddComponent<BA_Cheat_Flee>());
            tactics.Add(ptc.GetOrAddComponent<BA_Cheat_Win>());
            tactics.Add(ptc.GetOrAddComponent<BA_Cheat_Lose>());
        }
        if (MainManager.Instance.Cheat_BattleInfoActions)
        {
            tactics.Add(ptc.GetOrAddComponent<BA_Cheat_SeeDefenseTable>());
            tactics.Add(ptc.GetOrAddComponent<BA_Cheat_SeeStatusTable>());
            tactics.Add(ptc.GetOrAddComponent<BA_Cheat_SeeStatChanges>());
        }
    }
    public int GetWilexMoveMaxLevel(int index)
    {
        if (MainManager.Instance.Cheat_WilexMoveUnlock)
        {
            return GetWilexMoveTrueMaxLevel(index);
        }

        //To do later: flag check and add special case of not having the move unlocked but you have the badge equipped anyway (it unlocks the move)

        int baseLevel = GetWilexMoveBaseLevel(index);
        int maxLevel = GetWilexMoveTrueMaxLevel(index);

        if (MainManager.Instance.Cheat_LevelAnarchy)
        {
            if (index != 0 && index != 12)
            {
                return 20;
            }
        }

        switch (index)
        {
            case 0:
                return Mathf.Min(baseLevel, maxLevel);
            case 1:
                return Mathf.Min(baseLevel + BadgeEquippedCount(Badge.BadgeType.Focus), maxLevel);
            case 2:
                return Mathf.Min(baseLevel + BadgeEquippedCount(Badge.BadgeType.MultiStomp), maxLevel);
            case 3:
                return Mathf.Min(baseLevel + BadgeEquippedCount(Badge.BadgeType.ElectroStomp), maxLevel);
            case 4:
                return Mathf.Min(baseLevel + BadgeEquippedCount(Badge.BadgeType.Taunt), maxLevel);
            case 5:
                return Mathf.Min(baseLevel + BadgeEquippedCount(Badge.BadgeType.ParalyzeStomp), maxLevel);
            case 6:
                return Mathf.Min(baseLevel + BadgeEquippedCount(Badge.BadgeType.FlameStomp), maxLevel);
            case 7:
                return Mathf.Min(baseLevel + BadgeEquippedCount(Badge.BadgeType.DoubleStomp), maxLevel);
            case 8:
                return Mathf.Min(baseLevel + BadgeEquippedCount(Badge.BadgeType.Overstomp), maxLevel);
            case 9:
                return Mathf.Min(baseLevel + BadgeEquippedCount(Badge.BadgeType.SmartStomp), maxLevel);
            case 10:
                return Mathf.Min(baseLevel + BadgeEquippedCount(Badge.BadgeType.TeamQuake), maxLevel);
            case 11:
                return Mathf.Min(baseLevel + BadgeEquippedCount(Badge.BadgeType.StrangeEgg), maxLevel);
            case 12:
                return Mathf.Min(baseLevel, maxLevel);
            case 13:
                return Mathf.Min(baseLevel + BadgeEquippedCount(Badge.BadgeType.MultiSlash), maxLevel);
            case 14:
                return Mathf.Min(baseLevel + BadgeEquippedCount(Badge.BadgeType.SlipSlash), maxLevel);
            case 15:
                return Mathf.Min(baseLevel + BadgeEquippedCount(Badge.BadgeType.PoisonSlash), maxLevel);
            case 16:
                return Mathf.Min(baseLevel + BadgeEquippedCount(Badge.BadgeType.PreciseStab), maxLevel);
            case 17:
                return Mathf.Min(baseLevel + BadgeEquippedCount(Badge.BadgeType.SwordDischarge), maxLevel);
            case 18:
                return Mathf.Min(baseLevel + BadgeEquippedCount(Badge.BadgeType.SwordDance), maxLevel);
            case 19:
                return Mathf.Min(baseLevel + BadgeEquippedCount(Badge.BadgeType.BoomerangSlash), maxLevel);
            case 20:
                return Mathf.Min(baseLevel + BadgeEquippedCount(Badge.BadgeType.DarkSlash), maxLevel);
            case 21:
                int l = baseLevel + BadgeEquippedCount(Badge.BadgeType.Aetherize);
                if (l > 0)
                {
                    l++;
                }
                return Mathf.Min(l, maxLevel); //this one goes 0, 2, 3, 4...
            case 22:
                return Mathf.Min(baseLevel + BadgeEquippedCount(Badge.BadgeType.FlameBat), maxLevel);
            case 23:
                int l2 = baseLevel + BadgeEquippedCount(Badge.BadgeType.AstralWall);
                if (l2 > 0)
                {
                    l2++;
                }
                return Mathf.Min(l2, maxLevel);
        }
        return 1;
    }
    public int GetWilexMoveBaseLevel(int index)
    {
        int jlevel = BattleControl.Instance.playerData.GetPlayerDataEntry(entityID).jumpLevel;
        int wlevel = BattleControl.Instance.playerData.GetPlayerDataEntry(entityID).weaponLevel;
        int level = BattleControl.Instance.playerData.level;

        //to do: flag check
        switch (index)
        {
            case 0:
                return 1;   //high stomp (base move)
            case 1:
                return 1;   //focus
            case 2:
                return 0;   //multi stomp
            case 3:
                return jlevel >= 1 ? 1 : 0;   //electro stomp
            case 4:
                return 0;   //taunt
            case 5:
                return 0;   //paralyze
            case 6:
                return jlevel >= 2 ? 1 : 0;   //flame stomp
            case 7:
                return level >= 15 ? 1 : 0;   //double stomp
            case 8:
                return 0;   //overstomp
            case 9:
                return 0;   //smart stomp
            case 10:
                return level >= 21 ? 1 : 0;   //team quake
            case 11:
                return 0;   //egg toss 
            case 12:
                return wlevel >= 0 ? 1 : 0;   //slash
            case 13:
                return 1;   //multislash
            case 14:
                return level >= 3 ? 1 : 0;   //slip slash
            case 15:
                return 0;   //poison
            case 16:
                return wlevel >= 1 ? 1 : 0;   //precise stab
            case 17:
                return 0;   //sword discharge
            case 18:
                return 0;   //sword dance
            case 19:
                return 0;   //boomerang slash
            case 20:
                return wlevel >= 2 ? 1 : 0;   //dark slash
            case 21:
                return wlevel >= 2 ? 1 : 0; //aetherize (note: has special coding that makes it go 0, 2...)
            case 22:
                return 0;   //flame bat
            case 23:
                return 0;   //astral wall
        }
        return 1;
    }
    public int GetWilexMoveTrueMaxLevel(int index)
    {
        return 20; //;)

        /*
        //Note: these moves aren't set up properly to have higher levels than these
        //(Most of them do nothing)
        switch (index)
        {
            case 0:
                return 1;
            case 1:
                return 3;
            case 2:
                return 1;
            case 3:
                return 2;
            case 4:
                return 1;
            case 5:
                return 1;
            case 6:
                return 2;
            case 7:
                return 2;
            case 8:
                return 1;
            case 9:
                return 1;
            case 10:
                return 2;
            case 11:
                return 1;
            case 12:
                return 1;
            case 13:
                return 3;
            case 14:
                return 2;
            case 15:
                return 1;
            case 16:
                return 2;
            case 17:
                return 1;
            case 18:
                return 1;
            case 19:
                return 1;
            case 20:
                return 2;
            case 21:
                return 3; //this one goes 0, 2, 3
            case 22:
                return 1;
            case 23:
                return 3;
        }
        return 1;
        */
    }
    public int GetLunaMoveMaxLevel(int index)
    {
        if (MainManager.Instance.Cheat_LunaMoveUnlock)
        {
            return GetLunaMoveTrueMaxLevel(index);
        }

        //To do later: flag check and add special case of not having the move unlocked but you have the badge equipped anyway (it unlocks the move)

        int baseLevel = GetLunaMoveBaseLevel(index);
        int maxLevel = GetLunaMoveTrueMaxLevel(index);

        if (MainManager.Instance.Cheat_LevelAnarchy)
        {
            if (index != 0 && index != 12)
            {
                return 20;
            }
        }

        switch (index)
        {
            case 0:
                return Mathf.Min(baseLevel, maxLevel);
            case 1:
                return Mathf.Min(baseLevel + BadgeEquippedCount(Badge.BadgeType.Brace), maxLevel);
            case 2:
                return Mathf.Min(baseLevel + BadgeEquippedCount(Badge.BadgeType.DashThrough), maxLevel);
            case 3:
                return Mathf.Min(baseLevel + BadgeEquippedCount(Badge.BadgeType.FlipKick), maxLevel);
            case 4:
                int fh = baseLevel + BadgeEquippedCount(Badge.BadgeType.FluffHeal);
                if (fh > 0)
                {
                    fh++;
                }
                return Mathf.Min(fh, maxLevel);
            case 5:
                return Mathf.Min(baseLevel + BadgeEquippedCount(Badge.BadgeType.SleepStomp), maxLevel);
            case 6:
                return Mathf.Min(baseLevel + BadgeEquippedCount(Badge.BadgeType.MeteorStomp), maxLevel);
            case 7:
                return Mathf.Min(baseLevel + BadgeEquippedCount(Badge.BadgeType.UnderStrike), maxLevel);
            case 8:
                return Mathf.Min(baseLevel + BadgeEquippedCount(Badge.BadgeType.IronStomp), maxLevel);
            case 9:
                return Mathf.Min(baseLevel + BadgeEquippedCount(Badge.BadgeType.ElementalStomp), maxLevel);
            case 10:
                return Mathf.Min(baseLevel + BadgeEquippedCount(Badge.BadgeType.TeamThrow), maxLevel);
            case 11:
                return Mathf.Min(baseLevel + BadgeEquippedCount(Badge.BadgeType.StrangeEgg), maxLevel);
            case 12:
                return Mathf.Min(baseLevel, maxLevel);
            case 13:
                return Mathf.Min(baseLevel + BadgeEquippedCount(Badge.BadgeType.PowerSmash), maxLevel);
            case 14:
                return Mathf.Min(baseLevel + BadgeEquippedCount(Badge.BadgeType.DazzleSmash), maxLevel);
            case 15:
                return Mathf.Min(baseLevel + BadgeEquippedCount(Badge.BadgeType.HammerThrow), maxLevel);
            case 16:
                return Mathf.Min(baseLevel + BadgeEquippedCount(Badge.BadgeType.BreakerSmash), maxLevel);
            case 17:
                return Mathf.Min(baseLevel + BadgeEquippedCount(Badge.BadgeType.FlameSmash), maxLevel);
            case 18:
                return Mathf.Min(baseLevel + BadgeEquippedCount(Badge.BadgeType.MomentumSmash), maxLevel);
            case 19:
                return Mathf.Min(baseLevel + BadgeEquippedCount(Badge.BadgeType.QuakeSmash), maxLevel);
            case 20:
                return Mathf.Min(baseLevel + BadgeEquippedCount(Badge.BadgeType.LightSmash), maxLevel);
            case 21:
                int l = baseLevel + BadgeEquippedCount(Badge.BadgeType.Illuminate);
                if (l > 0)
                {
                    l++;
                }
                return Mathf.Min(l, maxLevel);
            case 22:
                return Mathf.Min(baseLevel + BadgeEquippedCount(Badge.BadgeType.HammerBeat), maxLevel);
            case 23:
                int l2 = baseLevel + BadgeEquippedCount(Badge.BadgeType.MistWall);
                if (l2 > 0)
                {
                    l2++;
                }
                return Mathf.Min(l2, maxLevel);
        }
        return 1;
    }
    public int GetLunaMoveBaseLevel(int index)
    {
        int jlevel = BattleControl.Instance.playerData.GetPlayerDataEntry(entityID).jumpLevel;
        int wlevel = BattleControl.Instance.playerData.GetPlayerDataEntry(entityID).weaponLevel;
        int level = BattleControl.Instance.playerData.level;

        //To do later: flag check
        switch (index)
        {
            case 0:
                return 1;   //heavy stomp
            case 1:
                return 1;   //brace
            case 2:
                return jlevel >= 1 ? 1 : 0;   //dash through
            case 3:
                return 0;   //flip kick
            case 4:
                return 0;   //fluff heal
            case 5:
                return 0;   //sleep stomp
            case 6:
                return level >= 12 ? 1 : 0;   //meteor stomp
            case 7:
                return jlevel >= 2 ? 1 : 0;   //under strike
            case 8:
                return 0;   //iron stomp
            case 9:
                return 0;   //elemental stomp
            case 10:
                return level >= 21 ? 1 : 0;   //team throw
            case 11:
                return 0;   //double egg
            case 12:
                return wlevel >= 0 ? 1 : 0;   //smash
            case 13:
                return 1;   //power smash
            case 14:
                return 0;   //dazzle smash
            case 15:
                return level >= 6 ? 1 : 0;   //hammer throw
            case 16:
                return wlevel >= 1 ? 1 : 0;   //breaker smash
            case 17:
                return 0;   //flame smash
            case 18:
                return 0;   //momentum smash
            case 19:
                return 0;   //quake smash
            case 20:
                return wlevel >= 2 ? 1 : 0;   //light smash
            case 21:
                return wlevel >= 2 ? 1 : 0;   //illuminate (note: has special coding that makes it go 0, 2...)
            case 22:
                return 0;   //hammer beat
            case 23:
                return 0;   //mist wall
        }
        return 1;
    }
    public int GetLunaMoveTrueMaxLevel(int index)
    {
        return 20;

        /*
        //Note: these moves aren't set up properly to have higher levels than these
        //(Most of them do nothing)
        switch (index)
        {
            case 0:
                return 1;
            case 1:
                return 3;
            case 2:
                return 2;
            case 3:
                return 1;
            case 4:
                return 2;
            case 5:
                return 1;
            case 6:
                return 2;
            case 7:
                return 2;
            case 8:
                return 1;
            case 9:
                return 1;
            case 10:
                return 2;
            case 11:
                return 1;
            case 12:
                return 1;
            case 13:
                return 3;
            case 14:
                return 1;
            case 15:
                return 2;
            case 16:
                return 2;
            case 17:
                return 1;
            case 18:
                return 1;
            case 19:
                return 1;
            case 20:
                return 2;
            case 21:
                return 3;
            case 22:
                return 1;
            case 23:
                return 3;
        }
        return 1;
        */
    }

    public int GetSoulMoveMaxLevel(int index)
    {
        if (MainManager.Instance.Cheat_LevelAnarchy)
        {
            return 20;
        }

        //To do later: flag check
        switch (index)
        {
            case 0:
                return 1;
            case 1:
                return 1;
            case 2:
                return 1;
            case 3:
                return 1;
            case 4:
                return 1;
            case 5:
                return 1;
            case 6:
                return 1;
            case 7:
                return 1;
            case 8:
                return 1;
            case 9:
                return 1;
            case 10:
                return 1;
            case 11:
                return 1;
        }
        return 1;
    }

    public override IEnumerator FirstStrike(BattleStartArguments.FirstStrikeMove move = BattleStartArguments.FirstStrikeMove.Default)
    {
        PlayerMove bmove = null; // jumpMoves[0];

        //this is usually good (weapon has higher damage than stomp usually, no contact hazards)
        //weaker than double jump / super jump first strikes but don't really want to give you those for free?
        if (move == BattleStartArguments.FirstStrikeMove.Default)
        {
            move = BattleStartArguments.FirstStrikeMove.Weapon;
        }

        switch (move)   //note: will mess up if the wrong character first strikes in an invalid way (e.g. Luna with Double Jump), though the moves in those positions are similar ish
        {
            case BattleStartArguments.FirstStrikeMove.Stomp:
                switch (entityID)
                {
                    case EntityID.Wilex:
                        bmove = GetOrAddComponent<WM_HighStomp>();
                        break;
                    case EntityID.Luna:
                        bmove = GetOrAddComponent<LM_HeavyStomp>();
                        break;
                }
                break;
            case BattleStartArguments.FirstStrikeMove.Weapon:
                switch (entityID)
                {
                    case EntityID.Wilex:
                        bmove = GetOrAddComponent<WM_Slash>();
                        break;
                    case EntityID.Luna:
                        bmove = GetOrAddComponent<LM_Smash>();
                        break;
                }
                break;
            case BattleStartArguments.FirstStrikeMove.DoubleJump:
                switch (entityID)
                {
                    case EntityID.Wilex:
                        bmove = GetOrAddComponent<WM_ElectroStomp>();
                        break;
                    case EntityID.Luna:
                        bmove = GetOrAddComponent<LM_FlipKick>();
                        break;
                }
                break;
            case BattleStartArguments.FirstStrikeMove.SuperJump:
                switch (entityID)
                {
                    case EntityID.Wilex:
                        bmove = GetOrAddComponent<WM_FlameStomp>();
                        break;
                    case EntityID.Luna:
                        bmove = GetOrAddComponent<LM_MeteorStomp>();
                        break;
                }
                break;
            case BattleStartArguments.FirstStrikeMove.DashHop:
                switch (entityID)
                {
                    case EntityID.Wilex:
                        bmove = GetOrAddComponent<WM_MultiStomp>();
                        break;
                    case EntityID.Luna:
                        bmove = GetOrAddComponent<LM_DashThrough>();
                        break;
                }
                break;
            case BattleStartArguments.FirstStrikeMove.Dig:
                switch (entityID)
                {
                    case EntityID.Wilex:
                        bmove = GetOrAddComponent<WM_DoubleStomp>();
                        break;
                    case EntityID.Luna:
                        bmove = GetOrAddComponent<LM_UnderStrike>();
                        break;
                }
                break;
        }

        //get first target
        //enforce first enemy targetting
        List<BattleEntity> elist = BattleControl.Instance.GetEntitiesSorted(this, new TargetArea(TargetArea.TargetAreaType.LiveEnemy)); //BattleControl.Instance.GetEntitiesSorted(this, bmove, 1);

        if (elist.Count > 0)
        {
            curTarget = elist[0];
        }
        else
        {
            curTarget = null;
        }

        //Sus targetting
        bool specialFly = false;
        if (curTarget != null && curTarget.GetEntityProperty(BattleHelper.EntityProperties.Airborne))
        {
            yield return StartCoroutine(curTarget.Move(curTarget.transform.position - Vector3.up * curTarget.transform.position.y + Vector3.up * 0.1f, 16));
            specialFly = true;
        }

        yield return StartCoroutine(bmove.Execute(this, 1));

        //Fix entity position to home
        //Note that falling down will change home pos anyway
        if (specialFly && BattleControl.Instance.EntityValid(curTarget))
        {
            yield return new WaitUntil(() => (!curTarget.inEvent));
            if (BattleControl.Instance.EntityValid(curTarget))
            {
                yield return StartCoroutine(curTarget.Move(curTarget.homePos));
            }
        }
    }

    public override void ChooseMove()
    {
        //reset the list
        while (contactImmunityList.Count > 0)
        {
            contactImmunityList.RemoveAt(0);
        }

        //action counter is incremented differently for player characters (since this choosemove function is not really used for players)
        //actionCounter++;

        //vestige of old system where chooseMove happened before you chose your moves
        //currMove = null;
        //extraMoves = null;
        //target = null;

        if (GetEntityProperty(EntityProperties.NoCount))
        {
            currMove = null;
            curTarget = null;
        }
    }

    //Blocking will reduce the turn count of status effects by half (3, 2 -> 1, 1 -> 0)
    //Super blocking will negate them completely
    //note: use ReceiveStatusForce to force statuses without restrictions (but this is not great design)
    public override void ReceiveEffect(Effect se, int casterID = int.MinValue, Effect.EffectStackMode mode = Effect.EffectStackMode.Default) // EffectPopupPriority priority = EffectPopupPriority.Never)
    {
        //Debug.Log("Effect tracking " + se);
        //No turncount restriction
        //...but turn count is halved if blocking, rounded down
        //Negated if super blocking

        bool bol = MainManager.Instance.GetGlobalFlag(MainManager.GlobalFlag.GF_FileCode_Lust);

        int statusTurnReduction = 0;

        float statusMultiplier = 1;
        if ((blockFrames == -2 || sblockFrames == -2) && (IsBlocking() || GetSafetyBlock()))
        {
            if (bol)
            {
                statusTurnReduction = 0;
                //statusMultiplier = 0.75f;
            }
            else
            {
                statusTurnReduction = 1;
                //statusMultiplier = 0.5f;
            }
        }
        if ((blockFrames == -2 || sblockFrames == -2) && (GetClearBlock() || IsSuperBlocking()))
        {
            if (bol)
            {
                statusTurnReduction = 1;
                //statusMultiplier = 0.5f;
            }
            else
            {
                statusTurnReduction = 100;
                //statusMultiplier = 0;
            }
        }
        if (bol)
        {
            statusMultiplier -= 0.25f * BadgeEquippedCount(Badge.BadgeType.StatusResist);
        }
        else
        {
            statusMultiplier -= 0.5f * BadgeEquippedCount(Badge.BadgeType.StatusResist);
        }
        if (statusMultiplier < 0)
        {
            statusMultiplier = 0;
        }

        if (Effect.GetEffectClass(se.effect) == Effect.EffectClass.Status || Effect.IsBlockableDebuff(se.effect))
        {
            if (se.duration != Effect.INFINITE_DURATION)
            {
                int olddur = se.duration;
                se.duration = (sbyte)(Mathf.CeilToInt(se.duration * statusMultiplier));
                if (statusMultiplier < 1 && olddur == se.duration && olddur > 0)
                {
                    se.duration--;
                }
                se.duration -= (sbyte)statusTurnReduction;
                if (statusTurnReduction > 0 && se.duration > olddur)
                {
                    //underflow
                    se.duration = 0;
                }
            } else
            {
                int oldpot = se.potency;
                se.potency = (sbyte)(Mathf.CeilToInt(se.potency * statusMultiplier));
                if (statusMultiplier < 1 && oldpot == se.potency && oldpot > 0)
                {
                    se.potency--;
                }
                se.potency -= (sbyte)statusTurnReduction;
                if (statusTurnReduction > 0 && se.potency > oldpot)
                {
                    //underflow
                    se.potency = 0;
                }
            }
        }

        if (se.potency > 0 && se.duration > 0)
        {
            ReceiveEffectForce(se, casterID, mode);
        }
    }
    public override void ReceiveEffectBuffered(Effect se, int casterID = int.MinValue, Effect.EffectStackMode mode = Effect.EffectStackMode.Default) // EffectPopupPriority priority = EffectPopupPriority.Never)
    {
        //No turncount restriction
        //...but turn count is halved if blocking, rounded down
        //Negated if super blocking

        bool bol = MainManager.Instance.GetGlobalFlag(MainManager.GlobalFlag.GF_FileCode_Lust);

        int statusTurnReduction = 0;

        float statusMultiplier = 1;
        if ((blockFrames == -2 || sblockFrames == -2) && (IsBlocking() || GetSafetyBlock()))
        {
            if (bol)
            {
                statusTurnReduction = 0;
                //statusMultiplier = 0.75f;
            }
            else
            {
                statusTurnReduction = 1;
                //statusMultiplier = 0.5f;
            }
        }
        if ((blockFrames == -2 || sblockFrames == -2) && (GetClearBlock() || IsSuperBlocking()))
        {
            if (bol)
            {
                statusTurnReduction = 1;
                //statusMultiplier = 0.5f;
            }
            else
            {
                statusTurnReduction = 100;
                //statusMultiplier = 0;
            }
        }
        if (bol)
        {
            statusMultiplier -= 0.25f * BadgeEquippedCount(Badge.BadgeType.StatusResist);
        }
        else
        {
            statusMultiplier -= 0.5f * BadgeEquippedCount(Badge.BadgeType.StatusResist);
        }
        if (statusMultiplier < 0)
        {
            statusMultiplier = 0;
        }

        if (Effect.GetEffectClass(se.effect) == Effect.EffectClass.Status || Effect.IsBlockableDebuff(se.effect))
        {
            if (se.duration != Effect.INFINITE_DURATION)
            {
                int olddur = se.duration;
                se.duration = (sbyte)(Mathf.CeilToInt(se.duration * statusMultiplier));
                if (statusMultiplier < 1 && olddur == se.duration && olddur > 0)
                {
                    se.duration--;
                }
                se.duration -= (sbyte)statusTurnReduction;
                if (statusTurnReduction > 0 && se.duration > olddur)
                {
                    //underflow
                    se.duration = 0;
                }
            }
            else
            {
                int oldpot = se.potency;
                se.potency = (sbyte)(Mathf.CeilToInt(se.potency * statusMultiplier));
                if (statusMultiplier < 1 && oldpot == se.potency && oldpot > 0)
                {
                    se.potency--;
                }
                se.potency -= (sbyte)statusTurnReduction;
                if (statusTurnReduction > 0 && se.potency > oldpot)
                {
                    //underflow
                    se.potency = 0;
                }
            }
        }

        if (se.potency > 0 && se.duration > 0)
        {
            ReceiveEffectForceBuffered(se, casterID, mode);
        }
    }

    public override int ShowStateIcons()
    {
        int output = 0;

        //most of these are not really supposed to be on a player character but I'll keep them for completeness
        output += base.ShowStateIcons();

        //make enviro effect sprite
        BattleHelper.EnvironmentalEffect ee = BattleControl.Instance.enviroEffect;
        if (ee != EnvironmentalEffect.None)
        {
            BattleHelper.EntityState checkState;
            Enum.TryParse(ee.ToString(), out checkState);
            MakeStateIcon(output, checkState, int.MinValue, int.MinValue);
            output++;
        }

        //new stuff
        PlayerData pd = BattleControl.Instance.playerData;

        //note: no charm sight equivalent for these effects since it would be mostly useless
        foreach (InnEffect i in pd.innEffects)
        {
            switch (i.innType)
            {
                case InnEffect.InnType.Health:
                    MakeStateIcon(output, BattleHelper.EntityState.HealthRest, int.MinValue, i.charges);
                    output++;
                    break;
                case InnEffect.InnType.Energy:
                    MakeStateIcon(output, BattleHelper.EntityState.EnergyRest, int.MinValue, i.charges);
                    output++;
                    break;
                case InnEffect.InnType.Absorb:
                    MakeStateIcon(output, BattleHelper.EntityState.AbsorbRest, int.MinValue, i.charges);
                    output++;
                    break;
                case InnEffect.InnType.Stamina:
                    MakeStateIcon(output, BattleHelper.EntityState.StaminaRest, int.MinValue, i.charges);
                    output++;
                    break;
                case InnEffect.InnType.Burst:
                    MakeStateIcon(output, BattleHelper.EntityState.BurstRest, int.MinValue, i.charges);
                    output++;
                    break;
                case InnEffect.InnType.Focus:
                    MakeStateIcon(output, BattleHelper.EntityState.FocusRest, int.MinValue, i.charges);
                    output++;
                    break;
                case InnEffect.InnType.Ethereal:
                    MakeStateIcon(output, BattleHelper.EntityState.EtherealRest, int.MinValue, i.charges);
                    output++;
                    break;
                case InnEffect.InnType.Immunity:
                    MakeStateIcon(output, BattleHelper.EntityState.ImmunityRest, int.MinValue, i.charges);
                    output++;
                    break;
                case InnEffect.InnType.BonusTurn:
                    MakeStateIcon(output, BattleHelper.EntityState.BonusTurnRest, int.MinValue, i.charges);
                    output++;
                    break;
                case InnEffect.InnType.ItemBoost:
                    MakeStateIcon(output, BattleHelper.EntityState.ItemBoostRest, int.MinValue, i.charges);
                    output++;
                    break;
                case InnEffect.InnType.Soul:
                    MakeStateIcon(output, BattleHelper.EntityState.SoulRest, int.MinValue, i.charges);
                    output++;
                    break;
                case InnEffect.InnType.Freebie:
                    MakeStateIcon(output, BattleHelper.EntityState.FreebieRest, int.MinValue, i.charges);
                    output++;
                    break;
            }
        }

        if (BadgeEquipped(Badge.BadgeType.CharmSight))   //badge check
        {
            foreach (CharmEffect c in pd.charmEffects)
            {
                switch (c.charmType)
                {
                    case CharmEffect.CharmType.Attack:
                        MakeStateIcon(output, BattleHelper.EntityState.CharmAttack, c.charges, c.duration);
                        output++;
                        break;
                    case CharmEffect.CharmType.Defense:
                        MakeStateIcon(output, BattleHelper.EntityState.CharmDefense, c.charges, c.duration);
                        output++;
                        break;
                    case CharmEffect.CharmType.Fortune:
                        MakeStateIcon(output, BattleHelper.EntityState.CharmFortune, c.charges, c.duration);
                        output++;
                        break;
                }
            }
        }

        return output;
    }

    public override ulong PreDealDamage(BattleEntity target, DamageType type, ulong properties = 0, ContactLevel contact = ContactLevel.Infinite)
    {
        if (BattleHelper.GetDamageProperty(properties, BattleHelper.DamageProperties.AC_Success))
        {
            actionCommandSuccesses++;
        }

        //note: I could make a clever system where it keeps a timestamp so that you can't get multiple successes in the same frame
        //but that won't work since some spread damage attacks are not simultaneous


        //note: the dealdamage function takes place in 1 frame so doing this first will still look right
        bool immune = target.GetDefense(type) == DefenseTableEntry.IMMUNITY_CONSTANT;
        bool absorb = target.GetDefense(type) > DefenseTableEntry.IMMUNITY_CONSTANT;

        if (absorb)
        {
            BattleControl.Instance.CreateActionCommandEffect(ActionCommandText.Absorb, target.GetDamageEffectPosition(), target);
        }
        else if (immune)
        {
            BattleControl.Instance.CreateActionCommandEffect(ActionCommandText.Immune, target.GetDamageEffectPosition(), target);
        }
        else if (BattleHelper.GetDamageProperty(properties, BattleHelper.DamageProperties.AC_Success) || BattleHelper.GetDamageProperty(properties, BattleHelper.DamageProperties.AC_SuccessStall))
        {
            BattleControl.Instance.CreateActionCommandEffect(actionCommandSuccesses, target.GetDamageEffectPosition(), target);
        }

        ulong outProperties = 0;

        if (!BattleHelper.GetDamageProperty(properties, BattleHelper.DamageProperties.Static))
        {
            if (BadgeEquipped(Badge.BadgeType.TenaciousStrikes))
            {
                outProperties |= (ulong)BattleHelper.DamageProperties.TrueMinOne;
            }

            if (BadgeEquipped(Badge.BadgeType.AilmentExploit))
            {
                outProperties |= (ulong)BattleHelper.DamageProperties.StatusExploit;
            }

            if (BadgeEquipped(Badge.BadgeType.SoftTouch))
            {
                outProperties |= (ulong)BattleHelper.DamageProperties.SoftTouch;
            }

            if (BadgeEquipped(Badge.BadgeType.NightmareStrike))
            {
                outProperties |= (ulong)BattleHelper.DamageProperties.NightmareStrike;
            }

            if (BadgeEquipped(Badge.BadgeType.Aggravate))
            {
                outProperties |= (ulong)BattleHelper.DamageProperties.Aggravate;
            }

            if (BadgeEquipped(Badge.BadgeType.Icebreaker))
            {
                outProperties |= (ulong)BattleHelper.DamageProperties.Icebreaker;
            }
        }

        if (target is PlayerEntity pcaller)
        {
            //Not a good place to put this but it is convenient
            if (!BattleHelper.GetDamageProperty(properties, BattleHelper.DamageProperties.Hardcode) && pcaller.BadgeEquipped(Badge.BadgeType.RevivalFlame))
            {
                pcaller.revivalFlameHPCheck = false;
                if (target.maxHP <= maxHP)  //player character is target here
                {
                    pcaller.revivalFlameHPCheck = true;
                }
            }
        }

        return outProperties;
    }

    public override void PostDealDamage(BattleEntity target, DamageType type, ulong properties = 0, ContactLevel contact = ContactLevel.Infinite, int damageTaken = 0)
    {
        hitsDealt++;
        damageDealt += damageTaken;

        BattleControl.Instance.playerData.GetPlayerDataEntry(entityID).UpdateMaxDamageDealtSingle(damageTaken);
        BattleControl.Instance.playerData.UpdateMaxDamageDealtSingle(damageTaken);

        if (damageTaken > 0)
        {
            perTurnDamageDealt += damageTaken;
            BattleControl.Instance.playerData.GetPlayerDataEntry(entityID).cumulativeDamageDealt += damageTaken;
            BattleControl.Instance.playerData.cumulativeDamageDealt += damageTaken;
        }

        if (!BattleHelper.GetDamageProperty(properties, DamageProperties.Item))
        {
            nonItemHitsDealt++;
        }
    }

    public override void TryContactHazard(BattleEntity target, ContactLevel contact, DamageType type, int damage)
    {
        EnvironmentalContactHazards(target, contact, type, damage);

        if (target.CanTriggerContactHazard(contact, type, damage))
        {
            bool slimyBlock = GetSlimyBlock();
            bool sharpBlock = GetSharpBlock();

            //Check for contact hazard immunity list
            //(prevents multihits on the same target from hurting multiple times)
            //(does not prevent multitarget moves from doing the same!)

            if (target.contactImmunityList.Contains(posId))
            {
                return;
            }

            //comes after the TakeDamage call so this will always only trigger when you die (no misfire)
            if (BadgeEquipped(Badge.BadgeType.LastCounter) && hp <= 0)
            {
                DealDamage(target, lastTrueDamageTaken * BadgeEquippedCount(Badge.BadgeType.LastCounter), DamageType.Normal, (ulong)DamageProperties.StandardContactHazard, ContactLevel.Contact);
                return;
            }

            if (contact <= ContactLevel.Contact)
            {
                if (slimyBlock)
                {
                    DealDamage(target, 3, DamageType.Dark, (ulong)DamageProperties.StandardContactHazard, ContactLevel.Contact);
                    target.contactImmunityList.Add(posId);
                    return;
                }

                if (sharpBlock)
                {
                    DealDamage(target, 2, DamageType.Normal, (ulong)DamageProperties.StandardContactHazard, ContactLevel.Contact);
                    target.contactImmunityList.Add(posId);
                    return;
                }

                if (HasEffect(Effect.EffectType.Paralyze) && BadgeEquipped(Badge.BadgeType.Inductor))
                {
                    DealDamage(target, 3 * BadgeEquippedCount(Badge.BadgeType.Inductor), DamageType.Air, (ulong)DamageProperties.StandardContactHazard, ContactLevel.Contact);
                    target.contactImmunityList.Add(posId);
                    return;
                }

                if (HasEffect(Effect.EffectType.Freeze) && BadgeEquipped(Badge.BadgeType.FrostEdge))
                {
                    DealDamage(target, 3 * BadgeEquippedCount(Badge.BadgeType.FrostEdge), DamageType.Light, (ulong)DamageProperties.StandardContactHazard, ContactLevel.Contact);
                    target.contactImmunityList.Add(posId);
                    return;
                }
            }
        }
    }

    public override bool GetEntityProperty(EntityProperties property, bool b = true)
    {
        if ((property & BattleHelper.EntityProperties.SoftTouch) != 0 && BadgeEquipped(Badge.BadgeType.SoftTouch))
        {
            return b;
        }

        if ((property & BattleHelper.EntityProperties.DeepSleep) != 0 && BadgeEquipped(Badge.BadgeType.DeepSleep))
        {
            return b;
        }

        if ((property & BattleHelper.EntityProperties.Glacier) != 0 && BadgeEquipped(Badge.BadgeType.Glacier))
        {
            return b;
        }

        return base.GetEntityProperty(property, b);
    }


    public bool GetSafetyBlock()
    {
        if (!RibbonEquipped(Ribbon.RibbonType.SafetyRibbon, true))
        {
            return false;
        }

        //hold
        return CanBlock() && InputManager.GetButtonHeldLonger(InputManager.Button.A, 0.2f);
    }
    public bool GetSharpBlock()
    {
        if (!RibbonEquipped(Ribbon.RibbonType.SharpRibbon, true))
        {
            return false;
        }

        //hold
        return CanBlock() && InputManager.GetButtonHeldLonger(InputManager.Button.B, 0.2f);
    }
    public bool GetSoftBlock()
    {
        //requires the A press
        if (!IsBlocking())
        {
            return false;
        }

        if (!RibbonEquipped(Ribbon.RibbonType.SoftRibbon, true))
        {
            return false;
        }

        //tap input
        if (InputManager.GetTimeSinceNeutralJoystick() >= BASE_GUARD_WINDOW)
        {
            return false;
        }

        //a Right input (mutually exclusive with other directions)
        if (InputManager.GetAxisHorizontal() > 0.25f && Mathf.Abs(InputManager.GetAxisVertical()) < 0.25f)
        {
            return true;
        }

        return false;
    }

    public bool GetAbsorbBlock()
    {
        //requires the A press
        if (!IsBlocking())
        {
            return false;
        }

        if (!RibbonEquipped(Ribbon.RibbonType.StaticRibbon, true))
        {
            return false;
        }

        //tap input
        if (InputManager.GetTimeSinceNeutralJoystick() >= BASE_GUARD_WINDOW)
        {
            return false;
        }

        //a Right input (mutually exclusive with other directions)
        if (InputManager.GetAxisHorizontal() < -0.25f && Mathf.Abs(InputManager.GetAxisVertical()) < 0.25f)
        {
            return true;
        }

        return false;
    }

    public bool GetSlimyBlock()
    {
        //requires the A press
        if (!IsBlocking())
        {
            return false;
        }

        if (!RibbonEquipped(Ribbon.RibbonType.SlimyRibbon, true))
        {
            return false;
        }

        //tap input
        if (InputManager.GetTimeSinceNeutralJoystick() >= BASE_GUARD_WINDOW)
        {
            return false;
        }

        //a Right input (mutually exclusive with other directions)
        if (InputManager.GetAxisVertical() > 0.25f && Mathf.Abs(InputManager.GetAxisHorizontal()) < 0.25f)
        {
            return true;
        }

        return false;
    }

    public bool GetClearBlock()
    {
        //requires the A press
        if (!IsBlocking())
        {
            return false;
        }

        if (!RibbonEquipped(Ribbon.RibbonType.SafetyRibbon, true))
        {
            return false;
        }

        //tap input
        if (InputManager.GetTimeSinceNeutralJoystick() >= BASE_GUARD_WINDOW)
        {
            return false;
        }

        //a Right input (mutually exclusive with other directions)
        if (InputManager.GetAxisVertical() < -0.25f && Mathf.Abs(InputManager.GetAxisHorizontal()) < 0.25f)
        {
            return true;
        }

        return false;
    }

    public bool GetDiamondBlock()
    {
        //requires the A press
        if (!IsBlocking())
        {
            return false;
        }

        if (!RibbonEquipped(Ribbon.RibbonType.DiamondRibbon, true))
        {
            return false;
        }

        //button press
        if (InputManager.GetButton(InputManager.Button.B) && !InputManager.GetButtonHeldLonger(InputManager.Button.B, BASE_GUARD_WINDOW))
        {
            return true;
        }

        return false;
    }

    /*
    public bool GetZBlock()
    {
        //requires the A press
        if (!IsBlocking())
        {
            return false;
        }

        //button press
        if (MainManager.GetButton(InputManager.Button.Z) && !MainManager.GetButtonHeldLonger(InputManager.Button.Z, BASE_GUARD_WINDOW))
        {
            return true;
        }

        return false;
    }
    public bool GetYBlock()
    {
        //requires the A press
        if (!IsBlocking())
        {
            return false;
        }

        //button press
        if (MainManager.GetButton(InputManager.Button.Y) && !MainManager.GetButtonHeldLonger(InputManager.Button.Y, BASE_GUARD_WINDOW))
        {
            return true;
        }

        return false;
    }
    */

    public override bool StatusWillWork(Effect.EffectType se, float boost = 1, int lostHP = 0)
    {
        //potential new change: hardcode something that prevents you from being status stunlocked?
        //but if I program all the statusing moves correctly you would never encounter this?
        return CanBlock();

        //you can always be statused
        //return true;
    }
    public override int StatusWorkingHP(Effect.EffectType se)
    {
        //you can always be statused
        return maxHP;
    }


    public override void ValidateEffects()
    {
        for (int i = 0; i < effects.Count; i++)
        {
            //Debug.Log(this + " " + effects[i]);
            //Enforce caps
            if (effects[i].effect == Effect.EffectType.Focus || effects[i].effect == Effect.EffectType.Defocus)
            {
                if (effects[i].potency > GetFocusCap() && !(!BattleControl.IsPlayerControlled(this, true) && effects[i].effect == Effect.EffectType.Focus))
                {
                    effects[i].potency = (sbyte)GetFocusCap();
                }
            }
            if (effects[i].effect == Effect.EffectType.Absorb || effects[i].effect == Effect.EffectType.Sunder)
            {
                if (effects[i].potency > GetAbsorbCap() && !(!BattleControl.IsPlayerControlled(this, true) && effects[i].effect == Effect.EffectType.Absorb))
                {
                    effects[i].potency = (sbyte)GetAbsorbCap();
                }

            }
            if (effects[i].effect == Effect.EffectType.Burst || effects[i].effect == Effect.EffectType.Enervate)
            {
                if (effects[i].potency > GetBurstCap() && !(!BattleControl.IsPlayerControlled(this, true) && effects[i].effect == Effect.EffectType.Burst))
                {
                    effects[i].potency = (sbyte)GetBurstCap();
                }
            }

            if (effects.Count > 0 && effects[i].potency <= 0) //potency 0 is not allowed
            {
                effects.Remove(effects[i]);
                if (i > 0)
                {
                    i--;
                }
                QueueEvent(BattleHelper.Event.CureStatus);
            }
            if (effects.Count > 0 && effects[i].duration <= 0)
            {
                effects.Remove(effects[i]);
                if (i > 0)
                {
                    i--;
                }
                //do status curing code
                QueueEvent(BattleHelper.Event.CureStatus);
            }
        }

        effects.Sort((a, b) => ((int)a.effect - (int)b.effect));

        //recalculate maxhp, maxep, maxse
        maxHP = BattleControl.Instance.playerData.GetMaxHP(entityID) + GetEffectPotency(Effect.EffectType.MaxHPBoost) - GetEffectPotency(Effect.EffectType.MaxHPReduction);
        BattleControl.Instance.maxEP = BattleControl.Instance.playerData.GetMaxEP() + GetPartyMaxEffectPotency(Effect.EffectType.MaxEPBoost) - GetPartyMaxEffectPotency(Effect.EffectType.MaxEPReduction);
        BattleControl.Instance.maxSE = BattleControl.Instance.playerData.GetMaxSE() + GetPartyMaxEffectPotency(Effect.EffectType.MaxSEBoost) - GetPartyMaxEffectPotency(Effect.EffectType.MaxSEReduction);

        if (MainManager.Instance.GetGlobalFlag(GlobalFlag.GF_FileCode_Greed))
        {
            if (maxHP > DangerHP())
            {
                maxHP = DangerHP();
            }
        }

        if (maxHP < 0)
        {
            //Lmao
            //I could floor this at 1 but nah
            maxHP = 0;
        }

        if (BattleControl.Instance.maxEP < 0)
        {
            BattleControl.Instance.maxEP = 0;
        }
        if (BattleControl.Instance.maxSE < 0)
        {
            BattleControl.Instance.maxSE = 0;
        }

        //then cap normal numbers
        if (hp > maxHP)
        {
            hp = maxHP;
        }
        if (BattleControl.Instance.ep > BattleControl.Instance.maxEP)
        {
            BattleControl.Instance.ep = BattleControl.Instance.maxEP;
        }
        if (BattleControl.Instance.se > BattleControl.Instance.maxSE)
        {
            BattleControl.Instance.se = BattleControl.Instance.maxSE;
        }

        EffectSpriteUpdate();
    }

    public int GetStompDamage()
    {
        return PlayerData.PlayerDataEntry.GetStompDamage(entityID, BattleControl.Instance.playerData.GetPlayerDataEntry(entityID).jumpLevel);
        //return PlayerData.PlayerDataEntry.GetStompDamage(entityID, 2);
    }
    public int GetWeaponDamage()
    {
        return PlayerData.PlayerDataEntry.GetWeaponDamage(entityID, GetWeaponLevel());
        //return PlayerData.PlayerDataEntry.GetWeaponDamage(entityID, 2);
    }

    public int GetWeaponLevel()
    {
        return BattleControl.Instance.playerData.GetPlayerDataEntry(entityID).weaponLevel;
    }

    public override void TakeDamageStatus(int damage, bool hide = false) //strict damage (used by more internal things, does not increment damage event counter, does status hurt events)
    {
        if (GetEntityProperty(EntityProperties.Invulnerable))
        {
            return;
        }

        hp -= (damage);
        if (!hide)
        {
            BattleControl.Instance.CreateDamageEffect(DamageEffect.Damage, damage, null, null, GetDamageEffectPosition(), this, DamageType.Normal);
        }

        //ShowHPBar();
        QueueEvent(BattleHelper.Event.StatusHurt);
        if (hp <= 0)
        {
            hp = 0;
            QueueEvent(BattleHelper.Event.StatusDeath);
        }
    }

    //Block damage reduction is calculated here (*this is after the other damage calculation in DealDamage)
    //To do: change how damage calculation works?
    public override int TakeDamage(int damage, DamageType type, ulong properties) //default type damage
    {
        lastDamageTakenInput = damage;
        string localBonusDamageString = bonusDamageString;
        bonusDamageString = null;

        if (Invulnerable())
        {
            //note that this function is not actually the one that queues the hurt events
            //so there may be some possibilities of desync?
            damageEventsThisTurn++;
            absorbDamageEvents++;
            //Debug.Log(name + " " + damageEventsCount);
            hitThisTurn = true;
            //How about I just make it dink
            BattleControl.Instance.CreateDamageEffect(BattleHelper.DamageEffect.Damage, 0, localBonusDamageString, "(-" + damage + ") (100%)", GetDamageEffectPosition(), this, type, properties);
            MainManager.Instance.PlaySound(gameObject, MainManager.Sound.SFX_Hit_Immune);
            return 0;
        }


        damageEventsThisTurn++;
        absorbDamageEvents++;
        hitThisTurn = true;

        //Grab the other player
        PlayerEntity other = null;
        List<PlayerEntity> players = BattleControl.Instance.GetPlayerEntities();
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i] != this)
            {
                other = players[i];
                break;
            }
        }


        int damageReduction = 0;
        string damageReductionString = null;

        //if you are dead then you can't block
        bool sb = IsSuperBlocking() && !AutoMove();
        bool b = IsBlocking() || ((AutoMove() || (HasEffect(Effect.EffectType.Sleep) && BadgeEquipped(Badge.BadgeType.LightSleep))) && hp > 0 && !RibbonEquipped(Ribbon.RibbonType.ThornyRibbon, false));
        bool safetyBlock = GetSafetyBlock() && BattleControl.Instance.playerData.ep >= 2;
        bool sharpBlock = GetSharpBlock();
        if (!BattleHelper.GetDamageProperty(properties, DamageProperties.Hardcode))
        {
            bool noSpecial = false;

            if (GetDamageProperty(properties, DamageProperties.Unblockable) || GetDamageProperty(properties, DamageProperties.SAC_BlockFail))
            {
                b = false;
                sb = false;
                safetyBlock = false;
                sharpBlock = false;
                noSpecial = true;
            }

            if (GetDamageProperty(properties, DamageProperties.SAC_BlockSuccess))
            {
                b = true;
                sb = false;
                safetyBlock = false;
                sharpBlock = false;
                noSpecial = true;
            }

            bool noReduction = false;
            if (!noSpecial && (GetAbsorbBlock() || GetDiamondBlock() || GetSharpBlock()))
            {
                noReduction = true;
            }

            //Reset for all player characters to make this work
            //Note: this is just iterates through all player characters
            PlayerTurnController.Instance.ResetGuardTimers();

            if (GetDamageProperty(properties, DamageProperties.Block_Negate))
            {
                if (sb || b)    //note: safety block is NOT here
                {
                    damage = 0;
                }
            }

            if (GetDamageProperty(properties, DamageProperties.Block_Resist))
            {
                if (sb || b)    //note: safety block is NOT here
                {
                    damage = Mathf.CeilToInt(damage / 2f);
                }
            }

            //Debug.Log(sblockFrames + " " + blockFrames);
            if (sb)
            {
                //Debug.Log("Great!");
                damage -= 4; //BASE_GUARD_AMOUNT;
                blockSuccesses++;
                BattleControl.Instance.CreateActionCommandEffect(BattleHelper.ActionCommandText.Perfect, GetDamageEffectPosition(), this);
                MainManager.Instance.PlaySound(gameObject, MainManager.Sound.SFX_Hit_Block);
                MainManager.Instance.PlaySound(gameObject, MainManager.Sound.SFX_Hit_BlockPerfect);
            }
            else if (b || safetyBlock || sharpBlock)
            {
                //Debug.Log("Nice!");
                if (GetSoftBlock())
                {
                    damage -= 1;
                }
                if (RibbonEquipped(Ribbon.RibbonType.ExpertRibbon, false))
                {
                    damage -= 2;
                }
                if (!noReduction)
                {
                    damage -= 1; //BASE_GUARD_AMOUNT;
                }
                if (sharpBlock)
                {
                    damage += 1;
                }
                blockSuccesses++;
                BattleControl.Instance.CreateActionCommandEffect(blockSuccesses, GetDamageEffectPosition(), this);

                if (!noReduction)
                {
                    MainManager.Instance.PlaySound(gameObject, MainManager.Sound.SFX_Hit_Block);
                }
            }

            if (safetyBlock)
            {
                HealEnergy(-2);
            }

            //Particles
            if (safetyBlock)    //note: does not have a rest effect so there is no corresponding effect call for SafetyRibbon in the rest script
            {
                RibbonEffect(new Color(0f, 0.7f, 0f));
                MainManager.Instance.PlaySound(gameObject, MainManager.Sound.SFX_Hit_BlockSpecial);
            }
            if (sharpBlock)     //likewise
            {
                RibbonEffect(new Color(0.7f, 0.0f, 0f));
                MainManager.Instance.PlaySound(gameObject, MainManager.Sound.SFX_Hit_BlockSpecial);
            }
            if (sb)
            {
                RibbonEffect(new Color(1f, 0.7f, 0));
                MainManager.Instance.PlaySound(gameObject, MainManager.Sound.SFX_Hit_BlockSpecial);
            }
            if (GetAbsorbBlock())
            {
                RibbonEffect(new Color(1f, 1f, 0.3f));
                MainManager.Instance.PlaySound(gameObject, MainManager.Sound.SFX_Hit_BlockSpecial);
            }
            if (GetSlimyBlock())
            {
                RibbonEffectDark(new Color(1f, 0.3f, 1f));
                MainManager.Instance.PlaySound(gameObject, MainManager.Sound.SFX_Hit_BlockSpecial);
            }
            if (GetClearBlock())
            {
                RibbonEffect(new Color(0.5f, 1f, 0.5f));
                MainManager.Instance.PlaySound(gameObject, MainManager.Sound.SFX_Hit_BlockSpecial);
            }
            if (GetSoftBlock())
            {
                RibbonEffect(new Color(0.3f, 0.3f, 1f));
                MainManager.Instance.PlaySound(gameObject, MainManager.Sound.SFX_Hit_BlockSpecial);
            }
            if (GetDiamondBlock())
            {
                RibbonEffectDiamond(new Color(0.6f, 1f, 1f));
                MainManager.Instance.PlaySound(gameObject, MainManager.Sound.SFX_Hit_BlockSpecial);
            }


            if (damage < 0)
            {
                damage = 0;
            }

            int bonusResistance = 0;

            if (BattleHelper.GetDamageProperty(properties, DamageProperties.ContactHazard) && HasEffect(Effect.EffectType.Berserk) && BadgeEquipped(Badge.BadgeType.RageShield))
            {
                bonusResistance += 2 * BadgeEquippedCount(Badge.BadgeType.RageShield);
            }

            if (BattleHelper.GetDamageProperty(properties, DamageProperties.ContactHazard) && BadgeEquipped(Badge.BadgeType.SoftTouch))
            {
                bonusResistance += 2 * BadgeEquippedCount(Badge.BadgeType.SoftTouch);
            }

            if (!sb && !b)
            {
                if (!BattleHelper.GetDamageProperty(properties, DamageProperties.ContactHazard) && MainManager.Instance.GetGlobalFlag(MainManager.GlobalFlag.GF_FileCode_Pride))
                {
                    bonusResistance -= 1;
                }
            }

            damageReduction = damage - MainManager.DamageReductionFormula(damage, GetResistance() + bonusResistance);
            damage -= damageReduction;

            if (damageReduction != 0)
            {
                if (damageReduction > 0)
                {
                    damageReductionString = "(-" + damageReduction + ")" + "(-" + MainManager.GetResistancePercent(GetResistance() + bonusResistance) + ")";
                }
                else
                {
                    damageReductionString = "(+" + (-damageReduction) + ") " + "(+" + MainManager.GetResistancePercent(GetResistance() + bonusResistance) + ")";
                }
            }

            int shieldCount = 0;

            //To do later: Particle effects for damage reduction
            //Payment for shields
            if (BadgeEquipped(Badge.BadgeType.EnergyShield) && BattleControl.Instance.GetEP(this) > 0)
            {
                int shields = 2 * BadgeEquippedCount(Badge.BadgeType.EnergyShield);
                int value = Mathf.CeilToInt((damageReduction * 2 * shields) / (GetResistance() + bonusResistance + 0.0f));
                HealEnergy(-value);
                shieldCount++;
            }
            if (BadgeEquipped(Badge.BadgeType.SpiritShield) && BattleControl.Instance.GetSE(this) > 0)
            {
                int shields = 2 * BadgeEquippedCount(Badge.BadgeType.SpiritShield);
                int value = Mathf.CeilToInt((damageReduction * 2 * shields) / (GetResistance() + bonusResistance + 0.0f));
                HealSoulEnergy(-value);
                shieldCount++;
            }
            if (BadgeEquipped(Badge.BadgeType.GoldenShield) && BattleControl.Instance.GetCoins(this) > 0)
            {
                int shields = 2 * BadgeEquippedCount(Badge.BadgeType.GoldenShield);
                int value = Mathf.CeilToInt((damageReduction * 10 * shields) / (GetResistance() + bonusResistance + 0.0f));
                HealCoins(-value);
                //BattleControl.Instance.AddCoins(this, -value);
                shieldCount++;
            }

            if (HasEffect(Effect.EffectType.Poison) && BadgeEquipped(Badge.BadgeType.ToxicShield))
            {
                InflictEffect(this, new Effect(Effect.EffectType.Defocus, (sbyte)(1 * BadgeEquippedCount(Badge.BadgeType.ToxicShield)), Effect.INFINITE_DURATION));
            }

            //Supercooled
            if (BadgeEquipped(Badge.BadgeType.Supercooled))
            {
                if (!HasEffect(Effect.EffectType.Freeze))
                {
                    InflictEffectBuffered(this, new Effect(Effect.EffectType.Freeze, (sbyte)(BadgeEquippedCount(Badge.BadgeType.Supercooled)), 2));
                }
            }

            if (other != null && other.BadgeEquipped(Badge.BadgeType.ProtectiveRush))
            {                
                if (!other.protectiveRushPerTurn)
                {
                    other.protectiveRushPerTurn = true;
                    InflictEffect(other, new Effect(Effect.EffectType.Focus, (sbyte)(other.BadgeEquippedCount(Badge.BadgeType.ProtectiveRush)), Effect.INFINITE_DURATION));
                }
            }


            //Apply special properties
            damage = ApplyDefensiveProperties(damage, properties);

            //Quantum Shield
            if (TokenRemoveOne(Effect.EffectType.QuantumShield))
            {
                damage = 0;
            }

            if (!noSpecial && GetAbsorbBlock())
            {
                HealEnergy(damage / 2);
            }

            if (!noSpecial && GetDiamondBlock())
            {
                HealCoins(damage);
                //BattleControl.Instance.AddCoins(this, damage);
            }
        }        

        //Apply Astral Wall
        if (HasEffect(Effect.EffectType.CounterFlare))
        {
            counterFlareTrackedDamage += damage;
        }
        if (HasEffect(Effect.EffectType.ArcDischarge))
        {
            arcDischargeDamage += damage;
        }
        if (HasEffect(Effect.EffectType.Splotch))
        {
            splotchDamage += damage;
        }

        if (!BattleHelper.GetDamageProperty(properties, DamageProperties.Hardcode) && HasEffect(Effect.EffectType.AstralWall))
        {
            astralWallTrackedDamage += damage;
            if (astralWallTrackedDamage > GetEffectEntry(Effect.EffectType.AstralWall).potency)
            {
                int diff = damageTakenThisTurn - GetEffectEntry(Effect.EffectType.AstralWall).potency;

                damageTakenThisTurn -= diff;
                astralWallTrackedDamage -= diff;
                damage -= diff;

                if (damage < 0)
                {
                    damage = 0;
                }
            }
        }

        //Anti-death statuses
        if (!BattleHelper.GetDamageProperty(properties, DamageProperties.Hardcode) && HasEffect(Effect.EffectType.Miracle))
        {
            if (damage >= hp)
            {
                BattleControl.Instance.CreateEffectParticles(GetEffectEntry(Effect.EffectType.Miracle), this);
                TokenRemoveOne(Effect.EffectType.Miracle);
                SetEntityProperty(BattleHelper.EntityProperties.NoMiracle, true);
                //damage = 0;
                damage = hp - 1;
            }
        }

        //other death negates
        if (!BattleHelper.GetDamageProperty(properties, DamageProperties.Hardcode) && (BadgeEquippedCount(Badge.BadgeType.LastChance) > lastChance))
        {
            if (damage >= hp)
            {
                damage = 0;
                lastChance++;
            }
        }

        if (!BattleHelper.GetDamageProperty(properties, DamageProperties.Hardcode) && HasEffect(Effect.EffectType.Berserk) && (BadgeEquippedCount(Badge.BadgeType.UndyingRage) > undyingRage))
        {
            if (damage >= hp)
            {
                damage = 0;
                undyingRage++;
            }
        }

        //death swap
        if (!BattleHelper.GetDamageProperty(properties, DamageProperties.Hardcode) && BadgeEquipped(Badge.BadgeType.DeathSwap))
        {
            if (other.hp > 0 && damage >= hp)
            {
                damage = hp - 1;
                other.TakeDamage(other.hp, DamageType.Normal, (ulong)BattleHelper.DamageProperties.Hardcode);
            }
        }


        if (BattleHelper.GetDamageProperty(properties, BattleHelper.DamageProperties.TrueMinOne))
        {
            if (damage < 1)
            {
                damage = 1;
            }
        }

        int preHP = hp;

        //Post damage calculation

        if (HasEffect(Effect.EffectType.Soulbleed))
        {
            sbyte bleedDamage = (sbyte)(damage / 8);
            if (damage > 0 && bleedDamage <= 0)
            {
                bleedDamage = 1;
            }
            if (bleedDamage > 0)
            {
                ReceiveEffectForce(new Effect(Effect.EffectType.DamageOverTime, bleedDamage, 3), posId, Effect.EffectStackMode.KeepDurAddPot);
            }
        }

        if (HasEffect(Effect.EffectType.Soften))
        {
            sbyte softDamage = (sbyte)(damage / (sbyte)(GetEffectEntry(Effect.EffectType.Soften).potency + 1));
            if (softDamage > 0)
            {
                ReceiveEffectForce(new Effect(Effect.EffectType.DamageOverTime, softDamage, (sbyte)(GetEffectEntry(Effect.EffectType.Soften).potency + 1)), posId, Effect.EffectStackMode.KeepDurAddPot);
            }
        }
        else
        {
            if (damage >= 14)
            {
                MainManager.Instance.AwardAchievement(MainManager.Achievement.ACH_Ouch);
            }

            hp -= damage;
        }



        //Sound effects
        bool normalDamage = true;

        //block sound has precedence
        if (b || sb)
        {
            normalDamage = false;
        }

        if (damage == 0 || Invulnerable())
        {
            normalDamage = false;
            MainManager.Instance.PlaySound(gameObject, MainManager.Sound.SFX_Hit_Immune);
        }
        if ((type & DamageType.Light) != 0)
        {
            normalDamage = false;
            MainManager.Instance.PlaySound(gameObject, MainManager.Sound.SFX_Hit_Light);
        }
        if ((type & DamageType.Dark) != 0)
        {
            normalDamage = false;
            MainManager.Instance.PlaySound(gameObject, MainManager.Sound.SFX_Hit_Dark);
        }
        if ((type & DamageType.Fire) != 0)
        {
            normalDamage = false;
            MainManager.Instance.PlaySound(gameObject, MainManager.Sound.SFX_Hit_Fire);
        }
        if ((type & DamageType.Water) != 0)
        {
            normalDamage = false;
            MainManager.Instance.PlaySound(gameObject, MainManager.Sound.SFX_Hit_Water);
        }
        if ((type & DamageType.Air) != 0)
        {
            normalDamage = false;
            MainManager.Instance.PlaySound(gameObject, MainManager.Sound.SFX_Hit_Air);
        }
        if ((type & DamageType.Earth) != 0)
        {
            normalDamage = false;
            MainManager.Instance.PlaySound(gameObject, MainManager.Sound.SFX_Hit_Earth);
        }
        if ((type & DamageType.Prismatic) != 0)
        {
            normalDamage = false;
            MainManager.Instance.PlaySound(gameObject, MainManager.Sound.SFX_Hit_Prismatic);
        }
        if ((type & DamageType.Void) != 0)
        {
            normalDamage = false;
            MainManager.Instance.PlaySound(gameObject, MainManager.Sound.SFX_Hit_Void);
        }
        if (normalDamage)
        {
            MainManager.Instance.PlaySound(gameObject, MainManager.Sound.SFX_Hit_Normal);
        }




        //Risky rush
        if ((preHP > DangerHP() && hp <= DangerHP()) || (preHP > 1 && hp <= 1))
        {
            if (BadgeEquipped(Badge.BadgeType.RiskyRush))
            {
                InflictEffect(this, new Effect(Effect.EffectType.Focus, (sbyte)(3 * BadgeEquippedCount(Badge.BadgeType.RiskyRush)), Effect.INFINITE_DURATION));
            }
        }

        //Revival flame
        if (revivalFlameHPCheck && BadgeEquipped(Badge.BadgeType.RevivalFlame))
        {
            if (revivalFlame < BadgeEquippedCount(Badge.BadgeType.RevivalFlame) && hp <= 0)
            {
                hp = 0;
                HealHealth(maxHP);
                BattleControl.Instance.CreateReviveParticles(this, 4);
                InflictEffect(this, new Effect(Effect.EffectType.Focus, (sbyte)(6 * BadgeEquippedCount(Badge.BadgeType.RevivalFlame)), Effect.INFINITE_DURATION));
                revivalFlame++;
            }
        }
        revivalFlameHPCheck = false;


        if (GetDamageProperty(properties, DamageProperties.RemoveMaxHP))
        {
            //for clarity and implementation reasons, this is set up with MaxHPReduction

            int d = damage;
            if (d > Effect.INFINITE_DURATION)
            {
                d = Effect.INFINITE_DURATION;
            }

            //hacky
            SetEntityProperty(EntityProperties.GetEffectsAtNoHP);
            ReceiveEffectForce(new Effect(Effect.EffectType.MaxHPReduction, (sbyte)d, Effect.INFINITE_DURATION), Effect.NULL_CASTERID, Effect.EffectStackMode.AdditivePot);
            SetEntityProperty(EntityProperties.GetEffectsAtNoHP, false);
            //maxHP -= damage;

            //if (maxHP < 0)
            //{
            //    maxHP = 0;
            //}
        }

        if (GetDamageProperty(properties, DamageProperties.RemoveMaxHP))
        {
            BattleControl.Instance.CreateDamageEffect(DamageEffect.MaxHPDamage, damage, localBonusDamageString, damageReductionString, GetDamageEffectPosition(), this, type, properties);
        } else
        {
            if (HasEffect(Effect.EffectType.Soften))
            {
                BattleControl.Instance.CreateDamageEffect(DamageEffect.SoftDamage, damage, localBonusDamageString, damageReductionString, GetDamageEffectPosition(), this, type, properties);
            } else
            {
                if (GetDamageProperty(properties, DamageProperties.Unblockable))
                {
                    BattleControl.Instance.CreateDamageEffect(DamageEffect.UnblockableDamage, damage, localBonusDamageString, damageReductionString, GetDamageEffectPosition(), this, type, properties);
                }
                else
                {
                    if (sb)
                    {
                        lastHitWasBlocked = true;
                        BattleControl.Instance.CreateDamageEffect(DamageEffect.SuperBlockedDamage, damage, localBonusDamageString, damageReductionString, GetDamageEffectPosition(), this, type, properties);
                    }
                    else if (b || safetyBlock)
                    {
                        lastHitWasBlocked = true;
                        BattleControl.Instance.CreateDamageEffect(DamageEffect.BlockedDamage, damage, localBonusDamageString, damageReductionString, GetDamageEffectPosition(), this, type, properties);
                    }
                    else
                    {
                        lastHitWasBlocked = false;
                        BattleControl.Instance.CreateDamageEffect(DamageEffect.Damage, damage, localBonusDamageString, damageReductionString, GetDamageEffectPosition(), this, type, properties);
                    }
                }
            }
        }


        lastTrueDamageTaken = damage;
        if (hp < 0)
        {
            lastTrueDamageTaken = damage + hp;
            hp = 0;
        }

        lastDamageTaken = damage;
        lastDamageType = type;
        hitsTaken += 1;
        damageTaken += damage;
        perTurnDamageTaken += damage;

        BattleControl.Instance.playerData.GetPlayerDataEntry(entityID).cumulativeDamageTaken += damage;
        BattleControl.Instance.playerData.cumulativeDamageTaken += damage;

        return damage;
    }
    //note that TakeDamageCalculation does not account for blocking (intentional because it doesn't really make sense)
    public override int TakeDamageCalculation(int damage, DamageType type, ulong properties)
    {
        if (damage < 0)
        {
            return 0;
        }

        if (BattleHelper.GetDamageProperty(properties, BattleHelper.DamageProperties.Hardcode))
        {
            return damage;
        }

        if (Invulnerable())
        {
            return 0;
        }

        int bonusResistance = 0;

        if (BattleHelper.GetDamageProperty(properties, DamageProperties.ContactHazard) && HasEffect(Effect.EffectType.Berserk) && BadgeEquipped(Badge.BadgeType.RageShield))
        {
            bonusResistance += 2 * BadgeEquippedCount(Badge.BadgeType.RageShield);
        }

        if (BattleHelper.GetDamageProperty(properties, DamageProperties.ContactHazard) && BadgeEquipped(Badge.BadgeType.SoftTouch))
        {
            bonusResistance += 2 * BadgeEquippedCount(Badge.BadgeType.SoftTouch);
        }

        int damageReduction = damage - MainManager.DamageReductionFormula(damage, GetResistance() + bonusResistance);
        damage -= damageReduction;

        //Apply special properties
        damage = ApplyDefensiveProperties(damage, properties);

        //Anti-death statuses
        if (HasEffect(Effect.EffectType.Miracle))
        {
            if (damage >= hp)
            {
                damage = hp - 1;
            }
        }

        if (!BattleHelper.GetDamageProperty(properties, DamageProperties.Hardcode) && (BadgeEquippedCount(Badge.BadgeType.LastChance) > lastChance))
        {
            if (damage >= hp)
            {
                damage = hp - 1;
            }
        }

        if (!BattleHelper.GetDamageProperty(properties, DamageProperties.Hardcode) && HasEffect(Effect.EffectType.Berserk) && (BadgeEquippedCount(Badge.BadgeType.UndyingRage) > undyingRage))
        {
            if (damage >= hp)
            {
                damage = 0;
            }
        }


        if (BattleHelper.GetDamageProperty(properties, BattleHelper.DamageProperties.TrueMinOne))
        {
            if (damage < 1)
            {
                damage = 1;
            }
        }

        return damage;
    }


    public override void DeathCheck()
    {
        //probably unnecessary
        /*
        for (int i = 0; i < eventQueue.Count; i++)
        {
            switch (eventQueue[i])
            {
                case BattleHelper.Event.Death:
                case BattleHelper.Event.StatusDeath:
                    return;
            }
        }
        */

        //only call this when you are not in an event (otherwise whatever event you're in might be the death event or death event may be on queue)
        if (hp <= 0 && alive)
        {
            QueueEvent(BattleHelper.Event.Death);
        }

        //Inverse
        if (hp > 0 && (!alive || dead))
        {
            QueueEvent(BattleHelper.Event.Revive);
        }
    }


    //ribbon block particle effects (same as the Rest particles)
    public void RibbonEffect(Color color)
    {
        Vector3 position = transform.position + Vector3.up * (height / 2);
        GameObject eo;
        eo = Instantiate(Resources.Load<GameObject>("VFX/Battle/Ribbon/Effect_RibbonConfetti"), gameObject.transform);
        eo.transform.position = position;
        EffectScript_GenericColorRateOverTime es_b = eo.GetComponent<EffectScript_GenericColorRateOverTime>();
        es_b.Setup(color, 1, 1);
    }

    public void RibbonEffectDiamond(Color color)
    {
        Vector3 position = transform.position + Vector3.up * (height / 2);
        GameObject eo;
        eo = Instantiate(Resources.Load<GameObject>("VFX/Battle/Ribbon/Effect_RibbonConfettiDiamond"), gameObject.transform);
        eo.transform.position = position;
        EffectScript_GenericColorRateOverTime es_b = eo.GetComponent<EffectScript_GenericColorRateOverTime>();
        es_b.Setup(color, 1, 1);
    }

    public void RibbonEffectDark(Color color)
    {
        Vector3 position = transform.position + Vector3.up * (height / 2);
        GameObject eo;
        eo = Instantiate(Resources.Load<GameObject>("VFX/Battle/Ribbon/Effect_RibbonConfettiDark"), gameObject.transform);
        eo.transform.position = position;
        EffectScript_GenericColorRateOverTime es_b = eo.GetComponent<EffectScript_GenericColorRateOverTime>();
        es_b.Setup(color, 1, 1);
    }

    public bool IsSuperBlocking()
    {
        if (!CanBlock())
        {
            return false;
        }
        if (!RibbonEquipped(Ribbon.RibbonType.ChampionRibbon, true))
        {
            return false;
        }

        int w = BASE_SUPERGUARD_WINDOW;
        if (w < 1)
        {
            w = 1; //force block window to be at least 1 frame (otherwise move is unblockable)
        }
        if (w > 60)
        {
            return true; //at that point there is no way you can miss that input
        }
        return sblockFrames == -2 || (sblockFrames >= 0 && sblockFrames < w) && IsBlocking(); //not <= because that could lead to rare block window increases (though it is very unlikely that deltaTime will sum to a whole number)
    }
    public bool IsBlocking()
    {
        if (!CanBlock())
        {
            return false;
        }

        //thorny ribbon prevents blocking
        if (RibbonEquipped(Ribbon.RibbonType.ThornyRibbon, false))
        {
            return false;
        }

        bool widenWindow = false;
        bool narrowWindow = false;

        if (RibbonEquipped(Ribbon.RibbonType.BeginnerRibbon, false))
        {
            widenWindow = true;
        }

        if (RibbonEquipped(Ribbon.RibbonType.ExpertRibbon, false))
        {
            narrowWindow = true;
        }


        //the other statuses prevent blocking but not movement

        int w = BASE_GUARD_WINDOW;

        if (widenWindow)
        {
            w += BASE_GUARD_WINDOW / 2;
        }
        if (narrowWindow)
        {
            w -= BASE_GUARD_WINDOW / 2;
        }

        if (MainManager.Instance.GetGlobalFlag(MainManager.GlobalFlag.GF_FileCode_Pride))
        {
            w /= 2;
        }

        if (SettingsManager.Instance.GetSetting(SettingsManager.Setting.EasyActionCommands) != 0)
        {
            w = Mathf.CeilToInt(w * 1.5f);
        }

        if (w < 1)
        {
            w = 1; //force block window to be at least 1 frame (otherwise move is unblockable)
        }
        if (w > 60)
        {
            return true; //at that point there is no way you can miss that input
        }
        return blockFrames == -2 || (blockFrames >= 0 && blockFrames < w); //not <= because that could lead to rare block window increases (though it is very unlikely that deltaTime will sum to a whole number)
    }

    //Note: called on the full party (iterated)
    //-2 is a special value (is blocking, but next frame later it becomes -1)
    //-1 is not guarding
    public void ResetGuardTimers()
    {
        if (IsBlocking())
        {
            blockFrames = -2;
        }
        else
        {
            blockFrames = -1;
        }
        if (IsSuperBlocking())
        {
            sblockFrames = -2;
        }
        else
        {
            sblockFrames = -1;
        }
    }

    //Recycled for the "can't relay or be relayed to" check
    public bool CanBlock()
    {
        if (!CanMove())
        {
            return false;
        }

        //if you have non-poison status, can't block

        Effect.EffectType[] effectlist = new Effect.EffectType[] { Effect.EffectType.Poison, Effect.EffectType.Soulbleed, Effect.EffectType.Inverted, Effect.EffectType.Sunflame, Effect.EffectType.Brittle, Effect.EffectType.ArcDischarge, Effect.EffectType.Splotch };

        for (int i = 0; i < effects.Count; i++)
        {
            if (Effect.GetEffectClass(effects[i].effect) == Effect.EffectClass.Status)
            {
                bool noblock = true;
                for (int j = 0; j < effectlist.Length; j++)
                {
                    if (effects[i].effect == effectlist[j])
                    {
                        noblock = false;
                        break;
                    }
                }
                if (noblock)
                {
                    return false;
                }
            }
        }

        return true;
    }
    public override bool CanReact() //must be blocking to trigger counter
    {
        return IsBlocking() && currMove != null;
    }

    public void SetInactiveColor(bool set)
    {
        //Debug.Log(set);
        //Bad coding but I don't care
        SpriteRenderer sp = ac.GetComponentInChildren<SpriteRenderer>();
        if (set)
        {
            sp.color = new Color(0.5f, 0.5f, 0.5f, 1);
        } else
        {
            sp.color = new Color(1f, 1f, 1f, 1);
        }
    }

    public override void HealHealth(int health)
    {
        if (HasEffect(Effect.EffectType.Soulbleed) && health > 0)
        {
            health = 0;
        }

        //Debug.Log(hp + " " + health);
        bool atFull = (hp >= maxHP);
        base.HealHealth(health);

        if (BadgeEquipped(Badge.BadgeType.PerfectFocus) && !atFull && (hp >= maxHP))
        {
            InflictEffect(this, new Effect(Effect.EffectType.Focus, (sbyte)(3 * BadgeEquippedCount(Badge.BadgeType.PerfectFocus)), Effect.INFINITE_DURATION));
        }
    }
    public override int HealHealthTrackOverhealPay(int health)
    {
        if (HasEffect(Effect.EffectType.Soulbleed) && health > 0)
        {
            health = 0;
        }

        //Debug.Log(hp + " " + health);
        bool atFull = (hp >= maxHP);
        int output = base.HealHealthTrackOverhealPay(health);

        if (BadgeEquipped(Badge.BadgeType.PerfectFocus) && !atFull && (hp >= maxHP))
        {
            InflictEffect(this, new Effect(Effect.EffectType.Focus, (sbyte)(3 * BadgeEquippedCount(Badge.BadgeType.PerfectFocus)), Effect.INFINITE_DURATION));
        }

        return output;
    }
    public override void PerTurnStaminaHeal()
    {
        int realAgility = GetRealAgility();

        int staminaIncrease = 0;

        //wacky thing?
        /*
        if (stamina > realAgility)
        {
            staminaIncrease = Mathf.CeilToInt(realAgility / 2f);
        }
        else
        {
            staminaIncrease += realAgility;
        }
        */

        staminaIncrease += realAgility;

        if (staminaBlock)
        {
            staminaBlock = false;
        } else
        {
            if (!dead)
            {
                HealStamina(staminaIncrease);
            }
        }

        /*
        if (stamina >= BattleControl.Instance.GetMaxStamina(this))
        {
            stamina = BattleControl.Instance.GetMaxStamina(this);
        }
        if (stamina < 0)
        {
            stamina = 0;
        }
        */

        if (agilityRush < BadgeEquippedCount(Badge.BadgeType.AgilityRush) && stamina >= BattleControl.Instance.GetMaxStamina(this))
        {
            agilityRush++;
            InflictEffect(this, new Effect(Effect.EffectType.BonusTurns, (sbyte)(BadgeEquippedCount(Badge.BadgeType.AgilityRush)), Effect.INFINITE_DURATION));
        }
    }
    public override void HealStamina(int st)
    {
        base.HealStamina(st);
        if (agilityRush < BadgeEquippedCount(Badge.BadgeType.AgilityRush) && stamina >= BattleControl.Instance.GetMaxStamina(this))
        {
            agilityRush++;
            InflictEffect(this, new Effect(Effect.EffectType.BonusTurns, (sbyte)(BadgeEquippedCount(Badge.BadgeType.AgilityRush)), Effect.INFINITE_DURATION));
        }
    }
    public override int HealStaminaTrackOverhealPay(int st)
    {
        int output = base.HealStaminaTrackOverhealPay(st);
        if (agilityRush < BadgeEquippedCount(Badge.BadgeType.AgilityRush) && stamina >= BattleControl.Instance.GetMaxStamina(this))
        {
            agilityRush++;
            InflictEffect(this, new Effect(Effect.EffectType.BonusTurns, (sbyte)(BadgeEquippedCount(Badge.BadgeType.AgilityRush)), Effect.INFINITE_DURATION));
        }
        return output;
    }


    public bool AutoActionCommand()
    {
        //if there is ever a "do action commands automatically" thing this is where it would go
        return AutoMove();
    }

    public override void CheckRemoveAbsorb()
    {
        //Debug.Log(name + " check " + absorbDamageEvents);
        if (absorbDamageEvents > 0)
        {
            if (BadgeEquipped(Badge.BadgeType.AbsorbRecycling))
            {
                int count = BadgeEquippedCount(Badge.BadgeType.AbsorbRecycling);
                Effect e = GetEffectEntry(Effect.EffectType.Absorb);
                if (e != null)
                {
                    e.potency = (sbyte)(e.potency * ((count + 0.0001f) / (count + 1f)));
                    if (e.potency == 0)
                    {
                        RemoveEffect(Effect.EffectType.Absorb);
                    }
                }
            }
            else
            {
                RemoveEffect(Effect.EffectType.Absorb);
            }
            RemoveEffect(Effect.EffectType.Sunder);
            if (HasEffect(Effect.EffectType.Brittle))
            {
                Effect e = GetEffectEntry(Effect.EffectType.Brittle);
                e.potency++;
            }
        }
        absorbDamageEvents = 0;
    }
    public override bool CanMove() //some statuses prevent movement
    {
        ValidateEffects();

        Effect.EffectType[] effectList =
            new Effect.EffectType[] {
                Effect.EffectType.Freeze,
                Effect.EffectType.Sleep,
                Effect.EffectType.TimeStop,
            };

        foreach (Effect.EffectType e in effectList)
        {
            if (e == Effect.EffectType.Sleep && BadgeEquipped(Badge.BadgeType.Sleepwalk))
            {
                continue;
            }
            if (GetEffectEntry(e) != null)
            {
                return false;
            }
        }
        /*
        if (HasStatus(Status.StatusEffect.Slow))
        {
            return BattleControl.Instance.turnCount % (GetStatusEntry(Status.StatusEffect.Slow).potency + 1) == 1; //slow enemies can move on turn 1
        }
        */
        if (hp <= 0)
        {
            DeathCheck();   //may break stuff later but it shouldn't?
            return false;
        }
        return true;
    }

    public override bool AutoMove()
    {
        return HasEffect(Effect.EffectType.Berserk) || (BadgeEquipped(Badge.BadgeType.Sleepwalk) && HasEffect(Effect.EffectType.Sleep));
    }

    //Move stuff
    public override IEnumerator ExecuteMoveCoroutine()
    {
        if (currMove == null)
        {
            moveExecuting = false;
            moveActive = false;
            yield break;
        }
        //moving = true;    //Set already
        yield return currExec = StartCoroutine(currMove.Execute(this)); //this looks wrong
        moveExecuting = false;
        moveActive = false;
        //negate Charge if needed
        if (chargedAttackCount > 0)
        {
            chargedAttackCount = 0;
            if (bufferRemoveCharge)
            {
                bufferRemoveCharge = false;
            }
            else
            {
                if (BadgeEquipped(Badge.BadgeType.FocusRecycling))
                {
                    int count = BadgeEquippedCount(Badge.BadgeType.FocusRecycling);
                    Effect e = GetEffectEntry(Effect.EffectType.Focus);
                    if (e != null)
                    {
                        e.potency = (sbyte)(e.potency * ((count + 0.001f) / (count + 1f)));
                        if (e.potency == 0)
                        {
                            RemoveEffect(Effect.EffectType.Focus);
                        }
                    }
                }
                else
                {
                    RemoveEffect(Effect.EffectType.Focus);
                }
                RemoveEffect(Effect.EffectType.Defocus);
            }
        }
    }
    public override IEnumerator ExecuteMoveCoroutine(int level)
    {
        if (currMove == null)
        {
            moveExecuting = false;
            moveActive = false;
            yield break;
        }
        //moving = true;    //Set already
        yield return currExec = StartCoroutine(currMove.Execute(this, level)); //this looks wrong
        moveExecuting = false;
        moveActive = false;
        //negate Charge if needed
        if (chargedAttackCount > 0)
        {
            chargedAttackCount = 0;
            if (bufferRemoveCharge)
            {
                bufferRemoveCharge = false;
            }
            else
            {
                if (BadgeEquipped(Badge.BadgeType.FocusRecycling))
                {
                    int count = BadgeEquippedCount(Badge.BadgeType.FocusRecycling);
                    Effect e = GetEffectEntry(Effect.EffectType.Focus);
                    if (e != null)
                    {
                        e.potency = (sbyte)(e.potency * ((count + 0.0001f) / (count + 1f)));
                        if (e.potency == 0)
                        {
                            RemoveEffect(Effect.EffectType.Focus);
                        }
                    }
                }
                else
                {
                    RemoveEffect(Effect.EffectType.Focus);
                }
                RemoveEffect(Effect.EffectType.Defocus);
            }
        }
    }



    public int DangerHP()
    {
        return PlayerData.PlayerDataEntry.GetDangerHP(entityID);
    }
    public bool InDanger()
    {
        return hp <= DangerHP();
    }

    public bool ShowDangerAnim()
    {
        return BattleControl.Instance.playerData.ShowDangerAnim(entityID);
    }


    public override int GetEffectAttackBonus(bool absolute = false)
    {
        int original = base.GetEffectAttackBonus();
        int passiveAtk = Item.CountItemsWithProperty(Item.ItemProperty.Passive_AttackUp, BattleControl.Instance.GetItemInventory(this));
        int passiveMAtk = Item.CountItemsWithProperty(Item.ItemProperty.Passive_AttackDown, BattleControl.Instance.GetItemInventory(this));

        if (absolute)
        {
            passiveMAtk *= -1;
        }

        return original + passiveAtk - passiveMAtk;
    }
    public override int GetEffectDefenseBonus(bool absolute = false)
    {
        int original = base.GetEffectDefenseBonus();
        int passiveDef = Item.CountItemsWithProperty(Item.ItemProperty.Passive_DefenseUp, BattleControl.Instance.GetItemInventory(this));
        int passiveMDef = Item.CountItemsWithProperty(Item.ItemProperty.Passive_DefenseDown, BattleControl.Instance.GetItemInventory(this));

        if (absolute)
        {
            passiveMDef *= -1;
        }

        return original + passiveDef - passiveMDef;
    }
    public override int GetEffectEnduranceBonus(bool absolute = false)
    {
        int original = base.GetEffectEnduranceBonus();
        int passiveEnd = Item.CountItemsWithProperty(Item.ItemProperty.Passive_EnduranceUp, BattleControl.Instance.GetItemInventory(this));
        int passiveMEnd = Item.CountItemsWithProperty(Item.ItemProperty.Passive_EnduranceDown, BattleControl.Instance.GetItemInventory(this));

        if (absolute)
        {
            passiveMEnd *= -1;
        }

        return original + passiveEnd - passiveMEnd;
    }
    public override int GetEffectAgilityBonus(bool absolute = false)
    {
        int original = base.GetEffectAgilityBonus();
        int passiveAgi = Item.CountItemsWithProperty(Item.ItemProperty.Passive_AgilityUp, BattleControl.Instance.GetItemInventory(this));
        int passiveMAgi = Item.CountItemsWithProperty(Item.ItemProperty.Passive_AgilityDown, BattleControl.Instance.GetItemInventory(this));

        if (absolute)
        {
            passiveMAgi *= -1;
        }

        return original + passiveAgi - passiveMAgi;
    }

    public override int GetBadgeAttackBonus()
    {
        int bonus = 0;

        //not a badge but nowhere else to put it
        if (RibbonEquipped(Ribbon.RibbonType.ThornyRibbon, false))
        {
            bonus++;
        }

        if (BadgeEquipped(Badge.BadgeType.RiskyPower) && InDanger())
        {
            bonus += 2 * BadgeEquippedCount(Badge.BadgeType.RiskyPower);
        }

        //Grab the other player
        PlayerEntity other = null;
        List<PlayerEntity> players = BattleControl.Instance.GetPlayerEntities();
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i] != this)
            {
                other = players[i];
                break;
            }
        }

        if (BadgeEquipped(Badge.BadgeType.ProtectivePower))
        {
            if (other != null && other.InDanger())
            {
                bonus += 1 * BadgeEquippedCount(Badge.BadgeType.ProtectivePower);
            }
        }

        if (BadgeEquipped(Badge.BadgeType.PerfectPower) && hp >= maxHP)
        {
            bonus += 2 * BadgeEquippedCount(Badge.BadgeType.PerfectPower);
        }

        if (BadgeEquipped(Badge.BadgeType.EnergeticPower) && BattleControl.Instance.GetEP(this) >= BattleControl.Instance.GetMaxEP(this))
        {
            bonus += 2 * BadgeEquippedCount(Badge.BadgeType.EnergeticPower);
        }

        if (BadgeEquipped(Badge.BadgeType.AgilePower) && stamina >= BattleControl.Instance.GetMaxStamina(this))
        {
            bonus += 2 * BadgeEquippedCount(Badge.BadgeType.AgilePower);
        }

        //note: the "pay for boost" badges will work even if you only have 1 of the resource despite the cost being higher
        //not very abusable as this cost thing is only usable once per turn ish
        if (BadgeEquipped(Badge.BadgeType.SpiritPower) && BattleControl.Instance.GetSE(this) > 0)
        {
            bonus += BadgeEquippedCount(Badge.BadgeType.SpiritPower);
        } else if (BadgeEquipped(Badge.BadgeType.SpiritPower) && BattleControl.Instance.GetSE(this) <= 0)
        {
            //backfire
            bonus -= BadgeEquippedCount(Badge.BadgeType.SpiritPower);
        }

        if (BadgeEquipped(Badge.BadgeType.GoldenPower) && BattleControl.Instance.playerData.coins > 0)
        {
            bonus += BadgeEquippedCount(Badge.BadgeType.GoldenPower);
        }
        else if (BadgeEquipped(Badge.BadgeType.GoldenPower) && BattleControl.Instance.playerData.coins <= 0)
        {
            //backfire
            bonus -= BadgeEquippedCount(Badge.BadgeType.GoldenPower);
        }

        if (BadgeEquipped(Badge.BadgeType.DarkPower) && BattleControl.Instance.GetSE(this) <= BattleControl.Instance.GetMaxSE(this) / 3)
        {
            bonus += BadgeEquippedCount(Badge.BadgeType.DarkPower);
        }

        if (BadgeEquipped(Badge.BadgeType.NullPower) && BattleControl.Instance.GetEP(this) <= 6)
        {
            bonus += BadgeEquippedCount(Badge.BadgeType.NullPower);
        }

        bonus += BadgeEquippedCount(Badge.BadgeType.AttackBoost);

        if (BadgeEquipped(Badge.BadgeType.SoftPower))
        {
            bonus += BadgeEquippedCount(Badge.BadgeType.SoftPower);
        }

        if (BadgeEquipped(Badge.BadgeType.MetalPower))
        {
            bonus += BadgeEquippedCount(Badge.BadgeType.MetalPower);
        }

        bool isFront = false;
        isFront = other == null || other.posId < posId || !other.alive; //Note: no ally or dead ally = being in front, as enemies will target you very often which is the drawback of being in front

        if (BadgeEquipped(Badge.BadgeType.AttackFormation))
        {
            if (isFront)
            {
                bonus += BadgeEquippedCount(Badge.BadgeType.AttackFormation);
            }
            else
            {
                bonus -= BadgeEquippedCount(Badge.BadgeType.AttackFormation);
            }

            if (BadgeEquipped(Badge.BadgeType.DefenseFormation))
            {
                if (isFront)
                {
                    bonus += BadgeEquippedCount(Badge.BadgeType.AttackFormation);
                }
                else
                {
                    bonus -= BadgeEquippedCount(Badge.BadgeType.AttackFormation);
                }
            }
        } else
        {
            if (BadgeEquipped(Badge.BadgeType.DefenseFormation))
            {
                if (isFront)
                {
                    bonus -= BadgeEquippedCount(Badge.BadgeType.AttackFormation);
                }
                else
                {
                    bonus += BadgeEquippedCount(Badge.BadgeType.AttackFormation);
                }
            }
        }

        if (BadgeEquipped(Badge.BadgeType.MagicClock))
        {
            if (BattleControl.Instance.turnCount % 4 == 1)
            {
                bonus += 2 * BadgeEquippedCount(Badge.BadgeType.MagicClock);
            }
            if (BattleControl.Instance.turnCount % 4 == 3)
            {
                bonus -= 2 * BadgeEquippedCount(Badge.BadgeType.MagicClock);
            }
        }

        if (BadgeEquipped(Badge.BadgeType.PowerGear))
        {
            if (nonItemHitsDealt % 4 == 3)
            {
                bonus += 3 * BadgeEquippedCount(Badge.BadgeType.PowerGear);
            } else
            {
                bonus -= BadgeEquippedCount(Badge.BadgeType.PowerGear);
            }
        }

        if (BadgeEquipped(Badge.BadgeType.PowerMomentum))
        {
            bonus += 2 * actionCounter * BadgeEquippedCount(Badge.BadgeType.PowerMomentum);
        }

        if (BadgeEquipped(Badge.BadgeType.Overexert))
        {
            bonus += 2 * BadgeEquippedCount(Badge.BadgeType.Overexert);
        }

        if (BadgeEquipped(Badge.BadgeType.RagesPower) && HasEffect(Effect.EffectType.Berserk))
        {
            bonus += 1 * BadgeEquippedCount(Badge.BadgeType.RagesPower);
        }

        if (other != null && other.BadgeEquipped(Badge.BadgeType.RageRally) && other.HasEffect(Effect.EffectType.Berserk))
        {
            bonus += 1 * BadgeEquippedCount(Badge.BadgeType.RageRally);
        }

        if (BadgeEquipped(Badge.BadgeType.ToxicStrength) && HasEffect(Effect.EffectType.Poison))
        {
            bonus += 2 * BadgeEquippedCount(Badge.BadgeType.ToxicStrength);
        }


        if (BadgeEquipped(Badge.BadgeType.HealthSteal))
        {
            bonus -= BadgeEquippedCount(Badge.BadgeType.HealthSteal);
        }

        if (BadgeEquipped(Badge.BadgeType.EnergySteal))
        {
            bonus -= BadgeEquippedCount(Badge.BadgeType.EnergySteal);
        }

        return bonus;
    }
    public override int GetBadgeDefenseBonus()
    {
        int bonus = 0;

        bonus += BadgeEquippedCount(Badge.BadgeType.DefenseBoost);

        if (BadgeEquipped(Badge.BadgeType.SoftPower) && BadgeEquipped(Badge.BadgeType.MetalPower))
        {
            bonus += 2 * BadgeEquippedCount(Badge.BadgeType.MetalPower);
        }

        bool isFront = false;
        //Grab the other player
        PlayerEntity otherB = null;
        List<PlayerEntity> playersB = BattleControl.Instance.GetPlayerEntities();
        for (int i = 0; i < playersB.Count; i++)
        {
            if (playersB[i] != this)
            {
                otherB = playersB[i];
                break;
            }
        }
        isFront = otherB == null || otherB.posId < posId;

        if (BadgeEquipped(Badge.BadgeType.DefenseFormation))
        {
            if (isFront)
            {
                bonus += BadgeEquippedCount(Badge.BadgeType.DefenseFormation);
            }
            else
            {
                bonus -= BadgeEquippedCount(Badge.BadgeType.DefenseFormation);
            }

            if (BadgeEquipped(Badge.BadgeType.AttackFormation))
            {
                if (isFront)
                {
                    bonus += BadgeEquippedCount(Badge.BadgeType.DefenseFormation);
                }
                else
                {
                    bonus -= BadgeEquippedCount(Badge.BadgeType.DefenseFormation);
                }
            }
        }
        else
        {
            if (BadgeEquipped(Badge.BadgeType.AttackFormation))
            {
                if (isFront)
                {
                    bonus -= BadgeEquippedCount(Badge.BadgeType.AttackFormation);
                }
                else
                {
                    bonus += BadgeEquippedCount(Badge.BadgeType.AttackFormation);
                }
            }
        }

        if (BadgeEquipped(Badge.BadgeType.MagicClock))
        {
            if (BattleControl.Instance.turnCount % 4 == 2)
            {
                bonus += 2 * BadgeEquippedCount(Badge.BadgeType.MagicClock);
            }
            if (BattleControl.Instance.turnCount % 4 == 0)
            {
                bonus -= 2 * BadgeEquippedCount(Badge.BadgeType.MagicClock);
            }
        }

        if (BadgeEquipped(Badge.BadgeType.ShieldGear))
        {
            if (hitsTaken % 4 == 3)
            {
                bonus += 3 * BadgeEquippedCount(Badge.BadgeType.ShieldGear);
            } else
            {
                bonus -= BadgeEquippedCount(Badge.BadgeType.ShieldGear);
            }
        }

        if (HasEffect(Effect.EffectType.Berserk) && BadgeEquipped(Badge.BadgeType.RageShield))
        {
            bonus += 1 * BadgeEquippedCount(Badge.BadgeType.RageShield);
        }

        if (HasEffect(Effect.EffectType.Poison) && BadgeEquipped(Badge.BadgeType.ToxicShield))
        {
            bonus += 2 * BadgeEquippedCount(Badge.BadgeType.ToxicShield);
        }

        if (HasEffect(Effect.EffectType.Freeze) && BadgeEquipped(Badge.BadgeType.IceShell))
        {
            bonus += 2 * BadgeEquippedCount(Badge.BadgeType.IceShell);
        }

        if (BadgeEquipped(Badge.BadgeType.Supercooled))
        {
            bonus += 2 * BadgeEquippedCount(Badge.BadgeType.Supercooled);
        }

        return bonus;
    }
    public override int GetBadgeEnduranceBonus()
    {
        int bonus = 0;

        if (BadgeEquipped(Badge.BadgeType.RiskyEndurance) && InDanger())
        {
            bonus += 1 * BadgeEquippedCount(Badge.BadgeType.RiskyEndurance);
        }

        if (BadgeEquipped(Badge.BadgeType.ProtectiveEndurance))
        {
            //Grab the other player
            PlayerEntity other = null;
            List<PlayerEntity> players = BattleControl.Instance.GetPlayerEntities();
            for (int i = 0; i < players.Count; i++)
            {
                if (players[i] != this)
                {
                    other = players[i];
                    break;
                }
            }

            if (other != null && other.InDanger())
            {
                bonus += 1 * BadgeEquippedCount(Badge.BadgeType.ProtectiveEndurance);
            }
        }

        if (BadgeEquipped(Badge.BadgeType.PerfectEndurance) && hp >= maxHP)
        {
            bonus += 1 * BadgeEquippedCount(Badge.BadgeType.PerfectEndurance);
        }

        if (BadgeEquipped(Badge.BadgeType.EnergyBurst) && BattleControl.Instance.GetEP(this) >= BattleControl.Instance.GetMaxEP(this))
        {
            bonus += 3 * BadgeEquippedCount(Badge.BadgeType.EnergyBurst);
        }

        if (BadgeEquipped(Badge.BadgeType.AgileEndurance) && stamina >= BattleControl.Instance.GetMaxStamina(this))
        {
            bonus += 3 * BadgeEquippedCount(Badge.BadgeType.AgileEndurance);
        }

        if (BadgeEquipped(Badge.BadgeType.DarkEndurance))
        {
            bonus += BadgeEquippedCount(Badge.BadgeType.DarkEndurance);
        }

        bonus += BadgeEquippedCount(Badge.BadgeType.EnduranceBoost);

        if (BadgeEquipped(Badge.BadgeType.EnergyGear))
        {
            if (movesUsed % 4 == 3)
            {
                bonus += 3 * BadgeEquippedCount(Badge.BadgeType.EnergyGear);
            } else 
            {
                bonus -= BadgeEquippedCount(Badge.BadgeType.EnergyGear);
            }
        }


        return bonus;
    }
    public override int GetBadgeAgilityBonus()
    {
        int bonus = 0;

        bonus += 1 * BadgeEquippedCount(Badge.BadgeType.AgilityBoost);
        /*
        if (BadgeEquipped(Badge.BadgeType.AgilityBoost))
        {
            bonus++;
        }
        if (BadgeEquipped(Badge.BadgeType.AgilityBoostB))
        {
            bonus++;
        }
        */

        return bonus;
    }
    public override int GetBadgeFlowBonus()
    {
        int bonus = 0;

        bonus += 3 * BadgeEquippedCount(Badge.BadgeType.SpiritFlow);

        return bonus;
    }
    public override int GetBadgeResistanceBonus()
    {
        int bonus = 0;


        if (BadgeEquipped(Badge.BadgeType.RiskyShield) && InDanger())
        {
            bonus += 2 * BadgeEquippedCount(Badge.BadgeType.RiskyShield);
        }

        if (BadgeEquipped(Badge.BadgeType.ProtectiveShield))
        {
            //Grab the other player
            PlayerEntity other = null;
            List<PlayerEntity> players = BattleControl.Instance.GetPlayerEntities();
            for (int i = 0; i < players.Count; i++)
            {
                if (players[i] != this)
                {
                    other = players[i];
                    break;
                }
            }

            if (other != null && other.InDanger())
            {
                bonus += BadgeEquippedCount(Badge.BadgeType.ProtectiveShield);
            }
        }

        if (BadgeEquipped(Badge.BadgeType.PerfectShield) && hp >= maxHP)
        {
            bonus += 2 * BadgeEquippedCount(Badge.BadgeType.PerfectShield);
        }

        if (BadgeEquipped(Badge.BadgeType.EnergyShield) && BattleControl.Instance.GetEP(this) > 0)
        {
            bonus += 2 * BadgeEquippedCount(Badge.BadgeType.EnergyShield);
        }

        if (BadgeEquipped(Badge.BadgeType.AgileShield) && stamina >= BattleControl.Instance.GetMaxStamina(this))
        {
            bonus += 2 * BadgeEquippedCount(Badge.BadgeType.AgileShield);
        }

        if (BadgeEquipped(Badge.BadgeType.SpiritShield) && BattleControl.Instance.GetSE(this) > 0)
        {
            bonus += 2 * BadgeEquippedCount(Badge.BadgeType.SpiritShield);
        }

        if (BadgeEquipped(Badge.BadgeType.GoldenShield) && BattleControl.Instance.GetCoins(this) > 0)
        {
            bonus += 2 * BadgeEquippedCount(Badge.BadgeType.GoldenShield);
        }

        if (BadgeEquipped(Badge.BadgeType.DarkShield) && BattleControl.Instance.GetSE(this) <= BattleControl.Instance.GetMaxSE(this) / 3)
        {
            bonus += BadgeEquippedCount(Badge.BadgeType.DarkShield);
        }

        if (BadgeEquipped(Badge.BadgeType.NullShield) && BattleControl.Instance.GetEP(this) <= 6)
        {
            bonus += BadgeEquippedCount(Badge.BadgeType.NullShield);
        }

        if (HasEffect(Effect.EffectType.Paralyze) && BadgeEquipped(Badge.BadgeType.Resistor))
        {
            bonus += BadgeEquippedCount(Badge.BadgeType.Resistor);
        }

        return bonus;
    }
    public override float GetItemUseBonus()
    {
        float bonus = 1;
        if (BadgeEquipped(Badge.BadgeType.ItemBoost))
        {
            bonus += 0.0001f + (BadgeEquippedCount(Badge.BadgeType.ItemBoost) / 3f);
        }
        if (MainManager.Instance.GetGlobalFlag(MainManager.GlobalFlag.GF_FileCode_Gluttony))
        {
            bonus /= 2;
        }
        if (BattleControl.Instance.enviroEffect == EnvironmentalEffect.AcidFlow)
        {
            bonus *= 0.10001f;
        }
        if (BattleControl.Instance.enviroEffect == EnvironmentalEffect.VoidShadow)
        {
            bonus *= 0.50001f;
        }
        if (BattleControl.Instance.enviroEffect == EnvironmentalEffect.TrialOfAmbition)
        {
            bonus *= 0.10001f;
        }
        return bonus;
    }
    public override float GetItemReceiveBonus()
    {
        float bonus = 1;
        if (BadgeEquipped(Badge.BadgeType.VoraciousEater))
        {
            bonus += 0.0001f + (BadgeEquippedCount(Badge.BadgeType.VoraciousEater) / 3f);
        }
        return bonus;
    }
    public override int GetBoostedAgility()
    {
        int baseAgility = agility;
        if (BadgeEquipped(Badge.BadgeType.DizzyAgility) && HasEffect(Effect.EffectType.Dizzy))
        {
            baseAgility += (agility / 2);
        }
        return baseAgility + GetBadgeAgilityBonus() + GetEffectAgilityBonus();
    }




    public override void OnHitEffects(BattleEntity target, int damage, DamageType type, ulong properties, int startDamage)
    {
        if (damage < 0)
        {
            return;
        }

        //Apply HP drain
        if (BattleHelper.GetDamageProperty(properties, BattleHelper.DamageProperties.HPDrainOneToOne))
        {
            HealHealth(damage);
        }

        //Astral recovery
        if (target.HasEffect(Effect.EffectType.AstralRecovery) && target.hp > 0 && !BattleHelper.GetDamageProperty(properties, BattleHelper.DamageProperties.Static))
        {
            target.HealHealth(startDamage / 5);
        }

        if (!BattleHelper.GetDamageProperty(properties, DamageProperties.Static))
        {
            //Pay for power stuff
            if (BadgeEquipped(Badge.BadgeType.SpiritPower))
            {
                HealSoulEnergy(-3 * BadgeEquippedCount(Badge.BadgeType.SpiritPower));
            }

            if (BadgeEquipped(Badge.BadgeType.GoldenPower))
            {
                //To do: Coin effects
                HealCoins(-10 * BadgeEquippedCount(Badge.BadgeType.GoldenPower));
            }

            //Status Catalyst
            if (!target.statusCatalyst)
            {
                if (target.HasStatus() && target.statusMaxTurns > 0 && BadgeEquipped(Badge.BadgeType.AilmentCatalyst))
                {
                    //Enforce type check
                    bool check = false;

                    if ((((int)type & (int)DamageType.Light) != 0) && target.HasEffect(Effect.EffectType.Freeze))
                    {
                        check = true;
                    }
                    if ((((int)type & (int)DamageType.Dark) != 0) && target.HasEffect(Effect.EffectType.Poison))
                    {
                        check = true;
                    }
                    if ((((int)type & (int)DamageType.Earth) != 0) && target.HasEffect(Effect.EffectType.Dizzy))
                    {
                        check = true;
                    }
                    if ((((int)type & (int)DamageType.Air) != 0) && target.HasEffect(Effect.EffectType.Paralyze))
                    {
                        check = true;
                    }
                    if ((((int)type & (int)DamageType.Water) != 0) && target.HasEffect(Effect.EffectType.Sleep))
                    {
                        check = true;
                    }
                    if ((((int)type & (int)DamageType.Fire) != 0) && target.HasEffect(Effect.EffectType.Berserk))
                    {
                        check = true;
                    }

                    if (check)
                    {
                        Effect status = null;
                        for (int i = 0; i < target.effects.Count; i++)
                        {
                            if (Effect.GetEffectClass(target.effects[i].effect) == Effect.EffectClass.Status)
                            {
                                status = target.effects[i];
                            }
                        }

                        if (status != null)
                        {
                            target.statusCatalyst = true;
                            sbyte boost = (sbyte)(BadgeEquippedCount(Badge.BadgeType.AilmentCatalyst));
                            status.duration += boost;
                            target.statusMaxTurns -= boost;
                        }
                    }
                }
            }

            //the various effect inflicting things
            if (target.HasEffect(Effect.EffectType.Dizzy) && BadgeEquipped(Badge.BadgeType.HypnoStrike))
            {
                InflictEffect(target, new Effect(Effect.EffectType.Defocus, (sbyte)(BadgeEquippedCount(Badge.BadgeType.HypnoStrike)), Effect.INFINITE_DURATION));
            }

            if (target.HasEffect(Effect.EffectType.Paralyze) && BadgeEquipped(Badge.BadgeType.Conductor))
            {
                InflictEffectBuffered(this, new Effect(Effect.EffectType.Focus, (sbyte)(BadgeEquippedCount(Badge.BadgeType.Conductor)), Effect.INFINITE_DURATION));
            }

            if (BadgeEquipped(Badge.BadgeType.Overexert))
            {
                InflictEffect(this, new Effect(Effect.EffectType.Paralyze, 1, (sbyte)(2 * BadgeEquippedCount(Badge.BadgeType.Overexert))));
                if (!HasEffect(Effect.EffectType.Paralyze))
                {
                    InflictEffectBuffered(this, new Effect(Effect.EffectType.Defocus, (sbyte)(3 * BadgeEquippedCount(Badge.BadgeType.Overexert)), Effect.INFINITE_DURATION));
                }
            }

            if (target.wakeUp && BadgeEquipped(Badge.BadgeType.NightmareStrike))
            {
                InflictEffect(target, new Effect(Effect.EffectType.Defocus, (sbyte)(3 * BadgeEquippedCount(Badge.BadgeType.NightmareStrike)), Effect.INFINITE_DURATION));
            }

            if (target.HasEffect(Effect.EffectType.Berserk) && BadgeEquipped(Badge.BadgeType.Aggravate))
            {
                InflictEffect(target, new Effect(Effect.EffectType.Focus, (sbyte)(BadgeEquippedCount(Badge.BadgeType.Aggravate)), Effect.INFINITE_DURATION));
            }

            if (target.HasEffect(Effect.EffectType.Poison) && BadgeEquipped(Badge.BadgeType.NerveStrike))
            {
                InflictEffectBuffered(target, new Effect(Effect.EffectType.Sunder, (sbyte)(2 * BadgeEquippedCount(Badge.BadgeType.NerveStrike)), Effect.INFINITE_DURATION));
            }

            if (BadgeEquipped(Badge.BadgeType.HealthSteal))
            {
                HealHealth(BadgeEquippedCount(Badge.BadgeType.HealthSteal));
            }

            if (BadgeEquipped(Badge.BadgeType.EnergySteal))
            {
                HealEnergy(BadgeEquippedCount(Badge.BadgeType.EnergySteal));
            }
        }

        //Apply sprout
        if (target.HasEffect(Effect.EffectType.DrainSprout))
        {
            //target gets defocus
            //note: inflict status
            sbyte drain = target.GetEffectEntry(Effect.EffectType.DrainSprout).potency;
            InflictEffect(target, new Effect(Effect.EffectType.Defocus, drain, Effect.INFINITE_DURATION));
            HealHealth(2 * drain);
        }

        if (target.HasEffect(Effect.EffectType.BoltSprout))
        {
            //target gets sunder (buffered)
            //note: inflict status
            sbyte drain = target.GetEffectEntry(Effect.EffectType.BoltSprout).potency;
            InflictEffectBuffered(target, new Effect(Effect.EffectType.Sunder, drain, Effect.INFINITE_DURATION));
            HealEnergy(2 * drain);
        }

        if (BattleControl.IsPlayerControlled(this, false) && BattleControl.Instance.enviroEffect == EnvironmentalEffect.SeasideAir)
        {
            bool hard = BattleControl.Instance.HarderEnviroEffects();
            float power = BattleControl.Instance.EnviroEffectPower();

            if (hard)
            {
                int procSeasideAir = BattleControl.Instance.EnviroEveryXTurns(2, power, cumulativeAttackHitCount);
                if (procSeasideAir > 0)
                {
                    InflictEffect(this, new Effect(Effect.EffectType.Sunder, (sbyte)procSeasideAir, Effect.INFINITE_DURATION));
                }
            }
            else
            {
                int procSeasideAir = BattleControl.Instance.EnviroEveryXTurns(4, power, cumulativeAttackHitCount);
                if (procSeasideAir > 0)
                {
                    InflictEffect(this, new Effect(Effect.EffectType.Sunder, (sbyte)procSeasideAir, Effect.INFINITE_DURATION));
                }
            }
        }

        if (target.HasEffect(Effect.EffectType.ParryAura))
        {
            InflictEffect(target, new Effect(Effect.EffectType.Focus, target.GetEffectEntry(Effect.EffectType.ParryAura).potency, Effect.INFINITE_DURATION));
        }
        if (target.HasEffect(Effect.EffectType.BolsterAura))
        {
            InflictEffectBuffered(target, new Effect(Effect.EffectType.Absorb, target.GetEffectEntry(Effect.EffectType.BolsterAura).potency, Effect.INFINITE_DURATION));
        }

        if (target.HasEffect(Effect.EffectType.Elusive))
        {
            InflictEffectBuffered(target, new Effect(Effect.EffectType.Ethereal, 1, target.GetEffectEntry(Effect.EffectType.Elusive).potency));
        }
    }

    public override void InvokeHurtEvents(DamageType type, ulong properties)
    {
        wakeUp = false;
        if (BattleHelper.GetDamageProperty(properties, BattleHelper.DamageProperties.ContactHazard))
        {
            QueueEvent(BattleHelper.Event.HiddenHurt);
            //if (target.hp == 0)
            //{
            //    target.hp = 1;
            //}
            return;
        }
        if (BattleHelper.GetDamageProperty(properties, BattleHelper.DamageProperties.MetaKnockback))
        {
            QueueEvent(BattleHelper.Event.MetaKnockbackHurt);
            if (hp <= 0 && !BattleHelper.GetDamageProperty(properties, BattleHelper.DamageProperties.Combo))
            {
                QueueEvent(BattleHelper.Event.Death);
            }
            return;
        }
        if (BattleHelper.GetDamageProperty(properties, BattleHelper.DamageProperties.Knockback))
        {
            QueueEvent(BattleHelper.Event.KnockbackHurt);
            if (hp <= 0 && !BattleHelper.GetDamageProperty(properties, BattleHelper.DamageProperties.Combo))
            {
                QueueEvent(BattleHelper.Event.Death);
            }
            return;
        }
        if (BattleHelper.GetDamageProperty(properties, BattleHelper.DamageProperties.Combo))
        {
            QueueEvent(BattleHelper.Event.ComboHurt);
        }
        else
        {
            QueueEvent(BattleHelper.Event.Hurt);

            if (!BattleHelper.GetDamageProperty(properties, DamageProperties.SoftTouch))
            {
                if (!GetEntityProperty(EntityProperties.DeepSleep) && HasEffect(Effect.EffectType.Sleep))
                {
                    Effect s = GetEffectEntry(Effect.EffectType.Sleep);
                    s.potency--;
                    if (s.potency <= 0)
                    {
                        effects.Remove(s);
                        QueueEvent(BattleHelper.Event.CureStatus);
                    }
                    wakeUp = true;
                }
                if (!GetEntityProperty(EntityProperties.Glacier) && HasEffect(Effect.EffectType.Freeze))
                {
                    Effect s = GetEffectEntry(Effect.EffectType.Freeze);
                    s.potency--;
                    if (s.potency <= 0)
                    {
                        effects.Remove(s);
                        QueueEvent(BattleHelper.Event.CureStatus);
                    }
                    if (BadgeEquipped(Badge.BadgeType.Icebreaker))
                    {
                        InflictEffect(this, new Effect(Effect.EffectType.Focus, (sbyte)(3 * BadgeEquippedCount(Badge.BadgeType.Icebreaker)), Effect.INFINITE_DURATION));
                    }
                }
            }

            if (hp <= 0 && !BattleHelper.GetDamageProperty(properties, BattleHelper.DamageProperties.Combo))
            {
                QueueEvent(BattleHelper.Event.Death);
            }
        }
    }

    public float CalculateStatusBoost(BattleEntity target)
    {
        float statusBoost = 1;
        if (BadgeEquipped(Badge.BadgeType.AilmentBoost))
        {
            statusBoost += 0.5f * BadgeEquippedCount(Badge.BadgeType.AilmentBoost);
        }
        if (target.HasStatus() && BadgeEquipped(Badge.BadgeType.AilmentConversion))
        {
            statusBoost += 0.5f * BadgeEquippedCount(Badge.BadgeType.AilmentConversion);
        }
        return statusBoost;
    }

    public override void InflictEffect(BattleEntity target, Effect se, int casterID = int.MinValue, Effect.EffectStackMode mode = Effect.EffectStackMode.Default)
    {
        if (target.HasEffect(Effect.EffectType.Inverted))
        {
            Effect e = se.Copy();
            InvertEffect(e);
            se = e;
        }

        //added stuff
        bool bol = MainManager.Instance.GetGlobalFlag(MainManager.GlobalFlag.GF_FileCode_Lust);

        float multiplier = 1;

        if (BadgeEquipped(Badge.BadgeType.EffectBoost))
        {
            multiplier = (1f + 0.5f * BadgeEquippedCount(Badge.BadgeType.EffectBoost));
        }

        if (bol)
        {
            multiplier += 0.5f;
        }


        if (target != null)
        {
            if (target.posId >= 0 && multiplier != 1)
            {
                se = se.Copy();
                if (se.duration != Effect.INFINITE_DURATION)
                {
                    if ((se.duration * multiplier) > Effect.MAX_NORMAL_DURATION)
                    {
                        se.duration = Effect.MAX_NORMAL_DURATION;
                    } else
                    {
                        se.duration = (sbyte)(se.duration * multiplier);
                    }
                }
            }
        }

        if (se.duration < 0)
        {
            se.duration = (sbyte)(-se.duration);
            Effect e = se.Copy();
            InvertEffect(e);
            se = e;
        }

        float statusBoost = CalculateStatusBoost(target);
        //end added stuff (but statusBoost is used later)


        if (target != null)
        {
            bool statusWorks = true;

            if (target.HasEffect(Effect.EffectType.Immunity) || target.HasEffect(Effect.EffectType.TimeStop))
            {
                if (Effect.IsCurable(se.effect))
                {
                    BattleControl.Instance.CreateEffectBlockedParticles(se, target);
                    return;
                }
            }

            if (target.HasEffect(Effect.EffectType.Seal) || target.HasEffect(Effect.EffectType.TimeStop))
            {
                if (Effect.IsCleanseable(se.effect))
                {
                    BattleControl.Instance.CreateEffectBlockedParticles(se, target);
                    return;
                }
            }

            if (target.GetEntityProperty(EntityProperties.NoMiracle))
            {
                if (se.effect == Effect.EffectType.Miracle)
                {
                    BattleControl.Instance.CreateEffectBlockedParticles(se, target);
                    return;
                }
            }

            //check status table?
            if (Effect.GetEffectClass(se.effect) == Effect.EffectClass.Status)
            {
                StatusTableEntry st = target.GetStatusTableEntry(se.effect);

                //Check hp/maxhp < modified suseptibility
                //Modified suseptibility = s -  1 + (mst / bsmt)

                //so
                //(hp/maxhp) <= s + (smt - bsmt) / bsmt
                //(hp/maxhp) * bsmt <= s * bsmt + smt - bsmt
                //hp * bsmt <= maxhp * (s * bsmt + smt - bsmt)
                //Yay no division required so no rounding error (?)
                //but this equation is a little wacky looking

                //statusWorks = (target.hp * target.baseStatusMaxTurns <= target.maxHP * (st.susceptibility * target.baseStatusMaxTurns + target.statusMaxTurns - target.baseStatusMaxTurns));
                statusWorks = target.StatusWillWork(se.effect, statusBoost);
                //Debug.Log(statusWorks);
            }

            //Debug.Log(statusWorks + " " + target.baseStatusMaxTurns);

            //bmst less than 0 is immunity to everything you can inflict
            if ((target.baseStatusMaxTurns >= 0 || Effect.IsCleanseable(se.effect, true)) && statusWorks)
            {
                BattleControl.Instance.CreateEffectParticles(se, target);
                target.ReceiveEffect(se, casterID, mode);
            }
            else
            {
                //blocked, but why
                bool statusBlocked = (Effect.GetEffectClass(se.effect) == Effect.EffectClass.Status) && target.StatusWorkingHP(se.effect) > 0;

                if (statusBlocked)
                {
                    BattleControl.Instance.CreateStatusNotYetParticles(se, target);
                }
                else
                {
                    BattleControl.Instance.CreateEffectBlockedParticles(se, target);
                }
            }
        }
    }
    public override void InflictEffectBuffered(BattleEntity target, Effect se, int casterID = int.MinValue, Effect.EffectStackMode mode = Effect.EffectStackMode.Default)
    {
        if (target.HasEffect(Effect.EffectType.Inverted))
        {
            Effect e = se.Copy();
            InvertEffect(e);
            se = e;
        }

        bool bol = MainManager.Instance.GetGlobalFlag(MainManager.GlobalFlag.GF_FileCode_Lust);

        float multiplier = 1;

        if (BadgeEquipped(Badge.BadgeType.EffectBoost))
        {
            multiplier = (1f + 0.5f * BadgeEquippedCount(Badge.BadgeType.EffectBoost));
        }

        if (bol)
        {
            multiplier += 0.5f;
        }

        if (target != null)
        {
            if (target.posId >= 0 && multiplier != 1)
            {
                se = se.Copy();
                if (se.duration != Effect.INFINITE_DURATION)
                {
                    if ((se.duration * multiplier) > Effect.MAX_NORMAL_DURATION)
                    {
                        se.duration = Effect.MAX_NORMAL_DURATION;
                    }
                    else
                    {
                        se.duration = (sbyte)(se.duration * multiplier);
                    }
                }
            }
        }

        if (se.duration < 0)
        {
            se.duration = (sbyte)(-se.duration);
            Effect e = se.Copy();
            InvertEffect(e);
            se = e;
        }

        float statusBoost = CalculateStatusBoost(target);

        if (target != null)
        {
            bool statusWorks = true;

            //check status table?
            if (Effect.GetEffectClass(se.effect) == Effect.EffectClass.Status)
            {
                StatusTableEntry st = target.GetStatusTableEntry(se.effect);

                //Check hp/maxhp < modified suseptibility
                //Modified suseptibility = s - 1 + (mst / bsmt)

                //so
                //(hp/maxhp) <= s + (smt - bsmt) / bsmt
                //(hp/maxhp) * bsmt <= s * bsmt + smt - bsmt
                //hp * bsmt <= maxhp * (s * bsmt + smt - bsmt)
                //Yay no division required so no rounding error (?)
                //but this equation is a little wacky looking

                //statusWorks = (target.hp * target.baseStatusMaxTurns <= target.maxHP * (st.susceptibility * target.baseStatusMaxTurns + target.statusMaxTurns - target.baseStatusMaxTurns));
                statusWorks = target.StatusWillWork(se.effect, statusBoost);
            }

            //Debug.Log(statusWorks + " " + target.baseStatusMaxTurns);

            //bmst less than 0 is immunity to everything you can inflict
            if ((target.baseStatusMaxTurns >= 0 || Effect.IsCleanseable(se.effect, true)) && statusWorks)
            {
                BattleControl.Instance.CreateEffectParticles(se, target);
                target.ReceiveEffectBuffered(se, casterID, mode);
            }
            else
            {
                //blocked, but why
                bool statusBlocked = (Effect.GetEffectClass(se.effect) == Effect.EffectClass.Status) && target.StatusWorkingHP(se.effect) > 0;

                if (statusBlocked)
                {
                    BattleControl.Instance.CreateStatusNotYetParticles(se, target);
                }
                else
                {
                    BattleControl.Instance.CreateEffectBlockedParticles(se, target);
                }
            }
        }
    }

    public void ApplyInnEffects()
    {
        PlayerData pd = BattleControl.Instance.playerData;

        bool longRest = BadgeEquipped(Badge.BadgeType.LongRest);
        float longRestBoost = longRest ? (1 + 0.5f * BadgeEquippedCount(Badge.BadgeType.LongRest)) : 1;

        foreach (InnEffect i in pd.innEffects)
        {
            switch (i.innType)
            {
                case InnEffect.InnType.Health:
                    int hh = Mathf.CeilToInt(longRestBoost * maxHP / 5.0f);
                    if (hh < 4)
                    {
                        hh = 4;
                    }
                    HealHealth(hh);
                    break;
                case InnEffect.InnType.Energy:
                    int he = Mathf.CeilToInt(longRestBoost * BattleControl.Instance.GetMaxEP(this) / 5.0f);
                    if (he < 4)
                    {
                        he = 4;
                    }
                    HealEnergy(he);
                    break;
                case InnEffect.InnType.Absorb:
                    InflictEffect(this, new Effect(Effect.EffectType.Absorb, (sbyte)(longRestBoost * 3), Effect.INFINITE_DURATION));
                    break;
                case InnEffect.InnType.Stamina:
                    int hst = Mathf.CeilToInt(longRestBoost * BattleControl.Instance.GetMaxEP(this) / 10.0f);
                    if (hst < 2)
                    {
                        hst = 2;
                    }
                    HealStamina(hst);
                    break;
                case InnEffect.InnType.Burst:
                    InflictEffect(this, new Effect(Effect.EffectType.Burst, (sbyte)(longRestBoost * 3), Effect.INFINITE_DURATION));
                    break;
                case InnEffect.InnType.Focus:
                    InflictEffect(this, new Effect(Effect.EffectType.Focus, (sbyte)(longRestBoost * 3), Effect.INFINITE_DURATION));
                    break;
                case InnEffect.InnType.Ethereal:
                    InflictEffect(this, new Effect(Effect.EffectType.Ethereal, 1, (sbyte)(longRestBoost * 1)));
                    break;
                case InnEffect.InnType.Immunity:
                    InflictEffect(this, new Effect(Effect.EffectType.Immunity, 1, (sbyte)(longRestBoost * 3)));
                    break;
                case InnEffect.InnType.BonusTurn:
                    InflictEffect(this, new Effect(Effect.EffectType.BonusTurns, (sbyte)(longRestBoost * 1), Effect.INFINITE_DURATION));
                    break;
                case InnEffect.InnType.ItemBoost:
                    InflictEffect(this, new Effect(Effect.EffectType.ItemBoost, (sbyte)(longRestBoost * 2), Effect.INFINITE_DURATION));
                    break;
                case InnEffect.InnType.Illuminate:
                    InflictEffect(this, new Effect(Effect.EffectType.Illuminate, 1, (sbyte)(longRestBoost * 3)));
                    break;
                case InnEffect.InnType.Soul:
                    int hs = Mathf.CeilToInt(longRestBoost * BattleControl.Instance.GetMaxSE(this) / 5.0f);
                    if (hs < 4)
                    {
                        hs = 4;
                    }
                    HealSoulEnergy(hs); 
                    break;
                case InnEffect.InnType.Freebie:
                    InflictEffect(this, new Effect(Effect.EffectType.Freebie, (sbyte)(longRestBoost * 1), Effect.INFINITE_DURATION));
                    break;
            }
        }
    }

    public override void PreBattle()
    {
        if (BadgeEquipped(Badge.BadgeType.RiskyStart))
        {
            if (BadgeEquippedCount(Badge.BadgeType.RiskyStart) > 1)
            {
                hp = 1;
            } else
            {
                hp = DangerHP();
            }
        }

        ApplyInnEffects();


        bool charmPower = BadgeEquipped(Badge.BadgeType.CharmBoost);
        float charmBoost = 1;
        if (charmPower)
        {
            charmBoost = 1.0001f + (2 * BadgeEquippedCount(Badge.BadgeType.CharmBoost)) / 3f;
        }
        //apply charms if possible
        //(note: the duration countdown is in BattleControl after this executes (after all PreBattle scripts))
        PlayerData pd = BattleControl.Instance.playerData;
        for (int i = 0; i < pd.charmEffects.Count; i++)
        {
            if (pd.charmEffects[i].duration <= 1)
            {
                if (pd.charmEffects[i].charmType == CharmEffect.CharmType.Attack)
                {
                    InflictEffect(this, new Effect(Effect.EffectType.Focus, (sbyte)(charmBoost * 3), Effect.INFINITE_DURATION));
                }
                else if (pd.charmEffects[i].charmType == CharmEffect.CharmType.Defense)
                {
                    InflictEffect(this, new Effect(Effect.EffectType.Absorb, (sbyte)(charmBoost * 3), Effect.INFINITE_DURATION));
                }
            }
        }


        if (BadgeEquipped(Badge.BadgeType.FirstPower))
        {
            InflictEffect(this, new Effect(Effect.EffectType.Focus, (sbyte)(3 * BadgeEquippedCount(Badge.BadgeType.FirstPower)), Effect.INFINITE_DURATION));
        }
        if (BadgeEquipped(Badge.BadgeType.FirstShield))
        {
            InflictEffect(this, new Effect(Effect.EffectType.Absorb, (sbyte)(3 * BadgeEquippedCount(Badge.BadgeType.FirstShield)), Effect.INFINITE_DURATION));
        }
        if (BadgeEquipped(Badge.BadgeType.FirstEndurance))
        {
            InflictEffect(this, new Effect(Effect.EffectType.Burst, (sbyte)(3 * BadgeEquippedCount(Badge.BadgeType.FirstEndurance)), Effect.INFINITE_DURATION));
        }

        base.PreBattle();
    }

    public override IEnumerator PreMove()
    {
        if (!CanMove())
        {
            currMove = null;
        }

        //if (attackDamageCount > 0)
        //{
        //    attackThisTurn = true;
        //}

        attackThisTurn = false;

        attackHitCount = 0;
        chargedAttackCount = 0;
        damageEventsThisTurn = 0;
        absorbDamageEvents = 0;
        //Debug.Log(name + " reset");

        if (BadgeEquipped(Badge.BadgeType.RagesPower) || (BattleControl.Instance.GetEntitiesSorted((e) => (e.posId < 0))[0] == this && MainManager.Instance.GetGlobalFlag(GlobalFlag.GF_FileCode_Wrath)))
        {
            InflictEffectForce(this, new Effect(Effect.EffectType.Berserk, (sbyte)(BadgeEquippedCount(Badge.BadgeType.RagesPower)), Effect.INFINITE_DURATION));
        }

        if (currMove != null)
        {
            currMove.PreMove(this);
        }

        if (idleRunning)
        {
            StopCoroutine("Idle");
        }

        yield return null;
    }
    public override IEnumerator PostMove()
    {
        //reset the list
        while (contactImmunityList.Count > 0)
        {
            contactImmunityList.RemoveAt(0);
        }

        if (attackHitCount > 0)
        {
            attackLastTurn = true;
        } else
        {
            attackLastTurn = false;
        }

        protectiveRushPerTurn = false;
        hitLastTurn = hitThisTurn;
        hitThisTurn = false;


        actionCounter = 0;

        if (currMove != null)
        {
            currMove.PostMove(this);
        }

        //Status effect actions
        ValidateEffects();
        /*
        if (HasStatus(Status.StatusEffect.SoulVine))
        {
            BattleEntity target = BattleControl.Instance.GetEntityByID(GetStatusEntry(Status.StatusEffect.SoulVine).casterID);
            if (target != null)
            {
                TakeDamageStrict(1 * (int)MainManager.StatusLevelToPower(GetStatusEntry(Status.StatusEffect.SoulVine).potency));
                HealHealth(GetStatusEntry(Status.StatusEffect.SoulVine).casterID, 1 * (int)MainManager.StatusLevelToPower(GetStatusEntry(Status.StatusEffect.SoulVine).potency));
            }
            yield return new WaitForSeconds(0.5f);
        }
        */
        /*
        if (HasStatus(Status.StatusEffect.SoulVortex))
        {
            BattleEntity target = BattleControl.Instance.GetEntityByID(GetStatusEntry(Status.StatusEffect.SoulVortex).casterID);
            if (target != null)
            {
                target.soulVortexDef = GetStatusEntry(Status.StatusEffect.SoulVortex).potency * 2;
            }
        }
        */


        //item passives (unused)
        bool isFront = (BattleControl.Instance.GetPlayerEntities()[0] == this);
        int passiveHP = Item.CountItemsWithProperty(Item.ItemProperty.Passive_HPRegen, BattleControl.Instance.GetItemInventory(this));
        int passiveMHP = Item.CountItemsWithProperty(Item.ItemProperty.Passive_HPLoss, BattleControl.Instance.GetItemInventory(this));
        if (passiveHP - passiveMHP != 0)
        {
            HealHealth(passiveHP - passiveMHP);
        }
        if (isFront)
        {
            int passiveEP = Item.CountItemsWithProperty(Item.ItemProperty.Passive_EPRegen, BattleControl.Instance.GetItemInventory(this));
            int passiveMEP = Item.CountItemsWithProperty(Item.ItemProperty.Passive_EPLoss, BattleControl.Instance.GetItemInventory(this));
            if (passiveEP - passiveMEP != 0)
            {
                HealEnergy(passiveEP - passiveMEP);
            }
            int passiveSE = Item.CountItemsWithProperty(Item.ItemProperty.Passive_SERegen, BattleControl.Instance.GetItemInventory(this));
            int passiveMSE = Item.CountItemsWithProperty(Item.ItemProperty.Passive_SELoss, BattleControl.Instance.GetItemInventory(this));
            if (passiveSE - passiveMSE != 0)
            {
                HealSoulEnergy(passiveSE - passiveMSE);
            }
        }


        bool effectStasis = false;
        if (HasEffect(Effect.EffectType.EffectStasis))
        {
            effectStasis = true;
        }
        bool preservative = false;
        if (HasEffect(Effect.EffectType.Freeze) && BadgeEquipped(Badge.BadgeType.Preservative))
        {
            preservative = true;
        }

        bool statusDamage = false;

        if (hp > 0)
        {

            if (HasEffect(Effect.EffectType.Poison))
            {
                statusDamage = true;
                int poisonDamage = Mathf.CeilToInt(maxHP / 10.0f);

                if (BadgeEquipped(Badge.BadgeType.ToxicEnergy))
                {
                    int tecount = BadgeEquippedCount(Badge.BadgeType.ToxicEnergy);
                    //poisonDamage = Mathf.CeilToInt(maxHP / 6.66f);
                    //poisonDamage = (int)(poisonDamage * (1 + 0.5f * tecount));

                    //Wacky formula I guess
                    poisonDamage = Mathf.CeilToInt(maxHP / (10 / (1 + 0.5f * tecount)));

                    if (poisonDamage < 2 + tecount)
                    {
                        poisonDamage = 2 + tecount;
                    }

                    if (poisonDamage > 10 + 5 * tecount)
                    {
                        poisonDamage = 10 + 5 * tecount;
                    }
                    HealEnergy(3 * tecount);
                    yield return new WaitForSeconds(0.5f);
                }
                else
                {
                    if (poisonDamage < 2)
                    {
                        poisonDamage = 2;
                    }

                    if (poisonDamage > 10)
                    {
                        poisonDamage = 10;
                    }
                }

                if (BadgeEquipped(Badge.BadgeType.ToxicResistance))
                {
                    if (BattleControl.Instance.GetEP(this) > 0)
                    {
                        HealEnergy(-poisonDamage * GetEffectEntry(Effect.EffectType.Poison).potency);
                    }
                    else
                    {
                        TakeDamageStatus(poisonDamage * GetEffectEntry(Effect.EffectType.Poison).potency);
                    }
                }
                else
                {
                    TakeDamageStatus(poisonDamage * GetEffectEntry(Effect.EffectType.Poison).potency);
                }
                yield return new WaitForSeconds(0.5f);
            }
            if (HasEffect(Effect.EffectType.Sunflame))
            {
                statusDamage = true;
                int sfDamage = maxHP / 10;

                int powermult = 1 + GetEffectEntry(Effect.EffectType.Sunflame).potency;

                if (sfDamage < 2)
                {
                    sfDamage = 2;
                }

                if (sfDamage > 10)
                {
                    sfDamage = 10;
                }

                TakeDamageStatus((int)(sfDamage * powermult * 0.5f));
                yield return new WaitForSeconds(0.5f);
            }
            if (HasEffect(Effect.EffectType.DamageOverTime))
            {
                TakeDamageStatus(GetEffectEntry(Effect.EffectType.DamageOverTime).potency);
                yield return new WaitForSeconds(0.5f);
            }
            if (HasEffect(Effect.EffectType.HealthLoss))
            {
                //statusDamage = true;
                //TakeDamageStatus(GetStatusEntry(Status.StatusEffect.HealthLoss).potency);

                //subtly different from damage
                HealHealth(-1 * GetEffectEntry(Effect.EffectType.HealthLoss).potency);
                yield return new WaitForSeconds(0.5f);
            }
            if (RibbonEquipped(Ribbon.RibbonType.ThornyRibbon, false))
            {
                TakeDamageStatus(1);
            }
            if (HasEffect(Effect.EffectType.HealthRegen))
            {
                HealHealth(GetEffectEntry(Effect.EffectType.HealthRegen).potency);
                yield return new WaitForSeconds(0.5f);
            }
            if (HasEffect(Effect.EffectType.Sleep))
            {
                int sleepHeal = Mathf.CeilToInt(maxHP / (10f));
                int dscount = BadgeEquippedCount(Badge.BadgeType.DeepSleep);
                
                if (BadgeEquipped(Badge.BadgeType.DeepSleep))
                {
                    sleepHeal = Mathf.CeilToInt(maxHP / (10f / (1 + 0.5f * dscount)));
                    if (sleepHeal < 4)
                    {
                        sleepHeal = 2;
                    }

                    if (sleepHeal > 20)
                    {
                        sleepHeal = 20;
                    }
                }
                else
                {
                    if (sleepHeal < 2)
                    {
                        sleepHeal = 2;
                    }

                    if (sleepHeal > 10)
                    {
                        sleepHeal = 10;
                    }
                }

                if (BadgeEquipped(Badge.BadgeType.SweetDreams))
                {
                    HealEnergy(sleepHeal * GetEffectEntry(Effect.EffectType.Sleep).potency);
                }
                else
                {
                    HealHealth(sleepHeal * GetEffectEntry(Effect.EffectType.Sleep).potency);
                }
                yield return new WaitForSeconds(0.5f);
            }
            if (HasEffect(Effect.EffectType.EnergyRegen))
            {
                HealEnergy(GetEffectEntry(Effect.EffectType.EnergyRegen).potency);
                yield return new WaitForSeconds(0.5f);
            }
            if (HasEffect(Effect.EffectType.EnergyLoss))
            {
                RemoveEnergy(GetEffectEntry(Effect.EffectType.EnergyLoss).potency);
                yield return new WaitForSeconds(0.5f);
            }

            if (HasEffect(Effect.EffectType.SoulRegen))
            {
                HealEnergy(GetEffectEntry(Effect.EffectType.SoulRegen).potency);
                yield return new WaitForSeconds(0.5f);
            }
            if (HasEffect(Effect.EffectType.SoulLoss))
            {
                RemoveEnergy(GetEffectEntry(Effect.EffectType.SoulLoss).potency);
                yield return new WaitForSeconds(0.5f);
            }

            if (HasEffect(Effect.EffectType.Hustle))
            {
                InflictEffectForce(this, new Effect(Effect.EffectType.BonusTurns, GetEffectEntry(Effect.EffectType.Hustle).potency, Effect.INFINITE_DURATION));
            }
            if (HasEffect(Effect.EffectType.Swift))
            {
                InflictEffectForce(this, new Effect(Effect.EffectType.BonusTurns, GetEffectEntry(Effect.EffectType.Swift).potency, Effect.INFINITE_DURATION));
            }

            int slowPower = 1;

            if (HasEffect(Effect.EffectType.Sluggish))
            {
                slowPower += GetEffectEntry(Effect.EffectType.Sluggish).potency;
            }
            if (HasEffect(Effect.EffectType.Slow))
            {
                slowPower += GetEffectEntry(Effect.EffectType.Slow).potency;
            }
            if (slowPower > 1 && BattleControl.Instance.turnCount % (slowPower) != 0)
            {
                InflictEffectForce(this, new Effect(Effect.EffectType.Cooldown, 1, Effect.INFINITE_DURATION));
            }

            if (HasEffect(Effect.EffectType.Illuminate) && attackHitCount > 0)
            {
                Effect ee = GetEffectEntry(Effect.EffectType.Illuminate);
                ee.potency++;
                if (ee.potency >= Effect.ILLUMINATE_CAP)
                {
                    ee.potency = Effect.ILLUMINATE_CAP;
                }
            }
            if (HasEffect(Effect.EffectType.Sunflame))
            {
                Effect ee = GetEffectEntry(Effect.EffectType.Sunflame);
                ee.potency++;
                if (ee.potency >= Effect.ILLUMINATE_CAP)
                {
                    ee.potency = Effect.ILLUMINATE_CAP;
                }
            }

            if (HasEffect(Effect.EffectType.AstralWall) && astralWallTrackedDamage >= GetEffectEntry(Effect.EffectType.AstralWall).potency)
            {
                RemoveEffect(Effect.EffectType.AstralWall);
            }
        }


        BattleControl.Instance.playerData.GetPlayerDataEntry(entityID).UpdateMaxDamageDealt(perTurnDamageDealt);
        BattleControl.Instance.playerData.UpdateMaxDamageDealt(perTurnDamageDealt);
        perTurnDamageDealt = 0;

        damageTakenLastTurn = damageTakenThisTurn;
        counterFlareDamage = 0;
        counterFlareTrackedDamage = 0;
        arcDischargeDamage = 0;
        arcDischargeTrackedDamage = 0;
        splotchDamage = 0;
        splotchTrackedDamage = 0;
        damageTakenThisTurn = 0;
        astralWallTrackedDamage = 0;


        //badge stuff

        if (hp > 0)
        {
            int hpregen = 0;
            int epregen = 0;
            int seregen = 0;

            if (BadgeEquipped(Badge.BadgeType.SoftPower) && BadgeEquipped(Badge.BadgeType.MetalPower))
            {
                hpregen += 2 * BadgeEquippedCount(Badge.BadgeType.SoftPower);
            }

            hpregen += BadgeEquippedCount(Badge.BadgeType.HealthRegen);
            /*
            if (BadgeEquipped(Badge.BadgeType.HealthRegen))
            {
                hpregen++;
            }
            if (BadgeEquipped(Badge.BadgeType.HealthRegenB))
            {
                hpregen++;
            }
            */
            if (hpregen != 0)
            {
                HealHealth(hpregen);
                yield return new WaitForSeconds(0.5f);
            }
            epregen += BadgeEquippedCount(Badge.BadgeType.EnergyRegen);

            if (HasEffect(Effect.EffectType.Paralyze) && BadgeEquipped(Badge.BadgeType.Generator))
            {
                epregen += Mathf.CeilToInt(BattleControl.Instance.GetMaxEP(this) / 20.0f) * BadgeEquippedCount(Badge.BadgeType.Generator);
                if ((BattleControl.Instance.GetMaxEP(this) / 20) < 1)
                {
                    epregen += 1;
                }
            }

            /*
            if (BadgeEquipped(Badge.BadgeType.EnergyRegen))
            {
                epregen++;
            }
            if (BadgeEquipped(Badge.BadgeType.EnergyRegenB))
            {
                epregen++;
            }
            */
            if (epregen != 0)
            {
                HealEnergy(epregen);
                yield return new WaitForSeconds(0.5f);
            }
            if (BadgeEquipped(Badge.BadgeType.SoulRegen))
            {
                seregen += 2 * BadgeEquippedCount(Badge.BadgeType.SoulRegen);
                //HealSoulEnergy(1);
                //yield return new WaitForSeconds(0.5f);
            }

            if (HasEffect(Effect.EffectType.Dizzy) && BadgeEquipped(Badge.BadgeType.Trance))
            {
                int soulHeal = Mathf.CeilToInt(0.001f + (BattleControl.Instance.GetSE(this) / (20.0f / BadgeEquippedCount(Badge.BadgeType.Trance))));
                if (soulHeal < 1)
                {
                    soulHeal = 1;
                }
                seregen += soulHeal;
                //HealSoulEnergy(soulHeal);
                //yield return new WaitForSeconds(0.5f);
            }

            if (seregen != 0)
            {
                HealSoulEnergy(seregen);
                yield return new WaitForSeconds(0.5f);
            }


            if (HasEffect(Effect.EffectType.Poison) && BadgeEquipped(Badge.BadgeType.ToxicStrength))
            {
                InflictEffect(this, new Effect(Effect.EffectType.Sunder, (sbyte)(2 * BadgeEquippedCount(Badge.BadgeType.ToxicStrength)), Effect.INFINITE_DURATION));
                yield return new WaitForSeconds(0.5f);
            }

            if (HasEffect(Effect.EffectType.Paralyze) && BadgeEquipped(Badge.BadgeType.Capacitor))
            {
                InflictEffect(this, new Effect(Effect.EffectType.Focus, (sbyte)(2 * BadgeEquippedCount(Badge.BadgeType.Capacitor)), Effect.INFINITE_DURATION));
                yield return new WaitForSeconds(0.5f);
            }

            if (HasEffect(Effect.EffectType.Freeze) && BadgeEquipped(Badge.BadgeType.Glacier))
            {
                InflictEffect(this, new Effect(Effect.EffectType.Focus, (sbyte)(2 * BadgeEquippedCount(Badge.BadgeType.Glacier)), Effect.INFINITE_DURATION));
                yield return new WaitForSeconds(0.5f);
            }


            //moved this down here so you can get more benefit from regen
            if (hp >= maxHP && BadgeEquipped(Badge.BadgeType.HealthGrowth))
            {
                InflictEffect(this, new Effect(Effect.EffectType.Absorb, (sbyte)(2 * BadgeEquippedCount(Badge.BadgeType.HealthGrowth)), Effect.INFINITE_DURATION));
                yield return new WaitForSeconds(0.5f);
            }
            if (BattleControl.Instance.GetEP(this) >= BattleControl.Instance.GetMaxEP(this) && BadgeEquipped(Badge.BadgeType.EnergyGrowth))
            {
                InflictEffect(this, new Effect(Effect.EffectType.Burst, (sbyte)(2 * BadgeEquippedCount(Badge.BadgeType.EnergyGrowth)), Effect.INFINITE_DURATION));
                yield return new WaitForSeconds(0.5f);
            }

            //health balance
            if (BadgeEquipped(Badge.BadgeType.HealthBalance))
            {
                int healthBalance;
                float proportion = (hp / (0.0f + maxHP));

                PlayerEntity other = null;
                List<PlayerEntity> players = BattleControl.Instance.GetPlayerEntities();
                for (int i = 0; i < players.Count; i++)
                {
                    if (players[i] != this)
                    {
                        other = players[i];
                        break;
                    }
                }

                //if ally is dead don't try to health balance (Free revives = bad!!!)
                if (other.hp > 0)
                {
                    float otherProportion = (other.hp) / (0.0f + other.maxHP);
                    //calculate target hp

                    //incorrect
                    //int targetHP = Mathf.CeilToInt(((proportion + otherProportion) / 2) * maxHP);

                    //Correct
                    //(hp diff such that proportion = otherProportion)
                    //Your hp after this = (other.hp + hp) * ((maxHP) / (maxHP + other.maxHP))

                    int targetHP = Mathf.CeilToInt((other.hp + hp) * (maxHP / (maxHP + other.maxHP + 0f)));

                    healthBalance = targetHP - hp;
                    int value = 5 * BadgeEquippedCount(Badge.BadgeType.HealthBalance);
                    if (healthBalance > value)
                    {
                        healthBalance = value;
                    }
                    if (healthBalance < -value)
                    {
                        healthBalance = -value;
                    }
                    if (healthBalance != 0)
                    {
                        HealHealth(healthBalance);
                        other.HealHealth(-healthBalance);
                    }
                    yield return new WaitForSeconds(0.5f);
                }
            }
        }



        bool charmPower = BadgeEquipped(Badge.BadgeType.CharmBoost);
        float charmBoost = 1;
        if (charmPower)
        {
            charmBoost =  1.0001f + (2 * BadgeEquippedCount(Badge.BadgeType.CharmBoost)) / 3;
        }
        //apply charms if possible
        //tick down charm effects
        //(note: the duration countdown is in BattleControl after this executes (after all PostMove scripts))
        PlayerData pd = BattleControl.Instance.playerData;
        for (int i = 0; i < pd.charmEffects.Count; i++)
        {
            if (pd.charmEffects[i].duration <= 1)
            {
                if (pd.charmEffects[i].charmType == CharmEffect.CharmType.Attack)
                {
                    InflictEffect(this, new Effect(Effect.EffectType.Focus, (sbyte)(charmBoost * 3), Effect.INFINITE_DURATION));
                }
                else if (pd.charmEffects[i].charmType == CharmEffect.CharmType.Defense)
                {
                    InflictEffect(this, new Effect(Effect.EffectType.Absorb, (sbyte)(charmBoost * 3), Effect.INFINITE_DURATION));
                }
            }
        }

        //decrement turn count of effects
        for (int i = 0; i < effects.Count; i++)
        {
            if (effects[i].duration != Effect.INFINITE_DURATION)
            {
                bool applyStasis = (effectStasis || preservative) && effects[i].duration <= 1;
                applyStasis &= (!effectStasis || effects[i].effect != Effect.EffectType.EffectStasis);
                if (!effectStasis)
                {
                    applyStasis &= (!preservative || effects[i].effect != Effect.EffectType.Freeze);
                }
                if (!applyStasis)
                {
                    effects[i].duration--;
                }
            }
        }


        //Enviro effects
        //Immediately after the tick down so the durations shown match what you see
        bool enviroStrong = BattleControl.Instance.HarderEnviroEffects();
        float enviroPower = BattleControl.Instance.EnviroEffectPower();

        int every1turn = BattleControl.Instance.EnviroEveryXTurns(1, enviroPower);
        int every2turn = BattleControl.Instance.EnviroEveryXTurns(2, enviroPower);
        int every3turn = BattleControl.Instance.EnviroEveryXTurns(3, enviroPower);

        switch (BattleControl.Instance.enviroEffect)
        {
            case EnvironmentalEffect.ElectricWind:
                if (BattleControl.Instance.GetFrontmostAlly(posId) == this)
                {
                    if (enviroStrong)
                    {
                        //One every 2 turns
                        if (every2turn > 0)
                        {
                            HealEnergy(-every2turn);
                        }
                    }
                    else
                    {
                        //One every 1 turn
                        if (every1turn > 0)
                        {
                            HealEnergy(-every1turn);
                        }
                    }
                }
                break;
            case EnvironmentalEffect.SeasideAir:
                break;
            case EnvironmentalEffect.ScaldingHeat:
                if (enviroStrong)
                {
                    if (every2turn > 0)
                    {
                        InflictEffect(this, new Effect(Effect.EffectType.DefenseDown, (sbyte)every2turn, Effect.INFINITE_DURATION), Effect.NULL_CASTERID, Effect.EffectStackMode.KeepDurAddPot);
                    }
                }
                else
                {
                    if (every2turn > 0)
                    {
                        InflictEffect(this, new Effect(Effect.EffectType.DefenseDown, (sbyte)every2turn, 2));
                    }
                }
                break;
            case EnvironmentalEffect.DarkFog:
                if (enviroStrong)
                {
                    if (every3turn > 0)
                    {
                        InflictEffect(this, new Effect(Effect.EffectType.EnduranceDown, (sbyte)every3turn, Effect.INFINITE_DURATION), Effect.NULL_CASTERID, Effect.EffectStackMode.KeepDurAddPot);
                    }
                }
                else
                {
                    if (every3turn > 0)
                    {
                        InflictEffect(this, new Effect(Effect.EffectType.EnduranceDown, (sbyte)every3turn, 3));
                    }
                }
                break;
            case EnvironmentalEffect.FrigidBreeze:
                if (enviroStrong)
                {
                    if (every2turn > 0)
                    {
                        InflictEffect(this, new Effect(Effect.EffectType.AttackDown, (sbyte)every2turn, Effect.INFINITE_DURATION), Effect.NULL_CASTERID, Effect.EffectStackMode.KeepDurAddPot);
                    }
                }
                else
                {
                    if (every2turn > 0)
                    {
                        InflictEffect(this, new Effect(Effect.EffectType.AttackDown, (sbyte)every2turn, 2));
                    }
                }
                break;
            case EnvironmentalEffect.AetherHunger:
                if (BattleControl.Instance.GetFrontmostAlly(posId) == this)
                {
                    if (enviroStrong)
                    {
                        //One every 2 turns
                        if (every2turn > 0)
                        {
                            HealSoulEnergy(-every2turn * 3);
                        }
                    }
                    else
                    {
                        //One every 1 turn
                        if (every1turn > 0)
                        {
                            HealSoulEnergy(-every1turn * 3);
                        }
                    }
                }
                if (enviroStrong)
                {
                    //One every 2 turns
                    if (every2turn > 0)
                    {
                        HealHealth(-every2turn);
                    }
                }
                else
                {
                    //One every 1 turn
                    if (every1turn > 0)
                    {
                        HealHealth(-every1turn);
                    }
                }
                break;
            case EnvironmentalEffect.DigestiveAcid:
                if (BattleControl.Instance.GetFrontmostAlly(posId) == this)
                {
                    if (enviroStrong)
                    {
                        //One every 2 turns
                        if (every2turn > 0)
                        {
                            HealSoulEnergy(-every2turn * 6);
                        }
                    }
                    else
                    {
                        //One every 1 turn
                        if (every1turn > 0)
                        {
                            HealSoulEnergy(-every1turn * 6);
                        }
                    }
                }
                if (enviroStrong)
                {
                    //One every 2 turns
                    if (every2turn > 0)
                    {
                        HealHealth(-every2turn * 3);
                    }
                }
                else
                {
                    //One every 1 turn
                    if (every1turn > 0)
                    {
                        HealHealth(-every1turn * 3);
                    }
                }
                break;
            case EnvironmentalEffect.AcidFlow:
                if (BattleControl.Instance.GetFrontmostAlly(posId) == this)
                {
                    if (enviroStrong)
                    {
                        //One every 1 turn
                        if (every1turn > 0)
                        {
                            HealSoulEnergy(-every1turn * 6);
                        }
                    }
                    else
                    {
                        //One every 1 turn
                        if (every1turn > 0)
                        {
                            HealSoulEnergy(-every1turn * 12);
                        }
                    }
                }
                if (enviroStrong)
                {
                    //One every 1 turn
                    if (every1turn > 0)
                    {
                        HealHealth(-every1turn * 3);
                        HealSoulEnergy(-every1turn * 6);
                    }
                }
                else
                {
                    //One every 1 turn
                    if (every1turn > 0)
                    {
                        HealHealth(-every1turn * 6);
                        HealSoulEnergy(-every1turn * 12);
                    }
                }
                break;
            case EnvironmentalEffect.SacredGrove:
                break;
            case EnvironmentalEffect.IonizedSand:
                break;
            case EnvironmentalEffect.WhiteoutBlizzard:
                break;
            case EnvironmentalEffect.VoidShadow:
                break;
            case EnvironmentalEffect.CounterWave:
                break;
            case EnvironmentalEffect.ScaldingMagma:
                break;
            case EnvironmentalEffect.TrialOfSimplicity:
                break;
            case EnvironmentalEffect.TrialOfHaste:
                break;
            case EnvironmentalEffect.TrialOfResolve:
                break;
            case EnvironmentalEffect.TrialOfAmbition:
                break;
            case EnvironmentalEffect.TrialOfPatience:
                break;
            case EnvironmentalEffect.TrialOfZeal:
                break;
        }

        ValidateEffects();

        ApplyBufferedEffects();

        //wait on the events
        while (inEvent)
        {
            yield return null;
        }

        //negate Charge if needed

        if (chargedAttackCount > 0)
        {
            chargedAttackCount = 0;
            if (bufferRemoveCharge)
            {
                bufferRemoveCharge = false;
            }
            else
            {
                if (BadgeEquipped(Badge.BadgeType.FocusRecycling))
                {
                    int count = BadgeEquippedCount(Badge.BadgeType.FocusRecycling);
                    Effect e = GetEffectEntry(Effect.EffectType.Focus);
                    if (e != null)
                    {
                        e.potency = (sbyte)(e.potency * ((count + 0.0001f) / (count + 1f)));
                        if (e.potency == 0)
                        {
                            RemoveEffect(Effect.EffectType.Focus);
                        }
                    }
                } else
                {
                    RemoveEffect(Effect.EffectType.Focus);
                }
                RemoveEffect(Effect.EffectType.Defocus);
            }
        }
        if (bufferRemoveCharge)
        {
            bufferRemoveCharge = false;
        }

        //check everything right after your turn

        //enforce this check
        //note that the damaging statuses trigger their effects already so no deathcheck
        if (!statusDamage)
        {
            DeathCheck();
        }

        //Fix a problem I'm having
        SetIdleAnimation();

        if (!idleRunning && hasIdle && idleActive)
        {
            StartCoroutine("Idle");
        }
    }

    public override IEnumerator PostBattle()
    {
        //in this case PostMove won't be run, so do this now
        BattleControl.Instance.playerData.GetPlayerDataEntry(entityID).UpdateMaxDamageDealt(perTurnDamageDealt);
        BattleControl.Instance.playerData.UpdateMaxDamageDealt(perTurnDamageDealt);

        if (hp > 0)
        {
            if (BadgeEquipped(Badge.BadgeType.VictoryHeal))
            {
                HealHealth(3 * BadgeEquippedCount(Badge.BadgeType.VictoryHeal));
                yield return new WaitForSeconds(0.5f);
            }

            if (BadgeEquipped(Badge.BadgeType.VictorySurge))
            {
                HealEnergy(3 * BadgeEquippedCount(Badge.BadgeType.VictorySurge));
                yield return new WaitForSeconds(0.5f);
            }
        }

        yield return null;
    }


    public virtual bool BadgeEquipped(Badge.BadgeType badge)
    {
        return BattleControl.Instance.playerData.BadgeEquippedFull(badge, entityID);
    }
    public virtual int BadgeEquippedCount(Badge.BadgeType badge)
    {
        return BattleControl.Instance.playerData.BadgeEquippedCountFull(badge, entityID);
    }

    public virtual Ribbon GetVisualRibbon()
    {
        return BattleControl.Instance.playerData.GetVisualRibbon(entityID);
    }
    public virtual bool RibbonEquipped(Ribbon.RibbonType ribbon, bool rainbowWildcard = false)
    {
        return BattleControl.Instance.playerData.GetRibbonEquipped(ribbon, entityID, rainbowWildcard);
    }

    public int GetFocusCap()
    {
        if (MainManager.Instance.Cheat_NoEffectCaps)
        {
            return Effect.INFINITE_POTENCY;
        }
        return BattleControl.Instance.playerData.focusCap;
    }
    public int GetAbsorbCap()
    {
        if (MainManager.Instance.Cheat_NoEffectCaps)
        {
            return Effect.INFINITE_POTENCY;
        }
        return BattleControl.Instance.playerData.absorbCap;
    }
    public int GetBurstCap()
    {
        if (MainManager.Instance.Cheat_NoEffectCaps)
        {
            return Effect.INFINITE_POTENCY;
        }
        return BattleControl.Instance.playerData.burstCap;
    }


    //rewrite some parts
    public override IEnumerator DoEvent(BattleHelper.Event eventID)
    {
        if (idleRunning)
        {
            StopCoroutine("Idle");
        }

        inEvent = true;

        IsAlive();

        //Debug.Log(eventID);

        BattleControl.Instance.BroadcastEvent(this, eventID);

        //check in case stuff changes
        switch (eventID)
        {
            case BattleHelper.Event.Hurt:
            case BattleHelper.Event.ComboHurt:
            case BattleHelper.Event.StatusInflicted:    //? needed to fix something
                if (!dead)
                {
                    if (lastHitWasBlocked)
                    {
                        SetAnimation("block", true);
                    }
                    else
                    {
                        SetAnimation("hurt", true);
                    }
                   
                    IEnumerator animReset()
                    {
                        yield return new WaitForSeconds(0.6f);
                        if (!moveActive && alive && !dead && ac.timeSinceLastAnimChange >= 0.6f - Time.deltaTime)// && (ac.GetCurrentAnim().Equals("hurt") || ac.GetCurrentAnim().Equals("block")))
                        {
                            SetIdleAnimation();
                        }
                    }
                    StartCoroutine(animReset());
                }
                break;
            case BattleHelper.Event.StatusDeath:
            case BattleHelper.Event.Death:
                alive = false;
                //BattleControl.Instance.RemoveEntity(BattleControl.Instance.GetIndexFromID(id)); //create consistent death timing
                if (!GetEntityProperty(EntityProperties.KeepEffectsAtNoHP))
                {
                    CureDeathCurableEffects();
                }
                if (!dead)
                {
                    SetAnimation("hurt", true);
                    yield return StartCoroutine(Spin(Vector3.up * 360, 0.5f));
                    yield return StartCoroutine(Move(homePos, 10,false)); //so you die from counters properly
                    yield return StartCoroutine(Spin(Vector3.left * 90, 0.125f));
                }
                SetRotation(Vector3.zero);
                SetAnimation("dead",true);
                yield return new WaitForSeconds(0.2f);
                //on death stuff
                if (!dead)
                {
                    OnDeathEffects();
                }
                dead = true;
                staminaBlock = false;
                break;
            case BattleHelper.Event.KnockbackHurt:
                yield return StartCoroutine(DefaultKnockbackHurt(false));
                break;
            case BattleHelper.Event.MetaKnockbackHurt:
                yield return StartCoroutine(DefaultKnockbackHurt(true));
                break;
            case BattleHelper.Event.Revive:
                dead = false;
                staminaBlock = false;
                SetRotation(Vector3.zero);
                yield return StartCoroutine(Jump(homePos, 0.5f, 0.25f));
                //force rage's power to give you the effect
                if (BadgeEquipped(Badge.BadgeType.RagesPower) || (BattleControl.Instance.GetEntitiesSorted((e) => (e.posId < 0))[0] == this && MainManager.Instance.GetGlobalFlag(GlobalFlag.GF_FileCode_Wrath)))
                {
                    InflictEffectForce(this, new Effect(Effect.EffectType.Berserk, (sbyte)(BadgeEquippedCount(Badge.BadgeType.RagesPower)), Effect.INFINITE_DURATION));
                }
                break;
            case BattleHelper.Event.CureStatus:
                //No
                if (BadgeEquipped(Badge.BadgeType.RagesPower) || (BattleControl.Instance.GetEntitiesSorted((e) => (e.posId < 0))[0] == this && MainManager.Instance.GetGlobalFlag(GlobalFlag.GF_FileCode_Wrath)))
                {
                    InflictEffectForce(this, new Effect(Effect.EffectType.Berserk, (sbyte)(BadgeEquippedCount(Badge.BadgeType.RagesPower)), Effect.INFINITE_DURATION));
                }
                break;
        }
        yield return null;

        //BattleControl.Instance.BroadcastEvent(this, eventID);

        inEvent = false;

        //by default the idle script is stopped
        if (!idleRunning && hasIdle && idleActive)
        {
            StartCoroutine("Idle");
        }
    }
    public void OnDeathEffects()
    {
        //Grab the other player
        PlayerEntity other = null;
        List<PlayerEntity> players = BattleControl.Instance.GetPlayerEntities();
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i] != this)
            {
                other = players[i];
                break;
            }
        }

        if (other != null && other.alive && other.BadgeEquipped(Badge.BadgeType.VengefulRage))
        {
            InflictEffect(other, new Effect(Effect.EffectType.Berserk, 1, 2));
            InflictEffect(other, new Effect(Effect.EffectType.Focus, (sbyte)(3 * BadgeEquippedCount(Badge.BadgeType.VengefulRage)), Effect.INFINITE_DURATION));
        }

        if (BadgeEquipped(Badge.BadgeType.LastBurst))
        {
            int heal = Mathf.CeilToInt((BadgeEquippedCount(Badge.BadgeType.LastBurst) * BattleControl.Instance.GetMaxEP(this)) / 2.0f);
            HealEnergy(heal);
        }

        //logic
        stamina = 0;
    }
    public override void Update()
    {
        base.Update();
        if (blockFrames == -2)
        {
            blockFrames = -1;
        }
        if (sblockFrames == -2)
        {
            sblockFrames = -1;
        }
        //-1 is not blocking (default state)

        //make absolutely sure you aren't blocking when you really shouldn't be able to
        if (moveExecuting)
        {
            blockFrames = -1;
            sblockFrames = -1;
        }
        

        //Track frames after pressing block button up to 60 (if you somehow get a full second block window, you basically can never miss if you try)
        if (blockFrames >= 0 && blockFrames < 60)
        {
            blockFrames += Time.deltaTime * 60;
        }
        if (sblockFrames >= 0 && sblockFrames < 60)
        {
            sblockFrames += Time.deltaTime * 60;
        }

        //allow blocking
        if (!moveExecuting && InputManager.GetButtonDown(InputManager.Button.A) && (blockFrames < 0 || blockFrames >= BASE_GUARD_COOLDOWN))
        {
            blockFrames = 0;
        }

        //Idea
        if (!moveExecuting && InputManager.GetButtonDown(InputManager.Button.A) && (blockFrames > 0 && blockFrames <= BASE_GUARD_COOLDOWN) && (sblockFrames < 0 || sblockFrames >= BASE_GUARD_COOLDOWN))
        {
            sblockFrames = 0;
        }

        //if (!moveExecuting && MainManager.GetButtonDown(InputManager.Button.B) && (sblockFrames < 0 || sblockFrames >= BASE_GUARD_COOLDOWN))
        //{
        //    sblockFrames = 0;
        //}
    }

    public override void SetIdleAnimation(bool force = false)
    {
        BattleControl.Instance.playerData.GetPlayerDataEntry(entityID).hp = hp;
        if (dead)
        {
            SetAnimation("dead", force);
            return;
        }
        if (!alive)
        {
            //Don't do anything?
            //!alive and !dead is a transition state?
            return;
        }
        if (HasEffect(Effect.EffectType.Freeze) || HasEffect(Effect.EffectType.TimeStop))
        {
            SetAnimation("idlefrozen", force);
        }
        else if (HasEffect(Effect.EffectType.Sleep))
        {
            SetAnimation("idlesleep", force);
        }
        else if (HasEffect(Effect.EffectType.Dizzy) || HasEffect(Effect.EffectType.Dread))
        {
            SetAnimation("idledizzy", force);
        }
        else if (HasEffect(Effect.EffectType.Berserk) || HasEffect(Effect.EffectType.Sunflame))
        {
            SetAnimation("idleangry", force);
        }
        else if (ShowDangerAnim() || HasEffect(Effect.EffectType.Poison) || HasEffect(Effect.EffectType.Paralyze) || HasEffect(Effect.EffectType.Soulbleed) || HasEffect(Effect.EffectType.Exhausted))
        {
            SetAnimation("idleweak", force);
        }
        else
        {
            SetAnimation("idle", force);
        }
    }

    public void SetScanTable(string s)
    {
        scanTable = s;
    }
    public override string RequestTextData(string request)
    {
        //Debug.Log("Request result " + scanTable);
        return scanTable;
    }
}
