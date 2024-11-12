using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pause_HandlerItem : Pause_HandlerShared_SideTabs
{
    public new class UpdateObject
    {
        public int tabindex;
        public BattleHelper.EntityID player;

        public UpdateObject(int p_tabindex, BattleHelper.EntityID p_player)
        {
            tabindex = p_tabindex;
            player = p_player;
        }
    }

    #pragma warning disable CS0067
    public override event EventHandler<MenuExitEventArgs> menuExit;

    private const int MAX_ITEM_ELEMENTS = 2;

    public PlayerData playerData;
    public List<PlayerData.PlayerDataEntry> sortedParty;

    public BattleHelper.EntityID selectedPlayer = 0;    //use 0 because -1 is a value it can take

    public enum PauseItemPage
    {
        Items = 0,
        KeyItems
    }

    public static Pause_HandlerItem BuildMenu(Pause_SectionShared section = null)
    {
        GameObject newObj = new GameObject("Pause Item Menu");
        Pause_HandlerItem newMenu = newObj.AddComponent<Pause_HandlerItem>();

        newMenu.SetSubsection(section);

        newMenu.Init();


        return newMenu;
    }

    public override int GetMaxTabs()
    {
        return MAX_ITEM_ELEMENTS - 1;
    }
    public override int GetMaxLR()
    {
        return sortedParty.Count - 1;
    }
    public override int GetLRIndex()
    {
        return sortedParty.FindIndex((e) => (e.entityID == selectedPlayer));
    }
    public override void LRChange(int index)
    {
        selectedPlayer = sortedParty[index].entityID;
        //Debug.Log("Player " + selectedPlayer);
        if (section != null)
        {
            section.ApplyUpdate(new UpdateObject(tabindex, selectedPlayer));
            //Debug.Log("Post update");
        }
        //Debug.Log("Player " + selectedPlayer);
    }

    public override void SendSectionUpdate()
    {
        if (section != null)
        {
            section.ApplyUpdate(new UpdateObject(tabindex, selectedPlayer));
        }
    }


    public override void Select()
    {
        //even more submenus (but we are getting close to the end)
        Debug.Log((PauseItemPage)tabindex + " select");
        Pause_SectionShared newSection = null;
        if (section != null)
        {
            newSection = section.GetSubsection(new UpdateObject(tabindex, selectedPlayer));
        }
        //build the menu
        MenuHandler b = null;
        switch ((PauseItemPage)tabindex)
        {
            case PauseItemPage.Items:
                b = Pause_HandlerItem_ConsumableMenu.BuildMenu(newSection, playerData);
                break;
            case PauseItemPage.KeyItems:
                b = Pause_HandlerItem_KeyItemMenu.BuildMenu(newSection, playerData);
                break;
        }

        b.transform.parent = transform;
        PushState(b);
        b.menuExit += InvokeExit;
    }


    public override void Init()
    {
        active = true;
        lifetime = 0;

        tabindex = 0;
        playerData = MainManager.Instance.playerData;
        sortedParty = playerData.GetSortedParty();

        if (section != null)
        {
            UpdateObject uo = (UpdateObject)section.GetState();
            tabindex = uo.tabindex;
            selectedPlayer = uo.player;
        }

        if (selectedPlayer == 0)
        {
            selectedPlayer = sortedParty[0].entityID;
        }
        //base.Init();

        SendSectionUpdate();
        //Debug.Log(tabindex);
    }
}
