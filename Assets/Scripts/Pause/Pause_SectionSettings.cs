using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pause_SectionSettings : Pause_SectionShared_BoxMenu
{
    public Pause_SectionSettings_ControlRebinding rebindMenu;
    public GameObject rebindArrow;
    public bool startMenu;  //set in the prefab inspector, removes the bottom 2 options (reload save, exit to main menu)

    public override void Init()
    {
        isInit = true;
        rebindMenu.gameObject.SetActive(false);    //keep off by default
        rebindArrow.gameObject.SetActive(false);    //keep off by default

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

        if (SettingsManager.Instance.settingsText == null)
        {
            SettingsManager.Instance.LoadSettingsText();
        }
        if (SettingsManager.Instance.settingsValues == null || SettingsManager.Instance.settingsValues.Length == 0)
        {
            SettingsManager.Instance.LoadSettings();
        }
        string[][] at = SettingsManager.Instance.settingsText;
        int[] originalValues = SettingsManager.Instance.settingsValues;
        int[] maxValues = SettingsManager.Instance.settingsMaxValues;
        if (startMenu)
        {
            menuEntries = new BoxMenuEntry[at.Length - 4];  //shave off the bottom 2 options
        }
        else
        {
            menuEntries = new BoxMenuEntry[at.Length - 2];
        }
        for (int i = 1; i < menuEntries.Length + 1; i++)
        {
            string[] tempSettingsText = new string[maxValues[i - 1] + 1];
            for (int j = 0; j <= maxValues[i - 1]; j++)
            {
                tempSettingsText[j] = at[i][j + 5];
            }
            menuEntries[i - 1] = new SettingsMenuEntry(at[i][1], tempSettingsText, originalValues[i - 1], at[i][2]);
        }

        if (menuEntries == null)
        {
            menuEntries = new BoxMenuEntry[0];
        }

        menuEntriesS = new List<BoxMenuEntryScript>();

        for (int i = 0; i < MENU_SIZE_PER_PAGE + MENU_BUFFER * 2; i++)
        {
            if (i + loadedTopIndex > menuEntries.Length - 1)
            {
                break;
            }

            GameObject g = Instantiate(pauseMenuEntry, mask.transform);
            g.transform.localPosition = GetRelativePosition(i + loadedTopIndex - visualTopIndex);

            SettingsMenuEntryScript b = g.GetComponent<SettingsMenuEntryScript>();
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

        if (menuEntriesS.Count == 0)
        {
            selectorArrow.enabled = false;
        }
        else
        {
            selectorArrow.enabled = true;
            Vector3 targetLocal = Vector3.left * 270f + Vector3.up * 20 + GetRelativePosition(visualSelectIndex - visualTopIndex) + Vector3.up * ARROW_OFFSET;
            Vector3 current = selectorArrow.transform.localPosition;
            selectorArrow.transform.localPosition = targetLocal;
        }

        //Hacky
        textbox.transform.parent.transform.parent.gameObject.SetActive(false);
    }

    public override void Update()
    {
        if (visualTopIndex != menuTopIndex)
        {
            visualTopIndex = MainManager.EasingQuadraticTime(visualTopIndex, menuTopIndex, 100);

            int desiredLoadedTopIndex = Mathf.FloorToInt(visualTopIndex) - MENU_BUFFER;   //higher up

            if (desiredLoadedTopIndex < 0)
            {
                desiredLoadedTopIndex = 0;
            }

            if (desiredLoadedTopIndex != loadedTopIndex)
            {
                //make the difference by juggling the scripts around
                int shift = desiredLoadedTopIndex - loadedTopIndex;

                shift = -shift;

                if (shift > 0)
                {
                    if (shift > menuEntriesS.Count - 1)
                    {
                        shift = menuEntriesS.Count - 1;
                    }

                    //take stuff from end
                    for (int i = 0; i < shift; i++)
                    {
                        BoxMenuEntryScript bmes = menuEntriesS[menuEntriesS.Count - 1];
                        menuEntriesS.RemoveAt(menuEntriesS.Count - 1);
                        menuEntriesS.Insert(0, bmes);
                        bmes.gameObject.SetActive(true);
                    }

                    //there are now (shift) things at the start that need to be modified
                    for (int i = 0; i < shift; i++)
                    {
                        ReSetup(i, desiredLoadedTopIndex);
                    }
                }
                else
                {
                    shift = -shift;

                    if (shift > menuEntriesS.Count - 1)
                    {
                        shift = menuEntriesS.Count - 1;
                    }

                    //take stuff from start
                    for (int i = 0; i < shift; i++)
                    {
                        BoxMenuEntryScript bmes = menuEntriesS[0];
                        menuEntriesS.RemoveAt(0);
                        menuEntriesS.Add(bmes);
                        bmes.gameObject.SetActive(true);
                    }

                    //there are now (shift) things at the end that need to be modified
                    for (int i = menuEntriesS.Count - shift; i < menuEntriesS.Count; i++)
                    {
                        if (i + desiredLoadedTopIndex > menuEntries.Length - 1)
                        {
                            menuEntriesS[i].gameObject.SetActive(false);
                            break;
                        }
                        ReSetup(i, desiredLoadedTopIndex);
                    }
                }

                loadedTopIndex = desiredLoadedTopIndex;
            }

            for (int i = 0; i < menuEntriesS.Count; i++)
            {
                //Debug.Log(i - menuTopIndex);
                //menuEntriesO[i].transform.localPosition = BoxMenuScript.GetRelativePosition(i - menuTopIndex);
                menuEntriesS[i].transform.localPosition = GetRelativePosition(i + loadedTopIndex - visualTopIndex);
            }
        }

        if (menuEntries != null && menuEntries.Length > 0)
        {
            visualSelectIndex = MainManager.EasingQuadraticTime(visualSelectIndex, menuIndex, 400);

            if (visualSelectIndex < visualTopIndex)
            {
                visualSelectIndex = visualTopIndex;
            }
            if (visualSelectIndex > visualTopIndex + MENU_SIZE_PER_PAGE - 1)
            {
                visualSelectIndex = visualTopIndex + MENU_SIZE_PER_PAGE - 1;
            }
            Vector3 next = Vector3.left * 270f + Vector3.up * 20 + GetRelativePosition(visualSelectIndex - visualTopIndex) + Vector3.up * ARROW_OFFSET;
            selectorArrow.transform.localPosition = next;

            //bm.selectorArrow.transform.localPosition = Vector3.left * 170f + menuEntriesO[menuIndex].transform.localPosition + Vector3.up * 7.5f;
            //Debug.Log(menuEntries[menuIndex].description);
        }
    }

    public override void Clear()
    {
        rebindMenu.gameObject.SetActive(true);    //keep off by default
        rebindArrow.gameObject.SetActive(true);    //keep off by default

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

    public void ReenableRebindMenu()
    {
        rebindMenu.gameObject.SetActive(true);
        rebindArrow.gameObject.SetActive(true);
        rebindMenu.Init();
    }

    public void DisableRebindMenu()
    {
        rebindMenu.Clear();
        rebindMenu.gameObject.SetActive(false);
        rebindArrow.gameObject.SetActive(false);
    }

    public override void ApplyUpdate(object state)
    {
        if (state == null)  //resets the inventory
        {
            Clear();
            Init();
            return;
        }

        //if you aren't in the rebind menu then you will get updates
        if (rebindMenu.gameObject.activeSelf)
        {
            rebindMenu.Clear();
        }
        rebindMenu.gameObject.SetActive(false);
        rebindArrow.gameObject.SetActive(false);

        int[] intState = (int[])state;
        int index = intState[0];
        int level = intState[1];
        menuIndex = index;

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
        menuEntries[index].level = level;
        SettingsMenuEntry sme = ((SettingsMenuEntry)menuEntries[index]);
        string newRightText = "";
        if (sme.rightTextSet != null && sme.rightTextSet.Length > 0 && sme.level >= 0 && sme.level <= sme.rightTextSet.Length - 1)
        {
            newRightText = sme.rightTextSet[level];
        }
        menuEntries[index].rightText = newRightText;
        menuEntriesS[index].Setup(menuEntries[index]);

        //put this in init too
        upArrow.enabled = menuTopIndex > 0;
        downArrow.enabled = menuTopIndex < menuEntries.Length - MENU_SIZE_PER_PAGE && menuEntries.Length > MENU_SIZE_PER_PAGE;

        if (textbox != null)
        {
            if (menuEntries.Length == 0 || menuEntries[menuIndex].description == null || menuEntries[menuIndex].description.Length < 2)
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

        return;
    }

    public override Pause_SectionShared GetSubsection(object state)
    {
        //Debug.Log("Asked for subsection");
        return rebindMenu;
    }
}
