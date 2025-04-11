using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using static TextDisplayer;


public class Text_SpecialSprite : MonoBehaviour
{

    //normal scaling is 20px
    protected const float defaultSize = 20f;
    public string[] args;
    public int index;

    public Image baseBox;

    protected Vector3 targetPos;
    protected float relativeSize;

    //Hacky thing
    //(ends up being basebox.transform.localPos and is set during the first EffectUpdate)
    protected Vector3 realTargetPos;

    //New tech: supporting effects
    public TextDisplayer.TextEffectSet tes;

    public int GetIndex()
    {
        return index;
    }

    public virtual void RecalculateBoxSize()
    {
        //no text but this is still needed to make sure the item is the right size
        //text.ForceMeshUpdate();

        //text.fontSize = defaultSize * relativeSize;
        //baseBox.rectTransform.sizeDelta = new Vector2((text.text.Length * 12 + 18) * relativeSize, 30 * relativeSize); //baseBox.rectTransform.sizeDelta.y);
        baseBox.rectTransform.sizeDelta = new Vector2(30 * relativeSize, 30 * relativeSize);
    }

    public virtual void Reposition(Vector3 position)
    {
        targetPos = position;
        baseBox.rectTransform.position = position;
    }

    public virtual void EffectUpdate(float fadeinAnimTime, float charsVisibleCont)
    {
        if (realTargetPos == Vector3.zero)
        {
            realTargetPos = baseBox.rectTransform.localPosition;
        }

        TextDisplayer.TextEffectSet set = tes;

        if (set.pieces == null)
        {
            return;
        }

        //Importing a lot of stuff from TextDisplayer

        //though I have to bring in a lot of definitions

        #pragma warning disable CS0219
        bool colorupdate = false;
        Color32 c0 = Color.white;

        float hsvMult = 60f;
        float timeMult = 180f;
        float saturation = 0.5f;
        float value = 1f;

        float shakyradius = 4f;

        float wavyradius = 1.25f;
        float wavyomega = 8f;
        float wavycharoffset = 0.5f;

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


            float newHsv = set.index * hsvMult * a_hsv + Time.time * timeMult * a_time;
            newHsv %= 360;
            newHsv /= 360;
            c0 = Color.HSVToRGB(newHsv, saturation * a_sat, value * a_val);

            baseBox.color = c0;

            colorupdate = true;
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
            jitterOffset = new Vector3(UnityEngine.Random.Range(-.25f, .25f) * a_x, UnityEngine.Random.Range(-.25f, .25f) * a_y, UnityEngine.Random.Range(-.25f, .25f) * a_z);

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
            if (waveAngle < 0)
            {
                waveAngle += 360;
            }
            waveAngle %= 360;

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
            if (fadeinAnimTime < charsVisibleCont + 50)
            {                
                Color oldColor = c0;

                oldColor.a = fadeAmount;// * oldColor.a;

                c0 = oldColor;

                baseBox.color = c0;

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

            Color oldColor = c0;

            if (tempArgs.Length > 6)
            {
                oldColor = Color.Lerp(colorA, colorB, fadeAmount);
            }

            oldColor.a = fadeAmount;// * oldColor.a;                        
            c0 = oldColor;

            baseBox.color = oldColor;

            colorupdate = true;
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

        //I can just set stuff
        Vector3 newpos = realTargetPos + waveOffset * shakyradius + jitterOffset + fiDeltaPos + jumpDelta;
        Quaternion newrot = Quaternion.Euler(ar, br + fiSpin, cr);
        Vector3 newscale = Vector3.one * fiSizeChange;
        //matrix = Matrix4x4.TRS(newpos, newrot, newscale);

        baseBox.rectTransform.localPosition = newpos;
        baseBox.rectTransform.localRotation = newrot;
        baseBox.rectTransform.localScale = newscale;
    }
}

public class Text_ButtonSprite : Text_SpecialSprite
{
    //text width number
    public static string GetButtonWidth(string b)
    {
        //use em
        return (GetWidth(b) + widthBoost) + "em";
    }

    public static float GetWidth(string b)
    {
        //this may look weird if you use buttons with long names
        //but 1 character buttons should be the same length (because most keyboard buttons are constant width usually)
        //return 0.6f * b.Length;
        InputManager.Button bu;
        Enum.TryParse(b, true, out bu);
        return 0.6f * MainManager.Instance.GetButtonString(bu).Length; //args[0];
    }

    //public string[] args;
    //public int index;
    public bool visible;
    public TMPro.TMP_Text text;

    const float widthBoost = 1.0f;
    float upoffset;

    //build a button sprite with given args
    //position is handled by the thing that makes the button sprites
    //(note for later: may need to check font size and stuff like that)
    public static GameObject Create(string[] args, int index, float relativeSize)
    {
        GameObject obj = Instantiate(MainManager.Instance.text_ButtonSprite);
        Text_ButtonSprite bs = obj.GetComponent<Text_ButtonSprite>();
        bs.args = args;
        bs.index = index;
        bs.relativeSize = relativeSize;

        if (args != null && args.Length > 0)
        {
            InputManager.Button b;
            Enum.TryParse(args[0], true, out b);
            bs.text.text = MainManager.Instance.GetButtonString(b); //args[0];
        } else
        {
            bs.text.text = "";
        }

        bs.RecalculateBoxSize();

        return obj;
    }

    public static GameObject Create(string button)
    {
        GameObject obj = Instantiate(MainManager.Instance.text_ButtonSprite);
        Text_ButtonSprite bs = obj.GetComponent<Text_ButtonSprite>();
        bs.args = new string[] { button };
        bs.index = -1;
        bs.visible = true;
        bs.text.text = button;

        //bs.RecalculateBoxSize();

        return obj;
    }

    public override void RecalculateBoxSize()
    {
        text.ForceMeshUpdate();

        text.fontSize = defaultSize * relativeSize;
        baseBox.rectTransform.sizeDelta = new Vector2((text.text.Length * 12 + 18) * relativeSize, 30 * relativeSize); //baseBox.rectTransform.sizeDelta.y);
    }
}
