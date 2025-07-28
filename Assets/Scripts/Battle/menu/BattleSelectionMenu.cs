using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ProBuilder;

//encompasses all three selection menu outcomes
public class BattleSelectionMenu : MenuHandler
{
    BaseBattleMenu.BaseMenuName parentMenuName;

    GameObject nameBox;
    NameBoxScript nameBoxScript;

    List<SelectPointerScript> pointers;
    List<BattleEntity> possibleEntities;
    TargetArea targetArea;
    BattleEntity caller;
    IEntityHighlighter eh;  //required to access the per-entity text thing
    SelectPointerScript spsBase;

    int level;

    //maybe I should not create them if not needed
    protected GameObject descriptionBoxO;
    protected DescriptionBoxScript descriptionBoxScript;
    protected string ACDesc;

    float inputDir;
    bool diff = false;

    int selectionIndex = 0;
    //public float lifetime = 0;

    public override event EventHandler<MenuExitEventArgs> menuExit;

    public static BattleSelectionMenu buildMenu(BattleEntity caller, TargetArea targetArea, BaseBattleMenu.BaseMenuName menuName, IEntityHighlighter eh, string acdesc, int level = 1)
    {
        GameObject newObj = new GameObject("Selection Menu");
        BattleSelectionMenu newMenu = newObj.AddComponent<BattleSelectionMenu>();

        newMenu.ACDesc = (string)acdesc.Clone(); //(string)p_ACDesc.Clone();
        newMenu.eh = eh;
        newMenu.level = level;

        newMenu.caller = caller;
        newMenu.targetArea = targetArea;

        if (MainManager.Instance.Cheat_TargetAnarchy)
        {
            if (!targetArea.allPossible)
            {
                newMenu.targetArea = new TargetArea(TargetArea.TargetAreaType.Anyone);
            }
        }

        newMenu.parentMenuName = menuName;
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
        lifetime += Time.deltaTime;
        if (possibleEntities.Count == 0 && targetArea.range != TargetArea.TargetAreaType.None) //note that the no target range will skip the selection menu
        {
            //Debug.Log("No target");
        }

        //moving selection arrow around (can't do that if you are selecting all targets)
        if (!targetArea.allPossible && possibleEntities.Count > 1)
        {
            if ((lifetime > MIN_SELECT_TIME && Mathf.Sign(InputManager.GetAxisHorizontal()) != inputDir) || InputManager.GetAxisHorizontal() == 0)
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
                        selectionIndex++;
                        diff = true;
                    }
                    else
                    {
                        MainManager.Instance.PlayGlobalSound(MainManager.Sound.Menu_ScrollLeft);
                        selectionIndex--;
                        diff = true;
                    }
                }

                if (selectionIndex > possibleEntities.Count - 1)
                {
                    selectionIndex = 0;
                }
                if (selectionIndex < 0)
                {
                    selectionIndex = possibleEntities.Count - 1;
                }

                if (possibleEntities.Count > 0)
                {
                    pointers[0].targetPos = possibleEntities[selectionIndex].transform.position + possibleEntities[selectionIndex].selectionOffset + Vector3.up * (possibleEntities[selectionIndex].height + 0.5f);
                }

                if (diff)
                {
                    diff = false;
                    nameBoxScript.SetText(possibleEntities[selectionIndex].GetName());

                    //SelectPointerScript sps = pointers[0].GetComponent<SelectPointerScript>();
                    if (spsBase == null)
                    {
                        spsBase = pointers[0].GetComponent<SelectPointerScript>();
                    }
                    spsBase.SetText(eh.GetHighlightText(caller, possibleEntities[selectionIndex], level));
                    spsBase.RepositionTextBelow(MainManager.Instance.WorldPosToCanvasPosProportion(pointers[0].targetPos).y >= 0.8f);
                }

                /*
                for (int i = 0; i < possibleEntities.Count; i++)
                {

                }
                */

