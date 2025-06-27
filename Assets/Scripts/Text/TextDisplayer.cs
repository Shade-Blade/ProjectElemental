using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TMPro;
using Unity.Collections;
using UnityEngine;

public class TextDisplayer : MonoBehaviour
{
    public TMP_Text textMesh;

    //no longer used (becomes a clone of input to SetText)
    //(note: can check for null here to see if it is uninitialized)
    public string inputString = null;

    public string cleanString;
    string rawString;
    List<TagEntry> tags;
    List<TagEntry> unseenTags;

    List<TagEntry> scrollTags;

    //special text effects
    List<TextEffectIndex> rainbowIndices;
    List<TextEffectIndex> wavyIndices;
    List<TextEffectIndex> shakyIndices;
    List<TextEffectIndex> scrambleIndices;
    List<TextEffectIndex> jumpIndices;


    List<TextEffectIndex> fiShrinkIndices;
    List<TextEffectIndex> fiGrowIndices;
    List<TextEffectIndex> fiSpinIndices;
    List<TextEffectIndex> fiAppearIndices;
    List<TextEffectIndex> fiWaveIndices;

    List<TextEffectSet> multiIndices;

    public int charsVisible = 0;
    float charsVisibleCont = 0;
    float fadeinAnimTime = 0;       //usually = charsVisibleCont but can go higher than however many characters are visible (so that the last characters animate correctly)
    float scrollWaitTime = 0;
    public bool scrollDone = false;
    public bool lockButtons; //prevent you from scrolling with buttons  (Note: TextboxScript handles the input so there is no code using this here)

    bool semitransparent = false;

    //Note: Handled in TextboxScript (the input is)
    //TextDisplayer only control scrolling
    public float aPressTime = 0;
    public bool bPress = false;


    //constants
    public const float aPressBoostTime = 0.25f;
    public const float bPressBoost = 10;

    protected const float defaultScrollSpeed = 15;
    protected float scrollSpeed = 15;

    protected const float resolution = 0.016f;
    protected const float hsvMult = 60f;
    protected const float timeMult = 180f;
    protected const float saturation = 0.5f;
    protected const float value = 1f;

    protected const float shakyradius = 4f;

    protected const float wavyradius = 1.25f;
    protected const float wavyomega = 8f;
    protected const float wavycharoffset = 0.5f;

    public bool hasTextChanged = false;

    Coroutine effectCor;
    int coroutineIndex = 0;     //for dumb reasons it isn't starting and stopping properly (no way to correctly stop the thing)
    //Each effect coroutine call gets a different index and gets halted if this index is not equal to its own

    List<GameObject> specialSprites;

    //takes the font size of the textmesh
    private float defaultFontSize;
    private float currentFontSize;

    //happens before accessing tags
    public event System.EventHandler<ScrollEventArgs> tagScroll;
    public class ScrollEventArgs : System.EventArgs
    {
        public readonly TagEntry tag;

        public ScrollEventArgs(TagEntry p_tag)
        {
            tag = p_tag;
        }
    }

    public virtual void Start()
    {
        if (textMesh == null) //(GetComponent<TMP_Text>() != null)
        {
            textMesh = GetComponent<TMP_Text>();
        }
        //defaultFontSize = textMesh.fontSize;
        /*
        inputString = textMesh.text;
        SetText(inputString);
        */

        /*
        StartCoroutine(RainbowText(rainbowIndices));
        StartCoroutine(ShakyText(shakyIndices));
        StartCoroutine(WavyText(wavyIndices));
        */

        //effectCor = StartCoroutine(AllEffects()); //rainbowIndices, wavyIndices, shakyIndices));        
    }

    public void StartEffectCoroutine(int offset)
    {
        //Debug.Log("Start a coroutine");
        //Debug.Log(effectCor != null);

        //Avoid memory leak? (and other problems)

        //Debug.Log(Time.time);
        if (effectCor != null)
        {
            //Debug.Log("Reset");
            StopCoroutine(effectCor);
            //return;
        }
        

        coroutineIndex++;

        //note: effect coroutine fails to start if it is not enabled / active
        if (isActiveAndEnabled)
        {
            effectCor = StartCoroutine(AllEffects(coroutineIndex, offset));
        }
    }
    public void StopEffectCoroutine()
    {
        //doesn't work?
        if (effectCor != null)
        {
            StopCoroutine(effectCor);
        }
        effectCor = null;
    }

    public virtual void SetTextNoFormat(string text)
    {
        DestroySpecialSprites();

        inputString = (string)text.Clone();

        cleanString = text;
        rawString = text;
        tags = new List<TagEntry>();

        textMesh.text = cleanString;
        TMP_TextInfo textInfo = textMesh.textInfo;
        textInfo.ClearAllMeshInfo();
        textMesh.ForceMeshUpdate();

        hasTextChanged = true;
        charsVisible = 0;
        charsVisibleCont = 0;
        fadeinAnimTime = 0;

        StartEffectCoroutine(0);
    }
    public virtual void SetText(string text, string[] vars, bool complete = false, bool forceOpaque = false, float fontSize = -1)
    {
        //Debug.Log("td " + (vars != null ? vars[0] : ""));
        //if the vars are tags then this will apply them here
        //don't do that though
        string fixedString = FormattedString.ParseVars(text, vars);

        SetText(fixedString, complete, forceOpaque, fontSize);
    }

