using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.EventSystems.EventTrigger;

//has a background image
public class EquipBoxMenuEntryScript : BoxMenuEntryScript
{
    BadgeMenuEntry.EquipType et;

    public void Setup(BadgeMenuEntry p_entry, bool active = true)
    {
        entry = p_entry;

        if (entry == null)
        {
            maintext.SetText("", true, true);
            righttext.SetText("", true, true);
            background.gameObject.SetActive(false);
            return;
        }

        //maintext.text = entry.name;
        //righttext.text = entry.rightText + entry.spriteString;

        string mainString = entry.name;
        string rightString = entry.rightText + entry.spriteString;

        this.et = p_entry.et;
        if (active)
        {
            SetActiveColor();
        }
        else
        {
            SetInactiveColor();
        }

        if (entry.canUse)
        {
            //maintext.color = enabledColor;
            //righttext.color = enabledColor;
        }
        else
        {
            //maintext.color = disabledColor;
            //righttext.color = disabledColor;

            mainString = "<descriptionfluffcolor>" + mainString + "</color>";
            rightString = "<descriptionfluffcolor>" + rightString + "</color>";
        }

        //Debug.Log("Badge: " + mainString);

        maintext.SetText(mainString, true, true);
        righttext.SetText(rightString, true, true);
    }

    public void SetActiveColor()
    {
        switch (et)
        {
            case BadgeMenuEntry.EquipType.None:
                background.gameObject.SetActive(false);
                break;
            case BadgeMenuEntry.EquipType.Wilex:
                background.gameObject.SetActive(true);
                background.color = new Color(1f, 0.5f, 0.5f);
                break;
            case BadgeMenuEntry.EquipType.Luna:
                background.gameObject.SetActive(true);
                background.color = new Color(1f, 1f, 0.5f);
                break;
            case BadgeMenuEntry.EquipType.Party:
                background.gameObject.SetActive(true);
                background.color = new Color(1f, 0.75f, 0.5f);
                break;
        }
    }
    public void SetInactiveColor()
    {
        switch (et)
        {
            case BadgeMenuEntry.EquipType.None:
                background.gameObject.SetActive(false);
                break;
            case BadgeMenuEntry.EquipType.Wilex:
                background.gameObject.SetActive(true);
                background.color = new Color(0.75f, 0.325f, 0.325f);
                break;
            case BadgeMenuEntry.EquipType.Luna:
                background.gameObject.SetActive(true);
                background.color = new Color(0.75f, 0.75f, 0.325f);
                break;
            case BadgeMenuEntry.EquipType.Party:
                background.gameObject.SetActive(true);
                background.color = new Color(0.75f, 0.5625f, 0.325f);
                break;
        }
    }

    public void Setup(RibbonMenuEntry p_entry, bool active = true)
    {
        entry = p_entry;

        if (entry == null)
        {
            maintext.SetText("", true, true);
            righttext.SetText("", true, true);
            background.gameObject.SetActive(false);
            return;
        }

        //maintext.text = entry.name;
        //righttext.text = entry.rightText + entry.spriteString;

        string mainString = entry.name;
        string rightString = entry.rightText + entry.spriteString;

        this.et = p_entry.et;
        if (active)
        {
            SetActiveColor();
        }
        else
        {
            SetInactiveColor();
        }

        if (entry.canUse)
        {
            //maintext.color = enabledColor;
            //righttext.color = enabledColor;
        }
        else
        {
            //maintext.color = disabledColor;
            //righttext.color = disabledColor;

            mainString = "<descriptionfluffcolor>" + mainString + "</color>";
            rightString = "<descriptionfluffcolor>" + rightString + "</color>";
        }

        //Debug.Log("Badge: " + mainString);

        maintext.SetText(mainString, true, true);
        righttext.SetText(rightString, true, true);
    }
}
