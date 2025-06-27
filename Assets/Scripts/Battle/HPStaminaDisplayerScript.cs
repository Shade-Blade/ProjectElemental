using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static BattleHelper;

public class HPStaminaDisplayerScript : MonoBehaviour
{
    public PlayerEntity entity;
    public TextDisplayer textNumber;
    public TextDisplayer textNumber2;
    public RectTransform rt;
    public Image backImage;
    public Image iconBackImage;
    public Image HPImage;
    public Image STImage;
    public Image characterImage;
    public Image barImage;

    public Image barImageBottom;

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

    public bool setcolor;

    public void SetEntity(PlayerEntity b)
    {
        entity = b;

        //backImage = GetComponentInChildren<Image>();
        HPImage.sprite = MainManager.Instance.commonSprites[(int)Text_CommonSprite.SpriteType.HP];
        STImage.sprite = MainManager.Instance.commonSprites[(int)Text_CommonSprite.SpriteType.Stamina];

        if (setcolor)
        {
            switch (b.entityID)
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
        switch (b.entityID)
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

        displayHP = b.hp;
        displayStamina = b.stamina;

        if (entity != null)
        {
            string healthText;
            if (displayHP <= PlayerData.PlayerDataEntry.GetDangerHP(entity.entityID))
            {
                healthText = "<descriptionwarncolor>" + Mathf.RoundToInt(displayHP) + "</color>";
            }
            else
            {
                if ((HighlightActive() && highlightHP) || displayHPCooldown > 0)
                {
                    healthText = "<color,#ff8080>" + Mathf.RoundToInt(displayHP) + "</color>";
                }
                else
                {
                    healthText = "" + Mathf.RoundToInt(displayHP) + "";
                }
            }
            int deltaMaxHP = entity.GetEffectPotency(Effect.EffectType.MaxHPBoost) - entity.GetEffectPotency(Effect.EffectType.MaxHPReduction);
            if (deltaMaxHP != 0)
            {
                if (deltaMaxHP > 0)
                {
                    healthText += "<space,0.1em>/<space,0.1em><size,60%><color,#80ff80>" + entity.maxHP + "</color></size></colordarkoutline>";
                }
                else
                {
                    healthText += "<space,0.1em>/<space,0.1em><size,60%><color,#ff8080>" + entity.maxHP + "</color></size></colordarkoutline>";
                }
            }
            else
            {
                healthText += "<space,0.1em>/<space,0.1em><size,60%>" + entity.maxHP + "</size></colordarkoutline>";
            }
            textNumber.SetText(healthText, true, true);

            string staminaText = "<colordarkoutline>" + entity.stamina + " ";
            if (Mathf.RoundToInt(displayStamina) < 0)
            {
                staminaText = "<colordarkoutline><descriptionwarncolor>" + Mathf.RoundToInt(displayStamina) + "</color> ";
            }

            if (entity.staminaBlock)
            {
                staminaText += "<descriptionwarncolor><strikethrough><size,60%>(+" + entity.GetRealAgility() + ")</size></strikethrough></descriptionwarncolor></outline>";
            }
            else
            {
                staminaText += "<size,60%>(+" + entity.GetRealAgility() + ")</size></strikethrough></outline>";
            }
            textNumber2.SetText(staminaText, true, true);
        }

        if (entity.maxHP == 0)
        {
            //treated as 0
            barImage.rectTransform.anchoredPosition = Vector2.left * (rt.sizeDelta.x - 15 - 27.5f);
        }
        else
        {
            barImage.rectTransform.anchoredPosition = Vector2.left * (1 - (displayHP / entity.maxHP)) * (rt.sizeDelta.x - 15 - 27.5f);
        }
        if (BattleControl.Instance.GetMaxStamina(entity) == 0)
        {
            barImageBottom.rectTransform.anchoredPosition = Vector2.zero;
        }
        else
        {
            barImageBottom.rectTransform.anchoredPosition = Vector2.left * (1 - (displayStamina / BattleControl.Instance.GetMaxStamina(entity))) * (barImageBottom.rectTransform.sizeDelta.x - 12);
        }
    }
    public void SetPosition(float index)
    {
        float pos = 76f + index * 140;
        rt.anchoredPosition = new Vector3(pos,-28f,0);
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
        if (entity.hp == 0 && entity.stamina == 0)
        {
            displayStamina = 0;
        }
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

        string healthText;
        if (displayHP <= PlayerData.PlayerDataEntry.GetDangerHP(entity.entityID))
        {
            healthText = "<descriptionwarncolor>" + Mathf.RoundToInt(displayHP) + "</color>";
        }
        else
        {
            if ((HighlightActive() && highlightHP) || displayHPCooldown > 0)
            {
                healthText = "<color,#ff8080>" + Mathf.RoundToInt(displayHP) + "</color>";
            }
            else
            {
                healthText = "" + Mathf.RoundToInt(displayHP) + "";
            }
        }
        int deltaMaxHP = entity.GetEffectPotency(Effect.EffectType.MaxHPBoost) - entity.GetEffectPotency(Effect.EffectType.MaxHPReduction);
        if (deltaMaxHP != 0)
        {
            if (deltaMaxHP > 0)
            {
                healthText += "<space,0.1em>/<space,0.1em><size,60%><color,#80ff80>" + entity.maxHP + "</color></size></colordarkoutline>";
            } else
            {
                healthText += "<space,0.1em>/<space,0.1em><size,60%><color,#ff8080>" + entity.maxHP + "</color></size></colordarkoutline>";
            }
        } else
        {
            healthText += "<space,0.1em>/<space,0.1em><size,60%>" + entity.maxHP + "</size></colordarkoutline>";
        }
        textNumber.SetText(healthText, true, true);


        bool maxStamina = entity.stamina >= BattleControl.Instance.GetMaxStamina(entity);
        string staminaText;
        if (Mathf.RoundToInt(displayStamina) < 0)
        {
            //<colordarkoutline>
            staminaText = "<descriptionwarncolor>" + Mathf.RoundToInt(displayStamina) + "</color> ";
        }
        else
        {
            if ((HighlightActive() && highlightStamina) || displayStaminaCooldown > 0 || maxStamina)
            {
                staminaText = "<colordarkoutline,#80ff80>" + Mathf.RoundToInt(displayStamina) + "</color> ";
            }
            else
            {
                staminaText = "<colordarkoutline>" + Mathf.RoundToInt(displayStamina) + " ";
            }
        }
        if (entity.staminaBlock)
        {
            staminaText += "<descriptionwarncolor><strikethrough><size,60%>(+" + entity.GetRealAgility() + ")</size></strikethrough></descriptionwarncolor>";
        }
        else
        {
            staminaText += "<size,60%>(+" + entity.GetRealAgility() + ")</size></strikethrough>";
        }
        textNumber2.SetText(staminaText, true, true);

        if (entity.maxHP == 0)
        {
            barImage.rectTransform.anchoredPosition = Vector2.left * (rt.sizeDelta.x - 15 - 27.5f);
        } else
        {
            barImage.rectTransform.anchoredPosition = Vector2.left * (1 - (displayHP / entity.maxHP)) * (rt.sizeDelta.x - 15 - 27.5f);
        }

        if (BattleControl.Instance.GetMaxStamina(entity) == 0)
        {
            barImageBottom.rectTransform.anchoredPosition = Vector2.zero;
        }
        else
        {
            barImageBottom.rectTransform.anchoredPosition = Vector2.left * (1 - (displayStamina / BattleControl.Instance.GetMaxStamina(entity))) * (barImageBottom.rectTransform.sizeDelta.x - 12);
        }
    }
}
