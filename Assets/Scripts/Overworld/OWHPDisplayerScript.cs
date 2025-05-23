using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

public class OWHPDisplayerScript : MonoBehaviour
{
    public TMPro.TMP_Text textNumber;
    public Image backImage;
    public Image HPImage;
    public Image characterImage;
    public Sprite wilexSprite;
    public Sprite lunaSprite;

    PlayerData.PlayerDataEntry pde;

    public float displayHP;
    public float displayHPCooldown;
    public bool highlightHP;

    public bool setcolor;

    public void SetEntity(PlayerData.PlayerDataEntry p_pde)
    {
        pde = p_pde;
        displayHP = pde.hp;

        //backImage = GetComponentInChildren<Image>();
        HPImage.sprite = MainManager.Instance.commonSprites[(int)Text_CommonSprite.SpriteType.HP];

        if (setcolor)
        {
            switch (pde.entityID)
            {
                case BattleHelper.EntityID.Wilex:
                    backImage.color = new Color(1, 0.8f, 0.8f, 0.9f);
                    characterImage.sprite = wilexSprite;
                    break;
                case BattleHelper.EntityID.Luna:
                    backImage.color = new Color(0.8f, 1f, 0.8f, 0.9f);
                    characterImage.sprite = lunaSprite;
                    break;
                default:
                    backImage.color = new Color(0.6f, 1, 1f, 0.9f);
                    break;
            }
        }
        switch (pde.entityID)
        {
            case BattleHelper.EntityID.Wilex:
                characterImage.sprite = wilexSprite;
                break;
            case BattleHelper.EntityID.Luna:
                characterImage.sprite = lunaSprite;
                break;
            default:
                break;
        }

        if (pde != null)
        {
            textNumber.text = pde.hp + "/<size=" + MainManager.Instance.fontSize / 2 + ">" + pde.maxHP + "</size>";
        }
    }
    public void SetPosition(int index)
    {
        float pos = 100f + index * 158;
        backImage.rectTransform.anchoredPosition = new Vector3(pos, -30, 0);
    }

    // Update is called once per frame
    void Update()
    {
        if (pde == null)
        {
            return;
        }

        if (displayHP != pde.hp)
        {
            displayHPCooldown = HPStaminaDisplayerScript.STAT_HIGHLIGHT_MINIMUM;
        }
        displayHP = MainManager.EasingQuadraticTime(displayHP, pde.hp, 60);
        if (displayHPCooldown > 0)
        {
            displayHPCooldown -= Time.deltaTime;
        }
        else
        {
            displayHPCooldown = 0;
        }

        if (displayHPCooldown > 0 || (HPStaminaDisplayerScript.HighlightActive() && highlightHP))
        {
            textNumber.text = "<color=#c00000>" + pde.hp + "</color>/<size=" + MainManager.Instance.fontSize / 2 + ">" + pde.maxHP + "</size>";
        } else
        {
            textNumber.text = pde.hp + "/<size=" + MainManager.Instance.fontSize / 2 + ">" + pde.maxHP + "</size>";
        }
    }
}
