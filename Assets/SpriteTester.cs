using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteTester : MonoBehaviour
{
    void Start()
    {
        List<Sprite> list = new List<Sprite>();

        /*
        for (int i = 1; i < (int)Item.ItemType.SpicyEgg; i++)
        {
            list.Add(GlobalItemScript.GetItemSprite((Item.ItemType)i));
        }
        */

        /*
        for (int i = 1; i < (int)Badge.BadgeType.EndOfTable; i++)
        {
            list.Add(GlobalBadgeScript.GetBadgeSprite((Badge.BadgeType)i));
        }
        */

        for (int i = 1; i < (int)Ribbon.RibbonType.EndOfTable; i++)
        {
            for (int j = 0; j < 40; j++)
            {
                list.Add(GlobalRibbonScript.GetRibbonSprite((Ribbon.RibbonType)i));
            }
        }

        SpawnSprites(list, 3, 3.8f);
    }


    void SpawnSprites(List<Sprite> sprites, float scale, float delta)
    {
        for (int i = 0; i < sprites.Count; i++)
        {
            GameObject g = new GameObject();
            g.transform.parent = transform;

            g.transform.localPosition = Vector3.right * RandomGenerator.GetRange(-delta * (16/9f), delta * (16/9f)) + Vector3.up * RandomGenerator.GetRange(delta, -delta) + Vector3.forward * RandomGenerator.GetRange(-0.1f, 0.1f);
            SpriteRenderer sr = g.AddComponent<SpriteRenderer>();
            sr.sprite = sprites[i];
            g.transform.localScale = Vector3.one * scale;
        }
    }
}
