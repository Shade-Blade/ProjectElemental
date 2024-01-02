using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pause_HandlerShared_InfoBox : Pause_HandlerShared
{
    public class UpdateObject
    {
        public string[] infoText;
        public int index;

        public UpdateObject(int p_index, string[] p_infoText)
        {
            index = p_index;
            infoText = p_infoText;
        }
    }

    public string[] infoText;
    public int index;

    public float inputDir;

    #pragma warning disable CS0067
    public override event EventHandler<MenuExitEventArgs> menuExit;

    public static Pause_HandlerShared_InfoBox BuildMenu(Pause_SectionShared section = null, int p_index = 0, string[] p_infoText = null)
    {
        GameObject newObj = new GameObject("Pause Info Box");
        Pause_HandlerShared_InfoBox newMenu = newObj.AddComponent<Pause_HandlerShared_InfoBox>();

        newMenu.SetSubsection(section);
        newMenu.section.gameObject.SetActive(true);
        newMenu.index = p_index;
        newMenu.infoText = p_infoText;
        newMenu.Init();


        return newMenu;
    }

    public override void Init()
    {
        index = 0;  //reset this always I guess
        section.ApplyUpdate(new UpdateObject(0, infoText));
        base.Init();
    }


    public void Update()
    {
        if (active)
        {
            MenuUpdate();
        }
    }

    void MenuUpdate()
    {
        if (Mathf.Sign(InputManager.GetAxisHorizontal()) != inputDir || InputManager.GetAxisHorizontal() == 0)
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
                if (inputDir < 0)
                {
                    index--;
                }
                else
                {
                    index++;
                }
            }

            if (index > infoText.Length - 1)
            {
                index = infoText.Length - 1;
            }
            if (index < 0)
            {
                index = 0;
            }

            if (inputDir != 0)
            {
                if (section != null)
                {
                    section.ApplyUpdate(new UpdateObject(index, infoText));
                }
                //Debug.Log((PauseItemPage)tabindex);
            }
        }

        if (InputManager.GetButtonDown(InputManager.Button.B)) //Press B to go back
        {
            section.gameObject.SetActive(false);
            PopSelf();
        }
    }
}
