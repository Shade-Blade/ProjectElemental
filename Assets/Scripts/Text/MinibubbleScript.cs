using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static TMPString;

//copy of textbox script with some stuff added
//Differences:
//  manual scrolling stuff is gone
//  a lot of tags don't work
//  Tail only exists in detached mode
public class MinibubbleScript : MonoBehaviour
{
    public bool detached;
    public bool free;   //not bound to a parent
    public bool deleteSignal = false;
    public bool superDeleteSignal = false;

    public TextboxScript parent;

    public Image innerBox;
    public Image borderBox;

    public TextDisplayer text;
    public GameObject baseObject;
    public Image tail;

    ITextSpeaker originalSpeaker;
    ITextSpeaker speaker;
    public bool hasTail;
    public Vector3 tailPointPos;
    private Vector2 tailPointTruePos;
    private float maxTailMovement = 70;

    public bool tailRealTimeUpdate;

    //All the important stuff for running the dialogue
    //public string[] lines;
    public List<string> lines;
    public string[] vars;
    public int currLine = 0;

    public bool textDone = false;
    //Controls
    //A press = small fast scroll
    //A press (with next icon) = move to next line
    //Z press = go back if possible
    //B hold = very fast scroll and move to next line if possible (does nothing if not at revealed line)

    protected Vector3 animOffset = new Vector3(0, -200, 0);
    protected float animBaseTime = 0.1f;
    private float animTime;
    protected float startScale = 0.1f; //scale ends at 1
    public Vector3 originalPos = Vector3.negativeInfinity;

    public const float scrollBufferBaseTime = 0.1f; //0.2f;
    protected float scrollBufferTime = 0.0f;

    protected bool noStartAnim;
    protected bool noEndAnim;

    public void DetachedPosition(Vector3 position)
    {
        Vector2 canvasPos = MainManager.Instance.WorldPosToCanvasPosC(position);

        //currently it is center anchored so I have to offset that
        canvasPos.y -= MainManager.CanvasHeight() / 2;
        canvasPos.x -= MainManager.CanvasWidth() / 2;

        //canvasPos.x -= 150;

        if (MainManager.Instance.IsPositionBehindCamera(position))
        {
            canvasPos.y *= -1;
            canvasPos.x *= -1;
        }


        //???
        canvasPos.y += 150;


        if (canvasPos.y > 90)
        {
            canvasPos.y = 90;
        }
        if (canvasPos.y < -180)
        {
            canvasPos.y = -180;
        }
        if (canvasPos.x < -300)
        {
            canvasPos.x = -300;
        }
        if (canvasPos.x > 300)
        {
            canvasPos.x = 300;
        }

        //Debug.Log("Canvas pos " + canvasPos);

        RectTransform rt = baseObject.GetComponent<RectTransform>();

        rt.anchoredPosition = canvasPos;
    }
    public void ConvertTailPos()
    {
        tailPointTruePos = MainManager.Instance.WorldPosToCanvasPosB(tailPointPos);
    }
    public void PointTail()
    {
        ConvertTailPos();

        //Set tail position
        //Tail position is +-300 off original position
        //This position is calculated based on how far along the screen the real point is
        tail.enabled = hasTail && detached;

        if (!tail.enabled)
        {
            return;
        }

        //Debug.Log("tail enabled = " + hasTail + " " + detached);
        float offset = maxTailMovement * Mathf.Clamp(2 * (((tailPointTruePos.x) / Screen.width) - 0.5f), -1, 1);
        tail.rectTransform.anchoredPosition = (offset) * Vector3.right + tail.rectTransform.anchoredPosition[1] * Vector3.up;



        //Point tail towards target
        Vector2 tailCenterScreenPos = tail.rectTransform.TransformPoint(Vector3.zero);
        //Vector2 ScreenToCanvasScale = new Vector2(MainManager.Instance.Canvas.GetComponent<RectTransform>().rect.width / Screen.width,
    //MainManager.Instance.Canvas.GetComponent<RectTransform>().rect.height / Screen.height);
        //tailCenterScreenPos *= ScreenToCanvasScale;
        //finally this calculation actually works

        float angle = (180f / Mathf.PI) * Mathf.Atan2(tailPointTruePos[1] - tailCenterScreenPos[1], tailPointTruePos[0] - tailCenterScreenPos[0]);

        angle += 90;

        //Change: impossible angles won't get displayed
        if (angle > 70 || angle < -70)
        {
            tail.enabled = false;
            //Debug.Log("Fail condition A " + angle);
        }

        //Tail won't be displayed if it's too close to the speaker
        if (tailCenterScreenPos[1] - tailPointTruePos[1] < 25)
        {
            tail.enabled = false;
            //Debug.Log("Fail condition B " + (tailCenterScreenPos[1] - tailPointTruePos[1]));
        }

        //Debug.Log(angle + " "  + tailPointPos + " " + tailPointTruePos + " " + tailCenterScreenPos);

        //Note that negative is pointing more left and positive is pointing more right
        tail.rectTransform.eulerAngles = (angle) * Vector3.forward;
    }
    public void MoveTail(Vector3 newPos)
    {
        tailPointPos = newPos;
        ConvertTailPos();
        PointTail();
    }

