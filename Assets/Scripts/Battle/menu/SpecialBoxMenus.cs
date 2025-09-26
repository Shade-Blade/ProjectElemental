using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class MoveBoxMenu : BoxMenu
{
    //MoveMenuEntry.StatDisplay displayMode;
    //public bool hasZero = false;            //set this before initialization to be correct

    public static string DebtColor()
    {
        return "#a07030";
    }

    public static string ExhaustColor()
    {
        return "#705030";
    }


    public override void Init()
    {
        base.Init();

        switch (menuName)
        {
            case BaseBattleMenu.BaseMenuName.Jump:
                menuEntries = new BoxMenuEntry[caller.jumpMoves.Count];
                for (int i = 0; i < caller.jumpMoves.Count; i++)
                {
                    MoveMenuEntry.StaminaState ss = MoveMenuEntry.StaminaState.None;
                    if (caller.stamina < caller.jumpMoves[i].GetStaminaCost(caller, caller.jumpMoves[i].level))
                    {
                        ss = MoveMenuEntry.StaminaState.Debt;
                    } else if (caller.GetRealAgility() < caller.jumpMoves[i].GetStaminaCost(caller, caller.jumpMoves[i].level))
                    {
                        ss = MoveMenuEntry.StaminaState.Exhaust;
                    }

                    menuEntries[i] = new MoveMenuEntry(caller, caller.jumpMoves[i], ss);
                    GameObject g = Instantiate(MainManager.Instance.menuEntryBase, bm.mask.transform);
                    g.transform.localPosition = BoxMenuScript.GetRelativePosition(i);
                    menuEntriesO.Add(g);
                    BoxMenuEntryScript b = menuEntriesO[i].GetComponent<BoxMenuEntryScript>();
                    if (ss == MoveMenuEntry.StaminaState.Debt)
                    {
                        b.Setup(menuEntries[i], true, null, DebtColor(), null);
                    }
                    else
                    {
                        if (ss == MoveMenuEntry.StaminaState.Exhaust)
                        {
                            b.Setup(menuEntries[i], true, null, ExhaustColor(), null);
                        }
                        else
                        {
                            b.Setup(menuEntries[i]);
                        }
                    }
                }
                //Cost reduction
                if (caller.GetBadgeEnduranceBonus() + caller.GetEffectEnduranceBonus() != 0)
                {
                    if (caller.GetBadgeEnduranceBonus() + caller.GetEffectEnduranceBonus() > 0)
                    {
                        descriptorString = "Skill costs reduced by " + (caller.GetBadgeEnduranceBonus() + caller.GetEffectEnduranceBonus());
                    } else
                    {
                        descriptorString = "Skill costs increased by " + -(caller.GetBadgeEnduranceBonus() + caller.GetEffectEnduranceBonus());
                    }
                }
                break;
            case BaseBattleMenu.BaseMenuName.Weapon:
                menuEntries = new BoxMenuEntry[caller.weaponMoves.Count];
                for (int i = 0; i < caller.weaponMoves.Count; i++)
                {
                    MoveMenuEntry.StaminaState ss = MoveMenuEntry.StaminaState.None;
                    if (caller.stamina < caller.weaponMoves[i].GetStaminaCost(caller, caller.weaponMoves[i].level))
                    {
                        ss = MoveMenuEntry.StaminaState.Debt;
                    }
                    else if (caller.GetRealAgility() < caller.weaponMoves[i].GetStaminaCost(caller, caller.weaponMoves[i].level))
                    {
                        ss = MoveMenuEntry.StaminaState.Exhaust;
                    }

                    menuEntries[i] = new MoveMenuEntry(caller, caller.weaponMoves[i], ss);
                    GameObject g = Instantiate(MainManager.Instance.menuEntryBase, bm.mask.transform);
                    g.transform.localPosition = BoxMenuScript.GetRelativePosition(i);
                    menuEntriesO.Add(g);
                    BoxMenuEntryScript b = menuEntriesO[i].GetComponent<BoxMenuEntryScript>();
                    if (ss == MoveMenuEntry.StaminaState.Debt)
                    {
                        b.Setup(menuEntries[i], true, null, DebtColor(), null);
                    }
                    else
                    {
                        if (ss == MoveMenuEntry.StaminaState.Exhaust)
                        {
                            b.Setup(menuEntries[i], true, null, ExhaustColor(), null);
                        }
                        else
                        {
                            b.Setup(menuEntries[i]);
                        }
                    }
                }
                //Cost reduction
                if (caller.GetBadgeEnduranceBonus() + caller.GetEffectEnduranceBonus() != 0)
                {
                    if (caller.GetBadgeEnduranceBonus() + caller.GetEffectEnduranceBonus() > 0)
                    {
                        descriptorString = "Skill costs reduced by " + (caller.GetBadgeEnduranceBonus() + caller.GetEffectEnduranceBonus());
                    }
                    else
                    {
                        descriptorString = "Skill costs increased by " + -(caller.GetBadgeEnduranceBonus() + caller.GetEffectEnduranceBonus());
                    }
                }
                break;
            case BaseBattleMenu.BaseMenuName.Soul:
                menuEntries = new BoxMenuEntry[caller.soulMoves.Count];
                for (int i = 0; i < caller.soulMoves.Count; i++)
                {
                    menuEntries[i] = new MoveMenuEntry(caller, caller.soulMoves[i]);
                    GameObject g = Instantiate(MainManager.Instance.menuEntryBase, bm.mask.transform);
                    g.transform.localPosition = BoxMenuScript.GetRelativePosition(i);
                    menuEntriesO.Add(g);
                    BoxMenuEntryScript b = menuEntriesO[i].GetComponent<BoxMenuEntryScript>();
                    b.Setup(menuEntries[i]);
                }
                //Cost reduction
                if (caller.GetBadgeFlowBonus() + caller.GetEffectFlowBonus() != 0)
                {
                    if (caller.GetBadgeFlowBonus() + caller.GetEffectFlowBonus() > 0)
                    {
                        descriptorString = "Soul costs reduced by " + (caller.GetBadgeFlowBonus() + caller.GetEffectFlowBonus());
                    } else
                    {
                        descriptorString = "Soul costs increased by " + -(caller.GetBadgeFlowBonus() + caller.GetEffectFlowBonus());
                    }
                }
                break;
            default:
                throw new NotImplementedException();
        }

        PostEntriesInit();
    }
    public override void SelectOption()
    {
        MainManager.Instance.PlayGlobalSound(MainManager.Sound.Menu_Select);
        //Selecting a move takes you to a selection menu
        BattleSelectionMenu s;

        switch (menuName)
        {
            case BaseBattleMenu.BaseMenuName.Jump:
                s = BattleSelectionMenu.buildMenu(caller, ((MoveMenuEntry)(menuEntries[menuIndex])).target, GetMenuName(), caller.jumpMoves[menuIndex], caller.jumpMoves[menuIndex].GetActionCommandDesc(), menuEntries[menuIndex].level);
                break;
            case BaseBattleMenu.BaseMenuName.Weapon:
                s = BattleSelectionMenu.buildMenu(caller, ((MoveMenuEntry)(menuEntries[menuIndex])).target, GetMenuName(), caller.weaponMoves[menuIndex], caller.weaponMoves[menuIndex].GetActionCommandDesc(), menuEntries[menuIndex].level);
                break;
            case BaseBattleMenu.BaseMenuName.Soul:
                s = BattleSelectionMenu.buildMenu(caller, ((MoveMenuEntry)(menuEntries[menuIndex])).target, GetMenuName(), caller.soulMoves[menuIndex], caller.soulMoves[menuIndex].GetActionCommandDesc(), menuEntries[menuIndex].level);
                break;
            default:
                throw new NotImplementedException();
        }
        //s = BattleSelectionMenu.buildMenu(caller, caller.moveset[menuIndex].GetBaseTarget(), GetMenuName(), caller.moveset[menuIndex].GetActionCommandDesc());
        //s = BattleSelectionMenu.buildMenu(caller, caller.moveset[menuIndex+1].GetBaseTarget(), GetMenuName(), caller.moveset[menuIndex+1].GetActionCommandDesc());

        s.transform.SetParent(transform);
        PushState(s);
        s.menuExit += InvokeExit;
    }
    public override void SelectDisabled()
    {
        MainManager.Instance.PlayGlobalSound(MainManager.Sound.Menu_Error);
        BattlePopup popup = null;

        switch (menuName)
        {
            case BaseBattleMenu.BaseMenuName.Jump:
                popup = new BattlePopup(caller.jumpMoves[menuIndex].GetCantMoveReason(caller, menuEntries[menuIndex].level));
                break;
            case BaseBattleMenu.BaseMenuName.Weapon:
                popup = new BattlePopup(caller.weaponMoves[menuIndex].GetCantMoveReason(caller, menuEntries[menuIndex].level));
                break;
            case BaseBattleMenu.BaseMenuName.Soul:
                popup = new BattlePopup(caller.soulMoves[menuIndex].GetCantMoveReason(caller, menuEntries[menuIndex].level));
                break;
            default:
                throw new NotImplementedException();
        }

        BattlePopupMenuScript s = BattlePopupMenuScript.BuildMenu(popup.text);
        s.transform.SetParent(transform);
        PushState(s);
        s.menuExit += InvokeExit;
    }
    public override void ZOption()
    {
        /*
        //increment display mode
        //The getNames way of finding the last enum value doesn't work if enums don't start at 0 and go up by 1 each time

        if (System.Enum.GetNames(typeof(MoveMenuEntry.StatDisplay)).Length - 1 == 0)
        {
            return;
        }

        if ((int)displayMode == System.Enum.GetNames(typeof(MoveMenuEntry.StatDisplay)).Length - 1)
        {
            displayMode = 0;
        } else
        {
            displayMode = displayMode + 1;
        }

        for (int i = 0; i < menuEntries.Length; i++)
        {
            menuEntries[i] = new MoveMenuEntry(caller, (PlayerMove)caller.moveset[i], displayMode);
            BoxMenuEntryScript b = menuEntriesO[i].GetComponent<BoxMenuEntryScript>();
            b.Setup(menuEntries[i]);
        }
        

        for (int i = 1; i < menuEntries.Length; i++)
        {
            menuEntries[i - 1] = new MoveMenuEntry(caller, (PlayerMove)caller.moveset[i], displayMode);
            BoxMenuEntryScript b = menuEntriesO[i - 1].GetComponent<BoxMenuEntryScript>();
            b.Setup(menuEntries[i - 1]);
        }
        
        */
    }
    public override void IncrementLevel(int inc)
    {
        if (menuEntries[menuIndex].maxLevel <= 1)
        {
            return;
        }

        menuEntries[menuIndex].level += inc;
        if (menuEntries[menuIndex].level <= 0)
        {
            menuEntries[menuIndex].level += menuEntries[menuIndex].maxLevel;
        }
        menuEntries[menuIndex].level = ((menuEntries[menuIndex].level - 1) % menuEntries[menuIndex].maxLevel) + 1;

        ((MoveMenuEntry)menuEntries[menuIndex]).RecalculateMove(caller);

        BoxMenuEntryScript b = menuEntriesO[menuIndex].GetComponent<BoxMenuEntryScript>();

        if (caller.stamina < ((MoveMenuEntry)menuEntries[menuIndex]).staminaCost)
        {
            b.Setup(menuEntries[menuIndex], true, null, DebtColor(), null);
        }
        else
        {
            if (caller.GetRealAgility() < ((MoveMenuEntry)menuEntries[menuIndex]).staminaCost)
            {
                b.Setup(menuEntries[menuIndex], true, null, ExhaustColor(), null);
            } else
            {
                b.Setup(menuEntries[menuIndex]);
            }
        }

        descriptionBoxScript.SetText(menuEntries[menuIndex].description);
    }

    public override void OnHover(BoxMenuEntry boxMenuEntry)
    {
        BattleControl.Instance.ResetStatHighlight();
        switch (menuName)
        {
            case BaseBattleMenu.BaseMenuName.Jump:
                if (caller.jumpMoves[menuIndex].GetCost(caller, menuEntries[menuIndex].level) > 0)
                {
                    BattleControl.Instance.StatHighlight(caller.jumpMoves[menuIndex].GetCurrency(), caller);
                    BattleControl.Instance.StatHighlight(BattleHelper.MoveCurrency.Stamina, caller);
                }
                break;
            case BaseBattleMenu.BaseMenuName.Weapon:
                if (caller.weaponMoves[menuIndex].GetCost(caller, menuEntries[menuIndex].level) > 0)
                {
                    BattleControl.Instance.StatHighlight(caller.weaponMoves[menuIndex].GetCurrency(), caller);
                    BattleControl.Instance.StatHighlight(BattleHelper.MoveCurrency.Stamina, caller);
                }
                break;
            case BaseBattleMenu.BaseMenuName.Soul:
                if (caller.soulMoves[menuIndex].GetCost(caller, menuEntries[menuIndex].level) > 0)
                {
                    BattleControl.Instance.StatHighlight(caller.soulMoves[menuIndex].GetCurrency(), caller);
                }
                break;
            default:
                throw new NotImplementedException();
        }
    }
    public override void Clear()
    {
        BattleControl.Instance.ResetStatHighlight();

        base.Clear();
    }

    /*
    public override Move GetCurrent()
    {
        return (BattleMove)caller.moveset[menuIndex];
    }
    public override BattleAction GetAction()
    {
        return null;
    }
    */

    public class MoveMenuResult
    {
        public PlayerMove playerMove;
        public int level;

        public MoveMenuResult(PlayerMove p_playerMove, int p_level)
        {
            playerMove = p_playerMove;
            level = p_level;
        }
    }

    public override MenuResult GetResult()
    {
        switch (menuName)
        {
            case BaseBattleMenu.BaseMenuName.Jump:
                return new MenuResult(new MoveMenuResult(caller.jumpMoves[menuIndex], menuEntries[menuIndex].level));
            case BaseBattleMenu.BaseMenuName.Weapon:
                return new MenuResult(new MoveMenuResult(caller.weaponMoves[menuIndex], menuEntries[menuIndex].level));
            case BaseBattleMenu.BaseMenuName.Soul:
                return new MenuResult(new MoveMenuResult(caller.soulMoves[menuIndex], menuEntries[menuIndex].level));
            default:
                throw new NotImplementedException();
        }
        //return new MenuResult((PlayerMove)caller.moveset[menuIndex]);
        //return new MenuResult((PlayerMove)caller.moveset[menuIndex + 1]);
    }
    public override BaseBattleMenu.BaseMenuName GetMenuName() => menuName;
}

