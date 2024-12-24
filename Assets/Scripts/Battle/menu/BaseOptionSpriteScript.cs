using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BaseOptionSpriteScript : MonoBehaviour
{
    public Image backImage;
    public Image iconImage;

    public BaseBattleMenu.BaseMenuName bname;
    public bool canUse;

    private Color[] colors = new Color[]
    {
        new Color(1,0.666f,0.666f, 0.8f), //jump
        new Color(1,1,0.666f, 0.8f),      //weapon
        new Color(0.666f,1,0.666f, 0.8f), //items
        new Color(1,0.666f,1, 0.8f),      //soul
        new Color(0.666f,0.666f,1, 0.8f), //tactics

        new Color(1,0.8f,0.6f, 0.8f),     //badge swap
        new Color(1,0.7f,0.888f, 0.8f),      //ribbon swap


        //disabled is the last one
        new Color(0.4f,0.4f,0.4f, 0.8f)
    };
    public List<Sprite> sprites;


    public void Setup(BaseBattleMenu.BaseMenuName p_bname, bool p_canUse, bool swordOrHammer, int weaponLevel)
    {
        bname = p_bname;
        canUse = p_canUse;

        int index = 0;
        int index2 = 0;

        switch (bname)
        {
            case BaseBattleMenu.BaseMenuName.Jump:
                index = 0;
                index2 = 0;
                break;
            case BaseBattleMenu.BaseMenuName.Weapon:
                if (swordOrHammer)
                {
                    index = 1 + weaponLevel;
                    index2 = 1;
                }
                else
                {
                    index = 4 + weaponLevel;
                    index2 = 1;
                }
                break;
            case BaseBattleMenu.BaseMenuName.Items:
                index = 8;
                index2 = 2;
                break;
            case BaseBattleMenu.BaseMenuName.Soul:
                index = 7;
                index2 = 3;
                break;
            case BaseBattleMenu.BaseMenuName.Tactics:
                index = 9;
                index2 = 4;
                break;
            case BaseBattleMenu.BaseMenuName.BadgeSwap:
                index = 10;
                index2 = 5;
                break;
            case BaseBattleMenu.BaseMenuName.RibbonSwap:
                index = 11;
                index2 = 6;
                break;
        }

        if (!canUse)
        {
            index2 = colors.Length - 1;
        }

        backImage.color = colors[index2];
        iconImage.sprite = sprites[index];
    }
}
