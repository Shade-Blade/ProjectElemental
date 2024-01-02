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

    // Start is called before the first frame update
    void Start()
    {
        //backImage = GetComponentInChildren<Image>();
    }

    public void Setup(PlayerData p_pd)
    {
        pd = p_pd;
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
        textNumber.text = pd.itemInventory.Count + "/<size=" + MainManager.Instance.fontSize / 2 + ">" + pd.GetMaxInventorySize() + "</size>";
    }
}
