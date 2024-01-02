using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pause_HandlerShared_InfoBoxMenu : Pause_HandlerShared_BoxMenu
{
    int indexCount = 0;
    bool canSelect = false;

    public static Pause_HandlerShared_InfoBoxMenu BuildMenu(Pause_SectionShared section = null, int count = 0, bool canSelect = false)
    {
        GameObject newObj = new GameObject("Pause Info Box Menu");
        Pause_HandlerShared_InfoBoxMenu newMenu = newObj.AddComponent<Pause_HandlerShared_InfoBoxMenu>();

        newMenu.SetSubsection(section);
        newMenu.indexCount = count;
        newMenu.canSelect = canSelect;
        newMenu.Init();


        return newMenu;
    }

    public override int GetObjectCount()
    {
        return indexCount;
    }


    public override void Select()
    {
        //even more submenus (but we are getting close to the end)
        if (canSelect)
        {
            Debug.Log(index + " select");
        }
        else
        {
            Debug.Log(index + " can't select");
            return;
        }

        //this selection may be more complex
        //but I may be fine with just a selection menu
        //The cases are similar to the battle selection menu (0 entities, 1 entity, multiple entities)
        //even more submenus (but we are getting close to the end)
        Pause_SectionShared newSection = null;
        if (section != null)
        {
            newSection = section.GetSubsection(index);
        }

        //try to get the data I guess
        //a lot of casting :P
        InformationMenuEntry ime = (InformationMenuEntry)(((Pause_SectionShared_BoxMenu)section).GetMenuEntry(index));

        //build the menu
        MenuHandler b = null;
        b = Pause_HandlerShared_InfoBox.BuildMenu(newSection, 0, ime.infoText);

        b.transform.parent = transform;
        PushState(b);
        b.menuExit += InvokeExit;
    }

    public override void Init()
    {
        base.Init();

        if (section != null)
        {
            int[] uo = (int[])(section.GetState());
            int newIndex = uo[0];
            index = newIndex;
        }

        if (index > indexCount - 1)
        {
            index = indexCount - 1;
        }
        if (index < 0)
        {
            index = 0;
        }

        if (section != null)
        {
            section.ApplyUpdate(index);
        }
    }


}
