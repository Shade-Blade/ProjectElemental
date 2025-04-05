using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class SEDisplayerScript : MonoBehaviour
{
    PlayerData pd;

    public TMPro.TMP_Text textNumber;
    public Image backImage;
    public Image SEImage;

    public float displaySE;
    public float displaySECooldown;
    public bool highlightSE;


    // Start is called before the first frame update
    void Start()
    {
        //backImage = GetComponentInChildren<Image>();
    }

    public void Setup(PlayerData p_pd)
    {
        pd = p_pd;
        //backImage = GetComponentInChildren<Image>();
        SEImage.sprite = MainManager.Instance.commonSprites[(int)Text_CommonSprite.SpriteType.SE];
        textNumber.text = pd.se + "/<size=" + MainManager.Instance.fontSize / 2 + ">" + pd.maxSE + "</size>";

        displaySE = pd.se;
    }

    public void SetPosition()
    {
        backImage.rectTransform.anchoredPosition = new Vector3(-60f, -25f, 0);
    }

    // Update is called once per frame
    void Update()
    {
        if (displaySE != pd.se)
        {
            displaySECooldown = HPStaminaDisplayerScript.STAT_HIGHLIGHT_MINIMUM;
        }
        displaySE = MainManager.EasingQuadraticTime(displaySE, pd.se, HPStaminaDisplayerScript.STAT_SCROLL_SPEED);
        if (displaySECooldown > 0)
        {
            displaySECooldown -= Time.deltaTime;
        }
        else
        {
            displaySECooldown = 0;
        }

        if (displaySECooldown > 0 || (HPStaminaDisplayerScript.HighlightActive() && highlightSE))
        {
            textNumber.text = "<color=#c000c0>" + Mathf.Round(displaySE) + "/<size=" + MainManager.Instance.fontSize / 2 + ">" + pd.maxSE + "</size></color>";
        }
        else
        {
            textNumber.text = Mathf.Round(displaySE) + "/<size=" + MainManager.Instance.fontSize / 2 + ">" + pd.maxSE + "</size>";
        }
    }
}
