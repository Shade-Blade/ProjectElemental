using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateIconScript : MonoBehaviour
{
    public BattleHelper.EntityState state;
    public SpriteRenderer sprite;
    public TMPro.TMP_Text potencyText;
    public TMPro.TMP_Text durationText;
    public const float VOFFSET = 0.35f;

    public void Setup(BattleHelper.EntityState p_state, int potency = int.MinValue, int duration = int.MinValue)
    {
        state = p_state;
        sprite.sprite = MainManager.Instance.stateSprites[(int)state];

        if (potency != int.MinValue)
        {
            potencyText.text = potency.ToString();
        } else
        {
            potencyText.text = "";
        }

        if (duration != int.MinValue)
        {
            durationText.text = duration.ToString();
        }
        else
        {
            durationText.text = "";
        }
    }
}
