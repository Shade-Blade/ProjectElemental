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
    public Image backImage;
    public Image epImage;

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
        textNumber.text = pd.coins + "";

        displayCoins = pd.coins;
    }

    public void SetPosition()
    {
        backImage.rectTransform.anchoredPosition = new Vector3(-170, 25, 0);
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

        if (displayCoinsCooldown > 0 || (HPStaminaDisplayerScript.HighlightActive() && highlightCoins))
        {
            textNumber.text = "<color=#c06000>"+Mathf.Round(displayCoins) + "</color>";
        }
        else
        {
            textNumber.text = Mathf.Round(displayCoins) + "";
        }
    }
}