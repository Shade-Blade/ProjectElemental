using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static BaseBattleMenu;

public class StartMenu_Base : MenuHandler
{
    //unlike most other menus this one handles its own visuals without a separate script
    #pragma warning disable CS0067
    public override event EventHandler<MenuExitEventArgs> menuExit;

    public bool firstInit;

    public RectTransform logo;

    public const float startHeight = 500;
    public const float endHeight = 125;

    public RectTransform[] menuEntries;
    public int menuIndex;
    public RectTransform selector;

    public TextDisplayer controlHint;
    public TextDisplayer versionString;

    float inputDir;

    public GameObject subsection;

    public static StartMenu_Base BuildMenu()
    {
        GameObject newObj = Instantiate((GameObject)Resources.Load("Menu/StartMenu/StartMenu_Base"), MainManager.Instance.Canvas.transform);
        StartMenu_Base newMenu = newObj.GetComponent<StartMenu_Base>();
        newMenu.firstInit = true;
        newMenu.Init();
        return newMenu;
    }

    //debug
    public void Start()
    {
        firstInit = true;
        Init();
    }

    public override void Init()
    {
        base.Init();

        if (subsection != null)
        {
            Destroy(subsection);
        }

        logo.gameObject.SetActive(true);
        selector.gameObject.SetActive(true);
        for (int i = 0; i < menuEntries.Length; i++)
        {
            menuEntries[i].gameObject.SetActive(true);
        }
        controlHint.gameObject.SetActive(true);
        controlHint.SetText("Select <buttonsprite,A> - Go Back <buttonsprite,B> - Up <buttonsprite,Up> - Down <buttonsprite,Down>", true, true);

        versionString.SetText("Version: " + MainManager.GetVersionString(), true, true);

        if (firstInit)
        {
            logo.anchoredPosition = Vector2.up * startHeight;
        } else
        {
            logo.anchoredPosition = Vector2.up * endHeight;
        }

        selector.anchoredPosition = Vector2.left * (150) + Vector2.up * menuEntries[menuIndex].anchoredPosition.y;

        firstInit = false;
    }

    public override void Clear()
    {
        base.Clear();

        logo.gameObject.SetActive(false);
        selector.gameObject.SetActive(false);
        for (int i = 0; i < menuEntries.Length; i++)
        {
            menuEntries[i].gameObject.SetActive(false);
        }
        controlHint.gameObject.SetActive(false);
    }

    public void Update()
    {
        if (active)
        {
            MenuUpdate();
        }
    }

    public void MenuUpdate()
    {
        lifetime += Time.deltaTime;
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

            if (menuIndex > menuEntries.Length - 1)
            {
                menuIndex = 0;
            }
            if (menuIndex < 0)
            {
                menuIndex = menuEntries.Length - 1;
            }
        }

        logo.anchoredPosition = Vector2.up * MainManager.EasingQuadraticTime(logo.anchoredPosition.y, endHeight, 3333);
        selector.anchoredPosition = MainManager.EasingQuadraticTime(selector.anchoredPosition, Vector2.left * (150) + Vector2.up * menuEntries[menuIndex].anchoredPosition.y, 2000);

        if (lifetime > MIN_SELECT_TIME && InputManager.GetButtonDown(InputManager.Button.A)) //Press A to select stuff
        {
            SelectOption();
        }
    }

    public void SelectOption()
    {
        MainManager.Instance.PlayGlobalSound(MainManager.Sound.Menu_Select);
        //hardcoded
        switch (menuIndex)
        {
            case 0: //Start Game                
                StartMenu_FileSelect sm_fs = StartMenu_FileSelect.BuildMenu();
                MenuHandler s = sm_fs;
                s.transform.parent = transform;
                PushState(s);
                break;
            case 1: //Settings
                //Reusing the pause menu version
                //(Though slightly different because I have set a special start menu mode flag)
                Pause_SectionSettings ss = Instantiate((GameObject)Resources.Load("Menu/StartMenu/StartMenu_SettingsMenu"), transform).GetComponent<Pause_SectionSettings>();
                subsection = ss.gameObject;
                ss.Init();
                Pause_HandlerSettings se = Pause_HandlerSettings.BuildMenu(ss, true);
                MenuHandler b = se;
                b.transform.parent = transform;
                PushState(b);                
                break;
            case 2: //Exit
                Application.Quit();
                break;
        }
    }
}
