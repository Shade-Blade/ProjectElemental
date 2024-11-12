using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BestiaryCardScript : MonoBehaviour
{
    public RectTransform rect;

    public Image image;
    public TextDisplayer nameText;
    public TextDisplayer text;

    public float animtime;
    public bool fadeout;

    public void Update()
    {
        if (fadeout)
        {
            rect.anchoredPosition = MainManager.EasingQuadraticTime(rect.anchoredPosition, Vector2.right * 150, 12500);
            if (animtime > 0.15f)
            {
                Destroy(gameObject);
            }
        }
        else
        {
            rect.anchoredPosition = MainManager.EasingQuadraticTime(rect.anchoredPosition, Vector2.left * 125, 12500);
        }
        animtime += Time.deltaTime;
    }

    public void Setup(Sprite sprite, string name, string text)
    {
        this.nameText.SetText(name, true, true);
        this.text.SetText(text, true, true);
        image.sprite = sprite;

        rect.anchoredPosition = Vector2.right * 150;

        animtime = 0;
        fadeout = false;
    }

    public void Fadeout()
    {
        animtime = 0;
        fadeout = true;
    }
}
