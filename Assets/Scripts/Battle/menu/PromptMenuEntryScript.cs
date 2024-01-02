using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PromptMenuEntryScript : MonoBehaviour
{
    public TMPro.TMP_Text textMesh;
    public TextDisplayer tdisplayer;
    PromptMenuEntry p;
    public bool wavy = false;

    public void Setup(BoxMenuEntry p_p) //ha
    {        
        p = (PromptMenuEntry)p_p;
        tdisplayer.SetText(p.text,true);
    }

    public void ResetText()
    {
        tdisplayer.SetText(p.text, true);
        wavy = false;
    }

    public void SetTextWavy()
    {
        //Debug.Log(p.text);
        string newString = "<color,red><wavy>" + p.text + "</wavy></color>";
        tdisplayer.SetText(newString, true);
        wavy = true;
    }
}
