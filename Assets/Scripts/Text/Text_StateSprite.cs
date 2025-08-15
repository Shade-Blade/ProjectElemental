using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Text_StateSprite : Text_SpecialSprite
{
    //text width number
    public static string GetStateWidth(string b)
    {
        //use em
        return (WIDTH) + "em";
    }

    //public string[] args;
    //public int index;
    public bool visible;

    public const float WIDTH = 1.5f;

    public Sprite effectSprite;


    //build an item sprite with given args
    //position is handled by the thing that makes the item sprites
    //(note for later: may need to check font size and stuff like that)
    public static GameObject Create(string[] args, int index, float relativeSize)
    {
        GameObject obj = Instantiate(MainManager.Instance.text_StateSprite);
        Text_StateSprite its = obj.GetComponent<Text_StateSprite>();
        its.args = args;
        its.index = index;
        its.relativeSize = relativeSize;

        if (args != null && args.Length > 0)
        {
            its.effectSprite = GetStateSprite(args[0]);
            its.baseBox.sprite = its.effectSprite;
        }
        else
        {
            its.effectSprite = null;
            its.baseBox.sprite = its.effectSprite;
        }

        return obj;
    }

    public static GameObject Create(string item)
    {
        GameObject obj = Instantiate(MainManager.Instance.text_StateSprite);
        Text_StateSprite its = obj.GetComponent<Text_StateSprite>();
        its.args = new string[] { item };
        its.index = -1;
        its.visible = true;
        its.effectSprite = GetStateSprite(item);
        its.baseBox.sprite = its.effectSprite;

        its.RecalculateBoxSize();

        return obj;
    }

    public static Sprite GetStateSprite(string state)
    {
        //Debug.Log("Display " + badge);
        BattleHelper.EntityState stateType;

        Enum.TryParse(state, out stateType);

        //random failsafe thing I guess
        if ((int)stateType < 1)
        {
            Debug.Log("Parse fail: " + state);
            return MainManager.Instance.stateSprites[MainManager.Instance.stateSprites.Length - 1];
        }

        return MainManager.Instance.stateSprites[(int)(stateType) - 1];
    }
}


