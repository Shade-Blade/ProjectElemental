using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemDisplayerScript : MonoBehaviour
{
    PlayerData pd;

    public TMPro.TMP_Text textNumber;
    public RectTransform rt;
    public Image epImage;
    public Image barImage;

    public float displayItemsCooldown = 0;
    public bool highlightItems;
    public float lastItems;

    // Start is called before the first frame update
    void Start()
    {
        //backImage = GetComponentInChildren<Image>();
    }

    public void Setup(PlayerData p_pd)
    {
        pd = p_pd;
        lastItems = pd.itemInventory.Count;
        //backImage = GetComponentInChildren<Image>();
        epImage.sprite = MainManager.Instance.commonSprites[(int)Text_CommonSprite.SpriteType.Carrot];
        textNumber.text = "" + pd.itemInventory.Count + "<space=0.1em>/<space=0.1em><size=60%>" + pd.GetMaxInventorySize() + "</size>";

        barImage.rectTransform.anchoredPosition = Vector2.left * (1 - ((0f + pd.itemInventory.Count) / pd.GetMaxInventorySize())) * (rt.sizeDelta.x - 15 - 27.5f);
    }

    public void SetPosition()
    {
        rt.anchoredPosition = new Vector3(-405, -28, 0);
    }
    public static bool HighlightActive()
    {
        return (int)(Time.time * 3) % 2 == 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (lastItems != pd.itemInventory.Count)
        {
            displayItemsCooldown = HPStaminaDisplayerScript.STAT_HIGHLIGHT_MINIMUM;
        }
        if (displayItemsCooldown > 0)
        {
            displayItemsCooldown -= Time.deltaTime;
        }
        else
        {
            displayItemsCooldown = 0;
        }
        lastItems = pd.itemInventory.Count;

        if (displayItemsCooldown > 0 || (HighlightActive() && highlightItems))
        {
            textNumber.text = "<color=#ffa880>" + pd.itemInventory.Count + "</color><space=0.1em>/<space=0.1em><size=60%>" + pd.GetMaxInventorySize() + "</size>";
        }
        else
        {
            textNumber.text = "" + pd.itemInventory.Count + "<space=0.1em>/<space=0.1em><size=60%>" + pd.GetMaxInventorySize() + "</size>";
        }

        barImage.rectTransform.anchoredPosition = Vector2.left * (1 - ((0f + pd.itemInventory.Count) / pd.GetMaxInventorySize())) * (rt.sizeDelta.x - 15 - 27.5f);
    }
}
