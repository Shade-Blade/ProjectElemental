using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Note: unlike other menus this doesn't have a "result" usually
//  There will probably be code to exit the pause menu (maybe in different ways?)
//  (certain key items will trigger things that force exit the pause menu)
//Note 2: each menu's script just modifies pieces that are always loaded (?)
//  (if the performance is too bad it should just modify a more hidden and compact data structure that can be read back)

//Note 3: Only one menu handler can be active at a time

//How to do this?
//  Make 2 parallel data structures
//  First one is the menu handler tree
//  Second one is the actual elements (that get moved around by the active menu)

//controls the UI components
public abstract class Pause_SectionShared : MonoBehaviour
{
    public TextDisplayer textbox;   //often shared by multiple sections, make sure the SetText calls don't try to interfere with each other (putting them in ApplyUpdate calls is safe usually)
    public List<Pause_SectionShared> subsections;

    //by convention the menu will call ApplyUpdate(null) when going back (to reset the state)
    public abstract void ApplyUpdate(object state);
    public abstract object GetState();  //Only use when initing the handler (otherwise don't use this, sections and the handler should be loosely connected)

    //if you press A, what section does the next menu refer to
    //(this also calls ApplyUpdate to get this answer)
    public virtual Pause_SectionShared GetSubsection(object state)
    {
        ApplyUpdate(state);
        return null;
    }

    public bool isInit = false;

    //init (create stuff)
    public virtual void Init()
    {
        isInit = true;
    }

    //delete all the data but don't delete the section

    public virtual void Clear()
    {
        isInit = false;
    }
}

//controls controlling (but has a reference to a UI section)
//handles its own state (which gets synced with the ui section)
//(note: handles the entirety of its state, so the relationship with the UI section is purely one way)
//  the reason I want to do it this way is that there are more UI sections than menu handlers
//  so 1 way relationships are probably good here
public abstract class Pause_HandlerShared : MenuHandler
{
    public Pause_SectionShared section;

    public void SetSubsection(Pause_SectionShared subsection)
    {
        section = subsection;
    }
}

public class Pause_SectionBase : Pause_SectionShared
{
    public enum PauseMenuPage
    {
        Status, //stats and stuff (also displays the story progress in the form of the soul cores in a circle thing)
        Items,  //items, key items
        Equip, //all, all equipped, single char equipped, ribbons
        Quests,     //available, taken, completed,  achievements
        Journal,    //recipes, bestiary, lore, info //a place for the hidden battle mechanics and stuff
        Map,        //this could be a key item but it's probably more convenient as a menu option
        Settings
    }
    public const int MAX_PAGES = 7;

    public int baseIndex;

    //public const float PER_PAGE_OFFSET = 1200;

    public float offset;
    public float targetOffset;
    public float verticalOffset;

    public float pastPerPageOffset;

    public MenuHandler menu; //holds the pause menu handler itself
    public MenuResult result;

    public PauseMenuPage[] pages;
    public int[] disabledTabs;

    public static Pause_SectionBase buildMenu()
    {
        //Generate a menu
        GameObject newObj = Instantiate(MainManager.Instance.pauseMenu, MainManager.Instance.Canvas.transform); //new GameObject("Pause Menu");
        //newObj.transform.parent = MainManager.Instance.Canvas.transform;
        //PauseMenuScript newMenu = newObj.AddComponent<PauseMenuScript>();
        Pause_SectionBase newMenu = newObj.GetComponent<Pause_SectionBase>();
        newMenu.verticalOffset = newMenu.GetStartVerticalOffset();
        newMenu.Init();

        return newMenu;
    }

    public override void ApplyUpdate(object state)
    {
        Pause_SectionShared_TabSection[] ts = GetComponentsInChildren<Pause_SectionShared_TabSection>();
        if (state == null)
        {
            foreach (Pause_SectionShared_TabSection tsc in ts)
            {
                tsc.SetCloseText();
            }
            return;
        }

        int index = (int)(state);
        baseIndex = index;
        //Debug.Log((PauseMenuPage)index);
        targetOffset = baseIndex * GetPerPageOffset();

        foreach (Pause_SectionShared_TabSection tsc in ts)
        {
            tsc.SetSelectText();
        }
    }
    public override object GetState()
    {
        return null;
    }

    public override Pause_SectionShared GetSubsection(object state)
    {
        ApplyUpdate(state);
        return subsections[baseIndex];
    }


