using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using static Pause_HandlerEquip;

public class Pause_SectionEquip_Inventory : Pause_SectionShared_BoxMenu
{
    public GameObject subobject;
    public TextDisplayer noItemText;
    public Pause_SectionShared_CharacterStats characterStats;

    public Pause_SectionEquip parent;

    public Pause_HandlerEquip.BadgeSubpage bs;
    public string emptyString;

    public BattleHelper.EntityID selectedPlayer;

    public Pause_HandlerEquip.SortMode sortMode;


    public override void ApplyUpdate(object state)
    {
        if (state == null)  //resets the inventory
        {
            Clear();
            Init();

            //Debug.Log("Inventory section reset: " + menuIndex + " " + menuTopIndex + " " + selectedPlayer);
            return;
        }

        Pause_HandlerEquip_BadgeSelect.UpdateObject uo = (Pause_HandlerEquip_BadgeSelect.UpdateObject)state;

        int index = uo.index;
        menuIndex = index;

        int selectedIndex = uo.selectIndex;

        bool updateRequired = (selectedPlayer != uo.player) && (bs == Pause_HandlerEquip.BadgeSubpage.SingleEquipped);
        //Debug.Log("Inventory sp: " + selectedPlayer + " vs " + uo.player);
        selectedPlayer = uo.player;
        if (updateRequired)
        {
            Clear();
            Init();
        }
        //Debug.Log("Inventory sp: " + selectedPlayer);

        selectorArrow.gameObject.SetActive(true);
        selectorArrow.color = new Color(1, 1, 1, 1);

        List<PlayerData.PlayerDataEntry> sortedParty = MainManager.Instance.playerData.GetSortedParty();        


        if (menuTopIndex > menuIndex)
        {
            menuTopIndex = menuIndex;
        }
        if (menuTopIndex < menuIndex - MENU_SIZE_PER_PAGE + 1)
        {
            menuTopIndex = menuIndex - MENU_SIZE_PER_PAGE + 1;
        }

        //constrain it in the correct range
        if (menuTopIndex > menuEntries.Length - MENU_SIZE_PER_PAGE)
        {
            menuTopIndex = menuEntries.Length - MENU_SIZE_PER_PAGE;
        }
        if (menuTopIndex < 0)
        {
            menuTopIndex = 0;
        }

        //BoxMenuScript bm = menuBaseO.GetComponent<BoxMenuScript>();

        //put this in init too
        upArrow.enabled = menuTopIndex > 0;
        downArrow.enabled = menuTopIndex < menuEntries.Length - MENU_SIZE_PER_PAGE && menuEntries.Length > MENU_SIZE_PER_PAGE;

        if (textbox != null)
        {
            if (menuIndex > menuEntries.Length - 1)
            {
                if (textbox.isActiveAndEnabled)
                {
                    textbox.SetText("", true, true);
                }
                textbox.transform.parent.transform.parent.gameObject.SetActive(false);
            } else
            {
                textbox.transform.parent.transform.parent.gameObject.SetActive(true);
                textbox.SetText(menuEntries[menuIndex].description, true, true);
            }
        }

        //update the selected badge
        //better than my old system of updating everything (probably gets very laggy)
        if (selectedIndex != -1)
        {
            PlayerData pd = MainManager.Instance.playerData;
            if (bs == BadgeSubpage.Ribbons)
            {
                Ribbon r = ((RibbonMenuEntry)menuEntries[selectedIndex]).r;
                BadgeMenuEntry.EquipType et = BadgeMenuEntry.EquipType.None;

                if (pd.GetPlayerDataEntry(BattleHelper.EntityID.Wilex) != null && pd.GetPlayerDataEntry(BattleHelper.EntityID.Wilex).ribbon.Equals(r))
                {
                    et = BadgeMenuEntry.EquipType.Wilex;
                }
                if (pd.GetPlayerDataEntry(BattleHelper.EntityID.Luna) != null && pd.GetPlayerDataEntry(BattleHelper.EntityID.Luna).ribbon.Equals(r))
                {
                    et = BadgeMenuEntry.EquipType.Luna;
                }

                bool power = pd.BadgeEquipped(Badge.BadgeType.RibbonPower);
                menuEntries[selectedIndex] = new RibbonMenuEntry(r, et, true, power);

                //do an update

                //which menu entry to update?
                int selectedScriptIndex = selectedIndex - loadedTopIndex;

                EquipBoxMenuEntryScript ebmes = (EquipBoxMenuEntryScript)menuEntriesS[selectedScriptIndex];
                ebmes.Setup((RibbonMenuEntry)(menuEntries[selectedIndex]));

                //seek out the others?
                if (et != BadgeMenuEntry.EquipType.None)
                {
                    for (int i = 0; i < menuEntries.Length; i++)
                    {
                        if (i == selectedIndex)
                        {
                            continue;
                        }

                        RibbonMenuEntry prme = (RibbonMenuEntry)(menuEntries[i]);

                        if (et == prme.et)
                        {
                            //Debug.Log(i + " is to be replaced " + prme.name);
                            prme.et = BadgeMenuEntry.EquipType.None;
                            //menuEntries[i] = new PauseRibbonMenuEntry(((PauseRibbonMenuEntry)menuEntries[i]).r, PauseBadgeMenuEntry.EquipType.None);
                            int selectedScriptIndexB = i - loadedTopIndex;

                            EquipBoxMenuEntryScript ebmesB = (EquipBoxMenuEntryScript)menuEntriesS[selectedScriptIndexB];
                            ebmesB.Setup(prme);
                        }
                    }
                }
            }
            else
            {
                Badge b = ((BadgeMenuEntry)menuEntries[selectedIndex]).b;
                //Debug.Log("Evaluate: " + b);

                BadgeMenuEntry.EquipType et = BadgeMenuEntry.EquipType.None;
                if (pd.partyEquippedBadges.Contains(b))
                {
                    et = BadgeMenuEntry.EquipType.Party;
                }

                if (pd.GetPlayerDataEntry(BattleHelper.EntityID.Wilex) != null && pd.GetPlayerDataEntry(BattleHelper.EntityID.Wilex).equippedBadges.Contains(b))
                {
                    et = BadgeMenuEntry.EquipType.Wilex;
                }
                if (pd.GetPlayerDataEntry(BattleHelper.EntityID.Luna) != null && pd.GetPlayerDataEntry(BattleHelper.EntityID.Luna).equippedBadges.Contains(b))
                {
                    et = BadgeMenuEntry.EquipType.Luna;
                }
                menuEntries[selectedIndex] = new BadgeMenuEntry(b, et);

                //do an update

                //which menu entry to update?
                int selectedScriptIndex = selectedIndex - loadedTopIndex;

                EquipBoxMenuEntryScript ebmes = (EquipBoxMenuEntryScript)menuEntriesS[selectedScriptIndex];
                ebmes.Setup((BadgeMenuEntry)(menuEntries[selectedIndex]));

                //unfortunately I still have to update everything's "can use" value (which changes based on whenever you equip or unequip badges, so you end up in this if statement)
                for (int i = 0; i < menuEntries.Length; i++)
                {
                    BadgeMenuEntry pbme = ((BadgeMenuEntry)menuEntries[i]);

                    //Only unequipped badges can become unusable or usable
                    if (pbme.et == BadgeMenuEntry.EquipType.None)
                    {
                        //Reevaluate
                        bool pastCanUse = pbme.canUse;

                        int usedSP = pd.usedSP;

                        bool newCanUse = usedSP + Badge.GetSPCost(pbme.b) <= pd.sp;
                        if (MainManager.Instance.Cheat_BadgeAnarchy)
                        {
                            newCanUse = true;
                        }

                        pbme.canUse = newCanUse;

                        //Debug.Log("pbme " + pbme.b + " " + pbme.canUse);

                        //A change
                        //if (pastCanUse != newCanUse)
                        //{

                        //not every menu entry corresponds to a loaded script
                        int newIndex = i - loadedTopIndex;
                        if (newIndex >= 0 && newIndex <= menuEntriesS.Count - 1)
                        {
                            pbme.canUse = newCanUse;

                            ebmes = (EquipBoxMenuEntryScript)menuEntriesS[newIndex];
                            ebmes.Setup(pbme);
                        }

                        //}
                    }
                }
            }
        }

        characterStats.ApplyUpdate(selectedPlayer);
        parent.UpdateSPUsage();

        //Debug.Log("Inventory section update: " + menuIndex + " " + menuTopIndex + " " + selectedPlayer);
        //Debug.Log("Selected index " + selectedIndex);
        return;
    }

    
    public override object GetState()
    {
        return new Pause_HandlerEquip_BadgeSelect.UpdateObject(menuIndex, selectedPlayer);
    }
    