    public void ChangeBoxStyle(TagEntry.BoxStyle bs)
    {
        borderBox.type = Image.Type.Sliced;
        innerBox.type = Image.Type.Sliced;
        switch (bs)
        {
            case TagEntry.BoxStyle.Default:
                borderBox.sprite = Resources.Load<Sprite>("Sprites/GUIBoxes/UIRoundedBorder");
                innerBox.sprite = Resources.Load<Sprite>("Sprites/GUIBoxes/UIRoundedInside");
                break;
            case TagEntry.BoxStyle.Outline:
                borderBox.sprite = Resources.Load<Sprite>("Sprites/GUIBoxes/UIRoundedOutlineBorder");
                innerBox.sprite = Resources.Load<Sprite>("Sprites/GUIBoxes/UIRoundedOutlineInside");
                break;
            case TagEntry.BoxStyle.DarkOutline:
                borderBox.sprite = Resources.Load<Sprite>("Sprites/GUIBoxes/UIRoundedDarkOutlineBorder");
                innerBox.sprite = Resources.Load<Sprite>("Sprites/GUIBoxes/UIRoundedDarkOutlineInside");
                break;
            case TagEntry.BoxStyle.FancyOutline:
                borderBox.sprite = Resources.Load<Sprite>("Sprites/GUIBoxes/UIRoundedFancyOutlineBorder");
                innerBox.sprite = Resources.Load<Sprite>("Sprites/GUIBoxes/UIRoundedFancyOutlineInside");
                break;
            case TagEntry.BoxStyle.Shaded:
                borderBox.sprite = Resources.Load<Sprite>("Sprites/GUIBoxes/UIRoundedShadedBorder");
                innerBox.sprite = Resources.Load<Sprite>("Sprites/GUIBoxes/UIRoundedShadedInside");
                break;
            case TagEntry.BoxStyle.Paper:
                borderBox.sprite = Resources.Load<Sprite>("Sprites/GUIBoxes/UIPaperBorder");
                innerBox.sprite = Resources.Load<Sprite>("Sprites/GUIBoxes/UIPaperInside");
                break;
            case TagEntry.BoxStyle.Beads:
                borderBox.sprite = Resources.Load<Sprite>("Sprites/GUIBoxes/UIRoundedBeadOutlineBorder");
                innerBox.sprite = Resources.Load<Sprite>("Sprites/GUIBoxes/UIRoundedBeadOutlineInside");
                borderBox.type = Image.Type.Tiled;
                innerBox.type = Image.Type.Sliced;
                break;
            case TagEntry.BoxStyle.System:
                borderBox.sprite = Resources.Load<Sprite>("Sprites/GUIBoxes/UIRoundedInverseOutlineBorder");
                innerBox.sprite = Resources.Load<Sprite>("Sprites/GUIBoxes/UIRoundedOutlineInside");
                break;
            default:
                Debug.LogError("Impossible box style: " + bs);
                break;
        }
    }


    public event System.EventHandler<TextDisplayer.ScrollEventArgs> seeSpecialTag;

