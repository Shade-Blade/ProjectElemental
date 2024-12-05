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
        selectorArrow.enabled = false;
        textbox.transform.parent.transform.parent.gameObject.SetActive(false);
        if (state == null)
        {
            for (int i = 0; i < tabs.Length; i++)
            {
                //I could make this better but I am lazy, and it doesn't really matter anyway
                //Less room for error if I runtime init this instead of manually setting it?
                if (tabImages == null || tabImages.Length != tabs.Length)
                {
                    tabImages = new Image[tabs.Length];
                }
                if (tabImages[i] == null)
                {
                    tabImages[i] = tabs[i].GetComponent<Image>();
                }
                tabImages[i].color = new Color(0.75f, 0.75f, 0.75f, 1);
            }
            return;
        }
        for (int i = 0; i < tabs.Length; i++)
        {
            if (tabImages == null || tabImages.Length != tabs.Length)
            {
                tabImages = new Image[tabs.Length];
            }
            if (tabImages[i] == null)
            {
                tabImages[i] = tabs[i].GetComponent<Image>();
            }
            tabImages[i].color = new Color(0.9f, 0.9f, 0.9f, 1);
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
        list.ApplyUpdate(null);

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
        list.ApplyUpdate(null);

        //Hacky
        textbox.transform.parent.transform.parent.gameObject.SetActive(false);

        base.Init();
    }

    public override void Clear()
    {
        subobject.SetActive(false);
        infoBox.gameObject.SetActive(false);
        base.Clear();
    }
}
