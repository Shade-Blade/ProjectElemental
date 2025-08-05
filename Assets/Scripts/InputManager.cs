using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using static MainManager;

//Class for all the input stuff
//MainManager is the usual place to ask for stuff though (for convenience)
public class InputManager : MonoBehaviour
{
    public static InputManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<InputManager>(); //this should work
            }
            return instance;
        }
        private set
        {
            instance = value;
        }
    }
    private static InputManager instance;

    //Input (for easier rebinding)
    //Horizontal and vertical axes are different so may need separate stuff
    //They still have definitions here for convenience and so that I can have stuff in text form ("press [] to go left")
    public enum Button
    {
        Up = -4,
        Down = -3,
        Right = -2,
        Left = -1,
        A = 0,  //main button, select stuff or jump
        B,      //secondary button, back button
        Z,      //tertiary button, (switch in overworld)
        Y,      //overworld tattle? (sometimes does opposite of Z)
        L,      //idk
        R,      //will probably be show/hide hud and other info
        Start   //pause menu
    }

    //treat joystick input in this range as neutral
    //(but not everything treats it as 0 though)
    public const float DEAD_ZONE = 0.15f;

    //60 entries, so 1 second history
    //If framerate is slower, you get more history but the code should not assume that there exists data before 1 second ago
    public CircleBuffer<InputSnapshot> inputCircleBuffer;

    public KeyCode[] keyCodes;
    public KeyCode[] directionalKeyCodes;   //Up down right left

    public string[][] inputText;

    //Sidenote: Start screen will have F1 reset your controls as a failsafe for you messing up your controls so badly you can't use menus correctly
    //public KeyCode f1 = KeyCode.F1;

    //Used by situations where all controls are disabled (and instead the key down events are tracked to get the keys used)
    //  (Only used by the rebinding menu and cheat menu, there is no reason to use it otherwise)
    public bool disableControl;    //Set to true by the settings menu thing and the cheat menu
    //(they also set it to false)

    public void Awake()
    {
        inputCircleBuffer = new CircleBuffer<InputSnapshot>(60);
        SettingsManager.Instance.LoadSettings();
    }

    public void LateUpdate()
    {
        inputCircleBuffer.Push(InputSnapshot.TakeSnapshot());
    }


    public void LoadInputText()
    {
        inputText = MainManager.GetAllTextFromFile("DialogueText/InputText");
    }
    public string GetInputText(Button b, int index)
    {
        if (inputText == null)
        {
            LoadInputText();
        }
        return inputText[(int)b + 5][index];
    }
    public string GetResetText(int index)
    {
        return inputText[12][index];
    }

    //stuff for testing any key down
    //not sure if it has to be in OnGUI
    //probably yes? (Needs to access Event.current)
    /*
    void OnGUI()
    {
        Event e = Event.current;
        if (e.isKey)
        {
            Debug.Log("Detected key code: " + e.keyCode + " Type letter " + e.character);
        }
        //e.character does respond to shift and caps lock

        //Characters to ban to avoid problems: <>,&
        //  (also \n)
        //The banned characters would let you make tags or escaped text which would mess things up
    }
    */


    //Go to https://docs.unity3d.com/Packages/com.unity.tiny@0.13/rt/tiny_runtime/enums/_runtimefull_.ut.core2d.keycode.html#slash
    //for keycodes

    public void ResetToDefaultKeyCodes()
    {
        Debug.Log("Reset controls");
        //Space, slash, period, comma, e, esc
        //keyCodes = new KeyCode[] {(KeyCode)32, (KeyCode)42, (KeyCode)46, (KeyCode)44, (KeyCode)101, (KeyCode)27};
        keyCodes = new KeyCode[] { KeyCode.Space, KeyCode.C, KeyCode.X, KeyCode.Z, KeyCode.Q, KeyCode.E, KeyCode.Return };
        //keyCodes = new KeyCode[] { KeyCode.Space, KeyCode.LeftShift, KeyCode.Period, KeyCode.Comma, KeyCode.E, KeyCode.Escape };

        //W, S, D, A
        //directionalKeyCodes = new KeyCode[] { (KeyCode)119, (KeyCode)115, (KeyCode)100, (KeyCode)97 };
        directionalKeyCodes = new KeyCode[] { KeyCode.W, KeyCode.S, KeyCode.D, KeyCode.A };
        //directionalKeyCodes = new KeyCode[] { KeyCode.UpArrow, KeyCode.DownArrow, KeyCode.RightArrow, KeyCode.LeftArrow };
    }

    //Standard get input
    public static bool GetButtonDown(Button b)
    {
        if (Instance.disableControl)
        {
            return false;
        }

        if (Instance.keyCodes == null || Instance.keyCodes.Length != 7)
        {
            Instance.ResetToDefaultKeyCodes();
        }
        /*
        switch (b)
        {
            case Button.A:
                return InputManager.GetButton(InputManager.Button.A);
            case Button.B:
                return InputManager.GetButton(InputManager.Button.B);
            case Button.Z:
                return Input.GetButtonDown("Z");
            case Button.Y:
                return Input.GetButtonDown("Y");
            case Button.R:
                return Input.GetButtonDown("R");
            case Button.Start:
                return Input.GetButtonDown("Start");
        }
        return false;
        */
        KeyCode counterpart = GetKeyCodeCounterpart(Instance.keyCodes[(int)b]);
        return Input.GetKeyDown(Instance.keyCodes[(int)b]) || (counterpart != KeyCode.None && Input.GetKeyDown(counterpart));
    }
    public static bool GetButtonUp(Button b)
    {
        if (Instance.disableControl)
        {
            return false;
        }

        if (Instance.keyCodes == null || Instance.keyCodes.Length != 7)
        {
            Instance.ResetToDefaultKeyCodes();
        }
        /*
        switch (b)
        {
            case Button.A:
                return Input.GetButtonUp("A");
            case Button.B:
                return Input.GetButtonUp("B");
            case Button.Z:
                return Input.GetButtonUp("Z");
            case Button.Y:
                return Input.GetButtonUp("Y");
            case Button.R:
                return Input.GetButtonUp("R");
        }
        return false;
        */
        KeyCode counterpart = GetKeyCodeCounterpart(Instance.keyCodes[(int)b]);
        return Input.GetKeyUp(Instance.keyCodes[(int)b]) || (counterpart != KeyCode.None && Input.GetKeyUp(counterpart));
    }
    public static bool GetButton(Button b)
    {
        if (Instance.disableControl)
        {
            return false;
        }

        if (Instance.keyCodes == null || Instance.keyCodes.Length != 7)
        {
            Instance.ResetToDefaultKeyCodes();
        }
        /*
        switch (b)
        {
            case Button.A:
                return Input.GetButton("A");
            case Button.B:
                return Input.GetButton("B");
            case Button.Z:
                return Input.GetButton("Z");
            case Button.Y:
                return Input.GetButton("Y");
            case Button.R:
                return Input.GetButton("R");
        }
        return false;
        */
        KeyCode counterpart = GetKeyCodeCounterpart(Instance.keyCodes[(int)b]);
        return Input.GetKey(Instance.keyCodes[(int)b]) || (counterpart != KeyCode.None && Input.GetKey(counterpart));
    }
    public static float GetAxisHorizontal()
    {
        if (Instance.disableControl)
        {
            return 0;
        }

        if (Instance.directionalKeyCodes == null || Instance.directionalKeyCodes.Length != 4)
        {
            Instance.ResetToDefaultKeyCodes();
        }
        float value = 0;
        KeyCode rightCounterpart = GetKeyCodeCounterpart(Instance.directionalKeyCodes[2]);
        KeyCode leftCounterpart = GetKeyCodeCounterpart(Instance.directionalKeyCodes[3]);
        if (rightCounterpart != KeyCode.None && Input.GetKey(rightCounterpart))
        {
            value += 1;
        } else
        {
            if (Input.GetKey(Instance.directionalKeyCodes[2]))
            {
                value += 1;
            }
        }
        if (leftCounterpart != KeyCode.None && Input.GetKey(leftCounterpart))
        {
            value -= 1;
        } else
        {
            if (Input.GetKey(Instance.directionalKeyCodes[3]))
            {
                value -= 1;
            }
        }
        float pastValue = value;
        if (Instance.inputCircleBuffer != null && Instance.inputCircleBuffer[-1] != null)
        {
            //Very subtle problem: -1 = 2 frames ago
            //0 = last frame (Because the current frame will be pasted onto the buffer in lateupdate)
            pastValue = Instance.inputCircleBuffer[0].horizontalValue;
            if (pastValue * value < 0)
            {
                pastValue = 0;
            }
            if (value == 0 && Mathf.Abs(pastValue) < 0.85f)
            {
                pastValue = 0;
            }
        }
        float offset = 20 * Time.deltaTime;  //Use this frame's delta time
        value = Mathf.Clamp(value, pastValue - offset, pastValue + offset);
        return value;
        //return InputManager.GetAxisHorizontal();
    }
    public static float GetAxisVertical()
    {
        if (Instance.disableControl)
        {
            return 0;
        }

        if (Instance.directionalKeyCodes == null || Instance.directionalKeyCodes.Length != 4)
        {
            Instance.ResetToDefaultKeyCodes();
        }
        float value = 0;
        KeyCode upCounterpart = GetKeyCodeCounterpart(Instance.directionalKeyCodes[0]);
        KeyCode downCounterpart = GetKeyCodeCounterpart(Instance.directionalKeyCodes[1]);
        if (upCounterpart != KeyCode.None && Input.GetKey(upCounterpart))
        {
            value += 1;
        } else
        {
            if (Input.GetKey(Instance.directionalKeyCodes[0]))
            {
                value += 1;
            }
        }
        if (downCounterpart != KeyCode.None && Input.GetKey(downCounterpart))
        {
            value -= 1;
        } else
        {
            if (Input.GetKey(Instance.directionalKeyCodes[1]))
            {
                value -= 1;
            }
        }
        float pastValue = value; 
        if (Instance.inputCircleBuffer != null && Instance.inputCircleBuffer[-1] != null)
        {
            //Very subtle problem: -1 = 2 frames ago
            //0 = last frame (Because the current frame will be pasted onto the buffer in lateupdate)
            pastValue = Instance.inputCircleBuffer[0].verticalValue;
            if (pastValue * value < 0)
            {
                pastValue = 0;
            }
            if (value == 0 && Mathf.Abs(pastValue) < 0.85f)
            {
                pastValue = 0;
            }
        }

        float offset = 20 * Time.deltaTime;  //Use this frame's delta time
        value = Mathf.Clamp(value, pastValue - offset, pastValue + offset);
        return value;
        //return InputManager.GetAxisVertical();
    }
    public static string GetKeyCodeString()
    {
        if (Instance.keyCodes.Length != 7)
        {
            Instance.ResetToDefaultKeyCodes();
        }
        string output = "";
        for (int i = 0; i < Instance.keyCodes.Length; i++)
        {
            if (i > 0)
            {
                output += ",";
            }
            output += Instance.keyCodes[i];
        }
        return output;
    }
    public static string GetDirectionalKeyCodeString()
    {
        if (Instance.directionalKeyCodes.Length != 4)
        {
            Instance.ResetToDefaultKeyCodes();
        }
        string output = "";
        for (int i = 0; i < Instance.directionalKeyCodes.Length; i++)
        {
            if (i > 0)
            {
                output += ",";
            }
            output += Instance.directionalKeyCodes[i];
        }
        return output;
    }
    public static void SetControlsWithKeyCodeStrings(string st, string dir)
    {
        string[] split = st.Split(",");
        KeyCode temp = KeyCode.None;
        Instance.ResetToDefaultKeyCodes();

        string[] splitb = dir.Split(",");

        for (int i = 0; i < split.Length; i++)
        {
            temp = KeyCode.None;
            Enum.TryParse(split[i], out temp);
            //Debug.Log(split[i] + " vs " + temp);

            if (temp != KeyCode.None)
            {
                Instance.keyCodes[i] = temp;
            }
        }

        for (int i = 0; i < splitb.Length; i++)
        {
            temp = KeyCode.None;
            Enum.TryParse(splitb[i], out temp);
            //Debug.Log(splitb[i] + " vs " + temp);

            if (temp != KeyCode.None)
            {
                Instance.directionalKeyCodes[i] = temp;
            }
        }
    }
    public static string GetButtonString(Button b)
    {
        if (Instance.keyCodes == null || Instance.keyCodes.Length != 7 || Instance.directionalKeyCodes == null || Instance.directionalKeyCodes.Length != 4)
        {
            Instance.ResetToDefaultKeyCodes();
        }
        KeyCode k = KeyCode.None;
        if ((int)b < 0)
        {
            k = Instance.directionalKeyCodes[(int)b + 4];
        } else
        {
            if ((int)b > Instance.keyCodes.Length - 1)
            {
                Debug.LogError("Impossible to find keycode for: " + b);
            }
            k = Instance.keyCodes[(int)b];
        }

        if (k < 0)
        {
            //failsafe
            return "";
        }

        //things that correspond to ascii codes? (except Space)
        if ((int)k >= 33 && (int)k <= 126)
        {
            return ((char)(k)).ToString();  //cursed casting
        }

        //various special cases
        if ((int)k >= 256 && (int)k <= 265)
        {
            //Keypad numbers
            return ((int)(k) - 256).ToString();
        }

        //arrows
        if (k == KeyCode.LeftArrow)
        {
            return "\u2190";
        }
        if (k == KeyCode.UpArrow)
        {
            return "\u2191";
        }
        if (k == KeyCode.RightArrow)
        {
            return "\u2192";
        }
        if (k == KeyCode.DownArrow)
        {
            return "\u2193";
        }

        //escape
        if (k == KeyCode.Escape)
        {
            return "Esc";
        }

        //does not catch left parenthesis and right parenthesis and brackets (because those are ascii)
        //"left" "right" -> (nothing)
        //(This is to convert LeftShift to Shift and stuff)
        if (k.ToString().Contains("Left"))
        {
            return k.ToString().Substring(4);
        }
        if (k.ToString().Contains("Right"))
        {
            return k.ToString().Substring(5);
        }

        return k.ToString();    //does this return the right things?
    }
    public static KeyCode GetKeyCodeCounterpart(KeyCode k)
    {
        //make things like numpad numbers and top of keyboard numbers work the same
        if ((int)k >= 48 && (int)k <= 57)
        {
            //Keypad numbers
            return (KeyCode)((int)(k) - 48 + 256);
        }
        if ((int)k >= 256 && (int)k <= 265)
        {
            //alpha numbers
            return (KeyCode)((int)(k) + 48 - 256);
        }

        //"left" and "right" things (left shift vs right shift)
        if ((int)k >= 303 && (int)k <= 312)
        {
            return (KeyCode)((int)(k) % 2 == 0 ? (int)(k) - 1 : (int)(k) + 1);
        }

        if (k == KeyCode.KeypadEnter)
        {
            return KeyCode.Return;
        }

        return KeyCode.None;
    }


    //Special get input (uses circle buffer)
    //a lot of these can be coded with states instead pretty easily (button presses are binary variables)
    //oh well
    public static bool GetButtonHeldLonger(Button b, float time)    //Has (button) been held down for >=(time) seconds? (because circle buffer only has >1 second of history this can't go above 1 sec)
    {
        if (Instance.disableControl)
        {
            return false;
        }

        int index = 0;
        float holdtime = 0;

        while (true)
        {
            index--;
            if (index < -59)    //circle buffer loops starting at -60
            {
                return true;
            }

            if (Instance.inputCircleBuffer[index] == null && Instance.inputCircleBuffer[-1] != null)
            {
                return false;
            }

            holdtime -= Instance.inputCircleBuffer[index].deltaTime;
            if (!Instance.inputCircleBuffer[index].GetActive(b))
            {
                return false;
            }
            if (-holdtime >= time)
            {
                return true;
            }
        }
    }
    //Logically, you can find if a button is held for shorter than (time) if the button is held and above check is false
    //  (this is not necessarily for tap inputs since you also need to check that you aren't holding for longer)
    public static bool GetButtonPressShorter(Button b, float time, float bufferTime = 0.1f)    //was the last time you pressed () less than X frames? (Note: use this for tap inputs, this one checks for not currently pressing)
    {
        if (Instance.disableControl)
        {
            return false;
        }

        //first, check not holding button
        if (GetButton(b))
        {
            return false;
        }

        /*
        string test = " ";
        for (int i = 0; i < 60; i++)
        {
            test += Instance.inputCircleBuffer[-i].GetActive(b);
            test += " ";
        }
        Debug.Log(test);
        */

        int index = -1;
        float holdtime = 0;

        bool test = false;

        while (true)
        {
            index--;
            if (index < -59)    //circle buffer loops starting at -60
            {
                return false;
            }

            if (Instance.inputCircleBuffer[index] == null)
            {
                return false;
            }

            holdtime -= Instance.inputCircleBuffer[index].deltaTime;

            if (test)
            {
                if (!Instance.inputCircleBuffer[index].GetActive(b))
                {
                    return true;    //Found a frame where you did not hold button that is within the acceptable range
                }
            }
            else
            {
                if (-holdtime > bufferTime)
                {
                    return false;
                }
                if (Instance.inputCircleBuffer[index].GetActive(b))
                {
                    test = true;
                }
            }


            if (-holdtime > time + bufferTime)
            {
                return false;
            }
        }
    }
    public static float GetTimeSincePressed(Button b) //(time in seconds), returns 1 if longer than 1 sec
    {
        if (Instance.disableControl)
        {
            return 1;
        }

        int ticks = 0;
        float time = 0;
        while (true)
        {
            if (ticks > 59)
            {
                return 1;
            }

            //delta time measures time between last frame and now
            //so if you find the frame where you pressed the button, do not add to time anymore
            if (Instance.inputCircleBuffer[-ticks].GetActive(b))
            {
                return time;
            }
            time += Instance.inputCircleBuffer[-ticks].deltaTime;

            ticks++;
        }
    }
    public static bool GetButtonPressInWindow(Button b, float ctime)    //did you press the button in the window? (i.e. there exists a frame within the window where GetButtonDown would be true)
    {
        if (Instance.disableControl)
        {
            return false;
        }

        //bool check = false;

        int ticks = 0;
        float time = 0;

        //Loop until true is found
        while (true)
        {
            if (ticks > 59) //no press in last time
            {
                return false;
            }

            //delta time measures time between last frame and now
            //so if you find the frame where you pressed the button, do not add to time anymore
            if (Instance.inputCircleBuffer[-ticks].GetActive(b))
            {
                break;  //Success
            }
            time += Instance.inputCircleBuffer[-ticks].deltaTime;

            if (time > ctime)
            {
                return false;   //This frame is outside the window and thus reaching this point is a fail
            }
            ticks++;
        }

        //Loop until false is found
        while (true)
        {
            if (ticks > 59) //no press in last time
            {
                return true;
            }

            //delta time measures time between last frame and now
            //so if you find the frame where you pressed the button, do not add to time anymore
            if (!Instance.inputCircleBuffer[-ticks].GetActive(b))
            {
                return true;
            }
            if (time > ctime)
            {
                return false;   //This frame is outside the window (but checking the first frame outside the window is intended, in case you press on the very first frame of the window)
            }

            time += Instance.inputCircleBuffer[-ticks].deltaTime;

            ticks++;
        }
    }

    public static bool GetNeutralJoystick()
    {
        if (Instance.disableControl)
        {
            return true;
        }

        return Mathf.Abs(GetAxisVertical()) < DEAD_ZONE && Mathf.Abs(GetAxisHorizontal()) < DEAD_ZONE;
    }
    //time you have been holding non-neutral joystick inputs (max of 1 second)
    //equivalent to "GetTimeHoldingJoystick" or something like that
    public static float GetTimeSinceNeutralJoystick()
    {
        if (Instance.disableControl)
        {
            return 0;
        }

        int ticks = 0;
        float time = 0;
        while (true)
        {
            if (ticks > 59)
            {
                return 1;
            }

            //delta time measures time between last frame and now
            //so if you find the frame where you pressed the button, do not add to time anymore
            if (Instance.inputCircleBuffer[-ticks].GetJoystickNeutral())
            {
                return time;
            }
            time += Instance.inputCircleBuffer[-ticks].deltaTime;

            ticks++;
        }
    }

    //checker for if you spun joystick input around
    public static bool SpinInput() //note: only checks the last 0.3s of input, having a neutral input invalidates this
    {
        if (Instance.disableControl)
        {
            return false;
        }

        //how to check this?
        //check for inputs in 3/4 quadrants
        //with quadrants being the coordinate quadrants
        //
        int ticks = 0;
        float time = 0;
        bool q1 = false;
        bool q2 = false;
        bool q3 = false;
        bool q4 = false;
        int qc = 0;
        bool lr = false;

        while (true)
        {
            if (ticks > 59 || time > 0.3f)
            {
                return false;
            }

            if (Instance.inputCircleBuffer[-ticks].GetJoystickNeutral())
            {
                return false;
            }

            if (Instance.inputCircleBuffer[-ticks].GetAxisHorizontal() > 0)
            {
                lr = true;
            }
            else
            {
                lr = false;
            }

            if (Instance.inputCircleBuffer[-ticks].GetAxisVertical() > 0)
            {
                if (lr)
                {
                    q1 = true;
                }
                else
                {
                    q2 = true;
                }
            }
            else
            {
                if (lr)
                {
                    q4 = true;
                }
                else
                {
                    q3 = true;
                }
            }

            qc = 0;
            if (q1)
            {
                qc++;
            }
            if (q2)
            {
                qc++;
            }
            if (q3)
            {
                qc++;
            }
            if (q4)
            {
                qc++;
            }

            if (qc >= 3)
            {
                return true;
            }

            time += Instance.inputCircleBuffer[-ticks].deltaTime;

            ticks++;
        }
    }

    //A snapshot of the input state
    //Useful for checking previous input using circle buffers
    public class InputSnapshot
    {
        public bool AState;
        public bool BState;
        public bool ZState;
        public bool YState;
        public bool RState;
        public float horizontalValue;
        public float verticalValue;
        public float deltaTime;

        public InputSnapshot(bool p_AState, bool p_BState, bool p_ZState, bool p_YState, bool p_RState, float p_hValue, float p_vValue, float p_deltaTime)
        {
            AState = p_AState;
            BState = p_BState;
            ZState = p_ZState;
            YState = p_YState;
            RState = p_RState;
            horizontalValue = p_hValue;
            verticalValue = p_vValue;
            deltaTime = p_deltaTime;
        }

        public static InputSnapshot TakeSnapshot()
        {
            return new InputSnapshot(GetButton(Button.A), GetButton(Button.B), GetButton(Button.Z), GetButton(Button.Y), GetButton(Button.R), InputManager.GetAxisHorizontal(), InputManager.GetAxisVertical(), Time.deltaTime);
        }

        public bool GetActive(Button b)
        {
            switch (b)
            {
                case Button.A:
                    return AState;
                case Button.B:
                    return BState;
                case Button.Z:
                    return ZState;
                case Button.Y:
                    return YState;
                case Button.R:
                    return RState;
            }
            return false;
        }

        public float GetAxisHorizontal()
        {
            return horizontalValue;
        }

        public float GetAxisVertical()
        {
            return verticalValue;
        }

        public bool GetJoystickNeutral()
        {
            return Mathf.Abs(verticalValue) < DEAD_ZONE && Mathf.Abs(horizontalValue) < DEAD_ZONE;
        }
    }
}
