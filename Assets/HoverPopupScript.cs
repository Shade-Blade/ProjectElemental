using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HoverPopupScript : TextDisplayer
{
    public RectTransform rectTransform;
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

    public void Update()
    {
        PositionUpdate();
    }

    public void PositionUpdate()
    {
        if (MainManager.Instance.RealMousePos().y - (baseBox.rectTransform.sizeDelta.y) < 20)
        {
            //hover popup
            if ((baseBox.rectTransform.sizeDelta.x) + MainManager.Instance.RealMousePos().x > MainManager.CanvasWidth() - 20)
            {
                rectTransform.anchoredPosition = MainManager.Instance.RealMousePos() - (baseBox.rectTransform.sizeDelta.x * Vector2.right * 0.5f) + (baseBox.rectTransform.sizeDelta.y * Vector2.up * 0.5f) + new Vector2(-20, 10);
            }
            else
            {
                rectTransform.anchoredPosition = MainManager.Instance.RealMousePos() + (baseBox.rectTransform.sizeDelta.x * Vector2.right * 0.5f) + (baseBox.rectTransform.sizeDelta.y * Vector2.up * 0.5f) + new Vector2(20, 10);
            }
        }
        else
        {
            if ((baseBox.rectTransform.sizeDelta.x) + MainManager.Instance.RealMousePos().x > MainManager.CanvasWidth() - 20)
            {
                rectTransform.anchoredPosition = MainManager.Instance.RealMousePos() - (baseBox.rectTransform.sizeDelta.x * Vector2.right * 0.5f) + (baseBox.rectTransform.sizeDelta.y * Vector2.down * 0.5f) + new Vector2(-20, -10);
            }
            else
            {
                rectTransform.anchoredPosition = MainManager.Instance.RealMousePos() + (baseBox.rectTransform.sizeDelta.x * Vector2.right * 0.5f) + (baseBox.rectTransform.sizeDelta.y * Vector2.down * 0.5f) + new Vector2(20, -10);
            }
        }
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
        base.SetText(text, vars, complete, forceOpaque, -2);
    }
    public override void SetText(string text, bool complete = true, bool forceOpaque = true, float fontSize = -1)
    {
        //textMesh = GetComponentInChildren<TMPro.TMP_Text>();
        //baseBox = GetComponentInChildren<Image>();

        base.SetText(text, complete, forceOpaque, -2);
        Canvas.ForceUpdateCanvases();
        RecalculateBoxSize();

        //this is going to cause stupid dependencies (may lead to the box size being wrong again :/)
        base.SetText(text, complete, forceOpaque, -2);
    }

    public void RecalculateBoxSize()
    {
        float width = Mathf.Min(textMesh.GetRenderedValues()[0] + 3, 290);

        textMesh.rectTransform.sizeDelta = new Vector2(width, textMesh.GetRenderedValues()[1] + 0); //baseBox.rectTransform.sizeDelta.y);
        //do it again for good measure (actually just make sure the height value fixes itself)
        textMesh.rectTransform.sizeDelta = new Vector2(width, textMesh.GetRenderedValues()[1] + 0); //baseBox.rectTransform.sizeDelta.y);

        baseBox.rectTransform.sizeDelta = new Vector2(width + 20, textMesh.GetRenderedValues()[1] + 20); //baseBox.rectTransform.sizeDelta.y);
        borderBox.rectTransform.sizeDelta = new Vector2(width + 20, textMesh.GetRenderedValues()[1] + 20); //baseBox.rectTransform.sizeDelta.y);
    }
}
