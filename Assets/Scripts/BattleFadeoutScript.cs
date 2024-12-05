using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleFadeoutScript : MonoBehaviour
{
    Image image;
    public float FadeOutProgress
    {
        get; private set;
    }
    public float defaultFadeTime = 0.4f;

    private MaterialPropertyBlock propertyBlockA;

    public const float maxZoom = 2.5f;

    public float easingPower = 1f;

    public void Start()
    {
        FadeOutProgress = 0;
        image = GetComponent<Image>();
        image.color = new Color(0, 0, 0, FadeOutProgress);
        propertyBlockA = new MaterialPropertyBlock();
    }

    float Easing(float f, float p)
    {
        return 1 - Mathf.Pow(1 - f, p);
    }

    public IEnumerator FadeToBlack(Color color, Vector2 screenPos)
    {
        Debug.Log("Fade to black");
        Shader.SetGlobalVector("_BattleFadeLocation", screenPos);
        while (FadeOutProgress < 1)
        {
            FadeOutProgress += Time.deltaTime / defaultFadeTime;
            image.color = new Color(color.r, color.g, color.b, FadeOutProgress);

            //This is a bad way of doing this, but I have no choice
            //Too lazy to make a real animation for this, so I guess I'll ram a square peg in a round hole
            Shader.SetGlobalFloat("_BattleFadeProgress", Easing((1 - FadeOutProgress), easingPower));

            //propertyBlockA.SetFloat("_Cutoff", 0.8f * Easing(FadeOutProgress, easingPower));
            //image.setpro
            yield return null;
        }
        Shader.SetGlobalFloat("_BattleFadeProgress", 0);
        FadeOutProgress = 1;
        image.color = new Color(color.r, color.g, color.b, 1);
    }

    public IEnumerator UnfadeToBlack(Color color, Vector2 screenPos)
    {
        Debug.Log("Fade to normal");
        Shader.SetGlobalVector("_BattleFadeLocation", screenPos);
        while (FadeOutProgress > 0)
        {
            FadeOutProgress -= Time.deltaTime / defaultFadeTime;
            image.color = new Color(color.r, color.g, color.b, FadeOutProgress);

            //This is a bad way of doing this, but I have no choice
            //Too lazy to make a real animation for this, so I guess I'll ram a square peg in a round hole
            Shader.SetGlobalFloat("_BattleFadeProgress", Easing((1 - FadeOutProgress), easingPower));

            yield return null;
        }
        Shader.SetGlobalFloat("_BattleFadeProgress", 1);
        FadeOutProgress = 0;
        image.color = new Color(color.r, color.g, color.b, 0);
    }
}
