using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectPointerScript : MonoBehaviour
{
    public TextDisplayer text;
    public GameObject textbox;

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
}
