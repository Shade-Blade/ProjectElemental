using UnityEngine;

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
        string newString = "<descriptionnoticecolor><wavy>" + p.text + "</wavy></color>";
        tdisplayer.SetText(newString, true);
        wavy = true;
    }
}
