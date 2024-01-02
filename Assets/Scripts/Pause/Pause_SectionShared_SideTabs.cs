using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pause_SectionShared_SideTabs : Pause_SectionShared
{
    public GameObject subobject;
    public GameObject[] tabs;
    //public RectTransform selectorArrow;
    public float leftPos = -210;
    public float rightPos = -165;

    public int tabIndex = -1;

    public override void ApplyUpdate(object state)
    {
        if (state == null)
        {
            return;
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
