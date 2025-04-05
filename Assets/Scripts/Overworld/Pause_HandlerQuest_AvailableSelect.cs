using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pause_HandlerQuest_AvailableSelect : Pause_HandlerShared_BoxMenu
{
    int indexCount = 0;
    public bool done = false;

    public static Pause_HandlerQuest_AvailableSelect BuildMenu(Pause_SectionShared section = null, int count = 0)
    {
        GameObject newObj = new GameObject("Overworld Quest Available Select");
        Pause_HandlerQuest_AvailableSelect newMenu = newObj.AddComponent<Pause_HandlerQuest_AvailableSelect>();

        newMenu.SetSubsection(section);
        newMenu.indexCount = count;
        newMenu.Init();

        return newMenu;
    }

    public override int GetObjectCount()
    {
        return indexCount;
    }

    public override void Select()
    {
        MainManager.Instance.PlayGlobalSound(MainManager.Sound.Menu_Select);
        done = true;        
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
