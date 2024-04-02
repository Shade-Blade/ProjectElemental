using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheatMenu : MenuHandler
{
    string currString;

    bool cancel;

    public GameObject baseObject;
    public CheatMenuScript cs;

    public override event EventHandler<MenuExitEventArgs> menuExit;

    public static CheatMenu BuildMenu(string baseString = null)
    {
        GameObject newObj = new GameObject("Cheat Menu");
        CheatMenu newMenu = newObj.AddComponent<CheatMenu>();
        newMenu.currString = baseString;
        newMenu.Init();
        return newMenu;
    }

    public override void Init()
    {
        active = true;
        cancel = false;
        InputManager.Instance.disableControl = true;

        //appears at the bottom of supercanvas's children so it displays on top
        baseObject = Instantiate((GameObject)Resources.Load("Menu/CheatMenu"), MainManager.Instance.SuperCanvas.transform);
        cs = baseObject.GetComponent<CheatMenuScript>();

        cs.text.SetText("~" + currString, true, true);
        cs.log.SetText("", true, true);
    }
    public override void Clear()
    {
        active = false;
        Destroy(baseObject);
    }

    void Update()
    {
        if (active)
        {
            MenuUpdate();
        }
    }

    public void OnGUI()
    {
        if (!active)
        {
            return;
        }

        Event e = Event.current;

        if (e.type == EventType.KeyDown)
        {
            if (e.character == '~')
            {
                SelectOption();
            }


            if (e.isKey)
            {
                if (e.character != '\0' && e.character != '\n' && e.character != '\t' && e.character != '\r')   //ban a few characters specifically
                {
                    currString = currString + e.character;
                }
            }

            if (e.keyCode == KeyCode.Backspace)
            {
                if (currString != null && currString.Length > 0)
                {
                    currString = currString.Substring(0, currString.Length - 1);
                } else
                {
                    Cancel();
                }
            }

            cs.text.SetText("~" + currString, true, true);
            //cs.log.SetText("", true, true);

            if (e.keyCode == KeyCode.Escape)
            {
                Cancel();
            }

            if (e.keyCode == KeyCode.Return)
            {
                SelectOption();
            }
        }
    }

    public void MenuUpdate()
    {
        //???
    }

    public void SelectOption()
    {
        //this will also execute the cheat immediately
        MenuResult mr = GetFullResult();
        string result = (string)mr.output;
        bool exit = ExecuteCheat(result);

        if (exit)
        {
            InvokeExit(this, new MenuExitEventArgs(GetFullResult()));
        } else
        {
            cs.text.SetText("", true, true);
        }
    }

    public bool ExecuteCheat(string cheat)
    {
        bool doexit = true;
        string[] input = cheat.Split(" ");

        if (input[0].Length == 0)
        {
            return true;
        }

        //Load file
        if (input[0].Equals("ld"))
        {
            int.TryParse(input[1], out int lfIndex);
            if (MainManager.Instance.worldMode != MainManager.WorldMode.Overworld)
            {
                doexit = false;
                cs.log.SetText("Overworld only cheat!", true, true);
            } else
            {
                MainManager.Instance.LoadSave(lfIndex);
            }
        }

        //Save file
        if (input[0].Equals("sv"))
        {
            MainManager.Instance.Save();
        }

        //Warp player
        if (input[0].Equals("w"))
        {
            if (MainManager.Instance.worldMode != MainManager.WorldMode.Overworld)
            {
                doexit = false;
                cs.log.SetText("Overworld only cheat!", true, true);
            } else
            {
                Vector3 warpPos = MainManager.ParseVector3(input[1]);
                WorldPlayer.Instance.transform.position = warpPos;
            }
        }

        //warp delta
        if (input[0].Equals("wd"))
        {
            if (MainManager.Instance.worldMode != MainManager.WorldMode.Overworld)
            {
                doexit = false;
                cs.log.SetText("Overworld only cheat!", true, true);
            }
            else
            {
                Vector3 warpPos = MainManager.ParseVector3(input[1]);
                WorldPlayer.Instance.transform.position = WorldPlayer.Instance.transform.position + warpPos;
            }
        }

        //heal
        if (input[0].Equals("h"))
        {
            if (MainManager.Instance.worldMode != MainManager.WorldMode.Overworld)
            {
                if (MainManager.Instance.worldMode == MainManager.WorldMode.Battle)
                {
                    List<PlayerEntity> pel = BattleControl.Instance.GetPlayerEntities();
                    for (int i = 0; i < pel.Count; i++)
                    {
                        pel[i].HealHealth(pel[i].maxHP);
                        pel[i].HealEnergy(BattleControl.Instance.GetMaxEP(pel[i]));
                        pel[i].HealSoulEnergy(BattleControl.Instance.GetMaxSE(pel[i]));
                    }
                } else
                {
                    doexit = false;
                    cs.log.SetText("Not possible in this mode!", true, true);
                }
            }
            else
            {
                MainManager.Instance.playerData.FullHeal();
            }
        }

        //set cheat
        if (input[0].Equals("sc"))
        {
            bool toggle = false;
            bool setValue = false;

            if (input.Length > 2 && bool.TryParse(input[2], out setValue))
            {

            } else
            {
                toggle = true;
            }

            //do stuff with every single name
            //Note that the value that allows you to open the cheat menu is not in this list
            if (input[1].Equals("NoClip") || input[1].Equals("nc"))
            {
                if (toggle)
                {
                    MainManager.Instance.Cheat_NoClip = !MainManager.Instance.Cheat_NoClip;
                } else
                {
                    MainManager.Instance.Cheat_NoClip = setValue;
                }
            }

            if (input[1].Equals("KeruAsterJump") || input[1].Equals("kaj"))
            {
                if (toggle)
                {
                    MainManager.Instance.Cheat_KeruAsterJump = !MainManager.Instance.Cheat_KeruAsterJump;
                }
                else
                {
                    MainManager.Instance.Cheat_KeruAsterJump = setValue;
                }
            }

            if (input[1].Equals("FirstPersonCamera") || input[1].Equals("fpc"))
            {
                if (toggle)
                {
                    MainManager.Instance.Cheat_FirstPersonCamera = !MainManager.Instance.Cheat_FirstPersonCamera;
                }
                else
                {
                    MainManager.Instance.Cheat_FirstPersonCamera = setValue;
                }
            }

            if (input[1].Equals("FreeCam") || input[1].Equals("fc"))
            {
                if (toggle)
                {
                    MainManager.Instance.Cheat_FreeCam = !MainManager.Instance.Cheat_FreeCam;
                }
                else
                {
                    MainManager.Instance.Cheat_FreeCam = setValue;
                }
            }

            if (input[1].Equals("RevolvingCam") || input[1].Equals("rc"))
            {
                if (toggle)
                {
                    MainManager.Instance.Cheat_RevolvingCam = !MainManager.Instance.Cheat_RevolvingCam;
                }
                else
                {
                    MainManager.Instance.Cheat_RevolvingCam = setValue;
                }
            }

            if (input[1].Equals("SuperSlowTimescale") || input[1].Equals("sst"))
            {
                if (toggle)
                {
                    MainManager.Instance.Cheat_SuperSlowTimeScale = !MainManager.Instance.Cheat_SuperSlowTimeScale;
                }
                else
                {
                    MainManager.Instance.Cheat_SuperSlowTimeScale = setValue;
                }
            }

            if (input[1].Equals("SlowTimeScale") || input[1].Equals("st"))
            {
                if (toggle)
                {
                    MainManager.Instance.Cheat_SlowTimeScale = !MainManager.Instance.Cheat_SlowTimeScale;
                }
                else
                {
                    MainManager.Instance.Cheat_SlowTimeScale = setValue;
                }
            }

            if (input[1].Equals("FastTimeScale") || input[1].Equals("ft"))
            {
                if (toggle)
                {
                    MainManager.Instance.Cheat_FastTimeScale = !MainManager.Instance.Cheat_FastTimeScale;
                }
                else
                {
                    MainManager.Instance.Cheat_FastTimeScale = setValue;
                }
            }

            if (input[1].Equals("SuperFastTimeScale") || input[1].Equals("sft"))
            {
                if (toggle)
                {
                    MainManager.Instance.Cheat_SuperFastTimeScale = !MainManager.Instance.Cheat_SuperFastTimeScale;
                }
                else
                {
                    MainManager.Instance.Cheat_SuperFastTimeScale = setValue;
                }
            }

            //note that setting this makes the cheat menu invisible too :P
            if (input[1].Equals("InvisibleText") || input[1].Equals("it"))
            {
                if (toggle)
                {
                    MainManager.Instance.Cheat_InvisibleText = !MainManager.Instance.Cheat_InvisibleText;
                }
                else
                {
                    MainManager.Instance.Cheat_InvisibleText = setValue;
                }
            }

            if (input[1].Equals("AlmostAllBadgesActive") || input[1].Equals("aaba"))
            {
                if (toggle)
                {
                    MainManager.Instance.Cheat_AlmostAllBadgesActive = !MainManager.Instance.Cheat_AlmostAllBadgesActive;
                }
                else
                {
                    MainManager.Instance.Cheat_AlmostAllBadgesActive = setValue;
                }
            }

            if (input[1].Equals("BadgeDoubleStrength") || input[1].Equals("bds"))
            {
                if (toggle)
                {
                    MainManager.Instance.Cheat_BadgeDoubleStrength = !MainManager.Instance.Cheat_BadgeDoubleStrength;
                }
                else
                {
                    MainManager.Instance.Cheat_BadgeDoubleStrength = setValue;
                }
            }

            if (input[1].Equals("BadgeNegativeStrength") || input[1].Equals("bns"))
            {
                if (toggle)
                {
                    MainManager.Instance.Cheat_BadgeNegativeStrength = !MainManager.Instance.Cheat_BadgeNegativeStrength;
                }
                else
                {
                    MainManager.Instance.Cheat_BadgeNegativeStrength = setValue;
                }
            }

            if (input[1].Equals("NoEffectCaps") || input[1].Equals("nec"))
            {
                if (toggle)
                {
                    MainManager.Instance.Cheat_NoEffectCaps = !MainManager.Instance.Cheat_NoEffectCaps;
                }
                else
                {
                    MainManager.Instance.Cheat_NoEffectCaps = setValue;
                }
            }

            if (input[1].Equals("BattleWin") || input[1].Equals("bw"))
            {
                if (toggle)
                {
                    MainManager.Instance.Cheat_BattleWin = !MainManager.Instance.Cheat_BattleWin;
                }
                else
                {
                    MainManager.Instance.Cheat_BattleWin = setValue;
                }
            }

            if (input[1].Equals("BattleRandomActions") || input[1].Equals("bra"))
            {
                if (toggle)
                {
                    MainManager.Instance.Cheat_BattleRandomActions = !MainManager.Instance.Cheat_BattleRandomActions;

                    if (BattleControl.Instance != null)
                    {
                        List<PlayerEntity> pel = BattleControl.Instance.GetPlayerEntities();
                        foreach (PlayerEntity pe in pel)
                        {
                            pe.tactics = new List<BattleAction>();
                            pe.AddTactics();
                        }
                    }
                }
                else
                {
                    MainManager.Instance.Cheat_BattleRandomActions = setValue;
                }
            }

            if (input[1].Equals("BattleCheatActions") || input[1].Equals("bca"))
            {
                if (toggle)
                {
                    MainManager.Instance.Cheat_BattleCheatActions = !MainManager.Instance.Cheat_BattleCheatActions;

                    if (BattleControl.Instance != null)
                    {
                        List<PlayerEntity> pel = BattleControl.Instance.GetPlayerEntities();
                        foreach (PlayerEntity pe in pel)
                        {
                            pe.tactics = new List<BattleAction>();
                            pe.AddTactics();
                        }
                    }
                }
                else
                {
                    MainManager.Instance.Cheat_BattleCheatActions = setValue;
                }
            }

            if (input[1].Equals("BattleInfoActions") || input[1].Equals("bia"))
            {
                if (toggle)
                {
                    MainManager.Instance.Cheat_BattleInfoActions = !MainManager.Instance.Cheat_BattleInfoActions;

                    if (BattleControl.Instance != null)
                    {
                        List<PlayerEntity> pel = BattleControl.Instance.GetPlayerEntities();
                        foreach (PlayerEntity pe in pel)
                        {
                            pe.tactics = new List<BattleAction>();
                            pe.AddTactics();
                        }
                    }
                }
                else
                {
                    MainManager.Instance.Cheat_BattleInfoActions = setValue;
                }
            }


            if (input[1].Equals("BadgeAnarchy") || input[1].Equals("ba"))
            {
                if (toggle)
                {
                    MainManager.Instance.Cheat_BadgeAnarchy = !MainManager.Instance.Cheat_BadgeAnarchy;
                }
                else
                {
                    MainManager.Instance.Cheat_BadgeAnarchy = setValue;
                }
            }

            if (input[1].Equals("StaminaAnarchy") || input[1].Equals("sa"))
            {
                if (toggle)
                {
                    MainManager.Instance.Cheat_StaminaAnarchy = !MainManager.Instance.Cheat_StaminaAnarchy;
                }
                else
                {
                    MainManager.Instance.Cheat_StaminaAnarchy = setValue;
                }
            }

            if (input[1].Equals("EnergyAnarchy") || input[1].Equals("ea"))
            {
                if (toggle)
                {
                    MainManager.Instance.Cheat_EnergyAnarchy = !MainManager.Instance.Cheat_EnergyAnarchy;
                }
                else
                {
                    MainManager.Instance.Cheat_EnergyAnarchy = setValue;
                }
            }

            if (input[1].Equals("TargetAnarchy") || input[1].Equals("ta"))
            {
                if (toggle)
                {
                    MainManager.Instance.Cheat_TargetAnarchy = !MainManager.Instance.Cheat_TargetAnarchy;
                }
                else
                {
                    MainManager.Instance.Cheat_TargetAnarchy = setValue;
                }
            }

            if (input[1].Equals("SkillSwap") || input[1].Equals("ss"))
            {
                if (toggle)
                {
                    MainManager.Instance.Cheat_SkillSwap = !MainManager.Instance.Cheat_SkillSwap;
                }
                else
                {
                    MainManager.Instance.Cheat_SkillSwap = setValue;
                }
            }

            if (input[1].Equals("InfiniteBite") || input[1].Equals("ib"))
            {
                if (toggle)
                {
                    MainManager.Instance.Cheat_InfiniteBite = !MainManager.Instance.Cheat_InfiniteBite;
                }
                else
                {
                    MainManager.Instance.Cheat_InfiniteBite = setValue;
                }
            }

            if (input[1].Equals("BadgeSwap") || input[1].Equals("bs"))
            {
                if (toggle)
                {
                    MainManager.Instance.Cheat_BadgeSwap = !MainManager.Instance.Cheat_BadgeSwap;
                }
                else
                {
                    MainManager.Instance.Cheat_BadgeSwap = setValue;
                }
            }

            if (input[1].Equals("RibbonSwap") || input[1].Equals("rs"))
            {
                if (toggle)
                {
                    MainManager.Instance.Cheat_RibbonSwap = !MainManager.Instance.Cheat_RibbonSwap;
                }
                else
                {
                    MainManager.Instance.Cheat_RibbonSwap = setValue;
                }
            }

            if (input[1].Equals("WilexMoveUnlock") || input[1].Equals("wmu"))
            {
                if (toggle)
                {
                    MainManager.Instance.Cheat_WilexMoveUnlock = !MainManager.Instance.Cheat_WilexMoveUnlock;
                }
                else
                {
                    MainManager.Instance.Cheat_WilexMoveUnlock = setValue;
                }
            }

            if (input[1].Equals("LunaMoveUnlock") || input[1].Equals("lmu"))
            {
                if (toggle)
                {
                    MainManager.Instance.Cheat_LunaMoveUnlock = !MainManager.Instance.Cheat_LunaMoveUnlock;
                }
                else
                {
                    MainManager.Instance.Cheat_LunaMoveUnlock = setValue;
                }
            }

            if (input[1].Equals("PlayerCurseAttack") || input[1].Equals("pca"))
            {
                if (toggle)
                {
                    MainManager.Instance.Cheat_PlayerCurseAttack = !MainManager.Instance.Cheat_PlayerCurseAttack;
                }
                else
                {
                    MainManager.Instance.Cheat_PlayerCurseAttack = setValue;
                }
            }

            if (input[1].Equals("SeePickupCounts") || input[1].Equals("spc"))
            {
                if (toggle)
                {
                    MainManager.Instance.Cheat_SeePickupCounts = !MainManager.Instance.Cheat_SeePickupCounts;
                }
                else
                {
                    MainManager.Instance.Cheat_SeePickupCounts = setValue;
                }
            }

            if (input[1].Equals("DoubleStrengthEnviroEffects") || input[1].Equals("dsee"))
            {
                if (toggle)
                {
                    MainManager.Instance.Cheat_DoubleStrengthEnviroEffects = !MainManager.Instance.Cheat_DoubleStrengthEnviroEffects;
                }
                else
                {
                    MainManager.Instance.Cheat_DoubleStrengthEnviroEffects = setValue;
                }
            }

            if (input[1].Equals("QuadrupleStrengthEnviroEffects") || input[1].Equals("qsee"))
            {
                if (toggle)
                {
                    MainManager.Instance.Cheat_QuadrupleStrengthEnviroEffects = !MainManager.Instance.Cheat_QuadrupleStrengthEnviroEffects;
                }
                else
                {
                    MainManager.Instance.Cheat_QuadrupleStrengthEnviroEffects = setValue;
                }
            }

            if (input[1].Equals("UnrestrictedTextEntry") || input[1].Equals("ute"))
            {
                if (toggle)
                {
                    MainManager.Instance.Cheat_UnrestrictedTextEntry = !MainManager.Instance.Cheat_UnrestrictedTextEntry;
                }
                else
                {
                    MainManager.Instance.Cheat_UnrestrictedTextEntry = setValue;
                }
            }

            if (input[1].Equals("OverworldHazardImmunity") || input[1].Equals("ohi"))
            {
                if (toggle)
                {
                    MainManager.Instance.Cheat_OverworldHazardImmunity = !MainManager.Instance.Cheat_OverworldHazardImmunity;
                }
                else
                {
                    MainManager.Instance.Cheat_OverworldHazardImmunity = setValue;
                }
            }

            //Sidenote: the control of the cheat menu bypasses the disabled controls completely (so you can enable it even in a disabled control condition)
            if (input[1].Equals("ControlNeverDisabled") || input[1].Equals("cnd"))
            {
                if (toggle)
                {
                    MainManager.Instance.Cheat_ControlNeverDisabled = !MainManager.Instance.Cheat_ControlNeverDisabled;
                }
                else
                {
                    MainManager.Instance.Cheat_ControlNeverDisabled = setValue;
                }
            }
        }

        //get cheat
        if (input[0].Equals("gc"))
        {
            string getText = "Unknown cheat";

            if (input[1].Equals("NoClip") || input[1].Equals("nc"))
            {
                getText = "" + MainManager.Instance.Cheat_NoClip;
            }

            if (input[1].Equals("KeruAsterJump") || input[1].Equals("kaj"))
            {
                getText = "" + MainManager.Instance.Cheat_KeruAsterJump;
            }

            if (input[1].Equals("FirstPersonCamera") || input[1].Equals("fpc"))
            {
                getText = "" + MainManager.Instance.Cheat_FirstPersonCamera;
            }

            if (input[1].Equals("FreeCam") || input[1].Equals("fc"))
            {
                getText = "" + MainManager.Instance.Cheat_FreeCam;
            }

            if (input[1].Equals("RevolvingCam") || input[1].Equals("rc"))
            {
                getText = "" + MainManager.Instance.Cheat_RevolvingCam;
            }

            if (input[1].Equals("SuperSlowTimescale") || input[1].Equals("sst"))
            {
                getText = "" + MainManager.Instance.Cheat_SuperSlowTimeScale;
            }

            if (input[1].Equals("SlowTimeScale") || input[1].Equals("st"))
            {
                getText = "" + MainManager.Instance.Cheat_SlowTimeScale;
            }

            if (input[1].Equals("FastTimeScale") || input[1].Equals("ft"))
            {
                getText = "" + MainManager.Instance.Cheat_FastTimeScale;
            }

            if (input[1].Equals("SuperFastTimeScale") || input[1].Equals("sft"))
            {
                getText = "" + MainManager.Instance.Cheat_SuperFastTimeScale;
            }

            //note that setting this makes the cheat menu invisible too :P
            if (input[1].Equals("InvisibleText") || input[1].Equals("it"))
            {
                getText = "" + MainManager.Instance.Cheat_InvisibleText;
            }

            if (input[1].Equals("AlmostAllBadgesActive") || input[1].Equals("aaba"))
            {
                getText = "" + MainManager.Instance.Cheat_AlmostAllBadgesActive;
            }

            if (input[1].Equals("BadgeDoubleStrength") || input[1].Equals("bds"))
            {
                getText = "" + MainManager.Instance.Cheat_BadgeDoubleStrength;
            }

            if (input[1].Equals("BadgeNegativeStrength") || input[1].Equals("bns"))
            {
                getText = "" + MainManager.Instance.Cheat_BadgeNegativeStrength;
            }

            if (input[1].Equals("NoEffectCaps") || input[1].Equals("nec"))
            {
                getText = "" + MainManager.Instance.Cheat_NoEffectCaps;
            }

            if (input[1].Equals("BattleWin") || input[1].Equals("bw"))
            {
                getText = "" + MainManager.Instance.Cheat_BattleWin;
            }

            if (input[1].Equals("BattleRandomActions") || input[1].Equals("bra"))
            {
                getText = "" + MainManager.Instance.Cheat_BattleRandomActions;
            }

            if (input[1].Equals("BattleCheatActions") || input[1].Equals("bca"))
            {
                getText = "" + MainManager.Instance.Cheat_BattleCheatActions;
            }

            if (input[1].Equals("BattleInfoActions") || input[1].Equals("bia"))
            {
                getText = "" + MainManager.Instance.Cheat_BattleInfoActions;
            }

            if (input[1].Equals("BadgeAnarchy") || input[1].Equals("ba"))
            {
                getText = "" + MainManager.Instance.Cheat_BadgeAnarchy;
            }
            if (input[1].Equals("StaminaAnarchy") || input[1].Equals("sa"))
            {
                getText = "" + MainManager.Instance.Cheat_StaminaAnarchy;
            }
            if (input[1].Equals("EnergyAnarchy") || input[1].Equals("ea"))
            {
                getText = "" + MainManager.Instance.Cheat_EnergyAnarchy;
            }

            if (input[1].Equals("TargetAnarchy") || input[1].Equals("ta"))
            {
                getText = "" + MainManager.Instance.Cheat_TargetAnarchy;
            }

            if (input[1].Equals("SkillSwap") || input[1].Equals("ss"))
            {
                getText = "" + MainManager.Instance.Cheat_SkillSwap;
            }

            if (input[1].Equals("InfiniteBite") || input[1].Equals("ib"))
            {
                getText = "" + MainManager.Instance.Cheat_InfiniteBite;
            }

            if (input[1].Equals("BadgeSwap") || input[1].Equals("bs"))
            {
                getText = "" + MainManager.Instance.Cheat_BadgeSwap;
            }

            if (input[1].Equals("RibbonSwap") || input[1].Equals("rs"))
            {
                getText = "" + MainManager.Instance.Cheat_RibbonSwap;
            }

            if (input[1].Equals("WilexMoveUnlock") || input[1].Equals("wmu"))
            {
                getText = "" + MainManager.Instance.Cheat_WilexMoveUnlock;
            }

            if (input[1].Equals("LunaMoveUnlock") || input[1].Equals("lmu"))
            {
                getText = "" + MainManager.Instance.Cheat_LunaMoveUnlock;
            }

            if (input[1].Equals("PlayerCurseAttack") || input[1].Equals("pca"))
            {
                getText = "" + MainManager.Instance.Cheat_PlayerCurseAttack;
            }

            if (input[1].Equals("SeePickupCounts") || input[1].Equals("spc"))
            {
                getText = "" + MainManager.Instance.Cheat_SeePickupCounts;
            }

            if (input[1].Equals("DoubleStrengthEnviroEffects") || input[1].Equals("dsee"))
            {
                getText = "" + MainManager.Instance.Cheat_DoubleStrengthEnviroEffects;
            }

            if (input[1].Equals("QuadrupleStrengthEnviroEffects") || input[1].Equals("qsee"))
            {
                getText = "" + MainManager.Instance.Cheat_QuadrupleStrengthEnviroEffects;
            }

            if (input[1].Equals("UnrestrictedTextEntry") || input[1].Equals("ute"))
            {
                getText = "" + MainManager.Instance.Cheat_UnrestrictedTextEntry;
            }

            if (input[1].Equals("OverworldHazardImmunity") || input[1].Equals("ohi"))
            {
                getText = "" + MainManager.Instance.Cheat_OverworldHazardImmunity;
            }

            //Sidenote: the control of the cheat menu bypasses the disabled controls completely (so you can enable it even in a disabled control condition)
            if (input[1].Equals("ControlNeverDisabled") || input[1].Equals("cnd"))
            {
                getText = "" + MainManager.Instance.Cheat_ControlNeverDisabled;
            }

            cs.log.SetText(getText, true, true);
            doexit = false;
        }


        //Set global
        if (input[0].Equals("sgf"))
        {
            Enum.TryParse(input[1], out MainManager.GlobalFlag globalFlag);
            bool.TryParse(input[2], out bool setFlag);

            MainManager.Instance.SetGlobalFlag(globalFlag, setFlag);
        }

        //Set global var
        if (input[0].Equals("sgv"))
        {
            Enum.TryParse(input[1], out MainManager.GlobalVar globalVar);

            MainManager.Instance.SetGlobalVar(globalVar, input[2]);
        }

        //slightly buggy, since you are supposed to fade to black?
        if (input[0].Equals("mw"))
        {
            if (MainManager.Instance.worldMode != MainManager.WorldMode.Overworld)
            {
                doexit = false;
                cs.log.SetText("Overworld only cheat!", true, true);
            } else
            {
                Enum.TryParse(input[1], out MainManager.MapID mapID);
                int exit = 0;
                if (input.Length > 2)
                {
                    int.TryParse(input[2], out exit);
                }
                Vector3 warpPos = Vector3.zero;
                if (input.Length > 3)
                {
                    MainManager.ParseVector3(input[3]);
                }
                float yawOffset = 0;
                if (input.Length > 4)
                {
                    float.TryParse(input[4], out yawOffset);
                }
                MainManager.Instance.StartCoroutine(MainManager.Instance.ChangeMap(mapID, exit, warpPos, yawOffset));
            }
        }

        if (input[0].Equals("gi"))
        {
            Item getItem = Item.Parse(input[1]);
            if (MainManager.Instance.worldMode != MainManager.WorldMode.Overworld)
            {
                doexit = false;
                cs.log.SetText("Overworld only cheat!", true, true);
            } else
            {
                MainManager.Instance.StartCoroutine(MainManager.Instance.Pickup(new PickupUnion(getItem)));
            }
        }

        if (input[0].Equals("gb"))
        {
            Badge getBadge = Badge.Parse(input[1]);
            if (MainManager.Instance.worldMode != MainManager.WorldMode.Overworld)
            {
                doexit = false;
                cs.log.SetText("Overworld only cheat!", true, true);
            }
            else
            {
                MainManager.Instance.StartCoroutine(MainManager.Instance.Pickup(new PickupUnion(getBadge)));
            }
        }

        if (input[0].Equals("gk"))
        {
            KeyItem getKeyItem = KeyItem.Parse(input[1]);
            if (MainManager.Instance.worldMode != MainManager.WorldMode.Overworld)
            {
                doexit = false;
                cs.log.SetText("Overworld only cheat!", true, true);
            }
            else
            {
                MainManager.Instance.StartCoroutine(MainManager.Instance.Pickup(new PickupUnion(getKeyItem)));
            }
        }

        if (input[0].Equals("gr"))
        {
            Ribbon getRibbon = Ribbon.Parse(input[1]);
            if (MainManager.Instance.worldMode != MainManager.WorldMode.Overworld)
            {
                doexit = false;
                cs.log.SetText("Overworld only cheat!", true, true);
            }
            else
            {
                MainManager.Instance.StartCoroutine(MainManager.Instance.Pickup(new PickupUnion(getRibbon)));
            }
        }

        if (input[0].Equals("gpos"))
        {
            doexit = false;
            cs.log.SetText("Position = " + WorldPlayer.Instance.transform.position + ", True Yaw = " + WorldPlayer.Instance.GetTrueFacingRotation(), true, true);
        }

        //Get global flag
        if (input[0].Equals("ggf"))
        {
            Enum.TryParse(input[1], out MainManager.GlobalFlag globalFlag);

            doexit = false;
            cs.log.SetText(globalFlag + " (aka " + (int)globalFlag + ") = " + MainManager.Instance.GetGlobalFlag(globalFlag), true, true);
        }

        //Get global var
        if (input[0].Equals("ggv"))
        {
            Enum.TryParse(input[1], out MainManager.GlobalVar globalVar);

            doexit = false;
            cs.log.SetText(globalVar + " (aka " + (int)globalVar + ") = " + MainManager.Instance.GetGlobalVar(globalVar), true, true);
        }

        //Get mapname
        if (input[0].Equals("gmn"))
        {
            string mn = MainManager.Instance.mapScript.mapName;
            Enum.TryParse(mn, out MainManager.MapID mapID);

            doexit = false;
            cs.log.SetText(mapID + " aka " + (int)mapID, true, true);
        }

        return doexit;
    }

    public void Cancel()
    {
        cancel = true;
        InvokeExit(this, new MenuExitEventArgs(GetFullResult()));
        Clear();
        Destroy(gameObject);
    }

    public void InvokeExit(object sender, MenuExitEventArgs meea)
    {
        InputManager.Instance.disableControl = false;
        menuExit?.Invoke(this, new MenuExitEventArgs(GetFullResult()));
        Clear();
        Destroy(gameObject);
    }
    public override MenuResult GetResult()
    {
        //cut out trailing spaces (allowable in Unrestricted mode)
        string newString = currString;

        if (currString == null)
        {
            return new MenuResult("");
        }

        if (!MainManager.Instance.Cheat_UnrestrictedTextEntry)
        {
            int cut = 0;
            for (int i = newString.Length - 1; i >= 0; i--)
            {
                if (newString[i] == ' ')
                {
                    cut++;
                }
                else
                {
                    break;
                }
            }

            newString = newString.Substring(0, currString.Length - cut);
        }

        //note: check for cancellation by checking for a 0 length string
        //In most environments that shouldn't be treated as valid anyway
        return new MenuResult(cancel ? "" : newString);
    }
}

