using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EPDisplayerScript : MonoBehaviour
{
    PlayerData pd;

    public TMPro.TMP_Text textNumber;
    public RectTransform rt;
    public Image epImage;
    public Image barImage;

    public float displayEP;
    public float displayEPCooldown;
    public bool highlightEP;

    // Start is called before the first frame update
    void Start()
    {
        //backImage = GetComponentInChildren<Image>();
    }

    public void Setup(PlayerData p_pd)
    {
        pd = p_pd;
        //backImage = GetComponentInChildren<Image>();
        epImage.sprite = MainManager.Instance.commonSprites[(int)Text_CommonSprite.SpriteType.EP];
        displayEP = pd.ep;

        string epText;
        if (displayEP <= 6)
        {
            epText = "<color=#808000>" + Mathf.Round(displayEP) + "</color>";
        }
        else
        {
            if (displayEPCooldown > 0 || (HPStaminaDisplayerScript.HighlightActive() && highlightEP))
            {
                epText = "<color=#ffff80>" + Mathf.Round(displayEP) + "</color>";
            }
            else
            {
                epText = "" + Mathf.Round(displayEP);
            }
        }
        int deltaMaxEP = 0;
        if (MainManager.Instance.worldMode == MainManager.WorldMode.Battle)
        {
            deltaMaxEP = BattleControl.Instance.GetPartyCumulativeEffectPotency(-1, Effect.EffectType.MaxEPBoost) - BattleControl.Instance.GetPartyCumulativeEffectPotency(-1, Effect.EffectType.MaxEPReduction);
        }
        if (deltaMaxEP != 0)
        {
            if (deltaMaxEP > 0)
            {
                epText += "<space=0.1em>/<space=0.1em><size=60%><color=#80ff80>" + pd.maxEP + "</color></size>";
            }
            else
            {
                epText += "<space=0.1em>/<space=0.1em><size=60%><color=#ff8080>" + pd.maxEP + "</color></size>";
            }
        }
        else
        {
            epText += "<space=0.1em>/<space=0.1em><size=60%>" + pd.maxEP + "</size>";
        }
        textNumber.text = epText;

        barImage.rectTransform.anchoredPosition = Vector2.left * (1 - (displayEP / pd.maxEP)) * (rt.sizeDelta.x - 15 - 27.5f);
    }
    
    public void SetPosition()
    {
        rt.anchoredPosition = new Vector3(76, -79, 0);
    }
    public void SetPositionBattle()
    {
        rt.anchoredPosition = new Vector3(76, -106, 0);
    }

    // Update is called once per frame
    void Update()
    {
        if (displayEP != pd.ep)
        {
            displayEPCooldown = HPStaminaDisplayerScript.STAT_HIGHLIGHT_MINIMUM;
        }
        displayEP = MainManager.EasingQuadraticTime(displayEP, pd.ep, HPStaminaDisplayerScript.STAT_SCROLL_SPEED);
        if (displayEPCooldown > 0)
        {
            displayEPCooldown -= Time.deltaTime;
        }
        else
        {
            displayEPCooldown = 0;
        }

        string epText;
        if (displayEP <= 6)
        {
            epText = "<color=#808000>" + Mathf.Round(displayEP) + "</color>";
        }
        else
        {
            if (displayEPCooldown > 0 || (HPStaminaDisplayerScript.HighlightActive() && highlightEP))
            {
                epText = "<color=#ffff80>" + Mathf.Round(displayEP) + "</color>";
            }
            else
            {
                epText = "" + Mathf.Round(displayEP);
            }
        }

        int deltaMaxEP = 0;
        if (MainManager.Instance.worldMode == MainManager.WorldMode.Battle)
        {
            deltaMaxEP = BattleControl.Instance.GetPartyCumulativeEffectPotency(-1, Effect.EffectType.MaxEPBoost) - BattleControl.Instance.GetPartyCumulativeEffectPotency(-1, Effect.EffectType.MaxEPReduction);
        }
        if (deltaMaxEP != 0)
        {
            if (deltaMaxEP > 0)
            {
                epText += "<space=0.1em>/<space=0.1em><size=60%><color=#80ff80>" + pd.maxEP + "</color></size>";
            }
            else
            {
                epText += "<space=0.1em>/<space=0.1em><size=60%><color=#ff8080>" + pd.maxEP + "</color></size>";
            }
        } else
        {
            epText += "<space=0.1em>/<space=0.1em><size=60%>" + pd.maxEP + "</size>";
        }
        textNumber.text = epText;

        if (pd.maxEP == 0)
        {
            barImage.rectTransform.anchoredPosition = Vector2.zero;
        }
        else
        {
            barImage.rectTransform.anchoredPosition = Vector2.left * (1 - (displayEP / pd.maxEP)) * (rt.sizeDelta.x - 15 - 27.5f);
        }
    }
}
