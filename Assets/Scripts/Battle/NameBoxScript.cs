using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//inherit from text displayer so that I can use the special tags and stuff
public class NameBoxScript : TextDisplayer
{
    public Image baseBox;
    public Image borderBox;

    public override void Start()
    {
        textMesh = GetComponentInChildren<TMPro.TMP_Text>();
        //baseBox = GetComponentInChildren<Image>();

        Canvas.ForceUpdateCanvases();

        //resize box properly        
        //baseBox.rectTransform.sizeDelta = new Vector2(1, 1);
        baseBox.rectTransform.sizeDelta = new Vector2(textMesh.GetRenderedValues()[0] + 35, 40); //baseBox.rectTransform.sizeDelta.y);
        borderBox.rectTransform.sizeDelta = new Vector2(textMesh.GetRenderedValues()[0] + 35, 40); //baseBox.rectTransform.sizeDelta.y);
    }
    public override void SetTextNoFormat(string text)
    {
        textMesh = GetComponentInChildren<TMPro.TMP_Text>();
        //baseBox = GetComponentInChildren<Image>();

        base.SetTextNoFormat(text);
        Canvas.ForceUpdateCanvases();
        baseBox.rectTransform.sizeDelta = new Vector2(textMesh.GetRenderedValues()[0] + 35, 40); //baseBox.rectTransform.sizeDelta.y);
        borderBox.rectTransform.sizeDelta = new Vector2(textMesh.GetRenderedValues()[0] + 35, 40); //baseBox.rectTransform.sizeDelta.y);
    }
    public override void SetText(string text, bool complete = true, bool forceOpaque = true, float fontSize = -1)
    {
        textMesh = GetComponentInChildren<TMPro.TMP_Text>();
        //baseBox = GetComponentInChildren<Image>();

        base.SetText(text, complete, forceOpaque, fontSize);
        Canvas.ForceUpdateCanvases();
        baseBox.rectTransform.sizeDelta = new Vector2(textMesh.GetRenderedValues()[0] + 35, 40); //baseBox.rectTransform.sizeDelta.y);
        borderBox.rectTransform.sizeDelta = new Vector2(textMesh.GetRenderedValues()[0] + 35, 40); //baseBox.rectTransform.sizeDelta.y);
    }
}
