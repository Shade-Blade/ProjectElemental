using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Pause_SectionQuest : Pause_SectionShared_SideTabs
{
    public Pause_SectionShared_InfoBox infoBox;
    public Pause_SectionQuest_List list;

    public bool availableMode;

    public Image selectorArrow;


    public override void ApplyUpdate(object state)
    {
        selectorArrow.color = new Color(0.5f, 0.5f, 0.5f, 1);
        if (state == null)
        {
            return;
        }

        //int index = (int)(state);
        int index = ((Pause_HandlerShared_SideTabs.UpdateObject)state).tabindex;
        tabIndex = index;

        list.Clear();
        if (availableMode)
        {
            list.subpage = Pause_HandlerQuest.QuestSubpage.Available;
        }
        else
        {
            list.subpage = (Pause_HandlerQuest.QuestSubpage)(tabIndex + 1);
        }
        list.Init();
        list.ApplyUpdate(0);

        return;
    }

    public override Pause_SectionShared GetSubsection(object state)
    {
        return list;
    }

    public override void Init()
    {
        if (tabIndex == -1)
        {
            tabIndex = 0;
        }
        subobject.SetActive(true);
        infoBox.gameObject.SetActive(false);    //keep off by default

        selectorArrow.gameObject.SetActive(false);

        list.Clear();
        if (availableMode)
        {
            list.subpage = Pause_HandlerQuest.QuestSubpage.Available;
        }
        else
        {
            list.subpage = (Pause_HandlerQuest.QuestSubpage)(tabIndex + 1);
        }
        list.Init();
        list.ApplyUpdate(0);

        base.Init();
    }

    public override void Clear()
    {
        subobject.SetActive(false);
        infoBox.gameObject.SetActive(false);
        base.Clear();
    }
}
