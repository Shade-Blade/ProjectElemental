using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Text_BadgeSprite : Text_SpecialSprite
{
    //text width number
    public static string GetBadgeWidth(string b)
    {
        //use em
        return (WIDTH) + "em";
    }

    //public string[] args;
    //public int index;
    public bool visible;

    public const float WIDTH = 1.5f;

    public Sprite itemSprite;

    //build an item sprite with given args
    //position is handled by the thing that makes the item sprites
    //(note for later: may need to check font size and stuff like that)
    public static GameObject Create(string[] args, int index, float relativeSize)
    {
        GameObject obj = Instantiate(MainManager.Instance.text_BadgeSprite);
        Text_BadgeSprite its = obj.GetComponent<Text_BadgeSprite>();
        its.args = args;
        its.index = index;
        its.relativeSize = relativeSize;

        if (args != null && args.Length > 0)
        {
            its.itemSprite = GetBadgeSprite(args[0]);
            its.baseBox.sprite = its.itemSprite;
        }
        else
        {
            its.itemSprite = null;
            its.baseBox.sprite = its.itemSprite;
        }

        return obj;
    }

    public static GameObject Create(string item)
    {
        GameObject obj = Instantiate(MainManager.Instance.text_ItemSprite);
        Text_BadgeSprite its = obj.GetComponent<Text_BadgeSprite>();
        its.args = new string[] { item };
        its.index = -1;
        its.visible = true;
        its.itemSprite = GetBadgeSprite(item);
        its.baseBox.sprite = its.itemSprite;

        its.RecalculateBoxSize();

        return obj;
    }

    public static Sprite GetBadgeSprite(string badge)
    {
        //Debug.Log("Display " + badge);
        Badge.BadgeType badgeType;

        Enum.TryParse(badge, out badgeType);

        //random failsafe thing I guess
        if ((int)badgeType < 1)
        {
            Debug.Log("Parse fail: " + badge);
            return MainManager.Instance.badgeSprites[MainManager.Instance.badgeSprites.Length - 1];
        }

        return MainManager.Instance.badgeSprites[(int)(badgeType) - 1];
    }
}
