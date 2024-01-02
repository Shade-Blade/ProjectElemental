using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatDisplayerScript : MonoBehaviour
{
    public PlayerEntity entity;
    public TMPro.TMP_Text textNumber;
    public TMPro.TMP_Text textNumber2;
    public Image backImage;
    public Image HPImage;
    public Image STImage;
    public Image characterImage;
    public Sprite wilexSprite;
    public Sprite lunaSprite;

    public void SetEntity(PlayerEntity b)
    {
        entity = b;

        //backImage = GetComponentInChildren<Image>();
        HPImage.sprite = MainManager.Instance.commonSprites[(int)Text_CommonSprite.SpriteType.HP];
        STImage.sprite = MainManager.Instance.commonSprites[(int)Text_CommonSprite.SpriteType.Stamina];

        switch (b.entityID)
        {
            case BattleHelper.EntityID.Wilex:
                backImage.color = new Color(1, 0.8f, 0.8f);
                characterImage.sprite = wilexSprite;
                break;
            case BattleHelper.EntityID.Luna:
                backImage.color = new Color(0.8f, 1f, 0.8f);
                characterImage.sprite = lunaSprite;
                break;
            default:
                backImage.color = new Color(0.8f, 1, 1f);
                break;
        }

        if (entity != null)
        {
            textNumber.text = entity.hp + "/<size=" + MainManager.Instance.fontSize / 2 + ">" + entity.maxHP + "</size>";
            textNumber2.text = entity.stamina + " <size=" + MainManager.Instance.fontSize / 2 + ">(" + entity.GetRealAgility() + (entity.staminaBlock ? "X" : "") +  ")</size>";
        }
    }
    public void SetPosition(int index)
    {
        float pos = 67.5f + index * 135;
        backImage.rectTransform.anchoredPosition = new Vector3(pos,-35,0);
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

        textNumber.text = entity.hp + "/<size=" + MainManager.Instance.fontSize/2 + ">" + entity.maxHP + "</size>";
        textNumber2.text = entity.stamina + " <size=" + MainManager.Instance.fontSize / 2 + ">(" + entity.GetRealAgility() + (entity.staminaBlock ? "X" : "")  + ")</size>";
    }
}