    public void SeeTag(object sender, TextDisplayer.ScrollEventArgs scrollEventArgs)
    {
        TagEntry t = scrollEventArgs.tag;
        //Debug.Log(t);
        switch (t.tag)
        {
            case TagEntry.TextTag.Tail:
                bool speakerPreNull = (speaker == null);
                if (t.args.Length == 1)
                {
                    bool a;
                    if (bool.TryParse(t.args[0], out a))
                    {
                        if (a)
                        {
                            Debug.LogWarning("<tail,true> is not supported. Use <tail,[vector]> or <tail,[speakerID]> instead.");
                            //ConvertTailPos();
                            //PointTail();
                        }
                        else
                        {
                            hasTail = a;
                            tail.enabled = a;
                            speaker = null;
                        }
                    }
                    int b;
                    if (int.TryParse(t.args[0], out b))
                    {
                        hasTail = true;
                        tail.enabled = true;
                        speaker = MainManager.Instance.GetSpeaker(b);
                        MoveTail(speaker.GetTextTailPosition());
                    }
                    if (t.args[0].Equals("o"))
                    {
                        hasTail = true;
                        tail.enabled = true;
                        speaker = originalSpeaker;
                        MoveTail(speaker.GetTextTailPosition());
                    }
                    if (t.args[0].Equals("w"))
                    {
                        hasTail = true;
                        tail.enabled = true;
                        speaker = MainManager.Instance.LocatePlayerSpeaker(MainManager.PlayerCharacter.Wilex);
                        MoveTail(speaker.GetTextTailPosition());
                    }
                    if (t.args[0].Equals("l"))
                    {
                        hasTail = true;
                        tail.enabled = true;
                        speaker = MainManager.Instance.LocatePlayerSpeaker(MainManager.PlayerCharacter.Luna);
                        MoveTail(speaker.GetTextTailPosition());
                    }
                    if (t.args[0].Equals("k"))
                    {
                        hasTail = true;
                        tail.enabled = true;
                        speaker = MainManager.Instance.LocateKeru();
                        if (speaker == null)
                        {
                            Color keruColorBorderA = new Color(1, 1, 1, 0.7f);
                            Color keruColorInnerA = new Color(0.6f, 1, 1, 0.7f);
                            borderBox.color = keruColorBorderA;
                            innerBox.color = keruColorInnerA;
                            ChangeBoxStyle(TagEntry.BoxStyle.System);
                            //tail false
                            hasTail = false;
                            tail.enabled = false;
                        }
                        else
                        {
                            MoveTail(speaker.GetTextTailPosition());
                        }
                    }

                    //Semi hacky setup for keru (to make sure text afterwards is reset automatically)
                    //I could just put a boxreset tag after every single instance of keru dialogue but that seems tedious (and potentially allow for mistakes)
                    //(It usually isn't the case that I want to keep box styles in this scenario anyway)
                    if (speakerPreNull && speaker != null)
                    {
                        ChangeBoxStyle(TagEntry.BoxStyle.Default);
                        //tail and inner box are forced to have the same color
                        innerBox.color = Color.white;
                        borderBox.color = Color.white;
                        tail.color = Color.white;
                    }
                }
                //check for a vector3
                if (t.args.Length == 3)
                {
                    hasTail = true;
                    tail.enabled = true;
                    float a = float.Parse(t.args[0]);
                    float b = float.Parse(t.args[1]);
                    float c = float.Parse(t.args[2]);
                    MoveTail(new Vector3(a, b, c));
                }
                break;
            case TagEntry.TextTag.TailRealTimeUpdate:
                if (t.args.Length > 0 && bool.TryParse(t.args[0], out bool trtu))
                {
                    if (speaker != null)
                    {
                        tailRealTimeUpdate = trtu;
                    }
                    else
                    {
                        Debug.LogWarning("Tail Real Time Update does not make sense without a speaker");
                    }
                }
                break;
            case TagEntry.TextTag.Anim:
                int animMEID;
                ITextSpeaker targetSpeaker = null;
                bool force = false;
                if (int.TryParse(t.args[0], out animMEID))
                {
                    targetSpeaker = MainManager.Instance.GetSpeaker(animMEID);
                }
                if (t.args.Length > 1 && bool.TryParse(t.args[1], out force))
                {

                }
                if (t.args[0].Equals("o"))
                {
                    targetSpeaker = originalSpeaker;
                }
                if (t.args[0].Equals("w"))
                {
                    targetSpeaker = MainManager.Instance.LocatePlayerSpeaker(MainManager.PlayerCharacter.Wilex);
                }
                if (t.args[0].Equals("l"))
                {
                    targetSpeaker = MainManager.Instance.LocatePlayerSpeaker(MainManager.PlayerCharacter.Luna);
                }
                if (t.args[0].Equals("k"))
                {
                    targetSpeaker = MainManager.Instance.LocateKeru();
                }
                if (targetSpeaker != null)
                {
                    targetSpeaker.SetAnimation(t.args[1], force);
                }
                break;
            case TagEntry.TextTag.AnimData:
                int animDMEID;
                ITextSpeaker targetSpeakerD = null;
                if (int.TryParse(t.args[0], out animDMEID))
                {
                    targetSpeakerD = MainManager.Instance.GetSpeaker(animDMEID);
                }
                if (t.args[0].Equals("o"))
                {
                    targetSpeakerD = originalSpeaker;
                }
                if (t.args[0].Equals("w"))
                {
                    targetSpeakerD = MainManager.Instance.LocatePlayerSpeaker(MainManager.PlayerCharacter.Wilex);
                }
                if (t.args[0].Equals("l"))
                {
                    targetSpeakerD = MainManager.Instance.LocatePlayerSpeaker(MainManager.PlayerCharacter.Luna);
                }
                if (t.args[0].Equals("k"))
                {
                    targetSpeakerD = MainManager.Instance.LocateKeru();
                }
                if (targetSpeakerD != null)
                {
                    targetSpeakerD.SendAnimationData(t.args[1]);
                }
                break;
            case TagEntry.TextTag.Face:
                int animFMEID;
                ITextSpeaker targetSpeakerF = null;
                if (int.TryParse(t.args[0], out animFMEID))
                {
                    targetSpeakerF = MainManager.Instance.GetSpeaker(animFMEID);
                }
                if (t.args[0].Equals("o"))
                {
                    targetSpeakerF = originalSpeaker;
                }
                if (t.args[0].Equals("w"))
                {
                    targetSpeakerF = MainManager.Instance.LocatePlayerSpeaker(MainManager.PlayerCharacter.Wilex);
                }
                if (t.args[0].Equals("l"))
                {
                    targetSpeakerF = MainManager.Instance.LocatePlayerSpeaker(MainManager.PlayerCharacter.Luna);
                }
                if (t.args[0].Equals("k"))
                {
                    targetSpeakerF = MainManager.Instance.LocateKeru();
                }
                if (targetSpeakerF != null)
                {
                    if (t.args.Length == 4)
                    {
                        float a = float.Parse(t.args[1]);
                        float b = float.Parse(t.args[2]);
                        float c = float.Parse(t.args[3]);
                        targetSpeakerF.SetFacing(new Vector3(a, b, c));
                    }
                    else
                    {
                        int animFBMEID;
                        ITextSpeaker targetSpeakerFB = null;
                        if (int.TryParse(t.args[1], out animFBMEID))
                        {
                            targetSpeakerFB = MainManager.Instance.GetSpeaker(animFBMEID);
                        }
                        if (t.args[1].Equals("o"))
                        {
                            targetSpeakerFB = originalSpeaker;
                        }
                        if (t.args[1].Equals("w"))
                        {
                            targetSpeakerFB = MainManager.Instance.LocatePlayerSpeaker(MainManager.PlayerCharacter.Wilex);
                        }
                        if (t.args[1].Equals("l"))
                        {
                            targetSpeakerFB = MainManager.Instance.LocatePlayerSpeaker(MainManager.PlayerCharacter.Luna);
                        }
                        if (t.args[1].Equals("k"))
                        {
                            targetSpeakerFB = MainManager.Instance.LocateKeru();
                        }
                        if (targetSpeakerFB != null)
                        {
                            targetSpeakerF.SetFacing(targetSpeakerFB.GetTextTailPosition());
                        }
                    }
                }
                break;
            case TagEntry.TextTag.Emote:
                int animEMEID;
                ITextSpeaker targetSpeakerE = null;
                if (int.TryParse(t.args[0], out animEMEID))
                {
                    targetSpeakerE = MainManager.Instance.GetSpeaker(animEMEID);
                }
                if (t.args[0].Equals("o"))
                {
                    targetSpeakerE = originalSpeaker;
                }
                if (t.args[0].Equals("w"))
                {
                    targetSpeakerE = MainManager.Instance.LocatePlayerSpeaker(MainManager.PlayerCharacter.Wilex);
                }
                if (t.args[0].Equals("l"))
                {
                    targetSpeakerE = MainManager.Instance.LocatePlayerSpeaker(MainManager.PlayerCharacter.Luna);
                }
                if (t.args[0].Equals("k"))
                {
                    targetSpeakerE = MainManager.Instance.LocateKeru();
                }
                if (targetSpeakerE != null)
                {
                    Enum.TryParse(t.args[1], true, out TagEntry.Emote emote);
                    targetSpeakerE.EmoteEffect(emote);
                }
                break;
            case TagEntry.TextTag.BoxStyle:
                TagEntry.BoxStyle bs = TagEntry.BoxStyle.Default;
                if (t.args.Length > 0)
                {
                    Enum.TryParse(t.args[0], true, out bs);
                }
                ChangeBoxStyle(bs);
                break;
            case TagEntry.TextTag.BoxColor:
                Color targetColor = Color.white;
                ColorNames color;
                if (t.args.Length > 0)
                {
                    if (Enum.TryParse(t.args[0], out color))
                    {
                        t.args[0] = ColorUtility.ToHtmlStringRGB(baseColors[(int)color]);
                    }
                    targetColor = MainManager.ParseColor(t.args[0]).GetValueOrDefault();
                }

                Color targetColorB = targetColor;

                if (t.args.Length > 1)
                {
                    if (Enum.TryParse(t.args[1], out color))
                    {
                        t.args[1] = ColorUtility.ToHtmlStringRGB(baseColors[(int)color]);
                    }
                    targetColorB = MainManager.ParseColor(t.args[1]).GetValueOrDefault();
                }

                //tail and inner box are forced to have the same color
                innerBox.color = targetColor;
                borderBox.color = targetColorB;
                tail.color = targetColor;
                break;
            case TagEntry.TextTag.Sign:
                Color signColorBorder = new Color(0.8f, 0.5f, 0.15f, 1);
                Color signColorInner = new Color(1, 0.95f, 0.7f, 1);
                borderBox.color = signColorBorder;
                innerBox.color = signColorInner;
                ChangeBoxStyle(TagEntry.BoxStyle.DarkOutline);
                //tail false
                hasTail = false;
                tail.enabled = false;
                speaker = null;
                break;
            case TagEntry.TextTag.System:
                Color systemColorBorder = new Color(1, 1, 1, 0.5f);
                Color systemColorInner = new Color(0, 0, 0, 0.5f);
                borderBox.color = systemColorBorder;
                innerBox.color = systemColorInner;
                ChangeBoxStyle(TagEntry.BoxStyle.System);
                //tail false
                hasTail = false;
                tail.enabled = false;
                speaker = null;
                break;
            case TagEntry.TextTag.KeruDistant:
                Color keruColorBorder = new Color(1, 1, 1, 0.7f);
                Color keruColorInner = new Color(0.6f, 1, 1, 0.7f);
                borderBox.color = keruColorBorder;
                innerBox.color = keruColorInner;
                ChangeBoxStyle(TagEntry.BoxStyle.System);
                //tail false
                hasTail = false;
                tail.enabled = false;
                speaker = null;
                break;
            case TagEntry.TextTag.BoxReset:
                ChangeBoxStyle(TagEntry.BoxStyle.Default);
                //tail and inner box are forced to have the same color
                innerBox.color = Color.white;
                borderBox.color = Color.white;
                tail.color = Color.white;
                break;
            case TagEntry.TextTag.Goto:
            case TagEntry.TextTag.Branch:
                seeSpecialTag?.Invoke(this, new TextDisplayer.ScrollEventArgs(t));
                break;
        }
    }

