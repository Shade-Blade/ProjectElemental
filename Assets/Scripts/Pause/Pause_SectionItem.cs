using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Pause_SectionItem : Pause_SectionShared_SideTabs
{
    //side tabs are the base level of the item menu

    //public GameObject subobject;

    public Pause_SectionItem_Inventory inventory;
    public Pause_SectionShared_CharacterStats characterStats;

    public TextDisplayer itemCounter;
    public TextDisplayer keyCounter;

    public Image selectorArrow;

    /*
    public override void ApplyUpdate(object state)
    {
        return;
    }
    public override object GetState()
    {
        return null;
    }
    */

    public override void ApplyUpdate(object state)
    {
        selectorArrow.color = new Color(0.5f, 0.5f, 0.5f, 1);
        if (state == null)
        {
            return;
        }

        Pause_HandlerItem.UpdateObject uo = (Pause_HandlerItem.UpdateObject)state;


        int index = uo.tabindex;
        tabIndex = index;


        Pause_HandlerItem.PauseItemPage pip = (Pause_HandlerItem.PauseItemPage)tabIndex;

        PlayerData pd = MainManager.Instance.playerData;

        UpdateItemCount();

        characterStats.ApplyUpdate(uo.player);

        inventory.Clear();
        inventory.selectedPlayer = uo.player;
        if (inventory.selectedPlayer == 0)
        {
            inventory.selectedPlayer = pd.GetSortedParty()[0].entityID;
        }
        inventory.pip = pip;
        inventory.Init();
        characterStats.ApplyUpdate(inventory.selectedPlayer);
        return;
    }
    public override object GetState()
    {
        Pause_HandlerItem.UpdateObject uo = new Pause_HandlerItem.UpdateObject(tabIndex, inventory.selectedPlayer);
        return uo;
    }
    public override Pause_SectionShared GetSubsection(object state)
    {
        ApplyUpdate(state);
        return subsections[0];
    }

    public void UpdateItemCount()
    {
        PlayerData pd = MainManager.Instance.playerData;
        itemCounter.SetText(pd.itemInventory.Count + "/" + pd.GetMaxInventorySize(), true, true);
        keyCounter.SetText(pd.keyInventory.Count + " Key", true, true);
    }

    public override void Init()
    {
        if (tabIndex < 0)
        {
            tabIndex = 0;
        }

        subobject.SetActive(true);

        selectorArrow.gameObject.SetActive(false);

        Pause_HandlerItem.PauseItemPage pip = (Pause_HandlerItem.PauseItemPage)tabIndex;
        inventory.pip = pip;

        PlayerData pd = MainManager.Instance.playerData;
        inventory.selectedPlayer = (BattleHelper.EntityID)(characterStats.GetState());
        if (inventory.selectedPlayer == 0)
        {
            inventory.selectedPlayer = pd.GetSortedParty()[0].entityID;
            characterStats.ApplyUpdate(pd.GetSortedParty()[0].entityID);
        }
        UpdateItemCount();
        inventory.Init();
        characterStats.ApplyUpdate(characterStats.entityID);

        base.Init();
    }

    public override void Clear()
    {
        inventory.Clear();
        subobject.SetActive(false);
        base.Clear();
    }
}
