using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemModifierSwitchScript : MonoBehaviour, ISignalReceiver
{
    public TextDisplayer textbox;

    public LeverScript lever;

    public void Start()
    {
        lever.target = this;
        SetText();
    }

    public void ReceiveSignal(int signal)
    {
        if (signal == 1)
        {
            Set(true);
        } else
        {
            Set(false);
        }
    }

    public void Set(bool set)
    {
        MainManager.Instance.SetGlobalFlag(MainManager.GlobalFlag.GF_RandomItemModifiers, set);
        SetText();
    }

    public void SetText()
    {
        //Note: can't put buttons on world objects because canvas objects don't really work in the world
        //It ends up in the correct place but it just displays as invisible
        //Future todo: make a second version of all the special sprites that works in the overworld?
        textbox.SetText("Item Modifiers: " + (MainManager.Instance.GetGlobalFlag(MainManager.GlobalFlag.GF_RandomItemModifiers) ? "ON" : "OFF") + "<line>(Smash this to toggle)", true);
    }
}
