using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

//attached to certain moves
//note that most moves will probably use 
public abstract class ActionCommand : MonoBehaviour
{
    public enum AC_Type
    {
        Jump,
        HoldLeft,
        HoldLeftBar
    }

    public enum AC_State
    {
        Idle,
        Active,
        Complete
    }

    public static Color COLOR_ON = new Color(1, 1, 0.75f);
    public static Color COLOR_OFF = new Color(0.5f, 0.25f, 0);


    //how close to being done is the command? (may auto end at 100%)
    protected bool autoComplete;
    protected List<GameObject> subObjects;
    protected PlayerEntity caller = null;

    public float lifetime = 0;

    //making these base constants for now
    //(Usually these are consistent)
    //(not much of a reason to make the timings tighter in some places and looser in others?)
    public const float TIMING_WINDOW = 0.15f;  //"9 frames"
    public const float PERFECT_TIMING_WINDOW = 0.05f;  //"3 frames"

    public float GetTimingWindow()
    {
        float window = TIMING_WINDOW;

        if (MainManager.Instance.GetGlobalFlag(MainManager.GlobalFlag.GF_FileCode_Pride))
        {
            window /= 2;
        }

        if (SettingsManager.Instance.GetSetting(SettingsManager.Setting.EasyActionCommands) != 0)
        {
            window *= 1.5f;
        }

        return window;
    }

    //How long should certain action commands stay on screen to give you feedback?
    public const float END_LAG = 0.15f;

    //How long the fade in animation is
    public const float FADE_IN_TIME = 0.1f;

    public GameObject descriptionBoxO;
    public DescriptionBoxScript descriptionBoxScript;

    public abstract bool IsStarted();   //(usually used for polling loops) usually it waits until the thing is true (the holding [direction] command uses this to wait to start)
    public abstract bool IsComplete();      //(usually used for polling loops) When it returns true, the action command is done (not accepting input anymore)

    //public void Setup(?)  //sets up the actual parameters and stuff (Different for each command so I don't have a base method here)

    //initialize everything needed
    public virtual void Init(PlayerEntity p_caller)
    {
        autoComplete = p_caller.AutoActionCommand();
        subObjects = new List<GameObject>();
        caller = p_caller;
        lifetime = 0;

        if (!autoComplete)
        {
            descriptionBoxO = Instantiate(MainManager.Instance.descriptionBoxBase, MainManager.Instance.Canvas.transform);
            descriptionBoxO.GetComponent<RectTransform>().anchoredPosition = descriptionBoxO.GetComponent<RectTransform>().anchoredPosition[1] * Vector2.up;
            descriptionBoxScript = descriptionBoxO.GetComponent<DescriptionBoxScript>();
            descriptionBoxScript.SetText(GetDescription(), true);
            subObjects.Add(descriptionBoxO);
        }
    }

    //destroy everything (then you get rid of the actioncommand)
    public virtual void End()
    {
        for (int i = 0; i < subObjects.Count; i++)
        {
            Destroy(subObjects[i]);
        }
    }

    public float FadeInProgress()
    {
        return Mathf.Clamp01(lifetime / FADE_IN_TIME);
    }

    public virtual void Update()
    {
        lifetime += Time.deltaTime;
    }

    public abstract string GetDescription();
}

public class AC_Jump : ActionCommand
{
    //this is very dependent on animations, so no default parameters

    public float delay = -1;

    bool showPress = false;

    public override void Init(PlayerEntity caller)
    {
        base.Init(caller);
    }

    public override void End()
    {
        base.End();
    }

    public override bool IsStarted()
    {
        return true;
    }
    public override bool IsComplete()
    {
        return false;
    }


    public void Setup(float p_delay)
    {
        delay = p_delay;
    }


    public bool GetSuccess()
    {
        return autoComplete || InputManager.GetButtonPressInWindow(InputManager.Button.A, GetTimingWindow());
    }

    public override void Update()
    {
        base.Update();

        if (lifetime > delay - GetTimingWindow())
        {
            if (!showPress)
            {
                showPress = true;
            }
        }
    }

    public static string GetACDesc()
    {
        return "Press <button,A> right before landing on the target.";
    }

    public override string GetDescription()
    {
        return GetACDesc();
    }
}

public class AC_MashLeft : ActionCommand
{
    public const float DEFAULT_DURATION = 2.0f;
    public const int DEFAULT_MASH_DIFFICULTY = 15; //default duration, how much do you have to mash
    public float duration;
    public AC_State state;