    public IEnumerator StartAnim()
    {
        animTime = 0;
        RectTransform rt = baseObject.GetComponent<RectTransform>();

        tail.enabled = hasTail && detached;
        PointTail();

        DetachedPosition(tailPointPos);
        originalPos = rt.anchoredPosition;  //end position?

        Vector3 newAnchoredPos = MainManager.Instance.WorldPosToCanvasPosC(tailPointPos);   //start position
        newAnchoredPos.y -= MainManager.CanvasHeight() / 2;
        newAnchoredPos.x -= MainManager.CanvasWidth() / 2;
        if (MainManager.Instance.IsPositionBehindCamera(tailPointPos))
        {
            newAnchoredPos.y *= -1;
            newAnchoredPos.x *= -1;
        }

        if (hasTail)
        {
            ConvertTailPos();
            //baseObject.transform.InverseTransformPoint(
        }
        else
        {
            newAnchoredPos = animOffset + originalPos;
        }


        while (animTime < animBaseTime)
        {
            rt.localScale = Vector3.Lerp(new Vector3(startScale, startScale, startScale), new Vector3(1, 1, 1), animTime / animBaseTime);
            rt.anchoredPosition = Vector3.Lerp(newAnchoredPos, originalPos, animTime / animBaseTime);
            animTime += Time.deltaTime;
            PointTail();
            yield return null;
        }

        if (speaker != null && !speaker.SpeakingAnimActive())
        {
            speaker.EnableSpeakingAnim();
        }

        PointTail();
        rt.localScale = new Vector3(1, 1, 1);
        rt.anchoredPosition = originalPos;
    }
    public IEnumerator EndAnim()
    {
        RectTransform rt = baseObject.GetComponent<RectTransform>();

        animTime = 0;
        originalPos = rt.anchoredPosition;  //start position

        Vector3 newAnchoredPos = MainManager.Instance.WorldPosToCanvasPosC(tailPointPos);   //start position
        newAnchoredPos.y -= MainManager.CanvasHeight() / 2;
        newAnchoredPos.x -= MainManager.CanvasWidth() / 2;
        if (MainManager.Instance.IsPositionBehindCamera(tailPointPos))
        {
            newAnchoredPos.y *= -1;
            newAnchoredPos.x *= -1;
        }
        if (hasTail)
        {
            ConvertTailPos();
        }
        else
        {
            newAnchoredPos = animOffset + originalPos;
        }

        while (animTime < animBaseTime)
        {
            rt.localScale = Vector3.Lerp(new Vector3(startScale, startScale, startScale), new Vector3(1, 1, 1), 1 - (animTime / animBaseTime));
            rt.anchoredPosition = Vector3.Lerp(originalPos, newAnchoredPos, animTime / animBaseTime);
            animTime += Time.deltaTime;

            yield return null;
        }

        if (speaker != null && speaker.SpeakingAnimActive())
        {
            speaker.DisableSpeakingAnim();
        }

        rt.localScale = new Vector3(startScale, startScale, startScale);
        rt.anchoredPosition = originalPos + animOffset;
    }

