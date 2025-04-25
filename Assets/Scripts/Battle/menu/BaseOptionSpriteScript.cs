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
        new Color(1f,1f,1f, 0.9f), //jump
        new Color(1,0.9f,0.8f, 0.9f),      //weapon
        new Color(0.9f,1,0.8f, 0.9f), //items
        new Color(0.8f,0.8f,1, 0.9f),      //soul
        new Color(0.8f,0.95f,1, 0.9f), //tactics
        
        new Color(1,0.975f,0.8f, 0.9f),     //badge swap
        new Color(1,0.8f,0.95f, 0.9f),      //ribbon swap
        new Color(0.8f,1,0.95f, 0.9f), //turn relay

        //disabled is the last one
        new Color(0.4f,0.4f,0.4f, 0.9f)
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
            case BaseBattleMenu.BaseMenuName.TurnRelay:
                index = 12;
                index2 = 7;
                break;
        }

        if (!canUse)
        {
            index2 = colors.Length - 1;
            backImage.color = colors[index2];
        }

        backImage.color = colors[index2];
        //backImage.color = new Color(0.85f, 1f, 1f, 0.8f);
        iconImage.sprite = sprites[index];
    }
}
