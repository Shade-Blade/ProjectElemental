using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusIconScript : MonoBehaviour
{
    public Effect status;
    public SpriteRenderer sprite;
    public TMPro.TMP_Text potencyText;
    public TMPro.TMP_Text durationText;
    public const float VOFFSET = 0.45f;

    public void Setup(Effect s)
    {
        status = s;
        sprite.sprite = MainManager.Instance.effectSprites[(int)s.effect];

        if (Effect.GetEffectClass(s.effect) != Effect.EffectClass.Status)
        {
            potencyText.text = s.potency.ToString();
        }
        else
        {
            if (s.potency > 1)
            {
                potencyText.text = s.potency.ToString();
            }
            else
            {
                potencyText.text = "";
            }
        }

        if (s.duration < 255)
        {
            durationText.text = s.duration.ToString();
        } else
        {
            if (Effect.GetEffectClass(s.effect) != Effect.EffectClass.Token && Effect.GetEffectClass(s.effect) != Effect.EffectClass.Static)
            {
                durationText.text = "X";
            } else
            {
                durationText.text = "";
            }
        }
    }
}
