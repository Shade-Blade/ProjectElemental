using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Pause_HandlerEquip;

public class Pause_HandlerEquip_RibbonSelect : Pause_HandlerShared_BoxMenu
{
    //public int index = -1;
    //float inputDir;

    float lrDir;

    //public override event EventHandler<MenuExitEventArgs> menuExit;

    public BadgeSubpage subpage;   //unchanging in this menu
    public SortMode sortmode;

    Pause_HandlerEquip bm_parent;
    public BattleHelper.EntityID selectedPlayer;
    private List<Ribbon> ribbonList;

    public PlayerData playerData;
    public List<PlayerData.PlayerDataEntry> sortedParty;

    //public const int PAGE_SIZE = 8;

    public static Pause_HandlerEquip_RibbonSelect BuildMenu(Pause_SectionShared section, BadgeSubpage p_subpage, SortMode p_sortmode, List<Ribbon> p_ribbonList, Pause_HandlerEquip p_parent, PlayerData p_playerData, int selectIndex, BattleHelper.EntityID p_selectedPlayer)
    {
        GameObject newObj = new GameObject("Pause Ribbon Select Menu");
        Pause_HandlerEquip_RibbonSelect newMenu = newObj.AddComponent<Pause_HandlerEquip_RibbonSelect>();

        newMenu.SetSubsection(section);
        newMenu.subpage = p_subpage;
        newMenu.sortmode = p_sortmode;

        newMenu.ribbonList = new List<Ribbon>();
        for (int i = 0; i < p_ribbonList.Count; i++)
        {
            newMenu.ribbonList.Add(p_ribbonList[i]);
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
        if (section != null)
        {
            section.ApplyUpdate(new Pause_HandlerEquip_BadgeSelect.UpdateObject(index, selectedPlayer, index));
        }
        bm_parent.SetSelectIndex(index);
    }
    public override void SendUpdate()
    {
        if (section != null)
        {
            section.ApplyUpdate(new Pause_HandlerEquip_BadgeSelect.UpdateObject(index, selectedPlayer));
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
                Debug.Log("Player " + selectedPlayer);
            }
        }
    }

    public override int GetObjectCount()
    {
        return ribbonList.Count;
    }
    public override void Select()
    {
        //MainManager.Instance.PlayGlobalSound(MainManager.Sound.Menu_Select);
        //even more submenus (but we are getting close to the end)
        Debug.Log(ribbonList[index] + " select");

        //No submenus
        Ribbon b = ribbonList[index];

        bool equipOrUnequip = false;

        for (int i = 0; i < sortedParty.Count; i++)
        {
            if (sortedParty[i].ribbon.Equals(b))
            {
                equipOrUnequip = true;
            }
        }

        if (equipOrUnequip)
        {
            MainManager.Instance.PlayGlobalSound(MainManager.Sound.Menu_UnequipRibbon);
            //unequip
            for (int i = 0; i < sortedParty.Count; i++)
            {
                if (sortedParty[i].ribbon.Equals(b))
                {
                    sortedParty[i].ribbon = new Ribbon(Ribbon.RibbonType.None);
                }
            }
        }
        else
        {
            MainManager.Instance.PlayGlobalSound(MainManager.Sound.Menu_EquipRibbon);
            //equip (note: implicitly forces 1 ribbon per character)
            for (int i = 0; i < sortedParty.Count; i++)
            {
                if (sortedParty[i].entityID == selectedPlayer)
                {
                    sortedParty[i].ribbon = b;
                }
            }
        }

        /*
        Debug.Log("--Result--");
        for (int i = 0; i < sortedParty.Count; i++)
        {
            Debug.Log("Ribbon on " + sortedParty[i].entityID + " is " + sortedParty[i].ribbon);
        }
        Debug.Log("--End Result--");
        */

        playerData.UpdateMaxStats();

        SendUpdate(index);
    }

    public override void Init()
    {
        if (index == -1)
        {
            index = 0;
        }
        //index = 0;

        base.Init();

        ResetUpdate();

        //MainManager.ListPrint(ribbonList, index);
    }

    public override MenuResult GetResult()
    {
        return new MenuResult(index);
    }
}
