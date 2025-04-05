using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Pause_HandlerEquip;
//using static Pause_HandlerItem;

public class Pause_HandlerEquip_BadgeSelect : Pause_HandlerShared_BoxMenu
{
    public class UpdateObject
    {
        public int index;
        public BattleHelper.EntityID player;
        public int selectIndex;

        public UpdateObject(int p_index, BattleHelper.EntityID p_player, int p_selectIndex = -1)
        {
            index = p_index;
            player = p_player;
            selectIndex = p_selectIndex;
        }
    }

    //public int index = -1;
    //float inputDir;

    float lrDir;

    //public override event EventHandler<MenuExitEventArgs> menuExit;

    public BadgeSubpage subpage;   //unchanging in this menu
    public SortMode sortmode;

    Pause_HandlerEquip bm_parent;
    public BattleHelper.EntityID selectedPlayer;
    private List<Badge> badgeList;

    public PlayerData playerData;
    public List<PlayerData.PlayerDataEntry> sortedParty;

    //public const int PAGE_SIZE = 8;

    public static Pause_HandlerEquip_BadgeSelect BuildMenu(Pause_SectionShared section, BadgeSubpage p_subpage, SortMode p_sortmode, List<Badge> p_badgeList, Pause_HandlerEquip p_parent, PlayerData p_playerData, int selectIndex, BattleHelper.EntityID p_selectedPlayer)
    {
        GameObject newObj = new GameObject("Pause Badge Select Menu");
        Pause_HandlerEquip_BadgeSelect newMenu = newObj.AddComponent<Pause_HandlerEquip_BadgeSelect>();

        newMenu.SetSubsection(section);
        newMenu.subpage = p_subpage;
        newMenu.sortmode = p_sortmode;

        newMenu.badgeList = new List<Badge>();
        for (int i = 0; i < p_badgeList.Count; i++)
        {
            newMenu.badgeList.Add(p_badgeList[i]);
        }

        newMenu.bm_parent = p_parent;
        newMenu.playerData = p_playerData;
        newMenu.sortedParty = p_playerData.GetSortedParty();
        newMenu.index = selectIndex;
        newMenu.selectedPlayer = p_selectedPlayer;

        newMenu.Init();


        return newMenu;
    }

    public void SendUpdate(int i)
    {
        //bm_parent.SetEntityID(selectedPlayer);
        if (section != null)
        {
            section.ApplyUpdate(new UpdateObject(index, selectedPlayer, index));
        }
        bm_parent.SetSelectIndex(index);
    }
    public override void SendUpdate()
    {
        bm_parent.SetEntityID(selectedPlayer);
        if (section != null)
        {
            section.ApplyUpdate(new UpdateObject(index, selectedPlayer));
        }
        bm_parent.SetSelectIndex(index);
    }

    public void ResetUpdate()
    {
        if (section != null)
        {
            section.ApplyUpdate(null);
        }
        SendUpdate();
    }   

    /*
    void Update()
    {
        if (active)
        {
            MenuUpdate();
        }
    }
    */

