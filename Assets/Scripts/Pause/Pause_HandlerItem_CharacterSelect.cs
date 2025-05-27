using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pause_HandlerItem_CharacterSelect : Pause_HandlerShared
{
    public int index = -1;
    float inputDir;

    #pragma warning disable CS0067
    public override event EventHandler<MenuExitEventArgs> menuExit;

    private PlayerData playerData;
    public List<PlayerData.PlayerDataEntry> sortedParty;

    private Item item;
    private int itemIndex;

    bool allPossible;

    //note: targetarea can't be used as it is a battle only construct
    //but the stuff required to set up out of battle things is relatively simple

    //isAttackItem -> can't select
    //battle only -> can't select
    //revive / miracle -> (no change)

    //dual -> dual
    //So in most cases it is just selecting one (but dual selects both)

    public static Pause_HandlerItem_CharacterSelect BuildMenu(Pause_SectionShared section = null, PlayerData p_playerData = null, Item p_item = default, int p_index = -1)
    {
        GameObject newObj = new GameObject("Pause Item Character Select Menu");
        Pause_HandlerItem_CharacterSelect newMenu = newObj.AddComponent<Pause_HandlerItem_CharacterSelect>();

        newMenu.SetSubsection(section);
        newMenu.playerData = p_playerData;
        newMenu.sortedParty = p_playerData.GetSortedParty();
        newMenu.item = p_item;
        newMenu.itemIndex = p_index;
        newMenu.Init();


        return newMenu;
    }

    void Update()
    {
        if (active)
        {
            MenuUpdate();
        }
    }

    void MenuUpdate()
    {
        lifetime += Time.deltaTime;
        //if (!allPossible)
        //{
        if ((lifetime > MIN_SELECT_TIME && Mathf.Sign(InputManager.GetAxisHorizontal()) != -inputDir) || InputManager.GetAxisHorizontal() == 0)
        {
            inputDir = -Mathf.Sign(InputManager.GetAxisHorizontal());
            if (InputManager.GetAxisHorizontal() == 0)
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

            if (index > sortedParty.Count - 1)
            {
                index = 0;
            }
            if (index < 0)
            {
                index = sortedParty.Count - 1;
            }

            if (inputDir != 0)
            {
                if (section != null)
                {
                    section.ApplyUpdate(sortedParty[index].entityID);
                    section.ApplyUpdate(null);
                }
                //Debug.Log(sortedParty[index]);
            }
        }
        //}

        if (lifetime > MIN_SELECT_TIME && InputManager.GetButtonDown(InputManager.Button.A)) //Press A to select stuff
        {
            //go to submenu
            Select();
        }
        if (lifetime > MIN_SELECT_TIME && InputManager.GetButtonDown(InputManager.Button.B)) //Press B to go back
        {
            PopSelf();
        }
    }

    void Select()
    {
        //even more submenus (but we are getting close to the end)
        MainManager.Instance.PlayGlobalSound(MainManager.Sound.SFX_Item_Use);
        Item.UseOutOfBattle(sortedParty[index], item, itemIndex);
        section.ApplyUpdate(sortedParty[index].entityID);
        //you go back automatically
        PopSelf();
    }

    public override void Init()
    {
        //index = 0;
        lifetime = 0;
        BattleHelper.EntityID eid = (BattleHelper.EntityID)section.GetState();
        index = sortedParty.IndexOf(playerData.GetPlayerDataEntry(eid));

        allPossible = Item.GetProperty(item.type, Item.ItemProperty.TargetAll) != null;
        section.ApplyUpdate(sortedParty[index].entityID);
        section.ApplyUpdate(null);
        base.Init();
    }


    public override MenuResult GetResult()
    {
        return new MenuResult(index);
    }
}
