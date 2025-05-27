using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattlePopupScript : TextDisplayer
{
    public Image baseBox;
    public Image borderBox;

    public override void Start()
    {
        //textMesh = GetComponentInChildren<TMPro.TMP_Text>();
        //baseBox = GetComponentInChildren<Image>();

        Canvas.ForceUpdateCanvases();

        //resize box properly        
        //baseBox.rectTransform.sizeDelta = new Vector2(1, 1);
        RecalculateBoxSize();
    }
    public override void SetTextNoFormat(string text)
    {
        //textMesh = GetComponentInChildren<TMPro.TMP_Text>();
        //baseBox = GetComponentInChildren<Image>();

        base.SetTextNoFormat(text);
        Canvas.ForceUpdateCanvases();
        RecalculateBoxSize();

        //this is going to cause stupid dependencies (may lead to the box size being wrong again :/)
        base.SetTextNoFormat(text);
    }
    public override void SetText(string text, string[] vars, bool complete = true, bool forceOpaque = true, float fontSize = -1)
    {
        //textMesh = GetComponentInChildren<TMPro.TMP_Text>();
        //baseBox = GetComponentInChildren<Image>();

        base.SetText(text, vars, complete, forceOpaque, -2);
        Canvas.ForceUpdateCanvases();
        RecalculateBoxSize();

        //this is going to cause stupid dependencies (may lead to the box size being wrong again :/)
        base.SetText(text, vars, complete, forceOpaque, fontSize);
    }
    public override void SetText(string text, bool complete = true, bool forceOpaque = true, float fontSize = -1)
    {
        //textMesh = GetComponentInChildren<TMPro.TMP_Text>();
        //baseBox = GetComponentInChildren<Image>();

        base.SetText(text, complete, forceOpaque, -2);
        Canvas.ForceUpdateCanvases();
        RecalculateBoxSize();
        
        //this is going to cause stupid dependencies (may lead to the box size being wrong again :/)
        base.SetText(text, complete, forceOpaque, fontSize);
    }

    public void RecalculateBoxSize()
    {
        float width = Mathf.Min(textMesh.GetRenderedValues()[0] + 10, 590);

        textMesh.rectTransform.sizeDelta = new Vector2(width, textMesh.GetRenderedValues()[1] + 10); //baseBox.rectTransform.sizeDelta.y);
        //do it again for good measure (actually just make sure the height value fixes itself)
        textMesh.rectTransform.sizeDelta = new Vector2(width, textMesh.GetRenderedValues()[1] + 10); //baseBox.rectTransform.sizeDelta.y);

        baseBox.rectTransform.sizeDelta = new Vector2(width + 20, textMesh.GetRenderedValues()[1] + 20); //baseBox.rectTransform.sizeDelta.y);
        borderBox.rectTransform.sizeDelta = new Vector2(width + 20, textMesh.GetRenderedValues()[1] + 20); //baseBox.rectTransform.sizeDelta.y);
    }
}
