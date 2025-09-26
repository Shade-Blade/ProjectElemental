using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static KeyItem;

public class Pause_HandlerItem_KeyItemMenu : Pause_HandlerShared_BoxMenu
{
    //public override event EventHandler<MenuExitEventArgs> menuExit;

    //public const float HYPER_SCROLL_TIME = 0.3f;
    float lrDir;
    public PlayerData playerData;
    public List<PlayerData.PlayerDataEntry> sortedParty;

    public BattleHelper.EntityID selectedPlayer = 0;    //use 0 because -1 is a value it can take

    private List<KeyItem> keyItemList;

    public bool awaitUse;
    public PromptBoxMenu prompt;

    public static Pause_HandlerItem_KeyItemMenu BuildMenu(Pause_SectionShared section = null, PlayerData p_playerData = null)
    {
        GameObject newObj = new GameObject("Pause Key Item Menu");
        Pause_HandlerItem_KeyItemMenu newMenu = newObj.AddComponent<Pause_HandlerItem_KeyItemMenu>();

        newMenu.SetSubsection(section);

        newMenu.playerData = p_playerData;
        newMenu.sortedParty = p_playerData.GetSortedParty();

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

    public override void Reinit()
    {
        section.ApplyUpdate(null);  //special thing
        base.Reinit();
    }

    public override int GetObjectCount()
    {
        return keyItemList.Count;
    }

    /*
    void MenuUpdate()
    {
        if (itemList.Count > 0)
        {
            if (Mathf.Sign(MainManager.GetAxisVertical()) != -inputDir || MainManager.GetAxisVertical() == 0)
            {
                holdDur = 0;
                holdValue = 0;
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

                if (index > itemList.Count - 1)
                {
                    //index = itemList.Count - 1;
                    index = 0;
                }
                if (index < 0)
                {
                    //index = 0;
                    index = itemList.Count - 1;
                }

                if (inputDir != 0)
                {
                    //Debug.Log("Apply update? " + (section != null));
                    if (section != null)
                    {
                        section.ApplyUpdate(index);
                    }
                    //MainManager.ListPrint(itemList, index);
                    //Debug.Log(itemList[index]);
                }
            }

            if (Mathf.Sign(MainManager.GetAxisVertical()) == -inputDir && MainManager.GetAxisVertical() != 0)
            {
                holdDur += Time.deltaTime;

                if (holdDur >= HYPER_SCROLL_TIME)
                {
                    int pastHoldValue = holdValue;

                    if (MainManager.Instance.GetHyperScrollRate() * (holdDur - HYPER_SCROLL_TIME) > holdValue)
                    {
                        holdValue = (int)(MainManager.Instance.GetHyperScrollRate() * (holdDur - HYPER_SCROLL_TIME));
                    }

                    if (inputDir > 0)
                    {
                        index += (holdValue - pastHoldValue);
                    }
                    else
                    {
                        index -= (holdValue - pastHoldValue);
                    }

                    //No loop around
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
                        section.ApplyUpdate(index);
                    }
                }
            }

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
        else
        {
            //No items

            if (MainManager.GetButtonDown(InputManager.Button.B)) //Press B to go back
            {
                PopSelf();
            }
        }
    }
    */
    public override void MenuUpdate()
    {
        //improper to make the menu work like this but ehh
        if (prompt != null && !prompt.menuDone)
        {
            //Reset these so that things don't become sus after you select something
            inputDir = 0;
            holdDur = 0;
            holdValue = 0;
            return;
        }
        if (prompt != null && prompt.menuDone && awaitUse)
        {
            int index = prompt.menuIndex;
            prompt.Clear();
            Destroy(prompt.gameObject);
            prompt = null;
            if (index == 0)
            {
                UseKeyItem();
            }
            awaitUse = false;
            return;
        }

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
            section.ApplyUpdate(new Pause_HandlerItem_ConsumableMenu.UpdateObject(index, selectedPlayer));
        }
    }
    List<KeyItem> GetKeyItemList()
    {
        return MainManager.Instance.playerData.keyInventory;
    }

    public override void Select()
    {
        bool canSelect = KeyItem.CanUse(keyItemList[index]);

        if (canSelect)
        {
            Debug.Log(keyItemList[index] + " select");
            MainManager.Instance.PlayGlobalSound(MainManager.Sound.Menu_Select);
        }
        else
        {
            Debug.Log(keyItemList[index] + " can't select");
            MainManager.Instance.PlayGlobalSound(MainManager.Sound.Menu_Error);
            return;
        }

        MainManager.Instance.PlayGlobalSound(MainManager.Sound.Menu_Select);
        prompt = PromptBoxMenu.BuildMenu(new string[] { "Yes", "No" }, new string[] { "0", "1" }, 1, "Do you want to use the " + KeyItem.GetName(keyItemList[index]) + "?");
        awaitUse = true;
    }    

