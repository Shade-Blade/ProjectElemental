using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StartMenu_FileSelect : MenuHandler
{
    //unlike most other menus this one handles its own visuals without a separate script
    #pragma warning disable CS0067
    public override event EventHandler<MenuExitEventArgs> menuExit;

    public Image[] bottomImages;

    public FileMenuEntryScript[] menuEntries;
    public int menuIndex = -1;
    public int bottomIndex = -1;

    public int copyIndex;

    public RectTransform selector;
    public RectTransform copyselector;

    public PromptBoxMenu prompt;
    public bool awaitSpecial;   //note: mode and index can be used to infer the action to take

    public TextEntryMenu textEntry;
    public bool awaitTextEntry;

    enum FileSelectMode
    {
        None = 0,
        Delete,
        Copy,
        Rename
    }
    FileSelectMode selectMode;

    float inputDir;
    float horizontalInputDir;

    public GameObject subsection;

    public static StartMenu_FileSelect BuildMenu()
    {
        GameObject newObj = Instantiate((GameObject)Resources.Load("Menu/StartMenu/StartMenu_FileSelect"), MainManager.Instance.Canvas.transform);
        StartMenu_FileSelect newMenu = newObj.GetComponent<StartMenu_FileSelect>();
        newMenu.Init();
        return newMenu;
    }

    //debug
    /*
    public void Start()
    {
        firstInit = true;
        Init();
    }
    */

    public override void Init()
    {
        base.Init();

        if (menuIndex == -1)
        {
            menuIndex = 0;
        }
        if (bottomIndex == -1)
        {
            bottomIndex = 0;
        }

        for (int i = 0; i < menuEntries.Length; i++)
        {
            menuEntries[i].gameObject.SetActive(true);
        }
        selector.gameObject.SetActive(true);
        copyselector.gameObject.SetActive(false);
        for (int i = 0; i < bottomImages.Length; i++)
        {
            bottomImages[i].gameObject.SetActive(true);
        }

        if (menuIndex == menuEntries.Length)
        {
            selector.anchoredPosition = Vector2.left * (125 + ((bottomIndex - 1) * -250)) + Vector2.up * (-265);
        }
        else
        {
            selector.anchoredPosition = Vector2.left * (400) + Vector2.up * menuEntries[menuIndex].gameObject.transform.localPosition.y;
        }

        for (int i = 0; i < menuEntries.Length; i++)
        {
            menuEntries[i].Setup(FileMenuEntry.ConstructFileMenuEntry(i));
        }


        if (subsection != null)
        {
            Destroy(subsection);
        }
    }

    public override void Clear()
    {
        base.Clear();

        for (int i = 0; i < menuEntries.Length; i++)
        {
            menuEntries[i].gameObject.SetActive(false);
        }
        selector.gameObject.SetActive(false);
        copyselector.gameObject.SetActive(false);
        for (int i = 0; i < bottomImages.Length; i++)
        {
            bottomImages[i].gameObject.SetActive(false);
        }

        //destroy the gameobject to delete everything for real
    }

    public void Update()
    {
        if (menuIndex == menuEntries.Length)
        {
            selector.anchoredPosition = MainManager.EasingQuadraticTime(selector.anchoredPosition, Vector2.left * (125 + ((bottomIndex - 1) * -250)) + Vector2.up * (-240), 2000);
        }
        else
        {
            selector.anchoredPosition = MainManager.EasingQuadraticTime(selector.anchoredPosition, Vector2.left * (400) + Vector2.up * menuEntries[menuIndex].gameObject.transform.localPosition.y, 2000);
        }

        if (active)
        {
            MenuUpdate();
        }
    }

    public void MenuUpdate()
    {
        lifetime += Time.deltaTime;

        //improper to make the menu work like this but ehh
        if (prompt != null && !prompt.menuDone)
        {
            //Reset these so that things don't become sus after you select something
            inputDir = 0;
            horizontalInputDir = 0;
            return;
        }
        if (textEntry != null)
        {
            inputDir = 0;
            horizontalInputDir = 0;
            return;
        }
        if (prompt != null && prompt.menuDone && awaitSpecial)
        {
            awaitSpecial = false;
            int index = prompt.menuIndex;
            prompt.Clear();
            Destroy(prompt.gameObject);
            prompt = null;
            if (index == 0)
            {
                //Infer the action to take
                switch (selectMode)
                {
                    case FileSelectMode.None:
                        //Load the file
                        MainManager.Instance.SnapFade(1);
                        MainManager.Instance.LoadSave(menuIndex);
                        MainManager.Instance.startMenu.Clear();
                        break;
                    case FileSelectMode.Delete:
                        MainManager.Instance.DeleteSave(menuIndex);
                        selectMode = FileSelectMode.None;
                        break;
                    case FileSelectMode.Copy:
                        MainManager.Instance.CopySave(copyIndex, menuIndex);
                        copyIndex = -1;
                        selectMode = FileSelectMode.None;
                        break;
                    case FileSelectMode.Rename:
                        selectMode = FileSelectMode.None;
                        break;
                }
            }
            menuEntries[menuIndex].Setup(FileMenuEntry.ConstructFileMenuEntry(menuIndex));
            return;
        }


        if ((lifetime > MIN_SELECT_TIME && Mathf.Sign(InputManager.GetAxisVertical()) != inputDir) || InputManager.GetAxisVertical() == 0)
        {
            inputDir = Mathf.Sign(InputManager.GetAxisVertical());
            if (InputManager.GetAxisVertical() == 0)
            {
                inputDir = 0;
            }
            //Debug.Log(InputManager.GetAxisHorizontal());
            //now go


            if (inputDir != 0)
            {
                //inputDir positive = up and - index, negative = down and + index
                if (inputDir > 0)
                {
                    MainManager.Instance.PlayGlobalSound(MainManager.Sound.Menu_ScrollUp);
                    menuIndex--;
                }
                else
                {
                    MainManager.Instance.PlayGlobalSound(MainManager.Sound.Menu_ScrollDown);
                    menuIndex++;
                }
            }

            if (menuIndex > menuEntries.Length)
            {
                menuIndex = menuEntries.Length;
            }
            if (menuIndex < 0)
            {
                menuIndex = 0;
            }
        }

        if (menuIndex != menuEntries.Length)
        {
            horizontalInputDir = 0;
        } else
        {
            if ((lifetime > MIN_SELECT_TIME && Mathf.Sign(InputManager.GetAxisHorizontal()) != horizontalInputDir) || InputManager.GetAxisHorizontal() == 0)
            {
                horizontalInputDir = Mathf.Sign(InputManager.GetAxisHorizontal());
                if (InputManager.GetAxisHorizontal() == 0)
                {
                    horizontalInputDir = 0;
                }
                //Debug.Log(InputManager.GetAxisHorizontal());
                //now go


                if (horizontalInputDir != 0)
                {
                    //inputDir positive = up and - index, negative = down and + index
                    if (horizontalInputDir > 0)
                    {
                        if (bottomIndex < 2)
                        {
                            MainManager.Instance.PlayGlobalSound(MainManager.Sound.Menu_ScrollRight);
                        }
                        bottomIndex++;
                    }
                    else
                    {
                        if (bottomIndex > 0)
                        {
                            MainManager.Instance.PlayGlobalSound(MainManager.Sound.Menu_ScrollLeft);
                        }
                        bottomIndex--;
                    }
                }

                if (bottomIndex > 2)
                {
                    bottomIndex = 2;
                }
                if (bottomIndex < 0)
                {
                    bottomIndex = 0;
                }
            }
        }


        for (int i = 0; i < bottomImages.Length; i++)
        {
            bottomImages[i].color = new Color(0.9f, 0.9f, 0.9f, 0.8f);
        }
        switch (selectMode)
        {
            case FileSelectMode.Delete:
                bottomImages[0].color = new Color(1, 0.6f, 0.6f, 0.8f);
                break;
            case FileSelectMode.Copy:
                bottomImages[1].color = new Color(0.6f, 1, 1, 0.8f);
                break;
            case FileSelectMode.Rename:
                bottomImages[2].color = new Color(0.6f, 1, 0.6f, 0.8f);
                break;
            default:
                break;
        }

        if (copyIndex < 0)
        {
            copyselector.gameObject.SetActive(false);
        }

        if (lifetime > MIN_SELECT_TIME && (InputManager.GetButtonDown(InputManager.Button.A) || InputManager.GetButtonDown(InputManager.Button.Start))) //Press A to select stuff
        {
            SelectOption();
        }
        if (lifetime > MIN_SELECT_TIME && InputManager.GetButtonDown(InputManager.Button.B))
        {
            if (selectMode != FileSelectMode.None)
            {
                MainManager.Instance.PlayGlobalSound(MainManager.Sound.Menu_Cancel);
                selectMode = FileSelectMode.None;
                copyIndex = -1;
            } else
            {
                MainManager.Instance.PlayGlobalSound(MainManager.Sound.Menu_Cancel);
                PopSelf();
            }
        }
    }

    public void SelectOption()
    {        
        if (menuIndex != menuEntries.Length)
        {
            //Debug.Log(menuEntries[menuIndex].entry.name != null ? menuEntries[menuIndex].entry.name.Length : "X");

            //Select
            switch (selectMode)
            {
                case FileSelectMode.Delete:
                    if (menuEntries[menuIndex].entry.name != null && menuEntries[menuIndex].entry.name.Length > 0)
                    {
                        MainManager.Instance.PlayGlobalSound(MainManager.Sound.Menu_Select);
                        prompt = PromptBoxMenu.BuildMenu(new string[] { "Yes", "No" }, new string[] { "0", "1" }, 1, "Are you sure you want to delete?");
                        awaitSpecial = true;
                    } else
                    {
                        MainManager.Instance.PlayGlobalSound(MainManager.Sound.Menu_Error);
                    }
                    break;
                case FileSelectMode.Copy:
                    if (copyIndex < 0)
                    {
                        if (menuEntries[menuIndex].entry.name != null && menuEntries[menuIndex].entry.name.Length > 0)
                        {
                            MainManager.Instance.PlayGlobalSound(MainManager.Sound.Menu_Select);
                            copyIndex = menuIndex;
                            copyselector.gameObject.SetActive(true);
                            copyselector.anchoredPosition = Vector2.left * (400) + Vector2.up * menuEntries[menuIndex].gameObject.transform.localPosition.y;
                        } else
                        {
                            MainManager.Instance.PlayGlobalSound(MainManager.Sound.Menu_Error);
                        }
                    }
                    else
                    {
                        if (copyIndex != menuIndex)
                        {
                            MainManager.Instance.PlayGlobalSound(MainManager.Sound.Menu_Select);
                            prompt = PromptBoxMenu.BuildMenu(new string[] { "Yes", "No" }, new string[] { "0", "1" }, 1, "Are you sure you want to copy?");
                            awaitSpecial = true;
                        } else
                        {
                            MainManager.Instance.PlayGlobalSound(MainManager.Sound.Menu_Error);
                        }
                    }
                    break;
                case FileSelectMode.Rename:
                    if (menuEntries[menuIndex].entry.name != null && menuEntries[menuIndex].entry.name.Length > 0)
                    {
                        MainManager.Instance.PlayGlobalSound(MainManager.Sound.Menu_Select);
                        //File name entry
                        string oldName = menuEntries[menuIndex].entry.name.Substring(3);
                        textEntry = TextEntryMenu.BuildMenu(16, oldName, "Choose a new name for your file.<line><size,60%>(Use Escape to cancel.)</size>");
                        textEntry.menuExit += TextDone;
                        awaitTextEntry = true;
                    } else
                    {
                        MainManager.Instance.PlayGlobalSound(MainManager.Sound.Menu_Error);
                    }
                    break;
                default:
                    if (menuEntries[menuIndex].entry != null)
                    {
                        if (menuEntries[menuIndex].entry.name != null && menuEntries[menuIndex].entry.name.Length > 0)
                        {
                            MainManager.Instance.PlayGlobalSound(MainManager.Sound.Menu_Select);
                            prompt = PromptBoxMenu.BuildMenu(new string[] { "Yes", "No" }, new string[] { "0", "1" }, 1, "Start game in File " + (menuIndex + 1) + "?");
                            awaitSpecial = true;
                        }
                        else
                        {
                            MainManager.Instance.PlayGlobalSound(MainManager.Sound.Menu_Select);
                            textEntry = TextEntryMenu.BuildMenu(16, "", "Choose a name for your new file.<line><size,60%>(Use Escape to cancel.)</size>");
                            textEntry.menuExit += TextDone;
                            awaitTextEntry = true;
                        }
                    }
                    break;
            }
        } else
        {
            MainManager.Instance.PlayGlobalSound(MainManager.Sound.Menu_Select);
            if (bottomIndex == (int)selectMode - 1)
            {
                selectMode = FileSelectMode.None;
                copyIndex = -1;
            }
            else
            {
                selectMode = (FileSelectMode)(bottomIndex + 1);
                copyIndex = -1;
            }
        }
    }

    private void TextDone(object sender, MenuExitEventArgs e)
    {
        MenuResult mr = e.mr;
        string output = (string)mr.output;

        //Debug.Log(output != null ? output : "X");
        //Debug.Log(menuIndex);

        if (output != null && output.Length > 0)
        {
            switch (selectMode)
            {
                case FileSelectMode.None:
                    MainManager.Instance.CreateNewSave(menuIndex, output);
                    break;
                case FileSelectMode.Rename:
                    Debug.Log(menuEntries[menuIndex].entry.playtime + " vs 0:00:00 " + (menuEntries[menuIndex].entry.playtime).Equals("0:00:00"));
                    MainManager.Instance.RenameSave(menuIndex, output, menuEntries[menuIndex].entry.playtime.Equals("0:00:00"));
                    selectMode = FileSelectMode.None;
                    break;
            }
            menuEntries[menuIndex].Setup(FileMenuEntry.ConstructFileMenuEntry(menuIndex));
        }

        awaitTextEntry = false;
        textEntry.Clear();
        Destroy(textEntry.gameObject);
    }
}