    // Start is called before the first frame update
    void Start()
    {
        
    }

    void Update()
    {
        //if you resize window while paused
        if (pastPerPageOffset != GetPerPageOffset() && pastPerPageOffset != 0)
        {
            targetOffset = baseIndex * GetPerPageOffset();
            offset *= (GetPerPageOffset() / pastPerPageOffset);
        }

        pastPerPageOffset = GetPerPageOffset();

        if (Mathf.Abs(targetOffset - offset) < 0.1f)
        {
            offset = targetOffset;
        } else
        {
            //30000f = 0.2s per page (go from one page to another)
            //offset = MainManager.EasingQuadraticTime(offset, targetOffset, 30000f); 
            offset = MainManager.EasingQuadraticTime(offset, targetOffset, 25 * GetPerPageOffset());
        }

        if (verticalOffset < 0.1f)
        {
            verticalOffset = 0;
        } else
        {
            verticalOffset = MainManager.EasingQuadraticTime(verticalOffset, 0, 25 * GetStartVerticalOffset());
        }

        for (int i = 0; i < subsections.Count; i++)
        {
            subsections[i].GetComponent<RectTransform>().anchoredPosition = Vector3.right * (-offset + (i * GetPerPageOffset())) + Vector3.up * verticalOffset;

            /*
            if (subsections[i].isInit && Mathf.Abs(-offset/PER_PAGE_OFFSET + i) > 1.5f)
            {
                subsections[i].Clear();
            }

            if (!subsections[i].isInit && Mathf.Abs(-offset / PER_PAGE_OFFSET + i) < 1.5f)
            {
                subsections[i].Init();
            }
            */

            //note: need to make sure that the other menus don't keep stale data
            //bad for performance though (To do: find a better way to fix this problem)
            //(Although conceptually the above isn't actually better since it does a similar number of init and clear calls except on the ends)
            //Other thing is to make the other pages not disappear while still on screen (but if your screen is too wide it happens anyway)
            if (subsections[i].isInit && Mathf.Abs(-offset / GetPerPageOffset() + i) > 0.95f)
            {
                subsections[i].Clear();
            }

            if (!subsections[i].isInit && Mathf.Abs(-offset / GetPerPageOffset() + i) < 0.95f)
            {
                subsections[i].Init();
            }
        }
    }

    public float GetPerPageOffset()
    {
        return 1.5f * MainManager.Instance.Canvas.GetComponent<RectTransform>().rect.width;
    }
    public float GetStartVerticalOffset()
    {
        return 1.5f * MainManager.Instance.Canvas.GetComponent<RectTransform>().rect.height;
    }

