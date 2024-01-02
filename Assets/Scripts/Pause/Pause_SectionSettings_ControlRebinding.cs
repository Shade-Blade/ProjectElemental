using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pause_SectionSettings_ControlRebinding : Pause_SectionShared_BoxMenu
{
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

        if (InputManager.Instance.keyCodes == null)
        {
            InputManager.Instance.ResetToDefaultKeyCodes();
        }
        if (InputManager.Instance.inputText == null)
        {
            InputManager.Instance.LoadInputText();
        }
        string[][] at = InputManager.Instance.inputText;
        //Note: current values can be obtained from InputManager
        //only show the buttonsprites after the controls are set
        menuEntries = new BoxMenuEntry[at.Length - 2];
        for (int i = 1; i < at.Length - 1; i++)
        {            
            if (i == at.Length - 2)
            {
                menuEntries[i - 1] = new BoxMenuEntry(InputManager.Instance.GetResetText(1), InputManager.Instance.GetResetText(2), null, "", true, 0);
            } else
            {
                menuEntries[i - 1] = new BoxMenuEntry(InputManager.Instance.GetInputText((InputManager.Button)(i - 5), 1), InputManager.Instance.GetInputText((InputManager.Button)(i - 5), 2), null, "<voffset,-0.6em><buttonsprite," + (InputManager.Button)(i - 5) + ">", true, 0);
            }
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

        int index = (int)state;
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
        int i = index;
        if (index == menuEntries.Length - 1)
        {
            menuEntries[i] = new BoxMenuEntry(InputManager.Instance.GetResetText(1), InputManager.Instance.GetResetText(2), null, "");
        }
        else
        {
            if (InputManager.GetButtonString((InputManager.Button)(i - 4)).Equals(""))
            {
                menuEntries[i] = new BoxMenuEntry(InputManager.Instance.GetInputText((InputManager.Button)(i - 4), 1), InputManager.Instance.GetInputText((InputManager.Button)(i - 4), 2), null, "Listening...", true, 0);
            }
            else
            {
                menuEntries[i] = new BoxMenuEntry(InputManager.Instance.GetInputText((InputManager.Button)(i - 4), 1), InputManager.Instance.GetInputText((InputManager.Button)(i - 4), 2), null, "<voffset,-0.6em><buttonsprite," + (InputManager.Button)(i - 4) + ">", true, 0);
            }
        }

        //put this in init too
        upArrow.enabled = menuTopIndex > 0;
        downArrow.enabled = menuTopIndex < menuEntries.Length - MENU_SIZE_PER_PAGE && menuEntries.Length > MENU_SIZE_PER_PAGE;

        if (textbox != null)
        {
            if (menuEntries.Length == 0 || menuEntries[menuIndex].description == null || menuEntries[menuIndex].description.Length < 2)
            {
                textbox.SetText("", true, true);
            }
            else
            {
                textbox.SetText(menuEntries[menuIndex].description, true, true);
            }
        }

        return;
    }

    public override Pause_SectionShared GetSubsection(object state)
    {
        return null;
    }
}