    public List<Badge> SortBadges(List<Badge> sortBadges)
    {
        SortMode sm = sortMode;
        List<Badge> badgeList = sortBadges;

        switch (sm)
        {
            case SortMode.Alphabet:
                //wacky way of doing it to avoid too many GetName calls (though sorting may be efficient enough to make this not really that good)
                List<Tuple<Badge, string>> badgeNamePairs = new List<Tuple<Badge, string>>();
                for (int i = 0; i < badgeList.Count; i++)
                {
                    badgeNamePairs.Add(new Tuple<Badge, string>(badgeList[i], Badge.GetName(badgeList[i])));
                }
                badgeNamePairs.Sort((a, b) =>
                {
                    int alpha = string.Compare(a.Item2, b.Item2);

                    if (alpha != 0)
                    {
                        return alpha;
                    }

                    int k = (int)a.Item1.type - (int)b.Item1.type;
                    if (k != 0)
                    {
                        return k;
                    }

                    return a.Item1.badgeCount - b.Item1.badgeCount;
                });
                //rebuild the badge list
                badgeList = new List<Badge>();
                for (int i = 0; i < badgeNamePairs.Count; i++)
                {
                    badgeList.Add(badgeNamePairs[i].Item1);
                }
                break;
            case SortMode.Order:
                List<Badge> listB = new List<Badge>();
                for (int i = 0; i < badgeList.Count; i++)
                {
                    listB.Add(badgeList[i]);
                }
                badgeList = listB;
                badgeList.Sort((a, b) => (a.badgeCount - b.badgeCount));
                break;
            case SortMode.Cost:
                //wacky way of doing it to avoid too many table references
                List<Tuple<Badge, int>> badgeCostPairs = new List<Tuple<Badge, int>>();
                for (int i = 0; i < badgeList.Count; i++)
                {
                    badgeCostPairs.Add(new Tuple<Badge, int>(badgeList[i], Badge.GetSPCost(badgeList[i])));
                }
                badgeCostPairs.Sort((a, b) =>
                {
                    int alpha = a.Item2 - b.Item2;

                    if (alpha != 0)
                    {
                        return alpha;
                    }

                    int k = (int)a.Item1.type - (int)b.Item1.type;
                    if (k != 0)
                    {
                        return k;
                    }

                    return a.Item1.badgeCount - b.Item1.badgeCount;
                });
                //rebuild the badge list
                badgeList = new List<Badge>();
                for (int i = 0; i < badgeCostPairs.Count; i++)
                {
                    badgeList.Add(badgeCostPairs[i].Item1);
                }
                break;
            case SortMode.Default:
                List<Badge> listC = new List<Badge>();
                for (int i = 0; i < badgeList.Count; i++)
                {
                    listC.Add(badgeList[i]);
                }
                badgeList = listC;
                badgeList.Sort((a, b) =>
                {
                    int alpha = a.type - b.type;

                    if (alpha != 0)
                    {
                        return alpha;
                    }

                    return a.badgeCount - b.badgeCount;

                });
                break;
        }

        return badgeList;
    }

