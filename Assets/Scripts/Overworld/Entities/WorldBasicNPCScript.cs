using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldBasicNPCScript : WorldNPCEntity
{
    public int textIndex;
    public bool loop;   //false = keep doing last text, true = loop around to first again

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
