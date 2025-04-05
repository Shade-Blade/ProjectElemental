using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EXPDisplayerScript : MonoBehaviour
{
    PlayerData pd;

    public TMPro.TMP_Text textIcon;
    public TMPro.TMP_Text textNumber;
    public Image backImage;
    public Image epImage;

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
        lastXP = pd.exp;
        //backImage = GetComponentInChildren<Image>();
        epImage.sprite = MainManager.Instance.commonSprites[(int)Text_CommonSprite.SpriteType.XP];
        textNumber.text = pd.exp + "/<size=" + MainManager.Instance.fontSize / 2 + ">100</size>";
    }

    public void SetPosition()
    {
        backImage.rectTransform.anchoredPosition = new Vector3(-60, 25, 0);
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
        lastXP = pd.exp;

        if (displayXPCooldown > 0)
        {
            textNumber.text = "<color=#00c0c0>" + pd.exp + "</color>/<size=" + MainManager.Instance.fontSize / 2 + ">100</size>";
        } else
        {
            textNumber.text = pd.exp + "/<size=" + MainManager.Instance.fontSize / 2 + ">100</size>";
        }
    }
}