    public List<Ribbon> SortRibbons(List<Ribbon> sortRibbons)
    {
        SortMode sm = sortMode;
        List<Ribbon> ribbonList = sortRibbons;

        switch (sm)
        {
            case SortMode.Alphabet:
                //wacky way of doing it to avoid too many GetName calls (though sorting may be efficient enough to make this not really that good)
                List<Tuple<Ribbon, string>> ribbonNamePairs = new List<Tuple<Ribbon, string>>();
                for (int i = 0; i < ribbonList.Count; i++)
                {
                    ribbonNamePairs.Add(new Tuple<Ribbon, string>(ribbonList[i], Ribbon.GetName(ribbonList[i])));
                }
                ribbonNamePairs.Sort((a, b) =>
                {
                    int alpha = string.Compare(a.Item2, b.Item2);

                    if (alpha != 0)
                    {
                        return alpha;
                    }

                    return (int)a.Item1.type - (int)b.Item1.type;
                });
                //rebuild the badge list
                ribbonList = new List<Ribbon>();
                for (int i = 0; i < ribbonNamePairs.Count; i++)
                {
                    ribbonList.Add(ribbonNamePairs[i].Item1);
                }
                break;
            case SortMode.Order:
                List<Ribbon> listB = new List<Ribbon>();
                for (int i = 0; i < ribbonList.Count; i++)
                {
                    listB.Add(ribbonList[i]);
                }
                ribbonList = listB;
                ribbonList.Sort((a, b) => (a.ribbonCount - b.ribbonCount));
                break;
            case SortMode.Default:
                List<Ribbon> listC = new List<Ribbon>();
                for (int i = 0; i < ribbonList.Count; i++)
                {
                    listC.Add(ribbonList[i]);
                }
                ribbonList = listC;
                ribbonList.Sort((a, b) =>
                {
                    int alpha = a.type - b.type;

                    if (alpha != 0)
                    {
                        return alpha;
                    }

                    return a.ribbonCount - b.ribbonCount;
                });
                break;
        }

        return ribbonList;
    }

