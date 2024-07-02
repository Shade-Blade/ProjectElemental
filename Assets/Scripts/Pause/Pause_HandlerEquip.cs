using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pause_HandlerEquip : Pause_HandlerShared_SideTabs
{
    public new class UpdateObject
    {
        public int tabindex;
        public BattleHelper.EntityID player;
        public int badgeIndex;
        public SortMode sortMode;

        public UpdateObject(int p_tabindex, BattleHelper.EntityID p_player, int p_badgeIndex, SortMode p_sortMode)
        {
            tabindex = p_tabindex;
            player = p_player;
            badgeIndex = p_badgeIndex;
            sortMode = p_sortMode;
        }
    }

    public int sortIndex = -1;

    #pragma warning disable CS0067
    public override event EventHandler<MenuExitEventArgs> menuExit;

    private const int MAX_BADGE_ELEMENTS = 4;

    public enum BadgeSubpage
    {
        AllBadges = 0,
        AllEquipped,
        SingleEquipped,
        Ribbons
    }

    public enum SortMode
    {
        Default,
        Order,
        Alphabet,
        Cost,
    }

    public PlayerData playerData;
    public List<PlayerData.PlayerDataEntry> sortedParty;

    public List<Badge> badgeList;
    public List<Ribbon> ribbonList;

    //set by changing the menu (you can't change in this menu, which is not normally how these menus work)
    //  the submenus change stuff in the main menu, which is not normal
    public BattleHelper.EntityID selectedPlayer = 0;    //use 0 because -1 is a value it can take
    public int badgeIndex = -1;

    public static Pause_HandlerEquip BuildMenu(Pause_SectionShared section = null)
    {
        GameObject newObj = new GameObject("Pause Badge Menu");
        Pause_HandlerEquip newMenu = newObj.AddComponent<Pause_HandlerEquip>();

        newMenu.SetSubsection(section);

        newMenu.Init();


        return newMenu;
    }

    void RebuildList(int lastIndex = -1)
    {
        if (lastIndex == -1)
        {
            lastIndex = tabindex;
        }
        //Debug.Log("Selected index was " + badgeIndex + " in mode " + (BadgeSubpage)lastIndex);

        Badge badge = new Badge(Badge.BadgeType.None);
        Ribbon ribbon = new Ribbon(Ribbon.RibbonType.None);
        if ((BadgeSubpage)lastIndex != BadgeSubpage.Ribbons)
        {
            if (badgeList != null && badgeIndex > -1 && badgeIndex < badgeList.Count)
            {
                badge = badgeList[badgeIndex];
            }
        } else
        {
            if (ribbonList != null && badgeIndex > -1 && badgeIndex < ribbonList.Count)
            {
                ribbon = ribbonList[badgeIndex];
            }
        }

        if ((BadgeSubpage)tabindex == BadgeSubpage.Ribbons)
        {
            if (sortIndex > 2)
            {
                sortIndex = 0;
            }
        }

        //Debug.Log("Ribbon is " + ribbon);

        switch ((BadgeSubpage)tabindex)
        {
            case BadgeSubpage.AllBadges:
                badgeList = new List<Badge>();
                for (int i = 0; i < playerData.badgeInventory.Count; i++)
                {
                    badgeList.Add(playerData.badgeInventory[i]);
                }
                //badgeList = playerData.badgeInventory
                break;
            case BadgeSubpage.AllEquipped:
                badgeList = new List<Badge>();
                for (int i = 0; i < playerData.equippedBadges.Count; i++)
                {
                    badgeList.Add(playerData.equippedBadges[i]);
                }
                //badgeList = playerData.equippedBadges;
                break;
            case BadgeSubpage.SingleEquipped:
                badgeList = new List<Badge>();
                for (int i = 0; i < playerData.GetPlayerDataEntry(selectedPlayer).equippedBadges.Count; i++)
                {
                    badgeList.Add(playerData.GetPlayerDataEntry(selectedPlayer).equippedBadges[i]);
                }
                //badgeList = playerData.GetPlayerDataEntry(selectedPlayer).equippedBadges;
                break;
            case BadgeSubpage.Ribbons:
                ribbonList = new List<Ribbon>();
                for (int i = 0; i < playerData.ribbonInventory.Count; i++)
                {
                    ribbonList.Add(playerData.ribbonInventory[i]);
                }
                //ribbonList = playerData.ribbonInventory;
                break;
        }



        SortMode sm = (SortMode)sortIndex;
        if ((BadgeSubpage)tabindex != BadgeSubpage.Ribbons)
        {
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
            }
        } else
        {
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

                        int k = (int)a.Item1.type - (int)b.Item1.type;
                        return k;
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
            }
        }

        if ((BadgeSubpage)tabindex == BadgeSubpage.Ribbons)
        {
            if (ribbonList != null && ribbon.type != Ribbon.RibbonType.None && (BadgeSubpage)tabindex == BadgeSubpage.Ribbons)
            {
                int oldSelect = badgeIndex;
                badgeIndex = ribbonList.IndexOf(ribbon);
                if (badgeIndex == -1)
                {
                    badgeIndex = 0;
                }
                //Debug.Log("translation of " + ribbon + " to " + selectIndex + " (was " + oldSelect + ")");
            }
            else
            {
                if (ribbon.type != Ribbon.RibbonType.None)
                {
                    badgeIndex = 0;
                }
                else
                {
                    //leave it?
                    if (badgeIndex > ribbonList.Count - 1 || badgeIndex < 0)
                    {
                        badgeIndex = 0;
                    }
                }
            }
        }
        else
        {
            if (badgeList != null && badge.type != Badge.BadgeType.None && (BadgeSubpage)tabindex != BadgeSubpage.Ribbons)
            {
                int oldSelect = badgeIndex;
                badgeIndex = badgeList.IndexOf(badge);
                if (badgeIndex == -1)
                {
                    badgeIndex = 0;
                }
                //Debug.Log("translation of " + badge + " to " + selectIndex + " (was " + oldSelect + ")");
            }
            else
            {
                if (badge.type != Badge.BadgeType.None)
                {
                    badgeIndex = 0;
                } else
                {
                    //leave it?
                    if (badgeIndex > badgeList.Count - 1 || badgeIndex < 0)
                    {
                        badgeIndex = 0;
                    }
                }
            }
        }

        //Debug.Log("Rebuild list, select index is now " + badgeIndex);
    }

    public List<Badge> RebuildListAndGet()
    {
        RebuildList();
        return badgeList;
    }

    public override int GetMaxTabs()
    {
        return MAX_BADGE_ELEMENTS - 1 + (!RibbonsAvailable() ? -1 : 0);
    }
    public override int GetLRIndex()
    {
        return sortedParty.FindIndex((e) => (e.entityID == selectedPlayer));
    }
    public override int GetMaxLR()
    {
        return sortedParty.Count - 1;
    }
    public override void IndexChange(int lastIndex, int tabIndex)
    {
        RebuildList(lastIndex);
    }
    public override void LRChange(int index)
    {
        selectedPlayer = sortedParty[index].entityID;
        RebuildList();
        SendSectionUpdate();
    }

    public override void SendSectionUpdate()
    {
        if (section != null)
        {
            section.ApplyUpdate(new UpdateObject(tabindex, selectedPlayer, badgeIndex, (SortMode)sortIndex));
        }
        //Debug.Log("Player " + selectedPlayer);
    }

    public override void Select()
    {
        //go to badge select handler
        //Debug.Log(index);

        Pause_SectionShared newSection = null;
        if (section != null)
        {
            newSection = section.GetSubsection(new UpdateObject(tabindex, selectedPlayer, badgeIndex, (SortMode)sortIndex));
        }
        //build the menu
        MenuHandler b = null;
        switch ((BadgeSubpage)tabindex)
        {
            case BadgeSubpage.AllBadges:
            case BadgeSubpage.AllEquipped:
            case BadgeSubpage.SingleEquipped:
                b = Pause_HandlerEquip_BadgeSelect.BuildMenu(newSection, (BadgeSubpage)tabindex, (SortMode)sortIndex, badgeList, this, playerData, badgeIndex, selectedPlayer);
                break;
            case BadgeSubpage.Ribbons:
                b = Pause_HandlerEquip_RibbonSelect.BuildMenu(newSection, (BadgeSubpage)tabindex, (SortMode)sortIndex, ribbonList, this, playerData, badgeIndex, selectedPlayer);
                break;
        }

        b.transform.parent = transform;
        PushState(b);
        b.menuExit += InvokeExit;
    }
    public override void ZOption()
    {
        sortIndex++;
        if (sortIndex > 3)
        {
            sortIndex = 0;
        }
        if ((BadgeSubpage)tabindex == BadgeSubpage.Ribbons)
        {
            if (sortIndex > 2)
            {
                sortIndex = 0;
            }
        }

        RebuildList(tabindex);
        if (section != null)
        {
            section.ApplyUpdate(new UpdateObject(tabindex, selectedPlayer, badgeIndex, (SortMode)sortIndex));
        }
    }

    public void SetSelectIndex(int i)
    {
        badgeIndex = i;
        //Debug.Log(selectIndex + " setup");
    }
    public void SetEntityID(BattleHelper.EntityID eid)
    {
        selectedPlayer = eid;
        if (section != null)
        {
            ((Pause_SectionEquip)section).UpdateCharacterSection(eid);
        }
    }

    public override void Init()
    {
        active = true;
        if (section != null)
        {
            UpdateObject uo = (UpdateObject)section.GetState();

            badgeIndex = uo.badgeIndex;
            tabindex = uo.tabindex;
            selectedPlayer = uo.player;
            sortIndex = (int)uo.sortMode;
        }
        //Debug.Log("Badge " + badgeIndex);

        //index = 0;
        if (tabindex == -1)
        {
            tabindex = 0;
        }
        if (sortIndex == -1)
        {
            sortIndex = 0;
        }
        //selectIndex = -1;
        if (badgeIndex == -1)
        {
            badgeIndex = 0;
        }
        playerData = MainManager.Instance.playerData;
        sortedParty = playerData.GetSortedParty();
        if (selectedPlayer == 0)
        {
            selectedPlayer = sortedParty[0].entityID;
        }
        //Debug.Log(selectedPlayer);
        RebuildList();
        //Debug.Log("Selected index: " + selectIndex);
        if (section != null)
        {
            section.ApplyUpdate(new UpdateObject(tabindex, selectedPlayer, badgeIndex, (SortMode)sortIndex));
        }
        //base.Init();
        //Debug.Log("Badge " + badgeIndex);

        SendSectionUpdate();
        //Debug.Log(tabindex);
    }

    public static bool RibbonsAvailable()
    {
        return true;
    }
}