    public int mashCount;
    public int mashObjective;

    public float displayBar;

    ACObject_MashBar acobject;

    public bool isHolding = false;

    public override void Init(PlayerEntity caller)
    {
        base.Init(caller);
        state = AC_State.Idle;
        mashCount = 0;

        if (!autoComplete)
        {
            GameObject o = Instantiate(Resources.Load<GameObject>("Battle/ActionCommand/AC_MashLeft"), MainManager.Instance.Canvas.transform);
            subObjects.Add(o);
            acobject = o.GetComponent<ACObject_MashBar>();
            acobject.SetValues(0, false);
        }
    }

    public override void End()
    {
        //?
        if (!GetSuccess())
        {
            MainManager.Instance.PlayGlobalSound(MainManager.Sound.SFX_AC_Fail);
        }

        base.End();
    }

    public override bool IsStarted()
    {
        return (autoComplete) || (int)state > (int)AC_State.Idle;
    }
    public override bool IsComplete()
    {
        return lifetime >= duration || (int)state > (int)AC_State.Active;
    }


    public void Setup(float p_duration = DEFAULT_DURATION, int p_difficulty = DEFAULT_MASH_DIFFICULTY)
    {
        duration = p_duration;
        mashObjective = p_difficulty;

        if (MainManager.Instance.GetGlobalFlag(MainManager.GlobalFlag.GF_FileCode_Pride))
        {
            //1.5 is really hard
            mashObjective = Mathf.CeilToInt(mashObjective * 1.25f);
        }

        if (SettingsManager.Instance.GetSetting(SettingsManager.Setting.EasyActionCommands) != 0)
        {
            mashObjective = Mathf.CeilToInt(mashObjective * 0.666f);
        }
    }


    public bool GetSuccess()
    {
        return autoComplete || mashCount == mashObjective;
    }

    public float GetCompletion()
    {
        return mashCount / (mashObjective + 0.0f);
    }

    public override void Update()
    {
        base.Update();

        float completion = mashCount / (mashObjective + 0.0f);
        bool success = GetSuccess();
        displayBar = MainManager.EasingExponentialTime(displayBar, completion, 0.3f);

        if (!autoComplete)
        {
            if (lifetime <= duration)
            {
                acobject.SetValues(completion, success);
            }
        }

        switch (state)
        {
            case AC_State.Idle:
                if (autoComplete || (InputManager.GetAxisHorizontal() < -0.5f && lifetime >= FADE_IN_TIME))
                {
                    state = AC_State.Active;
                }
                break;
            case AC_State.Active:
                /*
                holdTime += Time.deltaTime; //not sure about whether to put this before or after, probably won't make much of a difference
                if (MainManager.GetAxisHorizontal() >= 0 || holdTime >= duration + window)
                {
                    state = AC_State.Complete;
                }
                */
                if (InputManager.GetAxisHorizontal() < -0.5f)
                {
                    if (!isHolding)
                    {
                        mashCount++;
                        if (mashCount > mashObjective)
                        {
                            mashCount = mashObjective;
                        }
                    }
                    isHolding = true;
                } else
                {
                    isHolding = false;
                }

                if (completion == 1)
                {
                    MainManager.Instance.PlayGlobalSound(MainManager.Sound.SFX_AC_Success);
                    state = AC_State.Complete;
                }
                break;
            case AC_State.Complete:
                break;
        }
    }

    public static string GetACDesc()
    {
        return "Mash <button,left> to fill the bar.";
    }

    public override string GetDescription()
    {
        return GetACDesc();
    }
}

public class AC_MashLeftRight : ActionCommand
{
    public const float DEFAULT_DURATION = 2.0f;
    public const int DEFAULT_MASH_DIFFICULTY = 15; //default duration, how much do you have to mash
    public float duration;
    public AC_State state;

    public int mashCount;
    public int mashObjective;

    public float displayBar;

    ACObject_MashBar acobject;

    public bool isHolding = false;
    public bool rightLast = false;

    public override void Init(PlayerEntity caller)
    {
        base.Init(caller);
        state = AC_State.Idle;
        mashCount = 0;

        if (!autoComplete)
        {
            GameObject o = Instantiate(Resources.Load<GameObject>("Battle/ActionCommand/AC_MashLeft"), MainManager.Instance.Canvas.transform);
            subObjects.Add(o);
            acobject = o.GetComponent<ACObject_MashBar>();
            acobject.SetValues(0, false);
            acobject.controlHint.SetText("Mash <button,left> and <button,right>", true, true);
        }
    }