public class ItemBoxMenu : BoxMenu
{
    //int displayIndex;
    /*
    ItemMenuEntry.StatDisplay displayMode
    {
        get => displayModes[displayIndex];
    }
    List<ItemMenuEntry.StatDisplay> displayModes; //All possible display modes
    */

    //special list
    public List<Item> specialList;

    public List<bool> hasBackground;
    public List<Color> backgroundColors;
    public List<bool> blockList;

    public override void Init()
    {
        base.Init();

        //get items from item inventory
        List<Item> inv = BattleControl.Instance.playerData.itemInventory;

        if (specialList != null)
        {
            inv = specialList;
        }

        menuEntries = new BoxMenuEntry[inv.Count];
        for (int i = 0; i < inv.Count; i++)
        {
            if (hasBackground != null)
            {
                if (hasBackground[i])
                {
                    menuEntries[i] = new ItemMenuEntry(caller, inv[i], Item.GetItemMoveScript(inv[i]), backgroundColors[i]);
                }
                else
                {
                    menuEntries[i] = new ItemMenuEntry(caller, inv[i], Item.GetItemMoveScript(inv[i]));
                }
            } else
            {
                menuEntries[i] = new ItemMenuEntry(caller, inv[i], Item.GetItemMoveScript(inv[i]));
            }

            if (blockList != null)
            {
                if (blockList[i])
                {
                    menuEntries[i].canUse = false;
                }
            }


            GameObject g = Instantiate(MainManager.Instance.menuEntryBase, bm.mask.transform);
            g.transform.localPosition = BoxMenuScript.GetRelativePosition(i);
            menuEntriesO.Add(g);
            BoxMenuEntryScript b = menuEntriesO[i].GetComponent<BoxMenuEntryScript>();
            b.Setup(menuEntries[i]);
        }

        //caller.itemSaver
        if (caller.BadgeEquipped(Badge.BadgeType.ItemSaver))
        {
            descriptorString = "<effect,Freebie> Freebie: " + (2 - caller.itemSaver) + " away";
            if (caller.BadgeEquippedCount(Badge.BadgeType.ItemSaver) > 1)
            {
                descriptorString = "Freebie (x" + caller.BadgeEquippedCount(Badge.BadgeType.ItemSaver) + "): " + (2 - caller.itemSaver) + " away";
            }
        }
        if (caller.HasEffect(Effect.EffectType.Freebie))
        {
            descriptorString = "<effect,Freebie> Freebie: Next item won't be used.";
            if (caller.GetEffectEntry(Effect.EffectType.Freebie).potency > 1)
            {
                int p = caller.GetEffectEntry(Effect.EffectType.Freebie).potency;
                descriptorString = "<effect,Freebie> Freebie (x" + p + "): Next " + p + " items won't be used.";
            }
        }

        PostEntriesInit();
    }
    public override void SelectOption()
    {
        MainManager.Instance.PlayGlobalSound(MainManager.Sound.Menu_Select);
        //Debug.Log("Select " + menuIndex);
        if (menuIndex != -1 && menuIndex != -2)
        {
            //Selecting a move takes you to a selection menu
            List<Item> inv = BattleControl.Instance.playerData.itemInventory;
            if (specialList != null)
            {
                inv = specialList;
            }
            BattleSelectionMenu s2 = BattleSelectionMenu.buildMenu(caller, Item.GetTarget(inv[menuIndex]), GetMenuName(), Item.GetItemMoveScript(inv[menuIndex]), Item.GetActionCommandDesc(inv[menuIndex]));
            s2.transform.SetParent(transform);
            PushState(s2);
            s2.menuExit += InvokeExit;
        } else
        {
            InvokeExit(this, new MenuExitEventArgs(GetFullResult()));
            Clear();
        }
    }
    public override void SelectDisabled()
    {
        MainManager.Instance.PlayGlobalSound(MainManager.Sound.Menu_Error);
        BattlePopup popup = null;

        List<Item> inv = BattleControl.Instance.playerData.itemInventory;
        if (specialList != null)
        {
            inv = specialList;
        }

        ItemDataEntry ide = Item.GetItemDataEntry(inv[menuIndex]);

        if (Item.GetProperty(ide, Item.ItemProperty.NoBattle) != null)
        {
            popup = new BattlePopup(PlayerMove.CantMoveReason.ItemOverworldOnly);
        }
        else if (Item.GetProperty(ide, Item.ItemProperty.Limited) != null)
        {
            popup = new BattlePopup(PlayerMove.CantMoveReason.ItemExpended);
        } else if (blockList != null && blockList[menuIndex])
        {
            popup = new BattlePopup(PlayerMove.CantMoveReason.ItemMultiSupplyBlock);
        } else
        {
            popup = new BattlePopup(PlayerMove.CantMoveReason.NoTargets);
        }

        BattlePopupMenuScript s = BattlePopupMenuScript.BuildMenu(popup.text);
        s.transform.SetParent(transform);
        PushState(s);
        s.menuExit += InvokeExit;
    }

