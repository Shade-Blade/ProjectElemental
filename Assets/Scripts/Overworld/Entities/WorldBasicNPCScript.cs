using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldBasicNPCScript : WorldNPCEntity
{
    public int textIndex;
    public bool loop;   //false = keep doing last text, true = loop around to first again

    //Minibubble testing
    
    public MinibubbleScript minibubble;

    public override void WorldUpdate()
    {
        if (interacter.GetActive())
        {
            if (minibubble != null)
            {
                minibubble.superDeleteSignal = true;
            }
        } else
        {
            if (minibubble == null)
            {
                minibubble = MainManager.Instance.MakeMinibubble();
                StartCoroutine(minibubble.CreateText("<TailRealTimeUpdate,true>A", this, null, true));
                //Debug.Log("make minibubble");
            }
        }
        base.WorldUpdate();
    }

    public override IEnumerator InteractCutscene()
    {
        //inefficient
        string[][] testTextFile = new string[wed.talkStrings.Count][];
        for (int i = 0; i < testTextFile.Length; i++)
        {
            testTextFile[i] = new string[1];
            testTextFile[i][0] = FormattedString.ReplaceTextFileShorthand(wed.talkStrings[i]);
        }

        yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, textIndex, this));

        textIndex++;
        if (textIndex >= testTextFile.Length)
        {
            if (loop)
            {
                textIndex = 0;
            } else
            {
                textIndex--;
            }
        }
    }
}