    public virtual void SetText(string text, bool complete = false, bool forceOpaque = false, float fontSize = -1)
    {
        //Debug.Log(text);

        //fail
        if (!enabled)
        {
            textMesh.text = "";
            return;
        }

        if (defaultFontSize == 0)
        {
            defaultFontSize = textMesh.fontSize;
        }
        if (fontSize < 0)
        {
            textMesh.fontSize = defaultFontSize;
        } else
        {
            textMesh.fontSize = fontSize;
        }

        DestroySpecialSprites();
        specialSprites = new List<GameObject>();

        //just to be safe and avoid null reference exceptions
        //not sure what is setting text to null
        if (text == null)
        {
            text = "";
        }
        inputString = (string)text.Clone();

        semitransparent = complete && !forceOpaque;
        //Debug.Log("a");

        //Debug.Log(new FormattedString(text).GetCleanString(true));

        //need to preprocess and remove strings that resolve as actual text
        //(so variables and stuff)
        string internalString = FormattedString.ReplaceTextFileTags(text);
        internalString = FormattedString.ParseCutTags(internalString);
        internalString = FormattedString.ParseLines(internalString);
        internalString = FormattedString.ParseSymbols(internalString);
        internalString = FormattedString.ParseNonlocalVars(internalString);

        //new addition: ParseNonLocalVars here (to fix problem with wrong string lengths)
        //  Also put this in the tags setup thing
        cleanString = new FormattedString(internalString).GetCleanString(true);
        //Debug.Log(cleanString);
        rawString = text;
        //this looks hacky
        tags = new List<TagEntry>(new FormattedString(internalString).tags);
        unseenTags = tags.ConvertAll((e) => (e)); //List does not have a clone method so I have to improvise

        if (MainManager.Instance.Cheat_InvisibleText)
        {
            string newClean = "";
            for (int i = 0; i < cleanString.Length; i++)
            {
                newClean += " ";
            }
            cleanString = newClean;
        }

        //need to put this high up
        string formattedString = new TMPString(cleanString, tags).ToString();
        textMesh.text = formattedString;

        //this puts tags in the same order as they are in normal text (assuming the indices are valid)
        tags.Sort((a, b) => (a.startIndex == b.startIndex ? a.trueStartIndex - b.trueStartIndex : a.startIndex - b.startIndex));

        //All tags that affect text scrolling
        if (complete)
        {
            scrollTags = new List<TagEntry>();
        } else
        {
            scrollTags = tags.FindAll((e) => (e.tag == TagEntry.TextTag.Scroll || e.tag == TagEntry.TextTag.NoScroll || e.tag == TagEntry.TextTag.Sign || e.tag == TagEntry.TextTag.System || e.tag == TagEntry.TextTag.Wait));
        }

        //determine effect indices
        bool rainbow = false;
        bool wavy = false;
        bool shaky = false;
        bool scramble = false;
        bool jump = false;

        bool fiShrink = false;
        bool fiGrow = false;
        bool fiSpin = false;
        bool fiAppear = false;
        bool fiWave = false;



        rainbowIndices = new List<TextEffectIndex>();
        wavyIndices = new List<TextEffectIndex>();
        shakyIndices = new List<TextEffectIndex>();
        scrambleIndices = new List<TextEffectIndex>();
        jumpIndices = new List<TextEffectIndex>();

        fiShrinkIndices = new List<TextEffectIndex>();
        fiGrowIndices = new List<TextEffectIndex>();
        fiSpinIndices = new List<TextEffectIndex>();
        fiAppearIndices = new List<TextEffectIndex>();
        fiWaveIndices = new List<TextEffectIndex>();

        multiIndices = new List<TextEffectSet>();

        //int offset = 0;

        //Tag identifier debug code
        /*
        for (int k = 0; k < tags.Count; k++)
        {
            Debug.Log(tags[k].tag + " "+tags[k].startIndex);
            Debug.Log(cleanString.Substring(0, tags[k].startIndex) + "[]" + cleanString.Substring(tags[k].startIndex));
        }
        */

        float[] rainbow_args = null;
        float[] wavy_args = null;
        float[] shaky_args = null;
        float[] scramble_args = null;
        float[] jump_args = null;

        float[] fiShrink_args = null;
        float[] fiGrow_args = null;
        float[] fiSpin_args = null;
        float[] fiAppear_args = null;
        float[] fiWave_args = null;

        int offset = 0;

        //note for future shade:
        //should I use this approach for the regular effects?
        //Sidenote: try to see if there was a reason why past shade didn't try this?
        TextEffectSet BuildTextEffectSet(int index)
        {
            TextEffectSet output = new TextEffectSet(index, null);
            output.pieces = new List<TextEffectSet.TextEffectPiece>();

            if (rainbow)
            {
                output.pieces.Add(new TextEffectSet.TextEffectPiece(TextEffectSet.TextEffectPiece.Effect.Rainbow, rainbow_args));
            }
            if (wavy)
            {
                output.pieces.Add(new TextEffectSet.TextEffectPiece(TextEffectSet.TextEffectPiece.Effect.Wavy, wavy_args));
            }
            if (shaky)
            {
                output.pieces.Add(new TextEffectSet.TextEffectPiece(TextEffectSet.TextEffectPiece.Effect.Shaky, shaky_args));
            }
            if (scramble)
            {
                output.pieces.Add(new TextEffectSet.TextEffectPiece(TextEffectSet.TextEffectPiece.Effect.Scramble, scramble_args));
            }
            if (jump)
            {
                output.pieces.Add(new TextEffectSet.TextEffectPiece(TextEffectSet.TextEffectPiece.Effect.Jump, jump_args));
            }
            if (fiShrink)
            {
                output.pieces.Add(new TextEffectSet.TextEffectPiece(TextEffectSet.TextEffectPiece.Effect.FadeInShrink, fiShrink_args));
            }
            if (fiGrow)
            {
                output.pieces.Add(new TextEffectSet.TextEffectPiece(TextEffectSet.TextEffectPiece.Effect.FadeInGrow, fiGrow_args));
            }
            if (fiSpin)
            {
                output.pieces.Add(new TextEffectSet.TextEffectPiece(TextEffectSet.TextEffectPiece.Effect.FadeInSpin, fiSpin_args));
            }
            if (fiAppear)
            {
                output.pieces.Add(new TextEffectSet.TextEffectPiece(TextEffectSet.TextEffectPiece.Effect.FadeInAppear, fiAppear_args));
            }
            if (fiWave)
            {
                output.pieces.Add(new TextEffectSet.TextEffectPiece(TextEffectSet.TextEffectPiece.Effect.FadeInWave, fiWave_args));
            }
            return output;
        }


        //Needed for button sprite positioning
        textMesh.ForceMeshUpdate();
        TMP_TextInfo textInfo = textMesh.textInfo;
        textInfo.ClearAllMeshInfo();

        //new fix: need to adjust the startindex if the offset is updated
        List<int> tagOffsets = new List<int>();
        for (int i = 0; i < tags.Count; i++)
        {
            tagOffsets.Add(0);
        }

        //Determine where certain effects should be applied.
        for (int j = 0; j < cleanString.Length + 1 + offset; j++)
        {
            //Debug.Log(cleanString[j]);
            float[] t_args = new float[0]; //null;
            for (int i = 0; i < tags.Count; i++) //find tags starting at index
            {
                if (tags[i].startIndex != j)
                {
                    continue;
                }

                tagOffsets[i] = offset;

                if (tags[i].args == null)
                {
                    t_args = new float[0];
                } else
                {
                    t_args = new float[tags[i].args.Length];
                }
                
                
                for (int k = 0; k < tags[i].args.Length; k++)
                {
                    float temp = 0;
                    if (float.TryParse(tags[i].args[k], NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out temp))
                    {
                        //
                    }
                    t_args[k] = temp;
                }

                switch (tags[i].tag)
                {
                    case TagEntry.TextTag.Rainbow:
                        //Debug.Log(i + ", index" + j + " "+tags[i].open);
                        if (tags[i].open)
                        {
                            rainbow = true;
                            rainbow_args = (float[])t_args.Clone();
                        }
                        else
                        {
                            rainbow = false;
                            rainbow_args = new float[0];
                        }
                        break;
                    case TagEntry.TextTag.Wavy:
                        //Debug.Log(i + ", index" + j + " " + tags[i].open);
                        if (tags[i].open)
                        {
                            wavy = true;
                            wavy_args = (float[])t_args.Clone();
                        }
                        else
                        {
                            wavy = false;
                            wavy_args = new float[0];
                        }
                        break;
                    case TagEntry.TextTag.Shaky:
                        //Debug.Log(i + ", index" + j + " " + tags[i].open);
                        if (tags[i].open)
                        {
                            shaky = true;
                            shaky_args = (float[])t_args.Clone();
                        }
                        else
                        {
                            shaky = false;
                            shaky_args = new float[0];
                        }
                        break;
                    case TagEntry.TextTag.Scramble:
                        if (tags[i].open)
                        {
                            scramble = true;
                            scramble_args = (float[])t_args.Clone();
                        }
                        else
                        {
                            scramble = false;
                            scramble_args = new float[0];
                        }
                        break;
                    case TagEntry.TextTag.Jump:
                        if (tags[i].open)
                        {
                            jump = true;
                            jump_args = (float[])t_args.Clone();
                        }
                        else
                        {
                            jump = false;
                            jump_args = new float[0];
                        }
                        break;
                    case TagEntry.TextTag.FadeInShrink:
                        if (tags[i].open)
                        {
                            fiShrink = true;
                            fiShrink_args = (float[])t_args.Clone();
                        }
                        else
                        {
                            fiShrink = false;
                            fiShrink_args = new float[0];
                        }
                        break;
                    case TagEntry.TextTag.FadeInGrow:
                        if (tags[i].open)
                        {
                            fiGrow = true;
                            fiGrow_args = (float[])t_args.Clone();
                        }
                        else
                        {
                            fiGrow = false;
                            fiGrow_args = new float[0];
                        }
                        break;
                    case TagEntry.TextTag.FadeInSpin:
                        if (tags[i].open)
                        {
                            fiSpin = true;
                            fiSpin_args = (float[])t_args.Clone();
                        }
                        else
                        {
                            fiSpin = false;
                            fiSpin_args = new float[0];
                        }
                        break;
                    case TagEntry.TextTag.FadeInAppear:
                        if (tags[i].open)
                        {
                            fiAppear = true;
                            fiAppear_args = (float[])t_args.Clone();
                        }
                        else
                        {
                            fiAppear = false;
                            fiAppear_args = new float[0];
                        }
                        break;
                    case TagEntry.TextTag.FadeInWave:
                        if (tags[i].open)
                        {
                            fiWave = true;
                            fiWave_args = (float[])t_args.Clone();
                        }
                        else
                        {
                            fiWave = false;
                            fiWave_args = new float[0];
                        }
                        break;
                    case TagEntry.TextTag.Sprite:
                        offset++;   //Sprites are not characters (so the offset is used to correct some indices)
                        break;
                    case TagEntry.TextTag.Effect:
                    case TagEntry.TextTag.EffectSprite:
                        offset++;

                        Vector3 Eposition;
                        float Esize;
                        (Esize, Eposition) = SpritePosition(textInfo, j + offset, Text_SpecialSprite.MULTIPLIER);

                        GameObject Es = Text_EffectSprite.Create(tags[i].args, j + offset, Esize);
                        SpriteSetup(Es, Eposition, BuildTextEffectSet(j + offset));

                        specialSprites.Add(Es);

                        offset++;
                        break;
                    case TagEntry.TextTag.State:
                    case TagEntry.TextTag.StateSprite:
                        offset++;

                        Vector3 Sposition;
                        float Ssize;
                        (Ssize, Sposition) = SpritePosition(textInfo, j + offset, Text_SpecialSprite.MULTIPLIER);

                        GameObject Ss = Text_StateSprite.Create(tags[i].args, j + offset, Ssize);
                        SpriteSetup(Ss, Sposition, BuildTextEffectSet(j + offset));

                        specialSprites.Add(Ss);

                        offset++;
                        break;
                        //Now unnecessary as these tags should be destroyed by an earlier step
                    /*
                    case TagEntry.TextTag.ZeroSpace:    //all of these make a single character appear that isn't in the clean string (note: putting these characters in instead of using the tags won't cause problems)
                    case TagEntry.TextTag.LArrow:
                    case TagEntry.TextTag.RArrow:
                    case TagEntry.TextTag.UArrow:
                    case TagEntry.TextTag.DArrow:
                    case TagEntry.TextTag.LRArrow:
                    case TagEntry.TextTag.UDArrow:
                    case TagEntry.TextTag.ULArrow:
                    case TagEntry.TextTag.URArrow:
                    case TagEntry.TextTag.DRArrow:
                    case TagEntry.TextTag.DLArrow:
                    case TagEntry.TextTag.Star:
                    case TagEntry.TextTag.EmptyStar:
                    case TagEntry.TextTag.Male:
                    case TagEntry.TextTag.Female:
                    case TagEntry.TextTag.Heart:
                    case TagEntry.TextTag.EmptyHeart:
                    case TagEntry.TextTag.QuarterNote:
                    case TagEntry.TextTag.EighthNote:
                    case TagEntry.TextTag.TwoEighthNotes:
                    case TagEntry.TextTag.TwoSixteenthNotes:
                    case TagEntry.TextTag.Flat:
                    case TagEntry.TextTag.Natural:
                    case TagEntry.TextTag.Sharp:
                    case TagEntry.TextTag.Infinity:
                        offset++;
                        break;
                    */
                    case TagEntry.TextTag.Button:
                    case TagEntry.TextTag.ButtonSprite:
                        offset++;

                        Vector3 Bposition;
                        float Bsize;
                        (Bsize, Bposition) = SpritePosition(textInfo, j + offset, Text_SpecialSprite.MULTIPLIER);

                        GameObject bbs = Text_ButtonSprite.Create(tags[i].args, j + offset, Bsize);
                        SpriteSetup(bbs, Bposition, BuildTextEffectSet(j + offset));

                        specialSprites.Add(bbs);

                        offset++;
                        break;
                    case TagEntry.TextTag.Item:
                    case TagEntry.TextTag.ItemSprite:
                        offset++;

                        Vector3 Iposition;
                        float Isize;
                        (Isize, Iposition) = SpritePosition(textInfo, j + offset, Text_SpecialSprite.MULTIPLIER);

                        GameObject its = Text_ItemSprite.Create(tags[i].args, j + offset, Isize);
                        SpriteSetup(its, Iposition, BuildTextEffectSet(j + offset));

                        specialSprites.Add(its);

                        offset++;
                        break;
                    case TagEntry.TextTag.KeyItem:
                    case TagEntry.TextTag.KeyItemSprite:
                        offset++;

                        Vector3 Kposition;
                        float Ksize;
                        (Ksize, Kposition) = SpritePosition(textInfo, j + offset, Text_SpecialSprite.MULTIPLIER);

                        GameObject kits = Text_KeyItemSprite.Create(tags[i].args, j + offset, Ksize);
                        SpriteSetup(kits, Kposition, BuildTextEffectSet(j + offset));

                        specialSprites.Add(kits);

                        offset++;
                        break;
                    case TagEntry.TextTag.Badge:
                    case TagEntry.TextTag.BadgeSprite:
                        offset++;

                        Vector3 BAposition;
                        float BAsize;
                        (BAsize, BAposition) = SpritePosition(textInfo, j + offset, Text_SpecialSprite.MULTIPLIER);

                        GameObject bas = Text_BadgeSprite.Create(tags[i].args, j + offset, BAsize);
                        SpriteSetup(bas, BAposition, BuildTextEffectSet(j + offset));

                        specialSprites.Add(bas);

                        offset++;
                        break;
                    case TagEntry.TextTag.Ribbon:
                    case TagEntry.TextTag.RibbonSprite:
                        offset++;

                        Vector3 Rposition;
                        float Rsize;
                        (Rsize, Rposition) = SpritePosition(textInfo, j + offset, Text_SpecialSprite.MULTIPLIER);

                        GameObject ris = Text_RibbonSprite.Create(tags[i].args, j + offset, Rsize);
                        SpriteSetup(ris, Rposition, BuildTextEffectSet(j + offset));

                        specialSprites.Add(ris);

                        offset++;
                        break;
                    case TagEntry.TextTag.HP:
                    case TagEntry.TextTag.EP:
                    case TagEntry.TextTag.SE:
                    case TagEntry.TextTag.SP:
                    case TagEntry.TextTag.Stamina:
                    case TagEntry.TextTag.Carrot:
                    case TagEntry.TextTag.Clock:
                    case TagEntry.TextTag.Coin:
                    case TagEntry.TextTag.SilverCoin:
                    case TagEntry.TextTag.GoldCoin:
                    case TagEntry.TextTag.Shard:
                    case TagEntry.TextTag.XP:
                    case TagEntry.TextTag.AstralToken:
                    case TagEntry.TextTag.Common:
                    case TagEntry.TextTag.CommonSprite:
                        //Debug.Log(tags[i].tag + " " + cleanString);
                        if (tags[i].tag != TagEntry.TextTag.Common && tags[i].tag != TagEntry.TextTag.CommonSprite)
                        {
                            //construct a new args array
                            string[] newargs = new string[1 + tags[i].args.Length];
                            newargs[0] = tags[i].tag.ToString();
                            for (int c = 0; c < tags[i].args.Length; c++)
                            {
                                newargs[c + 1] = tags[i].args[c];
                            }
                            
                            offset++;

                            //Debug.Log(tags[i].tag);

                            Vector3 Cposition;
                            float Csize;
                            (Csize, Cposition) = SpritePosition(textInfo, j + offset, Text_SpecialSprite.MULTIPLIER);

                            Csize *= 0.75f;

                            GameObject cis = Text_CommonSprite.Create(newargs, j + offset, Csize);
                            SpriteSetup(cis, Cposition, BuildTextEffectSet(j + offset));

                            specialSprites.Add(cis);

                            offset++;
                        }
                        else
                        {
                            offset++;

                            Vector3 Cposition;
                            float Csize;
                            (Csize, Cposition) = SpritePosition(textInfo, j + offset, Text_SpecialSprite.MULTIPLIER);

                            Csize *= 0.75f;

                            GameObject cis = Text_CommonSprite.Create(tags[i].args, j + offset, Csize);

                            SpriteSetup(cis, Cposition, BuildTextEffectSet(j + offset));

                            specialSprites.Add(cis);

                            offset++;
                        }
                        break;
                    case TagEntry.TextTag.Misc:
                    case TagEntry.TextTag.MiscSprite:
                        offset++;

                        Vector3 Mposition;
                        float Msize;
                        (Msize, Mposition) = SpritePosition(textInfo, j + offset, Text_SpecialSprite.MULTIPLIER);

                        GameObject mis = Text_MiscSprite.Create(tags[i].args, j + offset, Msize);
                        SpriteSetup(mis, Mposition, BuildTextEffectSet(j + offset));

                        specialSprites.Add(mis);

                        offset++;
                        break;
                }
            }


            /*
            string temp2 = "";
            for (int l = 0; l < t_args.Length; l++)
            {
                temp2 += t_args[l] + " ";
            }
            Debug.Log(temp2);
            */

            if (rainbow)
            {
                rainbowIndices.Add(new TextEffectIndex(j + offset,rainbow_args));
            }
            if (wavy)
            {
                wavyIndices.Add(new TextEffectIndex(j + offset, wavy_args));
            }
            if (shaky)
            {
                shakyIndices.Add(new TextEffectIndex(j + offset, shaky_args));
            }
            if (scramble)
            {
                scrambleIndices.Add(new TextEffectIndex(j + offset, scramble_args));
            }
            if (jump)
            {
                jumpIndices.Add(new TextEffectIndex(j + offset, jump_args));
            }
            if (fiShrink)
            {
                fiShrinkIndices.Add(new TextEffectIndex(j + offset, fiShrink_args));
            }
            if (fiGrow)
            {
                fiGrowIndices.Add(new TextEffectIndex(j + offset, fiGrow_args));
            }
            if (fiSpin)
            {
                fiSpinIndices.Add(new TextEffectIndex(j + offset, fiSpin_args));
            }
            if (fiAppear)
            {
                fiAppearIndices.Add(new TextEffectIndex(j + offset, fiAppear_args));
            }
            if (fiWave)
            {
                fiWaveIndices.Add(new TextEffectIndex(j + offset, fiWave_args));
            }
        }

        for (int i = 0; i < tags.Count; i++)
        {
            tags[i].startIndex += tagOffsets[i];
        }

        for (int j = 0; j < cleanString.Length + offset; j++)
        {
            TextEffectSet t = new TextEffectSet(j, null);
            t.pieces = new List<TextEffectSet.TextEffectPiece>();
            if (rainbowIndices.Count > 0 && rainbowIndices[0].index == j)
            {
                t.pieces.Add(new TextEffectSet.TextEffectPiece(TextEffectSet.TextEffectPiece.Effect.Rainbow, rainbowIndices[0].args));
                rainbowIndices.RemoveAt(0);
            }
            if (wavyIndices.Count > 0 && wavyIndices[0].index == j)
            {
                t.pieces.Add(new TextEffectSet.TextEffectPiece(TextEffectSet.TextEffectPiece.Effect.Wavy, wavyIndices[0].args));
                wavyIndices.RemoveAt(0);
            }
            if (shakyIndices.Count > 0 && shakyIndices[0].index == j)
            {
                t.pieces.Add(new TextEffectSet.TextEffectPiece(TextEffectSet.TextEffectPiece.Effect.Shaky, shakyIndices[0].args));
                shakyIndices.RemoveAt(0);
            }
            if (scrambleIndices.Count > 0 && scrambleIndices[0].index == j)
            {
                t.pieces.Add(new TextEffectSet.TextEffectPiece(TextEffectSet.TextEffectPiece.Effect.Scramble, scrambleIndices[0].args));
                scrambleIndices.RemoveAt(0);
            }
            if (jumpIndices.Count > 0 && jumpIndices[0].index == j)
            {
                t.pieces.Add(new TextEffectSet.TextEffectPiece(TextEffectSet.TextEffectPiece.Effect.Jump, jumpIndices[0].args));
                jumpIndices.RemoveAt(0);
            }

            if (fiShrinkIndices.Count > 0 && fiShrinkIndices[0].index == j)
            {
                t.pieces.Add(new TextEffectSet.TextEffectPiece(TextEffectSet.TextEffectPiece.Effect.FadeInShrink, fiShrinkIndices[0].args));
                fiShrinkIndices.RemoveAt(0);
            }
            if (fiGrowIndices.Count > 0 && fiGrowIndices[0].index == j)
            {
                t.pieces.Add(new TextEffectSet.TextEffectPiece(TextEffectSet.TextEffectPiece.Effect.FadeInGrow, fiGrowIndices[0].args));
                fiGrowIndices.RemoveAt(0);
            }
            if (fiSpinIndices.Count > 0 && fiSpinIndices[0].index == j)
            {
                t.pieces.Add(new TextEffectSet.TextEffectPiece(TextEffectSet.TextEffectPiece.Effect.FadeInSpin, fiSpinIndices[0].args));
                fiSpinIndices.RemoveAt(0);
            }
            if (fiAppearIndices.Count > 0 && fiAppearIndices[0].index == j)
            {
                t.pieces.Add(new TextEffectSet.TextEffectPiece(TextEffectSet.TextEffectPiece.Effect.FadeInAppear, fiAppearIndices[0].args));
                fiAppearIndices.RemoveAt(0);
            }
            if (fiWaveIndices.Count > 0 && fiWaveIndices[0].index == j)
            {
                t.pieces.Add(new TextEffectSet.TextEffectPiece(TextEffectSet.TextEffectPiece.Effect.FadeInWave, fiWaveIndices[0].args));
                fiWaveIndices.RemoveAt(0);
            }

            if (t.pieces.Count > 0)
            {
                multiIndices.Add(t);
            }
        }

        //Debug.Log(multiIndices.Count);

        //string formattedString = new TMPString(cleanString,tags).ToString();
        //textMesh.text = formattedString;
        hasTextChanged = true;
        scrollDone = complete;
        //Debug.Log(scrollDone);
        scrollWaitTime = 0;
        aPressTime = 0;
        charsVisible = complete ? textMesh.text.Length : 0;
        charsVisibleCont = complete ? textMesh.text.Length : 0;
        fadeinAnimTime = complete ? charsVisibleCont + 50 : 0;

        //textInfo.ClearAllMeshInfo();
        //textInfo.Clear();
        //textMesh.ForceMeshUpdate();

        //-2 = special case (do not shrink text to fit the box)
        //  Used for popups that have dynamic heights
        if (textMesh.GetRenderedValues()[1] > textMesh.rectTransform.sizeDelta.y && fontSize != -2)
        {
            //Debug.Log(textMesh.GetRenderedValues()[1] + " " + textMesh.rectTransform.sizeDelta.y);
            //try again
            SetText(text, complete, forceOpaque, textMesh.fontSize * (12f / 15f));
            return;
        }


        StartEffectCoroutine(offset);
    }

