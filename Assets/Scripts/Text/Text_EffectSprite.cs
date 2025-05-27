using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Text_EffectSprite : Text_SpecialSprite
{
    //text width number
    public static string GetEffectWidth(string b)
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
        GameObject obj = Instantiate(MainManager.Instance.text_EffectSprite);
        Text_EffectSprite its = obj.GetComponent<Text_EffectSprite>();
        its.args = args;
        its.index = index;
        its.relativeSize = relativeSize;

        if (args != null && args.Length > 0)
        {
            its.effectSprite = GetEffectSprite(args[0]);
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
        GameObject obj = Instantiate(MainManager.Instance.text_EffectSprite);
        Text_EffectSprite its = obj.GetComponent<Text_EffectSprite>();
        its.args = new string[] { item };
        its.index = -1;
        its.visible = true;
        its.effectSprite = GetEffectSprite(item);
        its.baseBox.sprite = its.effectSprite;

        its.RecalculateBoxSize();

        return obj;
    }

    public static Sprite GetEffectSprite(string effect)
    {
        //Debug.Log("Display " + badge);
        Effect.EffectType effectType = (Effect.EffectType)(-1);

        Enum.TryParse(effect, true, out effectType);

        //random failsafe thing I guess
        //Note: effect type 0 is valid, so check for less than 0
        if ((int)effectType < 0)
        {
            Debug.Log("Parse fail: " + effect);
            return MainManager.Instance.effectSprites[MainManager.Instance.effectSprites.Length - 1];
        }

        return MainManager.Instance.effectSprites[(int)(effectType)];
    }
}

