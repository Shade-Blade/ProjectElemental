using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EXPDisplayerScript : MonoBehaviour
{
    PlayerData pd;

    public TMPro.TMP_Text textIcon;
    public TMPro.TMP_Text textNumber;
    public RectTransform rt;
    public Image expImage;
    public Image barImage;

    public float displayXPCooldown = 0;
    public float lastXP;

    // Start is called before the first frame update
    void Start()
    {
        //backImage = GetComponentInChildren<Image>();
    }

    public void Setup(PlayerData p_pd)
    {
        pd = p_pd;
        lastXP = pd.exp % 100;
        //backImage = GetComponentInChildren<Image>();
        expImage.sprite = MainManager.Instance.commonSprites[(int)Text_CommonSprite.SpriteType.XP];
        textNumber.text = "" + (pd.exp % 100) + "<space=0.1em>/<space=0.1em><size=60%>100</size>";

        barImage.rectTransform.anchoredPosition = Vector2.left * (1 - pd.exp / 100f) * (rt.sizeDelta.x - 15 - 27.5f);

        if (pd.level >= PlayerData.GetMaxLevel())
        {
            barImage.rectTransform.anchoredPosition = Vector2.zero;
        }
    }

    public void SetPosition()
    {
        rt.anchoredPosition = new Vector3(-85, -28, 0);
    }

    // Update is called once per frame
    void Update()
    {
        if (lastXP != pd.exp)
        {
            displayXPCooldown = HPStaminaDisplayerScript.STAT_HIGHLIGHT_MINIMUM;
        }
        if (displayXPCooldown > 0)
        {
            displayXPCooldown -= Time.deltaTime;
        } else
        {
            displayXPCooldown = 0;
        }
        lastXP = pd.exp % 100;

        if (displayXPCooldown > 0 || pd.level >= PlayerData.GetMaxLevel())
        {
            textNumber.text = "<color=#80ffff>" + (pd.exp % 100) + "</color><space=0.1em>/<space=0.1em><size=60%>100</size>";
        } else
        {
            textNumber.text = "" + (pd.exp % 100) + "<space=0.1em>/<space=0.1em><size=60%>100</size>";
        }

        barImage.rectTransform.anchoredPosition = Vector2.left * (1 - pd.exp % 100 / 100f) * (rt.sizeDelta.x - 15 - 27.5f);

        if (pd.level >= PlayerData.GetMaxLevel() || (pd.level == PlayerData.GetMaxLevel() - 1 && pd.exp >= 100))
        {
            barImage.rectTransform.anchoredPosition = Vector2.zero;
        }
    }
}