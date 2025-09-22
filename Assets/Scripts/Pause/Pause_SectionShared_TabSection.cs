using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Pause_SectionShared_TabSection : Pause_SectionShared
{
    public float tabLow;
    public float tabHigh;
    public GameObject[] tabs;   //objects
    public int selectedTab;
    public int[] disabledTabs;

    public Image leftArrow;
    public TextDisplayer leftArrowControlHint;
    public Image rightArrow;
    public TextDisplayer rightArrowControlHint;
    public TextDisplayer middleControlHint;

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

        Vector2 anchor = tabs[selectedTab].GetComponent<RectTransform>().anchoredPosition;
        leftArrow.rectTransform.anchoredPosition = Vector2.right * (anchor.x - 60) + Vector2.up * 190;
        rightArrow.rectTransform.anchoredPosition = Vector2.right * (anchor.x + 60) + Vector2.up * 190;
        middleControlHint.GetComponent<RectTransform>().anchoredPosition = anchor.x * Vector2.right + Vector2.up * 170;
    }

    public override object GetState()
    {
        return disabledTabs;
    }

    public void SetTabArrows(bool select)
    {
        if (selectedTab == 0)
        {
            leftArrowControlHint.SetText("", true, true);
            leftArrow.enabled = false;
        } else
        {
            leftArrowControlHint.SetText(!select ? "" : "<button,left>", true, true);
            leftArrow.enabled = select;
        }
        //leftArrowControlHint.enabled = select;

        if (selectedTab == 6)
        {
            rightArrowControlHint.SetText("", true, true);
            rightArrow.enabled = false;
        }
        else
        {
            rightArrowControlHint.SetText(!select ? "" : "<button,right>", true, true);
            rightArrow.enabled = select;
        }
        //rightArrowControlHint.enabled = select;
    }
    public void SetSelectText()
    {
        SetTabArrows(true);
        middleControlHint.enabled = true;
        middleControlHint.SetText("Open<button,A>", true, true);
    }
    public void SetCloseText()
    {
        SetTabArrows(false);
        middleControlHint.enabled = true;
        middleControlHint.SetText("Back<button,B>", true, true);
    }
    public void SetNoText()
    {
        SetTabArrows(false);
        middleControlHint.enabled = false;
    }
}
