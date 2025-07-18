using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ACObject_MashBar : MonoBehaviour
{
    public Image fullBar;
    public Image emptyBar;
    public Image fullBall;
    public TextDisplayer controlHint;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetValues(float completion, bool ball)
    {
        controlHint.SetText("Mash <button,left>", true, true);
        float width = completion * 150;

        fullBar.rectTransform.anchoredPosition = (width/2) * Vector2.right;
        fullBar.rectTransform.sizeDelta = Vector2.right * width + Vector2.up * 18;

        fullBall.color = ball ? new Color(0.75f, 1, 1) : new Color(0f, 0f, 0.5f);
    }
}
