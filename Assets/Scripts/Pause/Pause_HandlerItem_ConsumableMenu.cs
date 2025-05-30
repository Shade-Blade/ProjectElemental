using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Pause_HandlerItem;

public class Pause_HandlerItem_ConsumableMenu : Pause_HandlerShared_BoxMenu
{
    public class UpdateObject
    {
        public int index;
        public BattleHelper.EntityID player;

        public UpdateObject(int p_index, BattleHelper.EntityID p_player)
        {
            index = p_index;
            player = p_player;
        }
    }

    //public int index = -1;
    //float inputDir;
    //float holdDur;
    //int holdValue;

    //public override event EventHandler<MenuExitEventArgs> menuExit;

    //public const float HYPER_SCROLL_TIME = 0.3f;
    float lrDir;
    public PlayerData playerData;
    public List<PlayerData.PlayerDataEntry> sortedParty;

    public BattleHelper.EntityID selectedPlayer = 0;    //use 0 because -1 is a value it can take

    private List<Item> itemList;

    public static Pause_HandlerItem_ConsumableMenu BuildMenu(Pause_SectionShared section = null, PlayerData p_playerData = null)
    {
        GameObject newObj = new GameObject("Pause Item Consumable Menu");
        Pause_HandlerItem_ConsumableMenu newMenu = newObj.AddComponent<Pause_HandlerItem_ConsumableMenu>();

        newMenu.SetSubsection(section);

        newMenu.playerData = p_playerData;
        newMenu.sortedParty = p_playerData.GetSortedParty();

        newMenu.Init();


        return newMenu;
    }

    public override void Reinit()
    {
        section.ApplyUpdate(null);  //special thing
        base.Reinit();
    }

    public override int GetObjectCount()
    {
        return itemList.Count;
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
                //Debug.Log("Player " + selectedPlayer);
            }
        }
    }

    public override void SendUpdate()
    {
        if (section != null)
        {
            section.ApplyUpdate(new UpdateObject(index, selectedPlayer));
        }
    }
    List<Item> GetItemList()
    {
        return MainManager.Instance.playerData.itemInventory;
    }

    public override void Select()
    {       
        //even more submenus (but we are getting close to the end)
        bool canSelect = Item.CanUseOutOfBattle(itemList[index].type);

        if (canSelect)
        {
            Debug.Log(itemList[index] + " select");
            MainManager.Instance.PlayGlobalSound(MainManager.Sound.Menu_Select);
        }
        else
        {
            Debug.Log(itemList[index] + " can't select");
            MainManager.Instance.PlayGlobalSound(MainManager.Sound.Menu_Error);
            return;
        }

        //this selection may be more complex
        //but I may be fine with just a selection menu
        //The cases are similar to the battle selection menu (0 entities, 1 entity, multiple entities)
        //even more submenus (but we are getting close to the end)
        Pause_SectionShared newSection = null;
        if (section != null)
        {
            newSection = section.GetSubsection(new UpdateObject(index, selectedPlayer));
        }
        //build the menu
        MenuHandler b = null;
        b = Pause_HandlerItem_CharacterSelect.BuildMenu(newSection, MainManager.Instance.playerData, itemList[index], index);

        b.transform.parent = transform;
        PushState(b);
        b.menuExit += InvokeExit;
    }

    public override void Init()
    {
        itemList = GetItemList();


        base.Init();

        if (section != null)
        {
            UpdateObject uo = (UpdateObject)(section.GetState());
            int newIndex = uo.index;
            selectedPlayer = uo.player;
            index = newIndex;
        }

        if (index > itemList.Count - 1)
        {
            index = itemList.Count - 1;
        }
        if (index < 0)
        {
            index = 0;
        }

        if (section != null)
        {
            section.ApplyUpdate(new UpdateObject(index, selectedPlayer));
        }
    }

    public override MenuResult GetResult()
    {
        return new MenuResult(index);
    }
}