    //To do later: consolidate these 3 creation methods since they do a lot of the same stuff
    public IEnumerator CreateText(string dialogue, string[] vars = null, bool free = false) //string[] lines
    {
        this.free = free;        
        //Debug.Log("Create");
        tail.enabled = false;
        this.vars = vars;

        //Debug.Log("box " + vars);

        //Initial tags may change the start anim
        List<TagEntry> startTags = (new List<TagEntry>(new FormattedString(FormattedString.ReplaceTextFileShorthand(dialogue)).tags)).FindAll((t) => (t.trueStartIndex == 0));
        for (int i = 0; i < startTags.Count; i++)
        {
            TagEntry t = startTags[i];
            switch (t.tag)
            {
                case TagEntry.TextTag.Tail:
                    bool speakerPreNull = (speaker == null);
                    if (speaker != null && speaker.SpeakingAnimActive())
                    {
                        speaker.DisableSpeakingAnim();
                    }
                    if (t.args.Length == 1)
                    {
                        bool a;
                        if (bool.TryParse(t.args[0], out a))
                        {
                            if (a)
                            {
                                Debug.LogWarning("<tail,true> is not supported. Use <tail,[vector]> or <tail,[speakerID]> instead.");
                                //ConvertTailPos();
                                //PointTail();
                            }
                            else
                            {
                                hasTail = a;
                                tail.enabled = a;
                                speaker = null;
                            }
                        }
                        int b;
                        if (int.TryParse(t.args[0], out b))
                        {
                            hasTail = true;
                            tail.enabled = true;
                            speaker = MainManager.Instance.GetSpeaker(b);
                            MoveTail(speaker.GetTextTailPosition());
                        }
                        if (t.args[0].Equals("o"))
                        {
                            hasTail = true;
                            tail.enabled = true;
                            speaker = originalSpeaker;
                            MoveTail(speaker.GetTextTailPosition());
                        }
                        if (t.args[0].Equals("w"))
                        {
                            hasTail = true;
                            tail.enabled = true;
                            speaker = MainManager.Instance.LocatePlayerSpeaker(MainManager.PlayerCharacter.Wilex);
                            MoveTail(speaker.GetTextTailPosition());
                        }
                        if (t.args[0].Equals("l"))
                        {
                            hasTail = true;
                            tail.enabled = true;
                            speaker = MainManager.Instance.LocatePlayerSpeaker(MainManager.PlayerCharacter.Luna);
                            MoveTail(speaker.GetTextTailPosition());
                        }
                        if (t.args[0].Equals("k"))
                        {
                            hasTail = true;
                            tail.enabled = true;
                            speaker = MainManager.Instance.LocateKeru();
                            if (speaker == null)
                            {
                                Color keruColorBorderA = new Color(1, 1, 1, 0.7f);
                                Color keruColorInnerA = new Color(0.6f, 1, 1, 0.7f);
                                borderBox.color = keruColorBorderA;
                                innerBox.color = keruColorInnerA;
                                ChangeBoxStyle(TagEntry.BoxStyle.System);
                                //tail false
                                hasTail = false;
                                tail.enabled = false;
                            }
                            else
                            {
                                MoveTail(speaker.GetTextTailPosition());
                            }
                        }

                        //Semi hacky setup for keru (to make sure text afterwards is reset automatically)
                        //I could just put a boxreset tag after every single instance of keru dialogue but that seems tedious (and potentially allow for mistakes)
                        //(It usually isn't the case that I want to keep box styles in this scenario anyway)
                        if (speakerPreNull && speaker != null)
                        {
                            ChangeBoxStyle(TagEntry.BoxStyle.Default);
                            //tail and inner box are forced to have the same color
                            innerBox.color = Color.white;
                            borderBox.color = Color.white;
                            tail.color = Color.white;
                        }
                    }
                    //check for a vector3
                    if (t.args.Length == 3)
                    {
                        hasTail = true;
                        tail.enabled = true;
                        float a = float.Parse(t.args[0]);
                        float b = float.Parse(t.args[1]);
                        float c = float.Parse(t.args[2]);
                        MoveTail(new Vector3(a, b, c));
                    }
                    break;
                case TagEntry.TextTag.TailRealTimeUpdate:
                    if (t.args.Length > 0 && bool.TryParse(t.args[0], out bool trtu))
                    {
                        if (speaker != null)
                        {
                            tailRealTimeUpdate = trtu;
                        }
                        else
                        {
                            Debug.LogWarning("Tail Real Time Update does not make sense without a speaker");
                        }
                    }
                    break;
                case TagEntry.TextTag.BoxStyle:
                    TagEntry.BoxStyle bs = TagEntry.BoxStyle.Default;
                    if (t.args.Length > 0)
                    {
                        Enum.TryParse(t.args[0], true, out bs);
                    }
                    ChangeBoxStyle(bs);
                    break;
                case TagEntry.TextTag.BoxColor:
                    Color targetColor = Color.white;
                    ColorNames color;
                    if (t.args.Length > 0)
                    {
                        if (Enum.TryParse(t.args[0], out color))
                        {
                            t.args[0] = ColorUtility.ToHtmlStringRGB(baseColors[(int)color]);
                        }
                        targetColor = MainManager.ParseColor(t.args[0]).GetValueOrDefault();
                    }

                    Color targetColorB = targetColor;

                    if (t.args.Length > 1)
                    {
                        if (Enum.TryParse(t.args[1], out color))
                        {
                            t.args[1] = ColorUtility.ToHtmlStringRGB(baseColors[(int)color]);
                        }
                        targetColorB = MainManager.ParseColor(t.args[1]).GetValueOrDefault();
                    }

                    //tail and inner box are forced to have the same color
                    innerBox.color = targetColor;
                    borderBox.color = targetColorB;
                    tail.color = targetColor;
                    break;
                case TagEntry.TextTag.Sign:
                    Color signColorBorder = new Color(0.8f, 0.5f, 0.15f, 1);
                    Color signColorInner = new Color(1, 0.95f, 0.7f, 1);
                    borderBox.color = signColorBorder;
                    innerBox.color = signColorInner;
                    ChangeBoxStyle(TagEntry.BoxStyle.DarkOutline);
                    //tail false
                    hasTail = false;
                    tail.enabled = false;
                    speaker = null;
                    break;
                case TagEntry.TextTag.System:
                    Color systemColorBorder = new Color(1, 1, 1, 0.5f);
                    Color systemColorInner = new Color(0, 0, 0, 0.5f);
                    borderBox.color = systemColorBorder;
                    innerBox.color = systemColorInner;
                    ChangeBoxStyle(TagEntry.BoxStyle.System);
                    //tail false
                    hasTail = false;
                    tail.enabled = false;
                    speaker = null;
                    break;
                case TagEntry.TextTag.KeruDistant:
                    Color keruColorBorder = new Color(1, 1, 1, 0.7f);
                    Color keruColorInner = new Color(0.6f, 1, 1, 0.7f);
                    borderBox.color = keruColorBorder;
                    innerBox.color = keruColorInner;
                    ChangeBoxStyle(TagEntry.BoxStyle.System);
                    //tail false
                    hasTail = false;
                    tail.enabled = false;
                    speaker = null;
                    break;
                case TagEntry.TextTag.BoxReset:
                    ChangeBoxStyle(TagEntry.BoxStyle.Default);
                    //tail and inner box are forced to have the same color
                    innerBox.color = Color.white;
                    borderBox.color = Color.white;
                    tail.color = Color.white;
                    break;
            }
        }

        if (!noStartAnim)
        {
            yield return StartCoroutine(StartAnim());
        }

        string[] lines = FormattedString.SplitByTag(FormattedString.ReplaceTextFileShorthand(dialogue), false, TagEntry.TextTag.Next);

        this.lines = new List<string>(lines);
        currLine = 0;

        text.tagScroll += SeeTag;
        text.SetText(lines[currLine], vars);

        while (!textDone)
        {
            if ((!free && parent == null) || (deleteSignal && !detached) || superDeleteSignal)
            {
                textDone = true;
            }
            yield return null;
        }

        while (!(superDeleteSignal || (!free && parent == null) || detached || deleteSignal))
        {
            yield return null;
        }

        text.tagScroll -= SeeTag;

        if (!noEndAnim)
        {
            yield return StartCoroutine(EndAnim());
        }
    }
    public IEnumerator CreateText(string dialogue, Vector3 tailPos, string[] vars = null, bool free = false)
    {
        this.free = free;
        if (free)
        {
            DetachedPosition(tailPos);
        }
        //Debug.Log("Create with tail");
        tail.enabled = true && detached;
        tailPointPos = tailPos;
        ConvertTailPos();
        PointTail();
        this.vars = vars;

        hasTail = true && detached;

        //Initial tags may change the start anim
        //(This fixes the scenario where you format a minibubble a certain way but it shows up as the default white for a few frames)
        List<TagEntry> startTags = (new List<TagEntry>(new FormattedString(FormattedString.ReplaceTextFileShorthand(dialogue)).tags)).FindAll((t) => (t.trueStartIndex == 0));
        for (int i = 0; i < startTags.Count; i++)
        {
            TagEntry t = startTags[i];
            switch (t.tag)
            {
                case TagEntry.TextTag.Tail:
                    bool speakerPreNull = (speaker == null);
                    if (speaker != null && speaker.SpeakingAnimActive())
                    {
                        speaker.DisableSpeakingAnim();
                    }
                    if (t.args.Length == 1)
                    {
                        bool a;
                        if (bool.TryParse(t.args[0], out a))
                        {
                            if (a)
                            {
                                Debug.LogWarning("<tail,true> is not supported. Use <tail,[vector]> or <tail,[speakerID]> instead.");
                                //ConvertTailPos();
                                //PointTail();
                            }
                            else
                            {
                                hasTail = a;
                                tail.enabled = a;
                                speaker = null;
                            }
                        }
                        int b;
                        if (int.TryParse(t.args[0], out b))
                        {
                            hasTail = true;
                            tail.enabled = true;
                            speaker = MainManager.Instance.GetSpeaker(b);
                            MoveTail(speaker.GetTextTailPosition());
                        }
                        if (t.args[0].Equals("o"))
                        {
                            hasTail = true;
                            tail.enabled = true;
                            speaker = originalSpeaker;
                            MoveTail(speaker.GetTextTailPosition());
                        }
                        if (t.args[0].Equals("w"))
                        {
                            hasTail = true;
                            tail.enabled = true;
                            speaker = MainManager.Instance.LocatePlayerSpeaker(MainManager.PlayerCharacter.Wilex);
                            MoveTail(speaker.GetTextTailPosition());
                        }
                        if (t.args[0].Equals("l"))
                        {
                            hasTail = true;
                            tail.enabled = true;
                            speaker = MainManager.Instance.LocatePlayerSpeaker(MainManager.PlayerCharacter.Luna);
                            MoveTail(speaker.GetTextTailPosition());
                        }
                        if (t.args[0].Equals("k"))
                        {
                            hasTail = true;
                            tail.enabled = true;
                            speaker = MainManager.Instance.LocateKeru();
                            if (speaker == null)
                            {
                                Color keruColorBorderA = new Color(1, 1, 1, 0.7f);
                                Color keruColorInnerA = new Color(0.6f, 1, 1, 0.7f);
                                borderBox.color = keruColorBorderA;
                                innerBox.color = keruColorInnerA;
                                ChangeBoxStyle(TagEntry.BoxStyle.System);
                                //tail false
                                hasTail = false;
                                tail.enabled = false;
                            }
                            else
                            {
                                MoveTail(speaker.GetTextTailPosition());
                            }
                        }

                        //Semi hacky setup for keru (to make sure text afterwards is reset automatically)
                        //I could just put a boxreset tag after every single instance of keru dialogue but that seems tedious (and potentially allow for mistakes)
                        //(It usually isn't the case that I want to keep box styles in this scenario anyway)
                        if (speakerPreNull && speaker != null)
                        {
                            ChangeBoxStyle(TagEntry.BoxStyle.Default);
                            //tail and inner box are forced to have the same color
                            innerBox.color = Color.white;
                            borderBox.color = Color.white;
                            tail.color = Color.white;
                        }
                    }
                    //check for a vector3
                    if (t.args.Length == 3)
                    {
                        hasTail = true;
                        tail.enabled = true;
                        float a = float.Parse(t.args[0]);
                        float b = float.Parse(t.args[1]);
                        float c = float.Parse(t.args[2]);
                        MoveTail(new Vector3(a, b, c));
                    }
                    break;
                case TagEntry.TextTag.TailRealTimeUpdate:
                    if (t.args.Length > 0 && bool.TryParse(t.args[0], out bool trtu))
                    {
                        if (speaker != null)
                        {
                            tailRealTimeUpdate = trtu;
                        }
                        else
                        {
                            Debug.LogWarning("Tail Real Time Update does not make sense without a speaker");
                        }
                    }
                    break;
                case TagEntry.TextTag.BoxStyle:
                    TagEntry.BoxStyle bs = TagEntry.BoxStyle.Default;
                    if (t.args.Length > 0)
                    {
                        Enum.TryParse(t.args[0], true, out bs);
                    }
                    ChangeBoxStyle(bs);
                    break;
                case TagEntry.TextTag.BoxColor:
                    Color targetColor = Color.white;
                    ColorNames color;
                    if (t.args.Length > 0)
                    {
                        if (Enum.TryParse(t.args[0], out color))
                        {
                            t.args[0] = ColorUtility.ToHtmlStringRGB(baseColors[(int)color]);
                        }
                        targetColor = MainManager.ParseColor(t.args[0]).GetValueOrDefault();
                    }

                    Color targetColorB = targetColor;

                    if (t.args.Length > 1)
                    {
                        if (Enum.TryParse(t.args[1], out color))
                        {
                            t.args[1] = ColorUtility.ToHtmlStringRGB(baseColors[(int)color]);
                        }
                        targetColorB = MainManager.ParseColor(t.args[1]).GetValueOrDefault();
                    }

                    //tail and inner box are forced to have the same color
                    innerBox.color = targetColor;
                    borderBox.color = targetColorB;
                    tail.color = targetColor;
                    break;
                case TagEntry.TextTag.Sign:
                    Color signColorBorder = new Color(0.8f, 0.5f, 0.15f, 1);
                    Color signColorInner = new Color(1, 0.95f, 0.7f, 1);
                    borderBox.color = signColorBorder;
                    innerBox.color = signColorInner;
                    ChangeBoxStyle(TagEntry.BoxStyle.DarkOutline);
                    //tail false
                    hasTail = false;
                    tail.enabled = false;
                    speaker = null;
                    break;
                case TagEntry.TextTag.System:
                    Color systemColorBorder = new Color(1, 1, 1, 0.5f);
                    Color systemColorInner = new Color(0, 0, 0, 0.5f);
                    borderBox.color = systemColorBorder;
                    innerBox.color = systemColorInner;
                    ChangeBoxStyle(TagEntry.BoxStyle.System);
                    //tail false
                    hasTail = false;
                    tail.enabled = false;
                    speaker = null;
                    break;
                case TagEntry.TextTag.KeruDistant:
                    Color keruColorBorder = new Color(1, 1, 1, 0.7f);
                    Color keruColorInner = new Color(0.6f, 1, 1, 0.7f);
                    borderBox.color = keruColorBorder;
                    innerBox.color = keruColorInner;
                    ChangeBoxStyle(TagEntry.BoxStyle.System);
                    //tail false
                    hasTail = false;
                    tail.enabled = false;
                    speaker = null;
                    break;
                case TagEntry.TextTag.BoxReset:
                    ChangeBoxStyle(TagEntry.BoxStyle.Default);
                    //tail and inner box are forced to have the same color
                    innerBox.color = Color.white;
                    borderBox.color = Color.white;
                    tail.color = Color.white;
                    break;
            }
        }

        if (!noStartAnim)
        {
            yield return StartCoroutine(StartAnim());
        }

        string[] lines = FormattedString.SplitByTags(FormattedString.ReplaceTextFileShorthand(dialogue), false, TagEntry.TextTag.Next, TagEntry.TextTag.End, TagEntry.TextTag.CondEnd);

        this.lines = new List<string>(lines);
        currLine = 0;

        text.tagScroll += SeeTag;
        text.SetText(lines[currLine], vars);

        while (!textDone)
        {
            if ((!free && parent == null) || (deleteSignal && !detached) || superDeleteSignal)
            {
                textDone = true;
            }
            yield return null;
        }

        while (!(superDeleteSignal || (!free && parent == null) || detached || deleteSignal))
        {
            yield return null;
        }

        text.tagScroll -= SeeTag;

        if (!noEndAnim)
        {
            yield return StartCoroutine(EndAnim());
        }

        Destroy(gameObject);
    }