    public override void Cancel()
    {
        MainManager.Instance.PlayGlobalSound(MainManager.Sound.Menu_Close);
        PopSelf();
        if (parent == null) //special case for double bite
        {
            menuIndex = -1;
            SelectOption();
        }
    }
    public override void ZOption()
    {
        //Hacky setup for double bite
        if (backgroundColors != null)
        {
            if (specialList.Count == 0)
            {
                MainManager.Instance.PlayGlobalSound(MainManager.Sound.Menu_Cancel);
                //act like cancel
                Cancel();
            } else
            {
                MainManager.Instance.PlayGlobalSound(MainManager.Sound.Menu_Select);
                menuIndex = -2;
                SelectOption();
            }
        }


        /*
        //increment display mode
        //The getNames way of finding the last enum value doesn't work if enums don't start at 0 and go up by 1 each time

        if (displayIndex == displayModes.Count-1)//System.Enum.GetNames(typeof(ItemMenuEntry.StatDisplay)).Length-1)
        {
            displayIndex = 0;
        }
        else
        {
            displayIndex++;
        }


        List<Item> inv = MainManager.Instance.playerData.itemInventory;

        for (int i = 0; i < menuEntries.Length; i++)
        {
            menuEntries[i] = new ItemMenuEntry(caller, inv[i], Item.GetItemMoveScript(inv[i]), displayMode);
            BoxMenuEntryScript b = menuEntriesO[i].GetComponent<BoxMenuEntryScript>();
            b.Setup(menuEntries[i]);
        }
        */
    }

