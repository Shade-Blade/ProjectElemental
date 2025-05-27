using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class SEDisplayerScript : MonoBehaviour
{
    PlayerData pd;

    public TMPro.TMP_Text textNumber;
    public RectTransform rt;
    public Image SEImage;
    public Image barImage;

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
        displaySE = pd.se;

        string seText;
        if (displaySECooldown > 0 || (HPStaminaDisplayerScript.HighlightActive() && highlightSE))
        {
            seText = "<color=#8080ff>" + Mathf.Round(displaySE) + "</color>";
            textNumber.text = "<color=#8080ff>" + Mathf.Round(displaySE) + "</color><space=0.1em>/<space=0.1em><size=60%>" + pd.maxSE + "</size>";
        }
        else
        {
            seText = "" + Mathf.Round(displaySE);
            textNumber.text = "" + Mathf.Round(displaySE) + "<space=0.1em>/<space=0.1em><size=60%>" + pd.maxSE + "</size>";
        }
        int deltaMaxSE = 0;
        if (MainManager.Instance.worldMode == MainManager.WorldMode.Battle)
        {
            deltaMaxSE = BattleControl.Instance.GetPartyCumulativeEffectPotency(-1, Effect.EffectType.MaxSEBoost) - BattleControl.Instance.GetPartyCumulativeEffectPotency(-1, Effect.EffectType.MaxSEReduction);
        }
        if (deltaMaxSE != 0)
        {
            if (deltaMaxSE > 0)
            {
                seText += "<space=0.1em>/<space=0.1em><size=60%><color=#80ff80>" + pd.maxSE + "</color></size>";
            }
            else
            {
                seText += "<space=0.1em>/<space=0.1em><size=60%><color=#ff8080>" + pd.maxSE + "</color></size>";
            }
        }
        else
        {
            seText += "<space=0.1em>/<space=0.1em><size=60%>" + pd.maxSE + "</size>";
        }
        textNumber.text = seText;

        if (pd.maxSE == 0)
        {
            barImage.rectTransform.anchoredPosition = Vector2.zero;
        }
        else
        {
            barImage.rectTransform.anchoredPosition = Vector2.left * (1 - (displaySE / pd.maxSE)) * (rt.sizeDelta.x - 15 - 27.5f);
        }
    }

    public void SetPosition()
    {
        rt.anchoredPosition = new Vector3(216, -79f, 0);
    }
    public void SetPositionBattle()
    {
        rt.anchoredPosition = new Vector3(216, -106f, 0);
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

        string seText;
        if (displaySECooldown > 0 || (HPStaminaDisplayerScript.HighlightActive() && highlightSE))
        {
            seText = "<color=#8080ff>" + Mathf.Round(displaySE) + "</color>";
            textNumber.text = "<color=#8080ff>" + Mathf.Round(displaySE) + "</color><space=0.1em>/<space=0.1em><size=60%>" + pd.maxSE + "</size>";
        }
        else
        {
            seText = "" + Mathf.Round(displaySE);
            textNumber.text = "" + Mathf.Round(displaySE) + "<space=0.1em>/<space=0.1em><size=60%>" + pd.maxSE + "</size>";
        }
        int deltaMaxSE = 0;
        if (MainManager.Instance.worldMode == MainManager.WorldMode.Battle)
        {
            deltaMaxSE = BattleControl.Instance.GetPartyCumulativeEffectPotency(-1, Effect.EffectType.MaxSEBoost) - BattleControl.Instance.GetPartyCumulativeEffectPotency(-1, Effect.EffectType.MaxSEReduction);
        }
        if (deltaMaxSE != 0)
        {
            if (deltaMaxSE > 0)
            {
                seText += "<space=0.1em>/<space=0.1em><size=60%><color=#80ff80>" + pd.maxSE + "</color></size>";
            }
            else
            {
                seText += "<space=0.1em>/<space=0.1em><size=60%><color=#ff8080>" + pd.maxSE + "</color></size>";
            }
        }
        else
        {
            seText += "<space=0.1em>/<space=0.1em><size=60%>" + pd.maxSE + "</size>";
        }
        textNumber.text = seText;

        if (pd.maxSE == 0)
        {
            barImage.rectTransform.anchoredPosition = Vector2.zero;
        }
        else
        {
            barImage.rectTransform.anchoredPosition = Vector2.left * (1 - (displaySE / pd.maxSE)) * (rt.sizeDelta.x - 15 - 27.5f);
        }
    }
}
