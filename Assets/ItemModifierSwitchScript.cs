using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemModifierSwitchScript : MonoBehaviour, ISmashTrigger, ISlashTrigger
{
    public TextDisplayer textbox;

    public float cooldown;

    public bool Slash(Vector3 slashvector, Vector3 playerpos)
    {
        if (cooldown > 0)
        {
            return false;
        }
        MainManager.Instance.SetGlobalFlag(MainManager.GlobalFlag.GF_RandomItemModifiers, !MainManager.Instance.GetGlobalFlag(MainManager.GlobalFlag.GF_RandomItemModifiers));
        SetText();
        cooldown = 0.25f;
        return true;
    }

    public bool Smash(Vector3 smashvector, Vector3 playerpos)
    {
        if (cooldown > 0)
        {
            return false;
        }
        MainManager.Instance.SetGlobalFlag(MainManager.GlobalFlag.GF_RandomItemModifiers, !MainManager.Instance.GetGlobalFlag(MainManager.GlobalFlag.GF_RandomItemModifiers));
        SetText();
        cooldown = 0.35f;
        return true;
    }

    public void SetText()
    {
        //Note: can't put buttons on world objects because canvas objects don't really work in the world
        //It ends up in the correct place but it just displays as invisible
        //Future todo: make a second version of all the special sprites that works in the overworld?
        textbox.SetText("Item Modifiers: " + (MainManager.Instance.GetGlobalFlag(MainManager.GlobalFlag.GF_RandomItemModifiers) ? "ON" : "OFF") + "<line>(Smash this to toggle)", true);
    }

    public void Start()
    {
        SetText();
    }

    public void Update()
    {
        if (cooldown > 0)
        {
            cooldown -= Time.deltaTime;
        } else
        {
            cooldown = 0;
        }
    }
}