    public override void OnHover(BoxMenuEntry boxMenuEntry)
    {
        BattleControl.Instance.ResetStatHighlight();
        BattleControl.Instance.ItemHighlight();
    }

    public override void Clear()
    {
        BattleControl.Instance.ResetStatHighlight();

        base.Clear();
    }

    /*
    public override Move GetCurrent()
    {
        return Item.GetItemMoveScript(MainManager.Instance.playerData.itemInventory[menuIndex].itemType);
    }
    public override BattleAction GetAction()
    {
        return null;
    }
    */

    public override MenuResult GetResult()
    {
        if (menuIndex == -1 || menuIndex == -2)
        {
            return new MenuResult(null);
        }
        return new MenuResult(Item.GetItemMoveScript(((ItemMenuEntry)(menuEntries[menuIndex])).item));

        //this works but the top one is better since it only references the menu itself
        //return new MenuResult(Item.GetItemMoveScript(MainManager.Instance.playerData.itemInventory[menuIndex].itemType));
    }
    public override BaseBattleMenu.BaseMenuName GetMenuName() => BaseBattleMenu.BaseMenuName.Items;
}

//Meta item moves
//(double bite, quick bite...)
public class MetaItemBoxMenu : BoxMenu
{
    //entries   
    List<MetaItemMove.Move> moves;

