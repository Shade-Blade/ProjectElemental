using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public class Pause_HandlerShared_SideTabs : Pause_HandlerShared
{
    public class UpdateObject
    {
        public int tabindex;

        public UpdateObject(int p_tabindex)
        {
            tabindex = p_tabindex;
        }
    }

    public int tabindex = -1;
    float inputDir;

    public override event EventHandler<MenuExitEventArgs> menuExit;

    float lrDir;

    public void Update()
    {
        if (active)
        {
            MenuUpdate();
        }
    }

    public virtual void MenuUpdate()
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
            int lastIndex = tabindex;
            if (inputDir != 0)
            {
                if (inputDir > 0)
                {
                    tabindex--;
                    MainManager.Instance.PlayGlobalSound(MainManager.Sound.Menu_ScrollUp);
                }
                else
                {
                    tabindex++;
                    MainManager.Instance.PlayGlobalSound(MainManager.Sound.Menu_ScrollDown);
                }
            }

            if (tabindex > GetMaxTabs())
            {
                tabindex = GetMaxTabs();
            }
            if (tabindex < 0)
            {
                tabindex = 0;
            }

            if (inputDir != 0)
            {
                IndexChange(lastIndex, tabindex);
                SendSectionUpdate();
            }
        }

        if (lifetime > MIN_SELECT_TIME && InputManager.GetButtonDown(InputManager.Button.A)) //Press A to select stuff
        {
            //go to submenu
            Select();
        }
        if (lifetime > MIN_SELECT_TIME && InputManager.GetButtonDown(InputManager.Button.B)) //Press B to go back
        {
            MainManager.Instance.PlayGlobalSound(MainManager.Sound.Menu_Close);
            SendNullUpdate();
            PopSelf();
        }
        if (lifetime > MIN_SELECT_TIME && InputManager.GetButtonDown(InputManager.Button.Z)) //Z
        {
            ZOption();
        }

        if ((lifetime > MIN_SELECT_TIME && Mathf.Sign(InputManager.GetAxisHorizontal()) != lrDir) || InputManager.GetAxisHorizontal() == 0)
        {
            lrDir = Mathf.Sign(InputManager.GetAxisHorizontal());

            if (InputManager.GetAxisHorizontal() == 0)
            {
                lrDir = 0;
            }

            if (lrDir != 0)
            {
                int indexB = GetLRIndex();

                if (lrDir > 0)
                {
                    indexB++;

                    ScrollRightSound();
                }
                else
                {
                    indexB--;

                    ScrollLeftSound();
                }

                if (indexB < 0)
                {
                    indexB = GetMaxLR();
                }
                if (indexB > GetMaxLR())
                {
                    indexB = 0;
                }

                LRChange(indexB);                
                //Debug.Log("Player " + selectedPlayer);
                SendSectionUpdate();
            }
        }
    }

    public virtual int GetMaxTabs()
    {
        return 0;
    }
    public virtual int GetLRIndex()
    {
        return 0;
    }
    public virtual int GetMaxLR()
    {
        return 0;
    }
    public virtual void ScrollRightSound()
    {
        if (GetMaxLR() > 0)
        {
            MainManager.Instance.PlayGlobalSound(MainManager.Sound.Menu_ScrollRight);
        }
    }
    public virtual void ScrollLeftSound()
    {
        if (GetMaxLR() > 0)
        {
            MainManager.Instance.PlayGlobalSound(MainManager.Sound.Menu_ScrollLeft);
        }
    }
    public virtual void IndexChange(int lastIndex, int tabIndex)
    {

    }
    public virtual void LRChange(int index)
    {

    }

    public virtual void SendSectionUpdate()
    {
        if (section != null)
        {
            section.ApplyUpdate(new UpdateObject(tabindex));
        }
    }
    public virtual void SendNullUpdate()
    {
        if (section != null)
        {
            section.ApplyUpdate(null);
        }
    }

    public virtual void Select()
    {
        Pause_SectionShared newSection = null;
        if (section != null)
        {
            newSection = section.GetSubsection(new UpdateObject(tabindex));
        }
        //build the menu
        MenuHandler b = null;

        //Stuff

        if (b == null)
        {
            Debug.Log("Select null");
            return;
        }

        b.transform.parent = transform;
        PushState(b);
        b.menuExit += InvokeExit;
    }
    public virtual void ZOption()
    {

    }


    public override void Init()
    {
        tabindex = 0;

        if (section != null)
        {
            UpdateObject uo = (UpdateObject)section.GetState();
            tabindex = uo.tabindex;
        }

        SendSectionUpdate();
        base.Init();
    }
    public void InvokeExit(object sender, MenuExitEventArgs meea)
    {
        menuExit?.Invoke(this, new MenuExitEventArgs(GetFullResult()));
    }

    public override MenuResult GetResult()
    {
        return new MenuResult(tabindex);
    }
}
