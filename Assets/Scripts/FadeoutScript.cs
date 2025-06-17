using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FadeoutScript : MonoBehaviour
{
    Image image;
    public float FadeOutProgress
    {
        get; private set;
    }
    public float defaultFadeTime = 0.25f;

    public void Start()
    {
        FadeOutProgress = 0;
        image = GetComponent<Image>();
        image.color = new Color(0,0,0,FadeOutProgress);
    }

    public IEnumerator FadeToBlack()
    {
        Debug.Log("Fade to black");
        while (FadeOutProgress < 1)
        {
            FadeOutProgress += Time.deltaTime / defaultFadeTime;
            image.color = new Color(0, 0, 0, FadeOutProgress);
            yield return null;
        }
        FadeOutProgress = 1;
        image.color = new Color(0, 0, 0, 1);
    }

    public void SnapFade(float f)
    {
        FadeOutProgress = f;
        image.color = new Color(0, 0, 0, FadeOutProgress);
    }

    public IEnumerator UnfadeToBlack()
    {
        Debug.Log("Unfade to black");
        while (FadeOutProgress > 0)
        {
            FadeOutProgress -= Time.deltaTime / defaultFadeTime;
            image.color = new Color(0, 0, 0, FadeOutProgress);
            yield return null;
        }
        FadeOutProgress = 0;
        image.color = new Color(0, 0, 0, 0);
    }

    public IEnumerator FadeToWhite()
    {
        Debug.Log("Fade to white");
        while (FadeOutProgress < 1)
        {
            FadeOutProgress += Time.deltaTime / defaultFadeTime;
            image.color = new Color(1, 1, 1, FadeOutProgress);
            yield return null;
        }
        FadeOutProgress = 1;
        image.color = new Color(1, 1, 1, 1);
    }

    public IEnumerator UnfadeToWhite()
    {
        Debug.Log("Unfade to white");
        while (FadeOutProgress > 0)
        {
            FadeOutProgress -= Time.deltaTime / defaultFadeTime;
            image.color = new Color(1, 1, 1, FadeOutProgress);
            yield return null;
        }
        FadeOutProgress = 0;
        image.color = new Color(1, 1, 1, 0);
    }

    private void Update()
    {
        image.rectTransform.sizeDelta = Vector2.right * 800 + Vector2.up * 800 * (MainManager.Instance.SuperCanvas.GetComponent<RectTransform>().rect.height / (MainManager.Instance.SuperCanvas.GetComponent<RectTransform>().rect.width + 0f));
    }
}
