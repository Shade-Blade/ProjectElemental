using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ACObject_PressATimed : MonoBehaviour
{
    public Image fullBar;
    public Image emptyBar;
    public TextDisplayer controlHint;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetValues(float completion, bool success, ActionCommand.AC_State state)
    {
        controlHint.SetText("<button,a>", true, true);
        float width = (Mathf.Clamp01(1 - completion)) * 200 + 100;
        emptyBar.rectTransform.sizeDelta = Vector2.right * width + Vector2.up * width;

        if (state == ActionCommand.AC_State.Complete)
        {
            fullBar.color = success ? new Color(0.75f, 1, 1) : new Color(0.5f, 0f, 0);
        }
    }
}
