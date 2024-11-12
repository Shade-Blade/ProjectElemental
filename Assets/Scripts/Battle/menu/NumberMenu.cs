using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NumberMenu : MenuHandler
{
    int max;
    int currentNum;
    int min;

    float holdDuration;
    int holdValue;
    float inputDir;

    bool cancel;

    public const float HYPER_SCROLL_TIME = 0.3f;

    public GameObject baseObject;
    public NumberMenuScript nms;


    public override event EventHandler<MenuExitEventArgs> menuExit;

    public static NumberMenu BuildMenu(int max, int currentNum, int min = 0)
    {
        GameObject newObj = new GameObject("Number Menu");
        NumberMenu newMenu = newObj.AddComponent<NumberMenu>(); 
        newMenu.max = max;
        newMenu.currentNum = currentNum;
        newMenu.min = min;
        newMenu.Init();
        return newMenu;
    }

    public override void Init()
    {
        base.Init();
        cancel = false;

        baseObject = Instantiate(MainManager.Instance.numberMenu, MainManager.Instance.Canvas.transform);
        nms = baseObject.GetComponent<NumberMenuScript>();

        nms.text.SetText(currentNum + "", true, true);

        nms.upArrow.enabled = (currentNum < max);
        nms.downArrow.enabled = (currentNum > min);
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
    public void MenuUpdate()
    {
        if (nms == null)
        {
            nms = baseObject.GetComponent<NumberMenuScript>();
        }

        lifetime += Time.deltaTime;
        if ((lifetime > MIN_SELECT_TIME && Mathf.Sign(InputManager.GetAxisVertical()) != inputDir) || InputManager.GetAxisVertical() == 0)
        {
            holdDuration = 0;
            holdValue = 0;
            inputDir = Mathf.Sign(InputManager.GetAxisVertical());
            if (InputManager.GetAxisVertical() == 0)
            {
                inputDir = 0;
            }
            if (inputDir != 0)
            {
                //inputDir positive = up and - index, negative = down and + index
                if (inputDir > 0)
                {
                    currentNum++;
                }
                else
                {
                    currentNum--;
                }
            }

            //No loop around
            if (currentNum > max)
            {
                currentNum = max;
            }
            if (currentNum < min)
            {
                currentNum = min;
            }

            nms.text.SetText(currentNum + "", true, true);
            nms.upArrow.enabled = (currentNum < max);
            nms.downArrow.enabled = (currentNum > min);
        }
        if ((lifetime > MIN_SELECT_TIME && Mathf.Sign(InputManager.GetAxisVertical()) == inputDir) && InputManager.GetAxisVertical() != 0)
        {
            holdDuration += Time.deltaTime;

            if (holdDuration >= HYPER_SCROLL_TIME)
            {
                int pastHoldValue = holdValue;

                //note: hardcoded 30 so you scroll 30 per second
                //should be fast enough to be useful in big menus but also slow enough to stop on a specific number relatively easily (or get close easily)
                if (holdDuration >= HYPER_SCROLL_TIME * 3)
                {
                    //Number needs to be continuous
                    //lower formula ends at (HST * 2)
                    //higher formula is (10 * hd) - K
                    //  (R * 3 * HST) - K = 2 * HST
                    //  3R HST - K = 2 * HST
                    //  K = (3R - 2) HST
                    if (MainManager.Instance.GetHyperScrollRate() * ((6 * holdDuration) - 16 * HYPER_SCROLL_TIME) > holdValue)
                    {
                        holdValue = (int)(MainManager.Instance.GetHyperScrollRate() * ((6 * holdDuration) - 16 * HYPER_SCROLL_TIME));
                    }
                }
                else
                {
                    if (MainManager.Instance.GetHyperScrollRate() * (holdDuration - HYPER_SCROLL_TIME) > holdValue)
                    {
                        holdValue = (int)(MainManager.Instance.GetHyperScrollRate() * (holdDuration - HYPER_SCROLL_TIME));
                    }
                }

                if (inputDir > 0)
                {
                    currentNum += (holdValue - pastHoldValue);
                }
                else
                {
                    currentNum -= (holdValue - pastHoldValue);
                }

                //No loop around
                if (currentNum > max)
                {
                    currentNum = max;
                }
                if (currentNum < min)
                {
                    currentNum = min;
                }

                nms.text.SetText(currentNum + "", true, true);
                nms.upArrow.enabled = (currentNum < max);
                nms.downArrow.enabled = (currentNum > min);
            }
        }

        if (lifetime > MIN_SELECT_TIME && InputManager.GetButton(InputManager.Button.A)) //Press A to select stuff
        {
            SelectOption();
        }
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
        if (cancel)
        {
            return new MenuResult(int.MinValue);
        }
        return new MenuResult(currentNum);
    }
}