    /*
    void MenuUpdate()
    {
        if (Mathf.Sign(MainManager.GetAxisVertical()) != -inputDir || MainManager.GetAxisVertical() == 0)
        {
            inputDir = -Mathf.Sign(MainManager.GetAxisVertical());
            if (MainManager.GetAxisVertical() == 0)
            {
                inputDir = 0;
            }
            //Debug.Log(InputManager.GetAxisHorizontal());
            //now go
            if (inputDir != 0)
            {
                if (inputDir > 0)
                {
                    index++;
                }
                else
                {
                    index--;
                }
            }

            if (index > badgeList.Count - 1)
            {
                //index = badgeList.Count - 1;
                index = 0;
            }
            if (index < 0)
            {
                //index = 0;
                index = badgeList.Count - 1;
            }

            if (inputDir != 0)
            {
                if (section != null)
                {
                    section.ApplyUpdate(new UpdateObject(index, selectedPlayer));
                }
                //Debug.Log(badgeList[index]);
                bm_parent.SetSelectIndex(index);
                //MainManager.ListPrint(badgeList, index);
            }
        }

        if (MainManager.GetButtonDown(InputManager.Button.Z))
        {
            index += PAGE_SIZE;

            if (index > badgeList.Count - 1)
            {
                //index = badgeList.Count - 1;
                index = 0;
            }
            if (index < 0)
            {
                //index = 0;
                index = badgeList.Count - 1;
            }

            if (section != null)
            {
                section.ApplyUpdate(index);
            }
            //Debug.Log(badgeList[index]);
            bm_parent.SetSelectIndex(index);
            MainManager.ListPrint(badgeList, index);
        }

        if (MainManager.GetButtonDown(InputManager.Button.Y))
        {
            index -= PAGE_SIZE;

            if (index > badgeList.Count - 1)
            {
                //index = badgeList.Count - 1;
                index = 0;
            }
            if (index < 0)
            {
                //index = 0;
                index = badgeList.Count - 1;
            }

            if (section != null)
            {
                section.ApplyUpdate(new UpdateObject(index, selectedPlayer));
            }
            //Debug.Log(badgeList[index]);
            bm_parent.SetSelectIndex(index);
            MainManager.ListPrint(badgeList, index);
        }

        if (Mathf.Sign(MainManager.GetAxisHorizontal()) != lrDir || MainManager.GetAxisHorizontal() == 0)
        {
            lrDir = Mathf.Sign(MainManager.GetAxisHorizontal());

            if (MainManager.GetAxisHorizontal() == 0)
            {
                lrDir = 0;
            }

            if (lrDir != 0)
            {
                int indexB = sortedParty.FindIndex((e) => (e.entityID == selectedPlayer));

                if (lrDir > 0)
                {
                    indexB++;
                } else
                {
                    indexB--;
                }

                if (indexB < 0)
                {
                    indexB = sortedParty.Count - 1;
                }
                if (indexB > sortedParty.Count - 1)
                {
                    indexB = 0;
                }

                selectedPlayer = sortedParty[indexB].entityID;
                bm_parent.SetEntityID(selectedPlayer);
                if (section != null)
                {
                    section.ApplyUpdate(new UpdateObject(index, selectedPlayer));
                }
                Debug.Log("Player " + selectedPlayer);
            }
        }

        //bm_parent.SetEntityID(selectedPlayer);

        if (MainManager.GetButtonDown(InputManager.Button.A)) //Press A to select stuff
        {
            //go to submenu
            Select();
        }
        if (MainManager.GetButtonDown(InputManager.Button.B)) //Press B to go back
        {
            PopSelf();
        }
    }
    */
    public override void MenuUpdate()
    {
        base.MenuUpdate();

        if ((lifetime > MIN_SELECT_TIME && Mathf.Sign(InputManager.GetAxisHorizontal()) != lrDir) || InputManager.GetAxisHorizontal() == 0)
        {
            lrDir = Mathf.Sign(InputManager.GetAxisHorizontal());

            if (InputManager.GetAxisHorizontal() == 0)
            {
                lrDir = 0;
            }

            if (lrDir != 0)
            {
                int indexB = sortedParty.FindIndex((e) => (e.entityID == selectedPlayer));

                if (lrDir > 0)
                {
                    indexB++;

                    if (sortedParty.Count - 1 > 0)
                    {
                        MainManager.Instance.PlayGlobalSound(MainManager.Sound.Menu_BSwap);
                    }
                }
                else
                {
                    indexB--;

                    if (sortedParty.Count - 1 > 0)
                    {
                        MainManager.Instance.PlayGlobalSound(MainManager.Sound.Menu_BSwap);
                    }
                }

                if (indexB < 0)
                {
                    indexB = sortedParty.Count - 1;
                }
                if (indexB > sortedParty.Count - 1)
                {
                    indexB = 0;
                }

                selectedPlayer = sortedParty[indexB].entityID;
                SendUpdate();
                if (subpage == BadgeSubpage.SingleEquipped)
                {
                    badgeList = ((Pause_HandlerEquip)parent).RebuildListAndGet();
                }
                //Debug.Log("Player " + selectedPlayer);
            }
        }
    }