    //attached version is separate
    public IEnumerator CreateText(string dialogue, ITextSpeaker speaker, string[] vars = null, bool free = false)
    {
        //Debug.Log("Create with speaker");
        this.originalSpeaker = speaker;
        this.speaker = speaker;
        detached = true;
        Vector3 position = Vector3.zero;
        if (speaker != null)
        {
            position = speaker.GetTextTailPosition();
        }
        yield return StartCoroutine(CreateText(dialogue, position, vars, free));
    }


    public IEnumerator CreateTextAttached(string dialogue, ITextSpeaker speaker, float offset, string[] vars = null)
    {
        //Debug.Log("Create with speaker");
        this.originalSpeaker = speaker;
        this.speaker = speaker;
        Vector3 position = Vector3.zero;
        detached = false;

        RectTransform rt = baseObject.GetComponent<RectTransform>();
        rt.anchoredPosition = Vector2.right * (250 * offset) + (rt.anchoredPosition.y * Vector2.up);

        Debug.Log(rt.anchoredPosition);

        if (speaker != null)
        {
            position = speaker.GetTextTailPosition();
        }
        yield return StartCoroutine(CreateText(dialogue, position, vars));
    }


    public void ResetText()
    {
        text.StopEffectCoroutine();
        text.SetText(lines[currLine], vars, false);
    }

