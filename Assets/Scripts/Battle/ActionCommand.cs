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

    //How long should certain action commands stay on screen to give you feedback? (Not all action commands use this)
    public const float FEEDBACK_TIME = 0.30f;

    //How long the fade in animation is
    public const float FADE_IN_TIME = 0.1f;

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
        return autoComplete || InputManager.GetButtonPressInWindow(InputManager.Button.A, TIMING_WINDOW);
    }

    public override void Update()
    {
        base.Update();

        if (lifetime > delay - TIMING_WINDOW)
        {
            if (!showPress)
            {
                showPress = true;
            }
        }
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
            subObjects = new List<GameObject>
            {
                o
            };
            acobject = o.GetComponent<ACObject_MashBar>();
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
        return lifetime >= duration || (int)state > (int)AC_State.Active;
    }


    public void Setup(float p_duration = DEFAULT_DURATION, int p_difficulty = DEFAULT_MASH_DIFFICULTY)
    {
        duration = p_duration;
        mashObjective = p_difficulty;
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
        bool success = mashCount == mashObjective;
        displayBar = MainManager.EasingExponentialTime(displayBar, completion, 0.3f);

        if (!autoComplete)
        {
            acobject.SetValues(completion, success);
        }

        switch (state)
        {
            case AC_State.Idle:
                if (InputManager.GetAxisHorizontal() < 0 && lifetime >= FADE_IN_TIME)
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
                if (InputManager.GetAxisHorizontal() < 0)
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
                    state = AC_State.Complete;
                }
                break;
            case AC_State.Complete:
                break;
        }
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
            subObjects = new List<GameObject>
            {
                o
            };
            acobject = o.GetComponent<ACObject_HoldBar>();
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


    public void Setup(float p_duration = DEFAULT_DURATION, float p_window = TIMING_WINDOW)
    {
        duration = p_duration;
        window = p_window;
    }


    public bool GetSuccess()
    {
        return autoComplete || holdTime >= duration && holdTime < duration + window;
    }

    public override void Update()
    {
        base.Update();

        float completion = Mathf.Clamp01((holdTime / duration));
        bool success = (holdTime >= duration && holdTime < duration + window);

        if (!autoComplete)
        {
            acobject.SetValues(completion, success);
        }

        switch (state)
        {
            case AC_State.Idle:
                if (InputManager.GetAxisHorizontal() < 0 && lifetime >= FADE_IN_TIME)
                {
                    state = AC_State.Active;
                }
                break;
            case AC_State.Active:
                holdTime += Time.deltaTime; //not sure about whether to put this before or after, probably won't make much of a difference
                if (InputManager.GetAxisHorizontal() >= 0 || holdTime >= duration + window)
                {
                    state = AC_State.Complete;
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
}

public class AC_PressButtonTimed : ActionCommand
{
    public const float DEFAULT_DURATION = 0.5f;
    public float duration;
    public float window;

    ACObject_PressButtonTimed acobject;
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
            subObjects = new List<GameObject>
            {
                o
            };
            acobject = o.GetComponent<ACObject_PressButtonTimed>();
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
        return finishTime > FEEDBACK_TIME;
    }


    public void Setup(float p_duration = DEFAULT_DURATION, float p_window = TIMING_WINDOW)
    {
        duration = p_duration;
        window = p_window;
    }


    public bool GetSuccess()
    {
        return autoComplete || pressTime >= duration && pressTime < duration + TIMING_WINDOW;
    }

    public override void Update()
    {
        base.Update();

        float completion = Mathf.Clamp01(((lifetime - FADE_IN_TIME) / duration)) ;
        bool success = (pressTime >= duration && pressTime < duration + window);

        if (!autoComplete)
        {
            acobject.SetValues(completion, success, state);
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
                if (InputManager.GetButtonDown(InputManager.Button.A) || (lifetime >= duration + window))
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
}