    public (float, Vector3) SpritePosition(TMP_TextInfo textInfo, int offset, float multiplier)
    {
        //float size;
        //Vector3 position;

        //Debug.Log(textInfo.characterInfo.Length + " " + offset);
        /*
        string a = "";
        for (int i = 0; i < textInfo.characterInfo.Length; i++)
        {
            a += textInfo.characterInfo[i].character;
        }
        Debug.Log(a);
        */

        TMP_CharacterInfo charInfoA;// = textInfo.characterInfo[offset - 1];
        TMP_CharacterInfo charInfoB;// = textInfo.characterInfo[offset];

        charInfoB = textInfo.characterInfo[offset];
        if (offset == 0)
        {
            charInfoA = charInfoB;
        }
        else
        {
            charInfoA = textInfo.characterInfo[offset - 1];
        }

        Vector3 bottomLeft = Vector3.zero;
        Vector3 topRight = Vector3.zero;

        float maxAscender = -Mathf.Infinity;
        float minDescender = Mathf.Infinity;

        maxAscender = Mathf.Max(maxAscender, charInfoA.ascender);
        minDescender = Mathf.Min(minDescender, charInfoB.descender);

        bottomLeft = new Vector3(charInfoA.bottomRight.x, charInfoA.descender, 0);

        bottomLeft = new Vector3(bottomLeft.x, minDescender, 0);
        topRight = new Vector3(charInfoB.topLeft.x, maxAscender, 0);

        float width = topRight.x - bottomLeft.x;
        float height = topRight.y - bottomLeft.y;

        Vector2 centerPosition = bottomLeft;
        centerPosition.x += width / 2;
        //centerPosition.y += height / 2;

        //I made the positioner characters 0.01 as large as normal
        //so offset the center Y position to be where it should be the normal size difference

        //centerPosition.y += 100 * (height / 2);

        //100 is too high
        centerPosition.y += multiplier * (height / 2);
        //Debug.Log(multiplier + " " + (height / 2) + " " + (multiplier * (height / 2)));

        //Debug.Log(charInfoA.character);
        //Debug.Log(charInfoA.index);
        //Debug.Log(charInfoB.character);
        //Debug.Log(charInfoB.index);


        //for technical reasons I have to position it between two characters since I don't have access to a way to find the space tag's position directly
        Vector3 position = centerPosition;

        //size from a further away character
        float size = charInfoA.pointSize * 5;
        /*
        float size = charInfoA.pointSize / 20;
        if (offset - 2 > 0)
        {
            size = textInfo.characterInfo[offset - 2].pointSize;
        } else if (offset + 1 < textInfo.characterCount - 1)
        {
            size = textInfo.characterInfo[offset + 1].pointSize;
        }
        */




        return (size, position);
    }
    public void SpriteSetup(GameObject go, Vector3 position, TextEffectSet tes = default)
    {
        Text_SpecialSprite s = go.GetComponent<Text_SpecialSprite>();
        s.RecalculateBoxSize();
        s.Reposition(position);
        s.tes = tes;
        go.transform.SetParent(textMesh.rectTransform, false);
    }

    public void DestroySpecialSprites()
    {
        if (specialSprites != null)
        {
            for (int i = 0; i < specialSprites.Count; i++)
            {
                Destroy(specialSprites[i]);
            }
        }

        specialSprites = null;
    }

    void OnEnable()
    {
        // Subscribe to event fired when text object has been regenerated.
        TMPro_EventManager.TEXT_CHANGED_EVENT.Add(ON_TEXT_CHANGED);
    }

    void OnDisable()
    {
        TMPro_EventManager.TEXT_CHANGED_EVENT.Remove(ON_TEXT_CHANGED);
    }

    void ON_TEXT_CHANGED(Object obj)
    {
        if (obj == textMesh)
            hasTextChanged = true;
    }

    /// <summary>
    /// Structure to hold pre-computed animation data.
    /// </summary>
    private struct VertexAnim
    {
        public float angleRange;
        public float angle;
        public float speed;
    }
    public struct TextEffectIndex
    {
        public int index;
        public float[] args;

        public TextEffectIndex(int p_index) {
            index = p_index;
            args = new float[0];
        }

        public TextEffectIndex(int p_index, float[] p_args)
        {
            index = p_index;
            args = p_args;
        }
    }
    public struct TextEffectSet
    {
        public int index;
        public List<TextEffectPiece> pieces;

        public struct TextEffectPiece
        {
            public enum Effect
            {
                Rainbow,
                Shaky,
                Wavy,
                Scramble,
                Jump,

                FadeInShrink,
                FadeInGrow,
                FadeInSpin,
                FadeInAppear,
                FadeInWave,
            }

            public Effect effect;
            public float[] args;

            public TextEffectPiece(Effect p_effect, float[] p_args)
            {
                effect = p_effect;
                args = (float[])p_args.Clone();
            }
        }

        public TextEffectSet(int p_index, List<TextEffectPiece> p_pieces)
        {
            index = p_index;
            pieces = p_pieces;
        }
    }