    public static List<MetaItemMove.Move> GetAvailableMoves(PlayerData pd)
    {
        List<MetaItemMove.Move> moves = new List<MetaItemMove.Move>();

        moves.Add(MetaItemMove.Move.Normal);

        if (pd.BadgeEquipped(Badge.BadgeType.MultiSupply) || MainManager.Instance.Cheat_InfiniteBite)
        {
            moves.Add(MetaItemMove.Move.Multi);
        }
        if (pd.BadgeEquipped(Badge.BadgeType.QuickSupply))
        {
            moves.Add(MetaItemMove.Move.Quick);
        }
        if (pd.BadgeEquipped(Badge.BadgeType.VoidSupply))
        {
            moves.Add(MetaItemMove.Move.Void);
        }

        return moves;
    }

    public override void Init()
    {
        base.Init();

        //construct the menu
        moves = GetAvailableMoves(BattleControl.Instance.playerData);
        menuEntries = new BoxMenuEntry[moves.Count];
        for (int i = 0; i < moves.Count; i++)
        {
            menuEntries[i] = new MetaItemMenuEntry(moves[i]);
            menuEntries[i].canUse = BattleControl.Instance.CanUseMetaItemMove(moves[i]);

            int uses = BattleControl.Instance.GetMetaItemUsesRemaining(moves[i]);
            if (uses >= 0)  //-1 = infinite uses (no text displayed)
            {
                menuEntries[i].rightText = uses + " " + menuEntries[i].spriteString + " <size,0>.</size>";
                menuEntries[i].spriteString = "";
            } else
            {
                //hacky fix?
                //also aligns things correctly with the other options
                menuEntries[i].rightText = " " + menuEntries[i].spriteString + " <size,0>.</size>";
                menuEntries[i].spriteString = "";
            }

            GameObject g = Instantiate(MainManager.Instance.menuEntryBase, bm.mask.transform);
            g.transform.localPosition = BoxMenuScript.GetRelativePosition(i);
            menuEntriesO.Add(g);
            BoxMenuEntryScript b = menuEntriesO[i].GetComponent<BoxMenuEntryScript>();
            b.Setup(menuEntries[i]);
        }

        PostEntriesInit();
    }
    public override void SelectOption()
    {
        MainManager.Instance.PlayGlobalSound(MainManager.Sound.Menu_Select);
        switch (moves[menuIndex])
        {
            case MetaItemMove.Move.Normal:
                break;
            case MetaItemMove.Move.Multi:
                break;
            case MetaItemMove.Move.Quick:
                break;
            case MetaItemMove.Move.Void:
                break;
        }

        //kicks you to an item menu

        ItemBoxMenu s2 = null;
        if (moves[menuIndex] == MetaItemMove.Move.Void)
        {
            s2 = BoxMenu.BuildSpecialItemMenu(caller, BattleControl.Instance.GetUsedItemInventory(caller));
        }
        else
        {
            s2 = (ItemBoxMenu)(BoxMenu.BuildMenu(caller, BaseBattleMenu.BaseMenuName.Items));
        }

        //BattleSelectionMenu s2 = BattleSelectionMenu.buildMenu(caller, Item.GetTarget(inv[menuIndex]), GetMenuName(), Item.GetItemMoveScript(inv[menuIndex]), Item.GetActionCommandDesc(inv[menuIndex]));
        s2.transform.SetParent(transform);
        PushState(s2);
        s2.menuExit += InvokeExit;
    }
    public override void SelectDisabled()
    {
        MainManager.Instance.PlayGlobalSound(MainManager.Sound.Menu_Error);
        BattlePopup popup = null;

        popup = new BattlePopup(BattleControl.Instance.GetCantUseReasonMetaItemMove(moves[menuIndex]));

        BattlePopupMenuScript s = BattlePopupMenuScript.BuildMenu(popup.text);
        s.transform.SetParent(transform);
        PushState(s);
        s.menuExit += InvokeExit;
    }
    public override void ZOption()
    {
        /*
        //increment display mode
        //The getNames way of finding the last enum value doesn't work if enums don't start at 0 and go up by 1 each time

        if (displayIndex == displayModes.Count-1)//System.Enum.GetNames(typeof(ItemMenuEntry.StatDisplay)).Length-1)
        {
            displayIndex = 0;
        }
        else
        {
            displayIndex++;
        }


        List<Item> inv = MainManager.Instance.playerData.itemInventory;

        for (int i = 0; i < menuEntries.Length; i++)
        {
            menuEntries[i] = new ItemMenuEntry(caller, inv[i], Item.GetItemMoveScript(inv[i]), displayMode);
            BoxMenuEntryScript b = menuEntriesO[i].GetComponent<BoxMenuEntryScript>();
            b.Setup(menuEntries[i]);
        }
        */
    }

