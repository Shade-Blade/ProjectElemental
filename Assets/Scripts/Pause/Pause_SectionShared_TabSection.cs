using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pause_SectionShared_TabSection : Pause_SectionShared
{
    public float tabLow;
    public float tabHigh;
    public GameObject[] tabs;   //objects
    public int selectedTab;
    public int[] disabledTabs;

    //This only handles the disabled tabs
    //The selected tab is not really used
    public override void ApplyUpdate(object state)
    {
        int[] disabledTabs = (int[])state;
        for (int i = 0; i < tabs.Length; i++)
        {
            tabs[i].SetActive(true);
        }

        if (state == null)
        {
            return;
        }

        for (int i = 0; i < disabledTabs.Length; i++)
        {
            tabs[disabledTabs[i]].SetActive(false);
        }
    }

    public override object GetState()
    {
        return disabledTabs;
    }
}