    //Old separate methods
    /*
    public IEnumerator RainbowText(List<int> rainbowIndices)
    {
        textMesh.ForceMeshUpdate();
        TMP_TextInfo textInfo = textMesh.textInfo;

        int currentCharacter = 0;

        Color32[] newVertexColors;
        Color32 c0 = textMesh.color;

        while (true)
        {
            foreach (int index in rainbowIndices)
            {
                int characterCount = textInfo.characterCount;

                //if there is nothing to do, just stop
                if (characterCount == 0)
                {
                    yield break;
                }

                // Get the index of the material used by the current character.
                int materialIndex = textInfo.characterInfo[index].materialReferenceIndex;

                // Get the vertex colors of the mesh used by this text element (character or sprite).
                newVertexColors = textInfo.meshInfo[materialIndex].colors32;

                // Get the index of the first vertex used by this text element.
                int vertexIndex = textInfo.characterInfo[index].vertexIndex;

                // Only change the vertex color if the text element is visible.
                if (textInfo.characterInfo[index].isVisible)
                {
                    float newHsv = index * hsvMult + timeMult * Time.time;
                    newHsv %= 360;
                    newHsv /= 360;
                    c0 = Color.HSVToRGB(newHsv, saturation, value);

                    newVertexColors[vertexIndex + 0] = c0;
                    newVertexColors[vertexIndex + 1] = c0;
                    newVertexColors[vertexIndex + 2] = c0;
                    newVertexColors[vertexIndex + 3] = c0;

                    // New function which pushes (all) updated vertex data to the appropriate meshes when using either the Mesh Renderer or CanvasRenderer.
                    textMesh.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);

                    // This last process could be done to only update the vertex data that has changed as opposed to all of the vertex data but it would require extra steps and knowing what type of renderer is used.
                    // These extra steps would be a performance optimization but it is unlikely that such optimization will be necessary.
                }

                currentCharacter = (currentCharacter + 1) % characterCount;
            }
            yield return new WaitForSeconds(resolution);
        }
    }
    public IEnumerator ShakyText(List<int> shakyIndices)
    {
        // We force an update of the text object since it would only be updated at the end of the frame. Ie. before this code is executed on the first frame.
        // Alternatively, we could yield and wait until the end of the frame when the text object will be generated.
        textMesh.ForceMeshUpdate();

        TMP_TextInfo textInfo = textMesh.textInfo;

        Matrix4x4 matrix;

        int loopCount = 0;
        hasTextChanged = true;

        // Create an Array which contains pre-computed Angle Ranges and Speeds for a bunch of characters.
        VertexAnim[] vertexAnim = new VertexAnim[1024];
        for (int i = 0; i < 1024; i++)
        {
            vertexAnim[i].angleRange = Random.Range(10f, 25f);
            vertexAnim[i].speed = Random.Range(1f, 3f);
        }

        // Cache the vertex data of the text object as the Jitter FX is applied to the original position of the characters.
        TMP_MeshInfo[] cachedMeshInfo = textInfo.CopyMeshInfoVertexData();

        while (true)
        {
            // Get new copy of vertex data if the text has changed.
            if (hasTextChanged)
            {
                // Update the copy of the vertex data for the text object.
                cachedMeshInfo = textInfo.CopyMeshInfoVertexData();

                hasTextChanged = false;
            }


            int characterCount = textInfo.characterCount;

            // If No Characters then just yield and wait for some text to be added
            if (characterCount == 0)
            {
                yield return new WaitForSeconds(0.25f);
                continue;
            }


            foreach (int index in shakyIndices)
            {
                TMP_CharacterInfo charInfo = textInfo.characterInfo[index];

                // Skip characters that are not visible and thus have no geometry to manipulate.
                if (!charInfo.isVisible)
                    continue;

                // Retrieve the pre-computed animation data for the given character.
                VertexAnim vertAnim = vertexAnim[index];

                // Get the index of the material used by the current character.
                int materialIndex = textInfo.characterInfo[index].materialReferenceIndex;

                // Get the index of the first vertex used by this text element.
                int vertexIndex = textInfo.characterInfo[index].vertexIndex;

                // Get the cached vertices of the mesh used by this text element (character or sprite).
                Vector3[] sourceVertices = cachedMeshInfo[materialIndex].vertices;

                // Determine the center point of each character at the baseline.
                //Vector2 charMidBasline = new Vector2((sourceVertices[vertexIndex + 0].x + sourceVertices[vertexIndex + 2].x) / 2, charInfo.baseLine);
                // Determine the center point of each character.
                Vector2 charMidBasline = (sourceVertices[vertexIndex + 0] + sourceVertices[vertexIndex + 2]) / 2;

                // Need to translate all 4 vertices of each quad to aligned with middle of character / baseline.
                // This is needed so the matrix TRS is applied at the origin for each character.
                Vector3 offset = charMidBasline;

                Vector3[] destinationVertices = textInfo.meshInfo[materialIndex].vertices;

                destinationVertices[vertexIndex + 0] = sourceVertices[vertexIndex + 0] - offset;
                destinationVertices[vertexIndex + 1] = sourceVertices[vertexIndex + 1] - offset;
                destinationVertices[vertexIndex + 2] = sourceVertices[vertexIndex + 2] - offset;
                destinationVertices[vertexIndex + 3] = sourceVertices[vertexIndex + 3] - offset;

                vertAnim.angle = 0;
                //vertAnim.angle = Mathf.SmoothStep(-vertAnim.angleRange, vertAnim.angleRange, Mathf.PingPong(loopCount / 25f * vertAnim.speed, 1f));
                Vector3 jitterOffset = new Vector3(Random.Range(-.25f, .25f), Random.Range(-.25f, .25f), 0);

                matrix = Matrix4x4.TRS(jitterOffset * shakyradius, Quaternion.identity, Vector3.one);

                destinationVertices[vertexIndex + 0] = matrix.MultiplyPoint3x4(destinationVertices[vertexIndex + 0]);
                destinationVertices[vertexIndex + 1] = matrix.MultiplyPoint3x4(destinationVertices[vertexIndex + 1]);
                destinationVertices[vertexIndex + 2] = matrix.MultiplyPoint3x4(destinationVertices[vertexIndex + 2]);
                destinationVertices[vertexIndex + 3] = matrix.MultiplyPoint3x4(destinationVertices[vertexIndex + 3]);

                destinationVertices[vertexIndex + 0] += offset;
                destinationVertices[vertexIndex + 1] += offset;
                destinationVertices[vertexIndex + 2] += offset;
                destinationVertices[vertexIndex + 3] += offset;

                vertexAnim[index] = vertAnim;

                // Push changes into meshes
                
                for (int i = 0; i < textInfo.meshInfo.Length; i++)                
                {
                    textInfo.meshInfo[i].mesh.vertices = textInfo.meshInfo[i].vertices;
                    textMesh.UpdateGeometry(textInfo.meshInfo[i].mesh, i);
                }
                

                //update mesh info at index I think
                //# of meshes do not match # of characters I think (because some characters need multiple?)
                //textInfo.meshInfo[materialIndex].mesh.vertices = textInfo.meshInfo[materialIndex].vertices;
                //textMesh.UpdateGeometry(textInfo.meshInfo[materialIndex].mesh, index);
            }

            
            // Push changes into meshes
            for (int i = 0; i < textInfo.meshInfo.Length; i++)
            {
                textInfo.meshInfo[i].mesh.vertices = textInfo.meshInfo[i].vertices;
                textMesh.UpdateGeometry(textInfo.meshInfo[i].mesh, i);
            }
            

            loopCount += 1;
            

            yield return new WaitForSeconds(resolution);
        }
        
    }
    public IEnumerator WavyText(List<int> wavyIndices)
    {
        // We force an update of the text object since it would only be updated at the end of the frame. Ie. before this code is executed on the first frame.
        // Alternatively, we could yield and wait until the end of the frame when the text object will be generated.
        textMesh.ForceMeshUpdate();

        TMP_TextInfo textInfo = textMesh.textInfo;

        Matrix4x4 matrix;

        int loopCount = 0;
        hasTextChanged = true;

        // Create an Array which contains pre-computed Angle Ranges and Speeds for a bunch of characters.
        VertexAnim[] vertexAnim = new VertexAnim[1024];
        for (int i = 0; i < 1024; i++)
        {
            vertexAnim[i].angleRange = Random.Range(10f, 25f);
            vertexAnim[i].speed = Random.Range(1f, 3f);
        }

        // Cache the vertex data of the text object as the Jitter FX is applied to the original position of the characters.
        TMP_MeshInfo[] cachedMeshInfo = textInfo.CopyMeshInfoVertexData();

        while (true)
        {
            // Get new copy of vertex data if the text has changed.
            if (hasTextChanged)
            {
                // Update the copy of the vertex data for the text object.
                cachedMeshInfo = textInfo.CopyMeshInfoVertexData();

                hasTextChanged = false;
            }


            int characterCount = textInfo.characterCount;

            // If No Characters then just yield and wait for some text to be added
            if (characterCount == 0)
            {
                yield return new WaitForSeconds(0.25f);
                continue;
            }


            foreach (int index in wavyIndices)
            {
                TMP_CharacterInfo charInfo = textInfo.characterInfo[index];

                // Skip characters that are not visible and thus have no geometry to manipulate.
                if (!charInfo.isVisible)
                    continue;

                float waveAngle = index * wavycharoffset + wavyomega * Time.time;
                waveAngle %= 360;

                // Retrieve the pre-computed animation data for the given character.
                VertexAnim vertAnim = vertexAnim[index];

                // Get the index of the material used by the current character.
                int materialIndex = textInfo.characterInfo[index].materialReferenceIndex;

                // Get the index of the first vertex used by this text element.
                int vertexIndex = textInfo.characterInfo[index].vertexIndex;

                // Get the cached vertices of the mesh used by this text element (character or sprite).
                Vector3[] sourceVertices = cachedMeshInfo[materialIndex].vertices;

                // Determine the center point of each character at the baseline.
                //Vector2 charMidBasline = new Vector2((sourceVertices[vertexIndex + 0].x + sourceVertices[vertexIndex + 2].x) / 2, charInfo.baseLine);
                // Determine the center point of each character.
                Vector2 charMidBasline = (sourceVertices[vertexIndex + 0] + sourceVertices[vertexIndex + 2]) / 2;

                // Need to translate all 4 vertices of each quad to aligned with middle of character / baseline.
                // This is needed so the matrix TRS is applied at the origin for each character.
                Vector3 offset = charMidBasline;

                Vector3[] destinationVertices = textInfo.meshInfo[materialIndex].vertices;

                destinationVertices[vertexIndex + 0] = sourceVertices[vertexIndex + 0] - offset;
                destinationVertices[vertexIndex + 1] = sourceVertices[vertexIndex + 1] - offset;
                destinationVertices[vertexIndex + 2] = sourceVertices[vertexIndex + 2] - offset;
                destinationVertices[vertexIndex + 3] = sourceVertices[vertexIndex + 3] - offset;

                vertAnim.angle = 0;
                //vertAnim.angle = Mathf.SmoothStep(-vertAnim.angleRange, vertAnim.angleRange, Mathf.PingPong(loopCount / 25f * vertAnim.speed, 1f));
                Vector3 waveOffset = new Vector3(wavyradius * Mathf.Sin(waveAngle), wavyradius * Mathf.Cos(waveAngle), 0);

                matrix = Matrix4x4.TRS(waveOffset, Quaternion.identity, Vector3.one);

                destinationVertices[vertexIndex + 0] = matrix.MultiplyPoint3x4(destinationVertices[vertexIndex + 0]);
                destinationVertices[vertexIndex + 1] = matrix.MultiplyPoint3x4(destinationVertices[vertexIndex + 1]);
                destinationVertices[vertexIndex + 2] = matrix.MultiplyPoint3x4(destinationVertices[vertexIndex + 2]);
                destinationVertices[vertexIndex + 3] = matrix.MultiplyPoint3x4(destinationVertices[vertexIndex + 3]);

                destinationVertices[vertexIndex + 0] += offset;
                destinationVertices[vertexIndex + 1] += offset;
                destinationVertices[vertexIndex + 2] += offset;
                destinationVertices[vertexIndex + 3] += offset;

                vertexAnim[index] = vertAnim;

                for (int i = 0; i < textInfo.meshInfo.Length; i++)
                {
                    textInfo.meshInfo[i].mesh.vertices = textInfo.meshInfo[i].vertices;
                    textMesh.UpdateGeometry(textInfo.meshInfo[i].mesh, i);
                }

                //update mesh info at index I think
                //# of meshes do not match # of characters I think (because some characters need multiple?)
                //textInfo.meshInfo[materialIndex].mesh.vertices = textInfo.meshInfo[materialIndex].vertices;
                //textMesh.UpdateGeometry(textInfo.meshInfo[materialIndex].mesh, index);
            }

            
            // Push changes into meshes
            for (int i = 0; i < textInfo.meshInfo.Length; i++)
            {
                textInfo.meshInfo[i].mesh.vertices = textInfo.meshInfo[i].vertices;
                textMesh.UpdateGeometry(textInfo.meshInfo[i].mesh, i);
            }
            

            loopCount += 1;


            yield return new WaitForSeconds(resolution);
        }

    }
    */