    /*
    public override Move GetCurrent()
    {
        return Item.GetItemMoveScript(MainManager.Instance.playerData.itemInventory[menuIndex].itemType);
    }
    public override BattleAction GetAction()
    {
        return null;
    }
    */

    public override void OnHover(BoxMenuEntry boxMenuEntry)
    {
        BattleControl.Instance.ResetStatHighlight();
        BattleControl.Instance.ItemHighlight();
    }

    public override void Clear()
    {
        BattleControl.Instance.ResetStatHighlight();

        base.Clear();
    }


    public override MenuResult GetResult()
    {
        return new MenuResult(Item.GetMetaItemMoveScript(moves[menuIndex]));
    }
    public override BaseBattleMenu.BaseMenuName GetMenuName() => BaseBattleMenu.BaseMenuName.MetaItems;
}

public class TacticsBoxMenu : BoxMenu
{
    public override void Init()
    {
        base.Init();

        //build these actions manually
        //these are pretty hardcoded
        List<BattleAction> tactics = BattleControl.Instance.GetTactics(caller);

        menuEntries = new BoxMenuEntry[tactics.Count];
        for (int i = 0; i < tactics.Count; i++)
        {
            menuEntries[i] = new TacticsMenuEntry(caller, tactics[i]);
            GameObject g = Instantiate(MainManager.Instance.menuEntryBase, bm.mask.transform);
            g.transform.localPosition = BoxMenuScript.GetRelativePosition(i);
            menuEntriesO.Add(g);
            BoxMenuEntryScript b = menuEntriesO[i].GetComponent<BoxMenuEntryScript>();
            b.Setup(menuEntries[i]);
        }

        PostEntriesInit();
    }
    public override void SelectOption()
    {
        MainManager.Instance.PlayGlobalSound(MainManager.Sound.Menu_Select);
        //Selecting a move takes you to a selection menu
        List<BattleAction> tactics = BattleControl.Instance.GetTactics(caller);
        BattleSelectionMenu s3 = BattleSelectionMenu.buildMenu(caller, tactics[menuIndex].GetBaseTarget(), GetMenuName(), tactics[menuIndex], tactics[menuIndex].GetActionCommandDesc());
        s3.transform.SetParent(transform);
        PushState(s3);
        s3.menuExit += InvokeExit;
    }
    public override void SelectDisabled()
    {
        MainManager.Instance.PlayGlobalSound(MainManager.Sound.Menu_Error);
        BattlePopup popup = null;

        List<BattleAction> tactics = BattleControl.Instance.GetTactics(caller);
        popup = new BattlePopup(tactics[menuIndex].GetCantMoveReason(caller));

        BattlePopupMenuScript s = BattlePopupMenuScript.BuildMenu(popup.text);
        s.transform.SetParent(transform);
        PushState(s);
        s.menuExit += InvokeExit;
    }

    public override void OnHover(BoxMenuEntry boxMenuEntry)
    {
        BattleControl.Instance.ResetStatHighlight();

        List<BattleAction> tactics = BattleControl.Instance.GetTactics(caller);
        if (tactics[menuIndex].GetCost(caller) > 0)
        {
            BattleControl.Instance.StatHighlight(tactics[menuIndex].GetCurrency(), caller);
        }
    }

    public override void Clear()
    {
        BattleControl.Instance.ResetStatHighlight();

        base.Clear();
    }


    /*
    public override Move GetCurrent()
    {
        return null;
    }
    public override BattleAction GetAction()
    {
        List<BattleAction> tactics = BattleControl.Instance.GetTactics(caller);
        return tactics[menuIndex];
    }
    */

    public override MenuResult GetResult()
    {
        List<BattleAction> tactics = BattleControl.Instance.GetTactics(caller);
        return new MenuResult(tactics[menuIndex]);
    }
    public override BaseBattleMenu.BaseMenuName GetMenuName() => BaseBattleMenu.BaseMenuName.Tactics;
}

public class BadgeSwapBoxMenu : BoxMenu
{
    List<Badge> badges;