    public override void Init()
    {
        //Debug.Log("Equip inventory " + menuIndex);
        if (menuIndex == -1)
        {
            menuIndex = 0;
            menuTopIndex = 0;
        }
        if (menuTopIndex > menuIndex)
        {
            menuTopIndex = menuIndex;
        }
        if (menuTopIndex < menuIndex - MENU_SIZE_PER_PAGE + 1)
        {
            menuTopIndex = menuIndex - MENU_SIZE_PER_PAGE + 1;
        }

        visualTopIndex = menuTopIndex;
        visualSelectIndex = menuIndex;
        int desiredLoadedTopIndex = Mathf.FloorToInt(visualTopIndex) - MENU_BUFFER;   //higher up
        if (desiredLoadedTopIndex < 0)
        {
            desiredLoadedTopIndex = 0;
        }
        loadedTopIndex = desiredLoadedTopIndex;

        /*
        menuEntries = new BoxMenuEntry[caller.jumpMoves.Count];
        for (int i = 0; i < caller.jumpMoves.Count; i++)
        {
            menuEntries[i] = new MoveMenuEntry(caller, caller.jumpMoves[i]);
            BoxMenuEntryScript b = menuEntriesO[i].GetComponent<BoxMenuEntryScript>();
            b.Setup(menuEntries[i]);
        }
        */

        //get items from item inventory

        PlayerData pd = MainManager.Instance.playerData;

        switch (bs)
        {
            case Pause_HandlerEquip.BadgeSubpage.AllBadges:
                List<Badge> inv = pd.badgeInventory;
                inv = SortBadges(inv);

                menuEntries = new BoxMenuEntry[inv.Count];
                for (int i = 0; i < inv.Count; i++)
                {
                    BadgeMenuEntry.EquipType et = BadgeMenuEntry.EquipType.None;
                    if (pd.partyEquippedBadges.Contains(inv[i]))
                    {
                        et = BadgeMenuEntry.EquipType.Party;
                    }

                    int wilexIndex = -1;
                    for (int j = 0; j < pd.party.Count; j++)
                    {
                        if (pd.party[j].entityID == BattleHelper.EntityID.Wilex)
                        {
                            wilexIndex = j;
                        }
                    }

                    int lunaIndex = -1;
                    for (int j = 0; j < pd.party.Count; j++)
                    {
                        if (pd.party[j].entityID == BattleHelper.EntityID.Luna)
                        {
                            lunaIndex = j;
                        }
                    }

                    if (wilexIndex != -1 && pd.party[wilexIndex].equippedBadges.Contains(inv[i]))
                    {
                        et = BadgeMenuEntry.EquipType.Wilex;
                    }
                    if (lunaIndex != -1 && pd.party[lunaIndex].equippedBadges.Contains(inv[i]))
                    {
                        et = BadgeMenuEntry.EquipType.Luna;
                    }

                    bool canUse = (et != BadgeMenuEntry.EquipType.None) || pd.CalculateUsedSP() + Badge.GetSPCost(inv[i]) <= pd.sp;
                    menuEntries[i] = new BadgeMenuEntry(inv[i], et, canUse);
                }
                emptyString = "No Badges";
                break;
            case Pause_HandlerEquip.BadgeSubpage.AllEquipped:
                List<Badge> einv = pd.equippedBadges;
                einv = SortBadges(einv);

                menuEntries = new BoxMenuEntry[einv.Count];
                for (int i = 0; i < einv.Count; i++)
                {
                    BadgeMenuEntry.EquipType et = BadgeMenuEntry.EquipType.None;
                    if (pd.partyEquippedBadges.Contains(einv[i]))
                    {
                        et = BadgeMenuEntry.EquipType.Party;
                    }

                    int wilexIndex = -1;
                    for (int j = 0; j < pd.party.Count; j++)
                    {
                        if (pd.party[j].entityID == BattleHelper.EntityID.Wilex)
                        {
                            wilexIndex = j;
                        }
                    }

                    int lunaIndex = -1;
                    for (int j = 0; j < pd.party.Count; j++)
                    {
                        if (pd.party[j].entityID == BattleHelper.EntityID.Luna)
                        {
                            lunaIndex = j;
                        }
                    }

                    if (wilexIndex != -1 && pd.party[wilexIndex].equippedBadges.Contains(einv[i]))
                    {
                        et = BadgeMenuEntry.EquipType.Wilex;
                    }
                    if (lunaIndex != -1 && pd.party[lunaIndex].equippedBadges.Contains(einv[i]))
                    {
                        et = BadgeMenuEntry.EquipType.Luna;
                    }

                    bool canUse = (et != BadgeMenuEntry.EquipType.None) || pd.CalculateUsedSP() + Badge.GetSPCost(einv[i]) <= pd.sp;
                    menuEntries[i] = new BadgeMenuEntry(einv[i], et, canUse);
                }
                emptyString = "No Equipped Badges";
                break;
            case Pause_HandlerEquip.BadgeSubpage.SingleEquipped:
                //Debug.Log("Generate all of " + selectedPlayer + "'s badges");
                List<Badge> sinv = pd.GetPlayerDataEntry(selectedPlayer).equippedBadges;
                sinv = SortBadges(sinv);

                //pd.GetPlayerDataEntry(playerIndex)

                menuEntries = new BoxMenuEntry[sinv.Count];
                for (int i = 0; i < sinv.Count; i++)
                {
                    BadgeMenuEntry.EquipType etB = BadgeMenuEntry.EquipType.None;

                    if (selectedPlayer == BattleHelper.EntityID.Wilex)
                    {
                        etB = BadgeMenuEntry.EquipType.Wilex;
                    }
                    if (selectedPlayer == BattleHelper.EntityID.Luna)
                    {
                        etB = BadgeMenuEntry.EquipType.Luna;
                    }

                    bool canUse = (etB != BadgeMenuEntry.EquipType.None) || pd.CalculateUsedSP() + Badge.GetSPCost(sinv[i]) <= pd.sp;
                    menuEntries[i] = new BadgeMenuEntry(sinv[i], etB, canUse);
                }
                emptyString = "No Equipped Badges";
                break;
            case Pause_HandlerEquip.BadgeSubpage.Ribbons:
                List<Ribbon> rinv = pd.ribbonInventory;
                rinv = SortRibbons(rinv);

                menuEntries = new BoxMenuEntry[rinv.Count];
                for (int i = 0; i < rinv.Count; i++)
                {
                    BadgeMenuEntry.EquipType etR = BadgeMenuEntry.EquipType.None;

                    Ribbon wilexRibbon = pd.GetPlayerDataEntry(BattleHelper.EntityID.Wilex) != null ? pd.GetPlayerDataEntry(BattleHelper.EntityID.Wilex).ribbon : default;
                    Ribbon lunaRibbon = pd.GetPlayerDataEntry(BattleHelper.EntityID.Luna) != null ? pd.GetPlayerDataEntry(BattleHelper.EntityID.Luna).ribbon : default;

                    if (rinv[i].Equals(wilexRibbon))
                    {
                        etR = BadgeMenuEntry.EquipType.Wilex;
                    }
                    if (rinv[i].Equals(lunaRibbon))
                    {
                        etR = BadgeMenuEntry.EquipType.Luna;
                    }

                    bool power = pd.BadgeEquipped(Badge.BadgeType.RibbonPower);
                    menuEntries[i] = new RibbonMenuEntry(rinv[i], etR, true, power);
                }

                emptyString = "No Ribbons";
                break;
        }



        if (menuEntries == null)
        {
            menuEntries = new BoxMenuEntry[0];
        }

        //constrain it in the correct range
        if (menuTopIndex > menuEntries.Length - MENU_SIZE_PER_PAGE)
        {
            menuTopIndex = menuEntries.Length - MENU_SIZE_PER_PAGE;
        }
        if (menuTopIndex < 0)
        {
            menuTopIndex = 0;
        }


        menuEntriesS = new List<BoxMenuEntryScript>();

        for (int i = 0; i < MENU_SIZE_PER_PAGE + MENU_BUFFER * 2; i++)
        {
            if (i > menuEntries.Length)
            {
                //No reason to have more menu entries
                break;
            }

            if (i + loadedTopIndex > menuEntries.Length - 1)
            {
                //make some entries anyway to avoid making too few menu entries for later
                GameObject gB = Instantiate(pauseMenuEntry, mask.transform);
                gB.transform.localPosition = GetRelativePosition(i + loadedTopIndex - visualTopIndex);

                EquipBoxMenuEntryScript bB = gB.GetComponent<EquipBoxMenuEntryScript>();
                menuEntriesS.Add(bB);
                bB.Setup((BadgeMenuEntry)null);    //lol
                //break;
            } else
            {
                GameObject g = Instantiate(pauseMenuEntry, mask.transform);
                g.transform.localPosition = GetRelativePosition(i + loadedTopIndex - visualTopIndex);

                EquipBoxMenuEntryScript b = g.GetComponent<EquipBoxMenuEntryScript>();
                menuEntriesS.Add(b);

                if (bs == BadgeSubpage.Ribbons)
                {
                    b.Setup((RibbonMenuEntry)(menuEntries[i + loadedTopIndex]));
                }
                else
                {
                    b.Setup((BadgeMenuEntry)(menuEntries[i + loadedTopIndex]));
                }
            }
        }

        upArrow.enabled = menuEntries.Length > 0 && menuTopIndex > 0;
        downArrow.enabled = menuEntries.Length > 0 && (menuTopIndex < menuEntries.Length - MENU_SIZE_PER_PAGE && menuEntries.Length > MENU_SIZE_PER_PAGE);

        visualTopIndex = MainManager.EasingQuadraticTime(visualTopIndex, menuTopIndex, 25);
        for (int i = 0; i < menuEntriesS.Count; i++)
        {
            //Debug.Log(i - menuTopIndex);
            //menuEntriesO[i].transform.localPosition = BoxMenuScript.GetRelativePosition(i - menuTopIndex);
            menuEntriesS[i].transform.localPosition = GetRelativePosition(i + loadedTopIndex - visualTopIndex);
        }

        if (menuEntries.Length > 0)
        {
            noItemText.gameObject.SetActive(false);
        } else
        {
            //create some text
            noItemText.gameObject.SetActive(true);
            noItemText.SetText(emptyString, true, true);
            //noItemText.SetText(???, true, true);
        }

        if (menuIndex > menuEntries.Length - 1)
        {
            menuIndex = 0;
        }

        if (menuEntries.Length == 0)
        {
            selectorArrow.enabled = false;
        }
        else
        {
            if (menuIndex < 0 || menuIndex > menuEntriesS.Count - 1)
            {
                //menuIndex = 0;
            }
            selectorArrow.enabled = true;
            Vector3 targetLocal = Vector3.left * 160f + Vector3.up * 20 + GetRelativePosition(visualSelectIndex - visualTopIndex) + Vector3.up * ARROW_OFFSET;
            Vector3 current = selectorArrow.transform.localPosition;
            selectorArrow.transform.localPosition = targetLocal;
        }
        //bm.selectorArrow.transform.localPosition = MainManager.EasingQuadraticTime(current, targetLocal, 450);

        //only do this when you get to the menu
        //textbox.SetText(menuEntries[menuIndex].description, true, true);
        selectedPlayer = (BattleHelper.EntityID)(characterStats.GetState());
        //Debug.Log("Character stats says the current player is " + selectedPlayer);
        if (selectedPlayer == 0)
        {
            selectedPlayer = pd.GetSortedParty()[0].entityID;
            characterStats.ApplyUpdate(pd.GetSortedParty()[0].entityID);
        }
        //characterStats.ApplyUpdate(selectedPlayer);        
    }