    //Old combined method (does not use multiIndices, which are used to combine multiple effects on the same letters)
    /*
    public IEnumerator AllEffectsOld() //List<TextEffectIndex> rainbowIndices, List<TextEffectIndex> wavyIndices, List<TextEffectIndex> shakyIndices)
    {
        textMesh.ForceMeshUpdate();
        TMP_TextInfo textInfo = textMesh.textInfo;

        Matrix4x4 matrix;

        int loopCount = 0;
        hasTextChanged = true;

        // Create an Array which contains pre-computed Angle Ranges and Speeds for a bunch of characters.
        VertexAnim[] vertexAnim = new VertexAnim[1024];
        for (int i = 0; i < 1024; i++)
        {
            vertexAnim[i].angleRange = Random.Range(10f, 25f);
            vertexAnim[i].speed = Random.Range(1f, 3f);
        }

        Color32[] newVertexColors;
        Color32 c0 = textMesh.color;

        // Cache the vertex data of the text object as the Jitter FX is applied to the original position of the characters.
        TMP_MeshInfo[] cachedMeshInfo = textInfo.CopyMeshInfoVertexData();

        while (true)
        {
            if (!scrollDone)
            {
                int charsVisible2 = charsVisible;

                bool boost = aPressTime > 0 || bPress;
                if (aPressTime > 0)
                {
                    aPressTime -= Time.deltaTime;
                }

                if (scrollWaitTime > 0)
                {
                    scrollWaitTime -= Time.deltaTime;
                }
                else
                {
                    charsVisibleCont += Time.deltaTime * scrollSpeed * ((boost && !lockButtons) ? bPressBoost : 1);
                }
                charsVisible = (int)charsVisibleCont;

                if (unseenTags != null && unseenTags.Count > 0)
                {
                    TagEntry t = unseenTags.Find((e) => (e.startIndex <= charsVisible));
                    while (t != null)
                    {
                        //Debug.Log(t);
                        unseenTags.Remove(t);
                        tagScroll?.Invoke(this, new ScrollEventArgs(t));

                        //handle scroll tags
                        if (t.tag == TagEntry.TextTag.Scroll || t.tag == TagEntry.TextTag.NoScroll || t.tag == TagEntry.TextTag.Wait)
                        {
                            //Debug.Log(charsVisible);
                            //Debug.Log(cleanString.Substring(0, charsVisible));
                            //Debug.Log(textMesh.text.Substring(0, charsVisible));
                            charsVisible = t.startIndex;
                            charsVisibleCont = t.startIndex;

                            //Debug.Log("NOW: " + cleanString.Substring(0, charsVisible));
                            //Debug.Log("NOW: " + textMesh.text.Substring(0, charsVisible));
                            scrollTags.Remove(t);
                            if (t.tag == TagEntry.TextTag.NoScroll)
                            {
                                scrollSpeed = 10000;
                            }
                            else if (t.tag == TagEntry.TextTag.Scroll)
                            {
                                if (t.args != null && t.args.Length > 0)
                                {
                                    float speed;
                                    if (float.TryParse(t.args[0], out speed))
                                    {
                                        scrollSpeed = speed * defaultScrollSpeed;
                                    }
                                }
                            }
                            else if (t.tag == TagEntry.TextTag.Wait)
                            {
                                if (t.args != null && t.args.Length > 0)
                                {
                                    float wait;
                                    if (float.TryParse(t.args[0], out wait))
                                    {
                                        scrollWaitTime = wait;
                                    }
                                }
                            }

                            if (t.args.Length > 1)
                            {
                                bool nobuttons = false;
                                if (bool.TryParse(t.args[1], out nobuttons))
                                {
                                    lockButtons = nobuttons;
                                }
                            }
                        }

                        t = unseenTags.Find((e) => (e.startIndex <= charsVisible));
                    }
                }

                if (charsVisible2 != charsVisible)
                {
                    Bleep();
                    charsVisible2 = charsVisible;
                }


                //textMesh.maxVisibleCharacters = charsVisible;
                //Debug.Log(textMesh.maxVisibleCharacters);

                if (charsVisibleCont > textMesh.text.Length + 1)
                {
                    scrollDone = true;
                    charsVisibleCont = textMesh.text.Length;
                    charsVisible = (int)charsVisibleCont;
                    //textMesh.maxVisibleCharacters = charsVisible;
                }
            }
            else
            {
                if (textMesh.text == null)
                {
                    charsVisible = 0;
                    charsVisibleCont = 0;
                }
                else
                {
                    charsVisibleCont = textMesh.text.Length;
                    charsVisible = textMesh.text.Length;
                }
            }

            // Get new copy of vertex data if the text has changed.
            if (hasTextChanged)
            {
                // Update the copy of the vertex data for the text object.
                cachedMeshInfo = textInfo.CopyMeshInfoVertexData();

                hasTextChanged = false;
            }

            int characterCount = textInfo.characterCount;

            //if there is nothing to do, just stop
            if (characterCount == 0)
            {
                yield break;
            }

            foreach (TextEffectIndex index in rainbowIndices)
            {
                //check some arguments
                float a_hsv = index.args != null && index.args.Length > 0 ? index.args[0] : 1;
                float a_time = index.args != null && index.args.Length > 1 ? index.args[1] : 1;
                float a_sat = index.args != null && index.args.Length > 2 ? index.args[2] : 1;
                float a_val = index.args != null && index.args.Length > 3 ? index.args[3] : 1;

                if (index.index > charsVisible + 1)
                {
                    //if (index == charsVisible + 2)
                    //{
                    //    Debug.Log(textInfo.characterInfo[index].character + " a");
                    //}
                    continue;
                }

                // Get the index of the material used by the current character.
                int materialIndex = textInfo.characterInfo[index.index].materialReferenceIndex;

                // Get the vertex colors of the mesh used by this text element (character or sprite).
                newVertexColors = textInfo.meshInfo[materialIndex].colors32;

                // Get the index of the first vertex used by this text element.
                int vertexIndex = textInfo.characterInfo[index.index].vertexIndex;

                // Only change the vertex color if the text element is visible.
                if (textInfo.characterInfo[index.index].isVisible)
                {
                    float newHsv = index.index * hsvMult * a_hsv + Time.time * timeMult * a_time;
                    newHsv %= 360;
                    newHsv /= 360;
                    c0 = Color.HSVToRGB(newHsv, saturation * a_sat, value * a_val);

                    newVertexColors[vertexIndex + 0] = c0;
                    newVertexColors[vertexIndex + 1] = c0;
                    newVertexColors[vertexIndex + 2] = c0;
                    newVertexColors[vertexIndex + 3] = c0;

                    // New function which pushes (all) updated vertex data to the appropriate meshes when using either the Mesh Renderer or CanvasRenderer.
                    textMesh.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);

                    // This last process could be done to only update the vertex data that has changed as opposed to all of the vertex data but it would require extra steps and knowing what type of renderer is used.
                    // These extra steps would be a performance optimization but it is unlikely that such optimization will be necessary.
                }
            }


            foreach (TextEffectIndex index in shakyIndices)
            {
                //check some arguments
                float a_x = index.args != null && index.args.Length > 0 ? index.args[0] : 1;
                float a_y = index.args != null && index.args.Length > 1 ? index.args[1] : 1;
                float a_z = index.args != null && index.args.Length > 2 ? index.args[2] : 0; //no effect in normal canvas (text gets projected into the xy plane, so z coord is irrelevant unless there is no projection)

                if (index.index > charsVisible + 1)
                {
                    continue;
                }

                TMP_CharacterInfo charInfo = textInfo.characterInfo[index.index];

                // Skip characters that are not visible and thus have no geometry to manipulate.
                if (!charInfo.isVisible)
                    continue;

                // Retrieve the pre-computed animation data for the given character.
                VertexAnim vertAnim = vertexAnim[index.index];

                // Get the index of the material used by the current character.
                int materialIndex = textInfo.characterInfo[index.index].materialReferenceIndex;

                // Get the index of the first vertex used by this text element.
                int vertexIndex = textInfo.characterInfo[index.index].vertexIndex;

                // Get the cached vertices of the mesh used by this text element (character or sprite).
                Vector3[] sourceVertices = cachedMeshInfo[materialIndex].vertices;

                // Determine the center point of each character at the baseline.
                //Vector2 charMidBasline = new Vector2((sourceVertices[vertexIndex + 0].x + sourceVertices[vertexIndex + 2].x) / 2, charInfo.baseLine);
                // Determine the center point of each character.
                Vector2 charMidBasline = (sourceVertices[vertexIndex + 0] + sourceVertices[vertexIndex + 2]) / 2;

                // Need to translate all 4 vertices of each quad to aligned with middle of character / baseline.
                // This is needed so the matrix TRS is applied at the origin for each character.
                Vector3 offset = charMidBasline;

                //reference types mean that this directly affects the mesh vertices
                Vector3[] destinationVertices = textInfo.meshInfo[materialIndex].vertices;

                destinationVertices[vertexIndex + 0] = sourceVertices[vertexIndex + 0] - offset;
                destinationVertices[vertexIndex + 1] = sourceVertices[vertexIndex + 1] - offset;
                destinationVertices[vertexIndex + 2] = sourceVertices[vertexIndex + 2] - offset;
                destinationVertices[vertexIndex + 3] = sourceVertices[vertexIndex + 3] - offset;

                vertAnim.angle = 0;
                //vertAnim.angle = Mathf.SmoothStep(-vertAnim.angleRange, vertAnim.angleRange, Mathf.PingPong(loopCount / 25f * vertAnim.speed, 1f));
                Vector3 jitterOffset = new Vector3(Random.Range(-.25f, .25f) * a_x, Random.Range(-.25f, .25f) * a_y, Random.Range(-.25f, .25f) * a_z);

                matrix = Matrix4x4.TRS(jitterOffset * shakyradius, Quaternion.identity, Vector3.one);

                destinationVertices[vertexIndex + 0] = matrix.MultiplyPoint3x4(destinationVertices[vertexIndex + 0]);
                destinationVertices[vertexIndex + 1] = matrix.MultiplyPoint3x4(destinationVertices[vertexIndex + 1]);
                destinationVertices[vertexIndex + 2] = matrix.MultiplyPoint3x4(destinationVertices[vertexIndex + 2]);
                destinationVertices[vertexIndex + 3] = matrix.MultiplyPoint3x4(destinationVertices[vertexIndex + 3]);

                destinationVertices[vertexIndex + 0] += offset;
                destinationVertices[vertexIndex + 1] += offset;
                destinationVertices[vertexIndex + 2] += offset;
                destinationVertices[vertexIndex + 3] += offset;

                vertexAnim[index.index] = vertAnim;
            }

            foreach (TextEffectIndex index in wavyIndices)
            {
                //check some arguments
                float a_waveX = index.args != null && index.args.Length > 0 ? index.args[0] : 1;
                float a_waveY = index.args != null && index.args.Length > 1 ? index.args[1] : 1;
                float a_waveOmega = index.args != null && index.args.Length > 2 ? index.args[2] : 1;
                float a_waveOffset = index.args != null && index.args.Length > 3 ? index.args[3] : 1;

                if (index.index > charsVisible + 1)
                {
                    continue;
                }

                TMP_CharacterInfo charInfo = textInfo.characterInfo[index.index];

                // Skip characters that are not visible and thus have no geometry to manipulate.
                if (!charInfo.isVisible)
                    continue;

                float waveAngle = index.index * wavycharoffset * a_waveOffset + wavyomega * a_waveOmega * Time.time;
                waveAngle %= 360;

                // Retrieve the pre-computed animation data for the given character.
                VertexAnim vertAnim = vertexAnim[index.index];

                // Get the index of the material used by the current character.
                int materialIndex = textInfo.characterInfo[index.index].materialReferenceIndex;

                // Get the index of the first vertex used by this text element.
                int vertexIndex = textInfo.characterInfo[index.index].vertexIndex;

                // Get the cached vertices of the mesh used by this text element (character or sprite).
                Vector3[] sourceVertices = cachedMeshInfo[materialIndex].vertices;

                // Determine the center point of each character at the baseline.
                //Vector2 charMidBasline = new Vector2((sourceVertices[vertexIndex + 0].x + sourceVertices[vertexIndex + 2].x) / 2, charInfo.baseLine);
                // Determine the center point of each character.
                Vector2 charMidBasline = (sourceVertices[vertexIndex + 0] + sourceVertices[vertexIndex + 2]) / 2;

                // Need to translate all 4 vertices of each quad to aligned with middle of character / baseline.
                // This is needed so the matrix TRS is applied at the origin for each character.
                Vector3 offset = charMidBasline;

                Vector3[] destinationVertices = textInfo.meshInfo[materialIndex].vertices;

                destinationVertices[vertexIndex + 0] = sourceVertices[vertexIndex + 0] - offset;
                destinationVertices[vertexIndex + 1] = sourceVertices[vertexIndex + 1] - offset;
                destinationVertices[vertexIndex + 2] = sourceVertices[vertexIndex + 2] - offset;
                destinationVertices[vertexIndex + 3] = sourceVertices[vertexIndex + 3] - offset;

                vertAnim.angle = 0;
                //vertAnim.angle = Mathf.SmoothStep(-vertAnim.angleRange, vertAnim.angleRange, Mathf.PingPong(loopCount / 25f * vertAnim.speed, 1f));
                Vector3 waveOffset = new Vector3(wavyradius * a_waveX * Mathf.Sin(waveAngle), wavyradius * a_waveY * Mathf.Cos(waveAngle), 0);

                matrix = Matrix4x4.TRS(waveOffset * shakyradius, Quaternion.identity, Vector3.one);

                destinationVertices[vertexIndex + 0] = matrix.MultiplyPoint3x4(destinationVertices[vertexIndex + 0]);
                destinationVertices[vertexIndex + 1] = matrix.MultiplyPoint3x4(destinationVertices[vertexIndex + 1]);
                destinationVertices[vertexIndex + 2] = matrix.MultiplyPoint3x4(destinationVertices[vertexIndex + 2]);
                destinationVertices[vertexIndex + 3] = matrix.MultiplyPoint3x4(destinationVertices[vertexIndex + 3]);

                destinationVertices[vertexIndex + 0] += offset;
                destinationVertices[vertexIndex + 1] += offset;
                destinationVertices[vertexIndex + 2] += offset;
                destinationVertices[vertexIndex + 3] += offset;

                vertexAnim[index.index] = vertAnim;
    
            }

            foreach (TextEffectIndex index in scrambleIndices)
            {
                //check some arguments
                float a_probability = index.args != null && index.args.Length > 0 ? index.args[0] : 0.5f;
                //probability of flipping (applied three times)

                if (index.index > charsVisible + 1)
                {
                    continue;
                }

                TMP_CharacterInfo charInfo = textInfo.characterInfo[index.index];

                // Skip characters that are not visible and thus have no geometry to manipulate.
                if (!charInfo.isVisible)
                    continue;

                // Retrieve the pre-computed animation data for the given character.
                VertexAnim vertAnim = vertexAnim[index.index];

                // Get the index of the material used by the current character.
                int materialIndex = textInfo.characterInfo[index.index].materialReferenceIndex;

                // Get the index of the first vertex used by this text element.
                int vertexIndex = textInfo.characterInfo[index.index].vertexIndex;

                // Get the cached vertices of the mesh used by this text element (character or sprite).
                Vector3[] sourceVertices = cachedMeshInfo[materialIndex].vertices;

                // Determine the center point of each character at the baseline.
                //Vector2 charMidBasline = new Vector2((sourceVertices[vertexIndex + 0].x + sourceVertices[vertexIndex + 2].x) / 2, charInfo.baseLine);
                // Determine the center point of each character.
                Vector2 charMidBasline = (sourceVertices[vertexIndex + 0] + sourceVertices[vertexIndex + 2]) / 2;

                // Need to translate all 4 vertices of each quad to aligned with middle of character / baseline.
                // This is needed so the matrix TRS is applied at the origin for each character.
                Vector3 offset = charMidBasline;

                Vector3[] destinationVertices = textInfo.meshInfo[materialIndex].vertices;

                destinationVertices[vertexIndex + 0] = sourceVertices[vertexIndex + 0] - offset;
                destinationVertices[vertexIndex + 1] = sourceVertices[vertexIndex + 1] - offset;
                destinationVertices[vertexIndex + 2] = sourceVertices[vertexIndex + 2] - offset;
                destinationVertices[vertexIndex + 3] = sourceVertices[vertexIndex + 3] - offset;

                vertAnim.angle = 0;
                //vertAnim.angle = Mathf.SmoothStep(-vertAnim.angleRange, vertAnim.angleRange, Mathf.PingPong(loopCount / 25f * vertAnim.speed, 1f));
                //Vector3 jitterOffset = new Vector3(Random.Range(-.25f, .25f) * a_x, Random.Range(-.25f, .25f) * a_y, Random.Range(-.25f, .25f) * a_z);

                float a = Random.Range(0, 1f);
                float b = Random.Range(0, 1f);
                float c = Random.Range(0, 1f);
                float ar = a > a_probability ? 180 : 0;
                float br = b > a_probability ? 180 : 0;
                float cr = c > a_probability ? 180 : 0;

                matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(ar, br, cr), Vector3.one);

                destinationVertices[vertexIndex + 0] = matrix.MultiplyPoint3x4(destinationVertices[vertexIndex + 0]);
                destinationVertices[vertexIndex + 1] = matrix.MultiplyPoint3x4(destinationVertices[vertexIndex + 1]);
                destinationVertices[vertexIndex + 2] = matrix.MultiplyPoint3x4(destinationVertices[vertexIndex + 2]);
                destinationVertices[vertexIndex + 3] = matrix.MultiplyPoint3x4(destinationVertices[vertexIndex + 3]);

                destinationVertices[vertexIndex + 0] += offset;
                destinationVertices[vertexIndex + 1] += offset;
                destinationVertices[vertexIndex + 2] += offset;
                destinationVertices[vertexIndex + 3] += offset;

                vertexAnim[index.index] = vertAnim;
            }


            //Debug.Log(charsVisible);
            //Debug.Log(textMesh.text + " "+scrollDone);
            //Debug.Log(textMesh.text.Substring(0, charsVisible));
            //Hide characters of index (charsVisible) or more


            if (!(charsVisible > textMesh.text.Length))
            {
                string m = "";
                for (int k = 0; k < charsVisible; k++)
                {
                    m += textInfo.characterInfo[k].character;
                }
                //Debug.Log(m);
                for (int index = 0; index < textMesh.text.Length; index++)
                {
                    if (!textInfo.characterInfo[index].isVisible)
                    {
                        charsVisible++;
                        continue;
                    }

                    // Get the index of the material used by the current character.
                    int materialIndex = textInfo.characterInfo[index].materialReferenceIndex;

                    // Get the vertex colors of the mesh used by this text element (character or sprite).
                    newVertexColors = textInfo.meshInfo[materialIndex].colors32;

                    // Get the index of the first vertex used by this text element.
                    int vertexIndex = textInfo.characterInfo[index].vertexIndex;

                    // Only change the vertex color if the text element is visible.
                    if (textInfo.characterInfo[index].isVisible)
                    {
                        if (!(rainbowIndices.Any((e) => e.index == index)) || index >= charsVisible)
                        {
                            if (index >= charsVisible)
                            {
                                c0 = textInfo.characterInfo[index].color;
                                c0.a = 0;
                            }
                            else
                            {
                                //if (index == charsVisible - 1)
                                //{
                                //Debug.Log(textInfo.characterInfo[index].character + " b");                                       
                                //}
                                c0 = textInfo.characterInfo[index].color;
                                if (semitransparent)
                                {
                                    c0.a = 160;
                                }
                                else
                                {
                                    c0.a = Effect.INFINITE_DURATION;
                                }
                                //Debug.Log(c0);
                            }
                            newVertexColors[vertexIndex + 0] = c0;
                            newVertexColors[vertexIndex + 1] = c0;
                            newVertexColors[vertexIndex + 2] = c0;
                            newVertexColors[vertexIndex + 3] = c0;

                            // New function which pushes (all) updated vertex data to the appropriate meshes when using either the Mesh Renderer or CanvasRenderer.
                            textMesh.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
                        }
                    }
                }
            }


            // Push changes into meshes
            for (int i = 0; i < textInfo.meshInfo.Length; i++)
            {
                textInfo.meshInfo[i].mesh.vertices = textInfo.meshInfo[i].vertices;
                textMesh.UpdateGeometry(textInfo.meshInfo[i].mesh, i);
            }


            loopCount += 1;

            if (Time.deltaTime > resolution)
            {
                yield return null;
            }
            else
            {
                yield return new WaitForSeconds(resolution);
            }
        }
    }
    */