    public override void Init()
    {
        base.Init();        

        //build these actions manually
        //these are pretty hardcoded
        //List<BattleAction> tactics = BattleControl.Instance.GetTactics(caller);

        badges = BattleControl.Instance.playerData.badgeInventory;
        List<Badge> partyBadges = BattleControl.Instance.playerData.partyEquippedBadges;
        List<Badge> wilexBadges = null;
        List<Badge> lunaBadges = null;

        if (BattleControl.Instance.playerData.GetPlayerDataEntry(BattleHelper.EntityID.Wilex) != null)
        {
            wilexBadges = BattleControl.Instance.playerData.GetPlayerDataEntry(BattleHelper.EntityID.Wilex).equippedBadges;
        }
        if (BattleControl.Instance.playerData.GetPlayerDataEntry(BattleHelper.EntityID.Luna) != null)
        {
            lunaBadges = BattleControl.Instance.playerData.GetPlayerDataEntry(BattleHelper.EntityID.Luna).equippedBadges;
        }

        BadgeMenuEntry.EquipType et = BadgeMenuEntry.EquipType.None;


        menuEntries = new BoxMenuEntry[badges.Count];
        for (int i = 0; i < badges.Count; i++)
        {
            et = BadgeMenuEntry.EquipType.None;
            if (partyBadges.Contains(badges[i]))
            {
                et = BadgeMenuEntry.EquipType.Party;
            }
            if (wilexBadges != null && wilexBadges.Contains(badges[i]))
            {
                et = BadgeMenuEntry.EquipType.Wilex;
            }
            if (lunaBadges != null && lunaBadges.Contains(badges[i]))
            {
                et = BadgeMenuEntry.EquipType.Luna;
            }

            bool canUse = (et != BadgeMenuEntry.EquipType.None);
            if (canUse)
            {
                canUse = (BattleControl.Instance.playerData.CalculateUsedSP() - Badge.GetSPCost(badges[i]) <= BattleControl.Instance.playerData.sp);
            } else
            {
                canUse = (BattleControl.Instance.playerData.CalculateUsedSP() + Badge.GetSPCost(badges[i]) <= BattleControl.Instance.playerData.sp);
            }


            if (MainManager.Instance.Cheat_BadgeAnarchy)
            {
                canUse = true;
            }

            //you can't take off rage's power but this also stops weird cheese strats?
            //put on badge on other character, other character moves, take off badge
            //can only unequip the badges you wear
            if (caller.entityID == BattleHelper.EntityID.Wilex && et == BadgeMenuEntry.EquipType.Luna)
            {
                canUse = false;
            }
            if (caller.entityID == BattleHelper.EntityID.Luna && et == BadgeMenuEntry.EquipType.Wilex)
            {
                canUse = false;
            }

            //forbid some badges
            BadgeDataEntry bde = Badge.GetBadgeDataEntry(badges[i]);

            if (!bde.badgeSwap)
            {
                canUse = false;
            }

            //menuEntries[i] = new TacticsMenuEntry(caller, tactics[i]);
            menuEntries[i] = new BadgeMenuEntry(badges[i], et, canUse, badges.FindAll((e) => (e.type == badges[i].type)).Count > 1);

            switch (et)
            {
                case BadgeMenuEntry.EquipType.None:
                    menuEntries[i].hasBackground = false;
                    break;
                case BadgeMenuEntry.EquipType.Wilex:
                    menuEntries[i].hasBackground = true;
                    menuEntries[i].backgroundColor = new Color(1f, 0.5f, 0.5f);
                    break;
                case BadgeMenuEntry.EquipType.Luna:
                    menuEntries[i].hasBackground = true;
                    menuEntries[i].backgroundColor = new Color(1f, 1f, 0.5f);
                    break;
                case BadgeMenuEntry.EquipType.Party:
                    menuEntries[i].hasBackground = true;
                    menuEntries[i].backgroundColor = new Color(1f, 0.75f, 0.5f);
                    break;
            }

            GameObject g = Instantiate(MainManager.Instance.menuEntryBase, bm.mask.transform);
            g.transform.localPosition = BoxMenuScript.GetRelativePosition(i);
            menuEntriesO.Add(g);
            BoxMenuEntryScript b = menuEntriesO[i].GetComponent<BoxMenuEntryScript>();
            b.Setup(menuEntries[i]);
        }

        //note: selecting a badge kicks you out of the menu so I don't need to put this there
        BattleControl.Instance.playerData.usedSP = BattleControl.Instance.playerData.CalculateUsedSP();
        descriptorString = "Uses Left: " + (BattleControl.Instance.playerData.BadgeEquippedCount(Badge.BadgeType.BadgeSwap) * 4 - BattleControl.Instance.badgeSwapUses) + ", <sp> SP Left: " + (BattleControl.Instance.playerData.sp - BattleControl.Instance.playerData.usedSP);

        PostEntriesInit();
    }
    public override void SelectOption()
    {
        MainManager.Instance.PlayGlobalSound(MainManager.Sound.Menu_Select);
        //Selecting a move takes you to a selection menu
        BattleAction action = BattleControl.Instance.badgeSwap;
        BadgeMenuEntry bme = (BadgeMenuEntry)menuEntries[menuIndex];
        ((BA_BadgeSwap)BattleControl.Instance.badgeSwap).badge = badges[menuIndex];
        ((BA_BadgeSwap)BattleControl.Instance.badgeSwap).et = bme.et;
        BattleSelectionMenu s3 = BattleSelectionMenu.buildMenu(caller, action.GetBaseTarget(), GetMenuName(), action, action.GetActionCommandDesc());
        s3.transform.SetParent(transform);
        PushState(s3);
        s3.menuExit += InvokeExit;
    }
    public override void SelectDisabled()
    {
        MainManager.Instance.PlayGlobalSound(MainManager.Sound.Menu_Error);
        BattlePopup popup = null;

        BadgeDataEntry bde = Badge.GetBadgeDataEntry(badges[menuIndex]);

        if (!bde.badgeSwap)
        {
            popup = new BattlePopup(PlayerMove.CantMoveReason.BadgeSwapDisabled);
        }
        else
        {
            if (BattleControl.Instance.playerData.equippedBadges.Contains(badges[menuIndex]))
            {
                popup = new BattlePopup(PlayerMove.CantMoveReason.BadgeUnavailable);
            }
            else
            {
                popup = new BattlePopup(PlayerMove.CantMoveReason.BadgeTooExpensive);
            }
        }

        BattlePopupMenuScript s = BattlePopupMenuScript.BuildMenu(popup.text);
        s.transform.SetParent(transform);
        PushState(s);
        s.menuExit += InvokeExit;
    }

