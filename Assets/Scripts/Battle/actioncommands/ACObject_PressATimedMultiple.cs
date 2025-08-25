using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ACObject_PressATimedMultiple : MonoBehaviour
{
    public Image fullBar;
    public Image[] emptyBars;
    public TextDisplayer controlHint;

    public float[] completionAmounts;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetValues(float completion, float[] completionAmounts, int successes, ActionCommand.AC_State state)
    {
        controlHint.SetText("<button,a>", true, true);
        for (int i = 0; i < emptyBars.Length; i++)
        {
            emptyBars[i].rectTransform.sizeDelta = Vector2.zero;
        }

        for (int i = 0; i < completionAmounts.Length; i++)
        {
            float width = (Mathf.Clamp01((completionAmounts[i] - completion) / completionAmounts[i])) * (200 * (1 + completionAmounts.Length)) + 100;
            emptyBars[i].rectTransform.sizeDelta = Vector2.right * width + Vector2.up * width;

            emptyBars[i].color = new Color(1, 1, 1, Mathf.Clamp01((completionAmounts[i] - completion) / completionAmounts[i]) * 0.75f);
        }

        if (state == ActionCommand.AC_State.Complete)
        {
            fullBar.color = (successes == completionAmounts.Length) ? ActionCommand.COLOR_ON : ActionCommand.COLOR_OFF;
        }
    }
}
