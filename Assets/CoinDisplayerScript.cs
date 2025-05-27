using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

public class CoinDisplayerScript : MonoBehaviour
{
    PlayerData pd;

    public TMPro.TMP_Text textNumber;
    public RectTransform rt;
    public Image epImage;
    public Image barImage;

    public float displayCoins;
    public float displayCoinsCooldown;
    public bool highlightCoins;

    // Start is called before the first frame update
    void Start()
    {
        //backImage = GetComponentInChildren<Image>();
    }

    public void Setup(PlayerData p_pd)
    {
        pd = p_pd;
        //backImage = GetComponentInChildren<Image>();
        epImage.sprite = MainManager.Instance.commonSprites[(int)Text_CommonSprite.SpriteType.Coin];
        displayCoins = pd.coins;
        textNumber.text = "" + Mathf.Round(displayCoins) + "";

        barImage.rectTransform.anchoredPosition = Vector2.left * (1 - (pd.coins / 999f)) * (rt.sizeDelta.x - 15 - 27.5f);
    }

    public void SetPosition()
    {
        rt.anchoredPosition = new Vector3(-245, -28, 0);
    }
    public static bool HighlightActive()
    {
        return (int)(Time.time * 3) % 2 == 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (displayCoins != pd.coins)
        {
            displayCoinsCooldown = HPStaminaDisplayerScript.STAT_HIGHLIGHT_MINIMUM;
        }
        displayCoins = MainManager.EasingQuadraticTime(displayCoins, pd.coins, HPStaminaDisplayerScript.STAT_SCROLL_SPEED);
        if (displayCoinsCooldown > 0)
        {
            displayCoinsCooldown -= Time.deltaTime;
        }
        else
        {
            displayCoinsCooldown = 0;
        }

        if (displayCoinsCooldown > 0 || (HighlightActive() && (HPStaminaDisplayerScript.HighlightActive() && highlightCoins)) || displayCoins == 999)
        {
            textNumber.text = "<color=#ffb080>" + Mathf.Round(displayCoins) + "</color>";
        }
        else
        {
            textNumber.text = "" + Mathf.Round(displayCoins) + "";
        }

        barImage.rectTransform.anchoredPosition = Vector2.left * (1 - (displayCoins / 999f)) * (rt.sizeDelta.x - 15 - 27.5f);
    }
}