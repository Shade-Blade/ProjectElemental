using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SEDisplayerScript : MonoBehaviour
{
    PlayerData pd;

    public TMPro.TMP_Text textNumber;
    public Image backImage;
    public Image SEImage;

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
    }

    public void SetPosition()
    {
        backImage.rectTransform.anchoredPosition = new Vector3(-60f, -25f, 0);
    }

    // Update is called once per frame
    void Update()
    {
        textNumber.text = pd.se + "/<size=" + MainManager.Instance.fontSize / 2 + ">" + pd.maxSE + "</size>";
    }
}
