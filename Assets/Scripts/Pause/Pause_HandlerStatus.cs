using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Pause_SectionBase;

public class Pause_HandlerStatus : Pause_HandlerShared
{
    public Pause_SectionStatus_MenuNode selectedNode;
    Vector3 inputDir;

    #pragma warning disable CS0067
    public override event EventHandler<MenuExitEventArgs> menuExit;


    public static Pause_HandlerStatus BuildMenu(Pause_SectionShared section = null)
    {
        GameObject newObj = new GameObject("Pause Status Menu");
        Pause_HandlerStatus newMenu = newObj.AddComponent<Pause_HandlerStatus>();

        newMenu.SetSubsection(section);

        newMenu.Init();


        return newMenu;
    }

    void Update()
    {
        if (active)
        {
            MenuUpdate();
        }
    }

    void MenuUpdate()
    {
        Vector3 lastInput = inputDir;
        inputDir = InputManager.GetAxisVertical() * Vector3.up + InputManager.GetAxisHorizontal() * Vector3.right;

        //Normalization in case weird controller stuff happens
        if (inputDir.magnitude < 0.5f)
        {
            inputDir = Vector3.zero;
        } else
        {
            inputDir = inputDir.normalized;
        }

        if (Mathf.Abs(inputDir.x) > 0.5f)
        {
            if (inputDir.x > 0)
            {
                inputDir.x = 1;
            } else
            {
                inputDir.x = -1;
            }
        } else
        {
            inputDir.x = 0;
        }
        if (Mathf.Abs(inputDir.y) > 0.5f)
        {
            if (inputDir.y > 0)
            {
                inputDir.y = 1;
            }
            else
            {
                inputDir.y = -1;
            }
        } else
        {
            inputDir.y = 0;
        }


        if (lastInput != inputDir)
        {
            if (inputDir == Vector3.up)
            {
                TryMove(selectedNode.up);
            }
            if (inputDir == Vector3.down)
            {
                TryMove(selectedNode.down);
            }
            if (inputDir == Vector3.right)
            {
                TryMove(selectedNode.right);
            }
            if (inputDir == Vector3.left)
            {
                TryMove(selectedNode.left);
            }
            if (inputDir.x > 0 && inputDir.y > 0)
            {
                TryMove(selectedNode.upright);
            }
            if (inputDir.x > 0 && inputDir.y < 0)
            {
                TryMove(selectedNode.downright);
            }
            if (inputDir.x < 0 && inputDir.y > 0)
            {
                TryMove(selectedNode.upleft);
            }
            if (inputDir.x < 0 && inputDir.y < 0)
            {
                TryMove(selectedNode.downleft);
            }
        }

        if (InputManager.GetButtonDown(InputManager.Button.A)) //Press A to select stuff
        {
            //go to submenu
            Select();
        }
        if (InputManager.GetButtonDown(InputManager.Button.B)) //Press B to go back
        {
            section.ApplyUpdate(null);
            PopSelf();
        }
    }

    void TryMove(Pause_SectionStatus_MenuNode mn)
    {
        if (mn == null)
        {
            Debug.Log("Move in null dir " + inputDir);
            return;
        }

        selectedNode = mn;
        Debug.Log("Move to " + mn.menuNodeType);
        section.ApplyUpdate(mn);
    }

    void Select()
    {
        //this is a dead end (no submenus)
        Debug.Log(selectedNode.menuNodeType);
    }

    public override void Init()
    {
        if (section != null)
        {
            selectedNode = (Pause_SectionStatus_MenuNode)section.GetState();
            section.ApplyUpdate(selectedNode);
        }

        base.Init();
    }

    public override MenuResult GetResult()
    {
        return new MenuResult(selectedNode.menuNodeType);
    }
}