    public override void End()
    {
        //?
        if (!GetSuccess())
        {
            MainManager.Instance.PlayGlobalSound(MainManager.Sound.SFX_AC_Fail);
        }

        base.End();
    }

    public override bool IsStarted()
    {
        return (autoComplete) || (int)state > (int)AC_State.Idle;
    }
    public override bool IsComplete()
    {
        return lifetime >= duration || (int)state > (int)AC_State.Active;
    }


    public void Setup(float p_duration = DEFAULT_DURATION, int p_difficulty = DEFAULT_MASH_DIFFICULTY)
    {
        duration = p_duration;
        mashObjective = p_difficulty;

        if (MainManager.Instance.GetGlobalFlag(MainManager.GlobalFlag.GF_FileCode_Pride))
        {
            mashObjective = Mathf.CeilToInt(mashObjective * 1.5f);
        }

        if (SettingsManager.Instance.GetSetting(SettingsManager.Setting.EasyActionCommands) != 0)
        {
            mashObjective = Mathf.CeilToInt(mashObjective * 0.666f);
        }
        acobject.controlHint.SetText("Mash <button,left> and <button,right>", true, true);
    }


    public bool GetSuccess()
    {
        return autoComplete || mashCount == mashObjective;
    }

    public float GetCompletion()
    {
        return mashCount / (mashObjective + 0.0f);
    }

    public override void Update()
    {
        base.Update();

        float completion = mashCount / (mashObjective + 0.0f);
        bool success = GetSuccess();
        displayBar = MainManager.EasingExponentialTime(displayBar, completion, 0.3f);

        if (!autoComplete)
        {
            if (lifetime <= duration)
            {
                acobject.SetValues(completion, success);
                acobject.controlHint.SetText("Mash <button,left> and <button,right>", true, true);
            }
        }

        switch (state)
        {
            case AC_State.Idle:
                if (autoComplete || (InputManager.GetAxisHorizontal() < -0.5f && lifetime >= FADE_IN_TIME))
                {
                    state = AC_State.Active;
                }
                break;
            case AC_State.Active:
                if (InputManager.GetAxisHorizontal() * (rightLast ? 1 : -1) < -0.5f)
                {
                    rightLast = !rightLast;
                    mashCount++;
                    if (mashCount > mashObjective)
                    {
                        mashCount = mashObjective;
                    }
                    isHolding = true;
                }
                else
                {
                    isHolding = false;
                }

                if (completion == 1)
                {
                    MainManager.Instance.PlayGlobalSound(MainManager.Sound.SFX_AC_Success);
                    state = AC_State.Complete;
                }
                break;
            case AC_State.Complete:
                break;
        }
    }

    public static string GetACDesc()
    {
        return "Alternate <button,left> and <button,right> quickly to fill the bar.";
    }

    public override string GetDescription()
    {
        return GetACDesc();
    }
}

public class AC_HoldLeft : ActionCommand
{
    public const float DEFAULT_DURATION = 1.0f;
    public float duration;
    public float window;
    public AC_State state;

    public float holdTime = 0;

    ACObject_HoldBar acobject;

    public override void Init(PlayerEntity caller)
    {
        base.Init(caller);
        state = AC_State.Idle;
        holdTime = 0;

        if (!autoComplete)
        {
            GameObject o = Instantiate(Resources.Load<GameObject>("Battle/ActionCommand/AC_HoldLeft"), MainManager.Instance.Canvas.transform);
            subObjects.Add(o);
            acobject = o.GetComponent<ACObject_HoldBar>();
            acobject.SetValues(0, false);
        }
    }

    public override void End()
    {
        base.End();
    }

    public override bool IsStarted()
    {
        return (autoComplete) || (int)state > (int)AC_State.Idle;
    }
    public override bool IsComplete()
    {
        return (int)state > (int)AC_State.Active;
    }


    public void Setup(float p_duration = DEFAULT_DURATION)
    {
        duration = p_duration;
        window = GetTimingWindow();
    }


    public bool GetSuccess()
    {
        return autoComplete || (holdTime >= duration && holdTime < duration + GetTimingWindow());
    }

