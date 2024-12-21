using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static TMPString;

public class TextboxScript : MonoBehaviour
{
    public TextDisplayer text;
    public Image nextButton;
    public GameObject baseObject;
    public Image tail;

    //to change the style, both have to change
    public Image innerBox;
    public Image borderBox;

    ITextSpeaker originalSpeaker;
    ITextSpeaker speaker;
    public bool hasTail;
    public Vector3 tailPointPos;
    private Vector2 tailPointTruePos;
    private float maxTailMovement = 300;

    public Color nextButtonColorA;
    public Color nextButtonColorB;

    //All the important stuff for running the dialogue
    //public string[] lines;
    public List<string> lines;
    public string[] vars;
    public int currLine = 0;
    public int latestLine = 0; //used to insta scroll when you go back
    public int noReturn = 0; //index you can't go back from
    public List<bool> nextSkipList;
    public bool textDone = false;
    //Controls
    //A press = small fast scroll
    //A press (with next icon) = move to next line
    //Z press = go back if possible
    //B hold = very fast scroll and move to next line if possible (does nothing if not at revealed line)

    protected Vector3 animOffset = new Vector3(0,-200,0);
    protected float animBaseTime = 0.1f;
    private float animTime;
    protected float startScale = 0.1f; //scale ends at 1
    private Vector3 originalPos = Vector3.negativeInfinity;

    public const float scrollBufferBaseTime = 0.1f; //0.2f;
    protected float scrollBufferTime = 0.0f;

    protected bool noStartAnim;
    protected bool noEndAnim;

    public bool minibubbleWait;
    List<MinibubbleScript> attachedMinibubbles;
    public bool tailRealTimeUpdate;

    private MenuHandler menuHandler;
    private bool inMenu;
    public MenuResult menuResult;

    public int lastCharsVisible;
    public int realVisibleCount;
    public int lastBleepCount;

    public const int bleepDelta = 3;

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
        tail.enabled = hasTail;
        float offset = maxTailMovement * Mathf.Clamp(2*((tailPointTruePos.x / Screen.width)-0.5f),-1,1);
        tail.rectTransform.anchoredPosition = offset * Vector3.right + tail.rectTransform.anchoredPosition[1] * Vector3.up;

        //Point tail towards target
        Vector2 tailCenterScreenPos = tail.rectTransform.TransformPoint(Vector3.zero);
        //Vector2 ScreenToCanvasScale = new Vector2(MainManager.Instance.Canvas.GetComponent<RectTransform>().rect.width / Screen.width,
    //MainManager.Instance.Canvas.GetComponent<RectTransform>().rect.height / Screen.height);
        //tailCenterScreenPos *= ScreenToCanvasScale;
        //finally this calculation actually works

        float angle = (180f / Mathf.PI) * Mathf.Atan2(tailPointTruePos[1]- tailCenterScreenPos[1], tailPointTruePos[0]- tailCenterScreenPos[0]);

