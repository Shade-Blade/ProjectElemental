using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//use this for most text boxes (This handles more complex text switching that TextboxScript can't do)
public class TextManager : MonoBehaviour
{
    //Handled on a TextboxScript level
    //ITextSpeaker speaker;

    public TextboxScript textboxScript;

    string text;

    string[][] textFile;
    string[] vars;

    List<string> stringHistory;

    List<string> textSet;

    public void SeeTag(object sender, TextDisplayer.ScrollEventArgs scrollEventArgs)
    {
        TagEntry t = scrollEventArgs.tag;
        string[] args = t.args;
        Debug.Log(t);
        switch (t.tag)
        {
            case TagEntry.TextTag.Goto:
                int y = 0;
                bool check = int.TryParse(args[0], out y);

                int x = 0;
                if (args[0].Contains("arg"))
                {
                    string gts = FormattedString.ParseArg(textboxScript.GetMenuResultString(), args[0]);
                    //Debug.Log("Try to parse with textboxScript.GetMenuResultString() = " + textboxScript.GetMenuResultString());
                    int.TryParse(gts, out y);
                } else
                {
                    x = args.Length > 1 ? int.Parse(args[1]) : 0;
                }
                ReplaceGoTo(textboxScript.currLine, y, x);
                break;
            case TagEntry.TextTag.Branch:
                bool doBranch = FormattedString.ParseBranchCondition(t, textboxScript.GetMenuResultString());
                if (doBranch)
                {
                    int[] branchDest = FormattedString.GetBranchDestination(t);
                    ReplaceGoTo(textboxScript.currLine, branchDest[0], branchDest.Length > 1 ? branchDest[1] : 0);
                }
                break;
        }
    }

    public IEnumerator ShowText(string[][] p_textFile, int y, ITextSpeaker speaker = null, string[] vars = null)
    {
        yield return StartCoroutine(ShowText(p_textFile, y, 0, speaker, vars));
    }

    public IEnumerator ShowText(string[][] p_textFile, int y, int x, ITextSpeaker speaker = null, string[] vars = null)
    {
        MainManager.Instance.lastTextboxMenuResult = null;
        textFile = (string[][])(p_textFile.Clone());

        if (y > p_textFile.Length - 1 || p_textFile[y] == null || x > p_textFile[y].Length - 1)
        {
            text = "<color,red>Invalid text: " + y + ", " + x + "</color>";
        } else
        {
            text = p_textFile[y][x];
        }

        this.vars = vars;

        //Debug.Log("tm " + vars);

        //string[] tempLines = FormattedString.ParseByTags(text, true, TagEntry.TextTag.NextNew);

        //textboxScript.lines.Add("");

        MainManager.Instance.lastSpeaker = speaker;
        textboxScript.seeSpecialTag += SeeTag;
        StartCoroutine(textboxScript.CreateText(text, speaker, vars));

        while (!textboxScript.textDone)
        {

            yield return null;
        }

        MainManager.Instance.lastTextboxMenuResult = new string(textboxScript.GetMenuResultString());   //probably want to not keep anything of textmanager directly in memory to avoid problems with garbage collector (not sure if this makes sense)?
        textboxScript.seeSpecialTag -= SeeTag;
    }

    public void ReplaceGoTo(int gotoLine, int targetLineY, int targetLineX)
    {
        string tempNewLines = textFile[targetLineY][targetLineX];
        
        string[] tempFullLines = FormattedString.SplitByTags(tempNewLines, true, TagEntry.TextTag.Next, TagEntry.TextTag.End, TagEntry.TextTag.CondEnd);
        string[] tempLines = FormattedString.SplitByTags(tempNewLines, false, TagEntry.TextTag.Next, TagEntry.TextTag.End, TagEntry.TextTag.CondEnd);
        

        List<bool> newNSL = new List<bool>();
        for (int i = 0; i < tempFullLines.Length; i++)
        {
            TagEntry t;
            if (TagEntry.TryParse(tempFullLines[i], out t))
            {
                if (t.tag != TagEntry.TextTag.Next && t.tag != TagEntry.TextTag.End && t.tag != TagEntry.TextTag.CondEnd)
                {
                    continue;
                }

                bool test = false;
                if (t.tag == TagEntry.TextTag.CondEnd)
                {
                    test = FormattedString.ParseBranchCondition(t);
                }
                else
                {
                    if (t.args.Length > 0)
                    {
                        test = bool.Parse(t.args[0]);
                    }
                }
                if (t.tag == TagEntry.TextTag.End)
                {
                    test = true;
                }
                newNSL.Add(test);
            }
        }
        newNSL.Add(false); //to avoid index errors
        
        //leaves last index gotoLine - 1
        while (textboxScript.lines.Count > gotoLine)
        {
            textboxScript.lines.RemoveAt(textboxScript.lines.Count - 1);
        }
        while (textboxScript.nextSkipList.Count > gotoLine)
        {
            textboxScript.nextSkipList.RemoveAt(textboxScript.nextSkipList.Count - 1);
        }
        //textboxScript.lines.RemoveAt(gotoLine);

        
        //add in the nextSkip bools
        int nslIndex = textboxScript.currLine;
        for (int i = 0; i < newNSL.Count; i++)
        {
            textboxScript.nextSkipList.Add(newNSL[i]);
        }

        
        for (int i = 0; i < tempLines.Length; i++)
        {
            textboxScript.lines.Add(tempLines[i]);
        }

        //comment out for debugging (*this should technically not change anything)
        Cut(textboxScript.noReturn);

        textboxScript.ResetText();
    }

    //cut out text you can't go back to
    //(not necessary but does reduce lag very slightly)
    //(but lag would probably only appear if you open a ton of text boxes or go in a loop)
    public void Cut(int amount)
    {
        //shift these indices back
        textboxScript.currLine -= amount;
        textboxScript.latestLine -= amount;
        textboxScript.noReturn -= amount;

        //then cut stuff out of the lists
        for (int i = 0; i < amount; i++)
        {
            textboxScript.lines.RemoveAt(0);
            textboxScript.nextSkipList.RemoveAt(0);
        }
    }

    public ITextSpeaker GetSpeaker()
    {
        return textboxScript.GetSpeaker();
    }
}