    public override int GetObjectCount()
    {
        return badgeList.Count;
    }

    public override void Select()
    {        
        //even more submenus (but we are getting close to the end)
        //Debug.Log(badgeList[index] + " select");

        //No submenus
        Badge b = badgeList[index];

        bool equipOrUnequip = false;

        equipOrUnequip = playerData.equippedBadges.Contains(b);

        BadgeDataEntry bde = Badge.GetBadgeDataEntry(b);
        int cost = bde.cost;

        if (equipOrUnequip)
        {
            //unequip

            //Unequipping might not be allowed >:)
            if (!MainManager.Instance.Cheat_BadgeAnarchy && (playerData.CalculateUsedSP() - cost > playerData.sp))
            {
                //No
                MainManager.Instance.PlayGlobalSound(MainManager.Sound.Menu_Error);
            }
            else
            {
                MainManager.Instance.PlayGlobalSound(MainManager.Sound.Menu_Unequip);
                playerData.equippedBadges.Remove(b);

                if (bde.singleOrParty)
                {
                    playerData.partyEquippedBadges.Remove(b);
                }
                else
                {
                    for (int i = 0; i < sortedParty.Count; i++)
                    {
                        sortedParty[i].equippedBadges.Remove(b);
                    }
                }
            }
        } else
        {
            //equip
            if (MainManager.Instance.GetGlobalFlag(MainManager.GlobalFlag.GF_FileCode_Envy))
            {
                if (cost > 1)
                {
                    cost = 1;
                }
            }

            if (!MainManager.Instance.Cheat_BadgeAnarchy && (playerData.CalculateUsedSP() + cost > playerData.sp))
            {
                //No
                MainManager.Instance.PlayGlobalSound(MainManager.Sound.Menu_Error);
            } else
            {
                MainManager.Instance.PlayGlobalSound(MainManager.Sound.Menu_Equip);
                if (bde.singleOrParty)
                {
                    playerData.equippedBadges.Add(b);
                    playerData.partyEquippedBadges.Add(b);

                    playerData.equippedBadges.Sort((a, b) => (a.type - b.type));
                    playerData.partyEquippedBadges.Sort((a, b) => (a.type - b.type));
                }
                else
                {
                    playerData.equippedBadges.Add(b);
                    playerData.GetPlayerDataEntry(selectedPlayer).equippedBadges.Add(b);

                    playerData.equippedBadges.Sort((a, b) => (a.type - b.type));
                    playerData.GetPlayerDataEntry(selectedPlayer).equippedBadges.Sort((a, b) => (a.type - b.type));
                }
            }
        }
        playerData.usedSP = playerData.CalculateUsedSP();

        /*
        Debug.Log("--Result--");
        MainManager.ListPrint(playerData.equippedBadges);
        for (int i = 0; i < sortedParty.Count; i++)
        {
            Debug.Log("Badges from " + sortedParty[i].entityID);
            MainManager.ListPrint(sortedParty[i].equippedBadges);
        }
        Debug.Log("--End Result--");
        */

        playerData.UpdateMaxStats();

        SendUpdate(index);
        //ResetUpdate();
    }

    public override void Init()
    {
        //Readding this so that state is preserved when you scroll away from the badge menu        
        if (section is Pause_SectionEquip_Inventory)
        {
            Pause_SectionEquip_Inventory p_se_i = (Pause_SectionEquip_Inventory)section;
            index = p_se_i.menuIndex;
            selectedPlayer = p_se_i.selectedPlayer;
        }
        

        //Debug.Log(index);
        if (index == -1)
        {
            index = 0;
        }
        //index = 0;
        playerData.usedSP = playerData.CalculateUsedSP();

        ResetUpdate();

        base.Init();

        //MainManager.ListPrint(badgeList, index);
    }

    public override MenuResult GetResult()
    {
        return new MenuResult(index);
    }
}