    /*
    public override Move GetCurrent()
    {
        return null;
    }
    public override BattleAction GetAction()
    {
        List<BattleAction> tactics = BattleControl.Instance.GetTactics(caller);
        return tactics[menuIndex];
    }
    */

    public override MenuResult GetResult()
    {
        return new MenuResult(BattleControl.Instance.badgeSwap);
    }
    public override BaseBattleMenu.BaseMenuName GetMenuName() => BaseBattleMenu.BaseMenuName.BadgeSwap;
}

public class RibbonSwapBoxMenu : BoxMenu
{
    List<Ribbon> ribbons;

    public override void Init()
    {
        base.Init();

        ribbons = BattleControl.Instance.playerData.ribbonInventory;

        BadgeMenuEntry.EquipType et = BadgeMenuEntry.EquipType.None;
        PlayerData pd = BattleControl.Instance.playerData;

        menuEntries = new BoxMenuEntry[ribbons.Count];
        for (int i = 0; i < ribbons.Count; i++)
        {
            Ribbon wilexRibbon = pd.GetPlayerDataEntry(BattleHelper.EntityID.Wilex) != null ? pd.GetPlayerDataEntry(BattleHelper.EntityID.Wilex).ribbon : default;
            Ribbon lunaRibbon = pd.GetPlayerDataEntry(BattleHelper.EntityID.Wilex) != null ? pd.GetPlayerDataEntry(BattleHelper.EntityID.Luna).ribbon : default;

            et = BadgeMenuEntry.EquipType.None;
            if (ribbons[i].Equals(wilexRibbon))
            {
                et = BadgeMenuEntry.EquipType.Wilex;
            }
            if (ribbons[i].Equals(lunaRibbon))
            {
                et = BadgeMenuEntry.EquipType.Luna;
            }

            //menuEntries[i] = new TacticsMenuEntry(caller, tactics[i]);

            bool canUse = true;

            //can only unequip the ribbon you wear
            if (caller.entityID == BattleHelper.EntityID.Wilex && et == BadgeMenuEntry.EquipType.Luna)
            {
                canUse = false;
            }
            if (caller.entityID == BattleHelper.EntityID.Luna && et == BadgeMenuEntry.EquipType.Wilex)
            {
                canUse = false;
            }

            menuEntries[i] = new RibbonMenuEntry(ribbons[i], et, canUse, BattleControl.Instance.playerData.BadgeEquipped(Badge.BadgeType.RibbonPower));            

            switch (et)
            {
                case BadgeMenuEntry.EquipType.None:
                    menuEntries[i].hasBackground = false;
                    break;
                case BadgeMenuEntry.EquipType.Wilex:
                    menuEntries[i].hasBackground = true;
                    menuEntries[i].backgroundColor = new Color(1f, 0.5f, 0.5f);
                    break;
                case BadgeMenuEntry.EquipType.Luna:
                    menuEntries[i].hasBackground = true;
                    menuEntries[i].backgroundColor = new Color(1f, 1f, 0.5f);
                    break;
                case BadgeMenuEntry.EquipType.Party:
                    menuEntries[i].hasBackground = true;
                    menuEntries[i].backgroundColor = new Color(1f, 0.75f, 0.5f);
                    break;
            }            
            
            GameObject g = Instantiate(MainManager.Instance.menuEntryBase, bm.mask.transform);
            g.transform.localPosition = BoxMenuScript.GetRelativePosition(i);
            menuEntriesO.Add(g);
            BoxMenuEntryScript b = menuEntriesO[i].GetComponent<BoxMenuEntryScript>();
            b.Setup(menuEntries[i]);
        }

        PostEntriesInit();
    }
    public override void SelectOption()
    {
        MainManager.Instance.PlayGlobalSound(MainManager.Sound.Menu_Select);
        //Selecting a move takes you to a selection menu
        BattleAction action = BattleControl.Instance.ribbonSwap;
        RibbonMenuEntry bme = (RibbonMenuEntry)menuEntries[menuIndex];
        ((BA_RibbonSwap)BattleControl.Instance.ribbonSwap).ribbon = ribbons[menuIndex];
        ((BA_RibbonSwap)BattleControl.Instance.ribbonSwap).et = bme.et;
        BattleSelectionMenu s3 = BattleSelectionMenu.buildMenu(caller, action.GetBaseTarget(), GetMenuName(), action, action.GetActionCommandDesc());
        s3.transform.SetParent(transform);
        PushState(s3);
        s3.menuExit += InvokeExit;
    }
    public override void SelectDisabled()
    {
        MainManager.Instance.PlayGlobalSound(MainManager.Sound.Menu_Error);
        BattlePopup popup = new BattlePopup(PlayerMove.CantMoveReason.RibbonUnavailable);
        BattlePopupMenuScript s = BattlePopupMenuScript.BuildMenu(popup.text);
        s.transform.SetParent(transform);
        PushState(s);
        s.menuExit += InvokeExit;
    }

    /*
    public override Move GetCurrent()
    {
        return null;
    }
    public override BattleAction GetAction()
    {
        List<BattleAction> tactics = BattleControl.Instance.GetTactics(caller);
        return tactics[menuIndex];
    }
    */

    public override MenuResult GetResult()
    {
        return new MenuResult(BattleControl.Instance.ribbonSwap);
    }
    public override BaseBattleMenu.BaseMenuName GetMenuName() => BaseBattleMenu.BaseMenuName.RibbonSwap;
}