        angle += 90;
        if (angle > 70)
        {
            angle = 70;
        }
        if (angle < -70)
        {
            angle = -70;
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
            case TagEntry.TextTag.Null:
                Debug.LogWarning("Null (invalid) tag detected: " + t.startIndex + " / " + t.trueStartIndex + " to " + t.trueEndIndex);
                break;
            case TagEntry.TextTag.ShowCoins:
                MainManager.Instance.ShowCoinCounter();
                break;
            case TagEntry.TextTag.HideCoins:
                MainManager.Instance.HideCoinCounter();
                break;
            case TagEntry.TextTag.Minibubble:
                //Makes a minibubble args: [mode bool, whether it is detached or not (Detached means it acts like a small independent text box, Non-detached means it gets closed when going to the next text box)], [text, use shorthand], ["B" + float OR int (meid)])
                bool miniA;
                if (bool.TryParse(t.args[0], out miniA))
                {
                }
                string miniB = t.args.Length > 1 ? t.args[1] : "";

                float miniCA = 0.0f;
                int miniCB = 0;
                Vector3 miniCC = Vector3.zero;
                bool idOrPosition = false;
                if (!miniA)
                {
                    if (t.args.Length > 2 && float.TryParse(t.args[2], out miniCA))
                    {

                    }
                } else
                {
                    if (t.args.Length > 2 && int.TryParse(t.args[2], out miniCB))
                    {

                    }
                    else
                    {
                        miniCC = MainManager.ParseVector3(t.args[2]);
                        if (t.args[2].Split("|").Length == 3)
                        {
                            idOrPosition = true;
                        }
                    }
                }

                MinibubbleScript mbs = MainManager.Instance.MakeMinibubble();
                mbs.parent = this;
                if (miniA)
                {
                    ITextSpeaker speaker = null;

                    if (!idOrPosition)
                    {
                        bool special = false;
                        if (t.args[2].Equals("o"))
                        {
                            special = true;
                            speaker = originalSpeaker;
                        }
                        if (t.args[2].Equals("w"))
                        {
                            special = true;
                            speaker = MainManager.Instance.LocatePlayerSpeaker(MainManager.PlayerCharacter.Wilex);
                        }
                        if (t.args[2].Equals("l"))
                        {
                            special = true;
                            speaker = MainManager.Instance.LocatePlayerSpeaker(MainManager.PlayerCharacter.Luna);
                        }
                        if (t.args[2].Equals("k"))
                        {
                            special = true;
                            speaker = MainManager.Instance.LocateKeru();

                            if (speaker == null)
                            {
                                //apply the special keru style
                                Color keruColorBorderA = new Color(1, 1, 1, 0.7f);
                                Color keruColorInnerA = new Color(0.6f, 1, 1, 0.7f);
                                mbs.borderBox.color = keruColorBorderA;
                                mbs.innerBox.color = keruColorInnerA;
                                mbs.ChangeBoxStyle(TagEntry.BoxStyle.System);
                                //tail false
                                mbs.hasTail = false;
                                mbs.tail.enabled = false;
                            }
                        }

                        if (!special)
                        {
                            speaker = MainManager.Instance.GetSpeaker(miniCB);
                        }

                        if (speaker == null)
                        {
                            //Give up, make it attached anyway, use arg 3
                            if (t.args.Length > 3 && float.TryParse(t.args[3], out miniCA))
                            {

                            }
                            StartCoroutine(mbs.CreateTextAttached(miniB, speaker, miniCA, vars));
                        } else
                        {
                            Vector3 pos = speaker.GetTextTailPosition();

                            mbs.DetachedPosition(pos);
                            StartCoroutine(mbs.CreateText(miniB, speaker, vars));
                        }
                    }
                    else
                    {
                        Vector3 pos = miniCC;

                        mbs.detached = true;
                        mbs.DetachedPosition(pos);
                        StartCoroutine(mbs.CreateText(miniB, pos, vars));
                    }
                } else
                {
                    StartCoroutine(mbs.CreateTextAttached(miniB, speaker, miniCA, vars));
                }
                attachedMinibubbles.Add(mbs);

                break;
            case TagEntry.TextTag.KillMinibubbles:
                while (attachedMinibubbles.Count > 0)
                {
                    //Destroy(attachedMinibubbles[0]);
                    attachedMinibubbles[0].superDeleteSignal = true;
                    attachedMinibubbles.RemoveAt(0);
                }
                break;
            case TagEntry.TextTag.WaitMinibubbles:
                minibubbleWait = true;
                break;
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
                        } else
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
                    if (t.args[0].Equals("w") || t.args[0].Equals("wilex"))
                    {
                        hasTail = true;
                        tail.enabled = true;
                        speaker = MainManager.Instance.LocatePlayerSpeaker(MainManager.PlayerCharacter.Wilex);
                        MoveTail(speaker.GetTextTailPosition());
                    }
                    if (t.args[0].Equals("l") || t.args[0].Equals("luna"))
                    {
                        hasTail = true;
                        tail.enabled = true;
                        speaker = MainManager.Instance.LocatePlayerSpeaker(MainManager.PlayerCharacter.Luna);
                        MoveTail(speaker.GetTextTailPosition());
                    }
                    if (t.args[0].Equals("k") || t.args[0].Equals("keru"))
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
                        } else
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
                    if (speaker != null && !speaker.SpeakingAnimActive())
                    {
                        speaker.EnableSpeakingAnim();
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
                    } else
                    {
                        Debug.LogWarning("Tail Real Time Update does not make sense without a speaker");
                    }
                }
                break;
            case TagEntry.TextTag.Anim:
                int animMEID;
                ITextSpeaker targetSpeaker = null;
                bool force = false;
                if (t.args.Length > 1 && bool.TryParse(t.args[1], out force))
                {

                }
                if (int.TryParse(t.args[0], out animMEID))
                {
                    targetSpeaker = MainManager.Instance.GetSpeaker(animMEID);
                }
                if (t.args[0].Equals("o"))
                {
                    targetSpeaker = originalSpeaker;
                }
                if (t.args[0].Equals("w") || t.args[0].Equals("wilex"))
                {
                    targetSpeaker = MainManager.Instance.LocatePlayerSpeaker(MainManager.PlayerCharacter.Wilex);
                }
                if (t.args[0].Equals("l") || t.args[0].Equals("luna"))
                {
                    targetSpeaker = MainManager.Instance.LocatePlayerSpeaker(MainManager.PlayerCharacter.Luna);
                }
                if (t.args[0].Equals("k") || t.args[0].Equals("keru"))
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
                if (t.args[0].Equals("w") || t.args[0].Equals("wilex"))
                {
                    targetSpeakerD = MainManager.Instance.LocatePlayerSpeaker(MainManager.PlayerCharacter.Wilex);
                }
                if (t.args[0].Equals("l") || t.args[0].Equals("luna"))
                {
                    targetSpeakerD = MainManager.Instance.LocatePlayerSpeaker(MainManager.PlayerCharacter.Luna);
                }
                if (t.args[0].Equals("k") || t.args[0].Equals("keru"))
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
                if (t.args[0].Equals("w") || t.args[0].Equals("wilex"))
                {
                    targetSpeakerF = MainManager.Instance.LocatePlayerSpeaker(MainManager.PlayerCharacter.Wilex);
                }
                if (t.args[0].Equals("l") || t.args[0].Equals("luna"))
                {
                    targetSpeakerF = MainManager.Instance.LocatePlayerSpeaker(MainManager.PlayerCharacter.Luna);
                }
                if (t.args[0].Equals("k") || t.args[0].Equals("keru"))
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
                    } else
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
                if (t.args[0].Equals("w") || t.args[0].Equals("wilex"))
                {
                    targetSpeakerE = MainManager.Instance.LocatePlayerSpeaker(MainManager.PlayerCharacter.Wilex);
                }
                if (t.args[0].Equals("l") || t.args[0].Equals("luna"))
                {
                    targetSpeakerE = MainManager.Instance.LocatePlayerSpeaker(MainManager.PlayerCharacter.Luna);
                }
                if (t.args[0].Equals("k") || t.args[0].Equals("keru"))
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
            case TagEntry.TextTag.Prompt:
                int argCount = t.args.Length / 2;
                string[] tempText = new string[argCount];
                string[] tempArgs = new string[argCount];
                for (int i = 0; i < argCount; i++)
                {
                    tempText[i] = FormattedString.ReplaceTextFileShorthand(t.args[i * 2]);
                    tempArgs[i] = t.args[i * 2 + 1];
                }
                int tempCancel = t.args.Length % 2 == 1 ? int.Parse(t.args[t.args.Length-1]) : -1;

                menuHandler = PromptBoxMenu.BuildMenu(tempText,tempArgs,tempCancel);
                inMenu = true;
                menuHandler.menuExit += MenuExit;
                break;
            case TagEntry.TextTag.ItemMenu:
                List<Item> itemList = null;// = MainManager.Instance.playerData.itemInventory;
                List<string> stringList = null;
                List<Color?> backgroundList = null;
                List<bool> canUseList = null;
                bool zTarget = false;
                string itemDescriptor = null;
                if (t.args.Length > 0 && t.args[0].Contains("arg"))
                {
                    OWItemBoxMenu.CreationPreset p = OWItemBoxMenu.CreationPreset.Default; //how to create the thing with the data
                    if (t.args.Length > 1)
                    {
                        if (System.Enum.TryParse(t.args[1], true, out p))
                        {
                            //Debug.Log("a");
                        }
                    }

                    switch (p)
                    {
                        case OWItemBoxMenu.CreationPreset.Default:
                            itemList = Item.ParseList(FormattedString.ParseArg(menuResult, t.args[0]));
                            break;
                        case OWItemBoxMenu.CreationPreset.Pairs:
                            (itemList, stringList) = MainManager.ParseItemStringList(FormattedString.ParseArg(menuResult, t.args[0]));
                            break;
                        case OWItemBoxMenu.CreationPreset.PricePairs:
                            (itemList, stringList) = MainManager.ParseItemStringList(FormattedString.ParseArg(menuResult, t.args[0]));
                            for (int i = 0; i < stringList.Count; i++)
                            {
                                stringList[i] = stringList[i] + " <coin>";
                            }
                            break;
                        case OWItemBoxMenu.CreationPreset.Overworld:
                            itemList = MainManager.Instance.playerData.itemInventory;
                            break;
                        case OWItemBoxMenu.CreationPreset.OverworldHighlighted:
                            itemList = MainManager.Instance.playerData.itemInventory;
                            backgroundList = MainManager.ParseColorList(FormattedString.ParseArg(menuResult, t.args[0]));
                            break;
                        case OWItemBoxMenu.CreationPreset.OverworldHighlightedBlock:
                            itemList = MainManager.Instance.playerData.itemInventory;
                            backgroundList = MainManager.ParseColorList(FormattedString.ParseArg(menuResult, t.args[0]));
                            canUseList = new List<bool>();
                            for (int i = 0; i < backgroundList.Count; i++)
                            {
                                canUseList.Add(backgroundList[i] == null);
                            }
                            break;
                        case OWItemBoxMenu.CreationPreset.OverworldHighlightedBlockZ:
                            itemList = MainManager.Instance.playerData.itemInventory;
                            backgroundList = MainManager.ParseColorList(FormattedString.ParseArg(menuResult, t.args[0]));
                            canUseList = new List<bool>();
                            for (int i = 0; i < backgroundList.Count; i++)
                            {
                                canUseList.Add(backgroundList[i] == null);
                            }
                            zTarget = true;
                            break;
                        case OWItemBoxMenu.CreationPreset.Battle:
                            itemList = BattleControl.Instance.playerData.itemInventory;
                            break;
                        case OWItemBoxMenu.CreationPreset.Storage:
                            itemList = MainManager.Instance.playerData.storageInventory;
                            break;
                        case OWItemBoxMenu.CreationPreset.StorageHighlighted:
                            itemList = MainManager.Instance.playerData.storageInventory;
                            backgroundList = MainManager.ParseColorList(FormattedString.ParseArg(menuResult, t.args[0]));
                            break;
                        case OWItemBoxMenu.CreationPreset.StorageHighlightedBlock:
                            itemList = MainManager.Instance.playerData.storageInventory;
                            backgroundList = MainManager.ParseColorList(FormattedString.ParseArg(menuResult, t.args[0]));
                            canUseList = new List<bool>();
                            for (int i = 0; i < backgroundList.Count; i++)
                            {
                                canUseList.Add(backgroundList[i] == null);
                            }
                            break;
                        case OWItemBoxMenu.CreationPreset.Selling:
                            itemList = MainManager.Instance.playerData.itemInventory;
                            stringList = new List<string>();
                            for (int i = 0; i < itemList.Count; i++)
                            {
                                stringList.Add(Item.GetItemDataEntry(itemList[i].type).sellPrice + " <coin>");
                            }
                            itemDescriptor = "<coin>: <const,coins>";
                            break;
                    }
                } else
                {
                    OWItemBoxMenu.CreationPreset p = OWItemBoxMenu.CreationPreset.Default; //how to create the thing with the data
                    if (t.args.Length > 0)
                    {
                        if (System.Enum.TryParse(t.args[0], true, out p))
                        {
                            //Debug.Log("a");
                        }
                    }

                    //only the ones that don't need menuresult are here
                    switch (p)
                    {
                        case OWItemBoxMenu.CreationPreset.Overworld:
                            itemList = MainManager.Instance.playerData.itemInventory;
                            break;
                        case OWItemBoxMenu.CreationPreset.Battle:
                            itemList = BattleControl.Instance.playerData.itemInventory;
                            break;
                        case OWItemBoxMenu.CreationPreset.Storage:
                            itemList = MainManager.Instance.playerData.storageInventory;
                            break;
                        case OWItemBoxMenu.CreationPreset.Selling:
                            itemList = MainManager.Instance.playerData.itemInventory;
                            stringList = new List<string>();
                            for (int i = 0; i < itemList.Count; i++)
                            {
                                stringList.Add(Item.GetItemDataEntry(itemList[i].type).sellPrice + " <coin>");
                            }
                            itemDescriptor = "<coin>: <const,coins>";
                            break;
                    }
                }
                menuHandler = OWItemBoxMenu.BuildMenu(itemList, stringList, backgroundList, canUseList, itemDescriptor, false, false, false, zTarget, false, false);
                inMenu = true;
                menuHandler.menuExit += MenuExit;
                break;
            case TagEntry.TextTag.KeyItemMenu:
                //shade is a slightly lazy dev and just made key item a copy paste of item list
                List<KeyItem> kitemList = null;// = MainManager.Instance.playerData.itemInventory;
                List<string> kstringList = null;
                List<Color?> kbackgroundList = null;
                List<bool> kcanUseList = null;
                bool kzTarget = false;
                if (t.args.Length > 0 && t.args[0].Contains("arg"))
                {
                    OWItemBoxMenu.CreationPreset p = OWItemBoxMenu.CreationPreset.Default; //how to create the thing with the data
                    if (t.args.Length > 1)
                    {
                        if (System.Enum.TryParse(t.args[1], true, out p))
                        {
                            //Debug.Log("a");
                        }
                    }

                    switch (p)
                    {
                        case OWItemBoxMenu.CreationPreset.Default:
                            kitemList = KeyItem.ParseList(FormattedString.ParseArg(menuResult, t.args[0]));
                            break;
                        case OWItemBoxMenu.CreationPreset.Pairs:
                            (kitemList, kstringList) = MainManager.ParseKeyItemStringList(FormattedString.ParseArg(menuResult, t.args[0]));
                            break;
                        case OWItemBoxMenu.CreationPreset.PricePairs:
                            (kitemList, kstringList) = MainManager.ParseKeyItemStringList(FormattedString.ParseArg(menuResult, t.args[0]));
                            for (int i = 0; i < kstringList.Count; i++)
                            {
                                kstringList[i] = kstringList[i] + " <coin>";
                            }
                            break;
                        case OWItemBoxMenu.CreationPreset.Overworld:
                            kitemList = MainManager.Instance.playerData.keyInventory;
                            break;
                        case OWItemBoxMenu.CreationPreset.OverworldHighlighted:
                            kitemList = MainManager.Instance.playerData.keyInventory;
                            kbackgroundList = MainManager.ParseColorList(FormattedString.ParseArg(menuResult, t.args[0]));
                            break;
                        case OWItemBoxMenu.CreationPreset.OverworldHighlightedBlock:
                            kitemList = MainManager.Instance.playerData.keyInventory;
                            kbackgroundList = MainManager.ParseColorList(FormattedString.ParseArg(menuResult, t.args[0]));
                            kcanUseList = new List<bool>();
                            for (int i = 0; i < kbackgroundList.Count; i++)
                            {
                                kcanUseList.Add(kbackgroundList[i] == null);
                            }
                            break;
                        case OWItemBoxMenu.CreationPreset.OverworldHighlightedBlockZ:
                            kitemList = MainManager.Instance.playerData.keyInventory;
                            kbackgroundList = MainManager.ParseColorList(FormattedString.ParseArg(menuResult, t.args[0]));
                            kcanUseList = new List<bool>();
                            for (int i = 0; i < kbackgroundList.Count; i++)
                            {
                                kcanUseList.Add(kbackgroundList[i] == null);
                            }
                            kzTarget = true;
                            break;
                        case OWItemBoxMenu.CreationPreset.Battle:
                            kitemList = BattleControl.Instance.playerData.keyInventory;
                            break;
                    }
                }
                else
                {
                    OWItemBoxMenu.CreationPreset p = OWItemBoxMenu.CreationPreset.Default; //how to create the thing with the data
                    if (t.args.Length > 0)
                    {
                        if (System.Enum.TryParse(t.args[0], true, out p))
                        {
                            //Debug.Log("a");
                        }
                    }

                    //only the ones that don't need menuresult are here
                    switch (p)
                    {
                        case OWItemBoxMenu.CreationPreset.Overworld:
                            kitemList = MainManager.Instance.playerData.keyInventory;
                            break;
                        case OWItemBoxMenu.CreationPreset.Battle:
                            kitemList = BattleControl.Instance.playerData.keyInventory;
                            break;
                    }
                }
                menuHandler = OWKeyItemBoxMenu.BuildMenu(kitemList, kstringList, kbackgroundList, kcanUseList, null, false, false, kzTarget, false);
                inMenu = true;
                menuHandler.menuExit += MenuExit;
                break;
            case TagEntry.TextTag.NumberMenu:
                int nmMin = 0;
                int nmStart = 0;
                int nmMax = 0;
                if (t.args.Length > 0 && int.TryParse(t.args[0], out nmMin))
                {
                }
                if (t.args.Length > 1 && int.TryParse(t.args[1], out nmStart))
                {
                }
                if (t.args.Length > 2 && int.TryParse(t.args[2], out nmMax))
                {
                }
                menuHandler = NumberMenu.BuildMenu(nmMax, nmStart, nmMin);
                inMenu = true;
                menuHandler.menuExit += MenuExit;
                break;
            case TagEntry.TextTag.TextEntryMenu:
                int temMax = 0;
                if (t.args.Length > 0 && int.TryParse(t.args[0], out temMax))
                {
                }
                string startValue = "";
                if (t.args.Length > 1)
                {
                    startValue = t.args[1];
                    if (t.args[1].Contains("arg"))
                    {
                        startValue = FormattedString.ParseArg(menuResult, t.args[1]);
                    }
                }
                menuHandler = TextEntryMenu.BuildMenu(temMax, startValue);
                inMenu = true;
                menuHandler.menuExit += MenuExit;
                break;
            case TagEntry.TextTag.GenericMenu:
                //Extremely complex tag
                List<string> baseList = null;
                List<string> nameList = null;
                List<string> rightList = null;
                List<bool> usageList = null;
                List<string> descList = null;
                List<int> maxLevelList = null;
                List<Color?> colorList = null;
                string levelDescriptor = null;
                string descriptor = null;
                bool hasLevelDescriptor = false;
                bool hasDescriptor = false;
                bool canUseDisabled = false;
                bool canZSelect = false;
                bool showAtOne = false;
                if (t.args.Length > 0 && t.args[0].Contains("arg"))
                {
                    int g = 0;
                    if (t.args.Length > 1)
                    {
                        if (int.TryParse(t.args[1], out g))
                        {
                        }
                    }

                    if (t.args.Length > 2)
                    {
                        if (bool.TryParse(t.args[2], out showAtOne))
                        {
                        }
                    }

                    if (t.args.Length > 3)
                    {
                        if (bool.TryParse(t.args[3], out canUseDisabled))
                        {
                        }
                    }

                    if (t.args.Length > 4)
                    {
                        if (bool.TryParse(t.args[4], out canZSelect))
                        {
                        }
                    }

                    if (t.args.Length > 5)
                    {
                        if (bool.TryParse(t.args[5], out hasLevelDescriptor))
                        {
                        }
                        //levelDescriptor = new string(t.args[4]);
                    }

                    if (t.args.Length > 6)
                    {
                        if (bool.TryParse(t.args[6], out hasDescriptor))
                        {
                        }
                        //levelDescriptor = new string(t.args[4]);
                    }
                    //Debug.Log(hasDescriptor);

                    baseList = MainManager.ParsePipeStringList(FormattedString.ParseArg(menuResult, t.args[0]));
                    //Debug.Log(FormattedString.ParseArg(menuResult, t.args[0]));
                    //Debug.Log(baseList.Count);

                    nameList = new List<string>();
                    if (g > 1)
                    {
                        rightList = new List<string>();
                    }
                    if (g > 2)
                    {
                        usageList = new List<bool>();
                    }
                    if (g > 3)
                    {
                        descList = new List<string>();
                    }
                    if (g > 4)
                    {
                        maxLevelList = new List<int>();
                    }
                    if (g > 5)
                    {
                        colorList = new List<Color?>();
                    }
                    bool g_3 = true;
                    int g_5 = 1;
                    Color? g_6 = null;
                    for (int i = 0; i < baseList.Count; i++)
                    {
                        if (hasDescriptor && i == baseList.Count - 1)
                        {
                            descriptor = new string(baseList[i]);
                            break;
                        }
                        if (hasLevelDescriptor && hasDescriptor && i == baseList.Count - 2)
                        {
                            levelDescriptor = new string(baseList[i]);
                            continue;
                        }
                        if (hasLevelDescriptor && !hasDescriptor && i == baseList.Count - 1)
                        {
                            levelDescriptor = new string(baseList[i]);
                            break;
                        }
                        g_3 = true;
                        g_5 = 1;
                        switch (i % g)
                        {
                            case 0:
                                nameList.Add(baseList[i]);
                                //Debug.Log("name: " + baseList[i]);
                                break;
                            case 1:
                                rightList.Add(baseList[i]);
                                //Debug.Log("right: " + baseList[i]);
                                break;
                            case 2:
                                bool.TryParse(baseList[i], out g_3);
                                usageList.Add(g_3);
                                //Debug.Log("usage: " + g_3);
                                break;
                            case 3:
                                descList.Add(baseList[i]);
                                //Debug.Log("desc: " + baseList[i]);
                                break;
                            case 4:
                                int.TryParse(baseList[i], out g_5);
                                //Debug.Log("max level: " + g_5);
                                maxLevelList.Add(g_5);
                                break;
                            case 5:
                                g_6 = MainManager.ParseColor(baseList[i]);  //Null/invalid color => no background
                                colorList.Add(g_6);
                                break;
                        }
                    }
                }
                menuHandler = GenericBoxMenu.BuildMenu(nameList, rightList, canUseDisabled, canZSelect, usageList, descList, showAtOne, levelDescriptor, descriptor, maxLevelList, colorList);
                inMenu = true;
                menuHandler.menuExit += MenuExit;
                break;
            case TagEntry.TextTag.RemoveItem: //this code is very bad
                bool indexOrType = false; //remove by index if true

                int index = -1;
                Item.ItemType itemType = (Item.ItemType)(-1); //illegal enum casting?
                bool itemBool2 = false;
                bool removeAll = false;

                if (t.args[0].Contains("arg"))
                {
                    string arg = FormattedString.ParseArg(menuResult, t.args[0]);
                    indexOrType = true;
                    int.TryParse(arg, out index);
                } else if (int.TryParse(t.args[0], out index))
                {
                    indexOrType = true;
                } else if (System.Enum.TryParse(t.args[0], out itemType))
                {
                    indexOrType = false;
                }

                if (index == -1 && indexOrType)
                {
                    break;
                }

                if (t.args.Length > 1)
                {
                    if (bool.TryParse(t.args[1], out itemBool2))
                    {

                    }
                    else
                    {
                        itemBool2 = false;
                    }
                }
                if (t.args.Length > 2)
                {
                    if (bool.TryParse(t.args[2], out removeAll))
                    {

                    }
                    else
                    {
                        removeAll = false;
                    }
                }
                

                if (indexOrType)
                {
                    if (itemBool2)
                    {
                        /*
                        if (removeAll)
                        {
                            itemType = MainManager.Instance.playerData.keyInventory[index].type;
                            MainManager.Instance.playerData.keyInventory.RemoveAll((e) => (e.type == itemType));
                        } else
                        {
                            MainManager.Instance.playerData.keyInventory.RemoveAt(index);
                        }
                        */
                    } else
                    {
                        if (removeAll)
                        {
                            itemType = MainManager.Instance.playerData.itemInventory[index].type;
                            MainManager.Instance.playerData.itemInventory.RemoveAll((e) => (e.type == itemType));
                        }
                        else
                        {
                            MainManager.Instance.playerData.itemInventory.RemoveAt(index);
                        }
                    }
                } else
                {
                    if (itemBool2)
                    {
                        /*
                        if (removeAll)
                        {
                            MainManager.Instance.playerData.keyInventory.RemoveAll((e) => (e.type == itemType));
                        }
                        else
                        {
                            Item i = MainManager.Instance.playerData.keyInventory.Find((e) => (e.type == itemType));
                            MainManager.Instance.playerData.keyInventory.Remove(i);
                        }
                        */
                    }
                    else
                    {
                        if (removeAll)
                        {
                            MainManager.Instance.playerData.itemInventory.RemoveAll((e) => (e.type == itemType));
                        }
                        else
                        {
                            Item i = MainManager.Instance.playerData.itemInventory.Find((e) => (e.type == itemType));
                            MainManager.Instance.playerData.itemInventory.Remove(i);
                        }
                    }
                }
                break;
            case TagEntry.TextTag.Goto:
            case TagEntry.TextTag.Branch:
                seeSpecialTag?.Invoke(this, new TextDisplayer.ScrollEventArgs(t));
                break;
            case TagEntry.TextTag.Set:
                //set A to immediate value
                if (t.args[0].Contains("arg"))
                {
                    menuResult = FormattedString.SetMenuResult(menuResult, t.args[1], t.args[0]);
                    //Debug.Log("Set " + t.args[0] + " to (i)" + t.args[1]);
                } else
                {
                    if (t.args.Length < 3)
                    {
                        Debug.LogError("Not enough arguments for set tag.");
                    }
                    FormattedString.SetNonlocalVar(t.args[0], t.args[1], t.args[2]);
                    Debug.Log("Set " + t.args[0] + " " + t.args[1] + "to (i)" + t.args[2]);
                }
                break;
            case TagEntry.TextTag.SetVar:
                //set A to B (basically A = B)
                string arg1a;
                string arg1b;
                string arg2a;
                string arg2b;

                //int argIndex = -1;

                arg1a = t.args[0];
                if (arg1a.Contains("arg"))
                {
                    arg2a = t.args[1];
                    arg2b = t.args[2];

                    //Set arg to value
                    string value = FormattedString.ParseNonlocalVar(arg2a, arg2b);
                    menuResult = FormattedString.SetMenuResult(menuResult, value, t.args[0]);
                    //menuResult = new MenuResult(value);
                    Debug.Log("Set arg to " + value);
                }
                else
                {
                    arg1b = t.args[1];
                    arg2a = t.args[2];
                    if (!arg2a.Contains("arg"))
                    {
                        arg2b = t.args[3];

                        string value = FormattedString.ParseNonlocalVar(arg2a, arg2b);
                        FormattedString.SetNonlocalVar(arg1a, arg1b, value);
                        Debug.Log("Set " + arg1a + " " + arg1b + " to " + value);
                    }
                    else
                    {
                        //Set value to arg
                        string arg = FormattedString.ParseArg(menuResult, arg2a);
                        FormattedString.SetNonlocalVar(arg1a, arg1b, arg);
                        Debug.Log("Set " + arg1a + " " + arg1b + " to (a)" + arg);
                    }
                }

                break;
            case TagEntry.TextTag.DataGet:
                //<dataget,[arg?],[request]>
                //similar setup to SetVar
                //value in A is set to the result of sending the entity the text in B
                //set A to B (basically A = B)
                //however I am forcing A to be arg because you can set nonlocal variables in the entity side
                //The request also cannot be a variable since you can also check the variable value in the entity side
                //  (even the value of arg can be checked by using FormattedString.ParseArg and MainManager.Instance.lastTextboxMenuResult)
                string argdg1a;
                string argdg2a;

                //int argIndex = -1;

                if (t.args.Length > 2)
                {
                    ITextSpeaker dataGetSpeaker = null;
                    int dataGetSpeakerID;
                    if (int.TryParse(t.args[0], out dataGetSpeakerID))
                    {
                        dataGetSpeaker = MainManager.Instance.GetSpeaker(dataGetSpeakerID);
                    }
                    if (t.args[0].Equals("o"))
                    {
                        dataGetSpeaker = originalSpeaker;
                    }
                    if (t.args[0].Equals("w") || t.args[0].Equals("wilex"))
                    {
                        dataGetSpeaker = MainManager.Instance.LocatePlayerSpeaker(MainManager.PlayerCharacter.Wilex);
                    }
                    if (t.args[0].Equals("l") || t.args[0].Equals("luna"))
                    {
                        dataGetSpeaker = MainManager.Instance.LocatePlayerSpeaker(MainManager.PlayerCharacter.Luna);
                    }

                    argdg1a = t.args[1];
                    if (argdg1a.Contains("arg"))
                    {
                        argdg2a = t.args[2];

                        //Set arg to value
                        string value = "";
                        if (dataGetSpeaker != null)
                        {
                            value = dataGetSpeaker.RequestTextData(argdg2a);
                            //Debug.Log("dataget: " + value);
                        }
                        else
                        {
                            Debug.LogWarning("DataGet used in a null speaker context (using a different MEID) with request " + argdg2a);
                        }
                        menuResult = FormattedString.SetMenuResult(menuResult, value, t.args[1]);
                        //menuResult = new MenuResult(value);
                        Debug.Log("Set arg to " + value);
                    }
                    else
                    {
                        Debug.LogWarning("DataGet does not work with non-arg variables (That must be done entity side with DataSend)");
                    }
                } else
                {
                    argdg1a = t.args[0];
                    if (argdg1a.Contains("arg"))
                    {
                        argdg2a = t.args[1];

                        //Set arg to value
                        string value = "";
                        if (speaker != null)
                        {
                            value = speaker.RequestTextData(argdg2a);
                        }
                        else
                        {
                            Debug.LogWarning("DataGet used in a null speaker context with request " + argdg2a);
                        }
                        menuResult = FormattedString.SetMenuResult(menuResult, value, t.args[0]);
                        //menuResult = new MenuResult(value);
                        //Debug.Log("Set arg to " + value);
                    }
                    else
                    {
                        Debug.LogWarning("DataGet does not work with non-arg variables (That must be done entity side with DataSend)");
                    }
                }
                break;
            case TagEntry.TextTag.DataSend:
                //<datasend,[data]>
                //send value in A (non-variable)
                //Note: no DataSendVar because you can read variables in the speaker's script
                string argds1a;

                argds1a = t.args[0];
                int dataSendMEID;
                ITextSpeaker dataSendSpeaker = null;

                if (t.args.Length > 1)
                {
                    if (int.TryParse(t.args[0], out dataSendMEID))
                    {
                        dataSendSpeaker = MainManager.Instance.GetSpeaker(dataSendMEID);
                    }
                    if (t.args[0].Equals("o"))
                    {
                        dataSendSpeaker = originalSpeaker;
                    }
                    if (t.args[0].Equals("w") || t.args[0].Equals("wilex"))
                    {
                        dataSendSpeaker = MainManager.Instance.LocatePlayerSpeaker(MainManager.PlayerCharacter.Wilex);
                    }
                    if (t.args[0].Equals("l") || t.args[0].Equals("luna"))
                    {
                        dataSendSpeaker = MainManager.Instance.LocatePlayerSpeaker(MainManager.PlayerCharacter.Luna);
                    }
                    if (dataSendSpeaker != null)
                    {
                        dataSendSpeaker.SendTextData(t.args[1]);
                    }
                    else
                    {
                        Debug.LogWarning("DataSend used in a null speaker context (using a different MEID) with data " + argds1a);
                    }
                }
                else
                {
                    if (speaker != null)
                    {
                        speaker.SendTextData(argds1a);
                    }
                    else
                    {
                        Debug.LogWarning("DataSend used in a null speaker context with data " + argds1a);
                    }
                }
                break;
        }

        //hacky fix?
        //wasn't actually the problem at the time but I think I should keep this in here anyway
        if (inMenu)
        {
            text.bPress = false;
            text.aPressTime = 0;
        }
    }

    public void Bleep()
    {

    }

    public void MenuExit(object sender, MenuExitEventArgs meea)
    {
        Debug.Log("Menu exit");
        inMenu = false;
        if (speaker != null && !speaker.SpeakingAnimActive())
        {
            speaker.EnableSpeakingAnim();
        }
    }

    public string GetMenuResultString()
    {
        if (menuResult == null)
        {
            return "";
        }
        return menuResult.output.ToString();
    }

    public IEnumerator StartAnim()
    {
        animTime = 0;
        RectTransform rt = baseObject.GetComponent<RectTransform>();

        tail.enabled = hasTail;
        PointTail();

        originalPos = rt.anchoredPosition;  //end position?

        Vector3 newAnchoredPos = MainManager.Instance.WorldPosToCanvasPosC(tailPointPos);   //start position
        //Debug.Log(newAnchoredPos);

        //currently I anchored it at top middle so I have to offset that
        newAnchoredPos.y = -MainManager.CanvasHeight() + newAnchoredPos.y;
        newAnchoredPos.x -= MainManager.CanvasWidth() / 2;
        if (MainManager.Instance.IsPositionBehindCamera(tailPointPos))
        {
            //Debug.Log("behind");
            newAnchoredPos.y *= -1;
            newAnchoredPos.y += MainManager.CanvasHeight();
            newAnchoredPos.x *= -1;
        }

        //fixing this
        //
        //should be (-247, -383)

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
        //Debug.Log(newAnchoredPos);

        //currently I anchored it at top middle so I have to offset that
        newAnchoredPos.y = -MainManager.CanvasHeight() + newAnchoredPos.y;
        newAnchoredPos.x -= MainManager.CanvasWidth() / 2;
        if (MainManager.Instance.IsPositionBehindCamera(tailPointPos))
        {
            //Debug.Log("behind");
            newAnchoredPos.y *= -1;
            newAnchoredPos.y += MainManager.CanvasHeight();
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
    public IEnumerator CreateText(string dialogue, string[] vars = null) //string[] lines
    {
        //Debug.Log("Create");
        tail.enabled = false;
        this.vars = vars;

        attachedMinibubbles = new List<MinibubbleScript>();

        string dialogueB = FormattedString.ParseCutTags(dialogue);

        //Initial tags may change the start anim
        List<TagEntry> startTags = (new List<TagEntry>(new FormattedString(dialogueB).tags)).FindAll((t) => (t.trueStartIndex == 0));
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
                        if (t.args[0].Equals("w") || t.args[0].Equals("wilex"))
                        {
                            hasTail = true;
                            tail.enabled = true;
                            speaker = MainManager.Instance.LocatePlayerSpeaker(MainManager.PlayerCharacter.Wilex);
                            MoveTail(speaker.GetTextTailPosition());
                        }
                        if (t.args[0].Equals("l") || t.args[0].Equals("luna"))
                        {
                            hasTail = true;
                            tail.enabled = true;
                            speaker = MainManager.Instance.LocatePlayerSpeaker(MainManager.PlayerCharacter.Luna);
                            MoveTail(speaker.GetTextTailPosition());
                        }
                        if (t.args[0].Equals("k") || t.args[0].Equals("keru"))
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

        nextSkipList = new List<bool>();
        string[] tempLines = FormattedString.SplitByTags(dialogueB, true, TagEntry.TextTag.Next, TagEntry.TextTag.End, TagEntry.TextTag.CondEnd);
        for (int i = 0; i < tempLines.Length; i++)
        {
            TagEntry t;
            if (TagEntry.TryParse(tempLines[i],out t))
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
                nextSkipList.Add(test);
            }
        }
        nextSkipList.Add(false); //to avoid index errors
        //Note that the size of nextSkipList must always be within 1 of the number of lines since each line is separated by a tag
        //Note 2: A next tag at the start will cause the first line to be empty string, while a next string at the end will not cause the final line to be ""
        //Note 3: Two next tags adjacent to each other will have a "" between them

        string[] lines = FormattedString.SplitByTag(dialogueB, false, TagEntry.TextTag.Next);

        this.lines = new List<string>(lines);
        currLine = 0;
        latestLine = 0;

        if (this.lines.Count == 0)
        {
            this.lines = new List<string> { "<color,red>Empty string passed to TextboxScript (Bug)</color>" };
        }

        text.tagScroll += SeeTag;
        text.SetText(this.lines[currLine], vars);

        while (!textDone)
        {
            yield return null;
        }

        text.tagScroll -= SeeTag;

        if (!noEndAnim)
        {
            yield return StartCoroutine(EndAnim());
        }
    }
    public IEnumerator CreateText(string dialogue, Vector3 tailPos, string[] vars = null)
    {
        Debug.Log("Create with tail");
        tail.enabled = true;
        tailPointPos = tailPos;
        ConvertTailPos();
        PointTail();
        this.vars = vars;

        attachedMinibubbles = new List<MinibubbleScript>();

        hasTail = true;

        string dialogueB = FormattedString.ParseCutTags(dialogue);

        //Initial tags may change the start anim
        List<TagEntry> startTags = (new List<TagEntry>(new FormattedString(dialogueB).tags)).FindAll((t) => (t.trueStartIndex == 0));
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
                        if (t.args[0].Equals("w") || t.args[0].Equals("wilex"))
                        {
                            hasTail = true;
                            tail.enabled = true;
                            speaker = MainManager.Instance.LocatePlayerSpeaker(MainManager.PlayerCharacter.Wilex);
                            MoveTail(speaker.GetTextTailPosition());
                        }
                        if (t.args[0].Equals("l") || t.args[0].Equals("luna"))
                        {
                            hasTail = true;
                            tail.enabled = true;
                            speaker = MainManager.Instance.LocatePlayerSpeaker(MainManager.PlayerCharacter.Luna);
                            MoveTail(speaker.GetTextTailPosition());
                        }
                        if (t.args[0].Equals("k") || t.args[0].Equals("keru"))
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

        nextSkipList = new List<bool>();
        string[] tempLines = FormattedString.SplitByTags(dialogueB, true, TagEntry.TextTag.Next, TagEntry.TextTag.End, TagEntry.TextTag.CondEnd);
        for (int i = 0; i < tempLines.Length; i++)
        {
            TagEntry t;
            if (TagEntry.TryParse(tempLines[i], out t))
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
                nextSkipList.Add(test);
            }
        }
        nextSkipList.Add(false); //to avoid index errors at end
        //Note that the size of nextSkipList must always be within 1 of the number of lines since each line is separated by a tag
        //Note 2: A next tag at the start will cause the first line to be empty string, while a next string at the end will not cause the final line to be ""
        //Note 3: Two next tags adjacent to each other will have a "" between them

        string[] lines = FormattedString.SplitByTags(dialogueB, false, TagEntry.TextTag.Next, TagEntry.TextTag.End, TagEntry.TextTag.CondEnd);

        this.lines = new List<string>(lines);
        currLine = 0;
        latestLine = 0;

        if (this.lines.Count == 0)
        {
            this.lines = new List<string> { "<color,red>Empty string passed to TextboxScript (Bug)</color>" };
        }

        text.tagScroll += SeeTag;
        text.SetText(this.lines[currLine], vars);

        while (!textDone)
        {
            yield return null;
        }

        text.tagScroll -= SeeTag;

        if (!noEndAnim)
        {
            yield return StartCoroutine(EndAnim());
        }
    }
    public IEnumerator CreateText(string dialogue, ITextSpeaker speaker, string[] vars = null)
    {
        Debug.Log("Create with speaker");
        this.originalSpeaker = speaker;
        this.speaker = speaker;
        Vector3 position = Vector3.zero;
        if (speaker != null)
        {
            position = speaker.GetTextTailPosition();
        }
        yield return StartCoroutine(CreateText(dialogue, position, vars));
    }


    public void ResetText()
    {
        text.StopEffectCoroutine();
        text.SetText(lines[currLine], vars, currLine <= latestLine);
        //Debug.Log("g");
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
            MoveTail(speaker.GetTextTailPosition());
        }

        if (!inMenu)
        {
            if (menuHandler != null)
            {
                menuResult = menuHandler.GetFullResult();
                if (menuResult != null)
                {
                    MainManager.Instance.lastTextboxMenuResult = menuResult.output.ToString();
                    //Debug.Log("Get Result");
                } else
                {
                    MainManager.Instance.lastTextboxMenuResult = null;
                }
                nextSkipList[currLine] = true; //Required to fix how menus work
                menuHandler.Clear();
                Destroy(menuHandler.gameObject);
                menuHandler = null;
            }

            if (InputManager.GetButtonDown(InputManager.Button.A))
            {
                text.aPressTime = TextDisplayer.aPressBoostTime;
            }
            text.bPress = InputManager.GetButton(InputManager.Button.B);

            if (currLine < latestLine)
            {
                nextButton.color = nextButtonColorB;
            }
            else if (text.scrollDone)
            {
                if (minibubbleWait)
                {
                    //do nothing except check for minibubblewait to be disabled
                    bool check = true;
                    for (int i = 0; i < attachedMinibubbles.Count; i++)
                    {
                        if (attachedMinibubbles[i] == null)
                        {
                            attachedMinibubbles.RemoveAt(i);
                            i--;
                            continue;
                        }
                        check &= attachedMinibubbles[i].textDone;
                    }
                    minibubbleWait = !check;    //so if it is false no change, if it is true then all bubbles are done, so stop waiting
                } else
                {
                    nextButton.color = nextButtonColorA;
                    if (speaker != null && speaker.SpeakingAnimActive())
                    {
                        speaker.DisableSpeakingAnim();
                    }
                }
            }
            else
            {
                if (speaker != null && !speaker.SpeakingAnimActive())
                {
                    speaker.EnableSpeakingAnim();
                }
                nextButton.color = new Color(0, 0, 0, 0);

                //Bleep logic

                if (lastCharsVisible != text.charsVisible)
                {
                    if (text.charsVisible < text.cleanString.Length && text.cleanString[text.charsVisible] != ' ')
                    {
                        realVisibleCount += 1;
                    }
                }

                if (realVisibleCount > lastBleepCount + bleepDelta || lastBleepCount == 0)
                {
                    lastBleepCount = realVisibleCount;
                    if (speaker != null)
                    {
                        speaker.TextBleep();
                    }
                }

                lastCharsVisible = text.charsVisible;
            }

            if (scrollBufferTime > 0)
            {
                scrollBufferTime -= Time.deltaTime;
            }

            
            if (text.scrollDone && !minibubbleWait)
            {
                if (nextSkipList[currLine])
                {
                    //currLine++;
                    noReturn = currLine+1;
                    if (currLine > lines.Count - 1)
                    {
                        //Destroy(gameObject);
                        textDone = true;
                    }
                    else
                    {
                        TryScrollForwards();
                    }
                } else 
                if ((InputManager.GetButtonDown(InputManager.Button.A) || (InputManager.GetButton(InputManager.Button.B) && currLine >= latestLine)) && scrollBufferTime <= 0)
                {
                    TryScrollForwards();
                } else
                if ((InputManager.GetButtonDown(InputManager.Button.Z) && currLine > noReturn) && scrollBufferTime <= 0)
                {
                    TryScrollBackwards();
                }
            }
        } else
        {
            if (speaker != null && speaker.SpeakingAnimActive())
            {
                speaker.DisableSpeakingAnim();
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

        for (int i = 0; i < attachedMinibubbles.Count; i++)
        {
            if (attachedMinibubbles[i] == null)
            {
                attachedMinibubbles.RemoveAt(i);
                i--;
                continue;
            }
            attachedMinibubbles[i].deleteSignal = true;
        }

        if (currLine > lines.Count - 1)
        {
            //Destroy(gameObject);
            textDone = true;
        }
        else
        {
            text.StopEffectCoroutine();
            text.SetText(lines[currLine], vars, currLine <= latestLine);
            //text.StartEffectCoroutine();
            if (currLine > latestLine)
            {
                latestLine++;
            }
        }
    }

    void TryScrollBackwards()
    {
        if (scrollBufferTime > 0)
        {
            return;
        }

        scrollBufferTime = scrollBufferBaseTime;

        for (int i = 0; i < attachedMinibubbles.Count; i++)
        {
            if (attachedMinibubbles[i] == null)
            {
                attachedMinibubbles.RemoveAt(i);
                i--;
                continue;
            }
            attachedMinibubbles[i].deleteSignal = true;
        }

        currLine--;
        if (currLine < 0)
        {
            currLine = 0;
        }
        else
        {
            scrollBufferTime = scrollBufferBaseTime;
            text.StopEffectCoroutine();
            text.SetText(lines[currLine], vars, true);
        }
    }

    public ITextSpeaker GetSpeaker()
    {
        return speaker;
    }
}
