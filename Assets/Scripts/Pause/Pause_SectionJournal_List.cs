using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pause_SectionJournal_List : Pause_SectionShared_BoxMenu
{
    public Pause_SectionShared_SideText sidetext;
    public Pause_SectionShared_InfoBox infobox;

    public Pause_SectionShared_TextBox baseText;

    public Pause_HandlerJournal.JournalSubpage subpage;

    //to reuse code I'll just have this set by init instead of completely recalculating it
    public int currentCount;
    public int totalCount;

    public string GetRemainingTextString()
    {
        string output = "";
        //add a sprite string of some kind
        switch (subpage)
        {
            case Pause_HandlerJournal.JournalSubpage.Recipe:
                output = "<common,recipe>";
                break;
            case Pause_HandlerJournal.JournalSubpage.Bestiary:
                output = "<common,enemy>";
                break;
            case Pause_HandlerJournal.JournalSubpage.Lore:
                output = "<common,lore>";
                break;
            case Pause_HandlerJournal.JournalSubpage.Information:
                output = "<common,info>";
                break;
        }

        if (output.Length > 0)
        {
            output += " ";
        }
        output += currentCount + "/" + totalCount;
        return output;
    }

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

        if (GlobalInformationScript.Instance.infoText == null)
        {
            GlobalInformationScript.Instance.LoadInfoText();
        }
        if (GlobalInformationScript.Instance.loreText == null)
        {
            GlobalInformationScript.Instance.LoadLoreText();
        }
        string[][] infoText = GlobalInformationScript.Instance.infoText;
        string[][] loreText = GlobalInformationScript.Instance.loreText;

        switch (subpage)
        {
            case Pause_HandlerJournal.JournalSubpage.Recipe:
                if (GlobalItemScript.Instance.recipeOrder == null)
                {
                    GlobalItemScript.Instance.LoadRecipeDataTable();
                }
                menuEntries = new BoxMenuEntry[GlobalItemScript.Instance.recipeOrder.Length];
                totalCount = menuEntries.Length;
                currentCount = 0;
                for (int i = 0; i < menuEntries.Length; i++)
                {
                    Item.ItemType it = GlobalItemScript.Instance.recipeOrder[i];
                    if (MainManager.Instance.GetRecipeFlag(it))
                    {
                        currentCount++;
                        //Debug.Log(it);
                        menuEntries[i] = new InformationMenuEntry(GlobalItemScript.GetItemSprite(it), null, (i + 1) + ". " + Item.GetName(it), GlobalItemScript.Instance.GetRecipeText(it), null, GlobalItemScript.Instance.GetItemDescription(it));
                        menuEntries[i].canUse = true;
                    }
                    else
                    {
                        //Debug.Log(it);
                        menuEntries[i] = new InformationMenuEntry(null, null, (i + 1) + ". ???", null, null, null);
                        menuEntries[i].canUse = true;
                    }
                }
                break;
            case Pause_HandlerJournal.JournalSubpage.Bestiary:
                menuEntries = new BoxMenuEntry[MainManager.Instance.bestiaryOrder.Length];
                totalCount = menuEntries.Length;
                currentCount = 0;
                for (int i = 0; i < menuEntries.Length; i++)
                {
                    BattleHelper.EntityID eid = MainManager.Instance.bestiaryOrder[i].eid;

                    if (BattleEntity.GetBestiaryFlag(eid))
                    {
                        currentCount++;
                        string bestiaryOrderNumber = MainManager.Instance.bestiaryOrder[i].index.ToString();
                        if (MainManager.Instance.bestiaryOrder[i].subindex != 0)
                        {
                            bestiaryOrderNumber += (char)('a' + MainManager.Instance.bestiaryOrder[i].subindex);
                        }

                        menuEntries[i] = new InformationMenuEntry(BattleEntity.GetBestiarySprite(eid), null, bestiaryOrderNumber + ". " + BattleEntity.GetNameStatic(eid), BattleEntity.GetBestiarySideText(eid), BattleEntity.GetBestiaryEntry(eid));
                        menuEntries[i].canUse = true;
                    }
                    else
                    {
                        //Empty
                        string bestiaryOrderNumber = MainManager.Instance.bestiaryOrder[i].index.ToString();
                        if (MainManager.Instance.bestiaryOrder[i].subindex != 0)
                        {
                            bestiaryOrderNumber += (char)('a' + MainManager.Instance.bestiaryOrder[i].subindex);
                        }

                        menuEntries[i] = new InformationMenuEntry(null, null, bestiaryOrderNumber + ". ???", null, null, null);
                        menuEntries[i].canUse = true;   //note: null infotext is not usable
                    }
                }
                break;
            case Pause_HandlerJournal.JournalSubpage.Lore:
                totalCount = menuEntries.Length;
                currentCount = 0;
                //Todo: flag check
                menuEntries = new BoxMenuEntry[loreText.Length - 2];
                for (int i = 1; i < menuEntries.Length + 1; i++)
                {
                    currentCount++;
                    menuEntries[i - 1] = new InformationMenuEntry(null, null, (i) + ". " + loreText[i][1], loreText[i][2], loreText[i][3]);
                }
                break;
            case Pause_HandlerJournal.JournalSubpage.Information:
                totalCount = menuEntries.Length;
                currentCount = 0;
                //Todo: flag check
                menuEntries = new BoxMenuEntry[infoText.Length - 2];
                for (int i = 1; i < menuEntries.Length + 1; i++)
                {
                    currentCount++;
                    menuEntries[i - 1] = new InformationMenuEntry(null, null, (i) + ". " + infoText[i][1], infoText[i][2], infoText[i][3]);
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

        upArrow.enabled = menuTopIndex > 0;
        upArrowControlHint.enabled = upArrow.enabled;
        upArrowControlHint.SetText(selectorArrow.color.grayscale < 0.75f ? "" : "<button,y>", true, true);
        downArrow.enabled = menuTopIndex < menuEntries.Length - MENU_SIZE_PER_PAGE && menuEntries.Length > MENU_SIZE_PER_PAGE;
        downArrowControlHint.enabled = downArrow.enabled;
        downArrowControlHint.SetText(selectorArrow.color.grayscale < 0.75f ? "" : "<button,z>", true, true);

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
            Vector3 targetLocal = Vector3.left * 170f + Vector3.up * 20 + GetRelativePosition(visualSelectIndex - visualTopIndex) + Vector3.up * ARROW_OFFSET;
            Vector3 current = selectorArrow.transform.localPosition;
            selectorArrow.transform.localPosition = targetLocal;
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
            upArrowControlHint.SetText("", true, true);
            upArrowControlHint.enabled = false;
            downArrowControlHint.SetText("", true, true);
            downArrowControlHint.enabled = false;
            return;
        }

        selectorArrow.gameObject.SetActive(true);
        selectorArrow.enabled = true;
        selectorArrow.color = new Color(1, 1, 1, 1);
        upArrowControlHint.enabled = true;
        downArrowControlHint.enabled = true;

        int index = (int)state;
        menuIndex = index;

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
        upArrowControlHint.enabled = upArrow.enabled;
        upArrowControlHint.SetText(selectorArrow.color.grayscale < 0.75f ? "" : "<button,y>", true, true);
        downArrow.enabled = menuTopIndex < menuEntries.Length - MENU_SIZE_PER_PAGE && menuEntries.Length > MENU_SIZE_PER_PAGE;
        downArrowControlHint.enabled = downArrow.enabled;
        downArrowControlHint.SetText(selectorArrow.color.grayscale < 0.75f ? "" : "<button,z>", true, true);

        sidetext.ApplyUpdate((InformationMenuEntry)(menuEntries[menuIndex]));

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
                //Debug.Log("Set desc to " + menuEntries[menuIndex].description + " (length " + menuEntries[menuIndex].description.Length + ")");
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
