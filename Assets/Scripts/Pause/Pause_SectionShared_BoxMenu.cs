using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

//analogous to the battle box menu
//but there are some 
public class Pause_SectionShared_BoxMenu : Pause_SectionShared
{
    public Image upArrow;
    public TextDisplayer upArrowControlHint;
    public Image downArrow;
    public TextDisplayer downArrowControlHint;
    public Image selectorArrow;
    public Image menuBox;
    public RectMask2D mask;

    //public TextDisplayer textBox;

    public const int MENU_SIZE_PER_PAGE = 10;
    public const int MENU_BUFFER = 8;

    public const float ARROW_OFFSET = 4f;

    public int menuIndex = -1;
    public int menuTopIndex = -1;
    public float visualTopIndex = -1;
    public float visualSelectIndex = -1;

    public int loadedTopIndex = -1;

    public GameObject pauseMenuEntry;

    public BoxMenuEntry[] menuEntries;
    protected List<BoxMenuEntryScript> menuEntriesS;

    //note: unlike the battle menu, not all the entries are loaded (only ones close to the visible range)
    //This is to prevent big lag spikes from trying to load every single badge icon

    //no real reason to apply this system to the battle menu since the realistic limit is nowhere near enough for the lag to be bad enough to try to mitigate

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
        downArrow.enabled = menuTopIndex < menuEntries.Length - MENU_SIZE_PER_PAGE && menuEntries.Length > MENU_SIZE_PER_PAGE;
        downArrowControlHint.enabled = downArrow.enabled;

        if (textbox != null)
        {
            textbox.SetText(menuEntries[menuIndex].description, true, true);
        }

        return;
    }

    public override void OnActive()
    {
        menuBox.color = new Color(0.9f, 0.9f, 0.9f, 1);
    }
    public override void OnInactive()
    {
        menuBox.color = new Color(0.75f, 0.75f, 0.75f, 1);
    }

    public override object GetState()
    {
        return new int[] {menuIndex, menuTopIndex};
    }

    public BoxMenuEntry GetMenuEntry(int index)
    {
        return menuEntries[index];
    }
    public int GetMenuEntryCount()
    {
        return menuEntries.Length;
    }

    public static Vector3 GetRelativePosition(float relIndex)
    {
        //similar thing to the other one
        return Vector3.left * 5 + Vector3.up * 105 + Vector3.down * (216 / (MENU_SIZE_PER_PAGE - 1)) * relIndex;
    }

    public virtual void Update()
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
                } else
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
            Vector3 next = Vector3.left * 170f + Vector3.up * 20 + GetRelativePosition(visualSelectIndex - visualTopIndex) + Vector3.up * ARROW_OFFSET;
            selectorArrow.transform.localPosition = next;

            //bm.selectorArrow.transform.localPosition = Vector3.left * 170f + menuEntriesO[menuIndex].transform.localPosition + Vector3.up * 7.5f;
            //Debug.Log(menuEntries[menuIndex].description);
        }
    }

    public virtual void ReSetup(int index, int desiredLoadedTopIndex)
    {
        menuEntriesS[index].Setup(menuEntries[index + desiredLoadedTopIndex]);
    }
}
