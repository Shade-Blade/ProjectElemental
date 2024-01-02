using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Text_RibbonSprite : Text_SpecialSprite
{
    //text width number
    public static string GetRibbonWidth(string b)
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
        GameObject obj = Instantiate(MainManager.Instance.text_RibbonSprite);
        Text_RibbonSprite its = obj.GetComponent<Text_RibbonSprite>();
        its.args = args;
        its.index = index;
        its.relativeSize = relativeSize;

        if (args != null && args.Length > 0)
        {
            its.itemSprite = GetRibbonSprite(args[0]);
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
        Text_RibbonSprite its = obj.GetComponent<Text_RibbonSprite>();
        its.args = new string[] { item };
        its.index = -1;
        its.visible = true;
        its.itemSprite = GetRibbonSprite(item);
        its.baseBox.sprite = its.itemSprite;

        its.RecalculateBoxSize();

        return obj;
    }

    public static Sprite GetRibbonSprite(string ribbon)
    {
        Ribbon.RibbonType ribbonType;

        Enum.TryParse(ribbon, true, out ribbonType);

        return MainManager.Instance.ribbonSprites[(int)(ribbonType) - 1];
    }
}