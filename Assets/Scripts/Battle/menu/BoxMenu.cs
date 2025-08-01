using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class BoxMenu : MenuHandler
{
    public const int MENU_SIZE_PER_PAGE = 6;
    public const float HYPER_SCROLL_TIME = 0.3f;

    public BoxMenuEntry[] menuEntries;
    public int menuIndex = -1;
    public int menuTopIndex = -1;
    public float visualTopIndex = -1;
    public float visualSelectIndex = -1;
    protected float inputDir;
    protected float lrinputDir;
    protected float holdDur;
    protected int holdValue;
    public BaseBattleMenu.BaseMenuName menuName;

    protected PlayerEntity caller;

    protected bool canUseDisabled;

    protected GameObject menuBaseO;
    protected List<GameObject> menuEntriesO;

    //maybe I should not create them if not needed
    protected GameObject descriptionBoxO;
    protected DescriptionBoxScript descriptionBoxScript;

    protected BoxMenuScript bm;

    public string descriptorString;

    public override event EventHandler<MenuExitEventArgs> menuExit;

    public RectTransform rectTransform;

    //public float lifetime;
    //public const float MIN_SELECT_TIME = 0.0667f;


    public static BoxMenu BuildMenu(PlayerEntity caller, BaseBattleMenu.BaseMenuName p_menuName)
    {
        GameObject newObj = new GameObject("Box Menu");
        BoxMenu newMenu;
        switch (p_menuName)
        {
            case BaseBattleMenu.BaseMenuName.Jump:
                newMenu = newObj.AddComponent<MoveBoxMenu>();
                break;
            case BaseBattleMenu.BaseMenuName.Weapon:
                newMenu = newObj.AddComponent<MoveBoxMenu>();
                break;
            case BaseBattleMenu.BaseMenuName.Soul:
                newMenu = newObj.AddComponent<MoveBoxMenu>();
                break;
            case BaseBattleMenu.BaseMenuName.Items:
                newMenu = newObj.AddComponent<ItemBoxMenu>();
                break;
            case BaseBattleMenu.BaseMenuName.Tactics:
                newMenu = newObj.AddComponent<TacticsBoxMenu>();
                break;
            case BaseBattleMenu.BaseMenuName.BadgeSwap:
                newMenu = newObj.AddComponent<BadgeSwapBoxMenu>();
                break;
            case BaseBattleMenu.BaseMenuName.RibbonSwap:
                newMenu = newObj.AddComponent<RibbonSwapBoxMenu>();
                break;
            case BaseBattleMenu.BaseMenuName.MetaItems:
                newMenu = newObj.AddComponent<MetaItemBoxMenu>();
                break;
            //case BaseBattleMenu.BaseMenuName.SwitchCharacters:
            //    newMenu = newObj.AddComponent<SwitchCharactersBoxMenu>();
            //    break;
            default:
                newMenu = newObj.AddComponent<BoxMenu>();
                break;
        }

        newMenu.caller = caller;
        newMenu.menuName = p_menuName;
        newMenu.Init();

        return newMenu;
    }

    public static ItemBoxMenu BuildSpecialItemMenu(PlayerEntity caller, List<Item> specialList, List<bool> hasBackground = null, List<Color> colorList = null, List<bool> blockList = null, string descriptor = null)
    {
        GameObject newObj = new GameObject("Box Menu");
        ItemBoxMenu newMenu;
        newMenu = newObj.AddComponent<ItemBoxMenu>();

        newMenu.specialList = specialList;
        newMenu.hasBackground = hasBackground;
        newMenu.backgroundColors = colorList;
        newMenu.blockList = blockList;
        newMenu.caller = caller;
        newMenu.descriptorString = descriptor;
        newMenu.menuName = BaseBattleMenu.BaseMenuName.Items;
        newMenu.Init();

        return newMenu;
    }

    public override void Init()
    {
        lifetime = 0;
        active = true;
        if (menuIndex == -1)
        {
            menuIndex = 0;
            menuTopIndex = 0;
        }
        visualTopIndex = menuTopIndex;
        visualSelectIndex = menuIndex;

        menuBaseO = Instantiate(MainManager.Instance.menuBase, MainManager.Instance.Canvas.transform);
        descriptionBoxO = Instantiate(MainManager.Instance.descriptionBoxBase, MainManager.Instance.Canvas.transform);
        descriptionBoxO.transform.SetSiblingIndex(menuBaseO.transform.GetSiblingIndex());
        descriptionBoxScript = descriptionBoxO.GetComponent<DescriptionBoxScript>();
        //menuBaseO.transform.position = new Vector3(250, -70, 0);
        menuEntriesO = new List<GameObject>();
        bm = menuBaseO.GetComponent<BoxMenuScript>();

        rectTransform = menuBaseO.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = GetMenuAnchoredPosition() + Vector2.left * 300;
    }
    public void PostEntriesInit()
    {
        bm.upArrow.enabled = false; //menuTopIndex > 0;
        bm.downArrow.enabled = menuTopIndex < menuEntries.Length - MENU_SIZE_PER_PAGE && menuEntries.Length > MENU_SIZE_PER_PAGE;

        if (menuEntries[menuIndex].maxLevel <= 1)
        {
            bm.SetLevelChangeIndicator(false);
        }
        else
        {
            bm.SetLevelChangeIndicator(true);
        }

        if (descriptorString != null)
        {
            bm.descriptorBox.enabled = true;
            bm.descriptorTextBox.SetText(descriptorString, true);

            //Resize it to fit the text
            //float height = 22;
            bm.descriptorBox.rectTransform.sizeDelta = new Vector2(bm.descriptorTextBox.textMesh.GetRenderedValues()[0] + 20, bm.descriptorTextBox.textMesh.GetRenderedValues()[1] * 1.1f + 10);
        }
        else
        {
            bm.descriptorBox.enabled = false;
            bm.descriptorTextBox.gameObject.SetActive(false);
        }

        if (menuEntries[menuIndex].maxLevel <= 1)
        {
            bm.SetLevelChangeIndicator(false);
        }
        else
        {
            bm.SetLevelChangeIndicator(true);
        }

        for (int i = 0; i < menuEntriesO.Count; i++)
        {
            //Debug.Log(i - menuTopIndex);
            visualTopIndex = MainManager.EasingQuadraticTime(visualTopIndex, menuTopIndex, 25);
            //menuEntriesO[i].transform.localPosition = BoxMenuScript.GetRelativePosition(i - menuTopIndex);
            menuEntriesO[i].transform.localPosition = BoxMenuScript.GetRelativePosition(i - visualTopIndex);
        }

        bm.selectorArrow.transform.localPosition = GetSelectorPosition(menuEntriesO[menuIndex].transform.localPosition);
        descriptionBoxScript.SetText(menuEntries[menuIndex].description);
    }

    void Update()
    {
        if (active)
        {
            MenuUpdate();
        }
    }

    protected virtual void MenuUpdate()
    {
        rectTransform.anchoredPosition = MainManager.EasingQuadraticTime(rectTransform.anchoredPosition, GetMenuAnchoredPosition(), 30000);

        lifetime += Time.deltaTime;
        bool indexChange = false;
        if ((lifetime > MIN_SELECT_TIME && Mathf.Sign(InputManager.GetAxisVertical()) != inputDir) || InputManager.GetAxisVertical() == 0)
        {
            holdDur = 0;
            holdValue = 0;
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
                    indexChange = true;
                    menuIndex--;
                }
                else
                {
                    MainManager.Instance.PlayGlobalSound(MainManager.Sound.Menu_ScrollDown);
                    indexChange = true;
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

            if (menuEntries[menuIndex].maxLevel <= 1)
            {
                bm.SetLevelChangeIndicator(false);
            } else
            {
                bm.SetLevelChangeIndicator(true);
            }

            OnHover(menuEntries[menuIndex]);

            if (menuTopIndex > menuIndex)
            {
                menuTopIndex = menuIndex;
            }
            if (menuTopIndex < menuIndex - MENU_SIZE_PER_PAGE + 1)
            {
                menuTopIndex = menuIndex - MENU_SIZE_PER_PAGE + 1;
            }

            //BoxMenuScript bm = menuBaseO.GetComponent<BoxMenuScript>();
            bm.upArrow.enabled = menuTopIndex > 0;
            bm.downArrow.enabled = menuTopIndex < menuEntries.Length - MENU_SIZE_PER_PAGE && menuEntries.Length > MENU_SIZE_PER_PAGE;
        }
        if (lifetime > MIN_SELECT_TIME && Mathf.Sign(InputManager.GetAxisVertical()) == inputDir && InputManager.GetAxisVertical() != 0)
        {
            holdDur += Time.deltaTime;

            if (holdDur >= HYPER_SCROLL_TIME)
            {
                int pastHoldValue = holdValue;
                indexChange = true;

                if (MainManager.Instance.GetHyperScrollRate() * (holdDur - HYPER_SCROLL_TIME) > holdValue)
                {
                    holdValue = (int)(MainManager.Instance.GetHyperScrollRate() * (holdDur - HYPER_SCROLL_TIME));
                }

                if (inputDir > 0)
                {
                    if (menuIndex > 0 && (holdValue - pastHoldValue) > 0)
                    {
                        MainManager.Instance.PlayGlobalSound(MainManager.Sound.Menu_ScrollUp);
                    }
                    menuIndex -= (holdValue - pastHoldValue);
                }
                else
                {
                    if (menuIndex < menuEntries.Length - 1 && (holdValue - pastHoldValue) > 0)
                    {
                        MainManager.Instance.PlayGlobalSound(MainManager.Sound.Menu_ScrollDown);
                    }
                    menuIndex += (holdValue - pastHoldValue);
                }

                //No loop around
                if (menuIndex > menuEntries.Length - 1)
                {
                    menuIndex = menuEntries.Length - 1;
                    indexChange = false;    //this block cancels movement so index didn't actually change
                }
                if (menuIndex < 0)
                {
                    menuIndex = 0;
                    indexChange = false;    //this block cancels movement so index didn't actually change
                }

                if (menuEntries[menuIndex].maxLevel <= 1)
                {
                    bm.SetLevelChangeIndicator(false);
                }
                else
                {
                    bm.SetLevelChangeIndicator(true);
                }

                OnHover(menuEntries[menuIndex]);

                if (menuTopIndex > menuIndex)
                {
                    menuTopIndex = menuIndex;
                }
                if (menuTopIndex < menuIndex - MENU_SIZE_PER_PAGE + 1)
                {
                    menuTopIndex = menuIndex - MENU_SIZE_PER_PAGE + 1;
                }

                //BoxMenuScript bm = menuBaseO.GetComponent<BoxMenuScript>();
                bm.upArrow.enabled = menuTopIndex > 0;
                bm.downArrow.enabled = menuTopIndex < menuEntries.Length - MENU_SIZE_PER_PAGE && menuEntries.Length > MENU_SIZE_PER_PAGE;
            }
        }

        if (visualTopIndex != menuTopIndex)
        {
            visualTopIndex = MainManager.EasingQuadraticTime(visualTopIndex, menuTopIndex, 100);
            for (int i = 0; i < menuEntriesO.Count; i++)
            {
                //Debug.Log(i - menuTopIndex);
                //menuEntriesO[i].transform.localPosition = BoxMenuScript.GetRelativePosition(i - menuTopIndex);
                menuEntriesO[i].transform.localPosition = BoxMenuScript.GetRelativePosition(i - visualTopIndex);
            }
        }

        if (menuEntries.Length > 0)
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

            Vector3 next = GetSelectorPosition(BoxMenuScript.GetRelativePosition(visualSelectIndex - visualTopIndex));
            bm.selectorArrow.transform.localPosition = next;

            //bm.selectorArrow.transform.localPosition = Vector3.left * 170f + menuEntriesO[menuIndex].transform.localPosition + Vector3.up * 7.5f;
            if (indexChange && descriptionBoxScript != null)
            {
                //note: this needs to be in Init too
                descriptionBoxScript.SetText(menuEntries[menuIndex].description);
            }
            //Debug.Log(menuEntries[menuIndex].description);
        }

        if (lifetime > MIN_SELECT_TIME && Mathf.Sign(InputManager.GetAxisHorizontal()) != lrinputDir || InputManager.GetAxisHorizontal() == 0)
        {
            lrinputDir = Mathf.Sign(InputManager.GetAxisHorizontal());
            if (InputManager.GetAxisHorizontal() == 0)
            {
                lrinputDir = 0;
            }

            if (lrinputDir != 0)
            {
                if (lrinputDir > 0)
                {
                    if (menuEntries[menuIndex].maxLevel > 1)
                    {
                        MainManager.Instance.PlayGlobalSound(MainManager.Sound.Menu_ScrollRight);
                    }
                    //try to increment level
                    IncrementLevel(1);
                } else
                {
                    if (menuEntries[menuIndex].maxLevel > 1)
                    {
                        MainManager.Instance.PlayGlobalSound(MainManager.Sound.Menu_ScrollLeft);
                    }
                    //try to decrement level
                    IncrementLevel(-1);
                }
            }
        }

        if ((InputManager.GetButtonDown(InputManager.Button.Start) || InputManager.GetButtonDown(InputManager.Button.A)) && menuEntries.Length > 0 && (menuEntries[menuIndex].canUse || canUseDisabled) && menuEntries.Length > 0 && lifetime > MIN_SELECT_TIME) //Press A to select stuff
        {
            SelectOption();
        } else if (((InputManager.GetButtonDown(InputManager.Button.Start) || InputManager.GetButtonDown(InputManager.Button.A))) && menuEntries.Length > 0 && lifetime > MIN_SELECT_TIME)
        {
            SelectDisabled();
        }
        if (InputManager.GetButtonDown(InputManager.Button.B) && lifetime > MIN_SELECT_TIME) //Press B to go back
        {
            Cancel();
        }
        if (InputManager.GetButtonDown(InputManager.Button.Z) && lifetime > MIN_SELECT_TIME)
        {
            ZOption();
        }
    }

    public override void Clear()
    {
        active = false;

        Destroy(menuBaseO);
        Destroy(descriptionBoxO);
        for (int i = 0; i < menuEntriesO.Count; i++)
        {
            Destroy(menuEntriesO[i]);
        }
    }

    //public override void Init()
    /*
    {
        active = true;
        menuIndex = 0;

        menuBaseO = Instantiate(menuBase, MainManager.Instance.Canvas.transform);
        //menuBaseO.transform.position = new Vector3(250, -70, 0);
        menuEntriesO = new List<GameObject>();

        BoxMenuScript bm = menuBaseO.GetComponent<BoxMenuScript>();

        //Create all menu entries somehow
        //For reference, here is the code for the move menu
        menuEntries = new BMenuEntry[caller.moveset.Count];
        for (int i = 0; i < caller.moveset.Count; i++)
        {
            menuEntries[i] = new MoveMenuEntry(caller, (BattleMove)caller.moveset[i]);
            GameObject g = Instantiate(menuEntryBase, bm.mask.transform);
            g.transform.localPosition = BoxMenuScript.GetRelativePosition(i);
            menuEntriesO.Add(g);
            BMenuEntryScript b = menuEntriesO[i].GetComponent<BMenuEntryScript>();
            b.Setup(menuEntries[i]);
        }

        bm.upArrow.enabled = false; //menuTopIndex > 0;
        bm.downArrow.enabled = menuTopIndex < menuEntries.Length - 6 && menuEntries.Length > 6;
    }
    */

    public virtual BaseBattleMenu.BaseMenuName GetMenuName()
    {
        return BaseBattleMenu.BaseMenuName.None;
    }
    public virtual void ZOption()
    {

    }
    public virtual void Cancel()
    {
        MainManager.Instance.PlayGlobalSound(MainManager.Sound.Menu_Cancel);
        PopSelf();
    }
    public virtual void SelectOption()
    {

    }
    public virtual void SelectDisabled()
    {

    }
    public virtual void OnHover(BoxMenuEntry boxMenuEntry)
    {

    }


    public virtual Vector3 GetSelectorPosition(Vector3 position)
    {
        return Vector3.left * 140f + position + Vector3.up * 22.5f;
    }
    public static Vector2 GetMenuAnchoredPosition()
    {
        return Vector2.right * 200f + Vector2.down * 260f;
    }

    /*
    {
        //Selecting a move takes you to a selection menu
        switch (menuName)
        {
            case BaseBattleMenu.BaseMenuName.Move:
                SelectionMenu s = SelectionMenu.buildMenu(caller, caller.moveset[menuIndex].GetBaseTarget());
                s.transform.SetParent(transform);
                PushState(s);
                break;
            case BaseBattleMenu.BaseMenuName.Items:
                List<Item> inv = MainManager.Instance.playerData.itemInventory;
                SelectionMenu s2 = SelectionMenu.buildMenu(caller, Item.GetTarget(inv[menuIndex].itemType));
                s2.transform.SetParent(transform);
                PushState(s2);
                break;
            case BaseBattleMenu.BaseMenuName.Tactics:
                List<BattleAction> tactics = GetTactics();
                SelectionMenu s3 = SelectionMenu.buildMenu(caller, tactics[menuIndex].GetBaseTarget());
                s3.transform.SetParent(transform);
                PushState(s3);
                break;
        }
    }
    */

    //Move to BattleControl
    /*
    public List<BattleAction> GetTactics()
    {
        List<BattleAction> tactics = BattleControl.Instance.tactics;
        PlayerEntity t = (PlayerEntity)caller;
        if (t != null)
        {
            tactics = t.tactics;
        }
        return tactics;
    }
    */

    public virtual void IncrementLevel(int inc)
    {
        if (menuEntries[menuIndex].maxLevel <= 1)
        {
            return;
        }

        menuEntries[menuIndex].level += inc;
        if (menuEntries[menuIndex].level <= 0)
        {
            menuEntries[menuIndex].level += menuEntries[menuIndex].maxLevel;
        }
        menuEntries[menuIndex].level = ((menuEntries[menuIndex].level - 1) % menuEntries[menuIndex].maxLevel) + 1;

        //rebuild the entry I guess
        /*
        menuEntries[menuIndex] = new MoveMenuEntry(caller, (PlayerMove)caller.moveset[menuIndex], displayMode);
        BoxMenuEntryScript b = menuEntriesO[menuIndex].GetComponent<BoxMenuEntryScript>();
        b.Setup(menuEntries[menuIndex]);
        */
    }

    public void InvokeExit(object sender, MenuExitEventArgs meea)
    {
        menuExit?.Invoke(this, new MenuExitEventArgs(GetFullResult()));
    }
    public override MenuResult GetResult()
    {
        return new MenuResult(null);
    }

    /*
    public virtual Move GetCurrent()
    {
        return null;
    }
    public virtual BattleAction GetAction()
    {
        return null;
    }
    */
}
