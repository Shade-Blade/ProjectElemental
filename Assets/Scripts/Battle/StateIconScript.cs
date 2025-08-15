using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateIconScript : MonoBehaviour
{
    public BattleHelper.EntityState state;
    public SpriteRenderer sprite;
    public int potency;
    public int duration;
    public TMPro.TMP_Text potencyText;
    public TMPro.TMP_Text durationText;
    public const float VOFFSET = 0.3f;

    public void Setup(BattleHelper.EntityState p_state, int potency = int.MinValue, int duration = int.MinValue)
    {
        state = p_state;
        sprite.sprite = MainManager.Instance.stateSprites[(int)state - 1];

        this.potency = potency;
        this.duration = duration;

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

    public void OnMouseOver()
    {
        HoverTextMasterScript.Instance.SetHoverText(BattlePopup.GetStatePopup(state, potency, duration));
    }
}
