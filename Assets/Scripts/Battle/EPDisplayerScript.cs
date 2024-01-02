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
    }
    
    public void SetPosition()
    {
        backImage.rectTransform.anchoredPosition = new Vector3(-170, -25, 0);
    }

    // Update is called once per frame
    void Update()
    {
        textNumber.text = pd.ep + "/<size=" + MainManager.Instance.fontSize / 2 + ">" + pd.maxEP + "</size>";
    }
}