    public override void Clear()
    {
        for (int i = 0; i < menuEntriesS.Count; i++)
        {
            Destroy(menuEntriesS[i].gameObject);
        }
        menuEntriesS = null;
        isInit = false;
    }

    //I think the only difference is the menu entries being set up
    public override void Update()
    {
        if (visualTopIndex != menuTopIndex)
        {
            visualTopIndex = MainManager.EasingQuadraticTime(visualTopIndex, menuTopIndex, 100);

            int desiredLoadedTopIndex = Mathf.FloorToInt(visualTopIndex) - MENU_BUFFER;   //higher up

            if (desiredLoadedTopIndex < 0)
            {
                desiredLoadedTopIndex = 0;
            }

            if (desiredLoadedTopIndex != loadedTopIndex)
            {
                //make the difference by juggling the scripts around
                int shift = desiredLoadedTopIndex - loadedTopIndex;

                shift = -shift;

                if (shift > 0)
                {
                    if (shift > menuEntriesS.Count - 1)
                    {
                        shift = menuEntriesS.Count - 1;
                    }

                    //take stuff from end
                    for (int i = 0; i < shift; i++)
                    {
                        BoxMenuEntryScript bmes = menuEntriesS[menuEntriesS.Count - 1];
                        menuEntriesS.RemoveAt(menuEntriesS.Count - 1);
                        menuEntriesS.Insert(0, bmes);
                        bmes.gameObject.SetActive(true);
                    }

                    //there are now (shift) things at the start that need to be modified
                    for (int i = 0; i < shift; i++)
                    {
                        if (bs == BadgeSubpage.Ribbons)
                        {
                            ((EquipBoxMenuEntryScript)(menuEntriesS[i])).Setup((RibbonMenuEntry)(menuEntries[i + desiredLoadedTopIndex]));
                        }
                        else
                        {
                            ((EquipBoxMenuEntryScript)(menuEntriesS[i])).Setup((BadgeMenuEntry)(menuEntries[i + desiredLoadedTopIndex]));
                        }
                    }
                }
                else
                {
                    shift = -shift;

                    if (shift > menuEntriesS.Count - 1)
                    {
                        shift = menuEntriesS.Count - 1;
                    }

                    //take stuff from start
                    for (int i = 0; i < shift; i++)
                    {
                        BoxMenuEntryScript bmes = menuEntriesS[0];
                        menuEntriesS.RemoveAt(0);
                        menuEntriesS.Add(bmes);
                        bmes.gameObject.SetActive(true);
                    }

                    //there are now (shift) things at the end that need to be modified
                    for (int i = menuEntriesS.Count - shift; i < menuEntriesS.Count; i++)
                    {
                        if (i + desiredLoadedTopIndex > menuEntries.Length - 1)
                        {
                            menuEntriesS[i].gameObject.SetActive(false);
                            break;
                        }
                        if (bs == BadgeSubpage.Ribbons)
                        {
                            ((EquipBoxMenuEntryScript)(menuEntriesS[i])).Setup((RibbonMenuEntry)(menuEntries[i + desiredLoadedTopIndex]));
                        }
                        else
                        {
                            ((EquipBoxMenuEntryScript)(menuEntriesS[i])).Setup((BadgeMenuEntry)(menuEntries[i + desiredLoadedTopIndex]));
                        }
                    }
                }

                loadedTopIndex = desiredLoadedTopIndex;
            }

            for (int i = 0; i < menuEntriesS.Count; i++)
            {
                //Debug.Log(i - menuTopIndex);
                //menuEntriesO[i].transform.localPosition = BoxMenuScript.GetRelativePosition(i - menuTopIndex);
                menuEntriesS[i].transform.localPosition = GetRelativePosition(i + loadedTopIndex - visualTopIndex);
            }
        }

        if (menuEntries != null && menuEntries.Length > 0)
        {
            visualSelectIndex = MainManager.EasingQuadraticTime(visualSelectIndex, menuIndex, 400);

            if (visualSelectIndex < visualTopIndex)
            {
                visualSelectIndex = visualTopIndex;
            }
            if (visualSelectIndex > visualTopIndex + MENU_SIZE_PER_PAGE - 1)
            {
                visualSelectIndex = visualTopIndex + MENU_SIZE_PER_PAGE - 1;
            }
            Vector3 next = Vector3.left * 160f + Vector3.up * 20 + GetRelativePosition(visualSelectIndex - visualTopIndex) + Vector3.up * ARROW_OFFSET;
            selectorArrow.transform.localPosition = next;

            //bm.selectorArrow.transform.localPosition = Vector3.left * 170f + menuEntriesO[menuIndex].transform.localPosition + Vector3.up * 7.5f;
            //Debug.Log(menuEntries[menuIndex].description);
        }
    }
}
