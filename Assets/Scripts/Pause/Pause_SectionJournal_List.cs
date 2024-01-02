using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pause_SectionJournal_List : Pause_SectionShared_BoxMenu
{
    public Pause_SectionShared_SideText sidetext;
    public Pause_SectionShared_InfoBox infobox;

    public Pause_SectionShared_TextBox baseText;

    public Pause_HandlerJournal.JournalSubpage subpage;


    public override void Init()
    {
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
                for (int i = 0; i < menuEntries.Length; i++)
                {
                    Item.ItemType it = GlobalItemScript.Instance.recipeOrder[i];
                    //Debug.Log(it);
                    menuEntries[i] = new InformationMenuEntry(GlobalItemScript.GetItemSprite(it), null, (i + 1) + ". " + Item.GetName(it), GlobalItemScript.Instance.GetRecipeText(it), null, GlobalItemScript.Instance.GetItemDescription(it));
                    menuEntries[i].canUse = true;
                }
                break;
            case Pause_HandlerJournal.JournalSubpage.Bestiary:
                menuEntries = new BoxMenuEntry[MainManager.Instance.bestiaryOrder.Length];
                for (int i = 0; i < menuEntries.Length; i++)
                {
                    BattleHelper.EntityID eid = MainManager.Instance.bestiaryOrder[i].eid;
                    string bestiaryOrderNumber = MainManager.Instance.bestiaryOrder[i].index.ToString();
                    if (MainManager.Instance.bestiaryOrder[i].subindex != 0)
                    {
                        bestiaryOrderNumber += (char)('a' + MainManager.Instance.bestiaryOrder[i].subindex);
                    }

                    menuEntries[i] = new InformationMenuEntry(BattleEntity.GetBestiarySprite(eid), null, bestiaryOrderNumber + ". " + BattleEntity.GetNameStatic(eid), BattleEntity.GetBestiarySideText(eid), BattleEntity.GetBestiaryEntry(eid));
                    menuEntries[i].canUse = true;
                }
                break;
            case Pause_HandlerJournal.JournalSubpage.Lore:
                menuEntries = new BoxMenuEntry[loreText.Length - 2];
                for (int i = 1; i < menuEntries.Length + 1; i++)
                {
                    menuEntries[i - 1] = new InformationMenuEntry(null, null, (i) + ". " + loreText[i][1], loreText[i][2], loreText[i][3]);
                }
                break;
            case Pause_HandlerJournal.JournalSubpage.Information:
                menuEntries = new BoxMenuEntry[infoText.Length - 2];
                for (int i = 1; i < menuEntries.Length + 1; i++)
                {
                    menuEntries[i - 1] = new InformationMenuEntry(null, null, (i) + ". " + infoText[i][1], infoText[i][2], infoText[i][3]);
                }
                break;
        }
        
        if (menuEntries == null)
        {
            menuEntries = new BoxMenuEntry[0];
        }

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
            selectorArrow.enabled = true;
            Vector3 targetLocal = Vector3.left * 160f + Vector3.up * 20 + GetRelativePosition(visualSelectIndex - visualTopIndex) + Vector3.up * ARROW_OFFSET;
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
            return;
        }

        selectorArrow.gameObject.SetActive(true);
        selectorArrow.color = new Color(1, 1, 1, 1);

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
        downArrow.enabled = menuTopIndex < menuEntries.Length - MENU_SIZE_PER_PAGE && menuEntries.Length > MENU_SIZE_PER_PAGE;

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
