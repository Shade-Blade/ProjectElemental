using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Pause_SectionShared_SideTabs : Pause_SectionShared
{
    public GameObject subobject;
    public GameObject[] tabs;
    public Image[] tabImages;

    //public RectTransform selectorArrow;
    public float leftPos = -210;
    public float rightPos = -165;

    public int tabIndex = -1;

    public override void ApplyUpdate(object state)
    {
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
        return;
    }
    public override object GetState()
    {
        return new Pause_HandlerShared_SideTabs.UpdateObject(tabIndex);
    }

    void Update()
    {
        for (int i = 0; i < tabs.Length; i++)
        {
            float targetPos = rightPos;

            if (i == tabIndex)
            {
                targetPos = leftPos;
            }

            RectTransform trect = tabs[i].GetComponent<RectTransform>();

            if (Mathf.Abs(trect.anchoredPosition.x - targetPos) < 0.1f)
            {
                trect.anchoredPosition = Vector3.right * targetPos + trect.anchoredPosition.y * Vector3.up;
            }
            else
            {
                //4500f = 0.1s to move
                trect.anchoredPosition = Vector3.right * MainManager.EasingQuadraticTime(trect.anchoredPosition.x, targetPos, 4500f) + trect.anchoredPosition.y * Vector3.up;
            }
        }

        //Vector3 targetPosB = 305f * Vector3.left + Vector3.up * 130f + (selectedIndex * 25f * Vector3.down);

        //selectorArrow.anchoredPosition = MainManager.EasingQuadraticTime(targetPosB, selectorArrow.anchoredPosition, 20000f);
    }


    public override void Init()
    {
        if (tabIndex == -1)
        {
            tabIndex = 0;
        }
        subobject.SetActive(true);
        base.Init();
    }

    public override void Clear()
    {
        subobject.SetActive(false);
        base.Clear();
    }
}
