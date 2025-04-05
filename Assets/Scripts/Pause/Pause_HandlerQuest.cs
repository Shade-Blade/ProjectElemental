using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pause_HandlerQuest : Pause_HandlerShared_SideTabs
{
    public const int MAX_QUEST_ELEMENTS = 3;

    //different thing
    public bool availableMode;
    public bool availableInit;

    public enum QuestSubpage
    {
        Available,
        Taken,
        Complete,
        Achievements
    }

    QuestSubpage questSubpage;

    public static Pause_HandlerQuest BuildMenu(Pause_SectionShared section = null, QuestSubpage qs = QuestSubpage.Taken)
    {
        GameObject newObj = new GameObject("Pause Quest Menu");
        Pause_HandlerQuest newMenu = newObj.AddComponent<Pause_HandlerQuest>();

        newMenu.SetSubsection(section);
        newMenu.questSubpage = qs;
        newMenu.tabindex = (int)(newMenu.questSubpage) - 1;
        if (newMenu.tabindex < 0)
        {
            newMenu.tabindex = 0;
        }
        newMenu.availableMode = qs == QuestSubpage.Available;
        newMenu.Init();


        return newMenu;
    }

    public override int GetMaxTabs()
    {
        return MAX_QUEST_ELEMENTS - 1;
    }

    public override void MenuUpdate()
    {
        if (availableMode)
        {
            //Hacky setup lol
            tabindex = 0;
            Select();
            return;
        }
        base.MenuUpdate();
    }

    public override void Select()
    {
        MainManager.Instance.PlayGlobalSound(MainManager.Sound.Menu_Select);
        //even more submenus (but we are getting close to the end)
        Debug.Log((QuestSubpage)tabindex + " select");
        if (availableMode)
        {
            questSubpage = QuestSubpage.Available;
        }
        else
        {
            questSubpage = (QuestSubpage)(tabindex + 1);
        }
        Pause_SectionShared newSection = null;
        if (section != null)
        {
            newSection = section.GetSubsection(new UpdateObject(tabindex));
        }
        //build the menu

        int count = ((Pause_SectionShared_BoxMenu)newSection).GetMenuEntryCount();
        MenuHandler b = null;

        if (availableMode)
        {
            //Completely different script
            b = Pause_HandlerQuest_AvailableSelect.BuildMenu(newSection, count);
            availableInit = true;
        } else
        {
            b = Pause_HandlerShared_InfoBoxMenu.BuildMenu(newSection, count, questSubpage != QuestSubpage.Achievements);
        }

        b.transform.parent = transform;
        PushState(b);
        b.menuExit += InvokeExit;
    }

    public bool AvailableDone()
    {
        if (submenu == null && availableInit)
        {
            return true;
        }

        if (!availableInit)
        {
            return false;
        }

        return ((Pause_HandlerQuest_AvailableSelect)submenu).done;
    }
    public int AvailableIndex()
    {
        if (submenu == null && availableInit)
        {
            return -1;
        }

        if (!availableInit)
        {
            return -2;
        }

        return ((Pause_HandlerQuest_AvailableSelect)submenu).index;
    }
}