    // Update is called once per frame
    void Update()
    {
        if (!textDone)
        {
            TextUpdate();
        }
    }

    void TextUpdate()
    {
        //real time position change
        if (tailRealTimeUpdate)
        {
            if (speaker != null)
            {
                if (detached)
                {
                    DetachedPosition(speaker.GetTextTailPosition());
                }
                MoveTail(speaker.GetTextTailPosition());
            } else
            {
                //This is partially redundant (but this method fixes the tail's position on the screen
                MoveTail(tailPointPos);
            }
        }

        if (text.scrollDone)    //note: auto scrolls forwards, so use <wait> to prevent it from immediately skipping
        {
            if (currLine > lines.Count - 1)
            {
                if (speaker != null && speaker.SpeakingAnimActive())
                {
                    speaker.DisableSpeakingAnim();
                }
                textDone = true;
            }
            else
            {
                TryScrollForwards();
            }
        } else
        {
            if (speaker != null && !speaker.SpeakingAnimActive())
            {
                speaker.EnableSpeakingAnim();
            }
        }
    }

    void TryScrollForwards()
    {
        if (scrollBufferTime > 0)
        {
            return;
        }

        text.aPressTime = 0;
        scrollBufferTime = scrollBufferBaseTime;
        currLine++;

        if (currLine > lines.Count - 1)
        {
            //Destroy(gameObject);
            textDone = true;
        }
        else
        {
            text.StopEffectCoroutine();
            text.SetText(lines[currLine], vars, false);
        }
    }

    public ITextSpeaker GetSpeaker()
    {
        return speaker;
    }
}
