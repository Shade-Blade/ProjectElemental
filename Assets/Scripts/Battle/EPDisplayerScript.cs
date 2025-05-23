using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EPDisplayerScript : MonoBehaviour
{
    PlayerData pd;

    public TMPro.TMP_Text textNumber;
    public Image backImage;
    public Image epImage;

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
        textNumber.text = pd.ep + "/<size=" + MainManager.Instance.fontSize / 2 + ">" + pd.maxEP + "</size>";
        displayEP = pd.ep;
    }
    
    public void SetPosition()
    {
        backImage.rectTransform.anchoredPosition = new Vector3(-220, -30, 0);
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

        if (displayEPCooldown > 0 || (HPStaminaDisplayerScript.HighlightActive() && highlightEP))
        {
            textNumber.text = "<color=#c0c000>" + Mathf.Round(displayEP) + "</color>/<size=" + MainManager.Instance.fontSize / 2 + ">" + pd.maxEP + "</size>";
        }
        else
        {
            textNumber.text = Mathf.Round(displayEP) + "/<size=" + MainManager.Instance.fontSize / 2 + ">" + pd.maxEP + "</size>";
        }
    }
}
