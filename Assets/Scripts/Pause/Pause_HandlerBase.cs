using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static Pause_SectionBase;

public class Pause_HandlerBase : Pause_HandlerShared
{
    public PauseMenuPage page;
    public MenuResult result;

    public int baseIndex;
    float inputDir;

    public PauseMenuPage[] pages;
    public int[] disabledTabs;

    public override event EventHandler<MenuExitEventArgs> menuExit;

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
        if ((lifetime > MIN_SELECT_TIME && Mathf.Sign(InputManager.GetAxisHorizontal()) != inputDir) || InputManager.GetAxisHorizontal() == 0)
        {
            inputDir = Mathf.Sign(InputManager.GetAxisHorizontal());
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
                    baseIndex++;
                }
                else
                {
                    baseIndex--;
                }
            }

            if (baseIndex > pages.Length - 1)// MAX_PAGES - 1)
            {
                baseIndex = pages.Length - 1; //MAX_PAGES - 1;
            }
            if (baseIndex < 0)
            {
                baseIndex = 0;
            }

            if (inputDir != 0)
            {
                //Debug.Log((PauseMenuPage)baseIndex);
                page = pages[baseIndex]; //(PauseMenuPage)baseIndex;
                section.ApplyUpdate(baseIndex);
            }
        }

        if (lifetime > MIN_SELECT_TIME && InputManager.GetButtonDown(InputManager.Button.A)) //Press A to select stuff
        {
            //go to submenu
            Select();
        }
        
        if (lifetime > MIN_SELECT_TIME && InputManager.GetButtonDown(InputManager.Button.B)) //Press B to go back
        {
            Pause_SectionBase pms = (Pause_SectionBase)section;
            pms.Unpause();
        }
    }

    void Select()
    {
        Pause_SectionShared newSection = section.GetSubsection(baseIndex);
        section.ApplyUpdate(null);
        section.OnInactive();
        //build the menu
        MenuHandler b = null;
        switch (page)
        {
            case PauseMenuPage.Status:
                b = Pause_HandlerStatus.BuildMenu(newSection);
                break;
            case PauseMenuPage.Items:
                b = Pause_HandlerItem.BuildMenu(newSection);
                break;
            case PauseMenuPage.Equip:
                b = Pause_HandlerEquip.BuildMenu(newSection);
                break;            
            case PauseMenuPage.Quests:
                b = Pause_HandlerQuest.BuildMenu(newSection);
                break;
            case PauseMenuPage.Journal:
                b = Pause_HandlerJournal.BuildMenu(newSection);
                break;
            case PauseMenuPage.Map:
                b = Pause_HandlerMap.BuildMenu(newSection);
                break;
            case PauseMenuPage.Settings:
                b = Pause_HandlerSettings.BuildMenu(newSection);
                break;
        }

        MainManager.Instance.PlayGlobalSound(MainManager.Sound.Menu_Open);

        b.transform.parent = transform;
        PushState(b);
        b.menuExit += InvokeExit;
    }

    public override void Init()
    {
        if (baseIndex < 0)
        {
            baseIndex = 0;
        }
        pages = GetAvailablePages();
        List<int> disabledList = new List<int>();
        for (int i = 0; i < 7; i++)
        {
            disabledList.Add(i);
        }
        for (int i = 0; i < pages.Length; i++)
        {
            disabledList.Remove((int)pages[i]);
        }
        int[] disabledTabs = new int[disabledList.Count];
        for (int i = 0; i < disabledList.Count; i++)
        {
            disabledTabs[i] = disabledList[i];
        }
        section.ApplyUpdate(baseIndex);
        section.OnActive();
        base.Init();
    }

    //stuff that asks MainManager
    //To do: Make the main manager method (Should poll global flags, playerdata stuff to check which things should appear at that point of the game)
    public static PauseMenuPage[] GetAvailablePages()
    {
        List<PauseMenuPage> pages = new List<PauseMenuPage>();
        pages.Add(PauseMenuPage.Status);

        if (MainManager.Instance.playerData.itemCounter > 0 || MainManager.Instance.playerData.itemInventory.Count > 0)
        {
            pages.Add(PauseMenuPage.Items);
        }
        if (MainManager.Instance.playerData.badgeInventory.Count > 3)
        {
            pages.Add(PauseMenuPage.Equip);
        }

        string floorNo = MainManager.Instance.GetGlobalVar(MainManager.GlobalVar.GV_PitFloor);
        if (floorNo != null)
        {
            pages.Add(PauseMenuPage.Quests);
        }
        pages.Add(PauseMenuPage.Journal);
        if (floorNo != null)
        {
            pages.Add(PauseMenuPage.Map);
        }
        pages.Add(PauseMenuPage.Settings);

        PauseMenuPage[] output = new PauseMenuPage[] { PauseMenuPage.Status, PauseMenuPage.Items, PauseMenuPage.Equip, PauseMenuPage.Quests, PauseMenuPage.Journal, PauseMenuPage.Map, PauseMenuPage.Settings };
        //PauseMenuPage[] output = new PauseMenuPage[] { PauseMenuPage.Status, PauseMenuPage.Items, PauseMenuPage.Equip, PauseMenuPage.Settings };
        return pages.ToArray();
    }

    public void InvokeExit(object sender, MenuExitEventArgs meea)
    {
        menuExit?.Invoke(this, new MenuExitEventArgs(GetFullResult()));
    }

    public override MenuResult GetResult()
    {
        return new MenuResult(page);
    }
}
