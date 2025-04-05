using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemDisplayerScript : MonoBehaviour
{
    PlayerData pd;

    public TMPro.TMP_Text textNumber;
    public Image backImage;
    public Image epImage;

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
        textNumber.text = pd.itemInventory.Count + "/<size=" + MainManager.Instance.fontSize / 2 + ">" + pd.GetMaxInventorySize() + "</size>";
    }

    public void SetPosition()
    {
        backImage.rectTransform.anchoredPosition = new Vector3(-270, 25, 0);
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

        if (displayItemsCooldown > 0 || highlightItems)
        {
            textNumber.text = "<color=#c03000>" + pd.itemInventory.Count + "</color>/<size=" + MainManager.Instance.fontSize / 2 + ">" + pd.GetMaxInventorySize() + "</size>";
        }
        else
        {
            textNumber.text = pd.itemInventory.Count + "/<size=" + MainManager.Instance.fontSize / 2 + ">" + pd.GetMaxInventorySize() + "</size>";
        }
    }
}
