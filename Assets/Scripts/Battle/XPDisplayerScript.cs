using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class XPDisplayerScript : MonoBehaviour
{
    //public Sprite xpSprite;
    int visualXP;
    public List<GameObject> bigOrbs;
    public List<GameObject> littleOrbs;

    public void Setup()
    {
        visualXP = (int)BattleControl.Instance.visualXP;

        /*
        GameObject so = new GameObject("XP Sprite");
        so.transform.parent = BattleControl.Instance.transform;
        SpriteRenderer s = so.AddComponent<SpriteRenderer>();
        s.sprite = isp;
        so.transform.position = position;
         */

        int tensDigit = visualXP / 10;
        int onesDigit = visualXP % 10;

        for (int i = 0; i < bigOrbs.Count; i++)
        {
            bigOrbs[i].GetComponent<Image>().sprite = MainManager.Instance.commonSprites[(int)Text_CommonSprite.SpriteType.XP];
            if (i < tensDigit)
            {
                bigOrbs[i].SetActive(true);
            }
            else
            {
                bigOrbs[i].SetActive(false);
            }
        }
        for (int i = 0; i < littleOrbs.Count; i++)
        {
            littleOrbs[i].GetComponent<Image>().sprite = MainManager.Instance.commonSprites[(int)Text_CommonSprite.SpriteType.XP];
            if (i < onesDigit)
            {
                littleOrbs[i].SetActive(true);
            }
            else
            {
                littleOrbs[i].SetActive(false);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        visualXP = (int)BattleControl.Instance.visualXP;

        /*
        GameObject so = new GameObject("XP Sprite");
        so.transform.parent = BattleControl.Instance.transform;
        SpriteRenderer s = so.AddComponent<SpriteRenderer>();
        s.sprite = isp;
        so.transform.position = position;
         */

        int tensDigit = visualXP / 10;
        int onesDigit = visualXP % 10;

        for (int i = 0; i < bigOrbs.Count; i++)
        {
            if (i < tensDigit)
            {
                bigOrbs[i].SetActive(true);
            } else
            {
                bigOrbs[i].SetActive(false);
            }
        }
        for (int i = 0; i < littleOrbs.Count; i++)
        {
            if (i < onesDigit)
            {
                littleOrbs[i].SetActive(true);
            }
            else
            {
                littleOrbs[i].SetActive(false);
            }
        }
    }
}
