using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//has a background image
public class EquipBoxMenuEntryScript : BoxMenuEntryScript
{
    public void Setup(BadgeMenuEntry p_entry)
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

        switch (p_entry.et)
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
                background.color = new Color(0.5f, 1f, 0.5f);
                break;
            case BadgeMenuEntry.EquipType.Party:
                background.gameObject.SetActive(true);
                background.color = new Color(1f, 1f, 0.5f);
                break;
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

    public void Setup(RibbonMenuEntry p_entry)
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

        switch (p_entry.et)
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
                background.color = new Color(0.5f, 1f, 0.5f);
                break;
            case BadgeMenuEntry.EquipType.Party:
                background.gameObject.SetActive(true);
                background.color = new Color(1f, 1f, 0.5f);
                break;
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