    public override void Update()
    {
        base.Update();

        float completion = Mathf.Clamp01((holdTime / duration));
        bool success = GetSuccess();

        if (!autoComplete)
        {
            if (!IsComplete())
            {
                acobject.SetValues(completion, success);
            }
        }

        switch (state)
        {
            case AC_State.Idle:
                //Debug.Log(((InputManager.GetAxisHorizontal() < 0) + " " + (lifetime >= FADE_IN_TIME)) + " " + state);
                if (autoComplete || (InputManager.GetAxisHorizontal() < 0 && lifetime >= FADE_IN_TIME))
                {
                    state = AC_State.Active;
                }
                break;
            case AC_State.Active:
                holdTime += Time.deltaTime; //not sure about whether to put this before or after, probably won't make much of a difference
                if (!autoComplete && (InputManager.GetAxisHorizontal() >= 0 || holdTime >= duration + window))
                {
                    state = AC_State.Complete;

                    if (GetSuccess())
                    {
                        MainManager.Instance.PlayGlobalSound(MainManager.Sound.SFX_AC_Success);
                    } else
                    {
                        MainManager.Instance.PlayGlobalSound(MainManager.Sound.SFX_AC_Fail);
                    }
                }
                if (autoComplete && holdTime >= duration)
                {
                    state = AC_State.Complete;
                }
                break;
            case AC_State.Complete:
                break;
        }
    }

    public static string GetACDesc()
    {
        return "Hold <button,left> until the bar is full, then release <button,left>.";
    }

    public override string GetDescription()
    {
        return GetACDesc();
    }
}

public class AC_PressATimed : ActionCommand
{
    public const float DEFAULT_DURATION = 0.5f;
    public float duration;
    public float window;

    ACObject_PressATimed acobject;
    public AC_State state;

    public float finishTime;
    public float pressTime;

    public override void Init(PlayerEntity caller)
    {
        base.Init(caller);

        state = AC_State.Idle;

        if (!autoComplete)
        {
            GameObject o = Instantiate(Resources.Load<GameObject>("Battle/ActionCommand/AC_PressButtonTimed"), MainManager.Instance.Canvas.transform);
            subObjects.Add(o);
            acobject = o.GetComponent<ACObject_PressATimed>();
            acobject.SetValues(0, false, AC_State.Idle);
        }
    }

    public override void End()
    {
        base.End();
    }

    public override bool IsStarted()
    {
        return true;
    }
    public override bool IsComplete()
    {
        return finishTime > END_LAG;
    }


    public void Setup(float p_duration = DEFAULT_DURATION)
    {
        duration = p_duration;
        window = GetTimingWindow();
    }


    public bool GetSuccess()
    {
        return autoComplete || pressTime >= duration && pressTime < duration + GetTimingWindow();
    }

    public override void Update()
    {
        base.Update();

        float completion = Mathf.Clamp01(((lifetime - FADE_IN_TIME) / duration)) ;
        bool success = GetSuccess();

        if (!autoComplete)
        {
            /*
            if (lifetime <= duration + GetTimingWindow())
            {
                
            }
            */
            if (state == AC_State.Complete)
            {
                completion = Mathf.Clamp01(((pressTime - FADE_IN_TIME) / duration));
                acobject.SetValues(completion, success, state);
            }
            else
            {
                acobject.SetValues(completion, success, state);
            }
        }

        switch (state)
        {
            case AC_State.Idle:
                if (lifetime >= FADE_IN_TIME)
                {
                    state = AC_State.Active;
                }
                break;
            case AC_State.Active:
                if (!autoComplete && (InputManager.GetButtonDown(InputManager.Button.A) || (lifetime >= duration + GetTimingWindow())))
                {
                    state = AC_State.Complete;
                    pressTime = lifetime;
                    finishTime = 0;

                    if (GetSuccess())
                    {
                        MainManager.Instance.PlayGlobalSound(MainManager.Sound.SFX_AC_Success);
                    } else
                    {
                        MainManager.Instance.PlayGlobalSound(MainManager.Sound.SFX_AC_Fail);
                    }
                }
                if (autoComplete && lifetime >= duration)
                {
                    state = AC_State.Complete;
                    pressTime = lifetime;
                    finishTime = 0;
                }
                break;
            case AC_State.Complete:
                finishTime += Time.deltaTime;
                break;
        }
    }

