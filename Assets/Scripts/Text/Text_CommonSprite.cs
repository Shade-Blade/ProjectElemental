using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Text_CommonSprite : Text_SpecialSprite
{
    //text width number
    public static string GetWidth(string b)
    {
        //use em
        return (WIDTH) + "em";
    }

    //public string[] args;
    //public int index;
    public bool visible;

    public const float WIDTH = 1.5f;

    public Sprite sprite;

    public enum SpriteType
    {
        HP = 0,
        EP,
        SE,
        SP,
        Stamina,
        Carrot,
        Clock,
        Coin,
        SilverCoin,
        GoldCoin,
        Shard,
        XP,
        AstralToken
    }

    //build an item sprite with given args
    //position is handled by the thing that makes the item sprites
    //(note for later: may need to check font size and stuff like that)
    public static GameObject Create(string[] args, int index, float relativeSize)
    {
        GameObject obj = Instantiate(MainManager.Instance.text_CommonSprite);
        Text_CommonSprite its = obj.GetComponent<Text_CommonSprite>();
        its.args = args;
        its.index = index;
        its.relativeSize = relativeSize;

        if (args != null && args.Length > 0)
        {
            its.sprite = GetSprite(args[0]);
            its.baseBox.sprite = its.sprite;
        }
        else
        {
            its.sprite = null;
            its.baseBox.sprite = its.sprite;
        }

        return obj;
    }

    public static GameObject Create(string item)
    {
        GameObject obj = Instantiate(MainManager.Instance.text_ItemSprite);
        Text_CommonSprite its = obj.GetComponent<Text_CommonSprite>();
        its.args = new string[] { item };
        its.index = -1;
        its.visible = true;
        its.sprite = GetSprite(item);
        its.baseBox.sprite = its.sprite;

        its.RecalculateBoxSize();

        return obj;
    }

    public static Sprite GetSprite(string input)
    {
        SpriteType stype;

        Enum.TryParse(input, true, out stype);

        //Debug.Log(stype + " " + input);

        return MainManager.Instance.commonSprites[(int)(stype)];
    }
}