                /*
                if (inputDir != 0)
                {
                    //Debug.Log(possibleEntities[selectionIndex]);
                    string s = "";
                    for (int i = 0; i < possibleEntities.Count; i++)
                    {
                        if (i == selectionIndex)
                        {
                            s += "(";
                        }
                        s += possibleEntities[i].GetName();
                        if (i == selectionIndex)
                        {
                            s += ")";
                        }

                        if (i < possibleEntities.Count - 1)
                        {
                            s += " | ";
                        }
                    }
                    Debug.Log(s);
                }
                */
            }
        }

        //Note: This code basically makes it so that if the range is none, the selection menu appears for up to 1 frame before getting closed instantly
        //this also happens before the B button is checked so you can't somehow back out in that 1 frame window
        if (lifetime > MIN_SELECT_TIME && ((((InputManager.GetButtonDown(InputManager.Button.Start) || InputManager.GetButtonDown(InputManager.Button.A))) && possibleEntities.Count != 0) || targetArea.range == TargetArea.TargetAreaType.None)) //Time to move!
        {
            if (targetArea.range != TargetArea.TargetAreaType.None)
            {
                MainManager.Instance.PlayGlobalSound(MainManager.Sound.Menu_Select);
            }
            menuExit?.Invoke(this, new MenuExitEventArgs(GetFullResult()));
            switch (parentMenuName)
            {
                case BaseBattleMenu.BaseMenuName.Jump:
                case BaseBattleMenu.BaseMenuName.Weapon:
                case BaseBattleMenu.BaseMenuName.Soul:
                case BaseBattleMenu.BaseMenuName.Items: //note: meta items must pass through an item menu so no case in this list of cases
                    PlayerTurnController.Instance.ExitMenu(PlayerTurnController.MenuExitType.MoveExecute);
                    break;
                case BaseBattleMenu.BaseMenuName.Tactics:
                case BaseBattleMenu.BaseMenuName.BadgeSwap:
                case BaseBattleMenu.BaseMenuName.RibbonSwap:
                    //Debug.Log("e");
                    PlayerTurnController.Instance.ExitMenu(PlayerTurnController.MenuExitType.ActionExecute);
                    break;
                case BaseBattleMenu.BaseMenuName.TurnRelay:
                    PlayerTurnController.Instance.ExitMenu(PlayerTurnController.MenuExitType.TurnRelay);
                    break;
                //case BaseBattleMenu.BaseMenuName.SwitchCharacters:
                //    PlayerTurnController.Instance.ExitMenu(PlayerTurnController.MenuExitType.SwitchCharacter);
                //    break;
            }
        }
        if (lifetime > MIN_SELECT_TIME && InputManager.GetButtonDown(InputManager.Button.B)) //Press B to go back
        {
            MainManager.Instance.PlayGlobalSound(MainManager.Sound.Menu_Cancel);
            Debug.Log("Going back");
            PopSelf();
        }
    }

    public override void Init()
    {
        base.Init();

        selectionIndex = 0;

        descriptionBoxO = Instantiate(MainManager.Instance.descriptionBoxBase, MainManager.Instance.Canvas.transform);
        descriptionBoxO.GetComponent<RectTransform>().anchoredPosition = descriptionBoxO.GetComponent<RectTransform>().anchoredPosition[1] * Vector2.up;
        descriptionBoxScript = descriptionBoxO.GetComponent<DescriptionBoxScript>();
        descriptionBoxScript.SetText(ACDesc, true);
        //Debug.Log(ACDesc);

        possibleEntities = BattleControl.Instance.GetEntities(caller, targetArea);

        possibleEntities.Sort((a, b) => (MainManager.FloatCompare(a.transform.position.x, b.transform.position.x)));
        
        if (possibleEntities.Contains(caller))
        {
            //hardcode the user of the move being at the front of the list (so you select yourself by default)
            possibleEntities.Remove(caller);
            possibleEntities.Insert(0, caller);
        }

        if (nameBox == null)
        {
            if (possibleEntities.Count != 0 || targetArea.range != TargetArea.TargetAreaType.None)
            {
                nameBox = Instantiate(BattleControl.Instance.nameBoxBase, MainManager.Instance.Canvas.transform);
                nameBoxScript = nameBox.GetComponent<NameBoxScript>();
            }
        }
        if (pointers == null)
        {
            pointers = new List<SelectPointerScript>();
            if (possibleEntities.Count != 0)
            {
                if (targetArea.allPossible)
                {
                    nameBoxScript.SetText("All targets");
                    for (int i = 0; i < possibleEntities.Count; i++)
                    {
                        GameObject p = Instantiate(BattleControl.Instance.pointerBase, transform);
                        Vector3 startPos = possibleEntities[i].transform.position + possibleEntities[i].selectionOffset + Vector3.up * (possibleEntities[i].height + 0.5f);
                        p.transform.position = startPos + Vector3.up * 3.75f;
                        SelectPointerScript sps = p.GetComponent<SelectPointerScript>();
                        sps.SetText(eh.GetHighlightText(caller, possibleEntities[i], level));
                        sps.RepositionTextBelow(MainManager.Instance.WorldPosToCanvasPosProportion(startPos).y >= 0.8f);
                        sps.targetPos = startPos;
                        pointers.Add(sps);
                    }
                }
                else
                {
                    nameBoxScript.SetText(possibleEntities[selectionIndex].GetName());
                    GameObject p = Instantiate(BattleControl.Instance.pointerBase, transform);
                    Vector3 startPos = possibleEntities[selectionIndex].transform.position + possibleEntities[selectionIndex].selectionOffset + Vector3.up * (possibleEntities[selectionIndex].height + 0.5f);
                    p.transform.position = startPos + Vector3.up * 3.75f;
                    SelectPointerScript sps = p.GetComponent<SelectPointerScript>();
                    sps.SetText(eh.GetHighlightText(caller, possibleEntities[selectionIndex], level));
                    sps.RepositionTextBelow(MainManager.Instance.WorldPosToCanvasPosProportion(startPos).y >= 0.8f);
                    sps.targetPos = startPos;
                    pointers.Add(sps);
                }
            }
            else if (targetArea.range != TargetArea.TargetAreaType.None)
            {
                //set up a "no valid targets" message
                nameBoxScript.SetText("No valid targets");
            }
        }
    }
    public override void Clear()
    {
        active = false;
        if (nameBox != null)
        {
            Destroy(nameBox);
        }
        Destroy(descriptionBoxO);
        for (int i = 0; i < pointers.Count; i++)
        {
            Destroy(pointers[i]);
        }
    }

    public override MenuResult GetResult()
    {
        return new MenuResult(possibleEntities.Count == 0 ? null : possibleEntities[selectionIndex]);
    }

    public BattleEntity GetCurrent()
    {
        if (possibleEntities.Count == 0)
        {
            return null;
        }

        return possibleEntities[selectionIndex];
    }
}