    public static string GetACDesc()
    {
        return "Press <button,a> when the large box and the small box are the same size.";
    }

    public override string GetDescription()
    {
        return GetACDesc();
    }
}

public class AC_PressATimedMultiple : ActionCommand
{
    public const float DEFAULT_DURATION = 0.5f;
    public float duration;
    public float window;
    public int amount;
    public float[] completionAmounts;
    public int successes;

    ACObject_PressATimedMultiple acobject;
    public AC_State state;

    public float finishTime;
    public float stopTime;

    public override void Init(PlayerEntity caller)
    {
        base.Init(caller);

        state = AC_State.Idle;

        if (!autoComplete)
        {
            GameObject o = Instantiate(Resources.Load<GameObject>("Battle/ActionCommand/AC_PressButtonTimedMultiple"), MainManager.Instance.Canvas.transform);
            subObjects.Add(o);
            acobject = o.GetComponent<ACObject_PressATimedMultiple>();
        }
    }

    public override void End()
    {
        base.End();
    }

    public override bool IsStarted()
    {
        return true;
    }
    public override bool IsComplete()
    {
        return finishTime > END_LAG;
    }


    public void Setup(float p_duration = DEFAULT_DURATION, int p_amount = 2, float maxoffset = 0.25f)
    {
        duration = p_duration;
        window = GetTimingWindow();
        amount = p_amount;
        stopTime = -1;

        completionAmounts = new float[amount];
        for (int i = 0; i < completionAmounts.Length; i++)
        {
            if (i == completionAmounts.Length - 1)
            {
                completionAmounts[i] = 1;
            } else
            {
                completionAmounts[i] = 0.5f + (i + RandomGenerator.GetRange(-maxoffset, maxoffset)) * (0.5f / amount);
            }
        }
        acobject.SetValues(0, completionAmounts, 0, state);
    }


    public bool GetSuccess()
    {
        return autoComplete || successes >= amount;
    }

    public override void Update()
    {
        base.Update();

        float completion = Mathf.Clamp01(((lifetime - FADE_IN_TIME) / duration));
        bool success = GetSuccess();

        if (!autoComplete)
        {
            if (lifetime <= duration + GetTimingWindow())
            {
                if (state == AC_State.Complete)
                {
                    completion = Mathf.Clamp01(((stopTime - FADE_IN_TIME) / duration));
                    if (stopTime < 0)
                    {
                        completion = 1;
                    }
                    acobject.SetValues(completion, completionAmounts, successes, state);
                }
                else
                {
                    acobject.SetValues(completion, completionAmounts, successes, state);
                }
            }
        }

        switch (state)
        {
            case AC_State.Idle:
                if (lifetime >= FADE_IN_TIME)
                {
                    state = AC_State.Active;
                }
                break;
            case AC_State.Active:
                //press A
                if (!autoComplete && InputManager.GetButtonDown(InputManager.Button.A))
                {
                    //success or fail
                    float subduration = completionAmounts[successes] * duration;

                    if (lifetime >= subduration && lifetime < subduration + GetTimingWindow())
                    {
                        //success
                        successes++;
                        MainManager.Instance.PlayGlobalSound(MainManager.Sound.SFX_AC_Success);
                    } else
                    {
                        //fail
                        state = AC_State.Complete;
                        finishTime = 0;
                        stopTime = lifetime;
                        MainManager.Instance.PlayGlobalSound(MainManager.Sound.SFX_AC_Fail);
                    }
                }

                //timeout
                if (successes < amount && lifetime > completionAmounts[successes] * duration + GetTimingWindow())
                {
                    state = AC_State.Complete;
                    finishTime = 0;
                    stopTime = lifetime;
                    MainManager.Instance.PlayGlobalSound(MainManager.Sound.SFX_AC_Fail);
                }

                if (!autoComplete && (lifetime >= duration + GetTimingWindow()))
                {
                    state = AC_State.Complete;
                    finishTime = 0;
                }
                if ((autoComplete && lifetime >= duration) || successes == amount)
                {
                    state = AC_State.Complete;
                    successes = amount;
                    finishTime = 0;
                }
                break;
            case AC_State.Complete:
                finishTime += Time.deltaTime;
                break;
        }
    }

    public static string GetACDesc()
    {
        return "Press <button,a> when the large boxes and the small box are the same size.";
    }

    public override string GetDescription()
    {
        return GetACDesc();
    }
}