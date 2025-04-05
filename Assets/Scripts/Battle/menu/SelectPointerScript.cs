using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectPointerScript : MonoBehaviour
{
    public TextDisplayer text;
    public GameObject textbox;

    public Vector3 targetPos;

    public TextDisplayer GetText()
    {
        if (text == null)
        {
            text = GetComponent<TextDisplayer>();
        }

        return text;
    }

    public void SetText(string p_text)
    {
        if (p_text == null || p_text.Length == 0)
        {
            Destroy(textbox);
            text = null;
            return;
        }

        text.SetText(p_text, true, true);
    }

    public void RepositionTextBelow(bool set)
    {
        if (textbox != null)
        {
            if (set)
            {
                textbox.transform.localPosition = Vector3.down * 0.25f;
            }
            else
            {
                textbox.transform.localPosition = Vector3.up * 0.3f;
            }
        }
    }

    public void Update()
    {
        transform.localPosition = MainManager.EasingQuadraticTime(transform.localPosition, targetPos, 600);
    }
}