    public void UseKeyItem()
    {
        //I will have to manually create scripts for every single key item (or at least one per each "kind" of usable key item)

        PlayerData pd = MainManager.Instance.playerData;

        KeyItem targetItem = keyItemList[index];

        //Do something
        Debug.Log("Use " + targetItem.type);
        //KeyItem.UseKeyItem(targetItem);

        //Problem: Key items may require multiple menus which have to be tied pretty tightly to wherever they are used from
        //Therefore I cannot have a global UseKeyItem method

        switch (targetItem.type)
        {
            case KeyItemType.PlainCandle:
                pd.ClearInnEffect();
                pd.FullHeal();
                break;
            case KeyItemType.PeachCandle:
                pd.AddInnEffect(InnEffect.InnType.Health);
                pd.FullHeal();
                break;
            case KeyItemType.StrawberryCandle:
                pd.AddInnEffect(InnEffect.InnType.Energy);
                pd.FullHeal();
                break;
            case KeyItemType.AppleCandle:
                pd.AddInnEffect(InnEffect.InnType.Absorb);
                pd.FullHeal();
                break;
            case KeyItemType.LemonCandle:
                pd.AddInnEffect(InnEffect.InnType.Stamina);
                pd.FullHeal();
                break;
            case KeyItemType.WatermelonCandle:
                pd.AddInnEffect(InnEffect.InnType.Burst);
                pd.FullHeal();
                break;
            case KeyItemType.CherryCandle:
                pd.AddInnEffect(InnEffect.InnType.Focus);
                pd.FullHeal();
                break;
            case KeyItemType.EggplantCandle:
                pd.AddInnEffect(InnEffect.InnType.Ethereal);
                pd.FullHeal();
                break;
            case KeyItemType.PumpkinCandle:
                pd.AddInnEffect(InnEffect.InnType.Immunity);
                pd.FullHeal();
                break;
            case KeyItemType.PineappleCandle:
                pd.AddInnEffect(InnEffect.InnType.BonusTurn);
                pd.FullHeal();
                break;
            case KeyItemType.HoneyCandle:
                pd.AddInnEffect(InnEffect.InnType.ItemBoost);
                pd.FullHeal();
                break;
            case KeyItemType.FlowerCandle:
                pd.AddInnEffect(InnEffect.InnType.Illuminate);
                pd.FullHeal();
                break;
            case KeyItemType.StellarCandle:
                pd.AddInnEffect(InnEffect.InnType.Soul);
                pd.FullHeal();
                break;
            case KeyItemType.RainbowCandle:
                pd.AddInnEffect(InnEffect.InnType.Freebie);
                pd.FullHeal();
                break;
            case KeyItemType.PowerTotemA:
                pd.AddCharmEffect(CharmEffect.CharmType.Attack, 1);
                break;
            case KeyItemType.PowerTotemB:
                pd.AddCharmEffect(CharmEffect.CharmType.Attack, 2);
                break;
            case KeyItemType.PowerTotemC:
                pd.AddCharmEffect(CharmEffect.CharmType.Attack, 3);
                break;
            case KeyItemType.FortuneTotemA:
                pd.AddCharmEffect(CharmEffect.CharmType.Fortune, 1);
                break;
            case KeyItemType.FortuneTotemB:
                pd.AddCharmEffect(CharmEffect.CharmType.Fortune, 2);
                break;
            case KeyItemType.FortuneTotemC:
                pd.AddCharmEffect(CharmEffect.CharmType.Fortune, 3);
                break;
        }
        //To do later: logic for making another menu
        //My idea is that it builds a GenericMenu or some such though that is improper
        //This menu needs to have logic for controlling the sub menu properly
        //(Copy stuff from how TextboxScript works)

        //As long as the new menu gets put lower in the hierarchy than the entire pause menu tree it will display correctly

        bool consumable = KeyItem.IsConsumable(targetItem);
        if (consumable)
        {
            playerData.keyInventory.Remove(targetItem);
        }

        //fix the menu index to make it not look weird
        if (index > keyItemList.Count - 1)
        {
            index = keyItemList.Count - 1;
        }
        if (index < 0)
        {
            index = 0;
        }

        //Reset stuff (but this is a bit wacky)
        section.ApplyUpdate(new Pause_HandlerItem_ConsumableMenu.UpdateObject(index, selectedPlayer));
        section.ApplyUpdate(null);
        SendUpdate();

        //to make you go back
        //PopSelf();
    }


    public override void Init()
    {
        keyItemList = GetKeyItemList();


        base.Init();

        if (section != null)
        {
            Pause_HandlerItem_ConsumableMenu.UpdateObject uo = (Pause_HandlerItem_ConsumableMenu.UpdateObject)(section.GetState());
            int newIndex = uo.index;
            selectedPlayer = uo.player;
            index = newIndex;
            section.OnActive();
        }

        if (index < 0 || index > keyItemList.Count - 1)
        {
            index = 0;
        }

        if (section != null)
        {
            section.ApplyUpdate(new Pause_HandlerItem_ConsumableMenu.UpdateObject(index, selectedPlayer));
        }
    }

    public override MenuResult GetResult()
    {
        return new MenuResult(index);
    }
}
