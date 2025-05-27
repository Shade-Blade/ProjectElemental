using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

public class OWHPDisplayerScript : MonoBehaviour
{
    public TMPro.TMP_Text textNumber;
    public RectTransform rt;
    public Image backImage;
    public Image iconBackImage;
    public Image HPImage;
    public Image characterImage;
    public Image barImage;
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
                    //barImage.color = new Color(1, 0.95f, 0.95f, 0.9f);
                    //backImage.color = new Color(0.7f, 0.4f, 0.4f, 0.9f);
                    iconBackImage.color = new Color(1, 0.75f, 0.75f, 1f);
                    characterImage.sprite = wilexSprite;
                    break;
                case BattleHelper.EntityID.Luna:
                    //barImage.color = new Color(0.95f, 1f, 0.95f, 0.9f);
                    //backImage.color = new Color(0.4f, 0.7f, 0.4f, 0.9f);
                    iconBackImage.color = new Color(0.75f, 1f, 0.75f, 1f);
                    characterImage.sprite = lunaSprite;
                    break;
                default:
                    //barImage.color = new Color(0.95f, 1, 1f, 0.9f);
                    //backImage.color = new Color(0.4f, 0.7f, 0.4f, 0.9f);
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
            if (displayHP <= PlayerData.PlayerDataEntry.GetDangerHP(pde.entityID))
            {
                textNumber.text = "<color=#e02060>" + Mathf.RoundToInt(displayHP) + "</color><space=0.1em>/<space=0.1em><size=60%>" + pde.maxHP + "</size>";
            }
            else
            {
                if (displayHPCooldown > 0 || (HPStaminaDisplayerScript.HighlightActive() && highlightHP))
                {
                    textNumber.text = "<color=#ff8080>" + Mathf.RoundToInt(displayHP) + "</color><space=0.1em>/<space=0.1em><size=60%>" + pde.maxHP + "</size>";
                }
                else
                {
                    textNumber.text = "" + Mathf.RoundToInt(displayHP) + "<space=0.1em>/<space=0.1em><size=60%>" + pde.maxHP + "</size>";
                }
            }
        }

        barImage.rectTransform.anchoredPosition = Vector2.left * (1 - (displayHP / pde.maxHP)) * (rt.sizeDelta.x - 15 - 27.5f);
    }
    public void SetPosition(float index)
    {
        float pos = 76f + index * 140;
        rt.anchoredPosition = new Vector3(pos, -28, 0);
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
        displayHP = MainManager.EasingQuadraticTime(displayHP, pde.hp, HPStaminaDisplayerScript.STAT_SCROLL_SPEED);
        if (displayHPCooldown > 0)
        {
            displayHPCooldown -= Time.deltaTime;
        }
        else
        {
            displayHPCooldown = 0;
        }

        if (displayHP <= PlayerData.PlayerDataEntry.GetDangerHP(pde.entityID))
        {
            textNumber.text = "<color=#e02060>" + Mathf.RoundToInt(displayHP) + "</color><space=0.1em>/<space=0.1em><size=60%>" + pde.maxHP + "</size>";
        }
        else
        {
            if (displayHPCooldown > 0 || (HPStaminaDisplayerScript.HighlightActive() && highlightHP))
            {
                textNumber.text = "<color=#ff8080>" + Mathf.RoundToInt(displayHP) + "</color><space=0.1em>/<space=0.1em><size=60%>" + pde.maxHP + "</size>";
            }
            else
            {
                textNumber.text = "" + Mathf.RoundToInt(displayHP) + "<space=0.1em>/<space=0.1em><size=60%>" + pde.maxHP + "</size>";
            }
        }

        barImage.rectTransform.anchoredPosition = Vector2.left * (1 - (displayHP / pde.maxHP)) * (rt.sizeDelta.x - 15 - 27.5f);
    }
}
