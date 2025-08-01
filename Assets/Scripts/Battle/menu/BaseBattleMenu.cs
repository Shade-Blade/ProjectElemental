using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseBattleMenu : MenuHandler
{
    public enum BaseMenuName
    {
        None = -1,
        Jump,
        Weapon,
        Items,
        Tactics,
        Soul,

        BadgeSwap,
        RibbonSwap,

        TurnRelay,

        CharacterMenu,  //overworld only
        MetaItems,      //special handling thing
    }
    public List<BaseMenuName> menuOptions;
    public List<BaseMenuOption> baseMenuOptions;
    public int baseMenuIndex = -1;
    float inputDir;

    float angle = 0;
    float anglediffer = 0;

    int weaponLevel = 0;

    //borrow the name box stuff to make stuff more consistent
    GameObject nameBox;
    NameBoxScript nameBoxScript;

    List<GameObject> baseMenuOptionsO;
    List<BaseOptionSpriteScript> baseMenuOptionsS;
    List<RectTransform> baseMenuRectTransforms;
    TextDisplayer baseMenuDescriptor;
    TextDisplayer baseMenuBSwap;

    float xCoord;

    public float appearScale;

    PlayerEntity caller;

    public override event EventHandler<MenuExitEventArgs> menuExit;

    public static BaseBattleMenu buildMenu(PlayerEntity caller)
    {
        //Generate a menu based on caller
        GameObject newObj = new GameObject("Base Battle Menu");
        BaseBattleMenu newMenu = newObj.AddComponent<BaseBattleMenu>();
        newMenu.caller = caller;
        newMenu.weaponLevel = caller.GetWeaponLevel();
        newMenu.Init();

        return newMenu;
    }

    public static string MenuNameToString(BaseMenuName bmn, PlayerEntity caller = null)
    {
        switch (bmn)
        {
            case BaseMenuName.Jump:
                return "Stomp";
            case BaseMenuName.Weapon:
                if (caller == null)
                {
                    return "Weapon";
                }
                if (caller.entityID == BattleHelper.EntityID.Wilex)
                {
                    return "Sword";
                }
                if (caller.entityID == BattleHelper.EntityID.Luna)
                {
                    return "Hammer";
                }
                break;
            case BaseMenuName.Items:
                return "Items";
            case BaseMenuName.Soul:
                return "Soul Moves";
            case BaseMenuName.Tactics:
                return "Tactics";
            case BaseMenuName.BadgeSwap:
                //A bad idea? Probably not
                return "Change Badges (" + (BattleControl.Instance.playerData.BadgeEquippedCount(Badge.BadgeType.BadgeSwap) * 4 - BattleControl.Instance.badgeSwapUses) + ")";
            case BaseMenuName.RibbonSwap:
                return "Change Ribbons";
            case BaseMenuName.TurnRelay:
                int staminaCost = Mathf.CeilToInt(caller.GetBoostedAgility() / 2f);
                return "Turn Relay (" + staminaCost + "<stamina>)";
        }
        return "???";
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
        float lastAppearScale = appearScale;
        appearScale = MainManager.EasingQuadraticTime(appearScale, 1, 100);

        //BattleControl.Instance.ShowHPBars();
        lifetime += Time.deltaTime;
        if ((lifetime > BoxMenu.MIN_SELECT_TIME && Mathf.Sign(InputManager.GetAxisHorizontal()) != inputDir) || InputManager.GetAxisHorizontal() == 0)
        {
            inputDir = Mathf.Sign(InputManager.GetAxisHorizontal());
            if (InputManager.GetAxisHorizontal() == 0)
            {
                inputDir = 0;
            }
            //Debug.Log(InputManager.GetAxisHorizontal());
            //now go
            if (inputDir != 0)
            {
                if (inputDir > 0)
                {
                    MainManager.Instance.PlayGlobalSound(MainManager.Sound.Menu_ScrollRight);
                    baseMenuIndex++;
                }
                else
                {
                    MainManager.Instance.PlayGlobalSound(MainManager.Sound.Menu_ScrollLeft);
                    baseMenuIndex--;
                }
            }

            if (baseMenuIndex > baseMenuOptions.Count - 1)
            {
                baseMenuIndex = 0;
            }
            if (baseMenuIndex < 0)
            {
                baseMenuIndex = baseMenuOptions.Count - 1;
            }

            baseMenuDescriptor.SetText(MenuNameToString(baseMenuOptions[baseMenuIndex].oname, caller), true, true);

            if (!caller.HasEffect(Effect.EffectType.Dizzy))
            {
                if (baseMenuOptions[baseMenuIndex].oname == BaseMenuName.Weapon)
                {
                    caller.SetAnimation("idleweapon");
                }
                else
                {
                    caller.SetAnimation("idlethinking");
                }
            }

            if (inputDir != 0)
            {
                float pastangle = angle;
                angle = baseMenuIndex * (360f / baseMenuOptions.Count);
                anglediffer -= (pastangle - angle);

                if (anglediffer > 180)
                {
                    anglediffer -= 360;
                }

                if (anglediffer < -180)
                {
                    anglediffer += 360;
                }
            }
        }

        if (Mathf.Abs(anglediffer) > 0 || lastAppearScale != appearScale)
        {
            if (Mathf.Abs(anglediffer) < 5)
            {
                anglediffer = 0;
            } else
            {
                anglediffer *= Mathf.Pow(0.05f, 5 * Time.deltaTime);
            }

            float tempangle;

            for (int i = 0; i < baseMenuOptions.Count; i++)
            {
                tempangle = (i) * (360f / baseMenuOptions.Count) - angle + anglediffer;
                if (tempangle < 0)
                {
                    tempangle += 360f;
                }

                baseMenuRectTransforms[i].anchoredPosition = Vector3.right * (-(MainManager.Instance.Canvas.GetComponent<RectTransform>().rect.width / 2) + xCoord) + appearScale * (Vector3.up * 60 + offsetFromAngle(tempangle) + Vector3.up * 10);
                baseMenuOptionsO[i].transform.localScale = appearScale * scaleFromAngle(tempangle);
                //Debug.Log(weaponLevel);
                baseMenuOptionsS[i].Setup(menuOptions[i], baseMenuOptions[i].canSelect, caller.entityID == BattleHelper.EntityID.Wilex, weaponLevel);
                baseMenuOptionsS[i].SetOpacity(opacityFromAngle(tempangle));
            }

            Sort();
        }


        if (lifetime > MIN_SELECT_TIME && ((InputManager.GetButtonDown(InputManager.Button.Start) || InputManager.GetButtonDown(InputManager.Button.A)))) //Press A to select stuff
        {
            if (baseMenuOptions[baseMenuIndex].canSelect)
            {
                //MainManager.Instance.PlayGlobalSound(MainManager.Sound.Menu_Select);
                MainManager.Instance.PlayGlobalSound(MainManager.Sound.Menu_Open);
                /*
                if (baseMenuOptions[baseMenuIndex].oname == BaseMenuName.Jump)
                {
                    BattleSelectionMenu s = BattleSelectionMenu.buildMenu(caller, caller.moveset[0].GetBaseTarget(), BaseMenuName.Jump, caller.moveset[0].GetActionCommandDesc());
                    s.transform.SetParent(transform);
                    PushState(s);
                    s.menuExit += InvokeExit;
                } 
                */

                if (baseMenuOptions[baseMenuIndex].oname == BaseMenuName.TurnRelay)
                {
                    //selection menu
                    BattleSelectionMenu s;
                    s = BattleSelectionMenu.buildMenu(caller, BattleControl.Instance.turnRelay.GetBaseTarget(), BaseMenuName.TurnRelay, BattleControl.Instance.turnRelay, BattleControl.Instance.turnRelay.GetActionCommandDesc(), 1);
                    s.transform.SetParent(transform);
                    PushState(s);
                    s.menuExit += InvokeExit;
                } else if (baseMenuOptions[baseMenuIndex].oname == BaseMenuName.Items)
                {
                    if (MetaItemBoxMenu.GetAvailableMoves(BattleControl.Instance.playerData).Count > 1)
                    {
                        MenuHandler b = BoxMenu.BuildMenu(caller, BaseMenuName.MetaItems); //BoxMenu.BuildMetaItemMenu(caller);
                        b.transform.parent = transform;
                        PushState(b);
                        b.menuExit += InvokeExit;
                    }
                    else
                    {
                        MenuHandler b = baseMenuOptions[baseMenuIndex].CreateMenuHandler(caller);
                        b.transform.parent = transform;
                        PushState(b);
                        b.menuExit += InvokeExit;
                    }
                } else
                {
                    MenuHandler b = baseMenuOptions[baseMenuIndex].CreateMenuHandler(caller);
                    b.transform.parent = transform;
                    PushState(b);
                    b.menuExit += InvokeExit;
                }
            }
            else
            {
                MainManager.Instance.PlayGlobalSound(MainManager.Sound.Menu_Error);
                //Debug.Log("Can't select " + baseMenuOptions[baseMenuIndex].oname);
                BattlePopup popup = null;

                popup = new BattlePopup(baseMenuOptions[baseMenuIndex].cantMoveReason);

                BattlePopupMenuScript s = BattlePopupMenuScript.BuildMenu(popup.text);
                s.transform.SetParent(transform);
                PushState(s);
                s.menuExit += InvokeExit;
            }
        }
        if (lifetime > MIN_SELECT_TIME && InputManager.GetButtonDown(InputManager.Button.B)) //Press B to switch active characters
        {
            MainManager.Instance.PlayGlobalSound(MainManager.Sound.Menu_BSwap);
            if (BUsable())
            {
                menuExit?.Invoke(this, new MenuExitEventArgs(GetFullResult()));
                PlayerTurnController.Instance.ExitMenu(PlayerTurnController.MenuExitType.Switch);
            }
        }
        if (lifetime > MIN_SELECT_TIME && InputManager.GetButtonDown(InputManager.Button.Z)) //Press Z to switch character positions
        {
            MainManager.Instance.PlayGlobalSound(MainManager.Sound.Menu_CharacterSwap);
            if (ZUsable())
            {
                menuExit?.Invoke(this, new MenuExitEventArgs(GetFullResult()));
                PlayerTurnController.Instance.ExitMenu(PlayerTurnController.MenuExitType.SuperSwap);
            }
        }
    }

    public static bool BUsable()
    {
        return PlayerTurnController.Instance.movableParty.Count > 1;
    }
    public static bool ZUsable()
    {
        return (MainManager.Instance.GetGlobalFlag(MainManager.GlobalFlag.GF_FileCode_Wrath) || PlayerTurnController.Instance.MaxMovable()) && (PlayerTurnController.Instance.movableParty.Count + PlayerTurnController.Instance.immovableParty.Count > 1);
    }

    public void Sort()
    {
        //very inefficient and overcomplicated code coming up
        //but it isn't that bad with 6 elements only lol
        //sorting
        List<RectTransform> objects = new List<RectTransform>();
        for (int i = 0; i < baseMenuOptions.Count; i++)
        {
            objects.Add(baseMenuRectTransforms[i]);
        }

        int count = 0;
        float lowestY = baseMenuRectTransforms[0].anchoredPosition.y;
        List<int> sortList = new List<int>();
        for (int i = 0; i < baseMenuRectTransforms.Count; i++)
        {
            count = 0;
            for (int j = 0; j < baseMenuRectTransforms.Count; j++)
            {
                if (baseMenuRectTransforms[j].anchoredPosition.y > baseMenuRectTransforms[i].anchoredPosition.y)
                {
                    count++;
                }
            }
            sortList.Add(count);
        }

        int lowest = 0;
        int highestIndex = 0;
        for (int i = 0; i < baseMenuOptionsO.Count; i++)
        {
            if (sortList[i] > highestIndex)
            {
                highestIndex = sortList[i];
            }
        }

        while (lowest <= highestIndex)
        {
            for (int i = 0; i < baseMenuOptionsO.Count; i++)
            {
                if (sortList[i] == lowest)
                {
                    baseMenuOptionsO[i].transform.SetAsLastSibling();
                }
            }

            lowest++;
        }
    }

    public override void Init()
    {
        appearScale = 0;

        //BattleControl.Instance.ShowEffectIcons();
        //BattleControl.Instance.ShowHPBars();

        xCoord = MainManager.Instance.WorldPosToCanvasPos(caller.transform.position)[0];

        menuOptions = new List<BaseMenuName>();
        if (caller.jumpMoves.Count > 0)
        {
            menuOptions.Add(BaseMenuName.Jump);
        }
        if (caller.weaponMoves.Count > 0)
        {
            menuOptions.Add(BaseMenuName.Weapon);
        }
        if (BattleControl.Instance.playerData.itemCounter > 0 || BattleControl.Instance.playerData.itemInventory.Count > 0)  //i.e. if you have never collected a single item, don't show the item menu
        {
            menuOptions.Add(BaseMenuName.Items);
        }
        if (caller.soulMoves.Count > 0)
        {
            menuOptions.Add(BaseMenuName.Soul);
        }
        if (BattleControl.Instance.GetPlayerEntities().Count > 1)
        {
            menuOptions.Add(BaseMenuName.TurnRelay);
        }
        menuOptions.Add(BaseMenuName.Tactics);

        if (BattleControl.Instance.playerData.BadgeEquipped(Badge.BadgeType.BadgeSwap) || MainManager.Instance.Cheat_BadgeSwap)
        {
            menuOptions.Add(BaseMenuName.BadgeSwap);
        }
        if (BattleControl.Instance.playerData.BadgeEquipped(Badge.BadgeType.RibbonSwap) || MainManager.Instance.Cheat_RibbonSwap)
        {
            menuOptions.Add(BaseMenuName.RibbonSwap);
        }

        baseMenuOptions = new List<BaseMenuOption>();
        baseMenuOptionsO = new List<GameObject>();
        baseMenuOptionsS = new List<BaseOptionSpriteScript>();
        baseMenuRectTransforms = new List<RectTransform>();

        if (baseMenuIndex == -1)
        {
            baseMenuIndex = 0;
        }
        inputDir = 0;
        base.Init();

        //the "front" angle
        angle = baseMenuIndex * (360f / menuOptions.Count);

        BaseMenuOption temp;

        foreach (BaseMenuName bmn in menuOptions)
        {
            temp = null;
            switch (bmn)
            {
                case BaseMenuName.Jump:
                    temp = new BaseMenuOption(BaseMenuName.Jump);
                    temp.canSelect = caller.jumpMoves.Count > 0;
                    if (temp.canSelect)
                    {
                        temp.canSelect = false;
                        foreach (PlayerMove m in caller.jumpMoves)
                        {
                            temp.canSelect |= m.CanChoose(caller);
                            if (temp.canSelect)
                            {
                                break;
                            }
                        }
                    }

                    temp.canSelect &= !(caller.BadgeEquipped(Badge.BadgeType.MetalPower));
                    temp.cantMoveReason = PlayerMove.CantMoveReason.BlockJump;
                    break;
                case BaseMenuName.Weapon:
                    temp = new BaseMenuOption(BaseMenuName.Weapon);
                    temp.canSelect = caller.weaponMoves.Count > 0;
                    if (temp.canSelect)
                    {
                        temp.canSelect = false;
                        foreach (PlayerMove m in caller.weaponMoves)
                        {
                            temp.canSelect |= m.CanChoose(caller);
                            if (temp.canSelect)
                            {
                                break;
                            }
                        }
                    }

                    temp.canSelect &= !(caller.BadgeEquipped(Badge.BadgeType.SoftPower));
                    temp.cantMoveReason = PlayerMove.CantMoveReason.BlockWeapon;
                    break;
                case BaseMenuName.Items:
                    temp = new BaseMenuOption(BaseMenuName.Items);
                    temp.canSelect = BattleControl.Instance.playerData.itemInventory.Count > 0;
                    if (temp.canSelect)
                    {
                        for (int i = 0; i < BattleControl.Instance.playerData.itemInventory.Count; i++)
                        {
                            //warning: may need to get rid of GetItemMoveScript or make it better
                            ItemMove imove = Item.GetItemMoveScript(BattleControl.Instance.playerData.itemInventory[i]);
                            if (imove != null)
                            {
                                temp.canSelect |= imove.CanChoose(caller);
                            }
                        }
                    }

                    bool normalCanSelect = temp.canSelect;

                    temp.canSelect &= !caller.HasEffect(Effect.EffectType.Sticky);


                    //sidenote: no need to check double bite, quick bite etc because normal item use overshadows them (if you can't use items normally you can't use them either)
                    temp.canSelect &= !(caller.BadgeEquipped(Badge.BadgeType.VoraciousEater));
                    temp.canSelect &= !(caller.QuickSupply > 0);    //Block you from using 2 items on the same turn using quick supply

                    if (!temp.canSelect)
                    {
                        if (normalCanSelect)
                        {
                            temp.cantMoveReason = PlayerMove.CantMoveReason.BlockItems;
                        }
                        else
                        {
                            temp.cantMoveReason = PlayerMove.CantMoveReason.NoItems;
                        }
                    }

                    break;
                case BaseMenuName.Soul:
                    temp = new BaseMenuOption(BaseMenuName.Soul);
                    temp.canSelect = caller.soulMoves.Count > 0;
                    if (temp.canSelect)
                    {
                        temp.canSelect = false;
                        foreach (SoulMove m in caller.soulMoves)
                        {
                            temp.canSelect |= m.CanChoose(caller);
                            if (temp.canSelect)
                            {
                                break;
                            }
                        }
                    }
                    temp.cantMoveReason = PlayerMove.CantMoveReason.BlockSoul;
                    break;
                case BaseMenuName.Tactics:
                    //you can always select the tactics menu, since you can always select "Rest"
                    temp = new BaseMenuOption(BaseMenuName.Tactics);
                    break;
                case BaseMenuName.BadgeSwap:
                    //logically impossible to not be able to use this?
                    //Badge swap is a badge (but cheats can let you use badgeswap without actually having the badge)
                    //but not every badge can be badge swapped (*but there's only like 4 of them)
                    temp = new BaseMenuOption(BaseMenuName.BadgeSwap);
                    temp.canSelect = BattleControl.Instance.playerData.badgeInventory.Count > 0;

                    if (!MainManager.Instance.Cheat_BadgeSwap && (BattleControl.Instance.playerData.BadgeEquippedCount(Badge.BadgeType.BadgeSwap) * 4 - BattleControl.Instance.badgeSwapUses <= 0))
                    {
                        temp.canSelect = false;
                    }

                    temp.cantMoveReason = PlayerMove.CantMoveReason.BadgeSwapExpended;

                    break;
                case BaseMenuName.RibbonSwap:
                    //basically impossible to have ribbon swap but no ribbons?
                    temp = new BaseMenuOption(BaseMenuName.RibbonSwap);
                    temp.canSelect = BattleControl.Instance.playerData.ribbonInventory.Count > 0;
                    break;
                case BaseMenuName.TurnRelay:
                    temp = new BaseMenuOption(BaseMenuName.TurnRelay);
                    temp.canSelect = BattleControl.Instance.turnRelay.CanChoose(caller);
                    temp.cantMoveReason = BattleControl.Instance.turnRelay.GetCantMoveReason(caller);
                    break;
            }
            baseMenuOptions.Add(temp);
        }


        GameObject o;

        float tempangle;

        for (int i = 0; i < baseMenuOptions.Count; i++)
        {
            o = Instantiate(BattleControl.Instance.baseMenuOption, MainManager.Instance.Canvas.transform);
            baseMenuOptionsO.Add(o);
            baseMenuRectTransforms.Add(o.GetComponent<RectTransform>());
            baseMenuOptionsS.Add(o.GetComponent<BaseOptionSpriteScript>());

            tempangle = (i) * (360f / baseMenuOptions.Count) - angle;
            if (tempangle < 0)
            {
                tempangle += 360f;
            }

            baseMenuRectTransforms[i].anchoredPosition = Vector3.right * (-(MainManager.Instance.Canvas.GetComponent<RectTransform>().rect.width / 2) + xCoord) + appearScale * (Vector3.up * 60 + offsetFromAngle(tempangle) + Vector3.up * 10);
            baseMenuOptionsO[i].transform.localScale = appearScale * scaleFromAngle(tempangle);
            //Debug.Log(weaponLevel);
            baseMenuOptionsS[i].Setup(menuOptions[i], baseMenuOptions[i].canSelect, caller.entityID == BattleHelper.EntityID.Wilex, weaponLevel);
        }

        //descriptor
        o = Instantiate(BattleControl.Instance.baseMenuDescriptor, MainManager.Instance.Canvas.transform);
        baseMenuDescriptor = o.GetComponent<TextDisplayer>();
        baseMenuDescriptor.SetText(MenuNameToString(baseMenuOptions[baseMenuIndex].oname, caller), true, true);
        baseMenuDescriptor.gameObject.GetComponent<RectTransform>().anchoredPosition = Vector3.right * (-(MainManager.Instance.Canvas.GetComponent<RectTransform>().rect.width / 2) + xCoord) + Vector3.up * -40;

        o = Instantiate(BattleControl.Instance.baseMenuBSwap, MainManager.Instance.Canvas.transform);
        baseMenuBSwap = o.GetComponent<TextDisplayer>();
        string btext = "";
        if (BUsable())
        {
            //which way is the other character?
            if (caller.posId < -1)
            {
                btext += "<button,b><rarrow>";
            } else
            {
                btext += "<larrow><button,b>";
            }
        }
        if (ZUsable())
        {
            if (btext.Length > 0)
            {
                btext += ", ";
            }
            btext += "<carrow><button,z>";
        }
        baseMenuBSwap.SetText(btext, true, true);

        baseMenuBSwap.gameObject.GetComponent<RectTransform>().anchoredPosition = Vector3.right * (-(MainManager.Instance.Canvas.GetComponent<RectTransform>().rect.width / 2) + xCoord) + Vector3.down * 225;

        Sort();
        Canvas.ForceUpdateCanvases();


        //failsafe check
        bool anyUsable = false;
        for (int i = 0; i < baseMenuOptions.Count; i++)
        {
            if (baseMenuOptions[i].canSelect)
            {
                anyUsable = true;
            }
        }

        if (!anyUsable)
        {
            menuExit?.Invoke(this, new MenuExitEventArgs(GetFullResult()));
            PlayerTurnController.Instance.ExitMenu(PlayerTurnController.MenuExitType.Null);
        }
    }
    public override void Clear()
    {
        //BattleControl.Instance.HideEffectIcons();
        //BattleControl.Instance.HideHPBars();

        for (int i = 0; i < baseMenuOptionsO.Count; i++)
        {
            Destroy(baseMenuOptionsO[i]);
        }

        if (baseMenuDescriptor != null)
        {
            Destroy(baseMenuDescriptor.gameObject);
        }
        if (baseMenuBSwap != null)
        {
            Destroy(baseMenuBSwap.gameObject);
        }

        active = false;
        //Destroy(gameObject);
    }
    public virtual void Hide()
    {
        for (int i = 0; i < baseMenuOptionsO.Count; i++)
        {
            baseMenuOptionsO[i].SetActive(true);
        }

        if (baseMenuDescriptor != null)
        {
            Destroy(baseMenuDescriptor.gameObject);
        }
        if (baseMenuBSwap != null)
        {
            Destroy(baseMenuBSwap.gameObject);
        }
    }
    public virtual void Unhide()
    {
        for (int i = 0; i < baseMenuOptionsO.Count; i++)
        {
            Destroy(baseMenuOptionsO[i]);
        }

        if (baseMenuDescriptor != null)
        {
            Destroy(baseMenuDescriptor.gameObject);
        }
        if (baseMenuBSwap != null)
        {
            Destroy(baseMenuBSwap.gameObject);
        }
    }

    public int IndexFromOffset(int i)
    {
        int a = baseMenuIndex + i;
        while (a < 0)
        {
            a += baseMenuOptions.Count;
        }
        a %= baseMenuOptions.Count;

        return a;
    }

    //0 = down, then + is cw
    public Vector3 scaleFromAngle(float angle)
    {
        float max = 0.8f;
        float min = 0.4f;
        float val = (max + min) / 2 + ((max - min) / 2) * Mathf.Cos(angle * (Mathf.PI / 180));

        return new Vector3(val, val, 1);
    }

    public float opacityFromAngle(float angle)
    {
        float max = 1.2f;
        float min = 0.5f;
        float val = (max + min) / 2 + ((max - min) / 2) * Mathf.Cos(angle * (Mathf.PI / 180));

        return val;
    }

    public Vector3 offsetFromAngle(float angle)
    {
        float width = 200;
        float height =100;

        Vector3 output = new Vector3(1, 1, 1);
        output.y = (height/2) * -Mathf.Cos(angle * (Mathf.PI / 180));
        output.x = (width / 2) * Mathf.Sin(angle * (Mathf.PI / 180));

        return output;
    }

    public void InvokeExit(object sender, MenuExitEventArgs meea)
    {
        menuExit?.Invoke(this, new MenuExitEventArgs(GetFullResult()));
    }
    public override MenuResult GetResult()
    {
        return new MenuResult(baseMenuOptions[baseMenuIndex].oname);
    }
}

public class BaseMenuOption //: MonoBehaviour
{
    public BaseBattleMenu.BaseMenuName oname;
    public bool canSelect = true;
    public PlayerMove.CantMoveReason cantMoveReason;

    public BaseMenuOption(BaseBattleMenu.BaseMenuName p_name, bool p_canSelect = true)
    {
        oname = p_name;
        canSelect = p_canSelect;
        //SetBattleMenuHandler(b);
    }
    public MenuHandler CreateMenuHandler(PlayerEntity caller)
    {
        return BoxMenu.BuildMenu(caller, oname);
    }
}