    //offset is carried over from the setup stuff
    public IEnumerator AllEffects(int cindex, int offset) //List<TextEffectIndex> rainbowIndices, List<TextEffectIndex> wavyIndices, List<TextEffectIndex> shakyIndices)
    {
        int tempIndex = cindex;
        //there are multiple materials in play, reset everything fast to avoid sussery
        TMP_TextInfo textInfo = textMesh.textInfo;
        textInfo.ClearAllMeshInfo();

        textMesh.ForceMeshUpdate();
        //textMesh.renderMode = TextRenderFlags.DontRender;




        Matrix4x4 matrix = Matrix4x4.identity; //no transformation

        int loopCount = 0;
        hasTextChanged = true;


        Color32[] newVertexColors;
        Color32 c0 = textMesh.color;


        // Cache the vertex data of the text object as the Jitter FX is applied to the original position of the characters.
        TMP_MeshInfo[] cachedMeshInfo = textInfo.CopyMeshInfoVertexData();

        while (true)
        {
            //Debug.Log(Time.time);
            //Debug.Log(coroutineIndex + " " + tempIndex);
            if (coroutineIndex != tempIndex)
            {
                yield break;
            }

            if (!scrollDone)
            {
                int charsVisible2 = charsVisible;

                bool boost = aPressTime > 0 || bPress;
                if (aPressTime > 0)
                {
                    aPressTime -= Time.deltaTime;
                }

                if (scrollWaitTime > 0)
                {
                    scrollWaitTime -= Time.deltaTime;
                } else
                {
                    charsVisibleCont += Time.deltaTime * scrollSpeed * ((boost && !lockButtons) ? bPressBoost : 1);
                    //fadeinAnimTime = charsVisibleCont;
                    //no: charsVisibleCont will jump forward but fadeinAnimTime should not do that
                    fadeinAnimTime += Time.deltaTime * scrollSpeed * ((boost && !lockButtons) ? bPressBoost : 1);
                }
                charsVisible = (int)charsVisibleCont;

                if (unseenTags != null && unseenTags.Count > 0)
                {
                    TagEntry t = unseenTags.Find((e) => (e.startIndex <= charsVisible - 1));
                    while (t != null)
                    {
                        //Debug.Log(t);
                        unseenTags.Remove(t);
                        tagScroll?.Invoke(this, new ScrollEventArgs(t));

                        //handle scroll tags
                        if (t.tag == TagEntry.TextTag.Scroll || t.tag == TagEntry.TextTag.NoScroll || t.tag == TagEntry.TextTag.Sign || t.tag == TagEntry.TextTag.System || t.tag == TagEntry.TextTag.Wait)
                        {
                            //Debug.Log(charsVisible);
                            //Debug.Log(cleanString.Substring(0, charsVisible));
                            //Debug.Log(textMesh.text.Substring(0, charsVisible));
                            charsVisible = t.startIndex;
                            charsVisibleCont = t.startIndex;

                            //Debug.Log("NOW: " + cleanString.Substring(0, charsVisible));
                            //Debug.Log("NOW: " + textMesh.text.Substring(0, charsVisible));
                            scrollTags.Remove(t);
                            if (t.tag == TagEntry.TextTag.NoScroll || t.tag == TagEntry.TextTag.Sign || t.tag == TagEntry.TextTag.System)
                            {
                                scrollSpeed = 10000;
                            }
                            else if (t.tag == TagEntry.TextTag.Scroll)
                            {
                                if (t.args != null && t.args.Length > 0)
                                {
                                    float speed;
                                    if (float.TryParse(t.args[0], NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out speed))
                                    {
                                        scrollSpeed = speed * defaultScrollSpeed;
                                    }
                                }
                            }
                            else if (t.tag == TagEntry.TextTag.Wait)
                            {
                                if (t.args != null && t.args.Length > 0)
                                {
                                    float wait;
                                    if (float.TryParse(t.args[0], NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out wait))
                                    {
                                        scrollWaitTime = wait;
                                    }
                                }
                            }

                            if (t.args.Length > 1)
                            {
                                bool nobuttons = false;
                                if (bool.TryParse(t.args[1], out nobuttons))
                                {
                                    lockButtons = nobuttons;
                                }
                            }
                        }

                        t = unseenTags.Find((e) => (e.startIndex <= charsVisible));
                    }
                }

                //TextboxScript will handle this as it has access to the speaker of the text
                /*
                if (charsVisible2 != charsVisible)
                {
                    Bleep();
                    charsVisible2 = charsVisible;
                }
                */


                //textMesh.maxVisibleCharacters = charsVisible;
                //Debug.Log(textMesh.maxVisibleCharacters);

                if (charsVisibleCont > textMesh.text.Length + 1)
                {
                    scrollDone = true;
                    charsVisibleCont = textMesh.text.Length;
                    charsVisible = (int)charsVisibleCont;
                    //textMesh.maxVisibleCharacters = charsVisible;
                }
            } else
            {
                if (textMesh.text == null)
                {
                    charsVisible = 0;
                    charsVisibleCont = 0;
                    fadeinAnimTime = charsVisibleCont + 50;
                } else
                {
                    charsVisibleCont = textMesh.text.Length;
                    charsVisible = textMesh.text.Length;
                    //Note: charsVisibleCont will jump forward to the end so skipping forward is bad!
                    /*
                    if (fadeinAnimTime < charsVisibleCont)
                    {
                        fadeinAnimTime = charsVisibleCont;
                    } else
                    {
                        //normal scrolling speed
                        fadeinAnimTime += Time.deltaTime * scrollSpeed;
                    }
                    */
                    //normal scrolling speed
                    fadeinAnimTime += Time.deltaTime * scrollSpeed;
                }
            }

            // Get new copy of vertex data if the text has changed.
            if (hasTextChanged)
            {
                // Update the copy of the vertex data for the text object.
                textInfo.ClearAllMeshInfo();
                textInfo.ClearLineInfo();
                //Problem: textinfo.characterinfo is stale
                textMesh.ForceMeshUpdate();
                cachedMeshInfo = textInfo.CopyMeshInfoVertexData();
                hasTextChanged = false;
            }

            int characterCount = textInfo.characterCount;

            //if there is nothing to do, just stop
            if (characterCount == 0)
            {
                yield break;
            }

            //check for button visibility
            for (int i = 0; i < specialSprites.Count; i++)
            {
                Text_SpecialSprite sps = specialSprites[i].GetComponent<Text_SpecialSprite>();
                
                if (sps.index > charsVisible)
                {
                    specialSprites[i].SetActive(false);
                } else
                {
                    specialSprites[i].SetActive(true);
                    sps.EffectUpdate(fadeinAnimTime, charsVisibleCont);
                }
            }

            List<int> colorChangeIndices = new List<int>();

            bool colorupdate = false;
            foreach (TextEffectSet set in multiIndices)
            {
                if (set.index > charsVisible + 1)
                {
                    continue;
                }

                //Debug.Log(textInfo.characterInfo[set.index].character);
                //skip invisible characters
                TMP_CharacterInfo charInfo = textInfo.characterInfo[set.index];
                if (!charInfo.isVisible)
                {
                    continue;
                }

                // Get the index of the material used by the current character.
                int materialIndex = textInfo.characterInfo[set.index].materialReferenceIndex;

                // Get the index of the first vertex used by this text element.
                int vertexIndex = textInfo.characterInfo[set.index].vertexIndex;

                // Get the cached vertices of the mesh used by this text element (character or sprite).
                Vector3[] sourceVertices = cachedMeshInfo[materialIndex].vertices;

                int rainbowIndex = -1;
                int shakyIndex = -1;
                int wavyIndex = -1;
                int scrambleIndex = -1;
                int jumpIndex = -1;

                int fiShrinkIndex = -1;
                int fiGrowIndex = -1;
                int fiSpinIndex = -1;
                int fiAppearIndex = -1;
                int fiWaveIndex = -1;

                for (int i = 0; i < set.pieces.Count; i++)
                {
                    if (set.pieces[i].effect == TextEffectSet.TextEffectPiece.Effect.Rainbow)
                    {
                        rainbowIndex = i;
                    }
                    if (set.pieces[i].effect == TextEffectSet.TextEffectPiece.Effect.Wavy)
                    {
                        wavyIndex = i;
                    }
                    if (set.pieces[i].effect == TextEffectSet.TextEffectPiece.Effect.Shaky)
                    {
                        shakyIndex = i;
                    }
                    if (set.pieces[i].effect == TextEffectSet.TextEffectPiece.Effect.Scramble)
                    {
                        scrambleIndex = i;
                    }
                    if (set.pieces[i].effect == TextEffectSet.TextEffectPiece.Effect.Jump)
                    {
                        jumpIndex = i;
                    }

                    if (set.pieces[i].effect == TextEffectSet.TextEffectPiece.Effect.FadeInShrink)
                    {
                        fiShrinkIndex = i;
                    }
                    if (set.pieces[i].effect == TextEffectSet.TextEffectPiece.Effect.FadeInGrow)
                    {
                        fiGrowIndex = i;
                    }
                    if (set.pieces[i].effect == TextEffectSet.TextEffectPiece.Effect.FadeInSpin)
                    {
                        fiSpinIndex = i;
                    }
                    if (set.pieces[i].effect == TextEffectSet.TextEffectPiece.Effect.FadeInAppear)
                    {
                        fiAppearIndex = i;
                    }
                    if (set.pieces[i].effect == TextEffectSet.TextEffectPiece.Effect.FadeInWave)
                    {
                        fiWaveIndex = i;
                    }
                }

                if (rainbowIndex != -1)
                {
                    float[] tempArgs = set.pieces[rainbowIndex].args;

                    //check some arguments
                    float a_hsv = tempArgs != null && tempArgs.Length > 0 ? tempArgs[0] : 1;
                    float a_time = tempArgs != null && tempArgs.Length > 1 ? tempArgs[1] : 1;
                    float a_sat = tempArgs != null && tempArgs.Length > 2 ? tempArgs[2] : 1;
                    float a_val = tempArgs != null && tempArgs.Length > 3 ? tempArgs[3] : 1;

                    // Get the vertex colors of the mesh used by this text element (character or sprite).
                    newVertexColors = textInfo.meshInfo[materialIndex].colors32;

                    // Only change the vertex color if the text element is visible.
                    if (textInfo.characterInfo[set.index].isVisible)
                    {
                        float newHsv = set.index * hsvMult * a_hsv + Time.time * timeMult * a_time;
                        newHsv %= 360;
                        newHsv /= 360;
                        c0 = Color.HSVToRGB(newHsv, saturation * a_sat, value * a_val);

                        newVertexColors[vertexIndex + 0] = c0;
                        newVertexColors[vertexIndex + 1] = c0;
                        newVertexColors[vertexIndex + 2] = c0;
                        newVertexColors[vertexIndex + 3] = c0;

                        colorChangeIndices.Add(set.index);

                        colorupdate = true;
                    }
                }

                //Time for the spatial manipulation effects
                Vector3 jitterOffset = Vector3.zero;

                if (shakyIndex != -1)
                {
                    float[] tempArgs = set.pieces[shakyIndex].args;

                    //check some arguments
                    float a_x = tempArgs != null && tempArgs.Length > 0 ? tempArgs[0] : 5f;
                    float a_y = tempArgs != null && tempArgs.Length > 1 ? tempArgs[1] : 5f;
                    float a_z = tempArgs != null && tempArgs.Length > 2 ? tempArgs[2] : 0; //no effect in normal canvas (text gets projected into the xy plane, so z coord is irrelevant unless there is no projection)
                    //Debug.Log(a_x + " " + a_y + " " + a_z);
                    
                    //vertAnim.angle = Mathf.SmoothStep(-vertAnim.angleRange, vertAnim.angleRange, Mathf.PingPong(loopCount / 25f * vertAnim.speed, 1f));
                    jitterOffset = new Vector3(Random.Range(-.25f, .25f) * a_x, Random.Range(-.25f, .25f) * a_y, Random.Range(-.25f, .25f) * a_z);

                    //matrix = Matrix4x4.TRS(jitterOffset * shakyradius, Quaternion.identity, Vector3.one);
                }

                Vector3 waveOffset = Vector3.zero;

                if (wavyIndex != -1)
                {
                    float[] tempArgs = set.pieces[wavyIndex].args;
                    //Debug.Log(tempArgs.Length);

                    //check some arguments
                    float a_waveX = tempArgs != null && tempArgs.Length > 0 ? tempArgs[0] : 1;
                    float a_waveY = tempArgs != null && tempArgs.Length > 1 ? tempArgs[1] : 1;
                    float a_waveOmega = tempArgs != null && tempArgs.Length > 2 ? tempArgs[2] : 1;
                    float a_waveOffset = tempArgs != null && tempArgs.Length > 3 ? tempArgs[3] : 1;


                    float waveAngle = set.index * wavycharoffset * a_waveOffset + wavyomega * a_waveOmega * Time.time;
                    
                    //vertAnim.angle = Mathf.SmoothStep(-vertAnim.angleRange, vertAnim.angleRange, Mathf.PingPong(loopCount / 25f * vertAnim.speed, 1f));

                    waveOffset = new Vector3(wavyradius * a_waveX * Mathf.Sin(waveAngle), wavyradius * a_waveY * Mathf.Cos(waveAngle), 0);

                    //matrix = Matrix4x4.TRS(waveOffset * shakyradius, Quaternion.identity, Vector3.one);
                }

                float ar = 0;
                float br = 0;
                float cr = 0;

                if (scrambleIndex != -1)
                {
                    float[] tempArgs = set.pieces[scrambleIndex].args;

                    //check some arguments
                    float a_power = tempArgs != null && tempArgs.Length > 0 ? tempArgs[0] : 1f;
                    //slows down the flipping
                    
                    //should look random enough
                    //this setup makes the text change up to 12 times per frame
                    ar = RandomGenerator.Hash(45 * ((uint)(Time.time * 4 / a_power) + (uint)set.index)) % 360 > 180 ? 180 : 0;
                    br = RandomGenerator.Hash(165 * ((uint)(Time.time * 3 / a_power) + (uint)set.index)) % 360 > 180 ? 180 : 0;
                    cr = RandomGenerator.Hash((uint)(Time.time * 2 / a_power) + (uint)set.index) % 360;
                    //cr = c * 360; //c > a_probability ? 180 : 0;

                    //matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(ar, br, cr), Vector3.one);
                }

                Vector3 jumpDelta = Vector3.zero;

                if (jumpIndex != -1)
                {
                    float[] tempArgs = set.pieces[jumpIndex].args;

                    //check some arguments
                    float a_jumpVel = tempArgs != null && tempArgs.Length > 0 ? tempArgs[0] * 20 : 20f;
                    float a_jumpTime = tempArgs != null && tempArgs.Length > 0 ? tempArgs[0] * 0.5f : 0.5f;

                    float cycleTime = (Time.time % a_jumpTime) / a_jumpTime;

                    //jumpDelta = Vector3.up * (1 - 2 * cycleTime) * a_jumpVel;

                    //take the integral of above with respect to CT
                    jumpDelta = Vector3.up * (cycleTime - cycleTime * cycleTime) * a_jumpVel;

                    //matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(ar, br, cr), Vector3.one);
                }


                float fiDelta = fadeinAnimTime - set.index;
                //Debug.Log(fiDelta + " " + fadeinAnimTime + " " + set.index);

                float fiSizeChange = 1;
                Vector3 fiDeltaPos = Vector3.zero;
                float fiSpin = 0;

                if (fiShrinkIndex != -1)
                {
                    float[] tempArgs = set.pieces[fiShrinkIndex].args;

                    float fiFade = (fiDelta / 5);
                    if (tempArgs.Length > 0)
                    {
                        fiFade = (fiDelta / (5f * tempArgs[0]));
                    }
                    if (fiFade < 0)
                    {
                        fiFade = 0;
                    }
                    if (fiFade > 1)
                    {
                        fiFade = 1;
                    }
                    float fadeAmount = fiFade;

                    //Debug.Log(fiDelta + " " + fadeinAnimTime + " " + set.index + " " + fiFade);

                    float startSize = 3;
                    if (tempArgs.Length > 1)
                    {
                        startSize = 3 * tempArgs[1];
                    }

                    fiSizeChange = fiSizeChange * Mathf.Lerp(startSize, 1, fiFade);

                    //fadeout
                    if (textInfo.characterInfo[set.index].isVisible && fadeinAnimTime < charsVisibleCont + 50)
                    {
                        newVertexColors = textInfo.meshInfo[materialIndex].colors32;

                        Color oldColor = newVertexColors[vertexIndex + 0];

                        oldColor.a = fadeAmount;// * oldColor.a;

                        newVertexColors[vertexIndex + 0] = oldColor;
                        newVertexColors[vertexIndex + 1] = oldColor;
                        newVertexColors[vertexIndex + 2] = oldColor;
                        newVertexColors[vertexIndex + 3] = oldColor;

                        colorChangeIndices.Add(set.index);

                        colorupdate = true;
                    }
                }
                if (fiGrowIndex != -1)
                {
                    float[] tempArgs = set.pieces[fiGrowIndex].args;

                    float fiFade = (fiDelta / 5);
                    if (tempArgs.Length > 0)
                    {
                        fiFade = (fiDelta / (5f * tempArgs[0]));
                    }
                    if (fiFade < 0)
                    {
                        fiFade = 0;
                    }
                    if (fiFade > 1)
                    {
                        fiFade = 1;
                    }

                    fiSizeChange = fiSizeChange * Mathf.Lerp(0, 1, fiFade);
                }
                if (fiSpinIndex != -1)
                {
                    float[] tempArgs = set.pieces[fiSpinIndex].args;

                    float fiFade = (fiDelta / 5);
                    if (tempArgs.Length > 0)
                    {
                        fiFade = (fiDelta / (5f * tempArgs[0]));
                    }
                    if (fiFade < 0)
                    {
                        fiFade = 0;
                    }
                    if (fiFade > 1)
                    {
                        fiFade = 1;
                    }

                    float delta = (1 - (fiFade)) * (1 - (fiFade));

                    float fiSpinAmount = 720;
                    if (tempArgs.Length > 1)
                    {
                        fiSpinAmount = tempArgs[1] * 720;
                    }

                    //note: this is "delta spin" (how much to rotate the character)
                    //so it has to start high and go lower
                    fiSpin = fiSpinAmount * delta;
                }
                if (fiAppearIndex != -1)
                {
                    float[] tempArgs = set.pieces[fiAppearIndex].args;

                    float fiFade = (fiDelta / 5);
                    if (tempArgs.Length > 0)
                    {
                        fiFade = (fiDelta / (5f * tempArgs[0]));
                    }
                    if (fiFade < 0)
                    {
                        fiFade = 0;
                    }
                    if (fiFade > 1)
                    {
                        fiFade = 1;
                    }
                    float fadeAmount = fiFade;

                    Color colorA = Color.black;
                    Color colorB = Color.black;
                    if (tempArgs.Length > 6)
                    {
                        colorA = new Color(tempArgs[1], tempArgs[2], tempArgs[3]);
                        colorB = new Color(tempArgs[4], tempArgs[5], tempArgs[6]);
                    }

                    //fadeout
                    if (textInfo.characterInfo[set.index].isVisible)
                    {
                        newVertexColors = textInfo.meshInfo[materialIndex].colors32;

                        Color oldColor = newVertexColors[vertexIndex + 0];

                        if (tempArgs.Length > 6)
                        {
                            oldColor = Color.Lerp(colorA, colorB, fadeAmount);
                        }

                        oldColor.a = fadeAmount;// * oldColor.a;                        

                        newVertexColors[vertexIndex + 0] = oldColor;
                        newVertexColors[vertexIndex + 1] = oldColor;
                        newVertexColors[vertexIndex + 2] = oldColor;
                        newVertexColors[vertexIndex + 3] = oldColor;

                        colorChangeIndices.Add(set.index);

                        colorupdate = true;
                    }
                }
                if (fiWaveIndex != -1)
                {
                    float[] tempArgs = set.pieces[fiWaveIndex].args;

                    float fiFade = (fiDelta / 5);
                    if (tempArgs.Length > 0)
                    {
                        fiFade = (fiDelta / (5f * tempArgs[0]));
                    }
                    if (fiFade < 0)
                    {
                        fiFade = 0;
                    }
                    if (fiFade > 1)
                    {
                        fiFade = 1;
                    }

                    float dist = 10;
                    if (tempArgs.Length > 1)
                    {
                        dist = 10 * tempArgs[1];
                    }
                    dist = (1 - fiFade) * dist;

                    float startOmega = Mathf.PI;
                    if (tempArgs.Length > 2)
                    {
                        startOmega = Mathf.PI * tempArgs[2];
                    }

                    float omegaOffset = 0;
                    if (tempArgs.Length > 3)
                    {
                        omegaOffset = tempArgs[3];
                    }

                    float omegaOffsetPer = Mathf.PI / 6;
                    if (tempArgs.Length > 4)
                    {
                        omegaOffsetPer = (2 * Mathf.PI) * tempArgs[4];
                    }

                    Vector3 fiWaveOffset = new Vector3(dist * Mathf.Sin(startOmega * (1 - fiFade) + omegaOffset + omegaOffsetPer * set.index), dist * Mathf.Cos(startOmega * (1 - fiFade) + omegaOffset + omegaOffsetPer * set.index), 0);
                    fiDeltaPos = fiWaveOffset;
                }



                Vector3[] destinationVertices = textInfo.meshInfo[materialIndex].vertices;

                // Determine the center point of each character at the baseline.
                //Vector2 charMidBasline = new Vector2((sourceVertices[vertexIndex + 0].x + sourceVertices[vertexIndex + 2].x) / 2, charInfo.baseLine);
                // Determine the center point of each character.
                Vector2 charMidBasline = (sourceVertices[vertexIndex + 0] + sourceVertices[vertexIndex + 2]) / 2;

                // Need to translate all 4 vertices of each quad to aligned with middle of character / baseline.
                // This is needed so the matrix TRS is applied at the origin for each character.
                Vector3 offsetB = charMidBasline;

                //Apply everything with one transformation
                Vector3 newpos = waveOffset * shakyradius + jitterOffset + fiDeltaPos + jumpDelta;
                Quaternion newrot = Quaternion.Euler(ar, br + fiSpin, cr);
                Vector3 newscale = Vector3.one * fiSizeChange;
                matrix = Matrix4x4.TRS(newpos, newrot, newscale);

                destinationVertices[vertexIndex + 0] = sourceVertices[vertexIndex + 0] - offsetB;
                destinationVertices[vertexIndex + 1] = sourceVertices[vertexIndex + 1] - offsetB;
                destinationVertices[vertexIndex + 2] = sourceVertices[vertexIndex + 2] - offsetB;
                destinationVertices[vertexIndex + 3] = sourceVertices[vertexIndex + 3] - offsetB;

                destinationVertices[vertexIndex + 0] = matrix.MultiplyPoint3x4(destinationVertices[vertexIndex + 0]);
                destinationVertices[vertexIndex + 1] = matrix.MultiplyPoint3x4(destinationVertices[vertexIndex + 1]);
                destinationVertices[vertexIndex + 2] = matrix.MultiplyPoint3x4(destinationVertices[vertexIndex + 2]);
                destinationVertices[vertexIndex + 3] = matrix.MultiplyPoint3x4(destinationVertices[vertexIndex + 3]);

                destinationVertices[vertexIndex + 0] += offsetB;
                destinationVertices[vertexIndex + 1] += offsetB;
                destinationVertices[vertexIndex + 2] += offsetB;
                destinationVertices[vertexIndex + 3] += offsetB;
            }

            if (colorupdate)
            {
                textMesh.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
            }



            //Debug.Log(charsVisible);
            //Debug.Log(textMesh.text + " "+scrollDone);
            //Debug.Log(textMesh.text.Substring(0, charsVisible));
            //Hide characters of index (charsVisible) or more

            //sussy variable names
            int tmtl = textMesh.text.Length;
            int csl = cleanString.Length + (2 * specialSprites.Count);  //cleanString.Length + offset;
            int ticil = textInfo.characterInfo.Length;
            int timil = textInfo.meshInfo.Length;

            if (!(charsVisible > tmtl))
            {
                /*
                string m = "";
                for (int k = 0; k < charsVisible; k++)
                {
                    m += textInfo.characterInfo[k].character;
                }
                */
                //Debug.Log(m);

                bool rainUpdate2 = false;

                //bool keepGoing = false;

                //if (tempIndex > 1)
                //{
                //    MainManager.ListPrint<int>(indices);
                //}

                for (int index = 0; index < tmtl; index++)
                {
                    /*
                    if (!textInfo.characterInfo[index].isVisible)
                    {
                        charsVisible++;
                        continue;
                    }
                    */

                    /*
                    if (textInfo.characterInfo[index].character == '\0')
                    {
                        Debug.Log(index + " " + ((int)textInfo.characterInfo[index].character) + " NUL");
                    }
                    else
                    {
                        Debug.Log(index + " " + ((int)textInfo.characterInfo[index].character) + " " + textInfo.characterInfo[index].character);
                    }
                    */

                    //for some reason the text gets padded with a bunch of NUL characters (replaces the tag characters)
                    //use this to counteract that (so that the text ends when it should end)
                    while (charsVisible < tmtl && (charsVisible >= textInfo.characterInfo.Length || textInfo.characterInfo[charsVisible].character == '\0'))
                    {
                        charsVisible++;
                        charsVisibleCont++;
                    }

                    /*
                    if (textInfo.characterInfo[index].character == '\0')
                    {
                        keepGoing = true;
                    }

                    if (keepGoing)
                    {
                        continue;
                    }
                    */

                    //??? something is sus but I don't know what
                    if (index >= ticil)
                    {
                        Debug.Log(textInfo.characterInfo[textInfo.characterInfo.Length - 1].character);
                        break;
                    }

                    //more sus found with button sprites and item sprites
                    //each one seems to push the end of the string to be 2 closer to the start then it should be
                    //Debug.Log(index + " " + cleanString.Length + " " + tmtl + " " + textInfo.characterInfo[index].character);

                    //if (tempIndex > 1)
                    //{
                    //    Debug.Log("clean = " + cleanString.Length);
                    //}

                    //fixes some color problems
                    if (index > csl)
                    {
                        break;
                    }


                    // Get the index of the material used by the current character.
                    int materialIndex = textInfo.characterInfo[index].materialReferenceIndex;
                    //Debug.Log(materialIndex);

                    // Get the vertex colors of the mesh used by this text element (character or sprite).
                    newVertexColors = textInfo.meshInfo[materialIndex].colors32;

                    // Get the index of the first vertex used by this text element.
                    int vertexIndex = textInfo.characterInfo[index].vertexIndex;

                    // Only change the vertex color if the text element is visible.
                    if (textInfo.characterInfo[index].isVisible)
                    {
                        //bool condition = !(multiIndices.Any((e) => (e.index == index && e.pieces.Any((f) => f.effect == TextEffectSet.TextEffectPiece.Effect.Rainbow))));
                        //bool conditionB = !(multiIndices.Any((e) => (e.index == index)));
                        bool conditionC = !(colorChangeIndices.Any((e) => (e == index)));
                        //bool conditionD = index < 150;
                        //Debug.Log(conditionB ^ conditionC);
                        //if (!conditionC)
                        //{
                        //    Debug.Log(index);
                        //}

                        /*
                        if (index == 193)
                        {
                            Debug.Log(index + " " + conditionC + " " + textInfo.characterInfo[index].color);
                        }

                        if (index == 201)
                        {
                            Debug.Log(index + " " + textInfo.characterInfo[index].character);
                        }
                        */

                        //if removed this will sometimes break the colors of stuff near the end of the string for some reason
                        conditionC &= !(index >= csl);

                        //if (index > cleanString.Length)
                        //{
                        //    Debug.Log(index);
                        //}

                        if (conditionC || index >= charsVisible)
                        {
                            if (index >= charsVisible)
                            {
                                c0 = textInfo.characterInfo[index].color;
                                c0.a = 0;
                            }
                            else
                            {
                                //if (index == charsVisible - 1)
                                //{
                                //Debug.Log(textInfo.characterInfo[index].character + " b");                                       
                                //}
                                c0 = textInfo.characterInfo[index].color;
                                if (semitransparent)
                                {
                                    c0.a = 160;
                                }
                                else
                                {
                                    c0.a = 255;
                                }
                                //Debug.Log(c0);
                            }
                            //if (textInfo.characterInfo.Length >= 281)
                            //{
                                //Debug.Log(c0 + " " + textInfo.characterInfo[index].character + " " + index + " " + textInfo.characterInfo.Length);
                            //}
                            newVertexColors[vertexIndex + 0] = c0;
                            newVertexColors[vertexIndex + 1] = c0;
                            newVertexColors[vertexIndex + 2] = c0;
                            newVertexColors[vertexIndex + 3] = c0;

                            // New function which pushes (all) updated vertex data to the appropriate meshes when using either the Mesh Renderer or CanvasRenderer.
                            rainUpdate2 = true;
                            //textMesh.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
                        }                        
                    }
                }

                if (rainUpdate2)
                {
                    textMesh.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
                }
            }
            

            // Push changes into meshes
            for (int i = 0; i < timil; i++)
            {
                //Debug.Log(textInfo.meshInfo[i].colors32[0]);
                textInfo.meshInfo[i].mesh.vertices = textInfo.meshInfo[i].vertices;
                textMesh.UpdateGeometry(textInfo.meshInfo[i].mesh, i);
            }

            //textMesh.mesh.uv = textInfo.meshInfo[0].uvs0;
            //textMesh.mesh.uv2 = textInfo.meshInfo[0].uvs2;


            loopCount += 1;

            /*
            if (Time.deltaTime > resolution)
            {
                yield return null;
            } else
            {
                yield return new WaitForEndOfFrame();
                //yield return new WaitForSeconds(resolution);
            }
            */
            yield return null;
        }
    }
}
