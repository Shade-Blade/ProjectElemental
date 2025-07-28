using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattlePopupMenuScript : MenuHandler
{
    GameObject popupBox;
    BattlePopupScript popupBoxScript;

    string text;
    string[] vars;

    //bool normalPopup;   //I forgot what this is used for lol
    public bool exit;

    const float MIN_LIFETIME = 0.25f;
    public float time;

    #pragma warning disable CS0067
    public override event EventHandler<MenuExitEventArgs> menuExit;

    public static BattlePopupMenuScript BuildMenu(string popupText)//, bool normalPopup = false)
    {
        GameObject newObj = new GameObject("Popup");
        BattlePopupMenuScript newMenu = newObj.AddComponent<BattlePopupMenuScript>();

        newMenu.text = popupText;
        //newMenu.normalPopup = normalPopup;
        newMenu.exit = false;
        newMenu.Init();
        
        return newMenu;
    }

    public static BattlePopupMenuScript buildMenu(string popupText, string[] vars)//, bool normalPopup = false)
    {
        GameObject newObj = new GameObject("Popup");
        BattlePopupMenuScript newMenu = newObj.AddComponent<BattlePopupMenuScript>();

        newMenu.text = popupText;
        //newMenu.normalPopup = normalPopup;
        newMenu.vars = vars;
        newMenu.exit = false;
        newMenu.Init();

        return newMenu;
    }

    // Update is called once per frame
    void Update()
    {
        if (active)
        {
            MenuUpdate();
        }
    }

    void MenuUpdate()
    {
        time += Time.deltaTime;

        if (time > MIN_LIFETIME && ((InputManager.GetButtonDown(InputManager.Button.Start) || (InputManager.GetButtonDown(InputManager.Button.A) || InputManager.GetButtonDown(InputManager.Button.B))))) //Press B to go back
        {
            exit = true;
            PopSelf();
        }
    }

    public override void Init()
    {
        active = true;
        exit = false;

        if (popupBox == null)
        {
            popupBox = Instantiate(BattleControl.Instance.popupBoxBase, MainManager.Instance.Canvas.transform);
            popupBoxScript = popupBox.GetComponent<BattlePopupScript>();
        }
        popupBoxScript.SetText(text, vars, true, true);
    }
    public override void Clear()
    {
        active = false;
        if (popupBox != null)
        {
            Destroy(popupBox);
        }
    }

    public override MenuResult GetResult()
    {
        return new MenuResult(null);
    }
}
