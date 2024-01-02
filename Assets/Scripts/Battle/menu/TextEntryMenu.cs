using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextEntryMenu : MenuHandler
{
    int maxLength;
    string currString;

    bool cancel;

    public GameObject baseObject;
    public TextEntryMenuScript tms;

    public string topText;

    public override event EventHandler<MenuExitEventArgs> menuExit;

    public static TextEntryMenu BuildMenu(int max, string baseString = null, string topString = null)
    {
        GameObject newObj = new GameObject("Text Entry Menu");
        TextEntryMenu newMenu = newObj.AddComponent<TextEntryMenu>();
        newMenu.maxLength = max;
        newMenu.currString = baseString;

        newMenu.topText = topString;

        newMenu.Init();
        return newMenu;
    }

    public override void Init()
    {
        active = true;
        cancel = false;

        baseObject = Instantiate(MainManager.Instance.textEntryMenu, MainManager.Instance.Canvas.transform);
        tms = baseObject.GetComponent<TextEntryMenuScript>();

        if (MainManager.Instance.Cheat_UnrestrictedTextEntry)
        {
            tms.text.SetText(currString, true, true);
        }
        else
        {
            tms.text.SetText(FormattedString.InsertEscapeSequences(currString), true, true);
        }
        tms.lengthIndicator.SetText(currString.Length + "/" + maxLength, true, true);

        if (topText == null)
        {
            tms.topTextHolder.SetActive(false);
        } else
        {
            tms.topTextHolder.SetActive(true);
            tms.topText.SetText(topText, true, true);
        }
    }
    public override void Clear()
    {
        active = false;
        Destroy(baseObject);
    }

    void Update()
    {
        if (active)
        {
            MenuUpdate();
        }
    }

    public void OnGUI()
    {
        if (!active)
        {
            return;
        }

        Event e = Event.current;
        
        if (e.type == EventType.KeyDown)
        {
            if (e.isKey)
            {
                if (e.character != '\0' && e.character != '\n' && e.character != '\t' && e.character != '\r')   //ban a few characters specifically
                {
                    if (MainManager.Instance.Cheat_UnrestrictedTextEntry || currString.Length < maxLength)
                    {
                        currString = currString + e.character;
                    }
                }
            }

            if (e.keyCode == KeyCode.Backspace)
            {
                if (currString.Length > 0)
                {
                    currString = currString.Substring(0, currString.Length - 1);
                }
            }

            if (MainManager.Instance.Cheat_UnrestrictedTextEntry)
            {
                tms.text.SetText(currString, true, true);
            }
            else
            {
                tms.text.SetText(FormattedString.InsertEscapeSequences(currString), true, true);
            }
            tms.lengthIndicator.SetText(currString.Length + "/" + maxLength, true, true);

            if (e.keyCode == KeyCode.Escape)
            {
                Cancel();
            }

            if (e.keyCode == KeyCode.Return)
            {
                SelectOption();
            }
        }
    }

    public void MenuUpdate()
    {
        //???
    }

    public void SelectOption()
    {
        InvokeExit(this, new MenuExitEventArgs(GetFullResult()));
    }

    public void Cancel()
    {
        cancel = true;
        InvokeExit(this, new MenuExitEventArgs(GetFullResult()));
    }

    public void InvokeExit(object sender, MenuExitEventArgs meea)
    {
        menuExit?.Invoke(this, new MenuExitEventArgs(GetFullResult()));
    }
    public override MenuResult GetResult()
    {
        //cut out trailing spaces (allowable in Unrestricted mode)
        string newString = currString;

        if (currString == null)
        {
            return new MenuResult("");
        }

        if (!MainManager.Instance.Cheat_UnrestrictedTextEntry)
        {
            int cut = 0;
            for (int i = newString.Length - 1; i >= 0; i--)
            {
                if (newString[i] == ' ')
                {
                    cut++;
                }
                else
                {
                    break;
                }
            }

            newString = newString.Substring(0, currString.Length - cut);
        }

        //note: check for cancellation by checking for a 0 length string
        //In most environments that shouldn't be treated as valid anyway
        if (MainManager.Instance.Cheat_UnrestrictedTextEntry)
        {
            return new MenuResult(cancel ? "" : newString);
        }
        else
        {
            //add escape sequences to make this safe
            return new MenuResult(cancel ? "" : FormattedString.InsertEscapeSequences(newString));
        }
    }
}
