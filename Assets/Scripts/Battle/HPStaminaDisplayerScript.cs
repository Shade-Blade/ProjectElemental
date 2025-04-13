using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HPStaminaDisplayerScript : MonoBehaviour
{
    public PlayerEntity entity;
    public TextDisplayer textNumber;
    public TextDisplayer textNumber2;
    public Image backImage;
    public Image HPImage;
    public Image STImage;
    public Image characterImage;
    public Sprite wilexSprite;
    public Sprite lunaSprite;

    public float displayHP;
    public float displayStamina;

    public float displayHPCooldown;
    public float displayStaminaCooldown;

    public bool highlightHP;
    public bool highlightStamina;
    public const float STAT_SCROLL_SPEED = 30;
    public const float STAT_HIGHLIGHT_MINIMUM = 0.3f;

    public void SetEntity(PlayerEntity b)
    {
        entity = b;

        //backImage = GetComponentInChildren<Image>();
        HPImage.sprite = MainManager.Instance.commonSprites[(int)Text_CommonSprite.SpriteType.HP];
        STImage.sprite = MainManager.Instance.commonSprites[(int)Text_CommonSprite.SpriteType.Stamina];

        switch (b.entityID)
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

        displayHP = b.hp;
        displayStamina = b.stamina;

        if (entity != null)
        {
            textNumber.SetText(entity.hp + "/<size,50%>" + entity.maxHP + "</size>", true, true);
            if (entity.staminaBlock)
            {
                textNumber2.SetText(entity.stamina + " <descriptionwarncolor><strikethrough><size,50%>(" + entity.GetRealAgility() + ")</size></strikethrough></descriptionwarncolor>",true,true);
            }
            else
            {
                textNumber2.SetText(entity.stamina + " <size,50%>(" + entity.GetRealAgility() + ")</size>", true, true);
            }
        }
    }
    public void SetPosition(int index)
    {
        float pos = 100f + index * 158;
        backImage.rectTransform.anchoredPosition = new Vector3(pos,-40,0);
    }

    public static bool HighlightActive()
    {
        return (int)(Time.time * 3) % 2 == 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (entity == null)
        {
            Debug.LogWarning("Stale displayer");
            return; //BattleControl will fix any invalid displayers
            //Destroy(gameObject);
        }

        //move displayHP and displayStamina towards the values
        if (displayHP != entity.hp)
        {
            displayHPCooldown = STAT_HIGHLIGHT_MINIMUM;
        }
        displayHP = MainManager.EasingQuadraticTime(displayHP, entity.hp, STAT_SCROLL_SPEED);
        if (displayHPCooldown > 0)
        {
            displayHPCooldown -= Time.deltaTime;
        } else
        {
            displayHPCooldown = 0;
        }

        displayStamina = MainManager.EasingQuadraticTime(displayStamina, entity.stamina, STAT_SCROLL_SPEED);
        if (displayStamina != entity.stamina)
        {
            displayStaminaCooldown = STAT_HIGHLIGHT_MINIMUM;
        }
        if (displayStaminaCooldown > 0)
        {
            displayStaminaCooldown -= Time.deltaTime;
        }
        else
        {
            displayStaminaCooldown = 0;
        }

        if ((HighlightActive() && highlightHP) || displayHPCooldown > 0)
        {
            textNumber.SetText("<color,#c00000>" + Mathf.RoundToInt(displayHP) + "</color>" + "/<size,50%>" + entity.maxHP + "</size>", true, true);
        } else
        {
            textNumber.SetText(Mathf.RoundToInt(displayHP) + "/<size,50%>" + entity.maxHP + "</size>", true, true);
        }

        if ((HighlightActive() && highlightStamina) || displayStaminaCooldown > 0)
        {
            if (entity.staminaBlock)
            {
                textNumber2.SetText("<color,#00c000>" + Mathf.RoundToInt(displayStamina) + "</color> <descriptionwarncolor><strikethrough><size,50%>(" + entity.GetRealAgility() + ")</size></strikethrough></descriptionwarncolor>", true, true);
            }
            else
            {
                textNumber2.SetText("<color,#00c000>" + Mathf.RoundToInt(displayStamina) + "</color> <size,50%>(" + entity.GetRealAgility() + ")</size>", true, true);
            }
        }
        else
        {
            if (entity.staminaBlock)
            {
                textNumber2.SetText(Mathf.RoundToInt(displayStamina) + " <descriptionwarncolor><strikethrough><size,50%>(" + entity.GetRealAgility() + ")</size></strikethrough></descriptionwarncolor>", true, true);
            }
            else
            {
                textNumber2.SetText(Mathf.RoundToInt(displayStamina) + " <size,50%>(" + entity.GetRealAgility() + ")</size>", true, true);
            }
        }
    }
}