    public override void Init()
    {
        base.Init();

        //Not in HandlerJournal because that can be init multiple times which is not great
        //Note: Not in HandlerBase either because I need to make sure SectionJournal and SectionQuest are init after these are run
        //  HandlerJournal and HandlerQuest must be init after because you need to navigate HandlerBase to them first
        GlobalQuestScript.Instance.RebuildQuestStates();
        GlobalQuestScript.Instance.RebuildAchievementStates();


        //Note that this menu is special in that it is the only one that creates its own handler
        //It also only gets init once per pause
        GameObject newObj = new GameObject("Pause Menu Handler");
        Pause_HandlerBase newMenu = newObj.AddComponent<Pause_HandlerBase>();
        newObj.transform.parent = transform;
        newMenu.SetSubsection(this);
        menu = newMenu;
        //Other thing to do: make the submenus recursively

        subsections = new List<Pause_SectionShared>();

        //To do: Dynamically set up which menus should be created or not

        GameObject pauseStatusMenu = (GameObject)Resources.Load("Menu/Pause/Pause_StatusMenu");
        GameObject pauseItemMenu = (GameObject)Resources.Load("Menu/Pause/Pause_ItemMenu");
        GameObject pauseEquipMenu = (GameObject)Resources.Load("Menu/Pause/Pause_EquipMenu");
        GameObject pauseQuestMenu = (GameObject)Resources.Load("Menu/Pause/Pause_QuestMenu");
        GameObject pauseJournalMenu = (GameObject)Resources.Load("Menu/Pause/Pause_JournalMenu");
        GameObject pauseMapMenu = (GameObject)Resources.Load("Menu/Pause/Pause_MapMenu");
        GameObject pauseSettingsMenu = (GameObject)Resources.Load("Menu/Pause/Pause_SettingsMenu");

        Pause_SectionShared_TabSection topTabs = null;

        PauseMenuPage[] pages = Pause_HandlerBase.GetAvailablePages();
        List<int> disabledList = new List<int>();
        for (int i = 0; i < 7; i++) {
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

        GameObject a = null;
        
        for (int i = 0; i < pages.Length; i++)
        {
            switch (pages[i])
            {
                case PauseMenuPage.Status:
                    a = Instantiate(pauseStatusMenu, transform);
                    break;
                case PauseMenuPage.Items:
                    a = Instantiate(pauseItemMenu, transform);
                    break;
                case PauseMenuPage.Equip:
                    a = Instantiate(pauseEquipMenu, transform);
                    break;
                case PauseMenuPage.Quests:
                    a = Instantiate(pauseQuestMenu, transform);
                    break;
                case PauseMenuPage.Journal:
                    a = Instantiate(pauseJournalMenu, transform);
                    break;
                case PauseMenuPage.Map:
                    a = Instantiate(pauseMapMenu, transform);
                    break;
                case PauseMenuPage.Settings:
                    a = Instantiate(pauseSettingsMenu, transform);
                    break;
            }

            topTabs = a.GetComponentInChildren<Pause_SectionShared_TabSection>();
            topTabs.ApplyUpdate(disabledTabs);
            topTabs.SetSelectText();
            subsections.Add(a.GetComponent<Pause_SectionShared>());
        }

        /*
        a = Instantiate(pauseStatusMenu, transform);
        topTabs = a.GetComponentInChildren<Pause_SectionShared_TabSection>();
        topTabs.ApplyUpdate(disabledTabs);
        subsections.Add(a.GetComponent<Pause_SectionShared>());

        a = Instantiate(pauseItemMenu, transform);
        topTabs = a.GetComponentInChildren<Pause_SectionShared_TabSection>();
        topTabs.ApplyUpdate(disabledTabs);
        subsections.Add(a.GetComponent<Pause_SectionShared>());

        a = Instantiate(pauseEquipMenu, transform);
        topTabs = a.GetComponentInChildren<Pause_SectionShared_TabSection>();
        topTabs.ApplyUpdate(disabledTabs);
        subsections.Add(a.GetComponent<Pause_SectionShared>());

        a = Instantiate(pauseQuestMenu, transform);
        topTabs = a.GetComponentInChildren<Pause_SectionShared_TabSection>();
        topTabs.ApplyUpdate(disabledTabs);
        subsections.Add(a.GetComponent<Pause_SectionShared>());

        a = Instantiate(pauseJournalMenu, transform);
        topTabs = a.GetComponentInChildren<Pause_SectionShared_TabSection>();
        topTabs.ApplyUpdate(disabledTabs);
        subsections.Add(a.GetComponent<Pause_SectionShared>());

        a = Instantiate(pauseMapMenu, transform);
        topTabs = a.GetComponentInChildren<Pause_SectionShared_TabSection>();
        topTabs.ApplyUpdate(disabledTabs);
        subsections.Add(a.GetComponent<Pause_SectionShared>());

        a = Instantiate(pauseSettingsMenu, transform);
        topTabs = a.GetComponentInChildren<Pause_SectionShared_TabSection>();
        topTabs.ApplyUpdate(disabledTabs);
        subsections.Add(a.GetComponent<Pause_SectionShared>());
        */

        offset = 0;
        targetOffset = 0;

        for (int i = 0; i < subsections.Count; i++)
        {
            subsections[i].GetComponent<RectTransform>().anchoredPosition = Vector3.right * (-offset + (i * GetPerPageOffset())) + Vector3.up * GetStartVerticalOffset();

            if (subsections[i].isInit && Mathf.Abs(-offset / GetPerPageOffset() + i) > 1.5f)
            {
                subsections[i].Clear();
            }

            if (!subsections[i].isInit && Mathf.Abs(-offset / GetPerPageOffset() + i) < 1.5f)
            {
                subsections[i].Init();
            }
        }

        pastPerPageOffset = GetPerPageOffset();

        menu.Init();
    }

    public void Unpause()
    {
        MainManager.Instance.PlayGlobalSound(MainManager.Sound.Menu_Unpause);

        result = menu.GetFullResult();
        menu.ActiveClear();
        MainManager.Instance.isPaused = false;
        Destroy(gameObject);
    }

    

}
