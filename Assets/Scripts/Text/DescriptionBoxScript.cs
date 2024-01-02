using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//inherit from text displayer so that I can use the special tags and stuff
public class DescriptionBoxScript : TextDisplayer
{
    public Image baseBox;

    public override void Start()
    {
        //textMesh = GetComponentInChildren<TMPro.TMP_Text>();
        //baseBox = GetComponentInChildren<Image>();

        baseBox.enabled = inputString != null && inputString.Length != 0;
        //Debug.Log((inputString != null && inputString.Length != 0) + "1");
    }
    public override void SetTextNoFormat(string text)
    {
        //textMesh = GetComponentInChildren<TMPro.TMP_Text>();
        //baseBox = GetComponentInChildren<Image>();

        baseBox.enabled = text != null && text.Length != 0;

        //Debug.Log((text != null && text.Length != 0) + "2");

        Canvas.ForceUpdateCanvases();

        base.SetTextNoFormat(text);
    }
    public override void SetText(string text, bool complete = true, bool forceOpaque = true)
    {
        //Debug.Log(text);
        //textMesh = GetComponentInChildren<TMPro.TMP_Text>();
        //baseBox = GetComponentInChildren<Image>();

        baseBox.enabled = text != null && text.Length != 0;

        //Debug.Log((text != null && text.Length != 0) + "3");

        Canvas.ForceUpdateCanvases();

        base.SetText(text, complete, forceOpaque);
    }
}
