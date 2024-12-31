using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pause_SectionItem_Inventory : Pause_SectionShared_BoxMenu
{
    public GameObject subobject;
    public TextDisplayer noItemText;
    public Pause_SectionShared_CharacterStats characterStats;

    public Pause_SectionItem parent;

    public BattleHelper.EntityID selectedPlayer;

    public Pause_HandlerItem.PauseItemPage pip;
    public string emptyString;

    public override void Init()
    {
        if (menuIndex == -1)
        {
            menuIndex = 0;
            menuTopIndex = 0;
        }

        //moved below
        /*
        visualTopIndex = menuTopIndex;
        int desiredLoadedTopIndex = Mathf.FloorToInt(visualTopIndex) - MENU_BUFFER;   //higher up
        if (desiredLoadedTopIndex < 0)
        {
            desiredLoadedTopIndex = 0;
        }
        loadedTopIndex = desiredLoadedTopIndex;
        */

        /*
        menuEntries = new BoxMenuEntry[caller.jumpMoves.Count];
        for (int i = 0; i < caller.jumpMoves.Count; i++)
        {
            menuEntries[i] = new MoveMenuEntry(caller, caller.jumpMoves[i]);
            BoxMenuEntryScript b = menuEntriesO[i].GetComponent<BoxMenuEntryScript>();
            b.Setup(menuEntries[i]);
        }
        */

        //get items from item inventory

        //TO DO: make the emptystring get stuff from the menu text

        switch (pip)
        {
            case Pause_HandlerItem.PauseItemPage.Items:
                List<Item> inv = MainManager.Instance.playerData.itemInventory;
                menuEntries = new BoxMenuEntry[inv.Count];
                for (int i = 0; i < inv.Count; i++)
                {
                    menuEntries[i] = new ItemMenuEntry(inv[i]);
                    menuEntries[i].canUse = Item.GetProperty(inv[i].type, Item.ItemProperty.NoOverworld) == null;
                }
                emptyString = "No Items";
                break;
            case Pause_HandlerItem.PauseItemPage.KeyItems:
                List<KeyItem> kinv = MainManager.Instance.playerData.keyInventory;
                menuEntries = new BoxMenuEntry[kinv.Count];
                for (int i = 0; i < kinv.Count; i++)
                {
                    menuEntries[i] = new KeyItemMenuEntry(kinv[i]);
                    menuEntries[i].canUse = KeyItem.CanUse(kinv[i]);
                }
                emptyString = "No Key Items";
                break;
        }
        if (menuEntries == null)
        {
            menuEntries = new BoxMenuEntry[0];
        }

        //fix menu index?
        if (menuTopIndex < menuIndex - MENU_SIZE_PER_PAGE + 1)
        {
            menuTopIndex = menuIndex - MENU_SIZE_PER_PAGE + 1;
        }

        if (menuTopIndex > menuEntries.Length - MENU_SIZE_PER_PAGE)
        {
            menuTopIndex = menuEntries.Length - MENU_SIZE_PER_PAGE;
            if (menuTopIndex < 0)
            {
                menuTopIndex = 0;
            }
        }
        visualTopIndex = menuTopIndex;
        int desiredLoadedTopIndex = Mathf.FloorToInt(visualTopIndex) - MENU_BUFFER;   //higher up
        if (desiredLoadedTopIndex < 0)
        {
            desiredLoadedTopIndex = 0;
        }
        loadedTopIndex = desiredLoadedTopIndex;

        visualSelectIndex = menuIndex;


        menuEntriesS = new List<BoxMenuEntryScript>();

        //Debug.Log(menuEntries.Length + " menu entries");

        for (int i = 0; i < MENU_SIZE_PER_PAGE + MENU_BUFFER * 2; i++)
        {
            //commenting this will make it a bit more wasteful
            //probably won't be that bad
            if (i + loadedTopIndex > menuEntries.Length - 1)
            {
                break;
            }

            GameObject g = Instantiate(pauseMenuEntry, mask.transform);
            g.transform.localPosition = GetRelativePosition(i + loadedTopIndex - visualTopIndex);

            BoxMenuEntryScript b = g.GetComponent<BoxMenuEntryScript>();
            menuEntriesS.Add(b);

            if (i + loadedTopIndex > menuEntries.Length - 1)
            {
                b.Setup(null, false);
            }
            else
            {
                if (pip == Pause_HandlerItem.PauseItemPage.KeyItems)
                {
                    b.Setup(menuEntries[i + loadedTopIndex], false, null, "#0000ff", "#000000");
                }
                else
                {
                    b.Setup(menuEntries[i + loadedTopIndex]);
                }
            }
        }

        upArrow.enabled = false; //menuTopIndex > 0;
        downArrow.enabled = menuTopIndex < menuEntries.Length - MENU_SIZE_PER_PAGE && menuEntries.Length > MENU_SIZE_PER_PAGE;

        //visualTopIndex = MainManager.EasingQuadraticTime(visualTopIndex, menuTopIndex, 25);
        for (int i = 0; i < menuEntriesS.Count; i++)
        {
            //Debug.Log(i - menuTopIndex);
            //menuEntriesO[i].transform.localPosition = BoxMenuScript.GetRelativePosition(i - menuTopIndex);
            menuEntriesS[i].transform.localPosition = GetRelativePosition(i + loadedTopIndex - visualTopIndex);
        }

        if (menuEntries.Length > 0)
        {
            noItemText.gameObject.SetActive(false);
        } else
        {
            //create some text
            noItemText.gameObject.SetActive(true);
            noItemText.SetText(emptyString, true, true);
            //noItemText.SetText(???, true, true);
        }

        if (menuEntriesS.Count == 0)
        {
            selectorArrow.enabled = false;
        } else
        {
            //selectorArrow.enabled = true;
            Vector3 targetLocal = Vector3.left * 170f + Vector3.up * 20 + GetRelativePosition(visualSelectIndex - visualTopIndex) + Vector3.up * ARROW_OFFSET;
            Vector3 current = selectorArrow.transform.localPosition;
            selectorArrow.transform.localPosition = targetLocal;
        }

        //bm.selectorArrow.transform.localPosition = MainManager.EasingQuadraticTime(current, targetLocal, 450);

        //only do this when you get to the menu
        //textbox.SetText(menuEntries[menuIndex].description, true, true);

        PlayerData pd = MainManager.Instance.playerData;
        selectedPlayer = (BattleHelper.EntityID)(characterStats.GetState());
        if (selectedPlayer == 0)
        {
            selectedPlayer = pd.GetSortedParty()[0].entityID;
            characterStats.ApplyUpdate(pd.GetSortedParty()[0].entityID);
        }
        //characterStats.ApplyUpdate(selectedPlayer);
    }


    public override void ApplyUpdate(object state)
    {
        if (state == null)  //resets the inventory
        {
            Clear();
            Init();
            return;
        }

        if (menuEntries.Length > 0)
        {
            selectorArrow.enabled = true;
        }
        Pause_HandlerItem_ConsumableMenu.UpdateObject uo = (Pause_HandlerItem_ConsumableMenu.UpdateObject)state;

        int index = uo.index;

        BattleHelper.EntityID entityID = uo.player;
        selectedPlayer = entityID;

        menuIndex = index;

        if (menuTopIndex > menuIndex)
        {
            menuTopIndex = menuIndex;
        }
        if (menuTopIndex < menuIndex - MENU_SIZE_PER_PAGE + 1)
        {
            menuTopIndex = menuIndex - MENU_SIZE_PER_PAGE + 1;
        }

        selectorArrow.gameObject.SetActive(true);
        selectorArrow.color = new Color(1, 1, 1, 1);

        //BoxMenuScript bm = menuBaseO.GetComponent<BoxMenuScript>();

        //put this in init too
        upArrow.enabled = menuTopIndex > 0;
        downArrow.enabled = menuTopIndex < menuEntries.Length - MENU_SIZE_PER_PAGE && menuEntries.Length > MENU_SIZE_PER_PAGE;

        if (textbox != null)
        {
            if (menuIndex > menuEntries.Length - 1 || menuIndex < 0)
            {
                if (textbox.isActiveAndEnabled)
                {
                    textbox.SetText("", true, true);
                }
                textbox.transform.parent.transform.parent.gameObject.SetActive(false);
            }
            else
            {
                textbox.transform.parent.transform.parent.gameObject.SetActive(true);
                textbox.SetText(menuEntries[menuIndex].description, true, true);
            }
        }

        characterStats.ApplyUpdate(selectedPlayer);
        parent.UpdateItemCount();
        return;
    }

    public override Pause_SectionShared GetSubsection(object state)
    {
        //Debug.Log("Asked for subsection");
        return characterStats;
    }

    public override object GetState()
    {
        return new Pause_HandlerItem_ConsumableMenu.UpdateObject(menuIndex, selectedPlayer);
    }

    public override void Clear()
    {
        for (int i = 0; i < menuEntriesS.Count; i++)
        {
            Destroy(menuEntriesS[i].gameObject);
        }
        menuEntriesS = null;
        isInit = false;
    }

    public override void ReSetup(int index, int desiredLoadedTopIndex)
    {
        if (pip == Pause_HandlerItem.PauseItemPage.KeyItems)
        {
            menuEntriesS[index].Setup(menuEntries[index + desiredLoadedTopIndex], false, null, "#0000ff", "#000000");
        }
        else
        {
            menuEntriesS[index].Setup(menuEntries[index + desiredLoadedTopIndex]);
        }
    }
}
