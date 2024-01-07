using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleMoveNamePopupScript : TextDisplayer
{
    public float width;
    public Image baseBox;

    Vector3 targetPos;
    public RectTransform rectTransform;

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

        targetPos = Vector3.up * 70 + Vector3.left * (width / 2);
        rectTransform.anchoredPosition = Vector3.up * 70 + Vector3.right * width;

        //this is going to cause stupid dependencies (may lead to the box size being wrong again :/)
        base.SetTextNoFormat(text);
    }
    public override void SetText(string text, string[] vars, bool complete = true, bool forceOpaque = true)
    {
        //textMesh = GetComponentInChildren<TMPro.TMP_Text>();
        //baseBox = GetComponentInChildren<Image>();

        base.SetText(text, vars, complete, forceOpaque);
        Canvas.ForceUpdateCanvases();
        RecalculateBoxSize();

        targetPos = Vector3.up * 70 + Vector3.left * (width / 2);
        rectTransform.anchoredPosition = Vector3.up * 70 + Vector3.right * width;

        //this is going to cause stupid dependencies (may lead to the box size being wrong again :/)
        base.SetText(text, vars, complete, forceOpaque);
    }
    public override void SetText(string text, bool complete = true, bool forceOpaque = true)
    {
        //textMesh = GetComponentInChildren<TMPro.TMP_Text>();
        //baseBox = GetComponentInChildren<Image>();

        base.SetText(text, complete, forceOpaque);
        Canvas.ForceUpdateCanvases();
        RecalculateBoxSize();

        targetPos = Vector3.up * 70 + Vector3.left * (width / 2);
        rectTransform.anchoredPosition = Vector3.up * 70 + Vector3.right * width;

        //this is going to cause stupid dependencies (may lead to the box size being wrong again :/)
        base.SetText(text, complete, forceOpaque);
    }

    public void RecalculateBoxSize()
    {
        width = Mathf.Min(textMesh.GetRenderedValues()[0] + 10, 550) + 50;

        textMesh.rectTransform.sizeDelta = new Vector2(width, textMesh.GetRenderedValues()[1] + 10); //baseBox.rectTransform.sizeDelta.y);
        //do it again for good measure (actually just make sure the height value fixes itself)
        textMesh.rectTransform.sizeDelta = new Vector2(width, textMesh.GetRenderedValues()[1] + 10); //baseBox.rectTransform.sizeDelta.y);

        baseBox.rectTransform.sizeDelta = new Vector2(width + 30, textMesh.GetRenderedValues()[1] + 20); //baseBox.rectTransform.sizeDelta.y);

    }

    public void Update()
    {
        rectTransform.anchoredPosition = MainManager.EasingQuadraticTime(rectTransform.anchoredPosition, targetPos, 7000);
    }
}
