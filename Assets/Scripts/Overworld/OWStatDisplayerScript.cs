using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OWStatDisplayerScript : MonoBehaviour
{
    public TMPro.TMP_Text textNumber;
    public Image backImage;
    public Image HPImage;
    public Image characterImage;
    public Sprite wilexSprite;
    public Sprite lunaSprite;

    PlayerData.PlayerDataEntry pde;

    public void SetEntity(PlayerData.PlayerDataEntry p_pde)
    {
        pde = p_pde;

        //backImage = GetComponentInChildren<Image>();
        HPImage.sprite = MainManager.Instance.commonSprites[(int)Text_CommonSprite.SpriteType.HP];

        switch (pde.entityID)
        {
            case BattleHelper.EntityID.Wilex:
                backImage.color = new Color(1, 0.8f, 0.8f, 0.8f);
                characterImage.sprite = wilexSprite;
                break;
            case BattleHelper.EntityID.Luna:
                backImage.color = new Color(0.8f, 1f, 0.8f, 0.8f);
                characterImage.sprite = lunaSprite;
                break;
            default:
                backImage.color = new Color(0.8f, 1, 1f, 0.8f);
                break;
        }

        if (pde != null)
        {
            textNumber.text = pde.hp + "/<size=" + MainManager.Instance.fontSize / 2 + ">" + pde.maxHP + "</size>";
        }
    }
    public void SetPosition(int index)
    {
        float pos = 67.5f + index * 135;
        backImage.rectTransform.anchoredPosition = new Vector3(pos, -25, 0);
    }

    // Update is called once per frame
    void Update()
    {
        if (pde == null)
        {
            return;
        }

        textNumber.text = pde.hp + "/<size=" + MainManager.Instance.fontSize / 2 + ">" + pde.maxHP + "</size>";
    }
}
