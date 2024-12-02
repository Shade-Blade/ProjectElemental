using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SignScript : WorldObject, ITextSpeaker
{
    public string signstring;
    public GameObject unreadIndicator;
    public string globalFlag;

    public bool hasUnread;

    public override void Awake()
    {
        base.Awake();

        if (hasUnread && unreadIndicator != null && (globalFlag != null && globalFlag.Length > 0))
        {
            //attempt to parse
            MainManager.GlobalFlag gf;
            Enum.TryParse(globalFlag, out gf);
            bool flag = MainManager.Instance.GetGlobalFlag(gf);
            if (flag)
            {
                Destroy(unreadIndicator);
            }
        }

        if (!hasUnread && unreadIndicator != null)
        {
            Destroy(unreadIndicator);
        }
    }

    public virtual IEnumerator SignCutscene()
    {
        string[][] testTextFile = new string[4][];
        testTextFile[0] = new string[1];
        testTextFile[0][0] = FormattedString.ReplaceTextFileShorthand(signstring);

        if ((globalFlag != null && globalFlag.Length > 0))
        {
            //attempt to parse
            MainManager.GlobalFlag gf;
            Enum.TryParse(globalFlag, out gf);
            MainManager.Instance.SetGlobalFlag(gf, true);
        }

        if (unreadIndicator != null)
        {
            Destroy(unreadIndicator);
        }

        yield return StartCoroutine(MainManager.Instance.DisplayTextBoxBlocking(testTextFile, 0, this));
    }

    public string RequestTextData(string request)
    {
        return "";
    }

    public void SendTextData(string data)
    {

    }

    public void EnableSpeakingAnim()
    {
    }

    public bool SpeakingAnimActive()
    {
        return false;
    }

    public void DisableSpeakingAnim()
    {
    }

    public void SetAnimation(string animationID, bool force = false)
    {
    }

    public void SendAnimationData(string data)
    {
    }

    public Vector3 GetTextTailPosition()
    {
        return transform.position;
    }

    public void TextBleep()
    {
    }

    public void SetFacing(Vector3 facingTarget)
    {
    }

    public void EmoteEffect(TagEntry.Emote emote)
    {
    }
}
