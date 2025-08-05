using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pause_HandlerShared_BoxMenu : Pause_HandlerShared
{
    public int index = -1;
    protected float inputDir;
    protected float holdDur;
    protected int holdValue;

    public override event EventHandler<MenuExitEventArgs> menuExit;

    public const float HYPER_SCROLL_TIME = 0.3f;

    public const int PAGE_SIZE = 10;

    //private List<Item> itemList;

    /*
    public static Pause_HandlerShared_BoxMenu BuildMenu(Pause_SectionShared section = null)
    {
        GameObject newObj = new GameObject("Pause Box Menu");
        Pause_HandlerShared_BoxMenu newMenu = newObj.AddComponent<Pause_HandlerShared_BoxMenu>();

        newMenu.SetSubsection(section);

        newMenu.Init();


        return newMenu;
    }
    */

    void Update()
    {
        if (active)
        {
            MenuUpdate();
        }
    }

    public virtual int GetObjectCount()
    {
        return 0;
    }

    public virtual void SendUpdate()
    {
        if (section != null)
        {
            section.ApplyUpdate(index);
        }
    }

    public virtual void MenuUpdate()
    {
        lifetime += Time.deltaTime;
        int oc = GetObjectCount();
        if (oc > 0)
        {
            if ((lifetime > MIN_SELECT_TIME && Mathf.Sign(InputManager.GetAxisVertical()) != -inputDir) || InputManager.GetAxisVertical() == 0)
            {
                holdDur = 0;
                holdValue = 0;
                inputDir = -Mathf.Sign(InputManager.GetAxisVertical());
                if (InputManager.GetAxisVertical() == 0)
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
                        MainManager.Instance.PlayGlobalSound(MainManager.Sound.Menu_ScrollDown);
                    }
                    else
                    {
                        index--;
                        MainManager.Instance.PlayGlobalSound(MainManager.Sound.Menu_ScrollUp);
                    }
                }

                if (index > oc - 1)
                {
                    //index = itemList.Count - 1;
                    index = 0;
                }
                if (index < 0)
                {
                    //index = 0;
                    index = oc - 1;
                }

                if (inputDir != 0)
                {
                    //Debug.Log("Apply update? " + (section != null));
                    SendUpdate();
                    //MainManager.ListPrint(itemList, index);
                    //Debug.Log(itemList[index]);
                }
            }

            if (lifetime > MIN_SELECT_TIME && InputManager.GetButtonDown(InputManager.Button.Z))
            {
                if (index == oc - 1)
                {
                    index = 0;
                }
                else
                {
                    index += PAGE_SIZE;
                    MainManager.Instance.PlayGlobalSound(MainManager.Sound.Menu_BigScrollDown);

                    if (index > oc - 1)
                    {
                        //index = badgeList.Count - 1;
                        index = oc - 1;
                    }
                }

                SendUpdate();
            }

            if (lifetime > MIN_SELECT_TIME && InputManager.GetButtonDown(InputManager.Button.Y))
            {
                if (index == 0)
                {
                    index = oc - 1;
                } else
                {
                    index -= PAGE_SIZE;
                    MainManager.Instance.PlayGlobalSound(MainManager.Sound.Menu_BigScrollUp);

                    if (index < 0)
                    {
                        //index = 0;
                        index = 0;
                    }
                }

                SendUpdate();
            }

            if ((lifetime > MIN_SELECT_TIME && Mathf.Sign(InputManager.GetAxisVertical()) == -inputDir) && InputManager.GetAxisVertical() != 0)
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
                        if (index < oc - 1 && (holdValue - pastHoldValue) > 0)
                        {
                            MainManager.Instance.PlayGlobalSound(MainManager.Sound.Menu_ScrollDown);
                        }
                        index += (holdValue - pastHoldValue);
                    }
                    else
                    {
                        if (index > 0 && (holdValue - pastHoldValue) > 0)
                        {
                            MainManager.Instance.PlayGlobalSound(MainManager.Sound.Menu_ScrollUp);
                        }
                        index -= (holdValue - pastHoldValue);
                    }

                    //No loop around
                    if (index > oc - 1)
                    {
                        index = oc - 1;
                    }
                    if (index < 0)
                    {
                        index = 0;
                    }

                    SendUpdate();
                }
            }

            if (lifetime > MIN_SELECT_TIME && InputManager.GetButtonDown(InputManager.Button.A)) //Press A to select stuff
            {
                //go to submenu
                Select();
            }
            if (lifetime > MIN_SELECT_TIME && InputManager.GetButtonDown(InputManager.Button.B)) //Press B to go back
            {
                MainManager.Instance.PlayGlobalSound(MainManager.Sound.Menu_Cancel);
                PopSelf();
            }
        }
        else
        {
            //No items

            if (lifetime > MIN_SELECT_TIME && InputManager.GetButtonDown(InputManager.Button.B)) //Press B to go back
            {
                MainManager.Instance.PlayGlobalSound(MainManager.Sound.Menu_Cancel);
                PopSelf();
            }
        }
    }

    public virtual void Select()
    {

    }
    /*
    public virtual void Select()
    {
        //even more submenus (but we are getting close to the end)
        bool canSelect = Item.CanUseOutOfBattle(itemList[index].type);

        if (canSelect)
        {
            Debug.Log(itemList[index] + " select");
        }
        else
        {
            Debug.Log(itemList[index] + " can't select");
            return;
        }

        //this selection may be more complex
        //but I may be fine with just a selection menu
        //The cases are similar to the battle selection menu (0 entities, 1 entity, multiple entities)
        //even more submenus (but we are getting close to the end)
        Pause_SectionShared newSection = null;
        if (section != null)
        {
            newSection = section.GetSubsection(index);
        }
        //build the menu
        MenuHandler b = null;
        b = Pause_HandlerItem_CharacterSelect.BuildMenu(newSection, MainManager.Instance.playerData, itemList[index], index);

        b.transform.parent = transform;
        PushState(b);
        b.menuExit += InvokeExit;
    }
    */

    /*
    public override void Init()
    {
        itemList = GetItemList();

        if (index < 0 || index > itemList.Count - 1)
        {
            index = 0;
        }


        base.Init();
    }
    */

    public void InvokeExit(object sender, MenuExitEventArgs meea)
    {
        menuExit?.Invoke(this, new MenuExitEventArgs(GetFullResult()));
    }

    public override MenuResult GetResult()
    {
        return new MenuResult(index);
    }
}