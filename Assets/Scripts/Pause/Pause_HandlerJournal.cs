using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Pause_HandlerQuest;

public class Pause_HandlerJournal : Pause_HandlerShared_SideTabs
{
    public const int MAX_JOURNAL_ELEMENTS = 4;

    public enum JournalSubpage
    {
        Recipe,
        Bestiary,
        Lore,
        Information
    }

    JournalSubpage journalSubpage;

    public static Pause_HandlerJournal BuildMenu(Pause_SectionShared section = null, JournalSubpage journalSubpage = JournalSubpage.Recipe)
    {
        GameObject newObj = new GameObject("Pause Journal Menu");
        Pause_HandlerJournal newMenu = newObj.AddComponent<Pause_HandlerJournal>();

        newMenu.SetSubsection(section);
        newMenu.journalSubpage = journalSubpage;
        newMenu.tabindex = (int)(newMenu.journalSubpage);
        newMenu.Init();


        return newMenu;
    }

    public override int GetMaxTabs()
    {
        return MAX_JOURNAL_ELEMENTS - 1;
    }

    public override void Select()
    {
        MainManager.Instance.PlayGlobalSound(MainManager.Sound.Menu_Select);
        //even more submenus (but we are getting close to the end)
        Debug.Log((JournalSubpage)tabindex + " select");
        journalSubpage = (JournalSubpage)tabindex;
        Pause_SectionShared newSection = null;
        if (section != null)
        {
            newSection = section.GetSubsection(new UpdateObject(tabindex));
        }
        //build the menu

        int count = ((Pause_SectionShared_BoxMenu)newSection).GetMenuEntryCount();
        MenuHandler b = null;
        b = Pause_HandlerShared_InfoBoxMenu.BuildMenu(newSection, count, journalSubpage != JournalSubpage.Recipe);

        b.transform.parent = transform;
        PushState(b);
        b.menuExit += InvokeExit;
    }
}
