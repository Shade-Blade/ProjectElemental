using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ACObject_PressButtonTimed : MonoBehaviour
{
    public Image fullBar;
    public Image emptyBar;

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
        float width = (Mathf.Clamp01(1 - completion)) * 100 + 50;
        emptyBar.rectTransform.sizeDelta = Vector2.right * width + Vector2.up * width;

        if (completion >= 1 && state == ActionCommand.AC_State.Complete)
        {
            fullBar.color = success ? Color.green : Color.red;
        }
    }
}
