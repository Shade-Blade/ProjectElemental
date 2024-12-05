using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pause_SectionQuest_List : Pause_SectionShared_BoxMenu
{
    public Pause_SectionShared_SideText sidetext;
    public Pause_SectionShared_InfoBox infobox;
    public TextDisplayer noItemText;

    public Pause_SectionShared_TextBox baseText;

    public Pause_HandlerQuest.QuestSubpage subpage;


    public override void Init()
    {
        //menuIndex = 0;
        //menuTopIndex = 0;
        if (menuIndex == -1)
        {
            menuIndex = 0;
            menuTopIndex = 0;
        }
        visualTopIndex = menuTopIndex;
        int desiredLoadedTopIndex = Mathf.FloorToInt(visualTopIndex) - MENU_BUFFER;   //higher up
        if (desiredLoadedTopIndex < 0)
        {
            desiredLoadedTopIndex = 0;
        }
        loadedTopIndex = desiredLoadedTopIndex;

        string emptyString = "";

        switch (subpage)
        {
            case Pause_HandlerQuest.QuestSubpage.Available:
                emptyString = "No Available Quests";
                menuEntries = new BoxMenuEntry[GlobalQuestScript.Instance.availableQuests.Count];
                for (int i = 0; i < menuEntries.Length; i++)
                {
                    QuestText qt = GlobalQuestScript.Instance.GetQuestText(GlobalQuestScript.Instance.availableQuests[i]);
                    QuestFlags qf = GlobalQuestScript.Instance.GetQuestFlags(GlobalQuestScript.Instance.availableQuests[i]);

                    string tempString = "";
                    List<string> stringList = qt.FilterTextList(qf.CheckProgressFlags());
                    for (int j = 0; j < stringList.Count; j++)
                    {
                        if (j > 0)
                        {
                            tempString += "<next>";
                        }
                        tempString += stringList[j];
                    }
                    menuEntries[i] = new InformationMenuEntry(null, null, qt.name, qt.GetSideText(), tempString, qt.description);
                }
                break;
            case Pause_HandlerQuest.QuestSubpage.Taken:
                emptyString = "No Taken Quests";
                menuEntries = new BoxMenuEntry[GlobalQuestScript.Instance.startedQuests.Count];
                for (int i = 0; i < menuEntries.Length; i++)
                {
                    QuestText qt = GlobalQuestScript.Instance.GetQuestText(GlobalQuestScript.Instance.startedQuests[i]);
                    QuestFlags qf = GlobalQuestScript.Instance.GetQuestFlags(GlobalQuestScript.Instance.startedQuests[i]);

                    string tempString = "";
                    List<string> stringList = qt.FilterTextList(qf.CheckProgressFlags());
                    for (int j = 0; j < stringList.Count; j++)
                    {
                        if (j > 0)
                        {
                            tempString += "<next>";
                        }
                        tempString += stringList[j];
                    }
                    menuEntries[i] = new InformationMenuEntry(null, null, qt.name, qt.GetSideText(), tempString, qt.description);
                }
                break;
            case Pause_HandlerQuest.QuestSubpage.Complete:
                emptyString = "No Complete Quests";
                menuEntries = new BoxMenuEntry[GlobalQuestScript.Instance.completeQuests.Count];
                for (int i = 0; i < menuEntries.Length; i++)
                {
                    QuestText qt = GlobalQuestScript.Instance.GetQuestText(GlobalQuestScript.Instance.completeQuests[i]);
                    QuestFlags qf = GlobalQuestScript.Instance.GetQuestFlags(GlobalQuestScript.Instance.completeQuests[i]);

                    string tempString = "";
                    List<string> stringList = qt.FilterTextList(qf.CheckProgressFlags());
                    for (int j = 0; j < stringList.Count; j++)
                    {
                        if (j > 0)
                        {
                            tempString += "<next>";
                        }
                        tempString += stringList[j];
                    }
                    menuEntries[i] = new InformationMenuEntry(null, null, qt.name, qt.GetSideText(), tempString, qt.description);
                }
                break;
            case Pause_HandlerQuest.QuestSubpage.Achievements:
                emptyString = "Error: achievement menu should not be empty";
                if (GlobalQuestScript.Instance.achievementText == null)
                {
                    GlobalQuestScript.Instance.LoadAchievementText();
                }
                string[][] at = GlobalQuestScript.Instance.achievementText;
                bool[] bl = GlobalQuestScript.Instance.achievementStates;
                menuEntries = new BoxMenuEntry[at.Length];
                for (int i = 0; i < at.Length; i++)
                {
                    if (bl[i])
                    {
                        menuEntries[i] = new InformationMenuEntry(null, null, (i + 1) + ". " + at[i][1], at[i][2], null);
                    }
                    else
                    {
                        menuEntries[i] = new InformationMenuEntry(null, null, (i + 1) + ". ???", at[i][2], null);
                    }
                }
                break;
        }

        if (menuEntries == null)
        {
            menuEntries = new BoxMenuEntry[0];
        }

        if (menuIndex > menuEntries.Length - 1)
        {
            menuIndex = menuEntries.Length - 1;
        }
        if (menuTopIndex > menuEntries.Length - MENU_SIZE_PER_PAGE)
        {
            menuTopIndex = menuEntries.Length - MENU_SIZE_PER_PAGE;
        }
        if (menuTopIndex < 0)
        {
            menuTopIndex = 0;
        }
        visualTopIndex = menuTopIndex;
        desiredLoadedTopIndex = Mathf.FloorToInt(visualTopIndex) - MENU_BUFFER;   //higher up
        if (desiredLoadedTopIndex < 0)
        {
            desiredLoadedTopIndex = 0;
        }
        loadedTopIndex = desiredLoadedTopIndex;

        menuEntriesS = new List<BoxMenuEntryScript>();

        //Debug.Log(menuEntries.Length + " menu entries");

        for (int i = 0; i < MENU_SIZE_PER_PAGE + MENU_BUFFER * 2; i++)
        {
            if (i + loadedTopIndex > menuEntries.Length - 1)
            {
                break;
            }

            GameObject g = Instantiate(pauseMenuEntry, mask.transform);
            g.transform.localPosition = GetRelativePosition(i + loadedTopIndex - visualTopIndex);

            BoxMenuEntryScript b = g.GetComponent<BoxMenuEntryScript>();
            menuEntriesS.Add(b);

            //note: they all have canuse = true, because some of the menus just don't do anything when you select them (instead of actually using the canuse value as it is normally used for)
            b.Setup(menuEntries[i + loadedTopIndex]);
        }

        upArrow.enabled = false; //menuTopIndex > 0;
        downArrow.enabled = menuTopIndex < menuEntries.Length - MENU_SIZE_PER_PAGE && menuEntries.Length > MENU_SIZE_PER_PAGE;

        visualTopIndex = MainManager.EasingQuadraticTime(visualTopIndex, menuTopIndex, 25);
        for (int i = 0; i < menuEntriesS.Count; i++)
        {
            //Debug.Log(i - menuTopIndex);
            //menuEntriesO[i].transform.localPosition = BoxMenuScript.GetRelativePosition(i - menuTopIndex);
            menuEntriesS[i].transform.localPosition = GetRelativePosition(i + loadedTopIndex - visualTopIndex);
        }

        //No "no journal entries" text because that shouldn't happen

        if (menuEntriesS.Count == 0)
        {
            selectorArrow.enabled = false;
        }
        else
        {
            //selectorArrow.enabled = true;
            Vector3 targetLocal = Vector3.left * 160f + Vector3.up * 20 + GetRelativePosition(visualSelectIndex - visualTopIndex) + Vector3.up * ARROW_OFFSET;
            Vector3 current = selectorArrow.transform.localPosition;
            selectorArrow.transform.localPosition = targetLocal;
        }

        if (menuEntries.Length > 0)
        {
            noItemText.gameObject.SetActive(false);
        }
        else
        {
            //create some text
            noItemText.gameObject.SetActive(true);
            noItemText.SetText(emptyString, true, true);
            //noItemText.SetText(???, true, true);
        }
    }

    public override void Clear()
    {
        if (menuEntriesS != null)
        {
            for (int i = 0; i < menuEntriesS.Count; i++)
            {
                Destroy(menuEntriesS[i].gameObject);
            }
        }
        menuEntriesS = null;
        isInit = false;
    }


    public override void ApplyUpdate(object state)
    {
        if (state == null)  //resets the inventory
        {
            Clear();
            Init();
            selectorArrow.color = new Color(0.5f, 0.5f, 0.5f, 1);
            return;
        }

        selectorArrow.gameObject.SetActive(true);
        selectorArrow.enabled = true;
        selectorArrow.color = new Color(1, 1, 1, 1);

        int index = (int)state;
        menuIndex = index;

        if (menuEntries.Length == 0)
        {
            selectorArrow.gameObject.SetActive(false);
            selectorArrow.enabled = false;
        }

        if (menuIndex < 0)
        {
            menuIndex = 0;
        }
        if (menuIndex > menuEntries.Length - 1)
        {
            menuIndex = menuEntries.Length - 1;
        }

        if (menuTopIndex > menuIndex)
        {
            menuTopIndex = menuIndex;
        }
        if (menuTopIndex < menuIndex - MENU_SIZE_PER_PAGE + 1)
        {
            menuTopIndex = menuIndex - MENU_SIZE_PER_PAGE + 1;
        }

        //BoxMenuScript bm = menuBaseO.GetComponent<BoxMenuScript>();

        //put this in init too
        upArrow.enabled = menuTopIndex > 0;
        downArrow.enabled = menuTopIndex < menuEntries.Length - MENU_SIZE_PER_PAGE && menuEntries.Length > MENU_SIZE_PER_PAGE;

        if (menuEntries.Length == 0)
        {
            sidetext.ApplyUpdate(null);
        } else
        {
            sidetext.ApplyUpdate((InformationMenuEntry)(menuEntries[menuIndex]));
        }

        if (textbox != null)
        {
            if (menuEntries.Length == 0 || menuEntries[menuIndex].description == null || menuEntries[menuIndex].description.Length < 2)
            {
                if (baseText.gameObject.activeSelf)
                {
                    textbox.SetText("", true, true);
                }
                baseText.gameObject.SetActive(false);
            }
            else
            {
                baseText.gameObject.SetActive(true);
                textbox.SetText(menuEntries[menuIndex].description, true, true);
            }
        }

        return;
    }

    public override Pause_SectionShared GetSubsection(object state)
    {
        //Debug.Log("Asked for subsection");
        return infobox;
    }
}
